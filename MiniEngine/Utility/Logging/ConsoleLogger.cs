using System;
using System.Diagnostics;

namespace MiniEngine.Utility.Logging
{
    public class ConsoleLogger : ILogger
    {
        private bool _debugging;
        private const ConsoleColor
            InfoColor = ConsoleColor.Gray,
            WarnColor = ConsoleColor.Yellow,
            ErrorColor = ConsoleColor.DarkRed,
            FatalColor = ConsoleColor.Red,
            DebugColor = ConsoleColor.Green,
            TraceColor = ConsoleColor.Magenta;

        public bool AlwaysShowDebug { get; set; } = false;

        public ConsoleLogger()
        {
            CheckIfDebug();
        }

        [Conditional("DEBUG")]
        private void CheckIfDebug() => _debugging = true;

        private static void SetColor(ConsoleColor color, out ConsoleColor oldColor)
        {
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        private static void WriteLine(ConsoleColor color, string message, params object[] args)
        {
            SetColor(color, out var oldColor);
            Console.WriteLine(message, args);
            SetColor(oldColor, out _);
        }

        private static void WriteLine(ConsoleColor color, DateTime dateTime, string message, string label)
        {
            WriteLine(color, "[{0}] [{1}] {2}", dateTime, label, message);
        }

        public void Info(DateTime dateTime, string message)
        {
            WriteLine(InfoColor, dateTime, message, "INFO");
        }

        public void Warn(DateTime dateTime, string message)
        {
            WriteLine(WarnColor, dateTime, message, "WARNING");
        }

        public void Error(DateTime dateTime, string message)
        {
            WriteLine(ErrorColor, dateTime, message, "ERROR");
        }

        public void Fatal(DateTime dateTime, string message)
        {
            WriteLine(FatalColor, dateTime, message, "FATAL");
        }

        public void Fatal(DateTime dateTime, string message, Exception exception)
        {
            WriteLine(FatalColor, dateTime, message, "FATAL");
            WriteLine(FatalColor, "Exception:\n{0}", exception);
        }

        public void Debug(DateTime dateTime, string message)
        {
            if (_debugging || AlwaysShowDebug)
                WriteLine(DebugColor, dateTime, message, "DEBUG");
        }

        public void Trace(DateTime dateTime, string message, Exception exception)
        {
            WriteLine(TraceColor, dateTime, message, "TRACE");
            WriteLine(TraceColor, "Exception:\n{0}", exception);
        }
    }
}
