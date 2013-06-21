// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    public class MobileServiceApplicationTests : TestBase
    {
        [TestMethod]
        public void ArchitectureIsNeutral()
        {
            Assert.AreEqual("Neutral", MobileServiceApplication.Current.OperatingSystemArchitecture);
        }

        [TestMethod]
        public void OperatingSystemIsWindows8()
        {
            Assert.AreEqual("Windows 8", MobileServiceApplication.Current.OperatingSystemName);
        }

        [TestMethod]
        public void OperatingSystemVersionIsNotApplicaable()
        {
            Assert.AreEqual("--", MobileServiceApplication.Current.OperatingSystemVersion);
        }

        [TestMethod]
        public void SdkLanguageIsManaged()
        {
            Assert.AreEqual("Managed", MobileServiceApplication.Current.SdkLanguage);
        }

        [TestMethod]
        public void SdkNameIsZumo()
        {
            Assert.AreEqual("ZUMO", MobileServiceApplication.Current.SdkName);
        }

        [TestMethod]
        public void SdkVersionIsNotNull()
        {
            Assert.IsNotNull(MobileServiceApplication.Current.SdkVersion);
        }

        [TestMethod]
        public void UserAgentHeaderValueIsCorrect()
        {
            Assert.IsTrue(MobileServiceApplication.Current.UserAgentHeaderValue.StartsWith("ZUMO/"));
            Assert.IsTrue(MobileServiceApplication.Current.UserAgentHeaderValue.EndsWith(" (lang=Managed; os=Windows 8; os_version=--; arch=Neutral)"));
        }
    }
}