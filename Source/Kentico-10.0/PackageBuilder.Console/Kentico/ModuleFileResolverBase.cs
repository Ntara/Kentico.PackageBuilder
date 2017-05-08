using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CMS.Modules
{
    /// <summary>
    /// Base class for resolving paths to all module files and folders related to installation packages.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class ModuleFileResolverBase
    {
        /// <summary>
        /// File name pattern for import/export package.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// Substitution '{1}' is replaced by the module version.
        /// </remarks>
        protected const string EXPORT_PACKAGE_FILE_NAME_PATTERN = InstallableModulesManager.MODULE_DESCRIPTION_PATTERN + ".zip";


        /// <summary>
        /// Path pattern for module installation directory.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        protected const string INSTALLATION_DIRECTORY_PATH_PATTERN = @"App_Data\CMSModules\{0}\Install";


        /// <summary>
        /// Path pattern for module update directory.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        protected const string UPDATE_DIRECTORY_PATH_PATTERN = @"App_Data\CMSModules\{0}\Update";


        /// <summary>
        /// Path pattern for module uninstallation directory.
        /// </summary>
        /// <remarks>
        /// Substitution '{0}' is replaced by the module codename.
        /// </remarks>
        protected const string UNINSTALLATION_DIRECTORY_PATH_PATTERN = @"App_Data\CMSModules\{0}\Uninstall";


        /// <summary>
        /// Name of SQL script executed before data import.
        /// </summary>
        protected const string SQL_BEFORE_FILE_NAME = "before.sql";


        /// <summary>
        /// Name of SQL script executed after data import.
        /// </summary>
        protected const string SQL_AFTER_FILE_NAME = "after.sql";


        /// <summary>
        /// Performs substitution in path pattern.
        /// </summary>
        /// <param name="path">Path in which to perform the substitution.</param>
        /// <param name="codename">Module name.</param>
        /// <param name="version">Module version.</param>
        /// <returns>Substituted path.</returns>
        protected string SubstitutePath(string path, string codename, string version)
        {
            return String.Format(path, codename, version);
        }
    }
}
