// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// Provides logging methods.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Log(object message);
        
        /// <summary>
        /// Logs the specified message with warning level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogWarning(object message);
        
        /// <summary>
        /// Logs the specified message with error level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogError(object message);
        
        /// <summary>
        /// Logs the specified message with exception level.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        void LogException(Exception exception);
    }
}