using Doto.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doto
{
    /// <summary>
    /// ViewModel that implements the logic behind the Invite Users dialog
    /// </summary>
    public class InviteUserViewModel : ViewModel
    {
        private int _pageCount = 4;
        private long _totalCount = 0;
        private int _currentPage = 0;
        private MobileServiceTableQuery<Profile> _query;
        private IMobileServiceTable<Profile> _profileTable;
        private MainViewModel _parent;
        private Action _dismiss;

        public InviteUserViewModel(MainViewModel parent, IMobileServiceTable<Profile> profileTable, Action dismiss)
        {
            _profileTable = profileTable;
            _parent = parent;
            _dismiss = dismiss;

            SearchCommand = new DelegateCommand(() =>
            {
                _currentPage = 0;
                if (String.IsNullOrWhiteSpace(SearchText))
                {
                    _query = profileTable.Take(_pageCount).IncludeTotalCount();
                }
                else
                {
                    _query = profileTable.Take(_pageCount).IncludeTotalCount().Where(p => p.Name.IndexOf(SearchText) > -1);
                }
                RefreshQuery();
            });

            NextCommand = new DelegateCommand(() =>
            {
                _currentPage++;
                RefreshQuery();
            }, false);

            PrevCommand = new DelegateCommand(() =>
            {
                _currentPage--;
                RefreshQuery();
            }, false);
        }

        private async void RefreshQuery()
        {
            // Temporarily disable the next/prev commands until this query finishes
            NextCommand.IsEnabled = false;
            PrevCommand.IsEnabled = false;
            List<Profile> profiles = await _query.Skip(_currentPage * _pageCount).ToListAsync();
            _totalCount = ((ITotalCountProvider)profiles).TotalCount;
            Users.Clear();
            Users.AddRange(profiles);
            EnableDisablePaging();
        }

        private void EnableDisablePaging()
        {
            NextCommand.IsEnabled = ((_currentPage + 1) * _pageCount) < _totalCount;
            PrevCommand.IsEnabled = _currentPage > 0;
            ShowPageControls = NextCommand.IsEnabled || PrevCommand.IsEnabled;
        }

        public async void InviteUser(Profile profile)
        {
            string message = string.Format("Are you sure you want to invite '{0}' to share the list '{1}'? They will have full permissions over the list, including the ability to invite other users",
                profile.Name,
                _parent.CurrentList.Name);

            await _parent.PopupService.ShowDialogAsync("Invite user?", message,
                new SimpleCommand("Yes", async () =>
                {
                    try
                    {
                        await _parent.InviteUser(profile);
                    }
                    catch (MobileServiceInvalidOperationException exc)
                    {
                        // The server validates invitations, handle expected responses
                        // and display a friendly message to the user
                        if (exc.Response.StatusCode == 400)
                        {
                            _parent.ShowError(exc.Response.Content);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    _dismiss();
                }),
                new SimpleCommand("Cancel", () =>
                {
                    // nothing to do
                }));
        
            
        }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                SetValue(ref _searchText, value, "SearchText");
            }
        }

        private readonly ObservableCollection<Profile> _users = new ObservableCollection<Profile>();

        public ObservableCollection<Profile> Users
        {
            get { return _users; }
        }

        private bool _showPageControls;

        public bool ShowPageControls
        {
            get { return _showPageControls; }
            set
            {
                SetValue(ref _showPageControls, value, "ShowPageControls");
            }
        }

        public DelegateCommand SearchCommand { get; private set; }
        public DelegateCommand NextCommand { get; private set; }
        public DelegateCommand PrevCommand { get; private set; }
    }
}
