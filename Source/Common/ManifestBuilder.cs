// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CMS.Modules.NuGetPackages;

using NuGet;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// Constructs a NuSpec manifest document from module metadata and an optional NuSpec source file.
	/// </summary>
	/// <remarks>
	/// For more information on the NuSpec manifest schema, see <see href="https://docs.microsoft.com/en-us/nuget/schema/nuspec"/>
	/// </remarks>
	public class ManifestBuilder : IManifestBuilder
	{
		private const string NuSpecLibDir = "lib";
		private const string NuSpecContentDir = "content";
		private const string NuSpecToolsDir = "tools";

		private readonly List<ManifestFile> _files;

		/// <summary>
		/// Initializes a new instance of the <see cref="ManifestBuilder"/> class.
		/// </summary>
		/// <param name="moduleMetadata">The <see cref="ModulePackageMetadata"/> from which to draw required manifest information.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="moduleMetadata"/> is null.</exception>
		public ManifestBuilder(ModulePackageMetadata moduleMetadata)
		{
			if (moduleMetadata == null)
			{
				throw new ArgumentNullException(nameof(moduleMetadata), CommonResources.ArgumentNullException_ModuleMetadata);
			}

			ManifestMetadata =
				new ManifestMetadata()
				{
					Id = moduleMetadata.Id,
					Title = moduleMetadata.Title,
					Version = moduleMetadata.Version,
					Authors = moduleMetadata.Authors,
					Description = moduleMetadata.Description
				};

			_files = new List<ManifestFile>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManifestBuilder"/> class with NuSpec information.
		/// </summary>
		/// <param name="moduleMetadata">The <see cref="ModulePackageMetadata"/> from which to draw required manifest information.</param>
		/// <param name="nuspecFilePath">The NuSpec file path from with to extract additional manifest information.</param>
		/// <param name="nuspecProperties">A collection of token-value pairs where each occurrence of $token$ in the specified NuSpec file will be replaced with the given value.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="moduleMetadata"/> is null.</exception>
		/// <exception cref="FileNotFoundException">The file specified by <paramref name="nuspecFilePath"/> was not found.</exception>
		public ManifestBuilder(ModulePackageMetadata moduleMetadata, string nuspecFilePath, IDictionary<string, string> nuspecProperties = null)
		{
			if (moduleMetadata == null)
			{
				throw new ArgumentNullException(nameof(moduleMetadata), CommonResources.ArgumentNullException_ModuleMetadata);
			}

			if (!File.Exists(nuspecFilePath))
			{
				var errorMessage = string.Format(CommonResources.FileNotFoundException_NuSpecNotFound, nuspecFilePath);
				throw new FileNotFoundException(errorMessage);
			}

			using (var fileStream = new FileStream(nuspecFilePath, FileMode.Open))
			{
				var propertyProvider = new ModulePropertyProvider(moduleMetadata, nuspecProperties);
				var manifest = Manifest.ReadFrom(fileStream, propertyProvider, true);

				ManifestMetadata = manifest.Metadata;

				_files = manifest.Files;
			}
		}

		/// <summary>
		/// The metadata properties used when building the final manifest document.
		/// </summary>
		public ManifestMetadata ManifestMetadata { get; }

		/// <inheritdoc />
		public string PackageFileName
		{
			get { return $"{ManifestMetadata.Id}_{ManifestMetadata.Version}.nupkg"; }
		}

		/// <inheritdoc />
		public void AddFile(string source, string target, string exclude = null)
		{
			_files.Add(
				new ManifestFile()
				{
					Source = source,
					Target = target,
					Exclude = exclude
				}
			);
		}

		/// <inheritdoc />
		public void AddLibrary(string source, string destination, string exclude = null, string targetFramework = null)
		{
			AddResolvedFile(source, destination, NuSpecLibDir, exclude, targetFramework);
		}

		/// <inheritdoc />
		public void AddContent(string source, string destination, string exclude = null, string targetFramework = null)
		{
			AddResolvedFile(source, destination, NuSpecContentDir, exclude, targetFramework);
		}

		/// <inheritdoc />
		public void AddTools(string source, string destination, string exclude = null, string targetFramework = null)
		{
			AddResolvedFile(source, destination, NuSpecToolsDir, exclude, targetFramework);
		}

		/// <inheritdoc />
		public void BuildManifest(Stream stream)
		{
			var manifest =
				new Manifest()
				{
					Metadata = ManifestMetadata,
					Files = new List<ManifestFile>(_files)
				};

			manifest.Save(stream, true);
		}

		#region |-- Support Methods --|

		private void AddResolvedFile(string source, string destination, string packageFolder, string exclude = null, string targetFramework = null)
		{
			AddFile(
				source,
				BuildTargetPath(packageFolder, destination, targetFramework),
				exclude
			);
		}

		private string BuildTargetPath(string targetFolder, string destination, string targetFramework = null)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(targetFolder);

			if (!string.IsNullOrEmpty(targetFramework))
			{
				if (!targetFramework.StartsWith(@"\"))
				{
					stringBuilder.Append(@"\");
				}

				stringBuilder.Append(targetFramework);
			}

			if (!string.IsNullOrEmpty(destination))
			{
				if (!destination.StartsWith(@"\"))
				{
					stringBuilder.Append(@"\");
				}

				stringBuilder.Append(destination);
			}

			return stringBuilder.ToString();
		}

		#endregion
	}
}