using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;

using NuGet;

namespace CMS.Modules
{
    /// <summary>
    /// State object containing information regarding installable modules.
    /// </summary>
    internal class InstallableModulesState
    {
        #region "Fields"

        /// <summary>
        /// List with basic module installation meta data from the <see cref="InstallableModulesManager.NUGET_MODULES_META_FILES_PATH"/> folder.
        /// </summary>
        private readonly List<BasicModuleInstallationMetaData> mNuGetInstalledModules;


        /// <summary>
        /// List with module uninstallation tokens from the <see cref="InstallableModulesManager.INSTALLED_NUGET_MODULES_META_FILES_PATH"/> folder.
        /// </summary>
        private readonly List<BasicModuleInstallationMetaData> mUninstallationTokens;


        /// <summary>
        /// List of modules installed in the database.
        /// </summary>
        private readonly List<ResourceInfo> mDatabaseInstalledModules; 


        /// <summary>
        /// Dictionary with module name and version pairs. Contains the same entries as <see cref="mNuGetInstalledModules"/>.
        /// </summary>
        private Dictionary<string, string> mNuGetInstalledModuleVersions;


        /// <summary>
        /// Dictionary with uninstallation tokens for modules - set of versions for given name. Contains the same entries as <see cref="mUninstallationTokens"/>.
        /// </summary>
        /// <remarks>
        /// Token for certain module can be present in more than one version when upgrading.
        /// </remarks>
        private Dictionary<string, ISet<string>> mUninstallationTokenVersions;


        /// <summary>
        /// Dictionary with modules in the database - name and version pairs. Contains the same entries as <see cref="mDatabaseInstalledModules"/>.
        /// </summary>
        private Dictionary<string, string> mDatabaseInstalledModuleVersions; 

        #endregion


        #region "Properties"

        /// <summary>
        /// List with basic module installation meta data from the <see cref="InstallableModulesManager.NUGET_MODULES_META_FILES_PATH"/> folder.
        /// </summary>
        /// <seealso cref="IsDatabaseModulePresent(string)"/>
        public List<BasicModuleInstallationMetaData> NuGetInstalledModules
        {
            get
            {
                return mNuGetInstalledModules;
            }
        }


        /// <summary>
        /// List with module uninstallation tokens from the <see cref="InstallableModulesManager.INSTALLED_NUGET_MODULES_META_FILES_PATH"/> folder.
        /// </summary>
        /// <seealso cref="IsUninstallationTokenPresent"/>
        public List<BasicModuleInstallationMetaData> UninstallationTokens
        {
            get
            {
                return mUninstallationTokens;
            }
        }


