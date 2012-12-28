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
        /// Gets a format string for throwing InvalidOperationExceptions when
        /// a user provides a [DataMemberJsonConverter(ConverterType =
        /// typeof(Foo))] that does not implement IDataMemberJsonConverter.
        /// </summary>
        public static string SerializableType_GetConverter_DoesNotImplementConverter
        {
            get { return "ConverterType '{0}' does not implement the IDataMemberJsonConverter interface."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when a user
        /// tries to serialize a type without an "id" property.
        /// </summary>
        public static string SerializableType_Ctor_MemberNotFound
        {
            get { return "No '{0}' member found on type '{1}'."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when a user
        /// tries to deserialize something other than an object.
        /// </summary>
        public static string MobileServiceTableSerializer_Deserialize_NeedObject
        {
            get { return "Expected a JSON object to deserialize, not '{0}'."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when a user
        /// tries to deserialize a value that we can't process.
        /// </summary>
        public static string MobileServiceTableSerializer_Deserialize_CannotDeserializeValue
        {
            get { return "Cannot deserialize value {0} into '{1}.{2}'."; }
        }

        /// <summary>
        /// Gets a format string for throwing SerializationExceptions when a
        /// user tries to deserialize an object missing required members.
        /// </summary>
        public static string MobileServiceTableSerializer_Deserialize_MissingRequired
        {
            get { return "Cannot deserialize type '{0}' because the JSON object did not have required member(s) '{1}'."; }
        }

        /// <summary>
        /// Gets a format string for throwing ArgumentExceptions when a user
        /// tries to serialize an unknown type.
        /// </summary>
        public static string MobileServiceTableSerializer_Serialize_UnknownType
        {
            get { return "Cannot serialize member '{0}' of type '{1}' declared on type '{2}'."; }
        }

        /// <summary>
        /// Gets a format string for throwing InvalidOperationExceptions when
        /// we can't get a single object from a response.
        /// </summary>
        public static string MobileServiceTables_GetSingleValueAsync_NotSingleObject
        {
            get { return "Could not get object from response {0}."; }
        }

        /// <summary>
        /// Gets a format string for throwing InvalidOperationExceptions when
        /// we can't get an array from a response.
        /// </summary>
        public static string MobileServiceTables_GetResponseSequence_ExpectedArray
        {
            get { return "Could not get an array from response {0}."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported method expression.
        /// </summary>
        public static string MobileServiceTableQueryTranslator_ThrowForUnsupportedException_Unsupported
        {
            get { return "Expression '{1}' is not a supported '{0}' Mobile Services query expression."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported value for OrderBy.
        /// </summary>
        public static string MobileServiceTableQueryTranslator_GetOrdering_Unsupported
        {
            get { return "'{0}' Mobile Services query expressions must consist of members only, not '{1}'."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported value for Skip/Take.
        /// </summary>
        public static string MobileServiceTableQueryTranslator_GetCountArgument_Unsupported
        {
            get { return "'{0}' Mobile Services query expressions must consist of a single integer, not '{1}'."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported operator.
        /// </summary>
        public static string FilterBuildingExpressionVisitor_VisitOperator_Unsupported
        {
            get { return "The operator '{0}' is not supported in the 'Where' Mobile Services query expression '{1}'."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported member.
        /// </summary>
        public static string FilterBuildingExpressionVisitor_VisitMember_Unsupported
        {
            get { return "The member '{0}' is not supported in the 'Where' Mobile Services query expression '{1}'."; }
        }

        /// <summary>
        /// Get a format string for throwing a NotSupportedException when we
        /// see an unsupported expression.
        /// </summary>
        public static string FilterBuildingExpressionVisitor_Visit_Unsupported
        {
            get { return "'{0}' is not supported in a 'Where' Mobile Services query expression."; }
        }

        /// <summary>
        /// Get a format string for throwing an InvalidOperationException when we
        /// find a SerializableType with two members with the same name.
        /// </summary>
        public static string SerializableType_DuplicateKey
        {
            get { return "Two or more members of type '{0}' are mapped to the same name '{1}'. Verify that your DataMember annotations are correct."; }
        }
    }
}
