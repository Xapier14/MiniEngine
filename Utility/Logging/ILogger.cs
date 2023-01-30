using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Utility.Logging
{
    public interface ILogger
    {
        public void Info(DateTime dateTime, string message);
        public void Warn(DateTime dateTime, string message);
        public void Error(DateTime dateTime, string message);
        public void Fatal(DateTime dateTime, string message);
        public void Fatal(DateTime dateTime, string message, Exception exception);

        public void Debug(DateTime dateTime, string message);
        public void Trace(DateTime dateTime, string message, Exception exception);
    }
}
