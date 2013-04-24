using System;
namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public interface IQueryOption
    {
        ODataExpression Expression { get; }
        string RawValue { get; }
    }
}
