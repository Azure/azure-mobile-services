using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class TopQuery : IQueryOption
    {
        public TopQuery(string topString)
        {
            if (string.IsNullOrEmpty(topString))
            {
                throw new ArgumentNullException("topString");
            }

            this.RawValue = topString;
        }

        public string RawValue { get; private set; }

        public ODataExpression Expression 
        { 
            get 
            {
                return ODataParser.ParseTop(this.RawValue);
            }
        }
    }
}
