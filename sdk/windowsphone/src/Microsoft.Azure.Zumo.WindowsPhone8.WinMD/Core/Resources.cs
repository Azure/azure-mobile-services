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
        public static string MobileServiceClient_ErrorMessage
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

        /// <summary>
        /// Looks up a string similar to "Cannot start a login operation because login is already in progress."
        /// </summary>
        public static string MobileServiceClient_Login_In_Progress
        {
            get { return "Cannot start a login operation because login is already in progress."; }
        }

        /// <summary>
        /// Looks up a string similar to "Authentication failed with HTTP response code {0}.".
        /// </summary>
        public static string Authentication_Failed
        {
            get { return "Authentication failed with HTTP response code {0}."; }
        }

        /// <summary>
        /// Looks up a string similar to "Authentication was cancelled by the user.".
        /// </summary>
        public static string Authentication_Canceled
        {
            get { return "Authentication was cancelled by the user."; }
        }

        /// <summary>
        /// Looks up a string similar to "Invalid format of the authentication response.".
        /// </summary>
        public static string MobileServiceClient_Login_Invalid_Response_Format
        {
            get { return "Invalid format of the authentication response."; }
        }

        /// <summary>
        /// Looks up a string similar to "Login failed: {0}".
        /// </summary>
        public static string MobileServiceClient_Login_Error_Response
        {
            get { return "Login failed: {0}"; }
        }
    }
}
