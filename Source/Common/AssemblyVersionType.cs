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
	/// Represents an assembly version attribute type.
	/// </summary>
	public enum AssemblyVersionType
	{
		/// <summary>
		/// Represents the AssemblyVersion attribute.
		/// </summary>
		AssemblyVersion,
		/// <summary>
		/// Represents the AssemblyFileVersion attribute.
		/// </summary>
		AssemblyFileVersion,
		/// <summary>
		/// Represents the AssemblyInformationalVersion attribute.
		/// </summary>
		AssemblyInformationalVersion
	}
}