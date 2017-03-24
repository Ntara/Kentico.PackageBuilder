// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleFilePathRule
// Assembly: CMS.Modules, Version=9.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System.Collections.Generic;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Specifies path where module files are located and where they are installed.
	/// </summary>
	internal class ModuleFilePathRule
	{
		/// <summary>Creates new ModuleFilePath.</summary>
		/// <param name="sourceRelativePath">
		///     Path to module file or directory (relative to application root).
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" />
		/// </param>
		/// <param name="targetRelativePath">
		///     Path (relative to application root) where files specified by
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" /> are installed.
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.TargetRelativePath" />
		/// </param>
		/// <param name="exclude">
		///     List of file (or directory) paths that are excluded from
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" />.
		/// </param>
		public ModuleFilePathRule(string sourceRelativePath, string targetRelativePath = null,
			IEnumerable<string> exclude = null)
		{
			SourceRelativePath = sourceRelativePath;
			TargetRelativePath = targetRelativePath;
			Exclude = exclude ?? new List<string>();
		}

		/// <summary>
		///     Gets path (relative to application root) to module's file or directory (directory's path ends with '\').
		///     File (or directory's content) is included to the module's package.
		/// </summary>
		public string SourceRelativePath { get; private set; }

		/// <summary>
		///     Gets path (relative to application root) where files specified by
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" /> are installed.
		///     If <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" /> is directory, all its
		///     subdirectories are copied to target path.
		///     ResolvedFiles are copied to an application's root folder when no target path is specified.
		/// </summary>
		public string TargetRelativePath { get; private set; }

		/// <summary>
		///     Gets list of file (or directory) paths that are excluded from
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFilePathRule.SourceRelativePath" />.
		/// </summary>
		public IEnumerable<string> Exclude { get; private set; }
	}
}