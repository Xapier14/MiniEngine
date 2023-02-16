using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Utility.Logging
{
    public class ConsoleLogger : ILogger
    {
        private bool _debugging = false;
        private const ConsoleColor
            INFO_COLOR = ConsoleColor.Gray,
            WARN_COLOR = ConsoleColor.Yellow,
            ERROR_COLOR = ConsoleColor.DarkRed,
            FATAL_COLOR = ConsoleColor.Red,
            DEBUG_COLOR = ConsoleColor.Green,
            TRACE_COLOR = ConsoleColor.Magenta;

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
            WriteLine(INFO_COLOR, dateTime, message, "INFO");
        }

        public void Warn(DateTime dateTime, string message)
        {
            WriteLine(WARN_COLOR, dateTime, message, "WARNING");
        }

        public void Error(DateTime dateTime, string message)
        {
            WriteLine(ERROR_COLOR, dateTime, message, "ERROR");
        }

        public void Fatal(DateTime dateTime, string message)
        {
            WriteLine(FATAL_COLOR, dateTime, message, "FATAL");
        }

        public void Fatal(DateTime dateTime, string message, Exception exception)
        {
            WriteLine(FATAL_COLOR, dateTime, message, "FATAL");
            WriteLine(FATAL_COLOR, "Exception:\n{0}", exception);
        }

        public void Debug(DateTime dateTime, string message)
        {
            if (_debugging || AlwaysShowDebug)
                WriteLine(DEBUG_COLOR, dateTime, message, "DEBUG");
        }

        public void Trace(DateTime dateTime, string message, Exception exception)
        {
            WriteLine(TRACE_COLOR, dateTime, message, "TRACE");
            WriteLine(TRACE_COLOR, "Exception:\n{0}", exception);
        }
    }
}
