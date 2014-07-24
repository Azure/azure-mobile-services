// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The list of zumo features exposed in the http headers of requests for tracking usages.
    /// </summary>
    static class MobileServiceFeatures
    {
        /// <summary>
        /// Feature header value for requests going through typed tables.
        /// </summary>
        internal const string TypedTable = "TT";

        /// <summary>
        /// Feature header value for requests going through untyped (JSON) tables.
        /// </summary>
        internal const string UntypedTable = "TU";

        /// <summary>
        /// Feature header value for table / API requests which include additional query string parameters.
        /// </summary>
        internal const string AdditionalQueryParameters = "QS";

        /// <summary>
        /// Feature header value for API calls using typed (generic) overloads.
        /// </summary>
        internal const string TypedApiCall = "AT";

        /// <summary>
        /// Feature header value for API calls using JSON overloads.
        /// </summary>
        internal const string JsonApiCall = "AJ";

        /// <summary>
        /// Feature header value for API calls using the generic (HTTP) overload.
        /// </summary>
        internal const string GenericApiCall = "AG";
    }
}
