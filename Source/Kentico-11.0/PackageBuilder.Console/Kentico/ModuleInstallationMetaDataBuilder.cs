using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using CMS.Core;
using CMS.IO;


namespace CMS.Modules
{
    /// <summary>
    /// Builds meta data needed on target instance for module
    /// installation/uninstallation.
    /// </summary>
    internal class ModuleInstallationMetaDataBuilder
    {
        #region "Fields"

        private readonly ResourceInfo mResourceInfo;

        #endregion


        #region "Public methods"
        
        /// <summary>
        /// Builds a new module description meta file.
        /// </summary>
        /// <param name="targetFolderPath">Target folder where to build the meta file.</param>
        /// <param name="targetFileName">Name of the built meta file.</param>
        public void BuildModuleDescription(string targetFolderPath, string targetFileName)
        {
            DirectoryHelper.EnsureDiskPath(DirectoryHelper.EnsurePathBackSlash(targetFolderPath), null);
            string fileName = Path.Combine(targetFolderPath, targetFileName);

            ModuleInstallationMetaData metaData = new ModuleInstallationMetaData
            {
                Name = mResourceInfo.ResourceName,
                Version = mResourceInfo.ResourceVersion
            };

            Save(fileName, metaData);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new builder handling installation meta files creation.
        /// </summary>
        /// <param name="module">Module for which the meta files will be created</param>
        public ModuleInstallationMetaDataBuilder(ResourceInfo module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            mResourceInfo = module;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Saves meta data to file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="metaData">Meta data to be serialized to file.</param>
        private void Save(string fileName, ModuleInstallationMetaData metaData)
        {
            using (FileStream fs = FileStream.New(fileName, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ModuleInstallationMetaData));
                serializer.Serialize(fs, metaData);
            }
        }

        #endregion
    }
}
