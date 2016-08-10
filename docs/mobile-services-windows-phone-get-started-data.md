<properties
	pageTitle="Add Mobile Services to an existing app (WP8) | Microsoft Azure"
	description="Learn how to get started using data from your Azure Mobile Services Windows Phone 8 app."
	services="mobile-services"
	documentationCenter="windows"
	authors="ggailey777"
	manager="dwrede"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-windows-phone"
	ms.devlang="dotnet"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>


# Add Mobile Services to an existing app

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

&nbsp;


[AZURE.INCLUDE [mobile-services-selector-get-started-data](../../includes/mobile-services-selector-get-started-data.md)]

##Overview

This topic shows you how to use Azure Mobile Services to leverage data in a Windows Phone 8 app. In this tutorial, you will download an app that stores data in memory, create a new mobile service, integrate the mobile service with the app, and then login to the [Azure classic portal] to view changes to data made when running the app.

You can also see Nick Harris demonstrate this in the following video:
>[AZURE.VIDEO mobile-get-started-with-data-windows-phone]

##Prerequisites

+ Visual Studio 2012 Express for Windows Phone 8 and the [Windows Phone 8 SDK] running on Windows 8. To complete this tutorial to create a Windows Phone 8.1 app, you must use Visual Studio 2013 Update 2, or a later version.

+ An Azure account. If you don't have an account, you can create a free trial account in just a couple of minutes. For details, see [Azure Free Trial](https://azure.microsoft.com/pricing/free-trial/?WT.mc_id=A756A2826&amp;returnurl=http%3A%2F%2Fazure.microsoft.com%2Farticles%2Fdocumentation%2Fmobile-services-windows-phone-get-started-data%2F).

##<a name="download-app"></a>Download the GetStartedWithData project

This tutorial is built on the [GetStartedWithData app][Developer Code Samples site], which is a Windows Phone Silverlight 8 app project.

1. Download the GetStartedWithData sample app project from the [Developer Code Samples site].

	>[AZURE.NOTE]To create a Windows Phone Silverlght 8.1 app, just change the target OS in the downloaded Windows Phone Silverlight 8 app project to Windows Phone 8.1. To create a Windows Phone Store app, download the [Windows Phone Store app version](http://go.microsoft.com/fwlink/p/?LinkId=397372) of the GetStartedWithData sample app project.

2. In Visual Studio, open the downloaded project and examine the MainPage.xaml.cs file.

   	Notice that added **TodoItem** objects are stored in an in-memory **ObservableCollection&lt;TodoItem&gt;**.

3. Press the **F5** key to rebuild the project and start the app.

4. In the app, type some text in the text box, then click the **Save** button.

   	![][0]

   	Notice that the saved text is displayed in the list below.

##<a name="create-service"></a>Create a new mobile service in the Azure classic portal



Next, you will create a new mobile service to replace the in-memory list for data storage. Follow these steps to create a new mobile service.

1. Log into the [Azure classic portal](https://manage.windowsazure.com/). 
2.	At the bottom of the navigation pane, click **+NEW**.

	![plus-new](./media/mobile-services-create-new-service-data/plus-new.png)

3.	Expand **Compute** and **Mobile Service**, then click **Create**.

	![mobile-create](./media/mobile-services-create-new-service-data/mobile-create.png)

    This displays the **New Mobile Service** dialog.

4.	In the **Create a mobile service** page, select **Create a free 20 MB SQL Database**, then type a subdomain name for the new mobile service in the **URL** textbox and wait for name verification. Once name verification completes, click the right arrow button to go to the next page.	

	![mobile-create-page1](./media/mobile-services-create-new-service-data/mobile-create-page1.png)

    This displays the **Specify database settings** page.

    
	> [AZURE.NOTE] As part of this tutorial, you create a new SQL Database instance and server. You can reuse this new database and administer it as you would any other SQL Database instance. If you already have a database in the same region as the new mobile service, you can instead choose **Use existing Database** and then select that database. The use of a database in a different region is not recommended because of additional bandwidth costs and higher latencies.

5.	In **Name**, type the name of the new database, then type **Login name**, which is the administrator login name for the new SQL Database server, type and confirm the password, and click the check button to complete the process.

	![mobile-create-page2](./media/mobile-services-create-new-service-data/mobile-create-page2.png)

    
	> [AZURE.NOTE] When the password that you supply does not meet the minimum requirements or when there is a mismatch, a warning is displayed.  
	>
	> We recommend that you make a note of the administrator login name and password that you specify; you will need this information to reuse the SQL Database instance or the server in the future.

You have now created a new mobile service that can be used by your mobile apps. Next, you will add a new table in which to store app data. This table will be used by the app in place of the in-memory collection.


##<a name="add-table"></a>Add a new table to the mobile service

[AZURE.INCLUDE [mobile-services-create-new-service-data-2](../../includes/mobile-services-create-new-service-data-2.md)]

##<a name="update-app"></a>Update the app to use the mobile service for data access

Now that your mobile service is ready, you can update the app to store items in Mobile Services instead of the local collection.

1. In **Solution Explorer** in Visual Studio, right-click the project name, and then select **Manage NuGet Packages**.

2. In the left pane, select the **Online** category, search for `WindowsAzure.MobileServices`, click **Install** on the **Azure Mobile Services** package, then accept the license agreement.

  	![][7]

  	This adds the Mobile Services client library to the project.

3. In the [Azure classic portal], click **Mobile Services**, and then click the mobile service you just created.

4. Click the **Dashboard** tab and make a note of the **Site URL**, then click **Manage keys** and make a note of the **Application key**.

   	![][8]

  	You will need these values when accessing the mobile service from your app code.

5. In Visual Studio, open the file App.xaml.cs and add or uncomment the following `using` statement:

       	using Microsoft.WindowsAzure.MobileServices;

6. In this same file, uncomment the following code that defines the **MobileService** variable, and supply the URL and application key from the mobile service in the **MobileServiceClient** constructor, in that order.

		//public static MobileServiceClient MobileService = new MobileServiceClient(
        //    "AppUrl",
        //    "AppKey"
        //);

  	This creates a new instance of **MobileServiceClient** that is used to access your mobile service.

6. In the file MainPage.cs, add or uncomment the following `using` statements:

       	using Microsoft.WindowsAzure.MobileServices;
		using Newtonsoft.Json;

7. In this DataModel folder, replace the **TodoItem** class definition with the following code:

        public class TodoItem
        {
            public string Id { get; set; }

            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "complete")]
            public bool Complete { get; set; }
        }

