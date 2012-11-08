// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Create an OData filter expression by walking an expression tree.
    /// </summary>
    internal class FilterBuildingExpressionVisitor : ExpressionVisitor
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

        /// <summary>
        /// Defines the instance methods that are translated into OData filter
        /// expressions.
        /// </summary>
        private static Dictionary<MethodInfo, string> instanceMethods =
            new Dictionary<MethodInfo, string>
            {
                { TypeExtensions.FindInstanceMethod(typeof(string), "ToLower"), toLowerFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "ToUpper"), toUpperFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Trim"), trimFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "StartsWith", typeof(string)), startsWithFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "EndsWith", typeof(string)), endsWithFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "IndexOf", typeof(string)), indexOfFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "IndexOf", typeof(char)), indexOfFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Contains", typeof(string)), subStringOfFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Replace", typeof(string), typeof(string)), replaceFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Replace", typeof(char), typeof(char)), replaceFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Substring", typeof(int)), substringFilterMethod },
                { TypeExtensions.FindInstanceMethod(typeof(string), "Substring", typeof(int), typeof(int)), substringFilterMethod },
            };

        /// <summary>
        /// Defines the static methods that are translated into OData filter
        /// expressions.
        /// </summary>
        private static Dictionary<MethodInfo, string> staticMethods =
            new Dictionary<MethodInfo, string>
            {
                { TypeExtensions.FindStaticMethod(typeof(Math), "Floor", typeof(double)), floorFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(Math), "Floor", typeof(decimal)), floorFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(Math), "Ceiling", typeof(double)), ceilingFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(Math), "Ceiling", typeof(decimal)), ceilingFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(Math), "Round", typeof(double)), roundFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(Math), "Round", typeof(decimal)), roundFilterMethod },
                { TypeExtensions.FindStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)), concatFilterMethod }
            };

        /// <summary>
        /// Defines the instance properties that are translated into OData
        /// filter expressions.
        /// </summary>
        private static Dictionary<MemberInfo, string> instanceProperties =
            new Dictionary<MemberInfo, string>
            {
                { TypeExtensions.FindInstanceProperty(typeof(string), "Length"), "length" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Day"), "day" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Month"), "month" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Year"), "year" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Hour"), "hour" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Minute"), "minute" },
                { TypeExtensions.FindInstanceProperty(typeof(DateTime), "Second"), "second" },
            };

        /// <summary>
        /// Defines a table of implicit conversions for numeric types derived
        /// from http://msdn.microsoft.com/en-us/library/y5b434w4.aspx
        /// </summary>
        private static Dictionary<Type, Type[]> implicitConversions =
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

        /// <summary>
        /// The OData query generated by this visitor
        /// </summary>
        private StringBuilder filter = new StringBuilder();

        /// <summary>
        /// A value used to indicate whether an expression was actually visited
        /// or whether it was skipped (and hence not supported by our builder).
        /// </summary>
        private bool visited;

        /// <summary>
        /// Translate an expression tree into a compiled OData query.
        /// </summary>
        /// <param name="expression">The expression tree.</param>
        /// <returns>An OData query.</returns>
        public static string Create(Expression expression)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
                    
            // Walk the expression tree and build the filter.
            FilterBuildingExpressionVisitor visitor = new FilterBuildingExpressionVisitor();
            visitor.Visit(expression);
            return visitor.filter.ToString();
        }

        /// <summary>
        /// Mark an expression as visited.  This method is called when we're
        /// finished processing any expression we're accepting as a valid
        /// Mobile Services filter.  Note that it must be called immediately
        /// before returning the expression (because any nested Visit calls
        /// will stomp its tracking mechanism).
        /// </summary>
        private void MarkVisited()
        {
            // Set the visited flag for the current Visit call
            this.visited = true;
        }

        /// <summary>
        /// Visit a node of the expression.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The node of the exression.</returns>
        public override Expression Visit(Expression node)
        {
            // Mark this node as unvisited
            this.visited = false;

            // Call the virtual Visit which will dispatch to our various
            // VisitFoo overrides
            node = base.Visit(node);
            
            // If this expression wasn't accepted by any of our VisitFoo
            // overrides, then it's an filter type not currently supported by
            // Mobile Services and we should throw.
            if (!this.visited)
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.FilterBuildingExpressionVisitor_Visit_Unsupported,
                        node != null ? node.ToString() : null));
            }

            // Reset the visited flag so accepted nested expressions won't
            // cause unaccepted parent expressions to be allowed.  This is the
            // reason that MarkVisited() must be called immediately before
            // returning (i.e., calling it first would allow it to be stomped
            // by any recursive nesting).
            this.visited = false;
            
            return node;
        }

        /// <summary>
        /// Process the not operator.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression != null)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Not:
                        this.filter.Append(" not(");
                        this.Visit(expression.Operand);
                        this.filter.Append(")");

                        this.MarkVisited();
                        break;
                    case ExpressionType.Quote:
                        this.Visit(StripUnaryOperator(expression));
                        this.MarkVisited();
                        break;
                    case ExpressionType.Convert:
                        // Ignore conversion requests if the conversion will
                        // happen implicitly on the server anyway
                        if (IsConversionImplicit(expression, expression.Operand.Type, expression.Type))
                        {
                            this.Visit(StripUnaryOperator(expression));
                            this.MarkVisited();
                            break;
                        }
                        // Otherwise the conversion isn't supported
                        goto default;
                    default:
                        throw new NotSupportedException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.FilterBuildingExpressionVisitor_VisitOperator_Unsupported,
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
        /// <param name="expression">The conversion expression.</param>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>
        /// True if there is an implicit conversion, false otherwise.
        /// </returns>
        /// <remarks>
        /// This relies on the fact that all numeric types on the server are
        /// represented as doubles.
        /// </remarks>
        private static bool IsConversionImplicit(UnaryExpression expression, Type from, Type to)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            Debug.Assert(from != null, "from cannot be null!");
            Debug.Assert(to != null, "to cannot be null!");

            // We're only interested in conversions on table members
            if (GetTableMember(expression.Operand) != null)
            {
                // Check to see if the types can be implicitly converted            
                Type[] conversions = null;
                if (implicitConversions.TryGetValue(from, out conversions))
                {
                    return Array.IndexOf(conversions, to) >= 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Process binary comparison operators.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression != null)
            {
                // Special case concat as it's OData function isn't infix
                if (expression.NodeType == ExpressionType.Add &&
                    expression.Left.Type == typeof(string) &&
                    expression.Right.Type == typeof(string))
                {
                    this.filter.Append("concat(");
                    this.Visit(expression.Left);
                    this.filter.Append(",");
                    this.Visit(expression.Right);
                    this.filter.Append(")");
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
                                    Resources.FilterBuildingExpressionVisitor_VisitOperator_Unsupported,
                                    expression.NodeType,
                                    expression.ToString()));
                    }
                    this.Visit(expression.Right);
                    this.filter.Append(")");
                }

                this.MarkVisited();
            }

            return expression;
        }
        
        /// <summary>
        /// Process constant values.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression != null)
            {
                string value = TypeExtensions.ToODataConstant(expression.Value);
                this.filter.Append(value);

                this.MarkVisited();
            }
            return expression;
        }

        /// <summary>
        /// Process member references.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitMember(MemberExpression expression)
        {
            // Lookup the Mobile Services name of the member and use that
            SerializableMember member = GetTableMember(expression);
            if (member != null)
            {
                this.filter.Append(member.Name);

                this.MarkVisited();
                return expression;
            }
            
            // Check if this member is actually a function that looks like a
            // property (like string.Length, etc.)
            string methodName = null;
            if (instanceProperties.TryGetValue(expression.Member, out methodName))
            {
                this.filter.Append(methodName);
                this.filter.Append("(");
                this.Visit(expression.Expression);
                this.filter.Append(")");

                this.MarkVisited();
                return expression;
            }

            // Otherwise we can't process the member.
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.FilterBuildingExpressionVisitor_VisitMember_Unsupported,
                    expression != null && expression.Member != null ? expression.Member.Name : null,
                    expression != null ? expression.ToString() : null));
        }

        /// <summary>
        /// Process method calls and translate them into OData.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            // Look for either an instance or static method
            string methodName = null;
            bool isStatic = false;
            if (!instanceMethods.TryGetValue(expression.Method, out methodName) &&
                staticMethods.TryGetValue(expression.Method, out methodName))
            {
                isStatic = true;
            }

            if (methodName != null)
            {
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
                this.MarkVisited();
            }

            return expression;
        }

        /// <summary>
        /// Get the table member referenced by an expression or return null.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>The table member or null.</returns>
        protected static SerializableMember GetTableMember(Expression expression)
        {
            // Only parameter references are valid in a query (any other
            // references should have been partially evaluated away)
            MemberExpression member = expression as MemberExpression;
            if (member != null &&
                member.Expression != null &&
                member.Expression.NodeType == ExpressionType.Parameter &&
                member.Member != null)
            {
                // Lookup the Mobile Services name of the member and use that
                return SerializableType.GetMember(member.Member);
            }

            // Otherwise return null
            return null;
        }

        /// <summary>
        /// Remove the operator from certain unary expressions like quote or
        /// conversions we can ignore.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>An unquoted expression.</returns>
        private static Expression StripUnaryOperator(Expression expression)
        {
            Debug.Assert(expression != null, "expression cannot be null!");
            UnaryExpression unary = expression as UnaryExpression;
            return unary != null ? unary.Operand : expression;
        }

        /// <summary>
        /// Returns the ordered collection of arguments for the the given OData filter method given the
        /// <see cref="MethodCallExpression"/> instance.
        /// </summary>
        /// <param name="expression">The expression to convert into a list of filter method arguments.</param>
        /// <param name="methodName">The name of the OData filter method.</param>
        /// <param name="isStatic">
        /// Indicates if the <see cref="MethodCallExpression"/> instance is expected to be for a static method or not.
        /// </param>
        /// <returns>The ordered collection of arguments for the given OData filter method.</returns>
        private static IEnumerable<Expression> GetFilterMethodArguments(MethodCallExpression expression, string methodName, bool isStatic)
        {
            IEnumerable<Expression> arguments;
            if (methodName == subStringOfFilterMethod)
            {
                // Awkwardly, the OData protocol for URI conventions states that for subStringOf(), the search string
                // argument comes before the string to be search. Therefore when translating from String.Contains(), we
                // have to return the expression arguments (the search string) before returning the expression object (the
                // string to be searched).
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
    }
}
