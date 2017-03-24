// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleDataProvider
// Assembly: CMS.Modules, Version=10.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Modules;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Provides module objects of supported types from database.
	/// </summary>
	internal class ModuleDataProvider
	{
		/// <summary>
		///     Contains object types of supported module database objects.
		/// </summary>
		private static readonly string[] mSupportedObjectTypes = new string[7]
		{
			"cms.documenttype",
			"cms.webpart",
			"cms.webpartcategory",
			"cms.formusercontrol",
			"cms.settingskey",
			"cms.settingscategory",
			"cms.systemtable"
		};

		private readonly string mCodeNamePrefix;
		private readonly ResourceInfo mResourceInfo;

		/// <summary>Creates a new module database objects provider.</summary>
		/// <param name="module">Module for which the objects are retrieved.</param>
		public ModuleDataProvider(ResourceInfo module)
		{
			mResourceInfo = module;
			mCodeNamePrefix = module.ResourceName + ".";
		}

		/// <summary>
		///     Enumerates object types which are considered as module's objects.
		///     The module object type itself ("cms.resource") is not present in the enumeration.
		/// </summary>
		/// <returns>Enumeration of object types.</returns>
		/// <seealso cref="M:Ntara.PackageBuilder.Kentico.ModuleDataProvider.GetModuleObjects(System.String)" />
		public IEnumerable<string> SupportedObjectTypes
		{
			get { return mSupportedObjectTypes; }
		}

		/// <summary>
		///     Gets object query for module objects of given type.
		///     <paramref name="objectType" /> must be one of those enumerated in
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleDataProvider.SupportedObjectTypes" />, otherwise returns null.
		/// </summary>
		/// <param name="objectType">Type of object to return object query for.</param>
		/// <returns>Object query for given object type, or null.</returns>
		/// <seealso cref="P:Ntara.PackageBuilder.Kentico.ModuleDataProvider.SupportedObjectTypes" />
		public ObjectQuery GetModuleObjects(string objectType)
		{
			switch (objectType.ToLowerInvariant())
			{
				case "cms.documenttype":
					return GetModulePageTypes();
				case "cms.webpart":
					return GetModuleWebParts();
				case "cms.webpartcategory":
					return GetModuleWebPartCategories();
				case "cms.formusercontrol":
					return GetModuleFormControls();
				case "cms.settingskey":
					return GetModuleSettingsKeys();
				case "cms.settingscategory":
					return GetModuleSettingsCategories();
				case "cms.systemtable":
					return GetModuleSystemTables();
				default:
					return null;
			}
		}

		/// <summary>
		///     Gets object query for page types (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Page types of module</returns>
		private ObjectQuery GetModulePageTypes()
		{
			return new ObjectQuery("cms.documenttype", true).WhereEquals("ClassResourceID", mResourceInfo.ResourceID);
		}

		/// <summary>
		///     Gets object query for web parts (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Web parts of module</returns>
		private ObjectQuery GetModuleWebParts()
		{
			return
				new ObjectQuery("cms.webpart", true).WhereEquals("WebPartResourceID", mResourceInfo.ResourceID)
					.Or(new WhereCondition().WhereNull("WebPartResourceID").And().WhereStartsWith("WebPartName", mCodeNamePrefix));
		}

		/// <summary>
		///     Gets object query for web part categories (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Web part categories of module</returns>
		private ObjectQuery GetModuleWebPartCategories()
		{
			return new ObjectQuery("cms.webpartcategory", true).WhereStartsWith("CategoryName", mCodeNamePrefix);
		}

		/// <summary>
		///     Gets object query for form controls (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Form controls of module</returns>
		private ObjectQuery GetModuleFormControls()
		{
			return
				new ObjectQuery("cms.formusercontrol", true).WhereEquals("UserControlResourceID", mResourceInfo.ResourceID)
					.Or(new WhereCondition().WhereNull("UserControlResourceID")
						.And()
						.WhereStartsWith("UserControlCodeName", mCodeNamePrefix));
		}

		/// <summary>
		///     Gets object query for settings keys (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Settings keys of module</returns>
		private ObjectQuery GetModuleSettingsKeys()
		{
			return new ObjectQuery("cms.settingskey", true).WhereIn("KeyCategoryID", GetModuleSettingsCategories().AsIDQuery());
		}

		/// <summary>
		///     Gets object query for settings categories (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>Settings categories of module</returns>
		private ObjectQuery GetModuleSettingsCategories()
		{
			return
				new ObjectQuery("cms.settingscategory", true).WhereEquals("CategoryResourceID", mResourceInfo.ResourceID)
					.Or(new WhereCondition().WhereNull("CategoryResourceID").And().WhereStartsWith("CategoryName", mCodeNamePrefix));
		}

		/// <summary>
		///     Gets object query for system tables (contained in module) to be included in the export package.
		/// </summary>
		/// <returns>System tables of module</returns>
		private ObjectQuery GetModuleSystemTables()
		{
			return new ObjectQuery("cms.systemtable", true).WhereEquals("ClassResourceID", mResourceInfo.ResourceID);
		}
	}
}