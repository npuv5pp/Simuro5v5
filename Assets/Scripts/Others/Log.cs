using System;
using System.IO;
using UnityEngine;

namespace Simuro5v5
{
    class Logger
    {
        public static Logger MainLogger { get; private set; }

        static Logger()
        {
            var now = DateTime.Now;
            string filename = string.Format("{0}_{1}_{2}_{3}_{4}_{5}.log", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            MainLogger = new Logger(filename);
        }

        readonly FileStream LogFile;
        readonly StreamWriter LogWriter;

        public Logger(string logFilePath)
        {
            try
            {
                LogFile = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            catch
            {
                LogFile = null;
                throw;
            }
            finally
            {
                LogWriter = new StreamWriter(LogFile)
                {
                    AutoFlush = true
                };
            }
        }

        ~Logger()
        {
            if (LogFile != null)
            {
                LogFile.Close();
            }
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
            if (LogFile != null)
            {
                LogWriter.Write("ERROR {0}: {1}\n", DateTime.Now.ToString(), text);
            }
        }

        public void LogError(string text, string remark)
        {
            if (LogFile != null)
            {
                LogWriter.Write("ERROR {0} ({1}): {2}\n", DateTime.Now.ToString(), remark, text);
            }
        }

        public void LogInfo(string text)
        {
            if (LogFile != null)
            {
                LogWriter.Write("INFO {0}: {1}\n", DateTime.Now.ToString(), text);
            }
        }

        public void LogInfo(string text, string remark)
        {
            if (LogFile != null)
            {
                LogWriter.Write("INFO {0} ({1}): {2}\n", DateTime.Now.ToString(), remark, text);
            }
        }
    }
}
