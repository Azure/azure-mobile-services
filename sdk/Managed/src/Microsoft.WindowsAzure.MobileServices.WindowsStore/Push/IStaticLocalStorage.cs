// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using Windows.Foundation.Collections;

    /// <summary>
    /// interface for local storage, for test purpose
    /// </summary>
    internal interface IStaticLocalStorage
    {
        IPropertySet CreateContainer(string storageName);
    }
}
