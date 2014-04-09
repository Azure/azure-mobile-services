// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoviesDisplayControl : UserControl
    {
        private List<string> itemsAsString;

        public MoviesDisplayControl()
        {
            this.InitializeComponent();

            var bounds = Application.Current.MainWindow.RenderSize;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;

            this.itemsAsString = new List<string>();
        }

        internal void SetMoviesSource(IEnumerable itemsSource)
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
            popup.PlacementTarget = Application.Current.MainWindow;
            popup.Placement = PlacementMode.Center;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            popup.IsOpen = true;
            popup.Closed += (snd, ea) =>
            {
                tcs.SetResult(true);
            };

            // Close the dialog after a couple of seconds, saving the items in the list
            var threadProc = new ThreadStart(async delegate
            {
                Thread.CurrentThread.Join(3000);

                await this.Dispatcher.BeginInvoke(new Action(delegate
                {
                    this.btnClose_Click_1(this.btnClose, new RoutedEventArgs());
                }));
            });
            new Thread(threadProc).Start();

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
