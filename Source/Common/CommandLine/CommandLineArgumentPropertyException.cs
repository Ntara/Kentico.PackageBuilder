// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Security;

namespace Ntara.PackageBuilder
{
	[Serializable]
	internal class CommandLineArgumentPropertyException : CommandLineArgumentException
	{
		public CommandLineArgumentPropertyException(string argumentName, string propertyName, string message) : base(argumentName, message)
		{
			PropertyName = propertyName;
		}

		protected CommandLineArgumentPropertyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			PropertyName = info.GetString(nameof(PropertyName));
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

		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(PropertyName), PropertyName);
		}
	}
}