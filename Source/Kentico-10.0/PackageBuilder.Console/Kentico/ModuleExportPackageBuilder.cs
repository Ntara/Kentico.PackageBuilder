// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleExportPackageBuilder
// Assembly: CMS.Modules, Version=10.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Modules;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Allows you to create export package to be bundled with NuGet package.
	///     The package contains some useful DB objects supported by import/export.
	///     The package does not contain any files (those are handled by NuGet itself).
	/// </summary>
	internal class ModuleExportPackageBuilder
	{
		private readonly ModuleDataProvider mModuleDataProvider;
		private readonly ResourceInfo mResourceInfo;
		private readonly IUserInfo mUserInfo;

		/// <summary>
		///     Creates a new builder handling export package creation.
		/// </summary>
		/// <param name="module">Module for which the export package will be created</param>
		/// <param name="userInfo">Current user</param>
		public ModuleExportPackageBuilder(ResourceInfo module, IUserInfo userInfo)
		{
			mResourceInfo = module;
			mUserInfo = userInfo;
			mModuleDataProvider = new ModuleDataProvider(module);
		}

		/// <summary>
		///     The export package contains module objects of types enumerated by this property.
		///     The module object type itself ("cms.resource") is not present in the enumeration.
		/// </summary>
		/// <returns>Enumeration of object types</returns>
		/// <seealso cref="M:Ntara.PackageBuilder.Kentico.ModuleExportPackageBuilder.GetModuleObjects(System.String)" />
		public IEnumerable<string> IncludedObjectTypes
		{
			get { return mModuleDataProvider.SupportedObjectTypes; }
		}

		/// <summary>
		///     Builds a new export package using the import/export.
		///     The resulting zip will be placed in folder determined by <paramref name="targetFolderPath" /> parameter
		///     and named accordingly to <paramref name="targetFileName" />.
		/// </summary>
		/// <remarks>
		///     <para>
		///         In the process of export package creation a temporary folder is necessary.
		///         The folder is determined by <paramref name="tempFolderPath" /> parameter.
		///         Any contents of such folder are likely to be deleted before and after the package creation, thus do not use it
		///         for storing
		///         any useful data.
		///     </para>
		///     <para>
		///         All necessary folders are created before the export starts, if they do not exist already.
		///     </para>
		/// </remarks>
		/// <exception cref="T:System.UnauthorizedAccessException">Can be thrown when creating necessary folder structure.</exception>
		/// <exception cref="T:System.IO.IOException">Can be thrown when creating necessary folder structure.</exception>
		/// <param name="targetFolderPath">The resulting zip will be placed in folder determined by this parameter.</param>
		/// <param name="targetFileName">The resulting zip will be named accordingly to this parameter.</param>
		/// <param name="tempFolderPath">
		///     In the process of export package creation a temporary folder is necessary.
		///     This parameter specifies path to the temp folder that will be used.
		/// </param>
		public void BuildExportPackage(string targetFolderPath, string targetFileName, string tempFolderPath)
		{
			var newSettings = CreateNewSettings(targetFolderPath, targetFileName, tempFolderPath);
			EnsureFolders(newSettings);
			AddModule(newSettings);
			AddModuleObjects(newSettings);
			ExportProvider.DeleteTemporaryFiles(newSettings, true);
			new ExportManager(newSettings).Export(null);
			ExportProvider.DeleteTemporaryFiles(newSettings, true);
		}

		/// <summary>
		///     Gets object query for module objects of given type which are included in the export package.
		///     <paramref name="objectType" /> must be one of those enumerated in
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleExportPackageBuilder.IncludedObjectTypes" />, otherwise returns
		///     null.
		/// </summary>
		/// <param name="objectType">Type of object to return object query for</param>
		/// <returns>Object query for given object type, or null.</returns>
		/// <seealso cref="P:Ntara.PackageBuilder.Kentico.ModuleExportPackageBuilder.IncludedObjectTypes" />
		public ObjectQuery GetModuleObjects(string objectType)
		{
			return mModuleDataProvider.GetModuleObjects(objectType);
		}

		/// <summary>
		///     Makes sure that folders necessary for the export process are ready (they exist and are accessible).
		/// </summary>
		/// <param name="exportSettings">Determines the target path and temporary files path.</param>
		/// <exception cref="T:System.UnauthorizedAccessException">Can be thrown when creating necessary folder structure.</exception>
		/// <exception cref="T:System.IO.IOException">Can be thrown when creating necessary folder structure.</exception>
		private static void EnsureFolders(SiteExportSettings exportSettings)
		{
			DirectoryHelper.EnsureDiskPath(exportSettings.TargetPath, exportSettings.WebsitePath);
			if (!DirectoryHelper.CheckPermissions(exportSettings.TargetPath, true, true, false, false))
				throw new UnauthorizedAccessException(
					string.Format(
						"[ModuleExportPackageBuilder.BuildExportPackage]: Missing read and write permissions necessary for export package target folder ('{0}').",
						exportSettings.TargetPath));
			DirectoryHelper.EnsureDiskPath(exportSettings.TemporaryFilesPath, exportSettings.WebsitePath);
			if (!DirectoryHelper.CheckPermissions(exportSettings.TemporaryFilesPath, true, true, false, false))
				throw new UnauthorizedAccessException(
					string.Format(
						"[ModuleExportPackageBuilder.BuildExportPackage]: Missing read and write permissions for export package temporary folder ('{0}').",
						exportSettings.TemporaryFilesPath));
		}

		/// <summary>Adds module itself to the export settings.</summary>
		/// <param name="exportSettings">Export settings</param>
		private void AddModule(SiteExportSettings exportSettings)
		{
			exportSettings.Select("cms.resource", mResourceInfo.ResourceName, false);
		}

		/// <summary>Adds module objects to the export settings.</summary>
		/// <param name="exportSettings">Export settings</param>
		private void AddModuleObjects(SiteExportSettings exportSettings)
		{
			foreach (var supportedObjectType in mModuleDataProvider.SupportedObjectTypes)
			{
				var localObjectType = supportedObjectType;
				var moduleObjects = mModuleDataProvider.GetModuleObjects(supportedObjectType);
				var codeNameColumn = moduleObjects.TypeInfo.CodeNameColumn;
				moduleObjects.Column(codeNameColumn)
					.ForEachRow(obj => exportSettings.Select(localObjectType, (string) obj[codeNameColumn], false), -1);
			}
		}

		/// <summary>
		///     Creates new settings object for export and sets its basic properties
		/// </summary>
		/// <param name="targetFolderPath">The resulting zip will be placed in folder determined by this parameter.</param>
		/// <param name="targetFileName">The resulting zip will be named accordingly to this parameter.</param>
		/// <param name="tempFolderPath">
		///     In the process of export package creation a temporary folder is necessary.
		///     This parameter specifies path to the temp folder that will be used.
		/// </param>
		private SiteExportSettings CreateNewSettings(string targetFolderPath, string targetFileName, string tempFolderPath)
		{
			var siteExportSettings = new SiteExportSettings(mUserInfo);
			siteExportSettings.TargetPath = targetFolderPath;
			siteExportSettings.TargetFileName = targetFileName;
			siteExportSettings.TemporaryFilesPath = tempFolderPath;
			siteExportSettings.WebsitePath = SystemContext.WebApplicationPhysicalPath;
			siteExportSettings.SetInfo("ModuleName", mResourceInfo.ResourceName);
			siteExportSettings.CopyFiles = false;
			siteExportSettings.SetSettings("BizFormData", false);
			siteExportSettings.SetSettings("CustomTableData", false);
			siteExportSettings.SetSettings("ForumPosts", false);
			siteExportSettings.SetSettings("BoardMessages", false);
			siteExportSettings.SetSettings("GlobalFolders", false);
			siteExportSettings.SetSettings("SiteFolders", false);
			siteExportSettings.SetSettings("CopyASPXTemplatesFolder", false);
			siteExportSettings.SiteId = 0;
			siteExportSettings.DefaultProcessObjectType = ProcessObjectEnum.Selected;
			siteExportSettings.ExportType = ExportTypeEnum.None;
			siteExportSettings.TimeStamp = DateTimeHelper.ZERO_TIME;
			return siteExportSettings;
		}
	}
}