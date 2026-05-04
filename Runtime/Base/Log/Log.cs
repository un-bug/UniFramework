using System;
using UnityEngine;

namespace UniFramework
{
    public static class Log
    {
        public const string Prefix = "[UniFramework]";

        public static void Info(string message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public static void Warning(string message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }

        public static void Exception(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}