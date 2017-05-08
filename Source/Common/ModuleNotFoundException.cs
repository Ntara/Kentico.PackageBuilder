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
	/// The exception thrown when the specified module could not be resolved.
	/// </summary>
	[Serializable]
	public sealed class ModuleNotFoundException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class.
		/// </summary>
		public ModuleNotFoundException()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public ModuleNotFoundException(string message) : base(message)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
		public ModuleNotFoundException(string message, Exception innerException) : base(message, innerException)
		{

		}

		private ModuleNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
