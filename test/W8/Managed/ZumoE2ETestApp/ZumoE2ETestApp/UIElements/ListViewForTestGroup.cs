using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.UIElements
{
    public class ListViewForTestGroup
    {
        private int index;
        private ZumoTestGroup testGroup;

        public ListViewForTestGroup(int index, ZumoTestGroup testGroup)
        {
            this.index = index;
            this.testGroup = testGroup;
        }

        public string Name
        {
            get { return string.Format("{0}. {1}", this.index, this.testGroup.Name); }
        }
    }
}
