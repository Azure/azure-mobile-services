// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class ConverterType
    {
        public int id;
        [JsonConverter(typeof(TestConverter))]
        public int Number;
    }  
}
