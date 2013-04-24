using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class SkipQuery : IQueryOption
    {
        public SkipQuery(string skipString)
        {
            if (string.IsNullOrEmpty(skipString))
            {
                throw new ArgumentNullException("skipString");
            }

            this.RawValue = skipString;
        }

        public string RawValue { get; private set; }

        public ODataExpression Expression 
        {
            get
            {
                return ODataParser.ParseSkip(this.RawValue);
            }
        }
    }
}
