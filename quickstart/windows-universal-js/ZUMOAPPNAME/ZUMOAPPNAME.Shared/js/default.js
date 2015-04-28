﻿// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkId=232509
(function () {
    "use strict";

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize
                // your application here.
            } else {
                // TODO: This application has been reactivated from suspension.
                // Restore application state here.
            }
            args.setPromise(WinJS.UI.processAll());

			// This MobileServiceClient has been configured to communicate with your Mobile Service's url
			// and application key. You're all set to start working with your Mobile Service!
			var client = new WindowsAzure.MobileServiceClient(
				"ZUMOAPPURL",
				"ZUMOAPPKEY"
			);

            // The WinStore/JavaScript client by default caches the responses to GET requests;
            // since we want the 'Refresh' button to always retrieve the latest data from the
            // service, we'll add this header to force it to always go back to the service.
            var noCachingFilter = function (request, next, callback) {
                if (request.type === 'GET' && !request.headers['If-Modified-Since']) {
                    request.headers['If-Modified-Since'] = 'Mon, 27 Mar 1972 00:00:00 GMT';
                }
                next(request, callback);
            };

            var todoTable = client.withFilter(noCachingFilter).getTable('TodoItem');

            var todoItems = new WinJS.Binding.List();

            var insertTodoItem = function (todoItem) {
                // This code inserts a new TodoItem into the database. When the operation completes
                // and Mobile Services has assigned an id, the item is added to the Binding List
                todoTable.insert(todoItem).done(function (item) {
                    todoItems.push(item);
                });
            };

            var refreshTodoItems = function () {
                // This code refreshes the entries in the list view be querying the TodoItems table.
                // The query excludes completed TodoItems                
                todoTable.where({ complete: false })
                    .read()
                    .done(function (results) {
                        todoItems = new WinJS.Binding.List(results);
                        listItems.winControl.itemDataSource = todoItems.dataSource;
                    });
            };

            var updateCheckedTodoItem = function (todoItem) {
                // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
                // responds, the item is removed from the list 
                todoTable.update(todoItem).done(function (item) {
                    todoItems.splice(todoItems.indexOf(item), 1);
                });
            };

            buttonSave.addEventListener("click", function () {
                insertTodoItem({
                    text: textInput.value,
                    complete: false
                });
            });

            buttonRefresh.addEventListener("click", function () {
                refreshTodoItems();
            });

            listItems.addEventListener("change", function (eventArgs) {
                var todoItem = eventArgs.target.dataContext.backingData;
                todoItem.complete = eventArgs.target.checked;
                updateCheckedTodoItem(todoItem);

            });

            refreshTodoItems();
        }
    };

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state
        // that needs to persist across suspensions here. You might use the
        // WinJS.Application.sessionState object, which is automatically
        // saved and restored across suspension. If you need to complete an
        // asynchronous operation before your application is suspended, call
        // args.setPromise().
    };

    app.start();
})();
