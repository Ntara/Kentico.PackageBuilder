// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

namespace Ntara.PackageBuilder
{
	internal class CommandLineVersion
	{
		public CommandLineVersion()
		{
			// Set default version type
			AssemblyVersionType = AssemblyVersionType.AssemblyFileVersion;
		}

		[ArgumentProperty(CommandLineArguments.ObjectProperties.VersionValue, "CommandLine_VersionObject_Value_Description")]
		public string Value { get; set; }

		[ArgumentProperty(CommandLineArguments.ObjectProperties.VersionAssembly, "CommandLine_VersionObject_Assembly_Description")]
		public string Assembly { get; set; }

		[ArgumentProperty(CommandLineArguments.ObjectProperties.VersionAssemblyAttribute, "CommandLine_VersionObject_AssemblyAttribute_Description")]
		public AssemblyVersionType AssemblyVersionType { get; set; }
	}
}