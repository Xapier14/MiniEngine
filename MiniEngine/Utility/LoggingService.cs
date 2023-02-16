using System;
using System.Collections.Generic;
using System.Linq;
using MiniEngine.Utility.Logging;

namespace MiniEngine.Utility
{
    public static class LoggingService
    {
        private static readonly List<ILogger> _loggers = new();

        public static void Use(ILogger logger)
        {
            if (_loggers.Any(i => i.GetType() == logger.GetType()))
                return;
            _loggers.Add(logger);
        }

        public static void Info(string message, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Info(now, string.Format(message, args)));
        }

        public static void Warn(string message, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Warn(now, string.Format(message, args)));
        }

        public static void Error(string message, params object[] args)
        {

            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Error(now, string.Format(message, args)));
        }

        public static void Fatal(string message, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Fatal(now, string.Format(message, args)));
        }

        public static void Fatal(string message, Exception exception, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Fatal(now, string.Format(message, args), exception));
        }

        public static void Debug(string message, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Debug(now, string.Format(message, args)));
        }

        public static void Trace(string message, Exception exception, params object[] args)
        {
            var now = DateTime.Now;
            _loggers.ForEach(logger => logger.Trace(now, string.Format(message, args), exception));
        }
    }
}
