// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;

using Microsoft.WindowsAzure.MobileServices.Test.Functional;
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
                var registration = new TemplateRegistration("uri", "junkBodyTemplate", "testName");
            }
            catch (ArgumentException e)
            {
                // PASSES
            }           
        }

        [TestMethod]
        public void InvalidBodyTemplateIfImproperXml()
        {
            try
            {
                var registration = new TemplateRegistration(
                    "uri",
                    "<foo><visual><binding template=\"ToastText01\"><text id=\"1\">$(message)</text></binding></visual></foo>", 
                    "testName");
            }
            catch (ArgumentException e)
            {
                // PASSES
            }           
        }
    }
}