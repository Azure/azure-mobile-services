using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OfflinePerfCore.Types;

namespace OfflinePerfCore.Common
{
    public class SimpleTypeStream : LongStream
    {
        private Random rndGen;
        private int numberOfItems;
        private int itemsLeftToReturn;
        private int? totalCount;

        public SimpleTypeStream(Random rndGen, int numberOfItems, int? totalCount)
        {
            this.rndGen = rndGen;
            this.numberOfItems = numberOfItems;
            this.itemsLeftToReturn = numberOfItems;
            this.totalCount = totalCount;
        }

        private bool IncludeTotalCount
        {
            get { return this.totalCount.HasValue; }
        }

        protected override string GetHeader()
        {
            if (this.IncludeTotalCount)
            {
                return "{\"results\":[";
            }
            else
            {
                return "[";
            }
        }

        protected override string GetRecurringPart()
        {
            if (this.itemsLeftToReturn <= 0)
            {
                return null;
            }

            var json = JsonConvert.SerializeObject(new SimpleType(this.rndGen));
            var firstItem = this.itemsLeftToReturn == this.numberOfItems;
            if (!firstItem)
            {
                json = "," + json;
            }

            this.itemsLeftToReturn--;
            return json;
        }

        protected override string GetEnding()
        {
            if (this.IncludeTotalCount)
            {
                return "],\"count\":" + this.totalCount.Value + "}";
            }
            else
            {
                return "]";
            }
        }
    }
}
