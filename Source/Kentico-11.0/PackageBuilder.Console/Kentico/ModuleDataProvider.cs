using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;


namespace CMS.Modules
{
    /// <summary>
    /// Provides module objects of supported types from database.
    /// </summary>
    internal class ModuleDataProvider
    {
        #region "Fields"

        private readonly ResourceInfo mResourceInfo;
        private readonly string mCodeNamePrefix;

        /// <summary>
        /// Contains object types of supported module database objects.
        /// </summary>
        private static readonly string[] mSupportedObjectTypes =
        {
            "cms.documenttype",
            "cms.webpart",
            "cms.webpartcategory",
            "cms.formusercontrol",
            "cms.settingskey",
            "cms.settingscategory",
            "cms.systemtable"
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Enumerates object types which are considered as module's objects.
        /// The module object type itself ("cms.resource") is not present in the enumeration.
        /// </summary>
        /// <returns>Enumeration of object types.</returns>
        /// <seealso cref="GetModuleObjects"/>
        public IEnumerable<string> SupportedObjectTypes
        {
            get
            {
                return mSupportedObjectTypes;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets object query for module objects of given type.
        /// <paramref name="objectType"/> must be one of those enumerated in <see cref="SupportedObjectTypes"/>, otherwise returns null.
        /// </summary>
        /// <param name="objectType">Type of object to return object query for.</param>
        /// <returns>Object query for given object type, or null.</returns>
        /// <seealso cref="SupportedObjectTypes"/>
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

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets object query for page types (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Page types of module</returns>
        private ObjectQuery GetModulePageTypes()
        {
            // Locate page types by their ResourceID binding
            return new ObjectQuery("cms.documenttype").WhereEquals("ClassResourceID", mResourceInfo.ResourceID);
        }


        /// <summary>
        /// Gets object query for web parts (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Web parts of module</returns>
        private ObjectQuery GetModuleWebParts()
        {
            // Locate WebParts by their ResourceID binding or code name prefix
            return new ObjectQuery("cms.webpart").WhereEquals("WebPartResourceID", mResourceInfo.ResourceID)
                .Or(new WhereCondition().WhereNull("WebPartResourceID").And().WhereStartsWith("WebPartName", mCodeNamePrefix));
        }


        /// <summary>
        /// Gets object query for web part categories (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Web part categories of module</returns>
        private ObjectQuery GetModuleWebPartCategories()
        {
            // Locate WebParts by their code name prefix
            return new ObjectQuery("cms.webpartcategory").WhereStartsWith("CategoryName", mCodeNamePrefix);
        }


        /// <summary>
        /// Gets object query for form controls (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Form controls of module</returns>
        private ObjectQuery GetModuleFormControls()
        {
            // Locate FormControls by their ResourceID binding or code name prefix
            return new ObjectQuery("cms.formusercontrol").WhereEquals("UserControlResourceID", mResourceInfo.ResourceID)
                .Or(new WhereCondition().WhereNull("UserControlResourceID").And().WhereStartsWith("UserControlCodeName", mCodeNamePrefix));
        }


        /// <summary>
        /// Gets object query for settings keys (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Settings keys of module</returns>
        private ObjectQuery GetModuleSettingsKeys()
        {
            // Locate SettingsKeys by their parent SettingsCategory
            return new ObjectQuery("cms.settingskey").WhereIn("KeyCategoryID", GetModuleSettingsCategories().AsIDQuery());
        }


        /// <summary>
        /// Gets object query for settings categories (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>Settings categories of module</returns>
        private ObjectQuery GetModuleSettingsCategories()
        {
            // Locate SettingsCategory - must have ResourceID or code name prefix
            return new ObjectQuery("cms.settingscategory").WhereEquals("CategoryResourceID", mResourceInfo.ResourceID)
                .Or(new WhereCondition().WhereNull("CategoryResourceID").And().WhereStartsWith("CategoryName", mCodeNamePrefix));
        }


        /// <summary>
        /// Gets object query for system tables (contained in module) to be included in the export package.
        /// </summary>
        /// <returns>System tables of module</returns>
        private ObjectQuery GetModuleSystemTables()
        {
            return new ObjectQuery("cms.systemtable").WhereEquals("ClassResourceID", mResourceInfo.ResourceID);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new module database objects provider.
        /// </summary>
        /// <param name="module">Module for which the objects are retrieved.</param>
        public ModuleDataProvider(ResourceInfo module)
        {
            mResourceInfo = module;
            mCodeNamePrefix = module.ResourceName + ".";
        }

        #endregion
    }
}
