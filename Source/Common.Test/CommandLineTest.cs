using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ntara.PackageBuilder.Test
{
	[TestClass]
	public class CommandLineTest
	{
		#region |-- General Argument Tests --|

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestNotRecognized()
		{
			const string commandLine = "-unknown=Test";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- Help Argument Tests --|

		[TestMethod]
		public void TestHelp1()
		{
			const string commandLine = @"-help";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestHelp2()
		{
			const string commandLine = @"?";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestHelp3()
		{
			const string commandLine = @"-?";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestHelpDuplicate1()
		{
			const string commandLine = "-help -help";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestHelpDuplicate2()
		{
			const string commandLine = "-help ?";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestHelpDuplicate3()
		{
			const string commandLine = "-help -?";
			var expected = new CommandLine() { Help = true };

			AssertCommandLine(commandLine, expected);
		}

		#endregion

		#region |-- Module Argument Tests --|

		[TestMethod]
		public void TestModule()
		{
			const string commandLine = @"-module:Custom.ModuleResource";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestModuleSingleQuotes()
		{
			const string commandLine = @"-module:'Custom.ModuleResource'";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestModuleDoubleQuotes()
		{
			const string commandLine = @"-module:""Custom.ModuleResource""";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestModuleDuplicate()
		{
			const string commandLine = "-module:'Custom.ModuleResource1' -module:'Custom.ModuleResource2'";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- NuSpecFile Argument Tests --|

		[TestMethod]
		public void TestNuSpecFile()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:Test.nuspec";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestNuSpecAssemblyWildcard1()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = CommandLine.Wildcard
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestNuSpecAssemblyWildcard2()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec=*";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = CommandLine.Wildcard
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestNuSpecFileSingleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:'Test.nuspec'";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestNuSpecFileDoubleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:""Test.nuspec""";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestNuSpecFileDuplicate()
		{
			const string commandLine = "-nuspec:'Test1.nuspec' -nuspec:'Test2.nuspec'";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- OutputDirectory Argument Tests --|

		[TestMethod]
		public void TestOutputDirectory()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-output:Modules/Packages";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				OutputDirectory = "Modules/Packages"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestOutputDirectorySingleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-output:'Modules/Packages'";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				OutputDirectory = "Modules/Packages"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestOutputDirectoryDoubleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-output:""Modules/Packages""";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				OutputDirectory = "Modules/Packages"
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestOutputDirectoryDuplicate()
		{
			const string commandLine = "-output:Packages1 -output:Packages2";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- Metadata Argument Tests --|

		[TestMethod]
		public void TestMetadata()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-metadata:id=Test,title=TestTitle,description=Description...,authors=Person";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Metadata = new CommandLineMetadata()
				{
					Id = "Test",
					Title = "TestTitle",
					Description = "Description...",
					Authors = "Person"
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestMetadataWithSingleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-metadata:id=Test,title='Test Title',description='Test description here...',authors='Some Person'";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Metadata = new CommandLineMetadata()
				{
					Id = "Test",
					Title = "Test Title",
					Description = "Test description here...",
					Authors = "Some Person"
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestMetadataWithDoubleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-metadata:id=Test,title=""Test Title"",description=""Test description here..."",authors=""Some Person""";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Metadata = new CommandLineMetadata()
				{
					Id = "Test",
					Title = "Test Title",
					Description = "Test description here...",
					Authors = "Some Person"
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestMetadataDuplicate()
		{
			const string commandLine = "-metadata:id=Test1,title=Title1 -metadata:id=Test2,title=Title2";
			CommandLine.Parse(commandLine);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentPropertyException))]
		public void TestMetadataNotRecognized()
		{
			const string commandLine = "-metadata:unknown=Test";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- Properties Argument Tests --|

		[TestMethod]
		public void TestProperties()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:Test.nuspec " +
				@"-properties:custom1=CustomValue1,custom2=CustomValue2";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			expected.Properties.Add("custom1", "CustomValue1");
			expected.Properties.Add("custom2", "CustomValue2");

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestPropertiesWithSingleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:Test.nuspec " +
				@"-properties:custom1='Custom Value1',custom2='Custom Value2'";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			expected.Properties.Add("custom1", "Custom Value1");
			expected.Properties.Add("custom2", "Custom Value2");

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestPropertiesWithDoubleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-nuspec:Test.nuspec " +
				@"-properties:custom1=""Custom Value1"",custom2=""Custom Value2""";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				NuSpecFile = "Test.nuspec"
			};

			expected.Properties.Add("custom1", "Custom Value1");
			expected.Properties.Add("custom2", "Custom Value2");

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestPropertiesDuplicate()
		{
			const string commandLine = "-properties:token=TestValue1 -properties:token=TestValue2";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- Version Argument Tests --|

		[TestMethod]
		public void TestVersionExplicit()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:1.2.3";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Value = "1.2.3"
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssembly()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly=Custom.ModuleResource.dll,assemblyAttribute=AssemblyInformationalVersion";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = "Custom.ModuleResource.dll",
					AssemblyVersionType = AssemblyVersionType.AssemblyInformationalVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssemblyWildcard1()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = CommandLine.Wildcard,
					AssemblyVersionType = AssemblyVersionType.AssemblyFileVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssemblyWildcard2()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly=*";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = CommandLine.Wildcard,
					AssemblyVersionType = AssemblyVersionType.AssemblyFileVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssemblySingleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly='Custom.ModuleResource.dll',assemblyAttribute=AssemblyInformationalVersion";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = "Custom.ModuleResource.dll",
					AssemblyVersionType = AssemblyVersionType.AssemblyInformationalVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssemblyDoubleQuotes()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly=""Custom.ModuleResource.dll"",assemblyAttribute=AssemblyInformationalVersion";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = "Custom.ModuleResource.dll",
					AssemblyVersionType = AssemblyVersionType.AssemblyInformationalVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestVersionAssemblyDefaultType()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly=Custom.ModuleResource.dll";

			var expected = new CommandLine()
			{
				Module = "Custom.ModuleResource",
				Version = new CommandLineVersion()
				{
					Assembly = "Custom.ModuleResource.dll",
					AssemblyVersionType = AssemblyVersionType.AssemblyFileVersion
				}
			};

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentPropertyException))]
		public void TestVersionUnknownValueType()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:assembly=Custom.ModuleResource.dll,assemblyAttribute=Unknown";

			CommandLine.Parse(commandLine);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentPropertyException))]
		public void TestVersionExplicitMultipleValue()
		{
			const string commandLine =
				@"-module:Custom.ModuleResource " +
				@"-version:1.2.3,test";

			CommandLine.Parse(commandLine);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentException))]
		public void TestVersionDuplicate()
		{
			const string commandLine = "-version:1.2.3 -version:3.2.1";
			CommandLine.Parse(commandLine);
		}

		#endregion

		#region |-- Debug Argument Tests --|

		[TestMethod]
		public void TestDebug()
		{
			const string commandLine = @"-debug";
			var expected = new CommandLine() { Debug = true };

			AssertCommandLine(commandLine, expected);
		}

		[TestMethod]
		public void TestDebugDuplicate()
		{
			const string commandLine = "-debug -debug";
			var expected = new CommandLine() { Debug = true };

			AssertCommandLine(commandLine, expected);
		}

		#endregion

		#region |-- Support Methods --|

		private void AssertCommandLine(string commandLine, CommandLine expected)
		{
			var actual = CommandLine.Parse(commandLine);

			Assert.AreEqual(expected.Help, actual.Help);
			Assert.AreEqual(expected.Module, actual.Module);
			Assert.AreEqual(expected.NuSpecFile, actual.NuSpecFile);
			Assert.AreEqual(expected.OutputDirectory, actual.OutputDirectory);

			if (expected.Metadata == null)
			{
				Assert.IsNull(actual.Metadata);
			}
			else
			{
				Assert.AreEqual(expected.Metadata.Id, actual.Metadata.Id);
				Assert.AreEqual(expected.Metadata.Title, actual.Metadata.Title);
				Assert.AreEqual(expected.Metadata.Description, actual.Metadata.Description);
				Assert.AreEqual(expected.Metadata.Authors, actual.Metadata.Authors);
			}

			AreEquivalent(expected.Properties, actual.Properties);

			if (expected.Version == null)
			{
				Assert.IsNull(actual.Version);
			}
			else
			{
				Assert.AreEqual(expected.Version.Value, actual.Version.Value);
				Assert.AreEqual(expected.Version.Assembly, actual.Version.Assembly);
				Assert.AreEqual(expected.Version.AssemblyVersionType, actual.Version.AssemblyVersionType);
			}

			Assert.AreEqual(expected.Debug, actual.Debug);
		}

		private void AreEquivalent<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
		{
			if (expected == null)
			{
				Assert.IsNull(actual);

				return;
			}

			Assert.AreEqual(expected.Count, actual.Count, $"The expected dictionary count of ({expected.Count}) does not match the actual dictionary count of ({actual.Count}).");

			foreach (var expectedProperty in expected)
			{
				TValue actualPropertyValue;

				Assert.IsTrue(actual.TryGetValue(expectedProperty.Key, out actualPropertyValue), $"The expected key '{expectedProperty.Key}' does not exist.");
				Assert.AreEqual(expectedProperty.Value, actualPropertyValue);
			}
		}

		#endregion
	}
}
