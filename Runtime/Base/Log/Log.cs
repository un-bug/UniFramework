using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniFramework
{
    public static class Log
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string message, Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        public static void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
}