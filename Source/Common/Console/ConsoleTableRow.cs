// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

namespace Ntara.PackageBuilder
{
	internal class ConsoleTableRow
	{
		public ConsoleTableRow(string column1, string column2)
		{
			Column1 = column1 ?? string.Empty;
			Column2 = column2 ?? string.Empty;
		}

		public string Column1 { get; }
		public string Column2 { get; }
	}
}