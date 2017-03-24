// Decompiled with JetBrains decompiler
// Type: CMS.Modules.ModulePathRuleResolver
// Assembly: CMS.Modules, Version=9.0.0.0, Culture=neutral, PublicKeyToken=834b12a258f213f9

using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.IO;

namespace Ntara.PackageBuilder.Kentico
{
	/// <summary>Allows you to resolve module path rule.</summary>
	internal class ModulePathRuleResolver
	{
		private List<ModuleFile> mResolvedFiles;

		/// <summary>
		///     Creates new ModulePathRuleResolver for the given path rule.
		/// </summary>
		/// <param name="pathRule">Path rule that is being resolved.</param>
		/// <param name="context">Resolver's context.</param>
		public ModulePathRuleResolver(ModuleFilePathRule pathRule, ModulePathRuleResolverContext context)
		{
			ResolverContext = context;
			PathRule = pathRule;
		}

		/// <summary>Gets resolver's context.</summary>
		public ModulePathRuleResolverContext ResolverContext { get; }

		/// <summary>Gets path rule that is being resolved.</summary>
		public ModuleFilePathRule PathRule { get; }

		/// <summary>
		///     List of files that match the <see cref="P:Ntara.PackageBuilder.Kentico.ModulePathRuleResolver.PathRule" />.
		/// </summary>
		public List<ModuleFile> ResolvedFiles
		{
			get { return mResolvedFiles ?? (mResolvedFiles = GetResolvedFiles()); }
		}

		/// <summary>Gets list of files that match the ModuleFilePathRule.</summary>
		/// <returns>List of files that match the ModuleFilePathRule.</returns>
		private List<ModuleFile> GetResolvedFiles()
		{
			var sourcePath = Path.Combine(ResolverContext.RootPath, ResolvePath(PathRule.SourceRelativePath));
			var moduleFileList = new List<ModuleFile>();
			if (sourcePath.EndsWithCSafe("\\"))
			{
				var list = PathRule.Exclude.Select(x => Path.Combine(sourcePath, ResolvePath(x))).ToList();
				foreach (var directoryFile in GetDirectoryFiles(sourcePath))
					if (!list.Any(directoryFile.StartsWithCSafe) &&
					    !ResolverContext.ExcludedFileExtensions.Any(directoryFile.EndsWithCSafe))
					{
						var str = directoryFile.Substring(ResolverContext.RootPath.Length).TrimStart('\\');
						moduleFileList.Add(new ModuleFile(str, ResolveTargetPath(str)));
					}
			}
			else if (!ResolverContext.ExcludedFileExtensions.Any(sourcePath.EndsWithCSafe) && FileExists(sourcePath))
			{
				moduleFileList.Add(new ModuleFile(sourcePath.Substring(ResolverContext.RootPath.Length).TrimStart('\\'),
					ResolveTargetPath(sourcePath)));
			}
			return moduleFileList;
		}

		/// <summary>
		///     Resolves target path of the given file according to the file's source path, physical path and target path.
		/// </summary>
		/// <param name="file">File's physical path.</param>
		/// <returns>File's target path.</returns>
		private string ResolveTargetPath(string file)
		{
			var path1 = ResolvePath(PathRule.TargetRelativePath);
			if (PathRule.SourceRelativePath.EndsWithCSafe("\\"))
			{
				var path2 = ResolvePath(PathRule.SourceRelativePath);
				path1 = Path.Combine(path1, file.Substring(path2.Length));
			}
			return path1;
		}

		/// <summary>
		///     Resolves given path replacing the substitutions in it.
		/// </summary>
		/// <param name="path">Path that can contain substitutions.</param>
		/// <returns>Resolved path.</returns>
		private string ResolvePath(string path)
		{
			if (!string.IsNullOrEmpty(path))
				return string.Format(path, ResolverContext.ModuleName, ResolverContext.Module.ResourceVersion);
			return string.Empty;
		}

		/// <summary>
		///     Gets list of all files in directory and its subdirectories.
		/// </summary>
		/// <param name="path">Directory path.</param>
		/// <returns>List of all files in directory and its subdirectories.</returns>
		protected virtual IEnumerable<string> GetDirectoryFiles(string path)
		{
			var stringList = new List<string>();
			if (Directory.Exists(path))
			{
				var list = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
				list.Add(path);
				foreach (var path1 in list)
					stringList.AddRange(Directory.GetFiles(path1));
			}
			return stringList;
		}

		/// <summary>Checsk if file with given path exists.</summary>
		/// <param name="path">Physical file path.</param>
		/// <returns>True if file exists, false otherwise.</returns>
		protected virtual bool FileExists(string path)
		{
			return File.Exists(path);
		}
	}
}