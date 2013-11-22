// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadLogsControl : UserControl
    {
        private string uploadUrl;
        public string logs;

        public UploadLogsControl(string testGroupName, string logs, string uploadUrl)
        {
            this.InitializeComponent();
            this.lblTitle.Text = "Logs for " + testGroupName;
            var bounds = Application.Current.MainWindow.RenderSize;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;

            this.uploadUrl = uploadUrl;
            this.logs = logs;
            this.txtArea.Text = logs;
        }

        internal Task Display()
        {
            Task popupTask = this.DisplayPopup();
            Task uploadTask = this.UploadLogs();
            return Task.WhenAll(popupTask, uploadTask);
        }

        private Task DisplayPopup()
        {
            Popup popup = new Popup();
            popup.Child = this;
            popup.PlacementTarget = Application.Current.MainWindow;
            popup.Placement = PlacementMode.Center;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            popup.IsOpen = true;
            popup.Closed += (snd, ea) =>
            {
                tcs.SetResult(true);
            };

            return tcs.Task;
        }

        private async Task UploadLogs()
        {
            string uploadUrl = this.uploadUrl;
            if (!string.IsNullOrEmpty(uploadUrl))
            {
                await Util.UploadLogs(uploadUrl, this.logs, "net45", false);
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            ((Popup)this.Parent).IsOpen = false;
        }
    }
}
