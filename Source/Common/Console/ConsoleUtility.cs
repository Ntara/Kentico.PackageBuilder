// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ntara.PackageBuilder
{
	internal static class ConsoleUtility
	{
		private const int DefaultIndent = 2;
		private const int MinColumnSpacing = 4;

		public static readonly int ConsoleWidth = GetConsoleWidth();

		public static void WriteHeader()
		{
			var versionInfo = GetConsoleVersionInfo();
			var helpHeader = string.Format(CultureInfo.CurrentCulture, CommonResources.OutputHeader, versionInfo.OriginalFilename, versionInfo.ProductVersion);

			ConsoleWriter.WriteMessage(helpHeader);
		}

		public static void WriteHelp()
		{
			var versionInfo = GetConsoleVersionInfo();

			// Write command-line usage
			var toolUsage = string.Format(CultureInfo.CurrentCulture, CommonResources.ToolUsage, versionInfo.OriginalFilename);
			ConsoleWriter.WriteMessage(Environment.NewLine + toolUsage);

			// Write command-line argument usage table
			ConsoleWriter.NewLine();
			WriteArguments();

			// Write argument object tables
			ConsoleWriter.NewLine();
			WriteArgumentObject<CommandLineMetadata>(CommonResources.CommandLine_Metadata_SectionTitle);
			ConsoleWriter.NewLine();
			WriteArgumentObject<CommandLineVersion>(CommonResources.CommandLine_Version_SectionTitle);

			// Write notes
			ConsoleWriter.NewLine();
			ConsoleWriter.WriteMessage(CommonResources.ToolNotes_SectionTitle);
			ConsoleWriter.NewLine();
			var toolNotes = string.Format(CultureInfo.CurrentCulture, CommonResources.ToolNotes_Content, versionInfo.OriginalFilename, KenticoResources.KenticoHelpUrl);
			WriteWrappedMessage(toolNotes, ConsoleWriter.WriteMessage, DefaultIndent);

			// Write examples
			ConsoleWriter.NewLine();
			ConsoleWriter.WriteMessage(CommonResources.ToolExamples_SectionTitle);
			ConsoleWriter.NewLine();
			var toolExamples = string.Format(CultureInfo.CurrentCulture, CommonResources.ToolExamples_Content, versionInfo.OriginalFilename);
			WriteWrappedMessage(toolExamples, ConsoleWriter.WriteMessage, DefaultIndent, DefaultIndent);
		}

		public static void WriteWrappedMessage(string message, Action<string> writeMessage, int indent = 0, int overflowIndent = 0)
		{
			var messageLines =
				message.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

			foreach (var messageLine in messageLines)
			{
				var messagePart = messageLine;
				var messageOverflow = (string)null;

				do
				{
					var effectiveIndent = !string.IsNullOrEmpty(messageOverflow) ? indent + overflowIndent : indent;

					GetConsoleOverflow(messagePart, effectiveIndent, out messagePart, out messageOverflow);
					writeMessage(new string(' ', effectiveIndent) + messagePart);

					messagePart = messageOverflow;
				}
				while (messageOverflow.Length > 0);
			}
		}

		public static void WriteTable(IEnumerable<ConsoleTableRow> tableRows, int indent = DefaultIndent)
		{
			var tableRowList = tableRows.ToList();
			var column2StartIndex = tableRowList.Max(argument => indent + argument.Column1.Length + MinColumnSpacing);

			// Write command-line argument usage table
			foreach (var tableRow in tableRowList)
			{
				var column1 = tableRow.Column1.PadRight(column2StartIndex);
				var column2 = tableRow.Column2;

				WriteColumns(column1, column2, column2StartIndex, indent);
			}
		}

		public static void GetConsoleOverflow(string message, int startIndex, out string messagePart, out string messageOverflow)
		{
			var breakingCharacters = new [] { ' ', ',' };
			var maxCharacters = ConsoleWidth - 1;

			var newlineMessages = message.Split(new [] { Environment.NewLine, "\n" }, StringSplitOptions.None);

			// Favor newline over console overflow
			if (newlineMessages.Length > 1 && startIndex + newlineMessages.First().Length <= maxCharacters)
			{
				messagePart = newlineMessages.First();
				messageOverflow = message.Substring(messagePart.Length);

				// Trim leading newline character(s)
				if (messageOverflow.StartsWith(Environment.NewLine, StringComparison.Ordinal))
				{
					messageOverflow = messageOverflow.Substring(2);
				}
				else
				{
					messageOverflow = messageOverflow.Substring(1);
				}

				return;
			}

			if (startIndex + message.Length <= maxCharacters)
			{
				messagePart = message;
				messageOverflow = string.Empty;
			}
			else
			{
				var totalLength = maxCharacters - startIndex;
				var trimmedTotalLength = message.Substring(0, totalLength).LastIndexOfAny(breakingCharacters);

				if (trimmedTotalLength == -1)
				{
					messagePart = message.Substring(0, totalLength);
					messageOverflow = message.Substring(totalLength);
				}
				else
				{
					messagePart = message.Substring(0, trimmedTotalLength + 1);
					messageOverflow = message.Substring(trimmedTotalLength + 1);
				}
			}
		}

		#region |-- Support Methods --|

		private static FileVersionInfo GetConsoleVersionInfo()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			return FileVersionInfo.GetVersionInfo(assemblyLocation);
		}

		private static int GetConsoleWidth()
		{
			try
			{
				// Tricky: BufferWidth will throw an IOException during headless operation
				return Console.BufferWidth;
			}
			catch (IOException)
			{
				return int.MaxValue;
			}
		}

		private static void WriteColumns(string column1, string column2, int column2StartIndex, int indent)
		{
			var messagePart = $"{new string(' ', indent)}{column1}{column2}";
			var messageOverflow = (string)null;

			do
			{
				var effectiveIndent = !string.IsNullOrEmpty(messageOverflow) ? indent + column2StartIndex : 0;

				GetConsoleOverflow(messagePart, effectiveIndent, out messagePart, out messageOverflow);
				ConsoleWriter.WriteMessage(new string(' ', effectiveIndent) + messagePart);

				messagePart = messageOverflow;
			}
			while (messageOverflow.Length > 0);
		}

		private static IEnumerable<ArgumentAttribute> GetCommandLineArgumentUsage()
		{
			var commandLineType = typeof(CommandLine);
			var commandLineProperties = commandLineType.GetProperties();

			foreach (var commandLineProperty in commandLineProperties)
			{
				var argumentUsage = commandLineProperty.GetCustomAttribute<ArgumentAttribute>();

				if (argumentUsage != null)
				{
					yield return argumentUsage;
				}
			}
		}

		private static void WriteArguments()
		{
			var arguments = GetCommandLineArgumentUsage();
			var argumentRows = arguments.Select(argument => new ConsoleTableRow(argument.Usage, argument.Description));

			WriteTable(argumentRows);
		}

		private static void WriteArgumentObject<TArgumentObject>(string sectionTitle)
		{
			WriteArgumentObject(sectionTitle, typeof(TArgumentObject));
		}

		private static void WriteArgumentObject(string sectionTitle, Type argumentObjectType)
		{
			// Write argument object section title
			if (!string.IsNullOrEmpty(sectionTitle))
			{
				ConsoleWriter.WriteMessage(sectionTitle + Environment.NewLine);
			}

			var argumentObjectProperties = argumentObjectType.GetProperties();
			var argumentObjectRows = new List<ConsoleTableRow>();

			// Create property table data
			foreach (var argumentObjectProperty in argumentObjectProperties)
			{
				var propertyInfo = argumentObjectProperty.GetCustomAttribute<ArgumentPropertyAttribute>();

				if (propertyInfo != null)
				{
					argumentObjectRows.Add(
						new ConsoleTableRow(propertyInfo.Name, propertyInfo.Description)
					);
				}
			}

			// Write argument object property data
			WriteTable(argumentObjectRows);
		}

		#endregion
	}
}