using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public delegate TResult OptionalFunc<in T1, out TResult>(T1 arg1 = default(T1));
    public delegate TResult OptionalFunc<in T1, in T2, out TResult>(T1 arg1 = default(T1), T2 arg2 = default(T2));
    public delegate TResult OptionalFunc<in T1, in T2, in T3, out TResult>(T1 arg1 = default(T1), T2 arg2 = default(T2), T3 arg3 = default(T3));
}
