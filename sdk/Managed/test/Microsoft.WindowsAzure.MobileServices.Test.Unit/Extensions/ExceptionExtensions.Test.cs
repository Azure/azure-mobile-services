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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Extensions
{
    [TestClass]
    public class ExceptionExtensionsTests
    {
        [TestMethod]
        public void IsNetworkError_ReturnsTrue_OnNetworkErrors()
        {
            Assert.IsTrue(ExceptionExtensions.IsNetworkError(new HttpRequestException()));
        }

        [TestMethod]
        public void IsNetworkError_ReturnsFalse_OnOtherErrors()
        {
            Assert.IsFalse(ExceptionExtensions.IsNetworkError(new Exception()));
            Assert.IsFalse(ExceptionExtensions.IsNetworkError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage())));
        }

        [TestMethod]
        public void IsAuthenticationError_ReturnsTrue_OnAuthErrors()
        {
            Assert.IsTrue(ExceptionExtensions.IsAuthenticationError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.Unauthorized))));
        }

        [TestMethod]
        public void IsAuthenticationError_ReturnsFalse_OnOtherErrors()
        {
            Assert.IsFalse(ExceptionExtensions.IsAuthenticationError(new Exception()));
            Assert.IsFalse(ExceptionExtensions.IsAuthenticationError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage())));
        }
    }
}
