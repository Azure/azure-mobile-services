// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class LinkHeaderValue
    {
        static Regex pattern = new Regex(@"^(?<uri>.*?);\s*rel\s*=\s*(?<rel>\w+)\s*$");
        public Uri Uri { get; private set; }
        public string Relation { get; private set; }

        public LinkHeaderValue(string uri, string rel)
        {
            Uri value;
            Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out value);
            this.Uri = value;
            this.Relation = rel;
        }

        public static LinkHeaderValue Parse(string value)
        {
            string uri = null, rel = null;

            if (!String.IsNullOrEmpty(value))
            {
                Match result = pattern.Match(value ?? String.Empty);
                uri = result.Groups["uri"].Value;
                rel = result.Groups["rel"].Value;
            }

            return new LinkHeaderValue(uri, rel);
        }
    }
}