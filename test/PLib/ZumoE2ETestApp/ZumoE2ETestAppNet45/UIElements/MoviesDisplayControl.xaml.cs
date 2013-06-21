// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MoviesDisplayControl()
        {
            this.InitializeComponent();

            var bounds = Application.Current.MainWindow.RenderSize;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;
        }

        internal void SetMoviesSource(IEnumerable itemsSource)
        {
            this.lstMovies.ItemsSource = itemsSource;
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

            return tcs.Task;
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            ((Popup)this.Parent).IsOpen = false;
        }
    }
}
