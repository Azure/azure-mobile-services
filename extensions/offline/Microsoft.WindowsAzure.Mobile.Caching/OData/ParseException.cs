using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class ParseException : Exception
    {
        public ParseException(string message, int position)
            : base(string.Format(CultureInfo.InvariantCulture, string.Format(CultureInfo.CurrentCulture, R.GetString("ParseExceptionFormat"), message, position)))
        {
        }
    }
}
