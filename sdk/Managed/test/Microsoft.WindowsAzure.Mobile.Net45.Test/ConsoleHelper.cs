// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public static class ConsoleHelper
    {
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ParentProcess = -1;

        private static StreamWriter consoleWriter;

        public static void Attach()
        {
            consoleWriter = new StreamWriter(Console.OpenStandardOutput());
            AttachConsole(ParentProcess);
        }

        public static void Write(string format, params object[] args)
        {
            consoleWriter.Write(format, args);
            Console.Write(format, args);
        }

        public static void WriteLine(string message)
        {
            consoleWriter.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
