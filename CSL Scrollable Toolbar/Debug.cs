using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrollableToolbar
{
    internal static class Debug
    {
        public static void Log(string str)
        {
            string message = "[ScrollableToolbar] " + str;
            UnityEngine.Debug.Log(message);
        }

        public static void Log(string str, params object[] args)
        {
            Log(string.Format(str, args));
        }

        public static void Warning(string str)
        {
            string message = "[ScrollableToolbar] " + str;
            UnityEngine.Debug.LogWarning(message);
        }

        public static void Warning(string str, params object[] args)
        {
            Warning(string.Format(str, args));
        }

        public static void Error(string str)
        {
            string message = "[ScrollableToolbar] " + str;
            UnityEngine.Debug.LogError(message);
        }

        public static void Error(string str, params object[] args)
        {
            Error(string.Format(str, args));
        }
    }
}
