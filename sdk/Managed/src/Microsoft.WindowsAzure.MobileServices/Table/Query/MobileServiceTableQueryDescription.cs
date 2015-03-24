// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    /// <summary>
    /// Represents the structural elements of a Mobile Services query over the
    /// subset of OData it uses.
    /// </summary>
    /*
     * Our LINQ OData Provider will effectively compile expression trees into
     * MobileServiceTableQueryDescription instances which can be passed to a
     * MobileServiceCollectionView and evaluated on the server.  We don't
     * compile the expression all the way down to a single Uri fragment
     * because we'll need to re-evaluate the query with different Skip/Top
     * values for paging and data virtualization.
    */
    public sealed class MobileServiceTableQueryDescription
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MobileServiceTableQueryDescription"/> class.
        /// </summary>
        public MobileServiceTableQueryDescription(string tableName)
        {
            Debug.Assert(tableName != null, "tableName cannot be null");

            this.TableName = tableName;
            this.Selection = new List<string>();
            this.Projections = new List<Delegate>();
            this.Ordering = new List<OrderByNode>();
        }

        /// <summary>
        /// Gets the uri path if the query was an absolute or relative uri
        /// </summary>
        internal string UriPath { get; private set; }

        /// <summary>
        /// Gets or sets the name of the table being queried.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets or sets the query's filter expression.
        /// </summary>
        public QueryNode Filter { get; set; }

        /// <summary>
        /// Gets a list of fields that should be selected from the items in
        /// the table.
        /// </summary>
        public IList<string> Selection { get; private set; }

        /// <summary>
        /// Gets a list of expressions that specify the ordering
        /// constraints imposed on the query.
        /// </summary>
        public IList<OrderByNode> Ordering { get; private set; }

        /// <summary>
        /// Gets or sets the number of elements to skip.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to take.
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Gets a collection of projections that should be applied to each element of
        /// the query.
        /// </summary>
        internal List<Delegate> Projections { get; private set; }

        /// <summary>
        /// Gets or sets the type of the argument to the projection (i.e., the
        /// type that should be deserialized).
        /// </summary>
        internal Type ProjectionArgumentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the query should request
        /// the total count for all the records that would have been returned
        /// if the server didn't impose a data cap.
        /// </summary>
        public bool IncludeTotalCount { get; set; }

        /// <summary>
        /// Creates a copy of <see cref="MobileServiceTableQueryDescription"/>
        /// </summary>
        /// <returns>The cloned query</returns>
        public MobileServiceTableQueryDescription Clone()
        {
            var clone = new MobileServiceTableQueryDescription(this.TableName);

            clone.Filter = this.Filter;
            clone.Selection = this.Selection.ToList();
            clone.Ordering = this.Ordering.ToList();
            clone.Projections = this.Projections.ToList();
            clone.ProjectionArgumentType = this.ProjectionArgumentType;
            clone.Skip = this.Skip;
            clone.Top = this.Top;
            clone.IncludeTotalCount = this.IncludeTotalCount;

            return clone;
        }

        /// <summary>
        /// Convert the query structure into the standard OData URI protocol
        /// for queries.
        /// </summary>
        /// <returns>
        /// URI fragment representing the query.
        /// </returns>
        public string ToODataString()
        {
            char? separator = null;
            StringBuilder text = new StringBuilder();

            // Add the filter
            if (this.Filter != null)
            {
                string filterStr = ODataExpressionVisitor.ToODataString(this.Filter);
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", separator, ODataOptions.Filter, filterStr);
                separator = '&';
            }

            // Add the ordering
            if (this.Ordering.Count > 0)
            {
                IEnumerable<string> orderings = this.Ordering
                                                    .Select(o =>
                                                    {
                                                        string result = ODataExpressionVisitor.ToODataString(o.Expression);
                                                        if (o.Direction == OrderByDirection.Descending)
                                                        {
                                                            result += " desc";
                                                        }
                                                        return result;
                                                    });

                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", separator, ODataOptions.OrderBy, string.Join(",", orderings));
                separator = '&';
            }

            // Skip any elements
            if (this.Skip.HasValue && this.Skip >= 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", separator, ODataOptions.Skip, this.Skip);
                separator = '&';
            }

            // Take the desired number of elements
            if (this.Top.HasValue && this.Top >= 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", separator, ODataOptions.Top, this.Top);
                separator = '&';
            }

            // Add the selection
            if (this.Selection.Count > 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", separator, ODataOptions.Select, string.Join(",", this.Selection.Select(Uri.EscapeDataString)));
                separator = '&';
            }

            // Add the total count
            if (this.IncludeTotalCount)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}=allpages", separator, ODataOptions.InlineCount);
                separator = '&';
            }

            return text.ToString();
        }

        /// <summary>
        /// Parses a OData query and creates a <see cref="MobileServiceTableQueryDescription"/> instance
        /// </summary>
        /// <param name="tableName">The name of table for the query.</param>
        /// <param name="query">The odata query string</param>
        /// <returns>An instance of <see cref="MobileServiceTableQueryDescription"/></returns>
        public static MobileServiceTableQueryDescription Parse(string tableName, string query)
        {
            query = query ?? String.Empty;

            return Parse(tableName, query, null);
        }

        internal static MobileServiceTableQueryDescription Parse(Uri applicationUri, string tableName, string query)
        {
            query = query ?? String.Empty;
            string uriPath = null;
            Uri uri;
            bool absolute;
            if (HttpUtility.TryParseQueryUri(applicationUri, query, out uri, out absolute))
            {
                query = uri.Query.Length > 0 ? uri.Query.Substring(1) : String.Empty;
                uriPath = uri.AbsolutePath;
            }

            return Parse(tableName, query, uriPath);
        }

        private static MobileServiceTableQueryDescription Parse(string tableName, string query, string uriPath)
        {
            bool includeTotalCount = false;
            int? top = null;
            int? skip = null;
            string[] selection = null;
            QueryNode filter = null;
            IList<OrderByNode> orderings = null;

            IDictionary<string, string> parameters = HttpUtility.ParseQueryString(query);

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                string key = parameter.Key;
                string value = parameter.Value;
                if (String.IsNullOrEmpty(key))
                {
                    continue;
                }

                switch (key)
                {
                    case ODataOptions.Filter:
                        filter = ODataExpressionParser.ParseFilter(value);
                        break;
                    case ODataOptions.OrderBy:
                        orderings = ODataExpressionParser.ParseOrderBy(value);
                        break;
                    case ODataOptions.Skip:
                        skip = Int32.Parse(value);
                        break;
                    case ODataOptions.Top:
                        top = Int32.Parse(value);
                        break;
                    case ODataOptions.Select:
                        selection = value.Split(',');
                        break;
                    case ODataOptions.InlineCount:
                        includeTotalCount = "allpages".Equals(value);
                        break;
                    default:
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unrecognized query parameter '{0}'.", key), "query");
                }
            }

            var description = new MobileServiceTableQueryDescription(tableName)
            {
                IncludeTotalCount = includeTotalCount,
                Skip = skip,
                Top = top
            };
            description.UriPath = uriPath;
            if (selection != null)
            {
                ((List<string>)description.Selection).AddRange(selection);
            }
            if (orderings != null)
            {
                ((List<OrderByNode>)description.Ordering).AddRange(orderings);
            }
            description.Filter = filter;

            return description;
        }
    }
}
