// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A helper class that manages tags and associated metadata. Tag
    /// expressions are evaluated at the TestGroup level.
    /// </summary>
    internal sealed partial class TagManager
    {
        /// <summary>
        /// Evaluate tag expressions.
        /// </summary>
        /// <remarks>
        /// Tag expressions are derived from the following EBNF grammar:
        ///     {Expression} :=
        ///         {Expression} + {Term} |
        ///         {Expression} - {Term} |
        ///         {Term}
        ///     {Term} :=
        ///         {Term} * {Factor} |
        ///         {Factor}
        ///     {Factor} :=
        ///         !{Factor} |
        ///         ({Expression}) |
        ///         {Tag}
        ///     {Tag} :=
        ///         All |
        ///         [^InvalidCharacters]+
        ///  
        /// The non-terminals for {Expression} and {Term} will be left factored
        /// in the recursive descent parser below.
        /// </remarks>
        private class ExpressionEvaluator
        {
            /// <summary>
            /// Union character.
            /// </summary>
            private const string Union = "+";

            /// <summary>
            /// Intersection character.
            /// </summary>
            private const string Intersection = "*";

            /// <summary>
            /// Complement character.
            /// </summary>
            private const string Complement = "!";

            /// <summary>
            /// Difference character.
            /// </summary>
            private const string Difference = "-";

            /// <summary>
            /// The "All" string constant.
            /// </summary>
            private const string All = "all";

            /// <summary>
            /// Invalid characters in a tag name.
            /// </summary>
            private static char[] InvalidCharacters = new char[] 
            { 
                Union[0],
                Intersection[0], 
                Complement[0], 
                Difference[0], 
                '(', 
                ')', 
                '/' 
            };

            /// <summary>
            /// Evaluate a tag expression.
            /// </summary>
            /// <param name="owner">The owner object.</param>
            /// <param name="tagExpression">Tag expression.</param>
            /// <returns>Test methods associated with the tag expression.</returns>
            public static IEnumerable<TestMethod> Evaluate(TagManager owner, string tagExpression)
            {
                return new ExpressionEvaluator(owner, tagExpression).Evaluate();
            }

            /// <summary>
            /// The owning TagManager instance.
            /// </summary>
            private TagManager _owner;

            /// <summary>
            /// Expression being evaluated.
            /// </summary>
            private string _tagExpression;

            /// <summary>
            /// Current position in the expression.
            /// </summary>
            private int _position;

            /// <summary>
            /// Create an expression evaluator.
            /// </summary>
            /// <param name="owner">The owner object.</param>
            /// <param name="tagExpression">Expression object.</param>
            private ExpressionEvaluator(TagManager owner, string tagExpression)
            {
                if (tagExpression == null)
                {
                    throw new ArgumentNullException("tagExpression");
                }
                else if (tagExpression.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("tagExpression");
                }
                _owner = owner;
                _tagExpression = tagExpression;
            }

            /// <summary>
            /// Match a sequence of characters.
            /// </summary>
            /// <param name="expected">String to match.</param>
            private void Match(string expected)
            {
                if (!TryMatch(expected))
                {
                    throw new FormatException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Expected {1} at position {2} in expression {0}.",
                            _tagExpression,
                            expected,
                            _position));
                }
            }

            /// <summary>
            /// Try to match a sequence of characters.
            /// </summary>
            /// <param name="expected">String to match.</param>
            /// <returns>Returns a value indicating whether the match was 
            /// successful.</returns>
            private bool TryMatch(string expected)
            {
                if (_position + expected.Length > _tagExpression.Length)
                {
                    return false;
                }

                for (int i = 0; i < expected.Length; i++)
                {
                    if (_tagExpression[i + _position] != expected[i])
                    {
                        return false;
                    }
                }

                _position += expected.Length;
                return true;
            }

            /// <summary>
            /// Evaluate an expression.
            /// </summary>
            /// <returns>Test methods described by the expression.</returns>
            private IEnumerable<TestMethod> Evaluate()
            {
                IEnumerable<TestMethod> expression = ReadExpression();
                if (_position >= 0 && _position < _tagExpression.Length)
                {
                    throw new FormatException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Expected end of tag at position {1} in expression {0}",
                            _tagExpression,
                            _position));
                }
                return expression;
            }

            /// <summary>
            /// Evaluate an expression.
            /// </summary>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// We need to factor out left recursion, so:
            ///     {Expression} :=
            ///         {Expression} + {Term} |
            ///         {Expression} - {Term} |
            ///         {Term}
            /// becomes:
            ///     {Expression} :=
            ///     	{Term}{Expression'}
            ///     
            ///     {Expression'} :=
            ///     	#empty#
            ///     	+ {Term}{Expression'}
            ///     	- {Term}{Expression'}
            /// </remarks>
            private IEnumerable<TestMethod> ReadExpression()
            {
                return ReadExpression(ReadTerm());
            }

            /// <summary>
            /// Evaluate an expression.
            /// </summary>
            /// <param name="term">
            /// Left term already read as part of the expression.
            /// </param>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// Non-terminal created for left-factoring:
            ///     {Expression'} :=
            ///     	#empty#
            ///     	+ {Term}{Expression'}
            ///     	- {Term}{Expression'}
            /// </remarks>
            private IEnumerable<TestMethod> ReadExpression(IEnumerable<TestMethod> term)
            {
                if (TryMatch(Union))
                {
                    return ReadExpression(term.Union(ReadTerm()));
                }
                else if (TryMatch(Difference))
                {
                    return ReadExpression(term.Except(ReadTerm()));
                }
                else
                {
                    return term;
                }
            }

            /// <summary>
            /// Evaluate a term.
            /// </summary>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// We need to factor out left recursion, so:
            ///     {Term} :=
            ///         {Factor} * {Term} |
            ///         {Factor}
            /// becomes:
            ///     {Term} :=
            ///         {Factor}{Term'}
            /// 
            ///     {Term'} :=
            ///     	#empty#
            ///     	^ {Factor}{Term'}
            /// </remarks>
            private IEnumerable<TestMethod> ReadTerm()
            {
                return ReadTerm(ReadFactor());
            }

            /// <summary>
            /// Evaluate a term.
            /// </summary>
            /// <param name="factor">
            /// Left term already read as part of the expression.
            /// </param>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// Non-terminal created for left-factoring:
            ///     {Term'} :=
            ///     	#empty#
            ///     	^ {Factor}{Term'}
            /// </remarks>
            private IEnumerable<TestMethod> ReadTerm(IEnumerable<TestMethod> factor)
            {
                if (TryMatch(Intersection))
                {
                    return ReadTerm(factor.Intersect(ReadFactor()));
                }
                else
                {
                    return factor;
                }
            }

            /// <summary>
            /// Evaluate a factor.
            /// </summary>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// {Factor} :=
            ///     !{Factor} |
            ///     ({Expression}) |
            ///     {Tag}
            /// </remarks>
            private IEnumerable<TestMethod> ReadFactor()
            {
                if (TryMatch(Complement))
                {
                    IEnumerable<TestMethod> allMethods = _owner.Universe;
                    return allMethods.Except(ReadFactor());
                }
                else if (TryMatch("("))
                {
                    IEnumerable<TestMethod> expression = ReadExpression();
                    Match(")");
                    return expression;
                }
                else
                {
                    return ReadTag();
                }
            }

            /// <summary>
            /// Creates a new empty collection.
            /// </summary>
            /// <returns>Returns an empty collection.</returns>
            private static List<TestMethod> CreateEmptyList()
            {
                return new List<TestMethod>();
            }

            /// <summary>
            /// Evaluate a tag.
            /// </summary>
            /// <returns>Test methods described by the expression.</returns>
            /// <remarks>
            /// {Tag} :=
            ///     All |
            ///     [^InvalidCharacters]+
            /// </remarks>
            private IEnumerable<TestMethod> ReadTag()
            {
                int end = _tagExpression.IndexOfAny(InvalidCharacters, _position);
                if (end < 0)
                {
                    end = _tagExpression.Length;
                }
                string tag = _tagExpression.Substring(_position, end - _position);
                if (string.IsNullOrEmpty(tag))
                {
                    throw new FormatException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Expected a tag at position {1} in expression {0}",
                            _tagExpression,
                            _position));
                }
                _position = end;
                if (string.Compare(tag, All, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return new List<TestMethod>(_owner.Universe);
                }
                else
                {
                    List<TestMethod> methods;
                    if (!_owner._tagsToMethods.TryGetValue(tag, out methods))
                    {
                        methods = CreateEmptyList();
                    }
                    return methods;
                }
            }
        }
    }
}
