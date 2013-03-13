﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Create an OData filter expression by walking an expression tree.
    /// </summary>
    internal class FilterBuildingExpressionVisitor
    {
        // Instance OData filter method names
        private const string toLowerFilterMethod = "tolower";
        private const string toUpperFilterMethod = "toupper";
        private const string trimFilterMethod = "trim";
        private const string startsWithFilterMethod = "startswith";
        private const string endsWithFilterMethod = "endswith";
        private const string indexOfFilterMethod = "indexof";
        private const string subStringOfFilterMethod = "substringof";
        private const string replaceFilterMethod = "replace";
        private const string substringFilterMethod = "substring";

        // Static OData filter method names
        private const string floorFilterMethod = "floor";
        private const string ceilingFilterMethod = "ceiling";
        private const string roundFilterMethod = "round";
        private const string concatFilterMethod = "concat";

        // Static dictionaries to look up OData method names from .NET memberInfo and types
        private static Dictionary<MethodInfo, string> instanceMethods;
        private static Dictionary<MethodInfo, string> staticMethods;
        private static Dictionary<MemberInfo, string> instanceProperties;
        private static Dictionary<Type, Type[]> implicitConversions;

        private static readonly MethodInfo toStringMethod = FindInstanceMethod(typeof(object), "ToString");
        
        /// <summary>
        /// Defines the instance methods that are translated into OData filter
        /// expressions.
        /// </summary>
        private static Dictionary<MethodInfo, string> InstanceMethods
        {
            get
            {
                if (instanceMethods == null)
                {
                    instanceMethods =
                        new Dictionary<MethodInfo, string>
                        {
                            { FindInstanceMethod(typeof(string), "ToLower"), toLowerFilterMethod },
                            { FindInstanceMethod(typeof(string), "ToUpper"), toUpperFilterMethod },
                            { FindInstanceMethod(typeof(string), "Trim"), trimFilterMethod },
                            { FindInstanceMethod(typeof(string), "StartsWith", typeof(string)), startsWithFilterMethod },
                            { FindInstanceMethod(typeof(string), "EndsWith", typeof(string)), endsWithFilterMethod },
                            { FindInstanceMethod(typeof(string), "IndexOf", typeof(string)), indexOfFilterMethod },
                            { FindInstanceMethod(typeof(string), "IndexOf", typeof(char)), indexOfFilterMethod },
                            { FindInstanceMethod(typeof(string), "Contains", typeof(string)), subStringOfFilterMethod },
                            { FindInstanceMethod(typeof(string), "Replace", typeof(string), typeof(string)), replaceFilterMethod },
                            { FindInstanceMethod(typeof(string), "Replace", typeof(char), typeof(char)), replaceFilterMethod },
                            { FindInstanceMethod(typeof(string), "Substring", typeof(int)), substringFilterMethod },
                            { FindInstanceMethod(typeof(string), "Substring", typeof(int), typeof(int)), substringFilterMethod },
                        };
                }

                return instanceMethods;
            }
        }

        /// <summary>
        /// Defines the static methods that are translated into OData filter
        /// expressions.
        /// </summary>
        private static Dictionary<MethodInfo, string> StaticMethods
        {
            get
            {
                if (staticMethods == null)
                {
                    staticMethods =
                        new Dictionary<MethodInfo, string>
                        {
                            { FindStaticMethod(typeof(Math), "Floor", typeof(double)), floorFilterMethod },                 
                            { FindStaticMethod(typeof(Math), "Ceiling", typeof(double)), ceilingFilterMethod },  
                            { FindStaticMethod(typeof(Math), "Round", typeof(double)), roundFilterMethod },
                            { FindStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)), concatFilterMethod }
                        };

                    // Windows 8 supports decimal.Floor(), decimal.Ceiling() and decimal.Round(), but Windows Phone does not, 
                    // so we have to ensure these method infos exist before we add them.
                    MethodInfo possibleFloorMethod = FindStaticMethod(typeof(Decimal), "Floor", typeof(decimal));
                    if (possibleFloorMethod != null)
                    {
                        staticMethods.Add(possibleFloorMethod, floorFilterMethod);
                    }
                    
                    MethodInfo possibleCeilingMethod = FindStaticMethod(typeof(Decimal), "Ceiling", typeof(decimal));
                    if (possibleCeilingMethod != null)
                    {
                        staticMethods.Add(possibleCeilingMethod, ceilingFilterMethod);
                    }

                    MethodInfo possibleRoundMethod = FindStaticMethod(typeof(Decimal), "Round", typeof(decimal));
                    if (possibleRoundMethod != null)
                    {
                        staticMethods.Add(possibleRoundMethod, roundFilterMethod);
                    }

                    // Windows Phone 7.5 does not support Math.Floor(), Math.Round() and Math.Ceiling() for decimals 
                    MethodInfo possibleCeilingMethodMath = FindStaticMethod(typeof(Math), "Ceiling", typeof(decimal));
                    if (possibleCeilingMethodMath != null)
                    {
                        staticMethods.Add(possibleCeilingMethodMath, ceilingFilterMethod);
                    }

                    MethodInfo possibleFloorMethodMath = FindStaticMethod(typeof(Math), "Floor", typeof(decimal));
                    if (possibleFloorMethodMath != null)
                    {
                        staticMethods.Add(possibleFloorMethodMath, floorFilterMethod);
                    }

                    MethodInfo possibleRoundMethodMath = FindStaticMethod(typeof(Math), "Round", typeof(decimal));
                    if (possibleRoundMethodMath != null)
                    {
                        staticMethods.Add(possibleRoundMethodMath, roundFilterMethod);
                    }
                }

                return staticMethods;
            }
        }

        /// <summary>
        /// Defines the instance properties that are translated into OData
        /// filter expressions.
        /// </summary>
        private static Dictionary<MemberInfo, string> InstanceProperties
        {
            get
            {
                if (instanceProperties == null)
                {
                    instanceProperties =
                        new Dictionary<MemberInfo, string>
                        {
                            { FindInstanceProperty(typeof(string), "Length"), "length" },
                            { FindInstanceProperty(typeof(DateTime), "Day"), "day" },
                            { FindInstanceProperty(typeof(DateTime), "Month"), "month" },
                            { FindInstanceProperty(typeof(DateTime), "Year"), "year" },
                            { FindInstanceProperty(typeof(DateTime), "Hour"), "hour" },
                            { FindInstanceProperty(typeof(DateTime), "Minute"), "minute" },
                            { FindInstanceProperty(typeof(DateTime), "Second"), "second" },
                        };
                }

                return instanceProperties;
            }
        }

        /// <summary>
        /// Defines a table of implicit conversions for numeric types derived
        /// from http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
        /// </summary>
        private static Dictionary<Type, Type[]> ImplicitConversions
        {
            get
            {
                if (implicitConversions == null)
                {
                    implicitConversions =
                        new Dictionary<Type, Type[]>
                        {
                            { typeof(sbyte),  new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(byte),   new[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(short),  new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(ushort), new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(int),    new[] { typeof(long), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(uint),   new[] { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(long),   new[] { typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(char),   new[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
                            { typeof(float),  new[] { typeof(double) } },
                            { typeof(ulong),  new[] { typeof(float), typeof(double), typeof(decimal) } }
                        };
                }

                return implicitConversions;
            }
        }

        /// <summary>
        /// The OData query generated by this visitor
        /// </summary>
        private StringBuilder filter = new StringBuilder();

        /// <summary>
        /// The contract resolver used to determine property names from
        /// members used within expressions.
        /// </summary>
        private MobileServiceContractResolver contractResolver;

        /// <summary>
        /// Initializes a new instance of the FilterBuildingExpressionVisitor
        /// class.
        /// </summary>
        /// <param name="contractResolver">
        /// The contract resolver to use to determine property 
        /// names from members used within expressions.
        /// </param>
        private FilterBuildingExpressionVisitor(MobileServiceContractResolver contractResolver)
            : base()
        {
            Debug.Assert(contractResolver != null);

            this.contractResolver = contractResolver;
        }

        /// <summary>
        /// Translate an expression tree into a compiled OData query.
        /// </summary>
        /// <param name="expression">
        /// The expression tree.
        /// </param>
        /// <param name="contractResolver">
        /// The contract resolver used to determine property names from
        /// members used within expressions.
        /// </param>
        /// <returns>
        /// An OData query.
        /// </returns>
        public static string Create(Expression expression, MobileServiceContractResolver contractResolver)
        {
            Debug.Assert(expression != null);
            Debug.Assert(contractResolver != null);
                    
            // Walk the expression tree and build the filter.
            FilterBuildingExpressionVisitor visitor = new FilterBuildingExpressionVisitor(contractResolver);
            visitor.Visit(expression);
            return visitor.filter.ToString();
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

            // Special case a few primtive types
            RuntimeTypeHandle handle = value.GetType().TypeHandle;
            if (handle.Equals(typeof(bool).TypeHandle))
            {
                // Make sure booleans are lower case
                return ((bool)value).ToString().ToLower();
            }
            else if (handle.Equals(typeof(Byte).TypeHandle))
            {
                // Format bytes as hex pairs
                return ((byte)value).ToString("X2", CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(long).TypeHandle))
            {
                return value.ToString() + "L";
            }
            else if (handle.Equals(typeof(float).TypeHandle))
            {
                return value.ToString() + "f";
            }
            else if (handle.Equals(typeof(Decimal).TypeHandle))
            {
                return value.ToString() + "M";
            }
            else if (handle.Equals(typeof(string).TypeHandle))
            {
                // Escape the string constant by: (1) replacing single quotes with a 
                // pair of single quotes, and (2) Uri escaping with percent encoding
                string text = value as string ?? string.Empty;
                string textEscaped = Uri.EscapeDataString(text.Replace("'", "''"));
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", textEscaped);
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
                    ToRoundtripDateString(((DateTime)value)));
            }
            else if (handle.Equals(typeof(DateTimeOffset).TypeHandle))
            {
                // Format dates in the official OData format (note: the server
                // doesn't recgonize datetimeoffset'...', so we'll just convert
                // to a UTC date.
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "datetime'{0}'",
                    ToRoundtripDateString(((DateTimeOffset)value).DateTime));
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

            // We'll just ToString everything else
            return value.ToString();
        }

        /// <summary>
        /// Get the table member name referenced by an expression or return null.
        /// </summary>
        /// <param name="expression">
        /// The expression to check.
        /// </param>
        /// <param name="contractResolver">
        /// The contract resolver to use to determine property names from members used 
        /// within expressions.
        /// </param>
        /// <returns>
        /// The table member name or null.
        /// </returns>
        public static string GetTableMemberName(Expression expression,
                                                  MobileServiceContractResolver contractResolver)
        {
            Debug.Assert(expression != null);
            Debug.Assert(contractResolver != null);

            // Only parameter references are valid in a query (any other
            // references should have been partially evaluated away)
            MemberExpression member = expression as MemberExpression;
            if (member != null &&
                member.Expression != null &&
                member.Expression.NodeType == ExpressionType.Parameter &&
                member.Member != null)
            {
                // Lookup the Mobile Services name of the member and use that
                JsonProperty property = contractResolver.ResolveProperty(member.Member);
                return property.PropertyName;
            }

            // Otherwise return null
            return null;
        }

        /// <summary>
        /// Visit a node of the expression.
        /// </summary>
        /// <param name="node">
        /// The node to visit.
        /// </param>
        /// <returns>
        /// The node of the expression.
        /// </returns>
        private Expression Visit(Expression node)
        {
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        return this.VisitUnary((UnaryExpression)node);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.Power:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                        return this.VisitBinary((BinaryExpression)node);
                    case ExpressionType.Constant:
                        return this.VisitConstant((ConstantExpression)node);
                    case ExpressionType.MemberAccess:
                        return this.VisitMemberAccess((MemberExpression)node);
                    case ExpressionType.Call:
                        return this.VisitMethodCall((MethodCallExpression)node);
                    default:
                        throw new NotSupportedException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.FilterBuildingExpressionVisitor_UnsupportedInWhereExpression,
                                node != null ? node.ToString() : null));
                }
            }
            
            return node;
        }

        /// <summary>
        /// Process the not operator.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitUnary(UnaryExpression expression)
        {
            if (expression != null)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Not:
                        this.filter.Append("not(");
                        this.Visit(expression.Operand);
                        this.filter.Append(")");
                        break;
                    case ExpressionType.Quote:
                        this.Visit(StripUnaryOperator(expression));
                        break;
                    case ExpressionType.Convert:
                        // Ignore conversion requests if the conversion will
                        // happen implicitly on the server anyway
                        if (IsConversionImplicit(expression, expression.Operand.Type, expression.Type))
                        {
                            this.Visit(StripUnaryOperator(expression));
                            break;
                        }
                        // Otherwise the conversion isn't supported
                        goto default;
                    default:
                        throw new NotSupportedException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.FilterBuildingExpressionVisitor_OperatorUnsupported,
                                expression.NodeType,
                                expression.ToString()));
                }
            }
            return expression;
        }

        /// <summary>
        /// Check whether a conversion from one type to another will be made
        /// implicitly by the server.  This is only considered valid for 
        /// member references.
        /// </summary>
        /// <param name="expression">
        /// The conversion expression.
        /// </param>
        /// <param name="from">
        /// The type to convert from.
        /// </param>
        /// <param name="to">
        /// The type to convert to.
        /// </param>
        /// <returns>
        /// <c>true</c> if there is an implicit conversion, <c>false</c> otherwise.
        /// </returns>
        private bool IsConversionImplicit(UnaryExpression expression, Type from, Type to)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            Debug.Assert(from != null, "from cannot be null!");
            Debug.Assert(to != null, "to cannot be null!");

            // We're only interested in conversions on table members
            if (GetTableMemberName(expression.Operand, this.contractResolver) != null)
            {
                Type toType = to.UnwrapNullable();
                Type fromType = from.UnwrapNullable();

                // if the types are the same just return true
                if (toType == fromType)
                {
                    return true;
                }
                // check for enum
                if (fromType.IsEnum)
                {
                    return true;
                }

                // Check to see if the types can be implicitly converted            
                Type[] conversions = null;
                if (ImplicitConversions.TryGetValue(fromType, out conversions))
                {
                    return Array.IndexOf(conversions, toType) >= 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Process binary comparison operators.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitBinary(BinaryExpression expression)
        {
            if (expression != null)
            {
                // special case for enums, because we send them as strings
                UnaryExpression enumExpression = null;
                ConstantExpression constantExpression = null;
                if (this.CheckEnumExpression(expression, out enumExpression, out constantExpression))
                {
                    this.Visit(this.RewriteEnumExpression(enumExpression, (ConstantExpression)expression.Right, expression.NodeType));            
                }
                // special case concat as it's OData function isn't infix
                else if (expression.NodeType == ExpressionType.Add &&
                    expression.Left.Type == typeof(string) &&
                    expression.Right.Type == typeof(string))
                {
                    //rewrite addition into a call to concat, instead of duplicating generation code.
                    this.Visit(this.RewriteStringAddition(expression));
                }
                else
                {
                    this.filter.Append("(");
                    this.Visit(expression.Left);
                    switch (expression.NodeType)
                    {
                        case ExpressionType.AndAlso:
                            this.filter.Append(" and ");
                            break;
                        case ExpressionType.OrElse:
                            this.filter.Append(" or ");
                            break;
                        case ExpressionType.Equal:
                            this.filter.Append(" eq ");
                            break;
                        case ExpressionType.NotEqual:
                            this.filter.Append(" ne ");
                            break;
                        case ExpressionType.LessThan:
                            this.filter.Append(" lt ");
                            break;
                        case ExpressionType.LessThanOrEqual:
                            this.filter.Append(" le ");
                            break;
                        case ExpressionType.GreaterThan:
                            this.filter.Append(" gt ");
                            break;
                        case ExpressionType.GreaterThanOrEqual:
                            this.filter.Append(" ge ");
                            break;
                        case ExpressionType.Add:
                            this.filter.Append(" add ");
                            break;
                        case ExpressionType.Subtract:
                            this.filter.Append(" sub ");
                            break;
                        case ExpressionType.Multiply:
                            this.filter.Append(" mul ");
                            break;
                        case ExpressionType.Divide:
                            this.filter.Append(" div ");
                            break;
                        case ExpressionType.Modulo:
                            this.filter.Append(" mod ");
                            break;
                        default:
                            throw new NotSupportedException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    Resources.FilterBuildingExpressionVisitor_OperatorUnsupported,
                                    expression.NodeType,
                                    expression.ToString()));
                    }
                    this.Visit(expression.Right);
                    this.filter.Append(")");
                }
            }

            return expression;
        }
        
        /// <summary>
        /// Checks if the binary expression is an enum expression.
        /// </summary>
        /// <param name="expression">The binary expression to check.</param>
        /// <param name="unaryExp">The expression which is the enum.</param>
        /// <param name="constExp">The expression containing the enum value.</param>
        /// <returns>True if an enum expression is found, otherwise false.</returns>
        private bool CheckEnumExpression(BinaryExpression expression, out UnaryExpression unaryExp, out ConstantExpression constExp)
        {
            // we handle two cases because the enum part can be on both sides
            constExp = null;
            // case 1: enum on left side
            unaryExp = expression.Left as UnaryExpression;
            if (unaryExp != null && unaryExp.NodeType == ExpressionType.Convert && unaryExp.Operand.Type.IsEnum && expression.Right is ConstantExpression)
            {
                constExp = (ConstantExpression)expression.Right;
                return true;
            }
            // case 2: enum on right side
            unaryExp = expression.Right as UnaryExpression;
            if (unaryExp != null && unaryExp.NodeType == ExpressionType.Convert && unaryExp.Operand.Type.IsEnum && expression.Left is ConstantExpression)
            {
                constExp = (ConstantExpression)expression.Left;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rewrite a BinaryExpression containing enums into a binary expression with strings.
        /// </summary>
        /// <param name="enumExpression">the unaryexpression containing the enum parameter.</param>
        /// <param name="constant">The constantexpression containing the constant value representing an enum value.</param>
        /// <param name="type">the binaryexpression type.</param>
        /// <returns>The rewritten expression.</returns>
        private Expression RewriteEnumExpression(UnaryExpression enumExpression, ConstantExpression constant, ExpressionType type)
        {
            //get type of enum
            Type enumType = enumExpression.Operand.Type;
            
            // note: the constant part of the expression will always be a valid type for Enum.ToObject, 
            // because otherwise the compiler would have complained in the first place.
            // transform int into string
            string enumString = Enum.ToObject(enumType, constant.Value).ToString();

            // call ToString to match the types
            Expression call = Expression.Call(enumExpression, toStringMethod);

            //create new binary expression 
            return Expression.MakeBinary(type, call, Expression.Constant(enumString));
        }

        /// <summary>
        /// Rewrite a Addition expression with strings into a call to string.Concat.
        /// </summary>
        /// <param name="expression">The binaryexpression representing string addition.</param>
        /// <returns>The rewritten expression.</returns>
        private Expression RewriteStringAddition(BinaryExpression expression)
        {
            return Expression.Call(
                        FindStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)),
                        expression.Left,
                        expression.Right);
        }

        /// <summary>
        /// Process constant values.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitConstant(ConstantExpression expression)
        {
            if (expression != null)
            {
                string value = ToODataConstant(expression.Value);
                this.filter.Append(value);
            }
            return expression;
        }

        /// <summary>
        /// Process member references.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitMemberAccess(MemberExpression expression)
        {
            // Lookup the Mobile Services name of the member and use that
            string memberName = GetTableMemberName(expression, this.contractResolver);
            if (memberName != null)
            {
                this.filter.Append(memberName);
                return expression;
            }
            
            // Check if this member is actually a function that looks like a
            // property (like string.Length, etc.)
            string methodName = null;
            if (InstanceProperties.TryGetValue(expression.Member, out methodName))
            {
                this.filter.Append(methodName);
                this.filter.Append("(");
                this.Visit(expression.Expression);
                this.filter.Append(")");
                return expression;
            }

            // Otherwise we can't process the member.
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.FilterBuildingExpressionVisitor_MemberUnsupported,
                    expression != null && expression.Member != null ? expression.Member.Name : null,
                    expression != null ? expression.ToString() : null));
        }

        /// <summary>
        /// Process method calls and translate them into OData.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private Expression VisitMethodCall(MethodCallExpression expression)
        {
            // Look for either an instance or static method
            string methodName = null;
            if (InstanceMethods.TryGetValue(expression.Method, out methodName))
            {
                this.VisitODataMethodCall(expression, methodName, false);
            }
            else if (StaticMethods.TryGetValue(expression.Method, out methodName))
            {
                this.VisitODataMethodCall(expression, methodName, true);
            }
            else if (expression.Method.GetBaseDefinition() == toStringMethod)
            {
                // handle the ToString method here
                // toString will only occur on expression that rely on a parameter,
                // because otherwise the partial evaluator would have already evaluated it
                // we get the base definition to detect overrides of ToString, which are pretty common
                this.Visit(expression.Object);
            }
            else
            {
                this.VisitCustomMethodCall(expression);
            }

            return expression;
        }

        /// <summary>
        /// Process method calls which map one-to-one on an odata supported method.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <param name="methodName">
        /// The name of the method to look for.
        /// </param>
        /// <param name="isStatic">
        /// Indicates if the method to look for is static.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private void VisitODataMethodCall(MethodCallExpression expression, string methodName, bool isStatic)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            Debug.Assert(methodName != null, "methodName cannot be null!");

            this.filter.Append(methodName);
            this.filter.Append("(");
            bool needsComma = false;

            foreach (Expression argument in GetFilterMethodArguments(expression, methodName, isStatic))
            {
                if (needsComma)
                {
                    this.filter.Append(",");
                }
                this.Visit(argument);
                needsComma = true;
            }

            this.filter.Append(")");
        }

        /// <summary>
        /// Process method calls which do not map one-to-one on an odata supported method.
        /// Currently we only support Contains.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        private void VisitCustomMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name.Equals("Contains"))
            {
                this.VisitContainsMethodCall(expression);
            }
            else
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.FilterBuildingExpressionVisitor_UnsupportedInWhereExpression,
                        expression != null ? expression.ToString() : null));
            }
        }

        /// <summary>
        /// Process a Contains method call which does not map one-to-one on an odata supported method.
        /// </summary>
        /// <param name="expression">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The visited expression.
        /// </returns>
        private void VisitContainsMethodCall(MethodCallExpression expression)
        {
            IEnumerable<Expression> arguments = GetFilterMethodArguments(expression, expression.Method.Name, expression.Method.IsGenericMethod);

            //First argument should be a enumerable of constants
            // if you would like to do a enumerable.select().where..
            // you should do that outside of the query for now
            IEnumerable enumerable = null;
            Expression expr = arguments.FirstOrDefault();
            if (expr != null && expr is ConstantExpression)
            {
                enumerable = ((ConstantExpression)expr).Value as IEnumerable;
            }
            Expression comparand = arguments.Skip(1).FirstOrDefault();
            if (enumerable != null && comparand != null)
            {
                List<object> elements = enumerable.OfType<object>().ToList();
                if(elements.Count > 0)
                {
                    // create our rewritten expression tree
                    // by tranforming the contains into a concatenation of 'or' expressions
                    Expression orExpression = elements.Select(o => Expression.Equal(comparand, Expression.Constant(o)))
                    .Aggregate((e1, e2) => Expression.OrElse(e1, e2));
                    
                    this.Visit(orExpression);
                }
            }
            else
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.FilterBuildingExpressionVisitor_UnsupportedInWhereExpression,
                        expression != null ? expression.ToString() : null));
            }
        }

        /// <summary>
        /// Remove the operator from certain unary expressions like quote or
        /// conversions we can ignore.
        /// </summary>
        /// <param name="expression">
        /// The expression to check.
        /// </param>
        /// <returns>
        /// An unquoted expression.
        /// </returns>
        private static Expression StripUnaryOperator(Expression expression)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            UnaryExpression unary = expression as UnaryExpression;
            return unary != null ? unary.Operand : expression;
        }

        /// <summary>
        /// Returns the ordered collection of arguments for the the given OData filter 
        /// method given the <see cref="MethodCallExpression"/> instance.
        /// </summary>
        /// <param name="expression">
        /// The expression to convert into a list of filter method arguments.
        /// </param>
        /// <param name="methodName">
        /// The name of the OData filter method.
        /// </param>
        /// <param name="isStatic">
        /// Indicates if the <see cref="MethodCallExpression"/> instance is expected to 
        /// be for a static method or not.
        /// </param>
        /// <returns>
        /// The ordered collection of arguments for the given OData filter method.
        /// </returns>
        private static IEnumerable<Expression> GetFilterMethodArguments(MethodCallExpression expression, 
                                                                        string methodName, 
                                                                        bool isStatic)
        {
            IEnumerable<Expression> arguments;
            if (methodName == subStringOfFilterMethod)
            {
                // Awkwardly, the OData protocol for URI conventions states that for subStringOf(), 
                // the search string argument comes before the string to be search. Therefore when 
                // translating from String.Contains(), we have to return the expression arguments 
                // (the search string) before returning the expression object (the string to be searched).
                arguments = expression.Arguments.Concat(new Expression[] { expression.Object });
            }
            else
            {
                arguments = (isStatic) ?
                    new Expression[0] :
                    new Expression[] { expression.Object };
                arguments = arguments.Concat(expression.Arguments);
            }

            return arguments;
        }

        /// <summary>
        /// Get a sequence of types from the given type up through the base
        /// type.  This is helpful given that all of the new reflection APIs
        /// only provide DeclaredFoos, but you can use SelectMany with this
        /// sequence to get all of the inherited Foos.
        /// </summary>
        /// <param name="type">
        /// The type to get the hierarchy for.
        /// </param>
        /// <returns>
        /// Sequence describing the type hierarchy.
        /// </returns>
        private static IEnumerable<Type> GetBaseTypesAndSelf(Type type)
        {
            Debug.Assert(type != null, "type cannot be null!");

            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        /// <summary>
        /// Find a method by type and name.
        /// </summary>
        /// <param name="type">
        /// The type with the method.
        /// </param>
        /// <param name="name">
        /// The name of the method.
        /// </param>
        /// <param name="parameterTypes">
        /// The parameters to the method.
        /// </param>
        /// <returns>
        /// A sequence of MethodInfos.
        /// </returns>
        private static IEnumerable<MethodInfo> GetMethods(Type type, string name, Type[] parameterTypes)
        {
            return GetBaseTypesAndSelf(type)
                .SelectMany(t => t.GetMethods().Where(m => m.Name == name))
                .Where(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        /// <summary>
        /// Find an instance method info by type and name.
        /// </summary>
        /// <param name="type">
        /// The type declaring the method.
        /// </param>
        /// <param name="name">
        /// The name of the method.
        /// </param>
        /// <param name="parameterTypes">
        /// Types of the method parameters.
        /// </param>
        /// <returns>
        /// The desired MethodInfo or null.
        /// </returns>
        private static MethodInfo FindInstanceMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => !m.IsStatic)
                .SingleOrDefault();
        }

        /// <summary>
        /// Find an instance property info by type and name.
        /// </summary>
        /// <param name="type">
        /// The type declaring the property.
        /// </param>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// The desired property's getter's MethodInfo or null.
        /// </returns>
        private static MemberInfo FindInstanceProperty(Type type, string name)
        {
            return
                GetBaseTypesAndSelf(type)
                .SelectMany(t => t.GetProperties().Where(
                    p => p.Name == name && p.CanRead && !p.GetGetMethod().IsStatic))
                .Cast<MemberInfo>()
                .SingleOrDefault();
        }

        /// <summary>
        /// Find a static method info by type and name.
        /// </summary>
        /// <param name="type">
        /// The type declaring the method.
        /// </param>
        /// <param name="name">
        /// The name of the method.
        /// </param>
        /// <param name="parameterTypes">
        /// Types of the method parameters.
        /// </param>
        /// <returns>
        /// The desired MethodInfo or null.
        /// </returns>
        private static MethodInfo FindStaticMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => m.IsStatic)
                .SingleOrDefault();
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
