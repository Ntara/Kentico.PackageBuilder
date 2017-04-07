using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Modules
{
    /// <summary>
    /// Specifies path where module files are located and where they are installed.
    /// </summary>
    internal class ModuleFilePathRule
    {
        /// <summary>
        /// Gets path (relative to application root) to module's file or directory (directory's path ends with '\'). 
        /// File (or directory's content) is included to the module's package.
        /// </summary>
        public string SourceRelativePath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets path (relative to application root) where files specified by <see cref="SourceRelativePath"/> are installed.
        /// If <see cref="SourceRelativePath"/> is directory, all its subdirectories are copied to target path. 
        /// ResolvedFiles are copied to an application's root folder when no target path is specified.
        /// </summary>
        public string TargetRelativePath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets list of file (or directory) paths that are excluded from <see cref="SourceRelativePath"/>.
        /// </summary>
        public IEnumerable<string> Exclude
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new ModuleFilePath.
        /// </summary>
        /// <param name="sourceRelativePath">Path to module file or directory (relative to application root). <see cref="SourceRelativePath"/></param>
        /// <param name="targetRelativePath">Path (relative to application root) where files specified by <see cref="SourceRelativePath"/> are installed. <see cref="TargetRelativePath"/></param>
        /// <param name="exclude">List of file (or directory) paths that are excluded from <see cref="SourceRelativePath"/>.</param>
        public ModuleFilePathRule(string sourceRelativePath, string targetRelativePath = null, IEnumerable<string> exclude = null)
        {
            SourceRelativePath = sourceRelativePath;
            TargetRelativePath = targetRelativePath;
            Exclude = exclude ?? new List<string>();
        }
    }
}