        /// <summary>
        /// List of modules installed in the database.
        /// </summary>
        /// <seealso cref="IsDatabaseModulePresent(string)"/>
        public List<ResourceInfo> DatabaseInstalledModules
        {
            get
            {
                return mDatabaseInstalledModules;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Tells you whether module of given <paramref name="name"/> is present in any version.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <returns>True if the module is present, false otherwise.</returns>
        public bool IsNuGetModulePresent(string name)
        {
            return mNuGetInstalledModuleVersions.ContainsKey(name);
        }


        /// <summary>
        /// Tells you whether module of given <paramref name="name"/> is present in specified <paramref name="version"/>.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="version">Module version.</param>
        /// <returns>True if the module is present, false otherwise.</returns>
        public bool IsNuGetModulePresent(string name, string version)
        {
            return mNuGetInstalledModuleVersions.ContainsKey(name) && VersionsMatch(mNuGetInstalledModuleVersions[name], version);
        }


        /// <summary>
        /// Tells you whether uninstallation token for module of given <paramref name="name"/> is present in specified <paramref name="version"/>.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="version">Module version.</param>
        /// <returns>True if uninstallation token is present, false otherwise.</returns>
        public bool IsUninstallationTokenPresent(string name, string version)
        {
            return mUninstallationTokenVersions.ContainsKey(name) && mUninstallationTokenVersions[name].Any(it => VersionsMatch(it, version));
        }


        /// <summary>
        /// Tells you whether module of given <paramref name="name"/> is present in any version in the database.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <returns>True if the module is present, false otherwise.</returns>
        public bool IsDatabaseModulePresent(string name)
        {
            return mDatabaseInstalledModuleVersions.ContainsKey(name);
        }


        /// <summary>
        /// Tells you whether module of given <paramref name="name"/> is present in specified <paramref name="version"/> in the database.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="version">Module version.</param>
        /// <returns>True if the module is present, false otherwise.</returns>
        public bool IsDatabaseModulePresent(string name, string version)
        {
            return mDatabaseInstalledModuleVersions.ContainsKey(name) && VersionsMatch(mDatabaseInstalledModuleVersions[name], version);
        }


        /// <summary>
        /// Tells you whether module of given <paramref name="name"/> is present in version which is less than <paramref name="newVersion"/>.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="newVersion">Module version.</param>
        /// <returns>True if the module is present, false otherwise.</returns>
        public bool IsDatabaseModulePresentInLowerVersion(string name, string newVersion)
        {
            return mDatabaseInstalledModuleVersions.ContainsKey(name) && SemanticVersion.Parse(mDatabaseInstalledModuleVersions[name]) < SemanticVersion.Parse(newVersion);
        }


        /// <summary>
        /// Gets module of given <paramref name="name"/> from <see cref="DatabaseInstalledModules"/>.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <returns>Module of given <paramref name="name"/>.</returns>
        /// <remarks>
        /// Use <see cref="IsDatabaseModulePresent(string)"/>, <see cref="IsDatabaseModulePresent(string, string)"/> or <see cref="IsDatabaseModulePresentInLowerVersion"/> to determine whether module
        /// of name <paramref name="name"/> exists in the database.
        /// </remarks>
        public BasicModuleInstallationMetaData GetDatabaseModule(string name)
        {
            return new BasicModuleInstallationMetaData(name, mDatabaseInstalledModuleVersions[name]);
        }


        /// <summary>
        /// Tells you whether two version strings represent the same version or not.
        /// </summary>
        /// <param name="version1">Version string.</param>
        /// <param name="version2">Version string.</param>
        /// <returns>True if <paramref name="version1"/> represents the same version as <paramref name="version2"/>.</returns>
        public static bool VersionsMatch(string version1, string version2)
        {
            // Currently a simple comparison is enough
            return version1.EqualsCSafe(version2);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Prepares lookup dictionaries.
        /// </summary>
        private void FillDictionaries()
        {
            mNuGetInstalledModuleVersions = mNuGetInstalledModules.ToDictionary(it => it.Name, it => it.Version, StringComparer.InvariantCultureIgnoreCase);
            mDatabaseInstalledModuleVersions = mDatabaseInstalledModules.ToDictionary(it => it.ResourceName, it => it.ResourceInstalledVersion);

            mUninstallationTokenVersions = new Dictionary<string, ISet<string>>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var token in mUninstallationTokens)
            {
                if (mUninstallationTokenVersions.ContainsKey(token.Name))
                {
                    mUninstallationTokenVersions[token.Name].Add(token.Version);
                }
                else
                {
                    mUninstallationTokenVersions[token.Name] = new HashSet<string>
                    {
                        token.Version
                    };
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new installable modules state.
        /// </summary>
        /// <param name="installedNuGetModules">List of modules installed by NuGet.</param>
        /// <param name="uninstallationTokens">List of uninstallation tokens.</param>
        /// <param name="installedDatabaseModules">List of modules installed in the database.</param>
        public InstallableModulesState(List<BasicModuleInstallationMetaData> installedNuGetModules, List<BasicModuleInstallationMetaData> uninstallationTokens, List<ResourceInfo> installedDatabaseModules)
        {
            mNuGetInstalledModules = installedNuGetModules;
            mUninstallationTokens = uninstallationTokens;
            mDatabaseInstalledModules = installedDatabaseModules;

            // Prepare the dictionaries for quick lookup
            FillDictionaries();
        }
        
        #endregion
    }
}
