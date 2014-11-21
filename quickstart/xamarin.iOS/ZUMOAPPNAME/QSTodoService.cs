using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace ZUMOAPPNAME
{
    public class QSTodoService : DelegatingHandler
    {
        static QSTodoService instance = new QSTodoService ();

        const string applicationURL = @"ZUMOAPPURL";
        const string applicationKey = @"ZUMOAPPKEY";

        private MobileServiceClient client;
        private IMobileServiceSyncTable<ToDoItem> todoTable;
        private int busyCount = 0;

        public event Action<bool> BusyUpdate;

        private QSTodoService ()
        {
            CurrentPlatform.Init ();
            SQLitePCL.CurrentPlatform.Init(); 

            // Initialize the Mobile Service client with your URL and key
            client = new MobileServiceClient (applicationURL, applicationKey, this);

            // Create an MSTable instance to allow us to work with the TodoItem table
            todoTable = client.GetSyncTable <ToDoItem> ();
        }

        public static QSTodoService DefaultService {
            get {
                return instance;
            }
        }

        public List<ToDoItem> Items { get; private set;}

        public async Task InitializeStoreAsync()
        {
            string path = "localstore.db";
            var store = new MobileServiceSQLiteStore(path);
            store.DefineTable<ToDoItem>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync. For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);
        }

        public async Task SyncAsync()
        {
            try
            {
                await client.SyncContext.PushAsync();
                await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
            }

            catch (MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine(@"Sync Failed: {0}", e.Message);
            }
        }

        public async Task<List<ToDoItem>> RefreshDataAsync ()
        {
            try {
                // update the local store
                await SyncAsync();

                // This code refreshes the entries in the list view by querying the local TodoItems table.
                // The query excludes completed TodoItems
                Items = await todoTable
                    .Where (todoItem => todoItem.Complete == false).ToListAsync ();

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
                return null;
            }

            return Items;
        }

        public async Task InsertTodoItemAsync (ToDoItem todoItem)
        {
            try {
                // This code inserts a new TodoItem into the database. When the operation completes
                // and Mobile Services has assigned an Id, the item is added to the CollectionView
                await todoTable.InsertAsync (todoItem);
                Items.Add (todoItem); 

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
            }
        }

        public async Task CompleteItemAsync (ToDoItem item)
        {
            try {
                // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
                // responds, the item is removed from the list 
                item.Complete = true;
                await todoTable.UpdateAsync (item);
                Items.Remove (item);

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
            }
        }

        private void Busy (bool busy)
        {
            // assumes always executes on UI thread
            if (busy) {
                if (busyCount++ == 0 && BusyUpdate != null)
                    BusyUpdate (true);

            } else {
                if (--busyCount == 0 && BusyUpdate != null)
                    BusyUpdate (false);

            }
        }

        #region implemented abstract members of HttpMessageHandler

        protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Busy (true);
            var response = await base.SendAsync (request, cancellationToken);

            Busy (false);
            return response;
        }

        #endregion
    }
}

