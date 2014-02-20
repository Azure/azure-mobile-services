using System;

namespace ZumoE2ETestApp.Tests
{
    class SkipException : Exception
    {
        public SkipException(string message)
            : base(message)
        {
        }
    }
}