7. Comment the line that defines the existing **items** collection, then uncomment the following lines:

        private MobileServiceCollection<TodoItem, TodoItem> items;
        private IMobileServiceTable<TodoItem> todoTable =
			App.MobileService.GetTable<TodoItem>();

   	This code creates a mobile services-aware binding collection (**items**) and a proxy class for the SQL Database table **TodoItem** (**todoTable**).

7. In the **InsertTodoItem** method, remove the line of code that sets the **TodoItem**.**Id** property, add the **async** modifier to the method, and uncomment the following line of code:

        await todoTable.InsertAsync(todoItem);

  	This code inserts a new item into the table.

8. In the **RefreshTodoItems** method, add the **async** modifier to the method, then uncomment the following line of code:

        items = await todoTable.ToCollectionAsync();

   	This sets the binding to the collection of items in the todoTable, which contains all TodoItem objects returned from the mobile service.

9. In the **UpdateCheckedTodoItem** method, add the **async** modifier to the method, and uncomment the following line of code:

         await todoTable.UpdateAsync(item);

   	This sends an item update to the mobile service.

Now that the app has been updated to use Mobile Services for backend storage, it's time to test the app against Mobile Services.

##<a name="test-app"></a>Test the app against your new mobile service

1. In Visual Studio, press the F5 key to run the app.

2. As before, type text in the textbox, and then click **Save**.

   	This sends a new item as an insert to the mobile service.

3. In the [Azure classic portal], click **Mobile Services**, and then click your mobile service.

4. Click the **Data** tab, then click **Browse**.

   	![][9]

   	Notice that the **TodoItem** table now contains data, with id values generated by Mobile Services, and that columns have been automatically added to the table to match the TodoItem class in the app.

This concludes the tutorial.

## <a name="next-steps"> </a>Next steps

This tutorial demonstrated the basics of enabling a Windows Phone 8 app to work with data in Mobile Services. Next, consider reading up on one of these other topics:

* [Add authentication to your app](mobile-services-windows-phone-get-started-users.md)
  <br/>Learn how to authenticate users of your app.

* [Add push notifications to your app](mobile-services-javascript-backend-windows-phone-get-started-push.md)
  <br/>Learn how to send a very basic push notification to your app with Mobile Services.

* [Mobile Services C# How-to Conceptual Reference](mobile-services-dotnet-how-to-use-client-library.md)
  <br/>Learn more about how to use Mobile Services with .NET.

<!-- Anchors. -->
[Download the Windows Phone 8 app project]: #download-app
[Create the mobile service]: #create-service
[Add a data table for storage]: #add-table
[Update the app to use Mobile Services]: #update-app
[Test the app against Mobile Services]: #test-app
[Next Steps]:#next-steps

<!-- Images. -->
[0]: ./media/mobile-services-windows-phone-get-started-data/mobile-quickstart-startup-wp8.png
[7]: ./media/mobile-services-windows-phone-get-started-data/mobile-add-nuget-package-wp.png
[8]: ./media/mobile-services-windows-phone-get-started-data/mobile-dashboard-tab.png
[9]: ./media/mobile-services-windows-phone-get-started-data/mobile-todoitem-data-browse.png

<!-- URLs. -->

[Azure classic portal]: https://manage.windowsazure.com/
[Windows Phone 8 SDK]: http://go.microsoft.com/fwlink/p/?LinkID=268374
[Mobile Services SDK]: http://go.microsoft.com/fwlink/p/?LinkID=268375
[Developer Code Samples site]:  http://go.microsoft.com/fwlink/p/?LinkId=271146
