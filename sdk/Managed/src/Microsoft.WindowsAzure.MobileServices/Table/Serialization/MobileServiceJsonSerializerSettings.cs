// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// JSON serializer settings to use with a <see cref="MobileServiceClient"/>.
    /// </summary>
    public class MobileServiceJsonSerializerSettings : JsonSerializerSettings
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceJsonSerializerSettings
        /// class.
        /// </summary>
        public MobileServiceJsonSerializerSettings()
        {
            this.NullValueHandling = NullValueHandling.Include;
            this.ContractResolver = new MobileServiceContractResolver();

            this.Converters.Add(new MobileServiceIsoDateTimeConverter());
            this.Converters.Add(new MobileServicePrecisionCheckConverter());
            this.Converters.Add(new StringEnumConverter());
        }

        /// <summary>
        /// Indicates if the property names should be camel-cased when serialized
        /// out into JSON.
        /// </summary>
        public bool CamelCasePropertyNames
        {
            get
            {
                return this.ContractResolver.CamelCasePropertyNames;
            }

            set
            {
                this.ContractResolver.CamelCasePropertyNames = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MobileServiceContractResolver"/> instance.  
        /// </summary>
        public new MobileServiceContractResolver ContractResolver
        {
            get
            {
                // Because we are hiding the base member (which is of type 
                // IContractResolver) it is possible that the base has been 
                // set to an instance of IContractResolver that is not a 
                // MobileServiceContractResolver. Therefore, we must check for 
                // this condition and throw an exception as needed.
                MobileServiceContractResolver contractResolver = 
                    base.ContractResolver as MobileServiceContractResolver;
                if (contractResolver == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceJsonSerializerSettings_NullOrInvalidContractResolver,
                            this.GetType().FullName,
                            typeof(MobileServiceContractResolver).FullName));
                }

                return contractResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceJsonSerializerSettings_NullOrInvalidContractResolver,
                            this.GetType().FullName,
                            typeof(MobileServiceContractResolver).FullName));
                }

                base.ContractResolver = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="JsonSerializer"/> with the equivalent settings
        /// as this <see cref="MobileServiceJsonSerializerSettings"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/> with the equivalent settings
        /// as this <see cref="MobileServiceJsonSerializerSettings"/>.
        /// </returns>
        internal JsonSerializer GetSerializerFromSettings()
        {
            JsonSerializer serializer = new JsonSerializer();

            // We do not set the JsonSerializer.Binder because it breaks our .Net4.5 
            // release with Json.NET

            // JsonSerializer will throw id ReferenceResolver is set to null
            if (this.ReferenceResolver != null)
            {
                serializer.ReferenceResolver = this.ReferenceResolver;
            }

            // Add each of the converters
            foreach (var converter in this.Converters)
            {
                serializer.Converters.Add(converter);
            }

            // Set all of the other settings on the serializer
            serializer.CheckAdditionalContent = this.CheckAdditionalContent;
            serializer.ConstructorHandling = this.ConstructorHandling;
            serializer.Context = this.Context;
            serializer.ContractResolver = this.ContractResolver;
            serializer.Culture = this.Culture;
            serializer.DateFormatHandling = this.DateFormatHandling;
            serializer.DateParseHandling = this.DateParseHandling;
            serializer.DateTimeZoneHandling = this.DateTimeZoneHandling;
            serializer.DefaultValueHandling = this.DefaultValueHandling;
            serializer.Error += this.Error;
            serializer.Formatting = this.Formatting;
            serializer.MaxDepth = this.MaxDepth;
            serializer.MissingMemberHandling = this.MissingMemberHandling;
            serializer.NullValueHandling = this.NullValueHandling;
            serializer.ObjectCreationHandling = this.ObjectCreationHandling;
            serializer.PreserveReferencesHandling = this.PreserveReferencesHandling;
            serializer.ReferenceLoopHandling = this.ReferenceLoopHandling;
            serializer.TraceWriter = this.TraceWriter;
            serializer.TypeNameHandling = this.TypeNameHandling;

            return serializer;
        }
    }
}
