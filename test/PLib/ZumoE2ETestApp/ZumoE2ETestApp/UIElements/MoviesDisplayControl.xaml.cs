// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoviesDisplayControl : Page
    {
        private List<string> itemsAsString;

        public MoviesDisplayControl()
        {
            this.InitializeComponent();

            var bounds = Window.Current.Bounds;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;

            this.itemsAsString = new List<string>();
        }

        internal void SetMoviesSource(object itemsSource)
        {
            this.lstMovies.ItemsSource = itemsSource;
        }

        internal IEnumerable<string> ItemsAsString
        {
            get { return this.itemsAsString; }
        }

        internal Task Display()
        {
            Popup popup = new Popup();
            popup.Child = this;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            popup.IsOpen = true;
            popup.Closed += (snd, ea) =>
            {
                tcs.SetResult(true);
            };

            // Close the dialog after a couple of seconds, saving the items in the list
            Task.Run(async delegate
            {
                await Task.Delay(3000);

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                {
                    this.btnClose_Click_1(this.btnClose, new RoutedEventArgs());
                });
            });

            return tcs.Task;
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (dynamic item in this.lstMovies.Items)
            {
                this.itemsAsString.Add(string.Format("{0} - {1}", item.Date, item.Title));
            }

            ((Popup)this.Parent).IsOpen = false;
        }
    }
}
