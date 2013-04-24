using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Foreach<T>(this IEnumerable<T> This, Action<T> action)
        {
            foreach (var item in This)
            {
                action(item);
                yield return item;
            }
        }
    }
}
