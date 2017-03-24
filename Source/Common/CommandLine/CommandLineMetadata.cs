// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

namespace Ntara.PackageBuilder
{
	internal class CommandLineMetadata
	{
		[ArgumentProperty(CommandLineArguments.ObjectProperties.MetadataId, "CommandLine_MetadataObject_Id_Description")]
		public string Id { get; set; }

		[ArgumentProperty(CommandLineArguments.ObjectProperties.MetadataTitle, "CommandLine_MetadataObject_Title_Description")]
		public string Title { get; set; }

		[ArgumentProperty(CommandLineArguments.ObjectProperties.MetadataDescription, "CommandLine_MetadataObject_Description_Description")]
		public string Description { get; set; }

		[ArgumentProperty(CommandLineArguments.ObjectProperties.MetadataAuthors, "CommandLine_MetadataObject_Authors_Description")]
		public string Authors { get; set; }
	}
}