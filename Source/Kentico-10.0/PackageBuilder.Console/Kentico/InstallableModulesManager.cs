using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Manages installable modules.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class InstallableModulesManager
    {
        #region "Fields"

        /// <summary>
        /// Pattern for module description with module's name and version.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        public const string MODULE_DESCRIPTION_PATTERN = "{0}_{1}";


        /// <summary>
        /// File name pattern for module description meta file.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        public const string MODULE_DESCRIPTION_META_FILE_NAME_PATTERN = MODULE_DESCRIPTION_PATTERN + ".xml";


        /// <summary>
        /// Relative path to directory containing module descriptions managed by NuGet (with trailing slash).
        /// Each description file is expected to be named accordingly to <see cref="MODULE_DESCRIPTION_META_FILE_NAME_PATTERN"/>.
        /// </summary>
        public const string NUGET_MODULES_META_FILES_PATH = @"App_Data\CMSModules\CMSInstallation\Packages\";


        /// <summary>
        /// Relative path to directory containing meta files of installed modules (with trailing slash).
        /// </summary>
        /// <remarks>
        /// The folder denoted by this path contains uninstallation tokens (a token is a meta file). Such tokens allow the instance to uninstall a module
        /// for which it has a token.
        /// </remarks>
        internal const string INSTALLED_NUGET_MODULES_META_FILES_PATH = @"App_Data\CMSModules\CMSInstallation\Packages\Installed\";


        private static readonly object lockObject = new object();
        private static InstallableModulesManager mCurrent;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current instance.
        /// </summary>
        public static InstallableModulesManager Current
        {
            get
            {
                return LockHelper.Ensure(ref mCurrent, () => new InstallableModulesManager(), lockObject);
            }
        }


        /// <summary>
        /// Root path for resolving relative paths.
        /// </summary>
        private string RootPath
        {
            get
            {
                return SystemContext.WebApplicationPhysicalPath;
            }
        }


        /// <summary>
        /// Rooted path to directory containing module descriptions managed by NuGet (with trailing slash).
        /// Each description file is expected to be named accordingly to <see cref="MODULE_DESCRIPTION_META_FILE_NAME_PATTERN"/>.
        /// </summary>
        private string NuGetModulesMetaFilesPath
        {
            get
            {
                return Path.Combine(RootPath, NUGET_MODULES_META_FILES_PATH);
            }
        }


        /// <summary>
        /// Rooted path to directory containing meta files of installed modules (with trailing slash).
        /// </summary>
        private string InstalledNuGetModulesMetaFilesPath
        {
            get
            {
                return Path.Combine(RootPath, INSTALLED_NUGET_MODULES_META_FILES_PATH);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Prepares the only instance of installable module manager.
        /// </summary>
        private InstallableModulesManager()
        {

        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets current state of installable modules.
        /// </summary>
        /// <returns>Object representing current state of installable modules.</returns>
        public InstallableModulesState GetCurrentState()
        {
            var nuGetModules = GetNuGetModules();
            var uninstallationTokens = GetUninstallationTokens();
            var installedModules = GetInstalledModules();

            return new InstallableModulesState(nuGetModules, uninstallationTokens, installedModules);
        }


        /// <summary>
        /// Gets list of basic module meta data for which the instance has an uninstallation token.
        /// </summary>
        /// <returns>List of uninstallation tokens.</returns>
        public List<BasicModuleInstallationMetaData> GetUninstallationTokens()
        {
            List<BasicModuleInstallationMetaData> result = new List<BasicModuleInstallationMetaData>();

            foreach (string tokenFileName in GetUninstallationTokenFileNames())
            {
                string name;
                string version;
                ParseBasicModuleDescriptionFileName(tokenFileName, out name, out version);

                result.Add(new BasicModuleInstallationMetaData(name, version));
            }

            return result;
        }


        /// <summary>
        /// Gets set of module names which are not installed in the system in the same version as their corresponding NuGet package meta file has
        /// and are omitted from initialization.
        /// </summary>
        /// <returns>Set of module names omitted from initialization.</returns>
        public ISet<string> GetModuleNamesOmittedFromInitialization()
        {
            // Installable modules with database representation
            var initializableModules = GetInitializableModules().Columns("ResourceName", "ResourceInstalledVersion").ToDictionary(x => x.ResourceName, x => x.ResourceInstalledVersion, StringComparer.InvariantCultureIgnoreCase);
            
            // Iterate NuGet modules and filter out those which are not installed in the database in proper version
            return GetNuGetModules()
                .Where(module => !initializableModules.ContainsKey(module.Name) || !InstallableModulesState.VersionsMatch(module.Version, initializableModules[module.Name]))
                .Select(x => x.Name)
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets enumeration of modules which have NuGet package meta file but are not installed in the system.
        /// Only modules which were not processed by installation process on this instance are returned.
        /// </summary>
        /// <param name="state">State of installable modules from which the result is determined.</param>
        /// <returns>Enumeration of modules for installation.</returns>
        public IEnumerable<BasicModuleInstallationMetaData> GetModulesToBeInstalled(InstallableModulesState state)
        {
            // Modules which are in NuGet but not in the DB are to be installed
            // Exclude modules, for which the instance has uninstallation token in proper version, from installation.
            // Prevents re-installation of a module when one instance uninstalls the module and another one has not up-to-date code base
            return state.NuGetInstalledModules.Where(it => !state.IsDatabaseModulePresent(it.Name) && !state.IsUninstallationTokenPresent(it.Name, it.Version));
        }


        /// <summary>
        /// <para>
        /// Gets enumeration of module pairs which have NuGet package meta file in newer version than the version installed in the system.
        /// Only modules which were processed by installation process on this instance are returned.
        /// </para>
        /// <para>
        /// Each pair consists of older version meta data and newer version meta data.
        /// </para>
        /// </summary>
        /// <param name="state">State of installable modules from which the result is determined.</param>
        /// <returns>Enumeration of module pairs for update.</returns>
        public IEnumerable<Tuple<BasicModuleInstallationMetaData, BasicModuleInstallationMetaData>> GetModulesToBeUpdated(InstallableModulesState state)
        {
            // Modules which are both in NuGet and in the DB, but the DB version is older, are suitable for update
            // Exclude modules, for which the instance does not have uninstallation token in proper version (to prevent unintended module update when temporarily switching connection strings)            
            return state.NuGetInstalledModules.Where(it => state.IsDatabaseModulePresentInLowerVersion(it.Name, it.Version))
                 .Select(it => new Tuple<BasicModuleInstallationMetaData, BasicModuleInstallationMetaData>(state.GetDatabaseModule(it.Name), it))
                 .Where(it => state.IsUninstallationTokenPresent(it.Item1.Name, it.Item1.Version));
        }


        /// <summary>
        /// Gets enumeration of modules which are installed in the system, but their NuGet package meta file is gone.
        /// Only modules which were processed by installation process on this instance are returned.
        /// </summary>
        /// <param name="state">State of installable modules from which the result is determined.</param>
        /// <returns>Enumeration of modules for uninstallation.</returns>
        public IEnumerable<BasicModuleInstallationMetaData> GetModulesToBeUninstalled(InstallableModulesState state)
        {
            // Modules which were removed from NuGet are suitable for uninstallation - but only instance which has uninstallation token can do so
            return state.DatabaseInstalledModules.Where(it => !state.IsNuGetModulePresent(it.ResourceName) && state.IsUninstallationTokenPresent(it.ResourceName, it.ResourceInstalledVersion))
                                                    .Select(it => new BasicModuleInstallationMetaData(it.ResourceName, it.ResourceInstalledVersion));
        }


        /// <summary>
        /// Gets enumeration of modules which are installed in the system, but their uninstallation token is not present.
        /// </summary>
        /// <param name="state">State of installable modules from which the result is determined.</param>
        /// <returns>Enumeration of installed modules without token.</returns>
        /// <remarks>
        /// Missing token for installed modules is usually the result of module installation in an environment where multiple instances
        /// share one database.
        /// </remarks>
        public IEnumerable<BasicModuleInstallationMetaData> GetInstalledModulesWithoutUninstallationToken(InstallableModulesState state)
        {
            // Take modules which do not have their token in proper version (the module must be present in both the DB and NuGet)
            Func<string, string, bool> tokenIsMissing = (name, version) => !state.IsUninstallationTokenPresent(name, version) && state.IsNuGetModulePresent(name, version);

            return state.DatabaseInstalledModules.Where(it => tokenIsMissing(it.ResourceName, it.ResourceInstalledVersion)).Select(it => new BasicModuleInstallationMetaData(it.ResourceName, it.ResourceInstalledVersion));
        }


        /// <summary>
        /// Gets enumeration of modules which were uninstalled from the system, but their uninstallation token is still present.
        /// </summary>
        /// <param name="state">State of installable modules from which the result is determined.</param>
        /// <returns>Enumeration of uninstalled modules which have redundant token.</returns>
        /// <remarks>
        /// Redundant token for a module is usually the result of module uninstallation in an environment where multiple instances
        /// share one database.
        /// </remarks>
        public IEnumerable<BasicModuleInstallationMetaData> GetUninstalledModulesWithUninstallationToken(InstallableModulesState state)
        {
            // Take modules where the module is not among installed (in both the DB and NuGet)
            var redundantTokens = state.UninstallationTokens.Where(it => !state.IsDatabaseModulePresent(it.Name, it.Version) && !state.IsNuGetModulePresent(it.Name, it.Version));

            return redundantTokens;
        }
        

        /// <summary>
        /// Marks given module as installed, or installed but in need of a restart.
        /// </summary>
        /// <param name="moduleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="restartPending">Whether module needs a restart to be ready.</param>
        public void MarkModuleAsInstalled(BasicModuleInstallationMetaData moduleInstallationMetaData, bool restartPending = false)
        {
            SetModuleInstallationState(moduleInstallationMetaData, restartPending ? ModuleInstallationState.INSTALLED_PENDING_RESTART : ModuleInstallationState.INSTALLED);
        }


        /// <summary>
        /// Notifies the manager about performed restart.
        /// </summary>
        public void RestartPerformed()
        {
            foreach (var installedModule in GetModulesByInstallationState(ModuleInstallationState.INSTALLED_PENDING_RESTART))
            {
                installedModule.ResourceInstallationState = ModuleInstallationState.INSTALLED;
                ResourceInfoProvider.SetResourceInfo(installedModule);
            }
        }


        /// <summary>
        /// Ensures uninstallation token for given <paramref name="moduleInstallationMetaData"/>.
        /// </summary>
        /// <param name="moduleInstallationMetaData">Basic module installation meta data.</param>
        /// <remarks>
        /// <para>
        /// Module uninstallation token allows for module uninstallation. Module uninstallation can be performed only
        /// on an instance which has the token. This prevents unintended uninstallation of module whose code base
        /// has not been distributed to all application servers yet.
        /// </para>
        /// <para>
        /// Moreover once an instance has the token, it will not reinstall the module. This prevents unintended reinstallation
        /// of module which has been uninstalled on one instance and the code base has not been synchronized yet.
        /// </para>
        /// </remarks>
        public void EnsureModuleUninstallationToken(BasicModuleInstallationMetaData moduleInstallationMetaData)
        {
            var metaFilesPath = InstalledNuGetModulesMetaFilesPath;

            // Ensure directory for tokens
            if (!Directory.Exists(metaFilesPath))
            {
                DirectoryHelper.EnsureDiskPath(metaFilesPath, RootPath);
            }

            var tokenMetaFileName = GetTokenMetaFileName(moduleInstallationMetaData);

            // Copy the token
            File.Copy(NuGetModulesMetaFilesPath + tokenMetaFileName, metaFilesPath + tokenMetaFileName, true);
        }


        /// <summary>
        /// Removes uninstallation token for given <paramref name="moduleInstallationMetaData"/>.
        /// Does nothing when the token does not exist.
        /// </summary>
        /// <param name="moduleInstallationMetaData">Basic module installation meta data.</param>
        /// <remarks>
        /// <para>
        /// Module uninstallation token allows for module uninstallation. Module uninstallation can be performed only
        /// on an instance which has the token. This prevents unintended uninstallation of module whose code base
        /// has not been distributed to all application servers yet.
        /// </para>
        /// <para>
        /// Moreover once an instance has the token, it will not reinstall the module. This prevents unintended reinstallation
        /// of module which has been uninstalled on one instance and the code base has not been synchronized yet.
        /// </para>
        /// </remarks>
        public void RemoveModuleUninstallationToken(BasicModuleInstallationMetaData moduleInstallationMetaData)
        {
            var tokenMetaFileName = GetTokenMetaFileName(moduleInstallationMetaData);

            var tokeMetaFilePath = InstalledNuGetModulesMetaFilesPath + tokenMetaFileName;

            // Remove the token if it exists
            if (File.Exists(tokeMetaFilePath))
            {
                File.Delete(tokeMetaFilePath);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Sets state of installable module to desired state.
        /// </summary>
        /// <param name="moduleInstallationMetaData">Basic module installation meta data.</param>
        /// <param name="state">Installation state.</param>
        private void SetModuleInstallationState(BasicModuleInstallationMetaData moduleInstallationMetaData, string state)
        {
            var moduleInfo = ResourceInfoProvider.GetResourceInfo(moduleInstallationMetaData.Name);
            if (moduleInfo == null)
            {
                throw new ArgumentException(String.Format("Module with name '{0}' was not found.", moduleInstallationMetaData.Name), "moduleInstallationMetaData");
            }

            moduleInfo.ResourceInstallationState = state;
            moduleInfo.ResourceInstalledVersion = moduleInstallationMetaData.Version;

            ResourceInfoProvider.SetResourceInfo(moduleInfo);
        }


        /// <summary>
        /// Gets modules which are initializable.
        /// </summary>
        /// <returns>Object query for initializable modules</returns>
        private ObjectQuery<ResourceInfo> GetInitializableModules()
        {
            return ResourceInfoProvider.GetResources().WhereEquals("ResourceInstallationState", ModuleInstallationState.INSTALLED).Or().WhereEquals("ResourceInstallationState", ModuleInstallationState.INSTALLED_PENDING_RESTART);
        }


        /// <summary>
        /// Gets modules in given installation state.
        /// </summary>
        /// <param name="installationState">Module's installation state.</param>
        private ObjectQuery<ResourceInfo> GetModulesByInstallationState(string installationState)
        {
            return ResourceInfoProvider.GetResources().WhereEquals("ResourceInstallationState", installationState);
        }


        /// <summary>
        /// Gets list of modules installed in the database.
        /// </summary>
        /// <returns>List of installed modules.</returns>
        private List<ResourceInfo> GetInstalledModules()
        {
            return ResourceInfoProvider.GetResources().WhereNotEmpty("ResourceInstallationState").Columns("ResourceName", "ResourceInstalledVersion").ToList();
        }


        /// <summary>
        /// Gets token meta file name for module meta data.
        /// </summary>
        /// <param name="moduleInstallationMetaData">Basic module installation meta data.</param>
        /// <returns>Token meta file name.</returns>
        private static string GetTokenMetaFileName(BasicModuleInstallationMetaData moduleInstallationMetaData)
        {
            return String.Format(MODULE_DESCRIPTION_META_FILE_NAME_PATTERN, moduleInstallationMetaData.Name, moduleInstallationMetaData.Version);
        }


        /// <summary>
        /// Loads description of modules installed by NuGet.
        /// </summary>
        private List<BasicModuleInstallationMetaData> GetNuGetModules()
        {
            var result = new List<BasicModuleInstallationMetaData>();

            foreach (string descriptionFileName in GetNuGetModuleDescriptionFileNames())
            {
                string name;
                string version;
                ParseBasicModuleDescriptionFileName(descriptionFileName, out name, out version);

                result.Add(new BasicModuleInstallationMetaData(name, version));
        }

            return result;
        }


        /// <summary>
        /// Parses module name and version from basic module description meta file name.
        /// The file name has to follow the <see cref="MODULE_DESCRIPTION_META_FILE_NAME_PATTERN"/> format.
        /// </summary>
        /// <param name="descriptionFileName">Basic module description meta file name.</param>
        /// <param name="name">Module name parsed from file name.</param>
        /// <param name="version">Module version parsed from file name.</param>
        /// <exception cref="SystemIO.InvalidDataException">Thrown when file name has not the expected format.</exception>
        private void ParseBasicModuleDescriptionFileName(string descriptionFileName, out string name, out string version)
        {
            try
            {
                // Trim the .xml extension from <module>_<version>.xml
                var descriptionFileNameExtensionless = descriptionFileName.Substring(0, descriptionFileName.Length - 4);

                // Split <module>_<version>
                var delimiterIndex = descriptionFileNameExtensionless.LastIndexOf('_');
                name = descriptionFileNameExtensionless.Substring(0, delimiterIndex);
                version = descriptionFileNameExtensionless.Substring(delimiterIndex + 1);
            }
            catch (Exception ex)
            {
                throw new SystemIO.InvalidDataException("The module description meta file name '" + descriptionFileName + "' has an unexpected format. Expected format is '<name>_<version>.xml'.", ex);
            }
        }


        /// <summary>
        /// Gets enumeration of meta file names (managed by NuGet) containing description of modules.
        /// </summary>
        /// <returns>Enumeration of meta file names.</returns>
        private IEnumerable<string> GetNuGetModuleDescriptionFileNames()
        {
            var metaFilePath = NuGetModulesMetaFilesPath;
            try
            {
                return Directory.GetFiles(metaFilePath).Select(it => it.Substring(metaFilePath.Length).TrimStart('\\'));
            }
            catch (SystemIO.DirectoryNotFoundException)
            {
                // Try to prevent throwing unnecessary exception the next time the method is called and return empty enumeration
                TryEnsureMetaFilesPath(metaFilePath, RootPath);

                return new List<string>();
            }
        }


        /// <summary>
        /// Gets enumeration of uninstallation tokens (meta files).
        /// </summary>
        /// <returns>Enumeration of meta file names.</returns>
        private IEnumerable<string> GetUninstallationTokenFileNames()
        {
            var metaFilesPath = InstalledNuGetModulesMetaFilesPath;

            try
            {
                return Directory.GetFiles(metaFilesPath).Select(it => it.Substring(metaFilesPath.Length).TrimStart('\\'));
            }
            catch (SystemIO.DirectoryNotFoundException)
            {
                // Try to prevent throwing unnecessary exception the next time the method is called and return empty enumeration
                TryEnsureMetaFilesPath(metaFilesPath, RootPath);

                return new List<string>();
            }
        }


        /// <summary>
        /// Tries to ensure meta files path.
        /// </summary>
        /// <param name="metaFilesPath">Meta files path.</param>
        /// <param name="startingPath">Starting path.</param>
        private void TryEnsureMetaFilesPath(string metaFilesPath, string startingPath)
        {
            try
            {
                DirectoryHelper.EnsureDiskPath(metaFilesPath, startingPath);
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogEvent("I", "InstallableModulesManager", "ENSUREMETAFILESPATH",
                    String.Format("Could not ensure installable modules meta files path '{0}'. The path is usually missing when no installable modules are present and it is not an error. However, the path is ensured during installable modules loading for performance reasons. You can create the path manually to resolve the issue. {1}", metaFilesPath, ex));
            }
        }

        #endregion
    }
}
