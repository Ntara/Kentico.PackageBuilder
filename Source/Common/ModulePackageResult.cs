// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// The result details for a completed module package.
	/// </summary>
	public class ModulePackageResult
	{
		/// <summary>
		/// The output directory for the module package.
		/// </summary>
		public string OutputDirectory { get; set; }

		/// <summary>
		/// The file name of the module package.
		/// </summary>
		public string PackageFileName { get; set; }
	}
}