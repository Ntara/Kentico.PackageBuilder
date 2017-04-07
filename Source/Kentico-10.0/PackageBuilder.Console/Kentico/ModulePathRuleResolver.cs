using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Allows you to resolve module path rule.
    /// </summary>
    internal class ModulePathRuleResolver
    {

        #region "Fields"

        private readonly ModulePathRuleResolverContext mContext;
        private readonly ModuleFilePathRule mPathRule;
        private List<ModuleFile> mResolvedFiles; 

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets resolver's context.
        /// </summary>
        public ModulePathRuleResolverContext ResolverContext
        {
            get
            {
                return mContext;
            }
        }


        /// <summary>
        /// Gets path rule that is being resolved.
        /// </summary>
        public ModuleFilePathRule PathRule
        {
            get
            {
                return mPathRule;
            }
        }


        /// <summary>
        /// List of files that match the <see cref="PathRule"/>.
        /// </summary>
        public List<ModuleFile> ResolvedFiles
        {
            get
            {
                return mResolvedFiles ?? (mResolvedFiles = GetResolvedFiles());
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new ModulePathRuleResolver for the given path rule.
        /// </summary>
        /// <param name="pathRule">Path rule that is being resolved.</param>
        /// <param name="context">Resolver's context.</param>
        public ModulePathRuleResolver(ModuleFilePathRule pathRule, ModulePathRuleResolverContext context)
        {
            mContext = context;
            mPathRule = pathRule;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets list of files that match the ModuleFilePathRule.
        /// </summary>
        /// <returns>List of files that match the ModuleFilePathRule.</returns>
        private List<ModuleFile> GetResolvedFiles()
        {
            string sourcePath = Path.Combine(mContext.RootPath, ResolvePath(mPathRule.SourceRelativePath));
            List<ModuleFile> paths = new List<ModuleFile>();

            if (sourcePath.EndsWithCSafe("\\"))
            {
                // Add all files located in directory except excluded files
                List<string> excluded = mPathRule.Exclude.Select(x => Path.Combine(sourcePath, ResolvePath(x))).ToList();
                foreach (var file in GetDirectoryFiles(sourcePath))
                {
                    if (excluded.Any(file.StartsWithCSafe) || mContext.ExcludedFileExtensions.Any(file.EndsWithCSafe))
                    {
                        continue;
                    }

                    string fileRelativePath = file.Substring(mContext.RootPath.Length).TrimStart('\\');
                    paths.Add(new ModuleFile(fileRelativePath, ResolveTargetPath(fileRelativePath)));
                }
            }
            else if (!mContext.ExcludedFileExtensions.Any(sourcePath.EndsWithCSafe) && FileExists(sourcePath))
            {
                paths.Add(new ModuleFile(sourcePath.Substring(mContext.RootPath.Length).TrimStart('\\'), ResolveTargetPath(sourcePath)));
            }

            return paths;
        }


        /// <summary>
        /// Resolves target path of the given file according to the file's source path, physical path and target path.
        /// </summary>
        /// <param name="file">File's physical path.</param>
        /// <returns>File's target path.</returns>
        private string ResolveTargetPath(string file)
        {
            string targetPath = ResolvePath(mPathRule.TargetRelativePath);
            if (mPathRule.SourceRelativePath.EndsWithCSafe("\\"))
            {
                string sourcePath = ResolvePath(mPathRule.SourceRelativePath);
                targetPath = Path.Combine(targetPath, file.Substring(sourcePath.Length));
            }

            return targetPath;
        }


        /// <summary>
        /// Resolves given path replacing the substitutions in it.
        /// </summary>
        /// <param name="path">Path that can contain substitutions.</param>
        /// <returns>Resolved path.</returns>
        private string ResolvePath(string path)
        {
            return String.IsNullOrEmpty(path) ? String.Empty : String.Format(path, mContext.ModuleName, mContext.Module.ResourceVersion);
        }


        /// <summary>
        /// Gets list of all files in directory and its subdirectories.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <returns>List of all files in directory and its subdirectories.</returns>
        protected virtual IEnumerable<string> GetDirectoryFiles(string path)
        {
            List<string> files = new List<string>();

            if (Directory.Exists(path))
            {
                List<string> directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
                directories.Add(path);
                foreach (var directory in directories)
                {
                    files.AddRange(Directory.GetFiles(directory));
                }
            }

            return files;
        }


        /// <summary>
        /// Checsk if file with given path exists.
        /// </summary>
        /// <param name="path">Physical file path.</param>
        /// <returns>True if file exists, false otherwise.</returns>
        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        #endregion
    }
}
