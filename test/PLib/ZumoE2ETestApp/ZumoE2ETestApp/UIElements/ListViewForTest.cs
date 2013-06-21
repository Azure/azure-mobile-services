// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.ComponentModel;
#if NETFX_CORE
using Windows.UI;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.UIElements
{
    public class ListViewForTest : INotifyPropertyChanged
    {
        private int index;
        private ZumoTest test;

        public ListViewForTest(int index, ZumoTest test)
        {
            this.index = index;
            this.test = test;
            test.TestStatusChanged += test_TestStatusChanged;
        }

        void test_TestStatusChanged(object sender, TestStatusChangedEventArgs e)
        {
            var propChanged = this.PropertyChanged;
            if (propChanged != null)
            {
                propChanged(this, new PropertyChangedEventArgs("ColorFromStatus"));
                propChanged(this, new PropertyChangedEventArgs("TestName"));
            }
        }

        internal ZumoTest Test
        {
            get { return this.test; }
        }

        public string TestName
        {
            get { return string.Format("{0}. {1} - {2}", this.index, this.test.Name, this.test.Status); }
        }

        public Brush ColorFromStatus
        {
            get
            {
                Color color = Colors.Black;
                switch (this.test.Status)
                {
                    case TestStatus.Failed:
                        color = Colors.Red;
                        break;
                    case TestStatus.NotRun:
#if !WINDOWS_PHONE
                        color = Colors.Black;
#else
                        color = Colors.Gray;
#endif
                        break;
                    case TestStatus.Passed:
                        color = Colors.Green;
                        break;
                    case TestStatus.Running:
                        color = Colors.Gray;
                        break;
                }

                return new SolidColorBrush(color);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
