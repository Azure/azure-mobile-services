// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// UI model for a test group.
    /// </summary>
    public class GroupDescription : ObservableCollection<TestDescription>
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public GroupDescription()
        {
        }
    }
}
