using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doto
{
    /// <summary>
    /// Since the ViewModels frequently need to interact with the View programmatically
    /// (not through binding) to display dialogs and flyouts, we use this interface
    /// to abstract the ViewModel from the specific implementation in the View
    /// </summary>
    public interface IPopupService
    {
        Task ShowDialogAsync(string title, string message, params SimpleCommand[] commands);
        Dismiss ShowInviteUserFlyout(InviteUserViewModel viewModel);
        Dismiss ShowViewInvitesFlyout(ViewInvitesViewModel viewInvitesViewModel);
        Dismiss ShowAddItemFlyout();
        Dismiss ShowNewListFlyout();
        Dismiss ShowChooseListFlyout();
    }

    public class Dismiss
    {
        private readonly Action _onDismiss;

        public Dismiss(Action onDismiss)
        {
            _onDismiss = onDismiss;
        }

        public void DismissFlyout() {
            _onDismiss();
        }
    }
}
