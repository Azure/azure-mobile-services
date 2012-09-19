using Doto.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Doto.Controls
{
    public sealed partial class InviteUser : UserControl
    {
        public InviteUser()
        {
            this.InitializeComponent();
        }

        private void UsersList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var source = e.OriginalSource as FrameworkElement;
            Profile user = source.DataContext as Profile;
            if (user == null)
            {
                // didn't tap an item
                return;
            }
            ((InviteUserViewModel)DataContext).InviteUser(user);
        }
    }
}
