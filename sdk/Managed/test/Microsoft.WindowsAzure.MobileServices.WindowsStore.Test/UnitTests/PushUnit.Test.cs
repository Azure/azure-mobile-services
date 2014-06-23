// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;

using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("push")]
    public class PushUnit : TestBase
    {
        [TestMethod]
        public void InvalidBodyTemplateIfNotXml()
        {
            try
            {
                var registration = new WnsTemplateRegistration("uri", "junkBodyTemplate", "testName");
                Assert.Fail("Expected templateBody that is not XML to throw ArgumentException");
            }
            catch
            {
                // PASSES
            }           
        }

        [TestMethod]
        public void InvalidBodyTemplateIfImproperXml()
        {
            try
            {
                var registration = new WnsTemplateRegistration(
                    "uri",
                    "<foo><visual><binding template=\"ToastText01\"><text id=\"1\">$(message)</text></binding></visual></foo>", 
                    "testName");
                Assert.Fail("Expected templateBody with unexpected first XML node to throw ArgumentException");
            }
            catch
            {
                // PASSES
            }           
        }
    }
}