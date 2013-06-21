// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SaveAppsControl : Page
    {
        public string ApplicationUrl { get; set; }
        public string ApplicationKey { get; set; }

        public event EventHandler CloseRequested;

        public SaveAppsControl(List<MobileServiceInfo> savedServices)
        {
            this.InitializeComponent();
            var bounds = Window.Current.Bounds;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;
            this.lstApps.ItemsSource = savedServices;
        }

        private async void btnSelect_Click_1(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.lstApps.SelectedIndex;
            if (selectedIndex >= 0)
            {
                MobileServiceInfo info = (MobileServiceInfo)this.lstApps.SelectedItem;
                this.ApplicationUrl = info.AppUrl;
                this.ApplicationKey = info.AppKey;
                if (this.CloseRequested != null)
                {
                    this.CloseRequested(this, EventArgs.Empty);
                }
            }
            else
            {
                await new MessageDialog("Please select a service first", "Error").ShowAsync();
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            this.ApplicationUrl = null;
            this.ApplicationKey = null;
            if (this.CloseRequested != null)
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }

        private void lstApps_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            btnSelect_Click_1(sender, e);
        }
    }
}
