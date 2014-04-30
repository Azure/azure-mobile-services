// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    public interface ITemplateRegistration : IRegistration
    {
        /// <summary>
        /// Get templateName
        /// </summary>
        string TemplateName { get; }

        /// <summary>
        /// Gets bodyTemplate as string
        /// </summary>
        string BodyTemplate { get; }
    }
}
