using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simuro5v5
{
    class Logger
    {
        public readonly static Logger MainLogger = new Logger("match.log");

        readonly FileStream LogFile;
        readonly StreamWriter LogWriter;

        public Logger(string logFilePath)
        {
            LogFile = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            LogWriter = new StreamWriter(LogFile);
            LogWriter.AutoFlush = true;
        }

        ~Logger()
        {
            LogFile.Close();
        }

        public void Close()
        {
            LogFile.Close();
        }

        public void Log(string text)
        {
            LogInfo(text);
        }

        public void Log(string text, string remark)
        {
            LogInfo(text, remark);
        }

        public void LogError(string text)
        {
            LogWriter.Write("ERROR {0}: {1}\n", DateTime.Now.ToString(), text);
        }

        public void LogError(string text, string remark)
        {
            LogWriter.Write("ERROR {0} ({1}): {2}\n", DateTime.Now.ToString(), remark, text);
        }

        public void LogInfo(string text)
        {
            LogWriter.Write("INFO {0}: {1}\n", DateTime.Now.ToString(), text);
        }

        public void LogInfo(string text, string remark)
        {
            LogWriter.Write("INFO {0} ({1}): {2}\n", DateTime.Now.ToString(), remark, text);
        }
    }
}
