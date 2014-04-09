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
    /// <summary>
    /// This operation only serves as a bookmark in the operation queue and is never executed or passed to sync handler
    /// </summary>
    /// <remarks>
    /// Bookmark is used by a Push operation to find out the point in queue where it has to stop replaying the operations.
    /// If you perform 3 local operations then call push and then do a 4th operation then push should only execute the 3 operations.
    /// To achieve this, push places a bookmark operation in the queue and keeps a reference to it and stops when it finds it in the queue.
    /// </remarks>
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
