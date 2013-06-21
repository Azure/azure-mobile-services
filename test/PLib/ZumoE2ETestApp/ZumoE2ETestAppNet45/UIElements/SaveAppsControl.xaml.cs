// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SaveAppsControl : UserControl
    {
        public string ApplicationUrl { get; set; }
        public string ApplicationKey { get; set; }

        public event EventHandler CloseRequested;

        public SaveAppsControl(List<MobileServiceInfo> savedServices)
        {
            this.InitializeComponent();
            var bounds = Application.Current.MainWindow.RenderSize;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;
            this.lstApps.ItemsSource = savedServices;
        }

        private void btnSelect_Click_1(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Please select a service first", "Error");
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

        private void lstApps_DoubleTapped_1(object sender, MouseEventArgs e)
        {
            btnSelect_Click_1(sender, e);
        }
    }
}
