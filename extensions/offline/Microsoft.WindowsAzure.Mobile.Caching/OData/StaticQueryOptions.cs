using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class StaticQueryOptions : IQueryOptions
    {
        public IQueryOption Filter
        {
            get;
            set;
        }

        public IQueryOption InlineCount
        {
            get;
            set;
        }

        public IQueryOption OrderBy
        {
            get;
            set;
        }

        public IQueryOption Skip
        {
            get;
            set;
        }

        public IQueryOption Top
        {
            get;
            set;
        }
    }
}
