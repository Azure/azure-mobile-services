// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Query
{
    [TestClass]
    public class MobileServiceTableQueryDescriptionTests
    {
        [TestMethod]
        public void Parse_DoesNotThrow_OnIncompleteQuery()
        {
            MobileServiceTableQueryDescription desc = MobileServiceTableQueryDescription.Parse("someTable", "$select&");

        }
    }
}
