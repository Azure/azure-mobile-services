<properties
	pageTitle="Get Started with Mobile Services for Windows Store JavaScript apps | Azure Mobile Services"
	description="Follow this tutorial to get started using Azure Mobile Services for Windows Store development in JavaScript."
	services="mobile-services"
	documentationCenter="windows"
	authors="ggailey777"
	manager="erikre"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-windows-store"
	ms.devlang="javascript"
	ms.topic="get-started-article"
	ms.date="07/21/2016"
	ms.author="glenga"/>

# Get started with Mobile Services

> [AZURE.SELECTOR-LIST (Platform | Backend )]
- [(iOS | .NET)](mobile-services-dotnet-backend-ios-get-started.md)
- [(iOS | JavaScript)](mobile-services-ios-get-started.md)
- [(Windows Runtime 8.1 universal C# | .NET)](mobile-services-dotnet-backend-windows-store-dotnet-get-started.md)
- [(Windows Runtime 8.1 universal C# | Javascript)](mobile-services-javascript-backend-windows-store-dotnet-get-started.md)
- [(Windows Runtime 8.1 universal JavaScript | Javascript)](mobile-services-javascript-backend-windows-store-javascript-get-started.md)
- [(Android | .NET)](mobile-services-dotnet-backend-android-get-started.md)
- [(Android | Javascript)](mobile-services-android-get-started.md)
- [(Xamarin.iOS | .NET)](mobile-services-dotnet-backend-xamarin-ios-get-started.md)
- [(Xamarin.iOS | Javascript)](partner-xamarin-mobile-services-ios-get-started.md)
- [(Xamarin.Android | .NET)](mobile-services-dotnet-backend-xamarin-android-get-started.md)
- [(Xamarin.Android | Javascript)](partner-xamarin-mobile-services-android-get-started.md)
- [(HTML | Javascript)](mobile-services-html-get-started.md)
- [(PhoneGap | Javascript)](mobile-services-javascript-backend-phonegap-get-started.md)
- [(Sencha | Javascript)](partner-sencha-mobile-services-get-started.md)

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

This tutorial shows you how to add a cloud-based backend service to a Windows Store JavaScript app using Azure Mobile Services. In this tutorial, you will create both a new mobile service and a simple *To do list* app that stores app data in the new mobile service. The mobile service that you will create uses JavaScript for server-side business logic.

To complete this tutorial, you need the following:

* An active Azure account. If you don't have an account, you can create a free trial account in just a couple of minutes. For details, see [Azure Free Trial](https://azure.microsoft.com/pricing/free-trial/?WT.mc_id=A0E0E5C02&amp;returnurl=http%3A%2F%2Fazure.microsoft.com%2Fdocumentation%2Farticles%2Fmobile-services-javascript-backend-windows-store-javascript-get-started%2F).
* [Visual Studio 2013 Express for Windows]

## Create a new mobile service



Follow these steps to create a new mobile service.

1.	Log into the [Azure classic portal](https://manage.windowsazure.com/). At the bottom of the navigation pane, click **+NEW**. Expand **Compute** and **Mobile Service**, then click **Create**.

	![](./media/mobile-services-create-new-service/mobile-create.png)

	This displays the **Create a Mobile Service** dialog.

2.	In the **Create a Mobile Service** dialog, select **Create a free 20 MB SQL Database**, select **JavaScript** runtime, then type a subdomain name for the new mobile service in the **URL** textbox. Click the right arrow button to go to the next page.

	![](./media/mobile-services-create-new-service/mobile-create-page1.png)

	This displays the **Specify database settings** page.

	>[AZURE.NOTE]As part of this tutorial, you create a new SQL Database instance and server. You can reuse this new database and administer it as you would any other SQL Database instance. If you already have a database in the same region as the new mobile service, you can instead choose **Use existing Database** and then select that database. The use of a database in a different region is not recommended because of additional bandwidth costs and higher latencies.

3.	In **Name**, type the name of the new database, then type **Login name**, which is the administrator login name for the new SQL Database server, type and confirm the password, and click the check button to complete the process.
	![](./media/mobile-services-create-new-service/mobile-create-page2.png)

You have now created a new mobile service that can be used by your mobile apps.


## Create a new Windows Store app

Once you have created your mobile service, you can follow an easy quickstart in the Azure classic portal to create a new Windows Store 8.1 JavaScript app that connects to your mobile service.

1.  In the [Azure classic portal], click **Mobile Services**, and then click the mobile service that you just created.


2. In the quickstart tab, click **Windows** under **Choose platform** and expand **Create a new Windows Store app**.

3. If you haven't already done so, download and install [Visual Studio 2013][Visual Studio 2013 Express for Windows] on your local computer or virtual machine.

4. Click **Create TodoItem table** to create a table to store app data.

5. Under **Download and run your app**, select a language for your app, then click **Download**.

  	This downloads the project for the sample *To do list* application that is connected to your mobile service. Save the compressed project file to your local computer, and make a note of where you save it.

## Run your Windows app

The final stage of this tutorial is to build and run your new app.

1. Browse to the location where you saved the compressed project files, expand the files on your computer, and open the solution file in Visual Studio.

2. Press the **F5** key to rebuild the project and start the app.

3. In the app, type meaningful text, such as *Complete the tutorial*, in **Insert a TodoItem**, and then click **Save**.

   	This sends a POST request to the new mobile service hosted in Azure. Data from the request is inserted into the TodoItem table. Items stored in the table are returned by the mobile service, and the data is displayed in the second column in the app.

4. (Optional) Run the app again, and notice that data saved from the previous step is loaded from the mobile service after the app starts.

4. Back in the [Azure classic portal], click the **Data** tab and then click the **TodoItems** table.

   	This lets you browse the data inserted by the app into the table.

>[AZURE.NOTE] You can review the code that accesses your mobile service to query and insert data, which is found in the default.js file.

## Next Steps
Now that you have completed the quickstart, learn how to work with the [Mobile Services client for HTML/JavaScript](mobile-services-html-how-to-use-client-library.md).


<!-- Anchors. -->
[Getting started with Mobile Services]:#getting-started
[Create a new mobile service]:#create-new-service
[Define the mobile service instance]:#define-mobile-service-instance
[Next Steps]:#next-steps

<!-- Images. -->

<!-- URLs. -->
[Visual Studio 2013 Express for Windows]: http://go.microsoft.com/fwlink/?LinkId=257546
[Mobile Services SDK]: http://go.microsoft.com/fwlink/?LinkId=257545
[Azure classic portal]: https://manage.windowsazure.com/
