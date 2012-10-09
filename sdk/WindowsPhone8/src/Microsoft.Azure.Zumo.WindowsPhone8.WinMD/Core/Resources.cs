// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides access to localizable resources.
    /// </summary>
    internal static partial class Resources
    {
        // TODO: Pull these from resources once we figure out how to get
        // .resw's properly included in the resources.pri for a .winmd

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when an
        /// argument is empty.
        /// </summary>
        public static string EmptyArgumentExceptionMessage
        {
            get { return "{0} cannot be empty."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when a JSON
        /// object is passed for serialization without an ID member.
        /// </summary>
        public static string IdNotFoundExceptionMessage
        {
            get { return "Expected {0} member not found."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when an object
        /// is passed for insertion and has its ID already set.
        /// </summary>
        public static string CannotInsertWithExistingIdMessage
        {
            get { return "Cannot insert if the {0} member is already set."; }
        }

        /// <summary>
        /// Gets a format string for throwing InvalidOperationExceptions when
        /// a web request results in a failure.
        /// </summary>
        public static string MobileServiceClient_ThrowInvalidResponse_ErrorMessage
        {
            get { return "{2}  ({0} {1} - Details: {3})"; }
        }

        /// <summary>
        /// Gets a format string for throwing InvalidOperationExceptions when
        /// a web request results in a failure.
        /// </summary>
        public static string MobileServiceClient_ThrowConnectionFailure_ErrorMessage
        {
            get { return "The request could not be completed.  ({0})"; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentOutOfRangeExceptions when
        /// asked to serialize a long/ulong requiring more precision than a
        /// double.
        /// </summary>
        public static string JsonExtensions_TrySetValue_CannotRoundtripNumericValue
        {
            get { return "The value {0} for member {1} is outside the valid range for numeric columns."; }
        }
    }
}
