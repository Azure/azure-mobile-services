using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    /// <summary>
    /// Defines an enumeration for $inlinecount query option values.
    /// </summary>
    public enum InlineCount
    {
        /// <summary>
        /// Corresponds to the 'none' $inlinecount query option value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Corresponds to the 'allpages' $inlinecount query option value.
        /// </summary>
        AllPages = 1
    }
}
