// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// Assertions that will throw InvalidOperationExceptions when their
    /// conditions have not been met.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Ignore any deltas due to quantization below this threshold.
        /// </summary>
        private const double QuantizationThreshold = 0.0001;

        /// <summary>
        /// Force a failure.
        /// </summary>
        /// <param name="message">Details about the assertion.</param>
        public static void Fail(string message)
        {
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Assert that two values are equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        public static void AreEqual(object a, object b)
        {
            AreEqual(a, b, string.Format("Expected {0} equal to {1}.", a, b));
        }

        /// <summary>
        /// Assert that two values are equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void AreEqual(object a, object b, string message)
        {
            bool equal = object.Equals(a, b);
            if (a is double && b is double)
            {
                equal = Math.Abs((double)a - (double)b) < QuantizationThreshold;
            }
            else if (a is float && b is float)
            {
                equal = Math.Abs((float)a - (float)b) < QuantizationThreshold;
            }

            if (!equal)
            {
                Fail(message ?? string.Format("Expected {0} equal to {1}.", a, b));
            }
        }

        /// <summary>
        /// Assert that two values are the same.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        public static void AreSame(object a, object b)
        {
            AreSame(a, b, string.Format("Expected {0} the same as {1}.", a, b));
        }

        /// <summary>
        /// Assert that two values are the same.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void AreSame(object a, object b, string message)
        {
            if (!object.ReferenceEquals(a, b))
            {
                Fail(message ?? string.Format("Expected {0} the same as {1}.", a, b));
            }
        }

        /// <summary>
        /// Assert that two values are not equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        public static void AreNotEqual(object a, object b)
        {
            AreNotEqual(a, b, string.Format("Expected {0} not equal to {1}.", a, b));
        }

        /// <summary>
        /// Assert that two values are not equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void AreNotEqual(object a, object b, string message)
        {
            if (object.Equals(a, b))
            {
                Fail(message);
            }
        }

        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        public static void IsTrue(bool condition)
        {
            IsTrue(condition, "Expected condition to be true.");
        }

        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        public static void IsFalse(bool condition)
        {
            IsFalse(condition, "Expected condition to be false.");
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void IsFalse(bool condition, string message)
        {
            if (condition)
            {
                Fail(message);
            }
        }

        /// <summary>
        /// Assert that a substring is present in the text.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        public static void Contains(string text, string substring)
        {
            Contains(
                text,
                substring,
                string.Format(
                    "Expected to find substring \"{0}\" in \"{1}\".",
                    substring,
                    text));
        }

        /// <summary>
        /// Assert that a substring is present in the text.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void Contains(string text, string substring, string message)
        {
            if (text != null && substring != null && !text.Contains(substring))
            {
                Fail(message);
            }
        }

        /// <summary>
        /// Assert that a string starts with another.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        public static void StartsWith(string text, string substring)
        {
            StartsWith(
                text,
                substring,
                string.Format(
                    "Expected string \"{1}\" to start with \"{0}\".",
                    substring,
                    text));
        }

        /// <summary>
        /// Assert that a string starts with another.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void StartsWith(string text, string substring, string message)
        {
            if (text != null && substring != null && !text.StartsWith(substring))
            {
                Fail(message);
            }
        }

        /// <summary>
        /// Assert that a string ends with another.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        public static void EndsWith(string text, string substring)
        {
            EndsWith(
                text,
                substring,
                string.Format(
                    "Expected string \"{1}\" to start with \"{0}\".",
                    substring,
                    text));
        }

        /// <summary>
        /// Assert that a string ends with another.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="substring">The substring to find.</param>
        /// <param name="message">Details about the assertion.</param>
        public static void EndsWith(string text, string substring, string message)
        {
            if (text != null && substring != null && !text.EndsWith(substring))
            {
                Fail(message);
            }
        }

        public static void IsNull(object value)
        {
            IsNull(value, string.Format("Expected null, not {0}", value));
        }

        public static void IsNull(object value, string message)
        {
            if (value != null)
            {
                Fail(message);
            }
        }

        public static void IsNotNull(object value)
        {
            IsNotNull(value, "Expected a value, not null.");
        }

        public static void IsNotNull(object value, string message)
        {
            if (value == null)
            {
                Fail(message);
            }
        }

        public static void AreEquivalent(IEnumerable a, IEnumerable b)
        {
            AreEquivalent(a, b, "Sequences do not contain the same values");
        }

        public static void AreEquivalent(IEnumerable a, IEnumerable b, string message)
        {
            IEnumerable<object> first = a.OfType<object>();
            IEnumerable<object> second = b.OfType<object>();
            IEnumerable<object> intersection = first.Intersect(second);
            Assert.AreEqual(first.Count(), intersection.Count(), message);
        }
    }
}
