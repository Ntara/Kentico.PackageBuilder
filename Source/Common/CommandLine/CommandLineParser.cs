// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Ntara.PackageBuilder
{
	internal class CommandLineParser
	{
		#region |-- Constants --|

		private const char DashChar = '-';
		private const char SingleQuoteChar = '\'';
		private const char DoubleQuoteChar = '"';
		private const char CommaChar = ',';
		private const char EqualsChar = '=';
		private const char SpaceChar = ' ';

		#endregion

		#region |-- Public Methods --|

		public List<string> ParseCommandLine(string commandLine)
		{
			var arguments = new List<string>();

			if (!string.IsNullOrWhiteSpace(commandLine))
			{
				var commandLineBlock = commandLine.Trim();

				while (true)
				{
					var startIndex = SmartIndexOf(commandLineBlock, SpaceChar);

					if (startIndex > 0)
					{
						arguments.Add(commandLineBlock.Substring(0, startIndex));
						commandLineBlock = commandLineBlock.Substring(startIndex + 1).Trim();
					}
					else
					{
						break;
					}
				}

				if (!string.IsNullOrEmpty(commandLineBlock))
				{
					arguments.Add(commandLineBlock);
				}
			}

			return arguments;
		}

		public void ParseArgument(string argument, out string argumentName, out string argumentValue)
		{
			var startIndex = argument.IndexOfAny(new [] { ':', '=' });

			if (startIndex == -1)
			{
				argumentName = NormalizeDashes(argument);
				argumentValue = string.Empty;
			}
			else
			{
				argumentName = NormalizeDashes(argument).Substring(0, startIndex);
				argumentValue = argumentName.Length >= startIndex ? argument.Substring(startIndex + 1) : string.Empty;
			}
		}

		public Dictionary<string, string> ParseArgumentProperties(string argumentName, string argumentProperties)
		{
			var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			while (!string.IsNullOrEmpty(argumentProperties))
			{
				var commaIndex = SmartIndexOf(argumentProperties, CommaChar);
				var propertyBlock = commaIndex != -1 ? argumentProperties.Substring(0, commaIndex) : argumentProperties;
				var equalsIndex = SmartIndexOf(propertyBlock, EqualsChar);

				string propertyName;
				string propertyValue;

				if (equalsIndex == -1)
				{
					propertyName = propertyBlock;
					propertyValue = string.Empty;
				}
				else
				{
					propertyName = propertyBlock.Substring(0, equalsIndex);
					propertyValue = equalsIndex >= propertyBlock.Length - 1 ? string.Empty : propertyBlock.Substring(equalsIndex + 1);
				}

				// Normalize values
				propertyName = TrimQuotes(propertyName);
				propertyValue = TrimQuotes(propertyValue);

				// Notify on duplicate properties
				if (parameters.ContainsKey(propertyName))
				{
					var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.CommandLineArgumentPropertyException_AlreadyDefined, propertyName);
					throw new CommandLineArgumentPropertyException(argumentName, propertyName, errorMessage);
				}

				parameters.Add(propertyName, propertyValue);

				// Move to next property block
				argumentProperties = (commaIndex != -1 && commaIndex < argumentProperties.Length - 1) ? argumentProperties.Substring(commaIndex + 1) : null;
			}

			return parameters;
		}

		public string TrimQuotes(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			if (IsSurroundedBy(value, SingleQuoteChar) || IsSurroundedBy(value, DoubleQuoteChar))
			{
				return value.Substring(1, value.Length - 2);
			}

			return value;
		}

		#endregion

		#region |-- Support Methods --|

		private static int SmartIndexOf(string input, char character)
		{
			var stack = new Stack();

			for (var index = 0; index < input.Length; ++index)
			{
				if (input[index] == character)
				{
					if (stack.Count == 0)
					{
						return index;
					}
				}
				else if (input[index] == SingleQuoteChar || input[index] == DoubleQuoteChar)
				{
					if (stack.Count == 0 || (char)stack.Peek() != input[index])
					{
						stack.Push(input[index]);
					}
					else
					{
						stack.Pop();
					}
				}
			}

			return -1;
		}

		private static bool IsSurroundedBy(string value, char character)
		{
			var characterString = character.ToString();
			return value.StartsWith(characterString, StringComparison.Ordinal) && value.EndsWith(characterString, StringComparison.Ordinal);
		}

		private static string NormalizeDashes(string input)
		{
			var unicodeDashes = new [] { '᠆', '‐', '‑', '‒', '–', '—', '―', '−' };
			return (input.IndexOfAny(unicodeDashes, 0, 1) == 0) ? DashChar + input.Substring(1) : input;
		}

		#endregion
	}
}