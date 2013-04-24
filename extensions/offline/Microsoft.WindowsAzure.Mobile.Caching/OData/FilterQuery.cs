using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class FilterQuery : IQueryOption
    {
        public FilterQuery(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                throw new ArgumentNullException("filterString");
            }

            this.RawValue = filterString;
        }

        public string RawValue { get; private set; }

        public ODataExpression Expression
        {
            get
            {
                return ODataParser.ParseFilter(this.RawValue);
            }
        }
    }
}
