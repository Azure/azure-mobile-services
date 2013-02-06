using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ZUMOAPPNAME
{    
    public class TodoItem
    {
        public int Id { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "complete")]
        public bool Complete { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        // MobileServiceCollectionView implements ICollectionView (useful for databinding to lists) and 
        // is integrated with your Mobile Service to make it easy to bind your data to the ListView
        private MobileServiceCollectionView<TodoItem> items;

        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await todoTable.InsertAsync(todoItem);
            items.Add(todoItem);                        
        }

        private void RefreshTodoItems()
        {
            // This code refreshes the entries in the list view be querying the TodoItems table.
            // The query excludes completed TodoItems
            items = todoTable
                .Where(todoItem => todoItem.Complete == false)
                .ToCollectionView();
            ListItems.ItemsSource = items;
        }

        private async void UpdateCheckedTodoItem(TodoItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await todoTable.UpdateAsync(item);
            items.Remove(item);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTodoItems();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new TodoItem { Text = TextInput.Text };
            InsertTodoItem(todoItem);
        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            UpdateCheckedTodoItem(item);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshTodoItems();
        }
    }
}
