using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CMS.Modules
{
    /// <summary>
    /// Contains installation well-known states of modules.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ModuleInstallationState
    {
        /// <summary>
        /// State of a module which is installed in the DB (ready to use).
        /// </summary>
        public const string INSTALLED = "installed";


        /// <summary>
        /// State of a module which is installed in the DB, but needs a restart to be fully operable.
        /// </summary>
        public const string INSTALLED_PENDING_RESTART = "installed_pending_restart";
    }
}
