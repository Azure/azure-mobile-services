// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Http
{
    /// <summary>
    /// Provides a container for information for mappings between
    /// simple names (identifying tables, APIs, etc.) and their routes.
    /// </summary>
    internal class RouteHelper
    {
        /// <summary>
        /// Gets the route associated with the given kind of route and route name.
        /// </summary>
        /// <param name="routeKind">The kind of the route.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <returns>The route associated with the given kind of route and route name.</returns>
        public static string GetRoute(IMobileServiceClient client, RouteKind routeKind, string routeName)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            Uri test;
            if (Uri.TryCreate(routeName, UriKind.Absolute, out test))
            {
                return routeName;
            }

            int startOfQuery = routeName.IndexOf('?');

            string pathPartOfName = (startOfQuery < 0 ? routeName : routeName.Substring(0, startOfQuery));
            string queryPartOfName = (startOfQuery < 0 ? null : routeName.Substring(startOfQuery));

            string path;
            if (pathPartOfName.IndexOf('/') == 0)
            {
                path = pathPartOfName;
            }
            else
            {
                switch (routeKind)
                {
                    case RouteKind.API:
                        path = "api/" + pathPartOfName;
                        break;
                    case RouteKind.Table:
                        path = "tables/" + pathPartOfName;
                        break;
                    case RouteKind.Login:
                        path = "login/" + pathPartOfName;
                        break;
                    case RouteKind.Push:
                        path = "push/" + pathPartOfName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("clientRoute");
                }
            }

            // TODO: Need to choose which base URI to combine. For example,
            // if override the Login, need to specify the User path.
            path = path + queryPartOfName;

            Uri u = null;
            switch (routeKind)
            {
                case RouteKind.API:
                case RouteKind.Table:
                    u = new Uri(client.ApplicationUri, path);
                    break;
                case RouteKind.Login:
                case RouteKind.Push:
                    var applicationServicesUri = HttpUtility.ConstructServicesUri(client.ApplicationUri, "scm");
                    u = new Uri(applicationServicesUri, path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("clientRoute");
            }

            return u.ToString();
        }
    }
}
