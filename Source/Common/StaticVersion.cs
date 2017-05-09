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
	public struct StaticVersion : IModuleVersionResolver, IEquatable<StaticVersion>
	{
		#region |-- Constructors --|

		private readonly string _value;

		private StaticVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new ArgumentException(CommonResources.ArgumentException_ModuleVersionRequired, nameof(version));
			}

			_value = version;
		}

		#endregion

		#region |-- IModuleVersionResolver Implementation --|

		string IModuleVersionResolver.GetVersion()
		{
			return _value;
		}

		#endregion

		#region |-- IEquatable Implementation --|

		/// <summary>
		/// Determines whether two <see cref="StaticVersion"/> objects represent the same version.
		/// </summary>
		/// <param name="left">The first <see cref="StaticVersion"/> to compare.</param>
		/// <param name="right">The second <see cref="StaticVersion"/> to compare.</param>
		/// <returns><see langword="true"/>, if the <see cref="StaticVersion"/> objects are determined to be equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(StaticVersion left, StaticVersion right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether two <see cref="StaticVersion"/> objects represent the different versions.
		/// </summary>
		/// <param name="left">The first <see cref="StaticVersion"/> to compare.</param>
		/// <param name="right">The second <see cref="StaticVersion"/> to compare.</param>
		/// <returns><see langword="true"/>, if the <see cref="StaticVersion"/> objects are determined to be different; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(StaticVersion left, StaticVersion right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Determines if the <see cref="StaticVersion"/> object is equal to the parameter.
		/// </summary>
		/// <param name="other">The <see cref="StaticVersion"/> object to compare to the calling object.</param>
		/// <returns><see langword="true"/>, if the <see cref="StaticVersion"/> objects are equal; otherwise, <see langword="false"/>.</returns>
		public bool Equals(StaticVersion other)
		{
			return string.Equals(_value, other._value);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns><see langword="true"/>, if the current object is equal to the <paramref name="obj"/> parameter; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (!(obj is StaticVersion)) return false;

			return Equals((StaticVersion)obj);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="StaticVersion"/>.
		/// </summary>
		/// <returns>A hash code for the current <see cref="StaticVersion"/>.</returns>
		public override int GetHashCode()
		{
			return _value != null ? _value.GetHashCode() : 0;
		}

		#endregion

		#region |-- Object Overrides --|

		/// <inheritdoc />
		public override string ToString()
		{
			return _value;
		}

		#endregion

		#region |-- Implicit Operator Overrides --|

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
			return version.ToString();
		}

		#endregion
	}
}