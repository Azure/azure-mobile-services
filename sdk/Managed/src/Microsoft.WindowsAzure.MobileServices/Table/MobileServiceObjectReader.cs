// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceObjectReader
    {
        public string VersionPropertyName { get; set; }
        public string DeletedPropertyName { get; set; }
        public string UpdatedAtPropertyName { get; set; }
        public string IdPropertyName { get; set; }
        public string CreatedAtPropertyName { get; set; }

        public MobileServiceObjectReader()
        {
            this.VersionPropertyName = MobileServiceSystemColumns.Version;
            this.DeletedPropertyName = MobileServiceSystemColumns.Deleted;
            this.UpdatedAtPropertyName = MobileServiceSystemColumns.UpdatedAt;
            this.IdPropertyName = MobileServiceSystemColumns.Id;
            this.CreatedAtPropertyName = MobileServiceSystemColumns.CreatedAt;
        }

        public string GetVersion(JObject item)
        {
            return (string)item[this.VersionPropertyName];
        }

        public string GetId(JObject item)
        {
            return (string)item[IdPropertyName];
        }

        public bool IsDeleted(JObject item)
        {
            JToken deletedToken = item[DeletedPropertyName];
            bool isDeleted = deletedToken != null && deletedToken.Value<bool>();
            return isDeleted;
        }

        public DateTimeOffset? GetUpdatedAt(JObject item)
        {
            return GetDateTimeOffset(item, UpdatedAtPropertyName);
        }

        public DateTimeOffset? GetCreatedAt(JObject item)
        {
            return GetDateTimeOffset(item, CreatedAtPropertyName);
        }

        private static DateTimeOffset? GetDateTimeOffset(JObject item, string name)
        {
            JToken updatedAtToken = item[name];
            if (updatedAtToken != null)
            {
                return updatedAtToken.ToObject<DateTimeOffset?>();
            }
            return null;
        }
    }
}
