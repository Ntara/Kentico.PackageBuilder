// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System.IO;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// The base interface for constructing a NuSpec manifest document.
	/// </summary>
	/// <remarks>For more information on the NuSpec manifest schema, see <see href="https://docs.microsoft.com/en-us/nuget/schema/nuspec"/></remarks>
	public interface IManifestBuilder
	{
		/// <summary>
		/// The physical file name used when constructing the final package.
		/// </summary>
		string PackageFileName { get; }

		/// <summary>
		/// Adds a file or collection of files to the manifest.
		/// </summary>
		/// <param name="source">The location of the file or files to include, subject to exclusions specified by the <paramref name="exclude"/> attribute.
		///   The path is relative to the CMS root file unless an absolute path is specified. The wildcard character <code>*</code> is allowed, and the
		///   double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="target">The relative path to the folder within the package where the <paramref name="source"/> files will be placed, which should begin
		///   with <code>lib</code>, <code>content</code>, or <code>tools</code>.</param>
		/// <param name="exclude">A semicolon-delimited list of files or file patterns to exclude from the <paramref name="source"/> location. The wildcard character <code>*</code>
		///   is allowed, and the double wildcard <code>**</code> implies a recursive folder search.</param>
		void AddFile(string source, string target, string exclude = null);

		/// <summary>
		/// Adds a file or collection of files to the <code>lib</code> folder.
		/// </summary>
		/// <param name="source">The location of the file or files to include, subject to exclusions specified by the <paramref name="exclude"/> attribute.
		///   The path is relative to the CMS root file unless an absolute path is specified. The wildcard character <code>*</code> is allowed, and the
		///   double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="destination">The relative path to the <code>lib</code> folder within the package where the <paramref name="source"/> files will be placed.</param>
		/// <param name="exclude">A semicolon-delimited list of files or file patterns to exclude from the <paramref name="source"/> location. The wildcard character <code>*</code>
		///   is allowed, and the double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="targetFramework">The target framework for the <code>lib</code> files (i.e. "net45", "net46").</param>
		void AddLibrary(string source, string destination, string exclude = null, string targetFramework = null);

		/// <summary>
		/// Adds a file or collection of files to the <code>content</code> folder.
		/// </summary>
		/// <param name="source">The location of the file or files to include, subject to exclusions specified by the <paramref name="exclude"/> attribute.
		///   The path is relative to the CMS root file unless an absolute path is specified. The wildcard character <code>*</code> is allowed, and the
		///   double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="destination">The relative path to the <code>content</code> folder within the package where the <paramref name="source"/> files will be placed.</param>
		/// <param name="exclude">A semicolon-delimited list of files or file patterns to exclude from the <paramref name="source"/> location. The wildcard character <code>*</code>
		///   is allowed, and the double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="targetFramework">The target framework for the <code>content</code> files (i.e. "net45", "net46").</param>
		void AddContent(string source, string destination, string exclude = null, string targetFramework = null);

		/// <summary>
		/// Adds a file or collection of files to the <code>tools</code> folder.
		/// </summary>
		/// <param name="source">The location of the file or files to include, subject to exclusions specified by the <paramref name="exclude"/> attribute.
		///   The path is relative to the CMS root file unless an absolute path is specified. The wildcard character <code>*</code> is allowed, and the
		///   double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="destination">The relative path to the <code>tools</code> folder within the package where the <paramref name="source"/> files will be placed.</param>
		/// <param name="exclude">A semicolon-delimited list of files or file patterns to exclude from the <paramref name="source"/> location. The wildcard character <code>*</code>
		///   is allowed, and the double wildcard <code>**</code> implies a recursive folder search.</param>
		/// <param name="targetFramework">The target framework for the <code>tools</code> files (i.e. "net45", "net46").</param>
		void AddTools(string source, string destination, string exclude = null, string targetFramework = null);

		/// <summary>
		/// Build the final NuSpec manifest to the specified <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">The stream to which to write the manifest.</param>
		void BuildManifest(Stream stream);
	}
}