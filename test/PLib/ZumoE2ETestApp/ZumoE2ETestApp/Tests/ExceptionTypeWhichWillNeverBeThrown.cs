// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace ZumoE2ETestApp.Tests
{
    // Used as the type parameter to positive tests
    internal class ExceptionTypeWhichWillNeverBeThrown : Exception
    {
        private ExceptionTypeWhichWillNeverBeThrown() { }
    }
}
