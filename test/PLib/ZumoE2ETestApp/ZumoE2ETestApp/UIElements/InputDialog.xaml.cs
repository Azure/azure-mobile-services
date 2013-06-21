// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class InputDialog : Page
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

            var bounds = Window.Current.Bounds;
            this.grdRootPanel.Height = bounds.Height;
            this.grdRootPanel.Width = bounds.Width;
        }

        public InputDialog(string caption, string text, string cancelButtonText)
            : this(caption, text, cancelButtonText, null)
        {
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
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
