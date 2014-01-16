// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    // This operation only serves as a bookmark in the operation queue and is never executed or passed to sync handler
    internal class BookmarkOperation: MobileServiceTableOperation
    {
        public BookmarkOperation(): base(null, null)
        {
        }

        public override MobileServiceTableOperationKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        protected override Task<JToken> OnExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            throw new NotImplementedException();
        }

        public override void Validate(MobileServiceTableOperation newOperation)
        {
            throw new NotImplementedException();
        }

        public override void Collapse(MobileServiceTableOperation newOperation)
        {
            throw new NotImplementedException();
        }
    }
}
