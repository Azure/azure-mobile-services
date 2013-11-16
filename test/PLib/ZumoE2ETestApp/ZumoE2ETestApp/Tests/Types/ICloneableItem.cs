// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace ZumoE2ETestApp.Tests.Types
{
    public interface ICloneableItem<T> where T : ICloneableItem<T>
    {
        T Clone();
        object Id { get; set; }
    }
}
