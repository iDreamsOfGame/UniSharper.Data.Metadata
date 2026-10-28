// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata
{
    public static class Debugger
    {
        public static ILogger Logger { get; set; } = new UnityDebugLogger();
        
        internal static void Log(object message) => Logger?.Log(message);

        internal static void LogWarning(object message) => Logger?.LogWarning(message);

        internal static void LogError(object message) => Logger?.LogError(message);

        internal static void LogException(Exception exception) => Logger?.LogException(exception);
    }
}