using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    /// <summary>
    /// Represents the raw query values in the string format from the incoming request.
    /// </summary>
    public class RawQueryOptions
    {
        /// <summary>
        ///  Gets the raw $filter query value from the incoming request Uri if exists.
        /// </summary>
        public string Filter { get; internal set; }

        /// <summary>
        ///  Gets the raw $orderby query value from the incoming request Uri if exists.
        /// </summary>
        public string OrderBy { get; internal set; }

        /// <summary>
        ///  Gets the raw $top query value from the incoming request Uri if exists.
        /// </summary>
        public string Top { get; internal set; }

        /// <summary>
        ///  Gets the raw $skip query value from the incoming request Uri if exists.
        /// </summary>
        public string Skip { get; internal set; }

        /// <summary>
        ///  Gets the raw $inlineCount query value from the incoming request Uri if exists.
        /// </summary>
        public string InlineCount { get; internal set; }
    }
}
