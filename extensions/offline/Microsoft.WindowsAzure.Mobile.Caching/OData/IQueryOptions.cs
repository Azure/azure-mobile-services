using System;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public interface IQueryOptions
    {
        IQueryOption Filter { get; }
        IQueryOption InlineCount { get; }
        IQueryOption OrderBy { get; }
        IQueryOption Skip { get; }
        IQueryOption Top { get; }
    }
}
