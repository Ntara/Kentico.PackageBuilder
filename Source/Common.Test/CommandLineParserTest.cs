using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ntara.PackageBuilder.Test
{
	[TestClass]
	public class CommandLineParserTest
	{
		#region |-- ParseCommandLine Tests --|

		[TestMethod]
		public void ParseCommandLineTest()
		{
			const string commandLine = @"-argument1:ArgumentValue -argument2:property1=PropertyValue1,property2=PropertyValue2 -argument3:property1=PropertyValue1,property2=PropertyValue2";

			var expectedArguments = new[]
			{
				@"-argument1:ArgumentValue",
				@"-argument2:property1=PropertyValue1,property2=PropertyValue2",
				@"-argument3:property1=PropertyValue1,property2=PropertyValue2"
			};

			AssertCommandLine(commandLine, expectedArguments);
		}

		[TestMethod]
		public void ParseCommandLineTestWithSingleQuotes()
		{
			const string commandLine = @"-argument1:'Argument value...' -argument2:property1='Property value 1...',property2='Property value 2...' -argument3:property1='Property value 1...',property2='Property value 2...'";

			var expectedArguments = new[]
			{
				@"-argument1:'Argument value...'",
				@"-argument2:property1='Property value 1...',property2='Property value 2...'",
				@"-argument3:property1='Property value 1...',property2='Property value 2...'"
			};

			AssertCommandLine(commandLine, expectedArguments);
		}

		[TestMethod]
		public void ParseCommandLineTestWithDoubleQuotes()
		{
			const string commandLine = @"-argument1:""Argument value..."" -argument2:property1=""Property value 1..."",property2=""Property value 2..."" -argument3:property1=""Property value 1..."",property2=""Property value 2...""";

			var expectedArguments = new[]
			{
				@"-argument1:""Argument value...""",
				@"-argument2:property1=""Property value 1..."",property2=""Property value 2...""",
				@"-argument3:property1=""Property value 1..."",property2=""Property value 2..."""
			};

			AssertCommandLine(commandLine, expectedArguments);
		}

		#endregion

		#region |-- ParseArgument Tests --|

		[TestMethod]
		public void ParseArgumentTest()
		{
			AssertArgument(@"-switch", "-switch", string.Empty);
			AssertArgument(@"-argumentName:ArgumentValue", "-argumentName", @"ArgumentValue");
			AssertArgument(@"-argumentName:property1=Value1,property2=Value2", "-argumentName", @"property1=Value1,property2=Value2");
		}

		[TestMethod]
		public void ParseArgumentTestWithSingleQuotes()
		{
			AssertArgument(@"-argumentName:'ArgumentValue'", "-argumentName", @"'ArgumentValue'");
			AssertArgument(@"-argumentName:property1='Test value1...',property2='Test value2...'", "-argumentName", @"property1='Test value1...',property2='Test value2...'");
		}

		[TestMethod]
		public void ParseArgumentTestWithDoubleQuotes()
		{
			AssertArgument(@"-argumentName:""ArgumentValue""", "-argumentName", @"""ArgumentValue""");
			AssertArgument(@"-argumentName:property1=""Test value1..."",property2=""Test value2...""", "-argumentName", @"property1=""Test value1..."",property2=""Test value2...""");
		}

		#endregion

		#region |-- ParseArgumentProperties Tests --|

		[TestMethod]
		public void ParseArgumentPropertiesTest()
		{
			var argumentProperties = @"property1=Value1,property2=Value2";
			var expectedProperties = new Dictionary<string, string>()
			{
				{"property1", "Value1"},
				{"property2", "Value2"}
			};

			AssertArgumentProperties("TestArgument", argumentProperties, expectedProperties);
		}

		[TestMethod]
		public void ParseArgumentPropertiesTestWithSingleQuotes()
		{
			var argumentProperties = @"property1='Test value1...',property2='Test value2...'";
			var expectedProperties = new Dictionary<string, string>()
			{
				{"property1", "Test value1..."},
				{"property2", "Test value2..."}
			};

			AssertArgumentProperties("TestArgument", argumentProperties, expectedProperties);
		}

		[TestMethod]
		public void ParseArgumentPropertiesTestWithDoubleQuotes()
		{
			var argumentProperties = @"property1=""Test value1..."",property2=""Test value2...""";
			var expectedProperties = new Dictionary<string, string>()
			{
				{"property1", "Test value1..."},
				{"property2", "Test value2..."}
			};

			AssertArgumentProperties("TestArgument", argumentProperties, expectedProperties);
		}

		[TestMethod]
		public void ParseArgumentPropertiesTestEmpty1()
		{
			var argumentProperties = @"property1,property2";
			var expectedProperties = new Dictionary<string, string>()
			{
				{"property1", string.Empty},
				{"property2", string.Empty}
			};

			AssertArgumentProperties("TestArgument", argumentProperties, expectedProperties);
		}

		[TestMethod]
		public void ParseArgumentPropertiesTestEmpty2()
		{
			var argumentProperties = @"property1=,property2=";
			var expectedProperties = new Dictionary<string, string>()
			{
				{"property1", string.Empty},
				{"property2", string.Empty}
			};

			AssertArgumentProperties("TestArgument", argumentProperties, expectedProperties);
		}

		[TestMethod]
		[ExpectedException(typeof(CommandLineArgumentPropertyException))]
		public void ParseArgumentPropertiesTestDuplicate()
		{
			const string argumentProperties = @"property=""Test value1..."",property=""Test value2...""";

			var parser = new CommandLineParser();
			parser.ParseArgumentProperties("TestArgument", argumentProperties);
		}

		#endregion

		#region |-- TrimQuotes Tests --|

		[TestMethod]
		public void TrimQuotesTest()
		{
			AssertTrimQuotes(null, null);
			AssertTrimQuotes(string.Empty, string.Empty);
			AssertTrimQuotes("Test value...", "Test value...");
			AssertTrimQuotes(@"'Test value...'", "Test value...");
			AssertTrimQuotes(@"""Test value...""", "Test value...");
		}

		[TestMethod]
		public void TrimQuotesTestMultiple()
		{
			AssertTrimQuotes(@"''Test value...''", @"'Test value...'");
			AssertTrimQuotes(@"""""Test value...""""", @"""Test value...""");
		}

		[TestMethod]
		public void TrimQuotesTestUnbalanced()
		{
			AssertTrimQuotes(@"'Test value...", @"'Test value...");
			AssertTrimQuotes(@"""Test value...", @"""Test value...");
		}

		#endregion

		#region |-- Support Methods --|

		private void AssertCommandLine(string commandLine, string[] expected)
		{
			var parser = new CommandLineParser();
			var actual = parser.ParseCommandLine(commandLine);

			CollectionAssert.AreEqual(expected, actual, "The parsed command-line arguments do not match the expected value.");
		}

		private void AssertArgument(string argument, string expectedName, string expectedValue)
		{
			string actualName;
			string actualValue;

			var parser = new CommandLineParser();
			parser.ParseArgument(argument, out actualName, out actualValue);

			Assert.AreEqual(expectedName, actualName, "The parsed argument name does not match the expected value.");
			Assert.AreEqual(expectedValue, actualValue, "The parsed argument value does not match the expected value.");
		}

		private void AssertArgumentProperties(string argumentName, string argumentProperties, Dictionary<string, string> expectedProperties)
		{
			var parser = new CommandLineParser();
			var actualProperties = parser.ParseArgumentProperties(argumentName, argumentProperties);

			Assert.AreEqual(expectedProperties.Count, actualProperties.Count, "The argument property counts do not match.");

			foreach (var expectedProperty in expectedProperties)
			{
				Assert.IsTrue(actualProperties.ContainsKey(expectedProperty.Key), $"The expected argument property '{expectedProperty.Key}' does not exist.");
				Assert.AreEqual(expectedProperty.Value, actualProperties[expectedProperty.Key], $"The expected argument property value '{expectedProperty.Value}' does not match.");
			}
		}

		private void AssertTrimQuotes(string value, string expected)
		{
			var parser = new CommandLineParser();
			var actual = parser.TrimQuotes(value);

			Assert.AreEqual(expected, actual, "The trimmed value does not match the expected value.");
		}

		#endregion
	}
}