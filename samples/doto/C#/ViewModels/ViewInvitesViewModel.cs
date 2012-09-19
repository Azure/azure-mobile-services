using Doto.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doto
{
    /// <summary>
    /// ViewMoel that implements the logic for the View Invites dialog
    /// </summary>
    public class ViewInvitesViewModel : ViewModel
    {
        private IMobileServiceTable<Invite> _invitesTable;
        private MainViewModel _parent;

        public ViewInvitesViewModel(MainViewModel parent, IMobileServiceTable<Invite> invitesTable, Action dismiss)
        {
            _parent = parent;
            _invitesTable = invitesTable;
            Invites = parent.Invites;

            AcceptCommand = new DelegateCommand<Invite>(async invite =>
            {
                invite.Approved = true;
                await _invitesTable.UpdateAsync(invite);
                Invites.Remove(invite);
                _parent.LoadLists();
                _parent.ViewInvitesCommand.IsEnabled = Invites.Count > 0;
                if (Invites.Count == 0 )
                {
                    dismiss();
                }
            });

            RejectCommand = new DelegateCommand<Invite>(async invite =>
            {
                await _invitesTable.UpdateAsync(invite);
                Invites.Remove(invite);
                _parent.ViewInvitesCommand.IsEnabled = Invites.Count > 0;
                if (Invites.Count == 0)
                {
                    dismiss();
                }
            });
        }

        public ObservableCollection<Invite> Invites { get; private set; }

        public DelegateCommand<Invite> AcceptCommand { get; private set; }
        public DelegateCommand<Invite> RejectCommand { get; private set; }
    }
}
