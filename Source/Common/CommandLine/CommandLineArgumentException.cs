// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;

namespace Ntara.PackageBuilder
{
	[Serializable]
	internal class CommandLineArgumentException : Exception
	{
		public CommandLineArgumentException(string argumentName, string message) : base(message)
		{
			ArgumentName = argumentName;
		}

		protected CommandLineArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ArgumentName = info.GetString(nameof(ArgumentName));
		}

		public string ArgumentName { get; }

		public override string Message
		{
			get
			{
				if (!string.IsNullOrEmpty(ArgumentName))
				{
					var argumentMessage = string.Format(CultureInfo.CurrentCulture, "Argument name: {0}", ArgumentName);
					return base.Message + Environment.NewLine + argumentMessage;
				}

				return base.Message;
			}
		}

		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(ArgumentName), ArgumentName);
		}
	}
}