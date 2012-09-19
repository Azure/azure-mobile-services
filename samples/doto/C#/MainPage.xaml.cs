using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Callisto.Controls;
using Doto.Model;
using Microsoft.Live;
using Microsoft.Live.Controls;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using Doto.Controls;
using Windows.UI.ViewManagement;
using System.Diagnostics;

namespace Doto
{
    /// <summary>
    /// The main page, where it all happens inside Doto
    /// </summary>
    public sealed partial class MainPage : Page, IPopupService
    {
        private MainViewModel _viewModel;
        private SynchronizationContext _synchronizationContext;

        public MainPage()
        {
            InitializeComponent();
            _synchronizationContext = SynchronizationContext.Current;
            DataContext = _viewModel = new MainViewModel(this, _synchronizationContext);
            _viewModel.Initialize();

            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            this.SizeChanged += MainPage_SizeChanged;

            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (String.Equals(e.PropertyName, "ViewState"))
                {
                    SwitchViewState(_viewModel.ViewState);
                }
            };

            listView.SelectionChanged += (sender, e) =>
            {
                IList<object> selectedItems = (sender as ListView).SelectedItems;
                _viewModel.SetSelectedItems(selectedItems.Select(i => (Item)i).ToList());
            };
        }

        /// <summary>
        /// Called when the size of the app changes, and monitors for snapping. When the
        /// app is snapped - a number of features are removed to keep the app functional and
        /// usable
        /// </summary>
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var view = Windows.UI.ViewManagement.ApplicationView.Value;

            buttonAddItem.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            buttonInviteUser.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            buttonLeaveList.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            buttonNewList.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            buttonPendingInvites.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            panelUser.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            textTitle.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            sepAppBar2.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
            sepAppBar1.Visibility = view == ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Invoked by Windows if the user accesses the Settings panel in the app.
        /// </summary>
        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var commands = _viewModel.GetSettingsCommands();

            foreach (var command in commands)
            {
                args.Request.ApplicationCommands.Add(new SettingsCommand(command, command.Text, new UICommandInvokedHandler(ignored =>
                {
                    command.Action();
                })));
            }
        }

        /// <summary>
        /// Flips the visibility of some items in the view based on the ViewModel's state
        /// </summary>
        private void SwitchViewState(ViewState state)
        {
            loggedOutView.Visibility = state == ViewState.LoggedOut ? Visibility.Visible : Visibility.Collapsed;
            listView.Visibility = state == ViewState.ListSelected ? Visibility.Visible : Visibility.Collapsed;
            noListView.Visibility = state == ViewState.NoLists ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task ShowDialogAsync(string title, string message, params SimpleCommand[] commands)
        {
            var dialog = new MessageDialog(message, title);
            foreach (var command in commands)
            {
                dialog.Commands.Add(new UICommand(command.Text, x =>
                {
                    command.Action();
                }));
            }
            await dialog.ShowAsync();
            return;
        }

        public Dismiss ShowInviteUserFlyout(InviteUserViewModel viewModel)
        {
            Flyout flyout = CreateFlyout();
            var control = new InviteUser();
            control.DataContext = viewModel;
            flyout.Content = control;
            flyout.Placement = PlacementMode.Top;
            flyout.PlacementTarget = buttonInviteUser;
            flyout.IsOpen = true;
            return new Dismiss(() => flyout.IsOpen = false);
        }

        public Dismiss ShowViewInvitesFlyout(ViewInvitesViewModel viewModel)
        {
            Flyout flyout = CreateFlyout();
            var control = new ViewInvites();
            control.DataContext = viewModel;
            flyout.Content = control;
            flyout.Placement = PlacementMode.Top;
            flyout.PlacementTarget = buttonPendingInvites;
            flyout.IsOpen = true;
            return new Dismiss(() => flyout.IsOpen = false);
        }

        public Dismiss ShowAddItemFlyout()
        {
            Flyout flyout = CreateFlyout();
            var cp = new ContentPresenter { 
                Content = _viewModel, 
                ContentTemplate = (DataTemplate) App.Current.Resources["AddItemTemplate"]
            };
            flyout.Content = cp;
            flyout.Placement = PlacementMode.Top;
            flyout.PlacementTarget = buttonAddItem;
            flyout.IsOpen = true;

            return new Dismiss(() => flyout.IsOpen = false);
        }

        public Dismiss ShowNewListFlyout()
        {
            Flyout flyout = CreateFlyout();
            flyout.Content = new ContentPresenter
            {
                Content = _viewModel,
                ContentTemplate = (DataTemplate)App.Current.Resources["NewListTemplate"]
            };
            flyout.Placement = PlacementMode.Top;
            flyout.PlacementTarget = buttonNewList;
            flyout.IsOpen = true;
            return new Dismiss(() => flyout.IsOpen = false);
        }

        public Dismiss ShowChooseListFlyout()
        {
            Flyout flyout = CreateFlyout();
            flyout.Content = new ContentPresenter
            {
                Content = _viewModel,
                ContentTemplate = (DataTemplate)App.Current.Resources["ChooseListTemplate"]
            };
            flyout.Placement = PlacementMode.Bottom;
            flyout.PlacementTarget = buttonChooseList;
            flyout.IsOpen = true;
            return new Dismiss(() => flyout.IsOpen = false);
        }

        /// <summary>
        /// Creates a Flyout and parents it in the Visual Tree (to support on screen keyboard etc).
        /// </summary>
        private Flyout CreateFlyout()
        {
            Flyout flyout = new Flyout();
            flyout.BorderThickness = new Thickness(0);
            flyout.Background = new SolidColorBrush(Colors.Transparent);
            layoutRoot.Children.Add(flyout.HostPopup);
            return flyout;
        }
    }
}
