// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using CMS.Base;
using CMS.Modules;
using CMS.Modules.NuGetPackages;

using NuGet;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// Aggregates all module files, assets, and information into a redistributable NuGet package.
	/// </summary>
	public class ModulePackageBuilder
	{
		private const string ModulesDataPath = @"App_Data\CMSModules\Modules\";
		private const string ToolsSourcePath = ModulesDataPath + @"tools\";

		private const string ReadmeFileName = "Readme.txt";
		private const string ReadmeSourcePath = ModulesDataPath + ReadmeFileName;

		/// <summary>
		/// The CMS root directory.
		/// </summary>
		protected string RootPath { get; }

		/// <summary>
		/// The resource information of the module being packaged.
		/// </summary>
		protected ResourceInfo Module { get; }

		private ModulePackageMetadata _moduleMetadata;
		private readonly IModuleVersionResolver _versionResolver;
		private readonly ModuleFileResolver _fileResolver;
		private readonly ModuleExportPackageBuilder _exportBuilder;
		private readonly ModuleInstallationMetaDataBuilder _installationMetaDataBuilder;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModulePackageBuilder"/> class.
		/// </summary>
		/// <param name="module">The resource module to package.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="module"/> is null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="module"/> is the 'CMS.CustomSystemModule' or not in development mode.</exception>
		public ModulePackageBuilder(ResourceInfo module)
		{
			if (module == null)
				throw new ArgumentNullException(nameof(module), CommonResources.ArgumentNullException_Module);
			if (!module.ResourceIsInDevelopment)
				throw new ArgumentException(CommonResources.ArgumentException_DevelopmentModuleOnly, nameof(module));
			if (module.ResourceName.EqualsCSafe("CMS.CustomSystemModule", true))
				throw new ArgumentException(CommonResources.ArgumentException_CustomSystemModuleNotAllowed, nameof(module));

			Module = module;
			RootPath = SystemContext.WebApplicationPhysicalPath;

			_versionResolver = (StaticVersion)module.ResourceVersion;
			_fileResolver = new ModuleFileResolver(module, RootPath);
			_exportBuilder = new ModuleExportPackageBuilder(module, CMSActionContext.CurrentUser);
			_installationMetaDataBuilder = new ModuleInstallationMetaDataBuilder(module);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModulePackageBuilder"/> class with the specified <see cref="IModuleVersionResolver"/>.
		/// </summary>
		/// <param name="module">The resource module to package.</param>
		/// <param name="versionResolver">The version resolver for the module.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="module"/> or <paramref name="versionResolver"/> is null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="module"/> is 'CMS.CustomSystemModule' or not in development mode.</exception>
		public ModulePackageBuilder(ResourceInfo module, IModuleVersionResolver versionResolver) : this(module)
		{
			if (versionResolver == null)
			{
				throw new ArgumentNullException(nameof(versionResolver), CommonResources.ArgumentNullException_VersionResolver);
			}

			_versionResolver = versionResolver;
		}

		/// <summary>
		/// Gets or sets the NuSpec file used to generate the final NuGet manifest.
		/// </summary>
		public string NuSpecFile { get; set; }

		/// <summary>
		/// Gets or sets a collection of token-value pairs where each occurrence of $token$ in the specified NuSpec file will be replaced with the given value.
		/// </summary>
		public IDictionary<string, string> NuSpecProperties { get; set; }

		/// <summary>
		/// Gets the module's package metadata.
		/// </summary>
		public ModulePackageMetadata ModuleMetadata
		{
			get { return _moduleMetadata ?? (_moduleMetadata = GetModuleMetadata()); }
		}

		/// <summary>
		/// Builds the module installation package to the specified output directory.
		/// </summary>
		/// <param name="outputDirectory">The output directory to which to write the package.</param>
		/// <returns>The result details for the completed module package.</returns>
		/// <exception cref="ArgumentException">The <paramref name="outputDirectory"/> is null or empty.</exception>
		/// <exception cref="UnauthorizedAccessException">The <paramref name="outputDirectory"/> does not have the required permissions.</exception>
		public ModulePackageResult BuildPackage(string outputDirectory)
		{
			if (string.IsNullOrEmpty(outputDirectory))
			{
				throw new ArgumentException(CommonResources.ArgumentException_OutputDirectoryNullOrEmpty, nameof(outputDirectory));
			}

			// Ensure fully qualified directory path
			if (!Path.IsPathRooted(outputDirectory))
			{
				outputDirectory = Path.Combine(RootPath, outputDirectory);
			}

			// Ensure Windows path format with backslashes
			outputDirectory = CMS.IO.Path.EnsureBackslashes(outputDirectory);

			CMS.IO.DirectoryHelper.CreateDirectory(outputDirectory);

			if (!CMS.IO.DirectoryHelper.CheckPermissions(outputDirectory))
			{
				var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.UnauthorizedAccessException_InsufficientWritePermissions, outputDirectory);
				throw new UnauthorizedAccessException(errorMessage);
			}

			BuildExportPackages();

			var manifestBuilder = CreateManifestBuilder();
			var filePath = Path.Combine(outputDirectory, manifestBuilder.PackageFileName);

			using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
			{
				BuildPackageImpl(fileStream, manifestBuilder);
			}

			return
				new ModulePackageResult()
				{
					OutputDirectory = outputDirectory,
					PackageFileName = manifestBuilder.PackageFileName
				};
		}

		/// <summary>
		/// Builds the module installation package to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to which to write the package.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="stream"/> is null.</exception>
		public void BuildPackage(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			BuildExportPackages();

			var manifestBuilder = CreateManifestBuilder();
			BuildPackageImpl(stream, manifestBuilder);
		}

		private void BuildPackageImpl(Stream stream, IManifestBuilder manifestBuilder)
		{
			try
			{
				AddModuleFiles(manifestBuilder);
				AddMetadataFiles(manifestBuilder);
				AddToolFiles(manifestBuilder);

				using (var memoryStream = new MemoryStream())
				{
					manifestBuilder.BuildManifest(memoryStream);
					memoryStream.Position = 0L;

					// Initialize the NuGet builder from the provided manifest
					var packageBuilder = new NuGet.PackageBuilder(memoryStream, RootPath);

					// Add the default Readme.txt if none exists (simulates the default behavior found during native Kentico module export)
					// Tricky: Performing this validation after the manifest is processed allows support for wildcards, exclusions, etc.
					if (!packageBuilder.Files.Any(file => string.Equals(Path.GetFileName(file.Path), ReadmeFileName, StringComparison.OrdinalIgnoreCase)))
					{
						var readmeFilePath = Path.Combine(RootPath, ReadmeSourcePath);

						// Confirm that the default Readme.txt file still exists or an error will occur
						if (File.Exists(readmeFilePath))
						{
							var readmeFile = new PhysicalPackageFile();
							readmeFile.SourcePath = readmeFilePath;
							readmeFile.TargetPath = ReadmeFileName;

							// Add the physical file
							packageBuilder.Files.Add(readmeFile);
						}
					}

					// Write the finalized package to the stream
					packageBuilder.Save(stream);
				}
			}
			finally
			{
				CleanupTemporaryFiles();
			}
		}

		#region |-- Protected Methods --|

		/// <summary>
		/// Creates an instance of the <see cref="IManifestBuilder"/> interface for constructing NuSpec manifest documents.
		/// </summary>
		protected virtual IManifestBuilder CreateManifestBuilder()
		{
			if (string.IsNullOrEmpty(NuSpecFile))
			{
				return new ManifestBuilder(ModuleMetadata);
			}

			var nuspecFilePath = NuSpecFile;

			// Ensure fully qualified NuSpec path
			if (!Path.IsPathRooted(nuspecFilePath))
			{
				nuspecFilePath = Path.Combine(RootPath, nuspecFilePath);
			}

			// Ensure Windows path format with backslashes
			nuspecFilePath = CMS.IO.Path.EnsureBackslashes(nuspecFilePath);

			return new ManifestBuilder(ModuleMetadata, nuspecFilePath, NuSpecProperties);
		}

		/// <summary>
		/// Adds module files to the NuSpec manifest.
		/// </summary>
		/// <param name="manifestBuilder">The manifest builder to amend files.</param>
		protected virtual void AddModuleFiles(IManifestBuilder manifestBuilder)
		{
			foreach (var contentFile in _fileResolver.GetContentFiles())
			{
				manifestBuilder.AddContent(contentFile.SourceRelativePath, contentFile.TargetRelativePath);
			}

			foreach (var libraryFile in _fileResolver.GetLibraryFiles())
			{
				manifestBuilder.AddLibrary(libraryFile.SourceRelativePath, libraryFile.TargetRelativePath);
			}
		}

		/// <summary>
		/// Adds module metadata files to the NuSpec manifest.
		/// </summary>
		/// <param name="manifestBuilder">The manifest builder to amend files.</param>
		protected virtual void AddMetadataFiles(IManifestBuilder manifestBuilder)
		{
			foreach (var metaDataFile in _fileResolver.GetMetaDataFiles())
			{
				manifestBuilder.AddContent(metaDataFile.SourceRelativePath, metaDataFile.TargetRelativePath);
			}
		}

		/// <summary>
		/// Adds module tool files to the NuSpec manifest.
		/// </summary>
		/// <param name="manifestBuilder">The manifest builder to amend files.</param>
		protected virtual void AddToolFiles(IManifestBuilder manifestBuilder)
		{
			if (Directory.Exists(ToolsSourcePath))
			{
				manifestBuilder.AddTools(ToolsSourcePath, null);
			}
		}

		/// <summary>
		/// Removes all temporary files created when building the module package.
		/// </summary>
		protected virtual void CleanupTemporaryFiles()
		{
			CMS.IO.DirectoryHelper.DeleteDirectory(_fileResolver.TemporaryFilesPath, true);
		}

		#endregion

		#region |-- Support Methods --|

		private void BuildExportPackages()
		{
			_exportBuilder.BuildExportPackage(
				_fileResolver.ExportPackageFolderPhysicalPath,
				_fileResolver.ExportPackageFileName,
				_fileResolver.ExportPackageTempFolderPhysicalPath
			);

			_installationMetaDataBuilder.BuildModuleDescription(
				_fileResolver.InstallationMetaDataTempFolderPhysicalPath,
				_fileResolver.InstallationMetaDataModuleDescriptionFileName
			);
		}

		private ModulePackageMetadata GetModuleMetadata()
		{
			var moduleMetadata =
				new ModulePackageMetadata
				{
					Id = Module.ResourceName,
					Title = Module.ResourceDisplayName,
					Version = _versionResolver.GetVersion(),
					Description = string.IsNullOrWhiteSpace(Module.ResourceDescription) ? "No description provided." : Module.ResourceDescription,
					Authors = string.IsNullOrWhiteSpace(Module.ResourceAuthor) ? "Unknown" : Module.ResourceAuthor
				};

			return moduleMetadata;
		}

		#endregion
	}
}
