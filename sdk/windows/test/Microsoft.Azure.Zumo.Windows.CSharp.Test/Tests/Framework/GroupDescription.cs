// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    /// <summary>
    /// UI model for a test group.
    /// </summary>
    public class GroupDescription : INotifyPropertyChanged
    {
        private string _name;
        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TestDescription> Tests { get; private set; }

        public GroupDescription()
        {
            Tests = new ObservableCollection<TestDescription>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string memberName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
