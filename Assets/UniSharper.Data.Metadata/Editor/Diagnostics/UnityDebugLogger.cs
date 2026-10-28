using System;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal class UnityDebugLogger : ILogger
    {
        public void Log(object message)
        {
            Debug.Log(message);
        }

        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public void LogError(object message)
        {
            Debug.LogError(message);
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}