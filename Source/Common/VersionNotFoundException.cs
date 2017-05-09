// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// The exception thrown when the specified assembly version could not be resolved.
	/// </summary>
	[Serializable]
	public sealed class VersionNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotFoundException"/> class.
		/// </summary>
		public VersionNotFoundException()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotFoundException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public VersionNotFoundException(string message) : base(message)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
		public VersionNotFoundException(string message, Exception innerException) : base(message, innerException)
		{

		}

		private VersionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}