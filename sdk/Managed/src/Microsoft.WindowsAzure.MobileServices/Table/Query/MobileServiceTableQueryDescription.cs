// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents the structural elements of a Mobile Services query over the
    /// subset of OData it uses.
    /// </summary>
    /// <remarks>
    /// Our LINQ OData Provider will effectively compile expression trees into
    /// MobileServiceTableQueryDescription instances which can be passed to a
    /// MobileServiceCollectionView and evaluated on the server.  We don't
    /// compile the expression all the way down to a single Uri fragment
    /// because we'll need to re-evaluate the query with different Skip/Top
    /// values for paging and data virtualization.
    /// </remarks>
    internal class MobileServiceTableQueryDescription
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MobileServiceTableQueryDescription class.
        /// </summary>
        public MobileServiceTableQueryDescription(string tableName, bool includeTotalCount)
        {
            Debug.Assert(tableName != null, "tableName cannot be null");

            this.TableName = tableName;
            this.IncludeTotalCount = includeTotalCount;

            this.Selection = new List<string>();
            this.Projections = new List<Delegate>();
            this.Ordering = new List<KeyValuePair<string, bool>>();
        }

        /// <summary>
        /// Gets or sets the name of the table being queried.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets of sets the query's filter expression.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Gets a list of fields that should be selected from the items in
        /// the table.
        /// </summary>
        public List<string> Selection { get; private set; }

        /// <summary>
        /// Gets a list of (field, direction) pairs that specify the ordering
        /// constraints imposed on the query.  The direction element is true
        /// if the field should be ordered in ascending order and false if
        /// descending.
        /// </summary>
        public List<KeyValuePair<string, bool>> Ordering { get; private set; }

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
        public List<Delegate> Projections { get; private set; }

        /// <summary>
        /// Gets or sets the type of the argument to the projection (i.e., the
        /// type that should be deserialized).
        /// </summary>
        public Type ProjectionArgumentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the query should request
        /// the total count for all the records that would have been returned
        /// if the server didn't impose a data cap.
        /// </summary>
        public bool IncludeTotalCount { get; private set; }

        /// <summary>
        /// Convert the query structure into the standard OData URI protocol
        /// for queries.
        /// </summary>
        /// <returns>
        /// URI fragment representing the query.
        /// </returns>
        public string ToQueryString()
        {
            char? separator = null;
            StringBuilder text = new StringBuilder();
            
            // Add the filter
            if (!string.IsNullOrEmpty(this.Filter))
            {
                text.AppendFormat(CultureInfo.InvariantCulture, "{0}$filter={1}", separator, this.Filter);
                separator = '&';
            }

            // Add the ordering
            if (this.Ordering.Count > 0)
            {
                IEnumerable<string> orderings = this.Ordering.Select(
                    f => f.Value ? f.Key : f.Key + " desc");
                text.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    "{0}$orderby={1}",
                    separator,
                    string.Join(",", orderings));
                separator = '&';
            }

            // Skip any elements
            if (this.Skip.HasValue && this.Skip >= 0)
            {
                text.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}$skip={1}",
                    separator,
                    this.Skip);
                separator = '&';
            }

            // Take the desired number of elements
            if (this.Top.HasValue && this.Top >= 0)
            {
                text.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}$top={1}",
                    separator,
                    this.Top);
                separator = '&';
            }

            // Add the selection
            if (this.Selection.Count > 0)
            {
                text.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}$select={1}",
                    separator,
                    string.Join(",", this.Selection));
                separator = '&';
            }

            // Add the total count
            if (this.IncludeTotalCount)
            {
                text.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}$inlinecount=allpages",
                    separator);
                separator = '&';
            }

            return text.ToString();
        }
    }
}
