// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModulePathRuleResolverContext
// Assembly: CMS.Modules, Version=10.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System.Collections.Generic;

using CMS.Modules;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>Specifies module path rule resolver's context.</summary>
	internal class ModulePathRuleResolverContext
	{
		/// <summary>Creates new ModulePathRuleResolverContext.</summary>
		/// <param name="rootPath">Root physical path. All paths are relative to this path.</param>
		/// <param name="module">The module in whose context the path rule is resolved.</param>
		/// <param name="moduleName">
		///     Module name that is used for replacing substitutions in paths. This overrides the original
		///     module's name.
		/// </param>
		/// <param name="excludedFileExtensions">
		///     List of file extensions that are excluded when resolving path rule in format
		///     '.ext'.
		/// </param>
		public ModulePathRuleResolverContext(string rootPath, ResourceInfo module, string moduleName = null,
			IEnumerable<string> excludedFileExtensions = null)
		{
			RootPath = rootPath;
			Module = module;
			ModuleName = string.IsNullOrWhiteSpace(moduleName) ? module.ResourceName : moduleName;
			ExcludedFileExtensions = excludedFileExtensions ?? new List<string>();
		}

		/// <summary>
		///     Gets root physical path. All paths are relative to this path.
		/// </summary>
		public string RootPath { get; private set; }

		/// <summary>
		///     Gets module name that is used for replacing substitutions in paths. This overrides the original module's name.
		/// </summary>
		public string ModuleName { get; private set; }

		/// <summary>
		///     Gets the module in whose context the path rule is resolved.
		/// </summary>
		public ResourceInfo Module { get; private set; }

		/// <summary>
		///     Gets list of file extensions that are excluded when resolving path rule in format '.ext'.
		/// </summary>
		public IEnumerable<string> ExcludedFileExtensions { get; private set; }
	}
}