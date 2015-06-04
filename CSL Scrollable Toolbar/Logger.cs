using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrollableToolbar
{
    internal static class Logger
    {
        private static void LogUE(Action<object> logFunc, string message)
        {
            logFunc(string.Format("[{0}] {1}", Mod.AssemblyName, message));
        }

        public static void Debug(string str)
        {
            if (Configuration.Instance.ExtraDebugLogging)
                LogUE(UnityEngine.Debug.Log, "[DEBUG] " + str);
        }

        public static void Debug(string str, params object[] args)
        {
            Debug(string.Format(str, args));
        }

        public static void Info(string str)
        {
            LogUE(UnityEngine.Debug.Log, str);
        }

        public static void Info(string str, params object[] args)
        {
            Info(string.Format(str, args));
        }

        public static void Warning(string str)
        {
            LogUE(UnityEngine.Debug.LogWarning, str);
        }

        public static void Warning(string str, params object[] args)
        {
            Warning(string.Format(str, args));
        }

        public static void Error(string str)
        {
            LogUE(UnityEngine.Debug.LogError, str);
        }

        public static void Error(string str, params object[] args)
        {
            Error(string.Format(str, args));
        }

        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
}
