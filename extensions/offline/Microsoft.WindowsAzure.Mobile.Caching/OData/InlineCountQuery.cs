using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class InlineCountQuery : IQueryOption
    {
        public InlineCountQuery(string inlineCountString)
        {
            if (string.IsNullOrEmpty(inlineCountString))
            {
                throw new ArgumentNullException("inlineCountString");
            }

            this.RawValue = inlineCountString;
        }

        public string RawValue { get; private set; }

        public ODataExpression Expression 
        { 
            get 
            {
                return ODataParser.ParseInlineCount(this.RawValue);
            }
        }
    }
}
