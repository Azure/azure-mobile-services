using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Specialized;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    [Activity]
    public class HarnessActivity : Activity
    {
        private ExpandableListView list;
        private TextView runStatus;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            Title = "C# Client Library Tests";

            RequestWindowFeature (WindowFeatures.Progress);

            SetContentView (Resource.Layout.Harness);

            this.runStatus = FindViewById<TextView> (Resource.Id.RunStatus);

            this.list = FindViewById<ExpandableListView> (Resource.Id.List);
            this.list.SetAdapter (new TestListAdapter (this, App.Listener));
            this.list.ChildClick += (sender, e) => {
                Intent testIntent = new Intent (this, typeof(TestActivity));

                GroupDescription groupDesc = (GroupDescription)this.list.GetItemAtPosition (e.GroupPosition);
                TestDescription desc = groupDesc.Tests.ElementAt (e.ChildPosition);

                testIntent.PutExtra ("name", desc.Test.Name);
                testIntent.PutExtra ("desc", desc.Test.Description);
                testIntent.PutExtra ("log", desc.Log);

                StartActivity (testIntent);
            };

            SetProgressBarVisibility (true);
        }

        protected override void OnStart()
        {
            base.OnStart();

            App.Listener.PropertyChanged += OnListenerPropertyChanged;

            SetProgress (App.Listener.Progress);

            ShowStatus();
        }

        protected override void OnStop()
        {
            base.OnStop();

            App.Listener.PropertyChanged -= OnListenerPropertyChanged;
        }

        private void ShowStatus()
        {
            RunOnUiThread (() =>
            {
                if (String.IsNullOrWhiteSpace (App.Listener.Status))
                    return;

                Toast.MakeText (this, App.Listener.Status, ToastLength.Long)
                .Show ();
            });
        }

        private void UpdateProgress()
        {
            RunOnUiThread (() =>
            {
                SetProgress (App.Listener.Progress);

                this.runStatus.Text = String.Format ("Passed: {0}  Failed: {1}", App.Harness.Progress - App.Harness.Failures, App.Harness.Failures);
            });
        }

        private void OnListenerPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "Progress":
                    UpdateProgress();
                    break;

                case "Status":
                    ShowStatus();
                    break;
            }
        }

        class TestListAdapter
            : BaseExpandableListAdapter
        {
            public TestListAdapter (Activity activity, TestListener listener)
            {
                this.activity = activity;
                this.listener = listener;

                INotifyCollectionChanged changed = listener.Groups as INotifyCollectionChanged;
                if (changed != null)
                    changed.CollectionChanged += OnGroupsCollectionChanged;

                this.groups = new List<GroupDescription> (listener.Groups);
            }

            public override Java.Lang.Object GetChild (int groupPosition, int childPosition)
            {
                GroupDescription group = this.groups [groupPosition];
                return group.Tests.ElementAt (childPosition);
            }

            public override long GetChildId (int groupPosition, int childPosition)
            {
                return groupPosition * (childPosition * 2);
            }

            public override int GetChildrenCount (int groupPosition)
            {
                GroupDescription group = this.groups [groupPosition];
                return group.Tests.Count;
            }

            public override View GetChildView (int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
            {
                GroupDescription group = this.groups [groupPosition];
                TestDescription test = group.Tests.ElementAt (childPosition);

                View view = convertView;
                if (view == null)
                    view = this.activity.LayoutInflater.Inflate (Resource.Layout.ListedTest, null);

                TextView text = view.FindViewById<TextView> (Resource.Id.TestName);
                text.Text = test.Test.Name;

                if (!test.Test.Passed)
                    text.SetTextColor (Color.Red);
                else
                    text.SetTextColor (Color.White);

                return view;
            }

            public override Java.Lang.Object GetGroup (int groupPosition)
            {
                return this.groups [groupPosition];
            }

            public override long GetGroupId (int groupPosition)
            {
                return groupPosition;
            }

            public override View GetGroupView (int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
            {
                GroupDescription group = this.groups [groupPosition];

                View view = convertView;
                if (view == null)
                    view = this.activity.LayoutInflater.Inflate (Resource.Layout.ListedGroup, null);

                TextView text = view.FindViewById<TextView> (Resource.Id.TestName);
                text.Text = group.Group.Name;

                if (group.HasFailures)
                    text.SetTextColor (Color.Red);
                else
                    text.SetTextColor (Color.White);

                return view;
            }

            public override bool IsChildSelectable (int groupPosition, int childPosition)
            {
                return true;
            }

            public override int GroupCount
            {
                get { return this.groups.Count; }
            }

            public override bool HasStableIds
            {
                get { return false; }
            }

            private List<GroupDescription> groups;
            private Activity activity;
            private TestListener listener;

            void OnTestsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
            {
                this.activity.RunOnUiThread (() => {
                    NotifyDataSetChanged ();
                });
            }

            void OnGroupsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
            {
                this.activity.RunOnUiThread (() => {
                    foreach (INotifyCollectionChanged notify in this.groups.Select (g => g.Tests).OfType<INotifyCollectionChanged>())
                        notify.CollectionChanged -= OnTestsCollectionChanged;

                    this.groups = new List<GroupDescription> (this.listener.Groups);
                    foreach (INotifyCollectionChanged notify in this.groups.Select (g => g.Tests).OfType<INotifyCollectionChanged>())
                        notify.CollectionChanged += OnTestsCollectionChanged;

                    NotifyDataSetChanged ();
                });
            }
        }
    }
}