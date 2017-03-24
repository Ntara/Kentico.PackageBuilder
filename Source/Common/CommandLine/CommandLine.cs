// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Ntara.PackageBuilder
{
	internal class CommandLine
	{
		public static CommandLine Current
		{
			get
			{
				var parser = new CommandLineParser();
				var arguments = parser.ParseCommandLine(Environment.CommandLine);

				// Remove the first argument representing the current executable
				arguments.RemoveAt(0);

				return new CommandLine(arguments);
			}
		}

		public static CommandLine Parse(string commandLine)
		{
			var parser = new CommandLineParser();
			var arguments = parser.ParseCommandLine(commandLine);

			return new CommandLine(arguments);
		}

		internal CommandLine()
		{
			Properties = new Dictionary<string, string>();
		}

		private CommandLine(IEnumerable<string> arguments)
		{
			foreach (var argument in arguments)
			{
				ParseArgument(argument);
			}

			if (Properties == null)
			{
				Properties = new Dictionary<string, string>();
			}
		}

		#region |-- Public Properties --|

		public bool Help { get; set; }

		[Argument(CommandLineArguments.Module, "CommandLine_Module_Description", "codename")]
		public string Module { get; set; }

		[Argument(CommandLineArguments.NuSpecFile, "CommandLine_NuSpecFile_Description", "nuspecfile")]
		public string NuSpecFile { get; set; }

		[Argument(CommandLineArguments.OutputDirectory, "CommandLine_OutputDirectory_Description", "path")]
		public string OutputDirectory { get; set; }

		[Argument(CommandLineArguments.Metadata, "CommandLine_Metadata_Description", "object")]
		public CommandLineMetadata Metadata { get; set; }

		[Argument(CommandLineArguments.Properties, "CommandLine_Properties_Description", "object")]
		public IDictionary<string, string> Properties { get; private set; }

		[Argument(CommandLineArguments.Version, "CommandLine_Version_Description", "object")]
		public CommandLineVersion Version { get; set; }

		[Argument(CommandLineArguments.Debug, "CommandLine_Debug_Description")]
		public bool Debug { get; set; }

		#endregion

		#region |-- Support Methods --|

		private void ParseArgument(string argument)
		{
			string argumentName;
			string argumentValue;

			var parser = new CommandLineParser();
			parser.ParseArgument(argument, out argumentName, out argumentValue);

			if (string.Equals(argumentName, CommandLineArguments.Help, StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals(argumentName, "-?", StringComparison.InvariantCulture) ||
				string.Equals(argumentName, "/?", StringComparison.InvariantCulture))
			{
				Help = true;
			}
			else if (string.Equals(argumentName, CommandLineArguments.Debug, StringComparison.InvariantCultureIgnoreCase))
			{
				Debug = true;
			}
			else if (string.Equals(argumentName, CommandLineArguments.Module, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!string.IsNullOrEmpty(Module))
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.Module);
				}

				Module = parser.TrimQuotes(argumentValue);
			}
			else if (string.Equals(argumentName, CommandLineArguments.NuSpecFile, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!string.IsNullOrEmpty(NuSpecFile))
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.NuSpecFile);
				}

				NuSpecFile = parser.TrimQuotes(argumentValue);
			}
			else if (string.Equals(argumentName, CommandLineArguments.OutputDirectory, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!string.IsNullOrEmpty(OutputDirectory))
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.OutputDirectory);
				}

				OutputDirectory = parser.TrimQuotes(argumentValue);
			}
			else if (string.Equals(argumentName, CommandLineArguments.Metadata, StringComparison.InvariantCultureIgnoreCase))
			{
				if (Metadata != null)
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.Metadata);
				}

				Metadata = ParseMetadataArgument(argumentValue);
			}
			else if (string.Equals(argumentName, CommandLineArguments.Properties, StringComparison.InvariantCultureIgnoreCase))
			{
				if (Properties != null)
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.Properties);
				}

				Properties = ParsePropertiesArgument(argumentValue);
			}
			else if (string.Equals(argumentName, CommandLineArguments.Version, StringComparison.InvariantCultureIgnoreCase))
			{
				if (Version != null)
				{
					ThrowArgumentAlreadyDefined(CommandLineArguments.Version);
				}

				Version = ParseVersionArgument(argumentValue);
			}
			else
			{
				ThrowArgumentNotRecognized(argumentName);
			}
		}

		private CommandLineMetadata ParseMetadataArgument(string argumentValue)
		{
			var metadata = new CommandLineMetadata();

			var parser = new CommandLineParser();
			var properties = parser.ParseArgumentProperties(CommandLineArguments.Metadata, argumentValue);

			foreach (var property in properties)
			{
				if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.MetadataId, StringComparison.InvariantCultureIgnoreCase))
				{
					metadata.Id = property.Value;
				}
				else if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.MetadataTitle, StringComparison.InvariantCultureIgnoreCase))
				{
					metadata.Title = property.Value;
				}
				else if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.MetadataDescription, StringComparison.InvariantCultureIgnoreCase))
				{
					metadata.Description = property.Value;
				}
				else if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.MetadataAuthors, StringComparison.InvariantCultureIgnoreCase))
				{
					metadata.Authors = property.Value;
				}
				else
				{
					ThrowArgumentNotRecognized(CommandLineArguments.Metadata, property.Key);
				}
			}

			return metadata;
		}

		private IDictionary<string, string> ParsePropertiesArgument(string argumentValue)
		{
			IDictionary<string, string> nuspecProperties = new Dictionary<string, string>();

			var parser = new CommandLineParser();
			var properties = parser.ParseArgumentProperties(CommandLineArguments.Properties, argumentValue);

			foreach (var property in properties)
			{
				if (!nuspecProperties.ContainsKey(property.Key))
				{
					nuspecProperties.Add(property);
				}
			}

			return nuspecProperties;
		}

		private CommandLineVersion ParseVersionArgument(string argumentValue)
		{
			var version = new CommandLineVersion();

			var parser = new CommandLineParser();
			var properties = parser.ParseArgumentProperties(CommandLineArguments.Version, argumentValue);

			foreach (var property in properties)
			{
				if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.VersionAssembly, StringComparison.InvariantCultureIgnoreCase))
				{
					version.Assembly = property.Value;
				}
				else if (string.Equals(property.Key, CommandLineArguments.ObjectProperties.VersionAssemblyAttribute, StringComparison.InvariantCultureIgnoreCase))
				{
					AssemblyVersionType versionType;

					if (!Enum.TryParse(property.Value, out versionType))
					{
						var errorMessage = string.Format(CommonResources.CommandLineArgumentPropertyException_UnknownValueType, property.Value);
						throw new CommandLineArgumentPropertyException(CommandLineArguments.Version, property.Key, errorMessage);
					}

					version.AssemblyVersionType = versionType;
				}
				else if (string.IsNullOrEmpty(version.Value))
				{
					// Set manual/explicit version
					version.Value = property.Key;
				}
				else
				{
					// Only allow one "value" parameter to be specified
					ThrowArgumentPropertyAlreadyDefined(CommandLineArguments.Version, CommandLineArguments.ObjectProperties.VersionValue);
				}
			}

			return version;
		}

		private void ThrowArgumentNotRecognized(string argumentName)
		{
			var errorMessage = string.Format(CommonResources.CommandLineArgumentException_NotRecognized, argumentName);
			throw new CommandLineArgumentException(argumentName, errorMessage);
		}

		private void ThrowArgumentNotRecognized(string argumentName, string propertyName)
		{
			var errorMessage = string.Format(CommonResources.CommandLineArgumentPropertyException_NotRecognized, propertyName);
			throw new CommandLineArgumentPropertyException(argumentName, propertyName, errorMessage);
		}

		private void ThrowArgumentAlreadyDefined(string argumentName)
		{
			var errorMessage = string.Format(CommonResources.CommandLineArgumentException_AlreadyDefined, argumentName);
			throw new CommandLineArgumentException(argumentName, errorMessage);
		}

		private void ThrowArgumentPropertyAlreadyDefined(string argumentName, string propertyName)
		{
			var errorMessage = string.Format(CommonResources.CommandLineArgumentPropertyException_AlreadyDefined, propertyName);
			throw new CommandLineArgumentPropertyException(argumentName, propertyName, errorMessage);
		}

		#endregion
	}
}