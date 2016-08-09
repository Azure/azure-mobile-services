<properties
	pageTitle="Get Started with a Visual Studio .NET mobile services project (Connected Services) | Microsoft Azure"
	description="How to get started with Azure Mobile Services in a Visual Studio .NET project"
	services="mobile-services"
	documentationCenter=""
	authors="mlhoop"
	manager="douge"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="vs-getting-started"
	ms.devlang="dotnet"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="mlearned"/>

# Getting Started with Mobile Services (.NET Projects)

[AZURE.INCLUDE [mobile-service-note-mobile-apps](../../includes/mobile-services-note-mobile-apps.md)]

The first step you need to do in order to follow the code in these examples depends on what type of mobile service you connected to.

- For a JavaScript backend mobile service, create a table called TodoItem.  To create a table,  locate the mobile service under the Azure node in Server Explorer, right-click the mobile service's node to open the context menu, and choose **Create Table**. Enter "TodoItem" as the table name.

- If you have a .NET backend mobile service, there's already a TodoItem table in the default project template that Visual Studio created for you, but you need to publish it to Azure. To publish it, open the context menu for the mobile service project in Solution Explorer, and choose **Publish Web**. Accept the defaults, and choose the **Publish** button.

##Get a reference to a table

The following code creates a reference to a table (`todoTable`) that contains data for a TodoItem, which you can use in subsequent operations to read and update the data table. You'll need the TodoItem class with attributes set up to interpet the JSON that the mobile service sends in response to your queries.

	public class TodoItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }
    }

	IMobileServiceTable<TodoItem> todoTable = App.<yourClient>.GetTable<TodoItem>();

This code works if your table has permissions set to **Anybody with an Application Key**. If you change the permissions to secure your mobile service, you'll need to add user authentication support. See [Get Started with Authentication](mobile-services-dotnet-backend-windows-universal-dotnet-get-started-users.md).

##Add a table item

Insert a new item into a data table.

	TodoItem todoItem = new TodoItem() { Text = "My first to do item", Complete = false };
	await todoTable.InsertAsync(todoItem);

##Read or query a table

The following code queries a table for all items. Note that it returns only the first page of data, which by default is 50 items. You can pass the page size you want, since it's an optional parameter.

    List<TodoItem> items;
    try
    {
        // Query that returns all items.
        items = await todoTable.ToListAsync();
    }
    catch (MobileServiceInvalidOperationException e)
    {
        // handle exception
    }


##Update a table item

Update a row in a data table. The parameter item is the TodoItem object to be updated.

	await todoTable.UpdateAsync(item);

##Delete a table item

Delete a row in the database. The parameter item is the TodoItem object to be deleted.

	await todoTable.DeleteAsync(item);


[Learn more about mobile services](https://azure.microsoft.com/documentation/services/mobile-services/)
