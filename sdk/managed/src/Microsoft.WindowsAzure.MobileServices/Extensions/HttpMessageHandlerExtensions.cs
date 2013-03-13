// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension method to wire up <see cref="HttpMessageHandler" /> instances.
    /// </summary>
    internal static class HttpMessageHandlerExtensions
    {
        /// <summary>
        /// Transform an IEnumerable of <see cref="HttpMessageHandler"/>s into
        /// a chain of <see cref="HttpMessageHandler"/>s.
        /// </summary>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        /// <returns>A chain of <see cref="HttpMessageHandler"/>s</returns>
        public static HttpMessageHandler CreatePipeline(this IEnumerable<HttpMessageHandler> handlers)
        {
            HttpMessageHandler pipeline = handlers.LastOrDefault() ?? new HttpClientHandler();
            DelegatingHandler dHandler = pipeline as DelegatingHandler;
            if (dHandler != null)
            {
                dHandler.InnerHandler = new HttpClientHandler();
                pipeline = dHandler;
            }
            
            // Wire handlers up in reverse order
            IEnumerable<HttpMessageHandler> reversedHandlers = handlers.Reverse().Skip(1);
            foreach (HttpMessageHandler handler in reversedHandlers)
            {
                dHandler = handler as DelegatingHandler;
                if (dHandler == null)
                {
                    throw new ArgumentException(
                        string.Format(
                        Resources.HttpMessageHandlerExtensions_WrongHandlerType,
                        typeof(DelegatingHandler).Name));
                }

                dHandler.InnerHandler = pipeline;
                pipeline = dHandler;
            }

            return pipeline;
        }
    }
}
