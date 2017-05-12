// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Modules;

namespace Ntara.PackageBuilder
{
	internal static class Program
	{
		private static void Initialize()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location ?? string.Empty;
			var webApplicationRoot = Directory.GetParent(assemblyLocation).Parent?.FullName;

			SystemContext.WebApplicationPhysicalPath = webApplicationRoot;
			SystemContext.UseWebApplicationConfiguration = true;

			CMSApplication.Init();
		}

		public static int Main(string[] args)
		{
			var debug = false;

			try
			{
				ConsoleUtility.WriteHeader();

				if (args.Length == 0)
				{
					ConsoleUtility.WriteHelp();
					return -1;
				}

				var commandLine = CommandLine.Current;
				debug = commandLine.Debug;

				if (commandLine.Help)
				{
					ConsoleUtility.WriteHelp();
					return 0;
				}

				ConsoleWriter.NewLine();
				ConsoleWriter.WriteMessage(CommonResources.Progress_InitializingApplication);

				Initialize();

				if (string.IsNullOrEmpty(commandLine.Module))
				{
					throw new CommandLineArgumentException(CommandLineArguments.Module, CommonResources.CommandLineArgumentException_ModuleRequired);
				}

				var module = ResourceInfoProvider.GetResourceInfo(commandLine.Module);

				if (module == null)
				{
					var errorMessage = string.Format(CultureInfo.CurrentCulture, CommonResources.ModuleNotFoundException_InvalidResource, commandLine.Module);
					throw new ModuleNotFoundException(errorMessage);
				}

				var outputDirectory =
					!string.IsNullOrEmpty(commandLine.OutputDirectory) ? commandLine.OutputDirectory : DefaultOutputDirectory;

				ModulePackageBuilder packageBuilder;

				if (commandLine.Version != null)
				{
					var explicitVersion = commandLine.Version.Value;
					var assembly = commandLine.Version.Assembly;
					var assemblyVersionType = commandLine.Version.AssemblyVersionType;

					// Resolve wildcards to assemblies matching the resource name
					if (assembly == CommandLine.Wildcard)
					{
						assembly = module.ResourceName;
					}

					var versionResolver =
						!string.IsNullOrEmpty(explicitVersion) ?
							(StaticVersion)explicitVersion as IModuleVersionResolver :
							new AssemblyVersionResolver(assembly, assemblyVersionType);

					// Resolve version from resolver
					packageBuilder = new ModulePackageBuilder(module, versionResolver);
				}
				else
				{
					// Resolve version from module info
					packageBuilder = new ModulePackageBuilder(module);
				}

				// Set NuSpec file (if specified)
				if (!string.IsNullOrEmpty(commandLine.NuSpecFile))
				{
					var filePath = commandLine.NuSpecFile;

					if (filePath == CommandLine.Wildcard)
					{
						filePath = string.Format(CultureInfo.InvariantCulture, "{0}.nuspec", module.ResourceName);
					}

					packageBuilder.NuSpecFile = filePath;
					packageBuilder.NuSpecProperties = commandLine.Properties;
				}

				// Override module metadata (if specified)
				if (commandLine.Metadata != null)
				{
					if (!string.IsNullOrEmpty(commandLine.Metadata.Id))
						packageBuilder.ModuleMetadata.Id = commandLine.Metadata.Id;

					if (!string.IsNullOrEmpty(commandLine.Metadata.Title))
						packageBuilder.ModuleMetadata.Title = commandLine.Metadata.Title;

					if (!string.IsNullOrEmpty(commandLine.Metadata.Description))
						packageBuilder.ModuleMetadata.Description = commandLine.Metadata.Description;

					if (!string.IsNullOrEmpty(commandLine.Metadata.Authors))
						packageBuilder.ModuleMetadata.Authors = commandLine.Metadata.Authors;
				}

				ConsoleWriter.WriteMessage(CommonResources.Progress_BuildingPackage);

				// Execute package builder
				var result = packageBuilder.BuildPackage(outputDirectory);

				ConsoleWriter.NewLine();
				ConsoleWriter.WriteMessage(CommonResources.Progress_BuildSuccess);

				// Write result summary
				var resultRows = new List<ConsoleTableRow>()
				{
					new ConsoleTableRow(CommonResources.Report_OutputDirectory, result.OutputDirectory),
					new ConsoleTableRow(CommonResources.Report_PackageName, result.PackageFileName)
				};

				ConsoleWriter.NewLine();
				ConsoleUtility.WriteTable(resultRows);

				// Report success
				return 0;
			}
			catch (CommandLineArgumentException exception)
			{
				ConsoleWriter.NewLine();
				ConsoleUtility.WriteWrappedMessage(exception.Message, ConsoleWriter.WriteError);

				// Report failure
				return -1;
			}
			catch (Exception exception)
			{
				ConsoleWriter.NewLine();
				ConsoleUtility.WriteWrappedMessage(exception.Message, ConsoleWriter.WriteError);

				if (debug)
				{
					// Write error section title
					var errorSectionTitle = string.Format(CultureInfo.CurrentCulture, CommonResources.SectionFormat, Environment.NewLine + exception.GetType().FullName);
					ConsoleWriter.WriteErrorDetail(errorSectionTitle);

					// Write exception stack trace
					ConsoleUtility.WriteWrappedMessage(exception.StackTrace, ConsoleWriter.WriteErrorDetail, 0, 6);
				}

				// Report failure
				return -1;
			}
		}

		private static string DefaultOutputDirectory
		{
			get { return Path.Combine(ImportExportHelper.GetSiteUtilsFolder(), "Export"); }
		}
	}
}
