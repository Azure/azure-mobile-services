using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    sealed class GroupDescription
        : Java.Lang.Object
    {
        public GroupDescription (TestGroup group)
        {
            Group = group;
            this.tests.CollectionChanged += (sender, e) => HasFailures = this.tests.Any (t => !t.Test.Passed);
        }

        public TestGroup Group
        {
            get;
            private set;
        }

        public bool HasFailures
        {
            get;
            private set;
        }

        public ICollection<TestDescription> Tests
        {
            get { return this.tests; }
        }

        private readonly ObservableCollection<TestDescription> tests = new ObservableCollection<TestDescription>();
    }
}