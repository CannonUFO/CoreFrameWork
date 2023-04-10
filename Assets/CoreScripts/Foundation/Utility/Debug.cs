
using System;

namespace ible.Foundation.Utility
{
    public static class Debug
    {
        private static bool _isEnableLog;

        public static void Init(bool enableLog)
        {
            UnityEngine.Debug.unityLogger.logEnabled = _isEnableLog = enableLog;
        }

        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }

        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }

        public static void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition);
        }

        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }
}
