using Microsoft.WindowsAzure.MobileServices.Http;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("http")]
    public class RouteHelperTests : TestBase
    {
        [TestMethod]
        public static void GetRoute_ReturnsRouteForSimpleNames()
        {
            IMobileServiceClient client = new MobileServiceClient("http://contoso.azure-mobile.net", null);

            var data = new[]
            {
                new { Kind = RouteKind.API, Name = "myapi", Route = "http://contoso.azure-mobile.net/api/myapi" },
                new { Kind = RouteKind.Table, Name = "mytable", Route = "http://contoso.azure-mobile.net/tables/mytable" },
                new { Kind = RouteKind.Login, Name = "twitter", Route = "http://contoso.scm.azure-mobile.net/login/twitter" },
                new { Kind = RouteKind.Push, Name = "custom", Route = "http://contoso.scm.azure-mobile.net/push/custom" },
            };

            foreach (var item in data)
            {
                string actualRoute = RouteHelper.GetRoute(client, item.Kind, item.Name);
                Assert.AreEqual(item.Route, actualRoute);
            }
        }

        [TestMethod]
        public static void GetRoute_ForHttps_ReturnsRouteForSimpleNames()
        {
            IMobileServiceClient client = new MobileServiceClient("https://contoso.azure-mobile.net", null);

            var data = new[]
            {
                new { Kind = RouteKind.API, Name = "myapi", Route = "https://contoso.azure-mobile.net/api/myapi" },
                new { Kind = RouteKind.Table, Name = "mytable", Route = "https://contoso.azure-mobile.net/tables/mytable" },
                new { Kind = RouteKind.Login, Name = "twitter", Route = "https://contoso.scm.azure-mobile.net/login/twitter" },
                new { Kind = RouteKind.Push, Name = "custom", Route = "https://contoso.scm.azure-mobile.net/push/custom" },
            };

            foreach (var item in data)
            {
                string actualRoute = RouteHelper.GetRoute(client, item.Kind, item.Name);
                Assert.AreEqual(item.Route, actualRoute);
            }
        }

        [TestMethod]
        public static void GetRoute_ForNonMobileService_ReturnsRouteForSimpleNames()
        {
            IMobileServiceClient client = new MobileServiceClient("http://www.contoso.com", null);

            var data = new[]
            {
                new { Kind = RouteKind.API, Name = "myapi", Route = "http://www.contoso.com/api/myapi" },
                new { Kind = RouteKind.Table, Name = "mytable", Route = "http://www.contoso.com/tables/mytable" },
                new { Kind = RouteKind.Login, Name = "twitter", Route = "http://www.contoso.com/login/twitter" },
                new { Kind = RouteKind.Push, Name = "custom", Route = "http://www.contoso.com/push/custom" },
            };

            foreach (var item in data)
            {
                string actualRoute = RouteHelper.GetRoute(client, item.Kind, item.Name);
                Assert.AreEqual(item.Route, actualRoute);
            }
        }

        [TestMethod]
        public static void GetRoute_ReturnsRouteForUris()
        {
            IMobileServiceClient client = new MobileServiceClient("http://contoso.azure-mobile.net", null);

            var data = new[]
            {
                new { Kind = RouteKind.API, Name = "/myapi", Route = "http://contoso.azure-mobile.net/myapi" },
                new { Kind = RouteKind.Table, Name = "/api/mytable/a/b", Route = "http://contoso.azure-mobile.net/api/mytable/a/b" },
                new { Kind = RouteKind.Table, Name = "?api=mytable&a=b", Route = "http://contoso.azure-mobile.net/tables/?api=mytable&a=b" },
                new { Kind = RouteKind.Login, Name = "a/b", Route = "http://contoso.scm.azure-mobile.net/login/a/b" },
                new { Kind = RouteKind.Login, Name = "https://fabrikam.com/twitter", Route = "https://fabrikam.com/twitter" },
                new { Kind = RouteKind.Push, Name = "http://127.0.0.1/custom", Route = "http://127.0.0.1/custom" },
            };

            foreach (var item in data)
            {
                string actualRoute = RouteHelper.GetRoute(client, item.Kind, item.Name);
                Assert.AreEqual(item.Route, actualRoute);
            }
        }
    }
}
