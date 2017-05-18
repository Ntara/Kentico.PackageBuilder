// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using CMS.Base;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// Resolves a version from the specified assembly.
	/// </summary>
	public class AssemblyVersionResolver : IModuleVersionResolver
	{
		private readonly string _rootPath;
		private readonly string _assemblyFile;
		private readonly AssemblyVersionType _assemblyVersionType;

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyVersionResolver"/> class.
		/// </summary>
		/// <param name="assemblyFile">The assembly file to load.</param>
		/// <param name="assemblyVersionType">The version attribute type from which to extract version information.</param>
		/// <exception cref="ArgumentException">The <paramref name="assemblyFile"/> is null or empty.</exception>
		public AssemblyVersionResolver(string assemblyFile, AssemblyVersionType assemblyVersionType = AssemblyVersionType.AssemblyFileVersion)
		{
			if (string.IsNullOrEmpty(assemblyFile))
			{
				throw new ArgumentException(CommonResources.ArgumentException_AssemblyNullOrEmpty, nameof(assemblyFile));
			}

			_rootPath = SystemContext.WebApplicationPhysicalPath;
			_assemblyFile = assemblyFile;
			_assemblyVersionType = assemblyVersionType;
		}

		/// <summary>
		/// Retrieves the assembly version based on the specified version type.
		/// </summary>
		/// <returns>The version of the specified assembly.</returns>
		/// <exception cref="FileNotFoundException">The assembly file could not be found.</exception>
		/// <exception cref="NotSupportedException">The assembly version type is unknown or unsupported.</exception>
		/// <exception cref="VersionNotFoundException">The requested version attribute was not defined for the assembly.</exception>
		public string GetVersion()
		{
			var assemblyFilePath = GetAssemblyFilePath(_assemblyFile);

			// Assert assembly file exists
			if (!File.Exists(assemblyFilePath))
			{
				var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.FileNotFoundException_AssemblyNotFound, assemblyFilePath);
				throw new FileNotFoundException(errorMessage, assemblyFilePath);
			}

			var moduleAssembly = Assembly.UnsafeLoadFrom(assemblyFilePath);

			string moduleVersion;

			switch (_assemblyVersionType)
			{
				case AssemblyVersionType.AssemblyInformationalVersion:
					moduleVersion = moduleAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
					break;
				case AssemblyVersionType.AssemblyFileVersion:
					moduleVersion = moduleAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
					break;
				case AssemblyVersionType.AssemblyVersion:
					moduleVersion = moduleAssembly.GetName().Version.ToString();
					break;
				default:
					var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.NotSupportedException_UnknownAssemblyAttribute, _assemblyVersionType);
					throw new NotSupportedException(errorMessage);
			}

			if (string.IsNullOrEmpty(moduleVersion))
			{
				var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.VersionNotFoundException_AssemblyVersionNotResolved, _assemblyFile, _assemblyVersionType);
				throw new VersionNotFoundException(errorMessage);
			}

			return moduleVersion;
		}

		#region |-- Support Methods --|

		private string GetAssemblyFileName(string assembly)
		{
			if (assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
				assembly.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				return assembly;
			}

			return string.Format(CultureInfo.InvariantCulture, "{0}.dll", assembly);
		}

		private string GetAssemblyFilePath(string assembly)
		{
			var assemblyFileName = GetAssemblyFileName(assembly);
			var assemblyFilePath = assemblyFileName;

			// Ensure fully qualified assembly path
			if (!Path.IsPathRooted(assemblyFilePath))
			{
				assemblyFilePath = Path.Combine(_rootPath, assemblyFileName);

				if (!File.Exists(CMS.IO.Path.EnsureBackslashes(assemblyFilePath)) &&
					!assemblyFileName.Contains("\\") &&
					!assemblyFileName.Contains("/"))
				{
					assemblyFilePath = Path.Combine(Path.Combine(_rootPath, "bin"), assemblyFileName);
				}
			}

			// Ensure Windows path format with backslashes
			return CMS.IO.Path.EnsureBackslashes(assemblyFilePath);
		}

		#endregion
	}
}