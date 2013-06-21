// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ZumoE2ETestAppWP8.UIElements
{
    public partial class InputDialog : UserControl
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public static Task<bool> DisplayYesNo(string text)
        {
            Popup popup = new Popup();
            popup.Height = 240;
            popup.Width = 480;
            popup.VerticalOffset = 100;
            popup.VerticalAlignment = VerticalAlignment.Center;
            InputDialog dialog = new InputDialog();
            dialog.lblTitle.Text = "Question";
            dialog.txtContent.Text = text;
            dialog.btnCancel.Content = "No";
            dialog.btnOk.Content = "Yes";
            popup.Child = dialog;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            dialog.btnCancel.Click += (s, ea) =>
            {
                tcs.SetResult(false);
                popup.IsOpen = false;
            };

            dialog.btnOk.Click += (s, ea) =>
            {
                tcs.SetResult(true);
                popup.IsOpen = false;
            };

            popup.IsOpen = true;
            return tcs.Task;
        }

        public static Task<string> Display(string title, string text = null)
        {
            Popup popup = new Popup();
            popup.Height = 240;
            popup.Width = 480;
            popup.VerticalOffset = 100;
            popup.VerticalAlignment = VerticalAlignment.Center;
            InputDialog dialog = new InputDialog();
            dialog.lblTitle.Text = title;
            dialog.txtContent.Text = text ?? "";
            popup.Child = dialog;
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            dialog.btnCancel.Click += (s, ea) =>
            {
                tcs.SetResult(null);
                popup.IsOpen = false;
            };

            dialog.btnOk.Click += (s, ea) =>
            {
                tcs.SetResult(dialog.txtContent.Text);
                popup.IsOpen = false;
            };

            popup.IsOpen = true;
            return tcs.Task;
        }
    }
}
