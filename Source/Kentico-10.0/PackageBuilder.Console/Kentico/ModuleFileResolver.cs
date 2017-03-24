// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleFileResolver
// Assembly: CMS.Modules, Version=10.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.IO;
using CMS.Modules;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Provides paths to all module files and folders which are needed during installation package creation.
	/// </summary>
	internal class ModuleFileResolver : ModuleFileResolverBase
	{
		/// <summary>
		///     Specifies all content files extensions that are excluded from the package.
		/// </summary>
		private static readonly IEnumerable<string> mExcludedContentFileExtensions = new string[1] { ".cs" };

		/// <summary>
		///     Specifies all paths where module assemblies can be located.
		///     Module can reference additional libraries that are specified by <see cref="T:CMS.Modules.ResourceLibraryInfo" />.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		/// </remarks>
		private static readonly IEnumerable<ModuleFilePathRule> mLibraryPathRules = new ModuleFilePathRule[1] { new ModuleFilePathRule("bin\\{0}.dll", null, null) };

		/// <summary>
		///     GUID to be used in paths so that each instance has its own temp folders.
		/// </summary>
		private readonly Guid mInstanceGuid = Guid.NewGuid();

		private readonly ResourceInfo mModule;
		private readonly string mModuleName;
		private readonly string mModuleNameSpace;
		private readonly string mRootPath;
		private IEnumerable<ModuleFilePathRule> mContentPathRules;
		private string mExportPackageFileName;
		private string mExportPackageFolderPath;
		private string mExportPackageTempFolderPath;
		private string mInstallationMetaDataModuleDescriptionFileName;
		private IEnumerable<ModuleFilePathRule> mInstallationMetaDataPathRules;
		private string mInstallationMetaDataTempFolderPath;
		private string mTemporaryFilesPath;

		/// <summary>Creates new ModuleFileResolver.</summary>
		/// <param name="module">Resource info specifying module for which paths are created.</param>
		/// <param name="rootPath">Root physical path. All paths are relative to this path.</param>
		public ModuleFileResolver(ResourceInfo module, string rootPath)
		{
			mModule = module;
			mRootPath = rootPath;
			var length = module.ResourceName.IndexOf('.');
			if (length >= 0)
			{
				mModuleNameSpace = module.ResourceName.Substring(0, length);
				mModuleName = module.ResourceName.Substring(length + 1);
			}
			else
			{
				mModuleName = module.ResourceName;
			}
		}

		/// <summary>
		///     Path pattern to folder for temporary files. All temporary paths
		///     must be created from this path so that all temporary
		///     data can be deleted at once.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		/// </remarks>
		private string TemporaryFolderPathPattern
		{
			get { return "App_Data\\CMSModules\\{0}\\InstallTemp\\" + mInstanceGuid + "\\"; }
		}

		/// <summary>
		///     Path pattern to folder where export package is to be stored.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		/// </remarks>
		private string ExportPackageFolderPathPattern
		{
			get { return TemporaryFolderPathPattern; }
		}

		/// <summary>
		///     Path pattern to file where export package is to be stored.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		///     Substitution '{1}' is replaced by the module version.
		/// </remarks>
		private string ExportPackagePathPattern
		{
			get { return ExportPackageFolderPathPattern + "{0}_{1}.zip"; }
		}

		/// <summary>
		///     Path pattern to folder where export package temporary data are to be stored.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		/// </remarks>
		private string ExportPackageTempFolderPathPattern
		{
			get { return TemporaryFolderPathPattern + "ExportPackageTemp\\"; }
		}

		/// <summary>
		///     Path pattern to folder where installation meta data are to be stored.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		/// </remarks>
		private string InstallationMetaDataTempFolderPathPattern
		{
			get { return ExportPackageFolderPathPattern + "InstallationMetaData\\"; }
		}

		/// <summary>
		///     Specifies all paths where module files can be located.
		/// </summary>
		/// <remarks>
		///     Substitution '{0}' is replaced by the module codename.
		///     Substitution '{1}' is replaced by the module version.
		/// </remarks>
		private IEnumerable<ModuleFilePathRule> ContentPathRules
		{
			get
			{
				var contentPathRules = mContentPathRules;
				if (contentPathRules != null)
					return contentPathRules;
				return
					mContentPathRules =
						new ModuleFilePathRule[7]
						{
							new ModuleFilePathRule("App_Data\\CMSModules\\{0}\\", "App_Data\\CMSModules\\{0}", new string[1] {"InstallTemp\\"}),
							new ModuleFilePathRule(ExportPackagePathPattern, "App_Data\\CMSModules\\{0}\\Install", null),
							new ModuleFilePathRule("CMSFormControls\\{0}\\", "CMSFormControls\\{0}", null),
							new ModuleFilePathRule("CMSModules\\{0}\\", "CMSModules\\{0}", null),
							new ModuleFilePathRule("CMSScripts\\CMSModules\\{0}\\", "CMSScripts\\CMSModules\\{0}", null),
							new ModuleFilePathRule("CMSResources\\{0}\\", "CMSResources\\{0}", null),
							new ModuleFilePathRule("CMSWebParts\\{0}\\", "CMSWebParts\\{0}", null)
						};
			}
		}

		/// <summary>
		///     Specifies all paths where module installation meta data files can be located.
		/// </summary>
		private IEnumerable<ModuleFilePathRule> InstallationMetaDataPathRules
		{
			get
			{
				var metaDataPathRules = mInstallationMetaDataPathRules;
				if (metaDataPathRules != null)
					return metaDataPathRules;
				return
					mInstallationMetaDataPathRules =
						new ModuleFilePathRule[1]
						{
							new ModuleFilePathRule(
								InstallationMetaDataTempFolderPathPattern + InstallationMetaDataModuleDescriptionFileName,
								"App_Data\\CMSModules\\CMSInstallation\\Packages\\" + InstallationMetaDataModuleDescriptionFileName, null)
						};
			}
		}

		/// <summary>
		///     Gets the physical path of the folder where the export package with module's data is located.
		/// </summary>
		public string ExportPackageFolderPhysicalPath
		{
			get
			{
				return mExportPackageFolderPath ??
				       (mExportPackageFolderPath = Path.Combine(mRootPath, ResolvePath(ExportPackageFolderPathPattern)));
			}
		}

		/// <summary>
		///     Gets the name of file containing exported module's data.
		/// </summary>
		public string ExportPackageFileName
		{
			get { return mExportPackageFileName ?? (mExportPackageFileName = ResolvePath("{0}_{1}.zip")); }
		}

		/// <summary>
		///     Gets the physical path of the folder that is used as temporary storage while creating the package with module's
		///     data.
		/// </summary>
		public string ExportPackageTempFolderPhysicalPath
		{
			get
			{
				return mExportPackageTempFolderPath ??
				       (mExportPackageTempFolderPath = Path.Combine(mRootPath, ResolvePath(ExportPackageTempFolderPathPattern)));
			}
		}

		/// <summary>
		///     Gets the physical path of the folder that is used as temporary storage for installation meta data.
		/// </summary>
		public string InstallationMetaDataTempFolderPhysicalPath
		{
			get
			{
				return mInstallationMetaDataTempFolderPath ??
				       (mInstallationMetaDataTempFolderPath =
					       Path.Combine(mRootPath, ResolvePath(InstallationMetaDataTempFolderPathPattern)));
			}
		}

		/// <summary>
		///     Gets the name of file containing installation module description.
		/// </summary>
		public string InstallationMetaDataModuleDescriptionFileName
		{
			get
			{
				return mInstallationMetaDataModuleDescriptionFileName ??
				       (mInstallationMetaDataModuleDescriptionFileName = ResolvePath("{0}_{1}.xml"));
			}
		}

		/// <summary>
		///     Gets the physical path of the folder that is used as temporary storage during package creation.
		///     All temporary paths are subfolders of this path so deleting this folder safely removes all
		///     temporary data.
		/// </summary>
		public string TemporaryFilesPath
		{
			get
			{
				return mTemporaryFilesPath ??
				       (mTemporaryFilesPath = Path.Combine(mRootPath, ResolvePath(TemporaryFolderPathPattern)));
			}
		}

		/// <summary>
		///     Gets source and target paths of all module's installation meta data files.
		/// </summary>
		public IEnumerable<ModuleFile> GetMetaDataFiles()
		{
			var moduleFileList = new List<ModuleFile>();
			var context = new ModulePathRuleResolverContext(mRootPath, mModule, null, null);
			foreach (var metaDataPathRule in InstallationMetaDataPathRules)
			{
				var pathRuleResolver = new ModulePathRuleResolver(metaDataPathRule, context);
				moduleFileList.AddRange(pathRuleResolver.ResolvedFiles);
			}
			return moduleFileList;
		}

		/// <summary>Gets source and target paths of all module's files.</summary>
		public IEnumerable<ModuleFile> GetContentFiles()
		{
			var moduleFileList = new List<ModuleFile>();
			if (!string.IsNullOrEmpty(mModuleNameSpace) && mModuleNameSpace.EqualsCSafe("cms", true))
				moduleFileList.AddRange(GetContentFilesByModuleName(mModuleName));
			moduleFileList.AddRange(GetContentFilesByModuleName(mModule.ResourceName));
			return moduleFileList;
		}

		/// <summary>
		///     Gets source and target paths of all module's files that match given module name.
		/// </summary>
		/// <param name="moduleName">Module name that is used for replacing substitutions in paths.</param>
		private IEnumerable<ModuleFile> GetContentFilesByModuleName(string moduleName)
		{
			var moduleFileList = new List<ModuleFile>();
			var context = new ModulePathRuleResolverContext(mRootPath, mModule, moduleName, mExcludedContentFileExtensions);
			foreach (var contentPathRule in ContentPathRules)
			{
				var pathRuleResolver = new ModulePathRuleResolver(contentPathRule, context);
				moduleFileList.AddRange(pathRuleResolver.ResolvedFiles);
			}
			return moduleFileList;
		}

		/// <summary>
		///     Gets source and target paths of all module's libraries.
		/// </summary>
		public IEnumerable<ModuleFile> GetLibraryFiles()
		{
			var moduleFileList = new List<ModuleFile>();
			var list =
				ResourceLibraryInfoProvider.GetResourceLibraries()
					.WhereEquals("ResourceLibraryResourceID", mModule.ResourceID)
					.Select(
						x =>
							new ModuleFilePathRule(x.ResourceLibraryPath.TrimStart('~').TrimStart('\\'), default(string),
								default(IEnumerable<string>)))
					.ToList();
			list.AddRange(mLibraryPathRules);
			var context = new ModulePathRuleResolverContext(mRootPath, mModule, null, null);
			foreach (var pathRule in list)
			{
				var pathRuleResolver = new ModulePathRuleResolver(pathRule, context);
				moduleFileList.AddRange(pathRuleResolver.ResolvedFiles);
			}
			return moduleFileList;
		}

		/// <summary>
		///     Resolves given path replacing the substitutions in it.
		/// </summary>
		/// <param name="path">Path that can contain substitutions.</param>
		/// <returns>Resolved path.</returns>
		private string ResolvePath(string path)
		{
			return string.Format(path, mModule.ResourceName, mModule.ResourceVersion);
		}
	}
}