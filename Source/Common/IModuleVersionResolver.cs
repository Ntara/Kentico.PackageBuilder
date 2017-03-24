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
	/// The base interface for building or retrieving a module package version.
	/// </summary>
	public interface IModuleVersionResolver
	{
		/// <summary>
		/// Builds or retrieves the module package version.
		/// </summary>
		/// <returns>The semantic version of the module.</returns>
		string GetVersion();
	}
}