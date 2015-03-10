using Microsoft.WindowsAzure.MobileServices.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class TestHttpDelegatingHandler
    {
        public static DelegatingHandler CreateTestHttpHandler(string expectedUri, HttpMethod expectedMethod, string responseContent, HttpStatusCode? httpStatusCode = null, Uri location = null, string expectedRequestContent = null)
        {
            var handler = new TestHttpHandler
            {
                OnSendingRequest = message =>
                {
                    Assert.AreEqual(expectedUri, message.RequestUri.OriginalString, "The Http Uri used to send the request is different than expected.");
                    Assert.AreEqual(expectedMethod, message.Method, "The Http Method used to send the request is different than expected.");

                    if (expectedRequestContent != null)
                    {
                        var messageContent = Regex.Replace(message.Content.ReadAsStringAsync().Result, @"\s+", String.Empty);
                        expectedRequestContent = Regex.Replace(expectedRequestContent, @"\s+", String.Empty);
                        Assert.AreEqual(expectedRequestContent, messageContent, "The Http request content is different than expected.");
                    }

                    return Task.FromResult(message);
                }
            };

            if (responseContent != null)
            {
                handler.SetResponseContent(responseContent);
            }
            else
            {
                handler.Response = new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (location != null)
            {
                handler.Response.Headers.Location = location;
            }

            if (httpStatusCode.HasValue)
            {
                handler.Response.StatusCode = httpStatusCode.Value;
            }
            return handler;
        }
    }
}
