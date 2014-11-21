using System;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("http")]
    public class HttpUtilityTests : TestBase
    {
        [TestMethod]
        public static void TryParseQueryUri_ReturnsTrue_WhenQueryIsRelativeOrAbsoluteUri()
        {
            var data = new[]
            {
                new 
                {
                    ServiceUri = "http://www.test.com", 
                    Query = "/about?$filter=a eq b&$orderby=c", 
                    Absolute = false,
                    Result = "http://www.test.com/about?$filter=a eq b&$orderby=c"
                },
                new 
                {
                    ServiceUri = "http://www.test.com/", 
                    Query = "http://www.test.com/about?$filter=a eq b&$orderby=c", 
                    Absolute = true,
                    Result = "http://www.test.com/about?$filter=a eq b&$orderby=c"
                }
            };

            foreach (var item in data)
            {
                Uri result;
                bool absolute;
                Assert.IsTrue(HttpUtility.TryParseQueryUri(new Uri(item.ServiceUri), item.Query, out result, out absolute));
                Assert.AreEqual(absolute, item.Absolute);
                AssertEx.QueryEquals(result.AbsoluteUri, item.Result);
            }
        }

        [TestMethod]
        public static void TryParseQueryUri_ReturnsFalse_WhenQueryIsNotRelativeOrAbsoluteUri()
        {
            var data = new[]
            {
                new 
                {
                    ServiceUri = "http://www.test.com", 
                    Query = "about?$filter=a eq b&$orderby=c", 
                    Result = "http://www.test.com/about?$filter=a eq b&$orderby=c"
                },
                new 
                {
                    ServiceUri = "http://www.test.com/", 
                    Query = "$filter=a eq b&$orderby=c", 
                    Result = "http://www.test.com/about?$filter=a eq b&$orderby=c"
                }
            };

            foreach (var item in data)
            {
                Uri result;
                bool absolute;
                Assert.IsFalse(HttpUtility.TryParseQueryUri(new Uri(item.ServiceUri), item.Query, out result, out absolute));
                Assert.IsFalse(absolute);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void GetUriWithoutQuery_ReturnsUriWithPath()
        {
            Tuple<string, string>[] input = new[]
            {
                Tuple.Create("http://contoso.com/asdf?$filter=3", "http://contoso.com/asdf"),
                Tuple.Create("http://contoso.com/asdf/def?$filter=3", "http://contoso.com/asdf/def"),
                Tuple.Create("https://contoso.com/asdf/def?$filter=3", "https://contoso.com/asdf/def")
            };

            foreach (var item in input)
            {
                AssertEx.QueryEquals(HttpUtility.GetUriWithoutQuery(new Uri(item.Item1)), item.Item2);
            }
        }

        [TestMethod]
        public void ConstructServicesUri_ReturnsUriWithServiceIfMobileService()
        {
            Tuple<Uri, Uri>[] input = new[]
            {
                Tuple.Create(new Uri("http://contoso.azure-mobile.net"), new Uri("http://contoso.scm.azure-mobile.net")),
                Tuple.Create(new Uri("https://contoso.azure-mobile.net/"), new Uri("https://contoso.scm.azure-mobile.net/")),
                Tuple.Create(new Uri("http://contoso.azure-mobile.cn/asdf"), new Uri("http://contoso.scm.azure-mobile.cn/asdf")),
                Tuple.Create(new Uri("http://127.0.0.1"), new Uri("http://127.0.0.1")),
                Tuple.Create(new Uri("http://localhost:8000"), new Uri("http://localhost:8000")),
                Tuple.Create(new Uri("relativeUri", UriKind.Relative), new Uri("relativeUri", UriKind.Relative)),
            };

            foreach (var item in input)
            {
                Assert.AreEqual(HttpUtility.ConstructServicesUri(item.Item1, "scm"), item.Item2);
            }
        }
    }
}
