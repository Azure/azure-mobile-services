// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Attribute used to associate a code to a <see cref="MobileServiceFeature"/>.
    /// </summary>
    class FeatureCodeAttribute : Attribute
    {
        /// <summary>
        /// The code used for the feature when sent in an HTTP header.
        /// </summary>
        public string Value { private set; get; }

        public FeatureCodeAttribute(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Returns the code for a specific feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        internal static string GetFeatureCode(MobileServiceFeatures feature)
        {
            FieldInfo fi = typeof(MobileServiceFeatures).GetTypeInfo().GetDeclaredField(feature.ToString());
            string featureCode = null;
            if (fi != null)
            {
                FeatureCodeAttribute fca = fi.GetCustomAttribute<FeatureCodeAttribute>();
                if (fca != null)
                {
                    featureCode = fca.Value;
                }
            }

            return featureCode ?? feature.ToString();
        }
    }
}
