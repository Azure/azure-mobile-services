// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InputDialog : UserControl
    {
        public bool Cancelled { get; private set; }

        public InputDialog(string caption, string text, string cancelButtonText, string okButtonText)
        {
            this.InitializeComponent();

            this.btnCancel.Content = cancelButtonText;
            this.lblTitle.Text = caption;
            this.txtText.Text = text;
            if (okButtonText == null)
            {
                this.btnOk.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.btnOk.Visibility = Visibility.Visible;
                this.btnOk.Content = okButtonText;
            }

            var bounds = Application.Current.MainWindow.RenderSize;
            this.grdRootPanel.Height = bounds.Height;
            this.grdRootPanel.Width = bounds.Width;
        }

        public InputDialog(string caption, string text, string cancelButtonText)
            : this(caption, text, cancelButtonText, null)
        {
        }

        private void btnCancel_Click_1(object sender, RoutedEventArgs e)
        {
            this.Cancelled = true;
            ((Popup)this.Parent).IsOpen = false;
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            this.Cancelled = false;
            ((Popup)this.Parent).IsOpen = false;
        }

        public Task Display()
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
    }
}
