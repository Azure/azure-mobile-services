// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace System.Collections.Generic
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Splits the given sequence into sequences of the given length.
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int maxLength)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException("maxLength");
            }

            var enumerator = source.GetEnumerator();
            var batch = new List<T>(maxLength);

            while (enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);

                //Have we finished this batch? Yield it and start a new one.
                if (batch.Count == maxLength)
                {
                    yield return batch;
                    batch = new List<T>(maxLength);
                }
            }

            //Yield the final batch if it has any elements
            if (batch.Count > 0)
            {
                yield return batch;
            }

        }
    }
}
