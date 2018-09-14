using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.IO;
using CMS.Helpers;

using IOExceptions = System.IO;


namespace CMS.Modules
{
    /// <summary>
    /// Allows you to create export package to be bundled with NuGet package.
    /// The package contains some useful DB objects supported by import/export.
    /// The package does not contain any files (those are handled by NuGet itself).
    /// </summary>
    internal class ModuleExportPackageBuilder
    {
        #region "Fields"

        private readonly ResourceInfo mResourceInfo;
        private readonly IUserInfo mUserInfo;
        private readonly ModuleDataProvider mModuleDataProvider;

        #endregion


        #region "Properties"

        /// <summary>
        /// The export package contains module objects of types enumerated by this property.
        /// The module object type itself ("cms.resource") is not present in the enumeration.
        /// </summary>
        /// <returns>Enumeration of object types</returns>
        /// <seealso cref="GetModuleObjects"/>
        public IEnumerable<string> IncludedObjectTypes
        {
            get
            {
                return mModuleDataProvider.SupportedObjectTypes;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Builds a new export package using the import/export.
        /// The resulting zip will be placed in folder determined by <paramref name="targetFolderPath"/> parameter
        /// and named accordingly to <paramref name="targetFileName"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the process of export package creation a temporary folder is necessary.
        /// The folder is determined by <paramref name="tempFolderPath"/> parameter.
        /// Any contents of such folder are likely to be deleted before and after the package creation, thus do not use it for storing
        /// any useful data.
        /// </para>
        /// <para>
        /// All necessary folders are created before the export starts, if they do not exist already.
        /// </para>
        /// </remarks>
        /// <exception cref="UnauthorizedAccessException">Can be thrown when creating necessary folder structure.</exception>
        /// <exception cref="IOExceptions.IOException">Can be thrown when creating necessary folder structure.</exception>
        /// <param name="targetFolderPath">The resulting zip will be placed in folder determined by this parameter.</param>
        /// <param name="targetFileName">The resulting zip will be named accordingly to this parameter.</param>
        /// <param name="tempFolderPath">In the process of export package creation a temporary folder is necessary. 
        /// This parameter specifies path to the temp folder that will be used.</param>
        public void BuildExportPackage(string targetFolderPath, string targetFileName, string tempFolderPath)
        {
            SiteExportSettings exportSettings = CreateNewSettings(targetFolderPath, targetFileName, tempFolderPath);

            // Ensure working folders
            EnsureFolders(exportSettings);

            // Add export package contents - DB objects (files are handled by NuGet).
            AddModule(exportSettings);
            AddModuleObjects(exportSettings);

            // Make sure no data is in temp folder (possibly from previous unsuccessful export)
            ExportProvider.DeleteTemporaryFiles(exportSettings, true);

            // Create the export package
            ExportManager exportManager = new ExportManager(exportSettings);
            exportManager.Export(null);

            // Cleanup temp data after export
            ExportProvider.DeleteTemporaryFiles(exportSettings, true);
        }


        /// <summary>
        /// Gets object query for module objects of given type which are included in the export package.
        /// <paramref name="objectType"/> must be one of those enumerated in <see cref="IncludedObjectTypes"/>, otherwise returns null.
        /// </summary>
        /// <param name="objectType">Type of object to return object query for</param>
        /// <returns>Object query for given object type, or null.</returns>
        /// <seealso cref="IncludedObjectTypes"/>
        public ObjectQuery GetModuleObjects(string objectType)
        {
            return mModuleDataProvider.GetModuleObjects(objectType);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Makes sure that folders necessary for the export process are ready (they exist and are accessible).
        /// </summary>
        /// <param name="exportSettings">Determines the target path and temporary files path.</param>
        /// <exception cref="UnauthorizedAccessException">Can be thrown when creating necessary folder structure.</exception>
        /// <exception cref="IOExceptions.IOException">Can be thrown when creating necessary folder structure.</exception>
        private static void EnsureFolders(SiteExportSettings exportSettings)
        {
            // Ensure existence of target folder and check permissions
            DirectoryHelper.EnsureDiskPath(exportSettings.TargetPath, exportSettings.WebsitePath);
            bool missingPermissions = !DirectoryHelper.CheckPermissions(exportSettings.TargetPath, true, true, false, false);
            if (missingPermissions)
            {
                throw new UnauthorizedAccessException(
                    String.Format("[ModuleExportPackageBuilder.BuildExportPackage]: Missing read and write permissions necessary for export package target folder ('{0}').", exportSettings.TargetPath));
            }

            // Ensure existence of temporary folder and check permissions
            DirectoryHelper.EnsureDiskPath(exportSettings.TemporaryFilesPath, exportSettings.WebsitePath);
            missingPermissions = !DirectoryHelper.CheckPermissions(exportSettings.TemporaryFilesPath, true, true, false, false);
            if (missingPermissions)
            {
                throw new UnauthorizedAccessException(
                    String.Format("[ModuleExportPackageBuilder.BuildExportPackage]: Missing read and write permissions for export package temporary folder ('{0}').", exportSettings.TemporaryFilesPath));
            }
        }


        /// <summary>
        /// Adds module itself to the export settings.
        /// </summary>
        /// <param name="exportSettings">Export settings</param>
        private void AddModule(SiteExportSettings exportSettings)
        {
            exportSettings.Select(ResourceInfo.OBJECT_TYPE, mResourceInfo.ResourceName, false);
        }


        /// <summary>
        /// Adds module objects to the export settings.
        /// </summary>
        /// <param name="exportSettings">Export settings</param>
        private void AddModuleObjects(SiteExportSettings exportSettings)
        {
            foreach (string objectType in mModuleDataProvider.SupportedObjectTypes)
            {
                // Use local variable to avoid using foreach variable in closure
                string localObjectType = objectType;
                ObjectQuery moduleObjects = mModuleDataProvider.GetModuleObjects(objectType);
                string codeNameColumn = moduleObjects.TypeInfo.CodeNameColumn;

                moduleObjects.Column(codeNameColumn).ForEachRow(
                    obj => exportSettings.Select(localObjectType, (string)obj[codeNameColumn], false)
                );
            }
        }


        /// <summary>
        /// Creates new settings object for export and sets its basic properties
        /// </summary>
        /// <param name="targetFolderPath">The resulting zip will be placed in folder determined by this parameter.</param>
        /// <param name="targetFileName">The resulting zip will be named accordingly to this parameter.</param>
        /// <param name="tempFolderPath">In the process of export package creation a temporary folder is necessary. 
        /// This parameter specifies path to the temp folder that will be used.</param>
        private SiteExportSettings CreateNewSettings(string targetFolderPath, string targetFileName, string tempFolderPath)
        {
            SiteExportSettings result = new SiteExportSettings(mUserInfo);

            result.TargetPath = targetFolderPath;
            result.TargetFileName = targetFileName;
            result.TemporaryFilesPath = tempFolderPath;
            result.WebsitePath = SystemContext.WebApplicationPhysicalPath;

            result.SetInfo(ImportExportHelper.MODULE_NAME, mResourceInfo.ResourceName);

            // Additional settings
            result.CopyFiles = false;
            result.SetSettings(ImportExportHelper.SETTINGS_BIZFORM_DATA, false);
            result.SetSettings(ImportExportHelper.SETTINGS_CUSTOMTABLE_DATA, false);
            result.SetSettings(ImportExportHelper.SETTINGS_FORUM_POSTS, false);
            result.SetSettings(ImportExportHelper.SETTINGS_BOARD_MESSAGES, false);
            result.SetSettings(ImportExportHelper.SETTINGS_GLOBAL_FOLDERS, false);
            result.SetSettings(ImportExportHelper.SETTINGS_SITE_FOLDERS, false);
            result.SetSettings(ImportExportHelper.SETTINGS_COPY_ASPX_TEMPLATES_FOLDER, false);

            result.SiteId = 0;
            result.DefaultProcessObjectType = ProcessObjectEnum.Selected;

            // Include no objects to export by default
            result.ExportType = ExportTypeEnum.None;

            // Allow exporting objects since the big bang
            result.TimeStamp = DateTimeHelper.ZERO_TIME;

            return result;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new builder handling export package creation.
        /// </summary>
        /// <param name="module">Module for which the export package will be created</param>
        /// <param name="userInfo">Current user</param>
        public ModuleExportPackageBuilder(ResourceInfo module, IUserInfo userInfo)
        {
            mResourceInfo = module;
            mUserInfo = userInfo;
            mModuleDataProvider = new ModuleDataProvider(module);
        }

        #endregion
    }
}
