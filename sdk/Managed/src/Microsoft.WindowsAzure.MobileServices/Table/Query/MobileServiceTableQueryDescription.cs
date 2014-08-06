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
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$filter={1}", separator, filterStr);
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

                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$orderby={1}", separator, string.Join(",", orderings));
                separator = '&';
            }

            // Skip any elements
            if (this.Skip.HasValue && this.Skip >= 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$skip={1}", separator, this.Skip);
                separator = '&';
            }

            // Take the desired number of elements
            if (this.Top.HasValue && this.Top >= 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$top={1}", separator, this.Top);
                separator = '&';
            }

            // Add the selection
            if (this.Selection.Count > 0)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$select={1}", separator, string.Join(",", this.Selection.Select(Uri.EscapeDataString)));
                separator = '&';
            }

            // Add the total count
            if (this.IncludeTotalCount)
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$inlinecount=allpages", separator);
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
            bool includeTotalCount = false;
            int? top = null;
            int? skip = null;
            string[] selection = null;
            QueryNode filter = null;
            IList<OrderByNode> orderings = null;

            char[] separator = new[] { '=' };
            var parameters = query.Split('&').Select(part => part.Split(separator, 2));

            foreach (string[] parameter in parameters)
            {
                string key = Uri.UnescapeDataString(parameter[0]);
                string value = Uri.UnescapeDataString(parameter.Length > 1 ? parameter[1] : String.Empty);
                if (String.IsNullOrEmpty(key))
                {
                    continue;
                }

                switch (key)
                {
                    case "$filter":
                        filter = ODataExpressionParser.ParseFilter(value);
                        break;
                    case "$orderby":
                        orderings = ODataExpressionParser.ParseOrderBy(value);
                        break;
                    case "$skip":
                        skip = Int32.Parse(value);
                        break;
                    case "$top":
                        top = Int32.Parse(value);
                        break;
                    case "$select":
                        selection = value.Split(',');
                        break;
                    case "$inlinecount":
                        includeTotalCount = "allpages".Equals(value);
                        break;
                    default:
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.MobileServiceTableQueryDescription_UnrecognizedQueryParameter, key), "query");
                }
            }

            var queryDescription = new MobileServiceTableQueryDescription(tableName)
            {
                IncludeTotalCount = includeTotalCount,
                Skip = skip,
                Top = top
            };
            if (selection != null)
            {
                ((List<string>)queryDescription.Selection).AddRange(selection);
            }
            if (orderings != null)
            {
                ((List<OrderByNode>)queryDescription.Ordering).AddRange(orderings);
            }
            queryDescription.Filter = filter;

            return queryDescription;
        }
    }
}
