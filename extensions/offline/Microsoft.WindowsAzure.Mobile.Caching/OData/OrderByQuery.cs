using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class OrderByQuery : IQueryOption
    {
        public OrderByQuery(string orderbyString)
        {
            if (string.IsNullOrEmpty(orderbyString))
            {
                throw new ArgumentNullException("orderbyString");
            }

            this.RawValue = orderbyString;
        }

        public string RawValue { get; private set; }

        public ODataExpression Expression
        {
            get
            {
                return ODataParser.ParseOrderBy(this.RawValue);
            }
        }        
    }
}
