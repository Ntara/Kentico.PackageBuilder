using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Modules
{
    /// <summary>
    /// Specifies module path rule resolver's context.
    /// </summary>
    internal class ModulePathRuleResolverContext
    {
        /// <summary>
        /// Gets root physical path. All paths are relative to this path.
        /// </summary>
        public string RootPath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets module name that is used for replacing substitutions in paths. This overrides the original module's name.
        /// </summary>
        public string ModuleName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the module in whose context the path rule is resolved.
        /// </summary>
        public ResourceInfo Module
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets list of file extensions that are excluded when resolving path rule in format '.ext'.
        /// </summary>
        public IEnumerable<string> ExcludedFileExtensions
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new ModulePathRuleResolverContext.
        /// </summary>
        /// <param name="rootPath">Root physical path. All paths are relative to this path.</param>
        /// <param name="module">The module in whose context the path rule is resolved.</param>
        /// <param name="moduleName">Module name that is used for replacing substitutions in paths. This overrides the original module's name.</param>
        /// <param name="excludedFileExtensions">List of file extensions that are excluded when resolving path rule in format '.ext'.</param>
        public ModulePathRuleResolverContext(string rootPath, ResourceInfo module, string moduleName = null, IEnumerable<string> excludedFileExtensions = null)
        {
            RootPath = rootPath;
            Module = module;
            ModuleName = String.IsNullOrWhiteSpace(moduleName) ? module.ResourceName : moduleName;
            ExcludedFileExtensions = excludedFileExtensions ?? new List<string>();
        }
    }
}
