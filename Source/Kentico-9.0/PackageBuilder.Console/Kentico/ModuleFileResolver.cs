using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Provides paths to all module files and folders which are needed during installation package creation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ModuleFileResolver : ModuleFileResolverBase
    {

        #region "Private fields"

        /// <summary>
        /// GUID to be used in paths so that each instance has its own temp folders.
        /// </summary>
        private readonly Guid mInstanceGuid = Guid.NewGuid();
        private readonly ResourceInfo mModule;
        private readonly string mRootPath;
        private readonly string mModuleName;
        private readonly string mModuleNameSpace;

        private string mExportPackageFolderPath;
        private string mExportPackageFileName;
        private string mExportPackageTempFolderPath;
        private string mInstallationMetaDataTempFolderPath;
        private string mInstallationMetaDataModuleDescriptionFileName;
        private string mTemporaryFilesPath;


        /// <summary>
        /// Path pattern to folder for temporary files. All temporary paths
        /// must be created from this path so that all temporary
        /// data can be deleted at once.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        private string TemporaryFolderPathPattern
        {
            get
            {
                return @"App_Data\CMSModules\{0}\InstallTemp\" + mInstanceGuid + @"\";
            }
        }


        /// <summary>
        /// Path pattern to folder where export package is to be stored.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        private string ExportPackageFolderPathPattern
        {
            get
            {
                // Export package is in the root of the temp folder
                return TemporaryFolderPathPattern;
            }
        }


        /// <summary>
        /// Path pattern to file where export package is to be stored.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        private string ExportPackagePathPattern
        {
            get
            {
                return ExportPackageFolderPathPattern + EXPORT_PACKAGE_FILE_NAME_PATTERN;
            }
        }


        /// <summary>
        /// Path pattern to folder where export package temporary data are to be stored.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        private string ExportPackageTempFolderPathPattern
        {
            get
            {
                // Export package temp folder is subfolder of the temp folder
                return TemporaryFolderPathPattern + @"ExportPackageTemp\";
            }
        }


        /// <summary>
        /// Path pattern to folder where installation meta data are to be stored.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        private string InstallationMetaDataTempFolderPathPattern
        {
            get
            {
                // Meta data are in a subfolder of the temp folder
                return ExportPackageFolderPathPattern + @"InstallationMetaData\";
            }
        }


        private IEnumerable<ModuleFilePathRule> mContentPathRules;


        /// <summary>
        /// Specifies all paths where module files can be located. 
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        private IEnumerable<ModuleFilePathRule> ContentPathRules
        {
            get
            {
                return mContentPathRules ?? (mContentPathRules = new []
                {
                    new ModuleFilePathRule(@"App_Data\CMSModules\{0}\", @"App_Data\CMSModules\{0}", new []{ @"InstallTemp\" }),
                    new ModuleFilePathRule(ExportPackagePathPattern, INSTALLATION_DIRECTORY_PATH_PATTERN),
                    new ModuleFilePathRule(@"CMSFormControls\{0}\", @"CMSFormControls\{0}"),
                    new ModuleFilePathRule(@"CMSModules\{0}\", @"CMSModules\{0}"),
                    new ModuleFilePathRule(@"CMSScripts\CMSModules\{0}\", @"CMSScripts\CMSModules\{0}"),
                    new ModuleFilePathRule(@"CMSResources\{0}\", @"CMSResources\{0}"),
                    new ModuleFilePathRule(@"CMSWebParts\{0}\", @"CMSWebParts\{0}"),
                });
            }
        }


        /// <summary>
        /// Specifies all content files extensions that are excluded from the package.
        /// </summary>
        private static readonly IEnumerable<string> mExcludedContentFileExtensions = new[]
        {
            ".cs"
        };


        /// <summary>
        /// Specifies all paths where module assemblies can be located.
        /// Module can reference additional libraries that are specified by <see cref="ResourceLibraryInfo"/>.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        private static readonly IEnumerable<ModuleFilePathRule> mLibraryPathRules = new[]
        {
            new ModuleFilePathRule(@"bin\{0}.dll")
        };


        private IEnumerable<ModuleFilePathRule> mInstallationMetaDataPathRules;


        /// <summary>
        /// Specifies all paths where module installation meta data files can be located.
        /// </summary>
        private IEnumerable<ModuleFilePathRule> InstallationMetaDataPathRules
        {
            get
            {
                return mInstallationMetaDataPathRules ?? (mInstallationMetaDataPathRules = new[]
                {
                    new ModuleFilePathRule(InstallationMetaDataTempFolderPathPattern + InstallationMetaDataModuleDescriptionFileName, InstallableModulesManager.NUGET_MODULES_META_FILES_PATH + InstallationMetaDataModuleDescriptionFileName)
                });
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the physical path of the folder where the export package with module's data is located.
        /// </summary>
        public string ExportPackageFolderPhysicalPath
        {
            get
            {
                return mExportPackageFolderPath ?? (mExportPackageFolderPath = Path.Combine(mRootPath, ResolvePath(ExportPackageFolderPathPattern)));
            }
        }


        /// <summary>
        /// Gets the name of file containing exported module's data.
        /// </summary>
        public string ExportPackageFileName
        {
            get
            {
                return mExportPackageFileName ?? (mExportPackageFileName = ResolvePath(EXPORT_PACKAGE_FILE_NAME_PATTERN));
            }
        }


        /// <summary>
        /// Gets the physical path of the folder that is used as temporary storage while creating the package with module's data.
        /// </summary>
        public string ExportPackageTempFolderPhysicalPath
        {
            get
            {
                return mExportPackageTempFolderPath ?? (mExportPackageTempFolderPath = Path.Combine(mRootPath, ResolvePath(ExportPackageTempFolderPathPattern)));
            }
        }


        /// <summary>
        /// Gets the physical path of the folder that is used as temporary storage for installation meta data.
        /// </summary>
        public string InstallationMetaDataTempFolderPhysicalPath
        {
            get
            {
                return mInstallationMetaDataTempFolderPath ?? (mInstallationMetaDataTempFolderPath = Path.Combine(mRootPath, ResolvePath(InstallationMetaDataTempFolderPathPattern)));
            }
        }


        /// <summary>
        /// Gets the name of file containing installation module description.
        /// </summary>
        public string InstallationMetaDataModuleDescriptionFileName
        {
            get
            {
                return mInstallationMetaDataModuleDescriptionFileName ?? (mInstallationMetaDataModuleDescriptionFileName = ResolvePath(InstallableModulesManager.MODULE_DESCRIPTION_META_FILE_NAME_PATTERN));
            }
        }


        /// <summary>
        /// Gets the physical path of the folder that is used as temporary storage during package creation.
        /// All temporary paths are subfolders of this path so deleting this folder safely removes all
        /// temporary data.
        /// </summary>
        public string TemporaryFilesPath
        {
            get
            {
                return mTemporaryFilesPath ?? (mTemporaryFilesPath = Path.Combine(mRootPath, ResolvePath(TemporaryFolderPathPattern)));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new ModuleFileResolver.
        /// </summary>
        /// <param name="module">Resource info specifying module for which paths are created.</param>
        /// <param name="rootPath">Root physical path. All paths are relative to this path.</param>
        public ModuleFileResolver(ResourceInfo module, string rootPath)
        {
            mModule = module;
            mRootPath = rootPath;

            // Get module namespace and its name.
            int nameSpaceEnd = module.ResourceName.IndexOf('.');
            if (nameSpaceEnd >= 0)
            {
                mModuleNameSpace = module.ResourceName.Substring(0, nameSpaceEnd);
                mModuleName = module.ResourceName.Substring(nameSpaceEnd + 1);
            }
            else
            {
                mModuleName = module.ResourceName;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Gets source and target paths of all module's installation meta data files.
        /// </summary>
        public IEnumerable<ModuleFile> GetMetaDataFiles()
        {
            List<ModuleFile> metaDataFilePaths = new List<ModuleFile>();

            var pathResolverContext = new ModulePathRuleResolverContext(mRootPath, mModule);
            foreach (var rule in InstallationMetaDataPathRules)
            {
                ModulePathRuleResolver ruleResolver = new ModulePathRuleResolver(rule, pathResolverContext);
                metaDataFilePaths.AddRange(ruleResolver.ResolvedFiles);
            }

            return metaDataFilePaths;
        }


        /// <summary>
        /// Gets source and target paths of all module's files.
        /// </summary>
        public IEnumerable<ModuleFile> GetContentFiles()
        {
            List<ModuleFile> contentFilePaths = new List<ModuleFile>();
            if (!String.IsNullOrEmpty(mModuleNameSpace) && mModuleNameSpace.EqualsCSafe("cms", true))
            {
                // Special case - CMS modules can break naming conventions by not using namespace in paths.
                contentFilePaths.AddRange(GetContentFilesByModuleName(mModuleName));
            }
            contentFilePaths.AddRange(GetContentFilesByModuleName(mModule.ResourceName));

            return contentFilePaths;
        }


        /// <summary>
        /// Gets source and target paths of all module's files that match given module name.
        /// </summary>
        /// <param name="moduleName">Module name that is used for replacing substitutions in paths.</param>
        private IEnumerable<ModuleFile> GetContentFilesByModuleName(string moduleName)
        {
            List<ModuleFile> results = new List<ModuleFile>();
            
            var pathResolverContext = new ModulePathRuleResolverContext(mRootPath, mModule, moduleName, mExcludedContentFileExtensions);
            foreach (var rule in ContentPathRules)
            {
                ModulePathRuleResolver ruleResolver = new ModulePathRuleResolver(rule, pathResolverContext);
                results.AddRange(ruleResolver.ResolvedFiles);
            }

            return results;
        }


        /// <summary>
        /// Gets source and target paths of all module's libraries.
        /// </summary>
        public IEnumerable<ModuleFile> GetLibraryFiles()
        {
            List<ModuleFile> results = new List<ModuleFile>();

            // Add additional libraries
            List<ModuleFilePathRule> libraryRules = ResourceLibraryInfoProvider.GetResourceLibraries()
                                                                           .WhereEquals("ResourceLibraryResourceID", mModule.ResourceID)
                                                                           .Select(x => new ModuleFilePathRule(x.ResourceLibraryPath.TrimStart('~').TrimStart('\\'), null, null))
                                                                           .ToList();
            // Add default libraries
            libraryRules.AddRange(mLibraryPathRules);

            var pathResolverContext = new ModulePathRuleResolverContext(mRootPath, mModule);
            foreach (var rule in libraryRules)
            {
                ModulePathRuleResolver ruleResolver = new ModulePathRuleResolver(rule, pathResolverContext);
                results.AddRange(ruleResolver.ResolvedFiles);
            }
            
            return results;
        }


        /// <summary>
        /// Resolves given path replacing the substitutions in it.
        /// </summary>
        /// <param name="path">Path that can contain substitutions.</param>
        /// <returns>Resolved path.</returns>
        private string ResolvePath(string path)
        {
            return String.Format(path, mModule.ResourceName, mModule.ResourceVersion);
        }

        #endregion

    }
}
