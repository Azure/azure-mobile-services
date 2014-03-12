using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    sealed class TestDescription
        : Java.Lang.Object
    {
        public TestDescription (TestMethod test, string log)
        {
            Test = test;
            Log = log;
        }

        public TestMethod Test
        {
            get;
            private set;
        }

        public string Log
        {
            get;
            private set;
        }
    }
}