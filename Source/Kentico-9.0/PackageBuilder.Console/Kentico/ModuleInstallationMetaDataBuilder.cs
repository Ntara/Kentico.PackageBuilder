// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleInstallationMetaDataBuilder
// Assembly: CMS.Modules, Version=9.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System;
using System.Xml.Serialization;

using CMS.Core;
using CMS.IO;
using CMS.Modules;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Builds meta data needed on target instance for module
	///     installation/uninstallation.
	/// </summary>
	internal class ModuleInstallationMetaDataBuilder
	{
		private readonly ResourceInfo mResourceInfo;

		/// <summary>
		///     Creates a new builder handling installation meta files creation.
		/// </summary>
		/// <param name="module">Module for which the meta files will be created</param>
		public ModuleInstallationMetaDataBuilder(ResourceInfo module)
		{
			if (module == null)
				throw new ArgumentNullException("module");
			mResourceInfo = module;
		}

		/// <summary>Builds a new module description meta file.</summary>
		/// <param name="targetFolderPath">Target folder where to build the meta file.</param>
		/// <param name="targetFileName">Name of the built meta file.</param>
		public void BuildModuleDescription(string targetFolderPath, string targetFileName)
		{
			DirectoryHelper.EnsureDiskPath(DirectoryHelper.EnsurePathBackSlash(targetFolderPath), null);
			var fileName = Path.Combine(targetFolderPath, targetFileName);
			var metaData = new ModuleInstallationMetaData
			{
				Name = mResourceInfo.ResourceName,
				Version = mResourceInfo.ResourceVersion
			};
			Save(fileName, metaData);
		}

		/// <summary>Saves meta data to file.</summary>
		/// <param name="fileName">File name.</param>
		/// <param name="metaData">Meta data to be serialized to file.</param>
		private void Save(string fileName, ModuleInstallationMetaData metaData)
		{
			using (var fileStream = FileStream.New(fileName, FileMode.Create, FileAccess.Write))
			{
				new XmlSerializer(typeof(ModuleInstallationMetaData)).Serialize(fileStream.SystemStream, metaData);
			}
		}
	}
}