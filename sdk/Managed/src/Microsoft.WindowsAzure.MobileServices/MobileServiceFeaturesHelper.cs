// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Helper class to convert between the features enumeration and the string used in the HTTP headers.
    /// </summary>
    static class MobileServiceFeaturesHelper
    {
        static readonly Dictionary<MobileServiceFeatures, string> FeatureNames = new Dictionary<MobileServiceFeatures, string>
        {
            { MobileServiceFeatures.TypedTable, "TT" },
            { MobileServiceFeatures.UntypedTable, "TU" },
            { MobileServiceFeatures.AdditionalQueryParameters, "QS" },
            { MobileServiceFeatures.TypedApiCall, "AT" },
            { MobileServiceFeatures.JsonApiCall, "AJ" },
            { MobileServiceFeatures.GenericApiCall, "AG" },
        };

        static readonly MobileServiceFeatures[] AllFeatures = (MobileServiceFeatures[])Enum.GetValues(typeof(MobileServiceFeatures));

        /// <summary>
        /// Returns the value to be used in the HTTP header corresponding to the given features.
        /// </summary>
        /// <param name="features">The features to be sent as telemetry to the service.</param>
        /// <returns>The value of the HTTP header to be sent to the service.</returns>
        public static string FeaturesToString(MobileServiceFeatures features)
        {
            var result = new List<string>();
            foreach (MobileServiceFeatures feature in Enum.GetValues(typeof(MobileServiceFeatures)))
            {
                if ((features & feature) == feature)
                {
                    result.Add(FeatureNames[feature]);
                }
            }

            return string.Join(",", result);
        }

        /// <summary>
        /// Returns a dictionary that can be passed to the <see cref="MobileServiceHttpClient"/> with
        /// a header of the features used and the corresponding value.
        /// </summary>
        /// <param name="features">The features to be sent as telemetry to the service.</param>
        /// <returns>A dictionary containing the header with the features to be sent to the service.</returns>
        public static Dictionary<string, string> GetFeaturesHeader(MobileServiceFeatures features)
        {
            return new Dictionary<string, string>
            {
                { MobileServiceHttpClient.ZumoFeaturesHeader, FeaturesToString(features) }
            };
        }
    }
}
