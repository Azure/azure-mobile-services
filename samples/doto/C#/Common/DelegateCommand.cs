using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Doto
{
    /// <summary>
    /// Represents a command for use in databinding and MVVM, supports enable, disable
    /// and execution
    /// </summary>
    public class DelegateCommand : DelegateCommand<object>, INotifyPropertyChanged
    {
        public DelegateCommand(Action execute, bool isEnabled = true)
            : base(o => execute())
        {
            IsEnabled = isEnabled;
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
                RaiseCanExecuteChanged();
            }
        }

        public void Execute()
        {
            Execute(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler pceh = PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Represents a command for use in databinding and MVVM, supports enable, disable
    /// and execution with a command parameter
    /// </summary>
    /// <typeparam name="T">The expected type of the command parameter</typeparam>
    public class DelegateCommand<T> : ICommand where T : class
    {
        protected bool _isEnabled = true;
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (t => _isEnabled);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
