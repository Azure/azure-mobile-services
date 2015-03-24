// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal class ODataExpressionVisitor : QueryNodeVisitor<QueryNode>
    {
        private static readonly Type typeofInt = typeof(int);

        public StringBuilder Expression { get; private set; }

        public ODataExpressionVisitor()
        {
            this.Expression = new StringBuilder();
        }

        public static string ToODataString(QueryNode filter)
        {
            if (filter == null)
            {
                return String.Empty;
            }
            var visitor = new ODataExpressionVisitor();
            filter.Accept(visitor);
            return visitor.Expression.ToString();
        }

        public override QueryNode Visit(BinaryOperatorNode nodeIn)
        {
            this.Expression.Append("(");

            nodeIn.LeftOperand.Accept(this);

            string odataOperator;
            switch (nodeIn.OperatorKind)
            {
                case BinaryOperatorKind.Or:
                    odataOperator = " or ";
                    break;
                case BinaryOperatorKind.And:
                    odataOperator = " and ";
                    break;
                case BinaryOperatorKind.Equal:
                    odataOperator = " eq ";
                    break;
                case BinaryOperatorKind.NotEqual:
                    odataOperator = " ne ";
                    break;
                case BinaryOperatorKind.GreaterThan:
                    odataOperator = " gt ";
                    break;
                case BinaryOperatorKind.GreaterThanOrEqual:
                    odataOperator = " ge ";
                    break;
                case BinaryOperatorKind.LessThan:
                    odataOperator = " lt ";
                    break;
                case BinaryOperatorKind.LessThanOrEqual:
                    odataOperator = " le ";
                    break;
                case BinaryOperatorKind.Add:
                    odataOperator = " add ";
                    break;
                case BinaryOperatorKind.Subtract:
                    odataOperator = " sub ";
                    break;
                case BinaryOperatorKind.Multiply:
                    odataOperator = " mul ";
                    break;
                case BinaryOperatorKind.Divide:
                    odataOperator = " div ";
                    break;
                case BinaryOperatorKind.Modulo:
                    odataOperator = " mod ";
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                                                  "'{0}' is not supported in a 'Where' Mobile Services query expression.",
                                                                  nodeIn.OperatorKind));
            }

            this.Expression.Append(odataOperator);

            nodeIn.RightOperand.Accept(this);

            this.Expression.Append(")");

            return nodeIn;
        }

        public override QueryNode Visit(FunctionCallNode nodeIn)
        {
            this.Expression.Append(nodeIn.Name);
            this.Expression.Append("(");

            string separator = null;

            foreach (QueryNode arg in nodeIn.Arguments)
            {
                this.Expression.Append(separator);
                arg.Accept(this);
                separator = ",";
            }

            this.Expression.Append(")");

            return nodeIn;
        }

        public override QueryNode Visit(MemberAccessNode nodeIn)
        {
            this.Expression.Append(nodeIn.MemberName);

            return nodeIn;
        }

        public override QueryNode Visit(UnaryOperatorNode nodeIn)
        {
            Debug.Assert(nodeIn.OperatorKind == UnaryOperatorKind.Not);

            this.Expression.Append("not(");

            nodeIn.Operand.Accept(this);

            this.Expression.Append(")");

            return nodeIn;
        }

        public override QueryNode Visit(ConstantNode nodeIn)
        {
            string value = ToODataConstant(nodeIn.Value);
            this.Expression.Append(value);

            return nodeIn;
        }

        /// <summary>
        /// Convert a value into an OData literal.
        /// </summary>
        /// <param name="value">
        /// The value to convert into an OData literal.
        /// </param>
        /// <returns>
        /// The corresponding OData literal.
        /// </returns>
        public static string ToODataConstant(object value)
        {
            if (value == null)
            {
                return "null";
            }

            // Special case a few primitive types
            RuntimeTypeHandle handle = value.GetType().TypeHandle;
            if (handle.Equals(typeof(bool).TypeHandle))
            {
                // Make sure booleans are lower case
                return ((bool)value).ToString().ToLower();
            }
            else if (handle.Equals(typeof(byte).TypeHandle))
            {
                // Format bytes as hex pairs
                return ((byte)value).ToString("X2", CultureInfo.InvariantCulture);
            }
            // unsigned int doesn't fit an int so send as long
            else if (handle.Equals(typeof(long).TypeHandle) || handle.Equals(typeof(ulong).TypeHandle)
                 || handle.Equals(typeof(uint).TypeHandle))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}L", value);
            }
            else if (handle.Equals(typeof(float).TypeHandle))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}f", value);
            }
            else if (handle.Equals(typeof(Decimal).TypeHandle))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}M", value);
            }
            else if (handle.Equals(typeofInt.TypeHandle) || handle.Equals(typeof(short).TypeHandle)
                || handle.Equals(typeof(ushort).TypeHandle) || handle.Equals(typeof(sbyte).TypeHandle))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", value);
            }
            else if (handle.Equals(typeof(double).TypeHandle))
            {
                string temp = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                if (temp.Contains("E") || temp.Contains("."))
                {
                    return temp;
                }
                return temp + ".0";
            }
            else if (handle.Equals(typeof(char).TypeHandle))
            {
                // Escape the char constant by: (1) replacing a single quote with a 
                // pair of single quotes, and (2) Uri escaping with percent encoding
                char ch = (char)value;
                string charEscaped = Uri.EscapeDataString(ch == '\'' ? "''" : ch.ToString());
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", charEscaped);
            }
            else if (handle.Equals(typeof(DateTime).TypeHandle))
            {
                // Format dates in the official OData format
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "datetime'{0}'",
                    Uri.EscapeDataString(
                        ToRoundtripDateString(((DateTime)value)))
                    );
            }
            else if (handle.Equals(typeof(DateTimeOffset).TypeHandle))
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "datetimeoffset'{0}'",
                    Uri.EscapeDataString(
                        ((DateTimeOffset)value).ToString("o")
                    ));
            }
            else if (handle.Equals(typeof(Guid).TypeHandle))
            {
                // GUIDs are in registry format without the { }s
                Guid guid = (Guid)value;
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "guid'{0}'",
                    guid.ToString().TrimStart('{').TrimEnd('}'));
            }
            else
            {
                // Escape the string constant by: (1) replacing single quotes with a 
                // pair of single quotes, and (2) Uri escaping with percent encoding
                string text = value.ToString();
                string textEscaped = Uri.EscapeDataString(text.Replace("'", "''"));
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", textEscaped);
            }
        }

        /// <summary>
        /// Convert a date to the ISO 8601 roundtrip format supported by the
        /// server.
        /// </summary>
        /// <param name="date">
        /// The date to convert.
        /// </param>
        /// <returns>
        /// The date in UTC as a string. 
        /// </returns>
        private static string ToRoundtripDateString(DateTime date)
        {
            return date.ToUniversalTime().ToString(
                "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
                CultureInfo.InvariantCulture);
        }
    }
}
