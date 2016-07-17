using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace MK.Logging
{
    public static class Log
    {
        public const string ErrorCategory = "Error";
        public const string GeneralCategory = "General";

        public const int MaxLevel = 10;

        private static LogWriter _writer;

        private static LogWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    var factory = new LogWriterFactory();
                    Logger.SetLogWriter(factory.Create());
                    _writer = Logger.Writer;
                }

                return _writer ;
            }
        }

        public static void LogError(object msg)
        {
            Writer.Write(msg, ErrorCategory);
        }

        public static void LogException(Exception ex)
        {
            LogError(ex);
        }

        public static void LogMessage(object msg)
        {
            Writer.Write(msg, GeneralCategory);
        }

        public static void LogOperationStart(string operationDescription)
        {
            LogMessage(String.Format("{0} START", operationDescription));
        }

        public static void LogOperationEnd(string operationDescription)
        {
            LogMessage(String.Format("{0} END", operationDescription));
        }

        public static void LogOperationEndWithResult(string operationDescription, object res)
        {
            LogMessage(String.Format("{0} END RESULT = {1}", operationDescription, res));
        }

        public static void LogObject(object obj, int depth = 3)
        {
            try
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    ObjectDumper.Write(obj, depth, sw);
                }

                LogMessage(sb.ToString());
            }
            catch
            {
                LogMessage("Cannot log an object to a file!");
            }
        }

        #region Default Config

        public static void UseDefaultConfig()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Default.config");
            File.WriteAllText(path, Resources.Res.Default);
            var factory = new LogWriterFactory(new FileConfigurationSource(path));
            Logger.SetLogWriter(factory.Create());
            _writer = Logger.Writer;
        }

        #endregion

        #region Timing


        public static bool EnableTimning { get; set; }

        [ThreadStatic]
        private static int _level;

        [ThreadStatic]
        private static Dictionary<int, Stopwatch> _stopwatch;

        private static Stopwatch GetStopwatch(int level)
        {
            if (_stopwatch == null)
            {
                _stopwatch = new Dictionary<int, Stopwatch>();
            }

            Stopwatch sw;

            if (_stopwatch.TryGetValue(level, out sw))
                return sw;

            sw = _stopwatch[level] = new Stopwatch();

            return sw;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StartTiming(string subOperation = null)
        {
            if (EnableTimning)
            {
                var m = new StackFrame(1).GetMethod();
                if (String.IsNullOrEmpty(subOperation))
                    LogMessage(String.Format("{4}(Thread {0}, Level {1}) Start of an operation '{2}.{3}'", Thread.CurrentThread.ManagedThreadId, _level, m.DeclaringType.Name, m.Name, new String('\t', _level)));
                else
                    LogMessage(String.Format("{5}(Thread {0}, Level {1}) Start of an operation '{2}.{3}' - '{4}'", Thread.CurrentThread.ManagedThreadId, _level, m.DeclaringType.Name, m.Name, subOperation, new String('\t', _level)));

                var sw = GetStopwatch(_level);

                if (sw.IsRunning)
                    throw new Exception("Stopwatch has been alread started!");

                sw.Start();

                _level++;

                if (_level > MaxLevel)
                    throw new IndexOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StopTiming(string subOperation = null)
        {
            if (EnableTimning)
            {
                _level--;

                if (_level < 0)
                    throw new IndexOutOfRangeException();

                var sw = GetStopwatch(_level);

                if (!sw.IsRunning)
                    throw new Exception("Stopwatch has not been started yet!");

                sw.Stop();

                var m = new StackFrame(1).GetMethod();
                if (String.IsNullOrEmpty(subOperation))
                    LogMessage(String.Format("{5}(Thread {0}, Level {1}) Elapsed time of an operation '{2}.{3}' is {4}", Thread.CurrentThread.ManagedThreadId, _level, m.DeclaringType.Name, m.Name, sw.Elapsed, new String('\t', _level)));
                else
                    LogMessage(String.Format("{6}(Thread {0}, Level {1}) Elapsed time of an operation '{2}.{3}' - '{4}' is {5}", Thread.CurrentThread.ManagedThreadId, _level, m.DeclaringType.Name, m.Name, subOperation, sw.Elapsed, new String('\t', _level)));

                sw.Reset();
            }
        }

        #endregion
    }
}
