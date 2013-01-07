using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumoE2ETestApp.Tests
{
    // Used as the type parameter to positive tests
    internal class ExceptionTypeWhichWillNeverBeThrown : Exception
    {
        private ExceptionTypeWhichWillNeverBeThrown() { }
    }
}
