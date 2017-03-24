// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.IO;

namespace Ntara.PackageBuilder
{
	internal abstract class ConsoleWriter
	{
		private static readonly ConsoleWriter Current = GetCurrent();

		public static void NewLine()
		{
			Current.WriteMessageImpl(string.Empty);
		}

		public static void WriteMessage(string message)
		{
			Current.WriteMessageImpl(message);
		}

		public static void WriteWarning(string message)
		{
			Current.WriteWarningImpl(message);
		}

		public static void WriteError(string message)
		{
			Current.WriteErrorImpl(message);
		}

		public static void WriteErrorDetail(string message)
		{
			Current.WriteErrorDetailImpl(message);
		}

		#region |-- Abstract Methods --|

		protected abstract void WriteMessageImpl(string message);
		protected abstract void WriteWarningImpl(string message);
		protected abstract void WriteErrorImpl(string message);
		protected abstract void WriteErrorDetailImpl(string message);

		#endregion

		#region |-- Support Methods --|

		private static ConsoleWriter GetCurrent()
		{
			switch (Console.BackgroundColor)
			{
				case ConsoleColor.Black:
				case ConsoleColor.DarkBlue:
				case ConsoleColor.Blue:
					switch (Console.ForegroundColor)
					{
						case ConsoleColor.Gray:
						case ConsoleColor.White:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Cyan);
					}
					break;
				case ConsoleColor.Cyan:
					switch (Console.ForegroundColor)
					{
						case ConsoleColor.Black:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.White, ConsoleColor.DarkYellow);
						case ConsoleColor.Gray:
						case ConsoleColor.White:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.Black, ConsoleColor.Magenta);
					}
					break;
				case ConsoleColor.Red:
					switch (Console.ForegroundColor)
					{
						case ConsoleColor.Cyan:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.Black, ConsoleColor.Magenta);
						case ConsoleColor.White:
						case ConsoleColor.Gray:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.Black, ConsoleColor.Magenta);
						case ConsoleColor.Black:
							return new ColorizedConsoleWriter(Console.ForegroundColor, ConsoleColor.Yellow, ConsoleColor.White, ConsoleColor.Magenta);
					}
					break;
			}

			return new DefaultConsoleWriter();
		}

		private void WriteLine(TextWriter outputStream, string message)
		{
			outputStream.WriteLine(message);
		}

		private void WriteMessage(TextWriter outputStream, ConsoleColor color, string message)
		{
			var foregroundColor = Console.ForegroundColor;

			try
			{
				Console.ForegroundColor = color;
				WriteLine(outputStream, message);
			}
			finally
			{
				Console.ForegroundColor = foregroundColor;
			}
		}

		#endregion

		#region |-- Nested Classes --|

		private class ColorizedConsoleWriter : ConsoleWriter
		{
			private readonly ConsoleColor _messageColor;
			private readonly ConsoleColor _warningColor;
			private readonly ConsoleColor _errorColor;
			private readonly ConsoleColor _errorDetailColor;

			public ColorizedConsoleWriter(ConsoleColor messageColor, ConsoleColor warningColor, ConsoleColor errorColor, ConsoleColor errorDetailColor)
			{
				_messageColor = messageColor;
				_warningColor = warningColor;
				_errorColor = errorColor;
				_errorDetailColor = errorDetailColor;
			}

			protected override void WriteMessageImpl(string message)
			{
				WriteMessage(Console.Out, _messageColor, message);
			}

			protected override void WriteWarningImpl(string message)
			{
				WriteMessage(Console.Out, _warningColor, message);
			}

			protected override void WriteErrorImpl(string message)
			{
				WriteMessage(Console.Error, _errorColor, message);
			}

			protected override void WriteErrorDetailImpl(string message)
			{
				WriteMessage(Console.Error, _errorDetailColor, message);
			}
		}

		private class DefaultConsoleWriter : ConsoleWriter
		{
			protected override void WriteMessageImpl(string message)
			{
				WriteLine(Console.Out, message);
			}

			protected override void WriteWarningImpl(string message)
			{
				WriteLine(Console.Out, message);
			}

			protected override void WriteErrorImpl(string message)
			{
				WriteLine(Console.Error, message);
			}

			protected override void WriteErrorDetailImpl(string message)
			{
				WriteLine(Console.Out, message);
			}
		}

		#endregion
	}
}
