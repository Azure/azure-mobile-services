// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{

    public class ComplexType
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public MissingIdType Child { get; set; }
    }
}
