// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests
{
    /// <summary>
    /// Helper class for performing Mobile App URI validations.
    /// </summary>
    public class MobileAppUriValidator
    {
        #region Constants/Fields

        /// <summary>
        /// URI for a dummy mobile app.
        /// </summary>
        public const string DummyMobileApp = "http://www.testgateway.com/testmobileapp/";

        /// <summary>
        /// URI for a dummy mobile app.
        /// </summary>
        public const string DummyGateway = "http://www.testgateway.com/";

        /// <summary>
        /// Table component in a valid table URI.
        /// </summary>
        private const string TableComponentInUri = "tables/";

        /// <summary>
        /// API component in a valid custom API URI.
        /// </summary>
        private const string ApiComponentInUri = "api/";

        /// <summary>
        /// The associated <see cref="IMobileServiceClient"/> to use for URI validations.
        /// </summary>
        private readonly IMobileServiceClient _mobileServiceClient;

        #endregion

        #region Constructor(s)
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mobileServiceClient">
        /// The associated <see cref="IMobileServiceClient"/> to use for URI validations.
        /// </param>
        public MobileAppUriValidator(IMobileServiceClient mobileServiceClient)
        {
            this._mobileServiceClient = mobileServiceClient;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The base URI of all table URIs.
        /// </summary>
        public string TableBaseUri
        {
            get { return _mobileServiceClient.MobileAppUri.AbsoluteUri + TableComponentInUri; }
        }

        /// <summary>
        /// The base URI of all custom API URIs.
        /// </summary>
        public string ApiBaseUri
        {
            get { return _mobileServiceClient.MobileAppUri.AbsoluteUri + ApiComponentInUri; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get the table URI from the specified relative URI of the table.
        /// </summary>
        public string GetTableUri(string relativeUri)
        {
            return TableBaseUri + relativeUri;
        }

        /// <summary>
        /// Get the custom API URI from the specified relative URI of the API.
        /// </summary>
        public string GetApiUriPath(string relativeUri)
        {
            var apiUri = new Uri(ApiBaseUri + relativeUri);

            return apiUri.AbsolutePath;
        }

        #endregion
    }
}
