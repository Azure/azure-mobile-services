using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Doto.Model;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Live;
using Windows.Networking.PushNotifications;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Doto
{
    /// <summary>
    /// ViewModel that drives the majority of the logic behind the MainPage in Doto
    /// including Login, and Registration.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        // Enter the url and application key below...
        private const string _mobileServiceUrl = "enter url here";
        private const string _mobileServiceKey = "enter key here";

        private SynchronizationContext _synchronizationContext;
        private MobileServiceClient _mobileServiceClient;
        private IMobileServiceTable<Setting> _settingsTable;
        private IMobileServiceTable<Invite> _invitesTable;
        private IMobileServiceTable<ListMembership> _listMembersTable;
        private IMobileServiceTable<Device> _devicesTable;
        private IMobileServiceTable<Profile> _profilesTable;
        private IMobileServiceTable<Item> _itemsTable;
        private LiveAuthClient _liveAuthClient;
        private List<Item> _selectedItems;
        private IPopupService _popupService;

        // Doto doesn't support pagination of list items, instead there is an implicit maximum of 
        // 250 items in a list that can be viewed.
        private const int _maxListSize = 250;

        public MainViewModel(IPopupService popupService, SynchronizationContext synchonizationContext)
        {
            var client = new MobileServiceClient(
                _mobileServiceUrl,
                _mobileServiceKey);

            _liveAuthClient = new LiveAuthClient(_mobileServiceUrl);
            
            // Apply a ServiceFilter to the mobile client to help with our busy indication
            _mobileServiceClient = client.WithFilter(new DotoServiceFilter(
                busy =>
                {
                    IsBusy = busy;
                }));
            _popupService = popupService;
            _synchronizationContext = synchonizationContext;
            _invitesTable = _mobileServiceClient.GetTable<Invite>();
            _itemsTable = _mobileServiceClient.GetTable<Item>();
            _profilesTable = _mobileServiceClient.GetTable<Profile>();
            _listMembersTable = _mobileServiceClient.GetTable<ListMembership>();
            _devicesTable = _mobileServiceClient.GetTable<Device>();
            _settingsTable = _mobileServiceClient.GetTable<Setting>();

            SetupCommands();

            LoadSettings();
        }

        /// <summary>
        /// Loads the changeable application settings (accent color and background image)
        /// from Mobile Services
        /// </summary>
        private async void LoadSettings()
        {
            var settings = await _settingsTable.ToListAsync();
            if (settings.Count == 0)
            {
                // no settings
                return; 
            }
            AccentColor = settings.Single(s => s.Key == "accentColor").Value;
            BackgroundImage = settings.Single(s => s.Key == "backgroundImage").Value;
        }

        /// <summary>
        /// Sets up the execution logic for all the DelegateCommands in the ViewModel
        /// </summary>
        private void SetupCommands()
        {
            Dismiss userSearchDismiss = null;
            Dismiss viewInvitesDismiss = null;
            Dismiss showNewListDismiss = null;
            Dismiss showChooseListDismiss = null;

            RemoveItemsCommand = new DelegateCommand(async () =>
            {
                foreach (Item item in _selectedItems)
                {
                    try
                    {
                        await _itemsTable.DeleteAsync(item);
                    }
                    catch (MobileServiceInvalidOperationException exc)
                    {
                        // a 404 is an expected scenario here, another user 
                        // has most likely deleted the item whilst we've been
                        // viewing. 
                        if (exc.Response.StatusCode != 404)
                        {
                            throw;
                        }
                    }
                    Items.Remove(item);
                }
            }, false);

            ShowUserSearchCommand = new DelegateCommand(() =>
            {
                var inviteUserViewModel = new InviteUserViewModel(this, _profilesTable, () => userSearchDismiss.DismissFlyout());
                userSearchDismiss = _popupService.ShowInviteUserFlyout(inviteUserViewModel);
            }, false);

            ViewInvitesCommand = new DelegateCommand(() =>
            {
                var viewInvitesViewModel = new ViewInvitesViewModel(this, _invitesTable, () => viewInvitesDismiss.DismissFlyout());
                viewInvitesDismiss = _popupService.ShowViewInvitesFlyout(viewInvitesViewModel);
            }, false);

            ShowNewListCommand = new DelegateCommand(() =>
            {
                showNewListDismiss = _popupService.ShowNewListFlyout();
            }, false);

            ShowChooseListCommand = new DelegateCommand(() =>
            {
                showChooseListDismiss = _popupService.ShowChooseListFlyout();
            });

            ShowAddItemCommand = new DelegateCommand(() =>
            {
                _popupService.ShowAddItemFlyout();
            }, false);

            RefreshCommand = new DelegateCommand(() =>
            {
                LoadSettings();
                LoadInvites();
                if (CurrentList != null)
                {
                    LoadLists();
                    LoadItems();
                }
            }, false);

            LeaveListCommand = new DelegateCommand(() =>
            {
                _popupService.ShowDialogAsync(
                    "Leave list?",
                    "Are you sure you want to leave this list? If you're the only member, all data will be lost forever.",
                    new SimpleCommand("OK", async () =>
                    {
                        await _listMembersTable.DeleteAsync(CurrentList);
                        Lists.Remove(CurrentList);
                        ShowList(0);
                    }),
                    new SimpleCommand("Cancel", () => { }));
            }, false);

            RegisterCommand = new DelegateCommand(async () =>
            {
                if (string.IsNullOrWhiteSpace(User.Name) ||
                    string.IsNullOrWhiteSpace(User.City) ||
                    string.IsNullOrWhiteSpace(User.State))
                {
                    await _popupService.ShowDialogAsync("All fields required", "A valid name, city and state/county must be provided");
                    return;
                }
                await _profilesTable.InsertAsync(User);
                DisplayRegistrationForm = false;
                RegisterDevice();
                LoadLists(true);
                LoadInvites();
            }, true);

            AddItemCommand = new DelegateCommand(async () =>
            {
                if (String.IsNullOrWhiteSpace(NewItemText))
                {
                    await _popupService.ShowDialogAsync("Description is required", "You must enter a valid description for your item");
                    return;
                }
                var item = new Item
                {
                    Text = NewItemText,
                    ListId = CurrentList.ListId,
                    CreatedBy = User.Name,
                };
                NewItemText = String.Empty;
                await _itemsTable.InsertAsync(item);
                Items.Add(item);
            });

            SelectListCommand = new DelegateCommand<ListMembership>(lm =>
            {
                ShowList(Lists.IndexOf(lm));
                showChooseListDismiss.DismissFlyout();
            });

            NewListCommand = new DelegateCommand(async () =>
            {
                if (String.IsNullOrWhiteSpace(NewListName))
                {
                    await _popupService.ShowDialogAsync("Name is required", "You must enter a valid name for your list");
                    return;
                }
                ListMembership membership = new ListMembership
                {
                    Name = NewListName,
                    UserId = _mobileServiceClient.CurrentUser.UserId,
                };
                NewListName = String.Empty;
                await _listMembersTable.InsertAsync(membership);
                Lists.Add(membership);
                ShowList(Lists.IndexOf(membership));
                showNewListDismiss.DismissFlyout();
            });
        }

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand ShowAddItemCommand { get; private set; }
        public DelegateCommand NewListCommand { get; private set; }
        public DelegateCommand ShowNewListCommand { get; private set; }
        public DelegateCommand ShowChooseListCommand { get; private set; }
        public DelegateCommand ViewInvitesCommand { get; private set; }
        public DelegateCommand ShowUserSearchCommand { get; private set; }
        public DelegateCommand RemoveItemsCommand { get; private set; }
        public DelegateCommand RegisterCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand LeaveListCommand { get; private set; }
        public DelegateCommand<ListMembership> SelectListCommand { get; private set; }

        /// <summary>
        /// Kickstarts the ViewModel into life
        /// </summary>
        public async void Initialize()
        {
            await Login();
        }

        /// <summary>
        /// Clears all lists, sets the current list and user to null 
        /// </summary>
        public void Reset()
        {
            Lists.Clear();
            Items.Clear();
            Invites.Clear();
            CurrentList = null;
            User = null;
        }

        /// <summary>
        /// Performs login via Live Connect and then via Mobile Services
        /// </summary>
        private async Task Login()
        {
            // Force the user to login on app startup
            LiveLoginResult result = await _liveAuthClient.LoginAsync(new string[] { "wl.signin", "wl.basic", "wl.postal_addresses" });

            if (result.Session == null)
            {
                await _popupService.ShowDialogAsync("You must login to use doto.", "Not logged in");
                SetViewState(ViewState.LoggedOut);
                return;
            }

            await _mobileServiceClient.LoginAsync(result.Session.AuthenticationToken);
            var profiles = await _profilesTable.Where(p => p.UserId == _mobileServiceClient.CurrentUser.UserId).ToListAsync();

            if (profiles.Count == 0)
            {
                LiveConnectClient lcc = new LiveConnectClient(result.Session);
                var me = await lcc.GetAsync("me");
                dynamic pic = await lcc.GetAsync("me/picture");
                RegisterUser(me.Result, pic.Result, _mobileServiceClient.CurrentUser.UserId);
            }
            else
            {
                User = profiles.First();
                RegisterDevice();
                LoadLists(true);
                LoadInvites();
            }

            return;
        }

        /// <summary>
        /// Loads the invites from Mobile Services invites table
        /// </summary>
        private async void LoadInvites()
        {
            IEnumerable<Invite> invites = await _invitesTable
                .Where(i => i.ToUserId == User.UserId)
                .ToEnumerableAsync();

            Invites.Clear();
            Invites.AddRange(invites);

            ViewInvitesCommand.IsEnabled = Invites.Count > 0;
        }

        /// <summary>
        /// Loads Items from the current list from the Mobile Services items table
        /// </summary>
        private async void LoadItems()
        {
            List<Item> newItems = await _itemsTable.Where(i => i.ListId == CurrentList.ListId).Take(_maxListSize).ToListAsync();
 
            // Perform a simple sync of the existing list using the Ids.
            Queue<Item> matched = new Queue<Item>();
            Queue<Item> removals = new Queue<Item>();

            foreach (var item in Items)
            {
                bool didMatch = false;

                foreach (var newItem in newItems)
                {
                    if (newItem.Id == item.Id)
                    {
                        matched.Enqueue(newItem);
                        didMatch = true;
                        break;
                    }
                }

                // If no match, we should remove from Items collection
                if (!didMatch)
                {
                    removals.Enqueue(item);
                }

                // remove new items as quickly as soon as they're matched 
                while (matched.Count > 0)
                {
                    newItems.Remove(matched.Dequeue());
                }
            }

            while (removals.Count > 0)
            {
                Items.Remove(removals.Dequeue());
            }

            // add any remaining newItems - the must be genuinely new items
            Items.AddRange(newItems);
        }

        /// <summary>
        /// Load all the users lists
        /// </summary>
        /// <param name="showFirstList">An optional parameter that can force the method to load the 0th listas the current</param>
        public async void LoadLists(bool showFirstList = false)
        {
            IEnumerable<ListMembership> lists = await _listMembersTable.
                Where(m => m.UserId == User.UserId).ToEnumerableAsync();
            Lists.Clear();
            Lists.AddRange(lists);
            if (showFirstList)
            {
                ShowList(0);
            }
        }

        /// <summary>
        /// Sets the current list to specified index. If there are no lists, switches ViewModel
        /// to the NoLists ViewState
        /// </summary>
        public void ShowList(int activeListIndex)
        {
            if (Lists.Count == 0)
            {
                CurrentList = null;
                SetViewState(ViewState.NoLists);
            }
            else
            {
                SetViewState(ViewState.ListSelected);
                CurrentList = Lists[activeListIndex];
                LoadItems();
            }
        }

        /// <summary>
        /// Registers the user's profile with Mobile Services
        /// </summary>
        /// <param name="liveProfile">The response from Live Connect to get '/me'</param>
        /// <param name="livePicture">The response from Live Connect to get '/me/picture'</param>
        /// <param name="userId"></param>
        private void RegisterUser(dynamic liveProfile, dynamic livePicture, string userId)
        {
            User = new Profile
            {
                Name = liveProfile.name ?? "",
                UserId = userId,
                City = liveProfile.addresses.personal.city ?? "",
                State = liveProfile.addresses.personal.state ?? "",
                ImageUrl = livePicture.location
            };
            DisplayRegistrationForm = true;
        }

        /// <summary>
        /// Requests a push channel url and uploads this to Mobile Services
        /// devices table
        /// </summary>
        private async void RegisterDevice()
        {
            PushNotificationChannel channel =
                await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            channel.PushNotificationReceived += channel_PushNotificationReceived;

            string installationId = await InstallationId.GetInstallationId();

            await _devicesTable.InsertAsync(
                    new Device
                    {
                        UserId = User.UserId,
                        ChannelUri = channel.Uri.ToString(),
                        InstallationId = installationId
                    });
        }

        /// <summary>
        /// Handles a push notification being received whilst the app is live. If we receive a toast,
        /// it is likely we have received an invite. If we receive a tile, it's probably that we
        /// have a new item.
        /// </summary>
        void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            _synchronizationContext.Post(ignored =>
            {
                if (args.NotificationType == PushNotificationType.Toast)
                {
                    // we received a toast notification - let's check for invites
                    LoadInvites();
                }
                else if (args.NotificationType == PushNotificationType.Tile)
                {
                    // we received a tile - reload current items in case it was for this list
                    LoadItems();
                }
            }, null);
        }

        /// <summary>
        /// Sets the view into a particular state, enabling and disabling all controls
        /// based on that state.
        /// </summary>
        private void SetViewState(ViewState viewState)
        {
            switch (viewState)
            {
                case ViewState.LoggedOut:
                    _synchronizationContext.Post(async ignored =>
                    {
                        Reset();
                        ViewInvitesCommand.IsEnabled = false;
                        RemoveItemsCommand.IsEnabled = false;
                        ShowUserSearchCommand.IsEnabled = false;
                        ShowNewListCommand.IsEnabled = false;
                        ShowAddItemCommand.IsEnabled = false;
                        LeaveListCommand.IsEnabled = false;
                        RefreshCommand.IsEnabled = false;
                        await Login();
                    }, null);
                    break;
                case ViewState.NoLists:
                    ShowNewListCommand.IsEnabled = true;
                    ShowAddItemCommand.IsEnabled = false;
                    ViewInvitesCommand.IsEnabled = false;
                    RemoveItemsCommand.IsEnabled = false;
                    ShowUserSearchCommand.IsEnabled = false;
                    LeaveListCommand.IsEnabled = false;
                    RefreshCommand.IsEnabled = true;
                    break;
                case ViewState.ListSelected:
                    ShowNewListCommand.IsEnabled = true;
                    ShowAddItemCommand.IsEnabled = true;
                    RefreshCommand.IsEnabled = true;
                    ShowUserSearchCommand.IsEnabled = true;
                    LeaveListCommand.IsEnabled = true;
                    ViewInvitesCommand.IsEnabled = false;
                    break;
                default:
                    break;
            }
            ViewState = viewState;
        }

        public IPopupService PopupService
        {
            get { return _popupService; }
        }

        private Profile _user;

        public Profile User
        {
            get { return _user; }
            set
            {
                SetValue(ref _user, value, "User");
            }
        }

        private readonly ObservableCollection<ListMembership> _lists = new ObservableCollection<ListMembership>();

        public ObservableCollection<ListMembership> Lists
        {
            get { return _lists; }
        }

        private string _accentColor = "#63a9eb";

        public string AccentColor
        {
            get { return _accentColor; }
            set
            {
                SetValue(ref _accentColor, value, "AccentColor");
            }
        }

        private string _backgroundImage;

        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                SetValue(ref _backgroundImage, value, "BackgroundImage");
            }
        }

        private string _newItemText;

        public string NewItemText
        {
            get { return _newItemText; }
            set
            {
                SetValue(ref _newItemText, value, "NewItemText");
            }
        }

        private string _newListName;

        public string NewListName
        {
            get { return _newListName; }
            set
            {
                SetValue(ref _newListName, value, "NewListName");
            }
        }
        
        private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();

        public ObservableCollection<Item> Items
        {
            get { return _items; }
        }

        private readonly ObservableCollection<Invite> _invites = new ObservableCollection<Invite>();

        public ObservableCollection<Invite> Invites
        {
            get { return _invites; }
        }

        private ListMembership _currentList;

        public ListMembership CurrentList
        {
            get { return _currentList; }
            set
            {
                SetValue(ref _currentList, value, "CurrentList");
            }
        }

        private ViewState _viewState;

        public ViewState ViewState
        {
            get { return _viewState; }
            set
            {
                SetValue(ref _viewState, value, "ViewState");
            }
        }

        private bool _displayRegistrationForm = false;

        public bool DisplayRegistrationForm
        {
            get { return _displayRegistrationForm; }
            set
            {
                SetValue(ref _displayRegistrationForm, value, "DisplayRegistrationForm");
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetValue(ref _isBusy, value, "IsBusy");
            }
        }

        /// <summary>
        /// Returns a list of SimpleCommands based on the apps state. This is used
        /// to populate the Settings panel.
        /// </summary>
        public List<SimpleCommand> GetSettingsCommands()
        {
            List<SimpleCommand> commands = new List<SimpleCommand>();

            // if no user, no need to return any commands
            if (_mobileServiceClient.CurrentUser == null || _liveAuthClient.CanLogout == false)
            {
                return commands;
            }

            // Create the sign out command
            SimpleCommand signOut = new SimpleCommand();
            signOut.Text = string.Format("Sign Out {0}", User.Name);
            signOut.Action = () =>
            {
                _mobileServiceClient.Logout();
                _liveAuthClient.Logout();
                SetViewState(ViewState.LoggedOut);
            };
            commands.Add(signOut);
            return commands;
        }

        /// <summary>
        /// Allows the View to communicate with the ViewModel when 
        /// items are selected (as binding is not supported for this)
        /// </summary>
        public void SetSelectedItems(List<Item> selectedItems)
        {
            _selectedItems = selectedItems;
            RemoveItemsCommand.IsEnabled = selectedItems.Count > 0;
        }

        /// <summary>
        /// Invites a user to join the current list
        /// </summary>
        public async Task InviteUser(Profile user)
        {
            await _invitesTable.InsertAsync(new Invite()
            {
                FromUserId = User.UserId,
                FromUserName = User.Name,
                FromImageUrl = User.ImageUrl,
                ToUserId = user.UserId,
                ToUserName = user.Name,
                ListId = CurrentList.ListId,
                ListName = CurrentList.Name
            });

            await _popupService.ShowDialogAsync("Invite Sent", string.Format("Invite sent to {0}", user.Name));
            return;
        }

        /// <summary>
        /// Displays a simple 'ERROR' dialog to the user
        /// </summary>
        public void ShowError(string message)
        {
            _synchronizationContext.Post(ignored =>
            {
                _popupService.ShowDialogAsync("ERROR", message);
            }, null);
        }
    }
}
