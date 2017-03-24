// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModuleFile
// Assembly: CMS.Modules, Version=10.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>
	///     Specifies module file that will be included in the module package.
	/// </summary>
	internal class ModuleFile
	{
		/// <summary>Creates new ModuleFile.</summary>
		/// <param name="sourceRelativePath">File's path (relative to root).</param>
		/// <param name="targetRelativePath">
		///     Path (relative to root) where file specified by <paramref name="sourceRelativePath" />
		///     is installed.
		/// </param>
		public ModuleFile(string sourceRelativePath, string targetRelativePath = null)
		{
			SourceRelativePath = sourceRelativePath;
			TargetRelativePath = targetRelativePath;
		}

		/// <summary>Gets file's path (relative to root).</summary>
		public string SourceRelativePath { get; private set; }

		/// <summary>
		///     Gets path (relative to root) where the file specified by
		///     <see cref="P:Ntara.PackageBuilder.Kentico.ModuleFile.SourceRelativePath" /> is installed.
		///     File is copied to an application's root folder when no target path is specified.
		/// </summary>
		public string TargetRelativePath { get; private set; }
	}
}