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
	internal class CommandLineArgumentPropertyException : CommandLineArgumentException
	{
		public CommandLineArgumentPropertyException(string argumentName, string propertyName, string message) : base(argumentName, message)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }

		public override string Message
		{
			get
			{
				if (!string.IsNullOrEmpty(PropertyName))
				{
					string argumentMessage = $"Property name: {PropertyName}";
					return base.Message + Environment.NewLine + argumentMessage;
				}

				return base.Message;
			}
		}
	}
}