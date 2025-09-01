using System;
using System.Collections.Generic;
using System.IO;

namespace MiniEngine.Utility.Logging
{
    public class FileLogger(string fileName, bool enableDebug = false) : ILogger
    {
        private readonly Queue<string> _writeQueue = new();
        private StreamWriter? _writer;
        private bool _sessionStarted;

        private void WriteToFile(string message)
        {
            _writeQueue.Enqueue(message);
            if (_writer != null)
                return;

            var newlyCreated = !File.Exists(fileName);
            if (newlyCreated)
                File.Create(fileName);

            try
            {
                _writer = File.AppendText(fileName);
                if (!_sessionStarted)
                {
                    _writer.WriteLine();
                    _sessionStarted = true;
                }
                while (_writeQueue.TryDequeue(out var writeMsg))
                    _writer.WriteLine(writeMsg);
                _writer.Close();
            }
            catch
            {
                // ignored
            }

            _writer = null;
        }

        public void Info(DateTime dateTime, string message)
        {
            WriteToFile($"[{dateTime}] [Info] {message}");
        }

        public void Warn(DateTime dateTime, string message)
        {
            WriteToFile($"[{dateTime}] [Warn] {message}");
        }

        public void Error(DateTime dateTime, string message)
        {
            WriteToFile($"[{dateTime}] [Error] {message}");
        }

        public void Fatal(DateTime dateTime, string message)
        {
            WriteToFile($"[{dateTime}] [Fatal] {message}");
        }

        public void Fatal(DateTime dateTime, string message, Exception exception)
        {
            WriteToFile($"[{dateTime}] [Fatal] {message}");
            WriteToFile($"Exception:\n{exception}");
        }

        public void Debug(DateTime dateTime, string message)
        {
            if (enableDebug)
                WriteToFile($"[{dateTime}] [Debug] {message}");
        }

        public void Trace(DateTime dateTime, string message, Exception exception)
        {
            WriteToFile($"[{dateTime}] [Trace] {message}");
            WriteToFile($"Exception:\n{exception}");
        }
    }
}
