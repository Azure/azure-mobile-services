using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class ODataParser
    {
        public static ODataExpression ParseSkip(string skip)
        {
            int intValue;
            if (int.TryParse(skip, out intValue))
            {
                return new ODataSkipExpression(intValue);
            }
            else
            {
                throw new InvalidOperationException("Skip can only take an integer");
            }
        }

        public static ODataExpression ParseTop(string top)
        {
            int intValue;
            if (int.TryParse(top, out intValue))
            {
                return new ODataTopExpression(intValue);
            }
            else
            {
                throw new InvalidOperationException("Top can only take an integer");
            }
        }

        public static ODataExpression ParseOrderBy(string orderby)
        {
            return new ODataOrderByExpression(orderby.Split(',').Select(o => o.Trim()).Select(o => ToSelector(o)));
        }

        private static ODataOrderByExpression.Selector ToSelector(string orderby)
        {
            string[] orderParts = orderby.Split(' ');
            string propertyName = orderParts.First();
            Order order = (orderParts.Length > 1) ? (orderParts[1].Equals("desc") ? Order.Descending : Order.Ascending) : Order.Ascending;
            return new ODataOrderByExpression.Selector(new ODataMemberExpression(null,propertyName), order);
        }

        public static ODataExpression ParseInlineCount(string inlineCount)
        {
            InlineCount enumValue;
            if (Enum.TryParse(inlineCount, out enumValue))
            {
                return new ODataInlineCountExpression(enumValue);
            }
            else
            {
                throw new InvalidOperationException("Top can only take an integer");
            }
        }

        public static ODataExpression ParseFilter(string filter)
        {
            return new ODataFilterParser(filter).Parse();
        }
    }
}
