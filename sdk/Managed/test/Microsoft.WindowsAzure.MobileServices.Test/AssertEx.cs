// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Helper assert functions
    /// </summary>
    public static class AssertEx
    {
        public static TException Throws<TException>(Action action) where TException : Exception
        {
            return Throws<TException>(() => Task.Run(action)).Result;
        }

        public static async Task<TException> Throws<TException>(Func<Task> action) where TException : Exception
        {
            TException thrown = null;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                thrown = ex as TException;
            }

            Assert.IsNotNull(thrown, "Expected exception not thrown");

            return thrown;
        }

        public static void MatchUris(List<HttpRequestMessage> requests, params string[] uris)
        {
            Assert.AreEqual(requests.Count, uris.Length);
            for (int i = 0; i < uris.Length; i++)
            {
                Assert.AreEqual(requests[i].RequestUri.ToString(), uris[i]);
            }
        }
    }
}
