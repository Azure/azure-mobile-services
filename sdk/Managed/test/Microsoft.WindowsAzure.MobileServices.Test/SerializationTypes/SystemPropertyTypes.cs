// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class CreatedAtType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class NotSystemPropertyCreatedAtType
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class IntegerIdNotSystemPropertyCreatedAtType
    {
        public int Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class UpdatedAtType
    {
        public string Id { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }
    }

    public class NotSystemPropertyUpdatedAtType
    {
        public string Id { get; set; }

        public DateTime _UpdatedAt { get; set; }
    }

    public class VersionType
    {
        public string Id { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class NotSystemPropertyVersionType
    {
        public string Id { get; set; }

        public string version { get; set; }
    }

    public class AllSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class MultipleSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [CreatedAt]
        public DateTime CreatedAt2 { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class NamedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class NamedDifferentCasingSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __CreatedAt { get; set; }
    }

    public class NamedAndAttributedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class DoubleNamedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class DoubleJsonPropertyNamedSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName= "__createdAt")]
        public DateTime AlsoCreatedAt { get; set; }
    }

    public class IntegerIdWithSystemPropertiesType
    {
        public int Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class LongIdWithSystemPropertiesType
    {
        public long Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class IntegerIdWithNamedSystemPropertiesType
    {
        public int Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class LongIdWithNamedSystemPropertiesType
    {
        public long Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class StringCreatedAtType
    {
        public string Id { get; set; }

        [CreatedAt]
        public String CreatedAt { get; set; }
    }

    public class StringUpdatedAtType
    {
        public string Id { get; set; }

        [UpdatedAt]
        public String UpdatedAt { get; set; }
    }
}
