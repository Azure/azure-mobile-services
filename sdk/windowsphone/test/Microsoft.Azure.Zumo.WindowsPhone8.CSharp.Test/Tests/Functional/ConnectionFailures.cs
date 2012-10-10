// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public class ConnectionFailures : FunctionalTestBase
    {
        [AsyncTestMethod]
        [ExcludeTest("Only runs correctly when the application has no network connection")]
        public async Task OfflineError()
        {
            // Ensure we're offline
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp("http://www.microsoft.com");
                await Task<WebResponse>.Factory.FromAsync(
                        request.BeginGetResponse,
                        request.EndGetResponse,
                        request);

                Assert.Fail("There must be no network connection to complete this test");
            }
            catch (WebException ex)
            {
                Assert.AreNotEqual(ex.Status, WebExceptionStatus.Success);
            }

            // Make a ZUMO request
            string appUrl = "http://www.microsoft.com";
            string collection = "tests";
            try
            {
                await new MobileServiceClient(appUrl).GetTable(collection).ReadAsync("query-does-not-matter");
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.AreNotEqual(ex.Response.ResponseStatus, ServiceFilterResponseStatus.Success);
                App.Harness.Log(string.Format("Caught: {0}", ex.ToString()));
            }
        }

        [AsyncTestMethod]
        [FunctionalTest]
        [ExcludeTest("Only runs correctly when the application has been setup with fiddler intercepting HTTP in between (see bug #462250)")]       
        public async Task HttpsInspection()
        {
            // Make a ZUMO request
            try
            {
                IMobileServiceTable<ToDo> table = GetClient().GetTable<ToDo>();
                ToDo item = new ToDo { Title = "Testing", Complete = false };
                await table.InsertAsync(item);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.AreNotEqual(ex.Response.ResponseStatus, ServiceFilterResponseStatus.Success);
                App.Harness.Log(string.Format("Caught: {0}", ex.ToString()));
            }
        }
    }
}
