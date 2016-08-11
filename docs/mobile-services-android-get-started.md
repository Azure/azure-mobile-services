<properties
	pageTitle="Get Started with Azure Mobile Services for Android apps (JavaScript backend)"
	description="Follow this tutorial to get started using Azure Mobile Services for Android development (JavaScript backend)."
	services="mobile-services"
	documentationCenter="android"
	authors="RickSaling"
	manager="reikre"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-android"
	ms.devlang="java"
	ms.topic="hero-article"
	ms.date="07/21/2016"
	ms.author="ricksal"/>

# Get started with Mobile Services for Android  (JavaScript backend)

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

This tutorial shows you how to add a cloud-based backend service to an Android app using Azure Mobile Services. In this tutorial, you will create both a new mobile service and a simple **To do list** app that stores app data in the new mobile service.

> [AZURE.VIDEO mobile-get-started-android]

A screenshot from the completed app is below:

![](./media/mobile-services-android-get-started/mobile-quickstart-completed-android.png)

## Prerequisites

Completing this tutorial requires the [Android Developer Tools](https://developer.android.com/sdk/index.html), which includes the Android Studio integrated development environment, and the latest Android platform. Android 4.2 or a later version is required.

The downloaded quickstart project contains the Azure Mobile Services SDK for Android.

> [AZURE.IMPORTANT] To complete this tutorial, you need an Azure account. If you don't have an account, you can create a free trial account in just a couple of minutes. For details, see [Azure Free Trial](https://azure.microsoft.com/pricing/free-trial/?WT.mc_id=AE564AB28).


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


## Create a new Android app

Once you have created your mobile service, you can follow an easy quickstart in the Azure classic portal to either create a new app or modify an existing app to connect to your mobile service.

In this section you will create a new Android app that is connected to your mobile service.

1.  In the Azure classic portal, click **Mobile Services**, and then click the mobile service that you just created.

2. In the quickstart tab, click **Android** under **Choose platform** and expand **Create a new Android app**.

   	![](./media/mobile-services-android-get-started/mobile-portal-quickstart-android1.png)

   	This displays the three easy steps to create an Android app connected to your mobile service.

  	![](./media/mobile-services-android-get-started/mobile-quickstart-steps-android-AS.png)

3. If you haven't already done so, download and install the [Android Developer Tools](https://go.microsoft.com/fwLink/p/?LinkID=280125) on your local computer or virtual machine.

4. Click **Create TodoItem table** to create a table to store app data.


5. Now download your app by pressing the **Download** button.

## Run your Android app

The final stage of this tutorial is to build and run your new app.

### Load project into Android Studio and sync Gradle

1. Browse to the location where you saved the compressed project files and expand the files on your computer into your Android Studio projects directory.

2. Open Android Studio. If you are working with a project and it appears, close the project (File => Close Project).

3. Select **Open an existing Android Studio project**, browse to the project location, and then click **OK.** This will load the project and start to sync it with Gradle.

 	![](./media/mobile-services-android-get-started/android-studio-import-project.png)

4. Wait for the Gradle sync activity to complete. If you see a "failed to find target" error, this is because the version used in Android Studio doesn't match that of the sample. The easiest way to fix this is to click the **Install missing platform(s) and sync project** link in the error message. You might get additional version error messages, and you simply repeat this process until no errors appear.
    - There is another way to fix this if you want to run with the "latest and greatest" version of Android. You can update the **targetSdkVersion** in the *build.gradle* file in the *app* directory to match the version already installed on your machine, which you can discover by clicking the **SDK Manager** icon and seeing what version is listed. Next you press the **Sync Project with Gradle Files**. You may get an error message for the version of Build Tools, and you fix that the same way.

### Running the app

You can run the app using the emulator, or using an actual device.

1. To run from a device, connect it to your computer with a USB cable. You must [set up the device for development](https://developer.android.com/training/basics/firstapp/running-app.html). If you are developing on a Windows machine, you must also download and install a USB driver.

2. To run using the Android emulator, you must define at least one Android Virtual Device (AVD). Click the AVD Manager icon to create and manage these devices.

3. From the **Run** menu, click **Run** to start the project. and choose a device or emulator from the dialog box that appears.

4. When the app appears, type meaningful text, such as _Complete the tutorial_, and then click **Add**.

   	![](./media/mobile-services-android-get-started/mobile-quickstart-startup-android.png)

   	This sends a POST request to the new mobile service hosted in Azure. Data from the request is inserted into the TodoItem table. Items stored in the table are returned by the mobile service, and the data is displayed in the list.

	> [AZURE.NOTE] You can review the code that accesses your mobile service to query and insert data, which is found in the ToDoActivity.java file.

8. Back in the Azure classic portal, click the **Data** tab and then click the **TodoItems** table.

   	![](./media/mobile-services-android-get-started/mobile-data-tab1.png)

   	This lets you browse the data inserted by the app into the table.

   	![](./media/mobile-services-android-get-started/mobile-data-browse.png)



## <a name="next-steps"> </a>Next Steps
Now that you have completed the quickstart, learn how to perform additional important tasks in Mobile Services:

* [Get started with data]
  <br/>Learn more about storing and querying data using Mobile Services.

* [Get started with authentication]
  <br/>Learn how to authenticate users of your app with an identity provider.

* [Get started with push notifications]
  <br/>Learn how to send a very basic push notification to your app.






<!-- URLs. -->
[Get started (Eclipse)]: mobile-services-android-get-started-ec.md
[Get started with data]: mobile-services-android-get-started-data.md
[Get started with authentication]: mobile-services-android-get-started-users.md
[Get started with push notifications]: mobile-services-javascript-backend-android-get-started-push.md
[Mobile Services Android SDK]: https://go.microsoft.com/fwLink/p/?LinkID=266533
