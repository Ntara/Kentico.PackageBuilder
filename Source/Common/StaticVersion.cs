// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// Represents a static module version that implements <see cref="IModuleVersionResolver"/>.
	/// </summary>
	public struct StaticVersion : IModuleVersionResolver
	{
		private readonly string _value;

		private StaticVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new ArgumentException(CommonResources.ArgumentException_ModuleVersionRequired, nameof(version));
			}

			_value = version;
		}

		string IModuleVersionResolver.GetVersion()
		{
			return _value;
		}

		/// <summary>
		/// Implicitly converts a string to a <see cref="StaticVersion"/>.
		/// </summary>
		/// <param name="version">The static string version.</param>
		/// <returns>The version as a <see cref="StaticVersion"/>.</returns>
		public static implicit operator StaticVersion(string version)
		{
			return new StaticVersion(version);
		}

		/// <summary>
		/// Implicitly converts a <see cref="StaticVersion"/> to a string.
		/// </summary>
		/// <param name="version">The <see cref="StaticVersion"/>.</param>
		/// <returns>The <see cref="StaticVersion"/> as a string.</returns>
		public static implicit operator string(StaticVersion version)
		{
			return version._value;
		}
	}
}