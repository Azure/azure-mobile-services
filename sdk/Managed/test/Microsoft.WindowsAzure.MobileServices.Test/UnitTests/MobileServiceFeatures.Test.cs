// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("client")]
    public class MobileServiceFeaturesTests : TestBase
    {
        /// <summary>
        /// Verify we have an installation ID created whenever we use a ZUMO
        /// service. 
        /// </summary>
        [TestMethod]
        public void ValidateFeatureCodes()
        {
            Assert.AreEqual("TT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedTable));
            Assert.AreEqual("TU", EnumValueAttribute.GetValue(MobileServiceFeatures.UntypedTable));
            Assert.AreEqual("AT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedApiCall));
            Assert.AreEqual("AJ", EnumValueAttribute.GetValue(MobileServiceFeatures.JsonApiCall));
            Assert.AreEqual("AG", EnumValueAttribute.GetValue(MobileServiceFeatures.GenericApiCall));
            Assert.AreEqual("TC", EnumValueAttribute.GetValue(MobileServiceFeatures.TableCollection));
            Assert.AreEqual("OL", EnumValueAttribute.GetValue(MobileServiceFeatures.Offline));
            Assert.AreEqual("QS", EnumValueAttribute.GetValue(MobileServiceFeatures.AdditionalQueryParameters));
            Assert.IsNull(EnumValueAttribute.GetValue(MobileServiceFeatures.None));
        }
    }
}
