// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

namespace Ntara.PackageBuilder
{
	internal static class CommandLineArguments
	{
		public const string Help = "-help";
		public const string Debug = "-debug";

		public const string Module = "-module";
		public const string NuSpecFile = "-nuspec";
		public const string OutputDirectory = "-output";
		public const string Metadata = "-metadata";
		public const string Properties = "-properties";
		public const string Version = "-version";

		public static class ObjectProperties
		{
			// Metadata
			public const string MetadataId = "id";
			public const string MetadataTitle = "title";
			public const string MetadataDescription = "description";
			public const string MetadataAuthors = "authors";

			// Version
			public const string VersionValue = "<value>";
			public const string VersionAssembly = "assembly";
			public const string VersionAssemblyAttribute = "assemblyAttribute";
		}
	}
}