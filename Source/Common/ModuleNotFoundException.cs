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
	/// The exception thrown when the specified module could not be resolved.
	/// </summary>
	public class ModuleNotFoundException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class with message details.
		/// </summary>
		/// <param name="message">A description of the error.</param>
		public ModuleNotFoundException(string message) : base(message)
		{

		}
	}
}
