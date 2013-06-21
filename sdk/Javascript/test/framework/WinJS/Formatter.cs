// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test.WinJS
{
    /// <summary>
    /// Utility to provide .NET string formatting to WinJS tests.
    /// </summary>
    public static class Formatter
    {
        /// <summary>
        /// Format a string.
        /// </summary>
        /// <param name="format">The format specifier.</param>
        /// <param name="arg">Format argument.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object arg)
        {
            return string.Format(format, arg);
        }

        /// <summary>
        /// Format a string.
        /// </summary>
        /// <param name="format">The format specifier.</param>
        /// <param name="arg0">Format argument.</param>
        /// <param name="arg1">Format argument.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object arg0, object arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        /// <summary>
        /// Format a string.
        /// </summary>
        /// <param name="format">The format specifier.</param>
        /// <param name="arg0">Format argument.</param>
        /// <param name="arg1">Format argument.</param>
        /// <param name="arg2">Format argument.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Format a string.
        /// </summary>
        /// <param name="format">The format specifier.</param>
        /// <param name="arg0">Format argument.</param>
        /// <param name="arg1">Format argument.</param>
        /// <param name="arg2">Format argument.</param>
        /// <param name="arg3">Format argument.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object arg0, object arg1, object arg2, object arg3)
        {
            return string.Format(format, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// Format a string.
        /// </summary>
        /// <param name="format">The format specifier.</param>
        /// <param name="arg0">Format argument.</param>
        /// <param name="arg1">Format argument.</param>
        /// <param name="arg2">Format argument.</param>
        /// <param name="arg3">Format argument.</param>
        /// <param name="arg4">Format argument.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            return string.Format(format, arg0, arg1, arg2, arg3, arg4);
        }
    }
}
