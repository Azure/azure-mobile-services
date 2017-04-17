
<properties
	pageTitle="Get started with push notifications (Android JavaScript) | Microsoft Azure"
	description="Learn how to use Azure Mobile Services to send push notifications to your Android JavaScript app."
	services="mobile-services, notification-hubs"
	documentationCenter="android"
	authors="RickSaling"
	writer="ricksal"
	manager="erikre"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-android"
	ms.devlang="java"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="ricksal"/>


# Add push notifications to your Mobile Services Android app

> [AZURE.SELECTOR-LIST (Platform | Backend )]
- [(iOS | .NET)](mobile-services-dotnet-backend-ios-get-started-push.md)
- [(iOS | JavaScript)](mobile-services-javascript-backend-ios-get-started-push.md)
- [(Windows Runtime 8.1 universal C# | .NET)](mobile-services-dotnet-backend-windows-universal-dotnet-get-started-push.md)
- [(Windows Runtime 8.1 universal C# | Javascript)](mobile-services-javascript-backend-windows-universal-dotnet-get-started-push.md)
- [(Windows Phone Silverlight 8.x | Javascript)](mobile-services-javascript-backend-windows-phone-get-started-push.md)
- [(Android | .NET)](mobile-services-dotnet-backend-android-get-started-push.md)
- [(Android | Javascript)](mobile-services-javascript-backend-android-get-started-push.md)
- [(Xamarin.iOS | Javascript)](partner-xamarin-mobile-services-ios-get-started-push.md)
- [(Xamarin.Android | Javascript)](partner-xamarin-mobile-services-android-get-started-push.md)
- [(Xamarin.Android | .NET)](mobile-services-dotnet-backend-xamarin-android-get-started-push.md)
- [(Xamarin.Forms | JavaScript)](partner-xamarin-mobile-services-xamarin-forms-get-started-push.md)

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

## Summary

This topic shows how to use Azure Mobile Services to send push notifications to your Android app using Google Cloud Messaging ("GCM"). You will add push notifications to the quickstart project that is a prerequisite for this tutorial. Push notifications are enabled by using the Azure Notification Hub that is included in your mobile service. When complete, your mobile service will send a push notification each time a record is inserted.

## Prerequisites

This tutorial is based on the code you download in the Mobile Services quickstart. Before you start this tutorial, you must first complete either [Get started with Mobile Services] or [Add Mobile Services to an existing app].

> [AZURE.IMPORTANT] If you completed the quickstart tutorial prior to the release of Azure Mobile Services Android SDK 2.0, you must re-do it, because the SDK is not backwards compatible. To verify the version, check the **dependencies** section of your project's **build.gradle** file.


<!-- URLs.
[Get started with Mobile Services]: mobile-services-android-get-started.md
[Add Mobile Services to an existing app]: mobile-services-android-get-started-data.md
-->

## Sample code
To see the completed source code go [here](https://github.com/Azure/mobile-services-samples/tree/master/GettingStartedWithPush).

## Enable Google Cloud Messaging


1. Navigate to the [Google Cloud Console](https://console.developers.google.com/project), sign in with your Google account credentials.

2. Click **Create Project**, type a project name, then click **Create**. If requested, carry out the SMS Verification, and click **Create** again.

   	![](./media/mobile-services-enable-google-cloud-messaging/mobile-services-google-new-project.png)   

	 Type in your new **Project name** and click **Create project**.

3. Click the **Utilities and More** button and then click **Project Information**. Make a note of the **Project Number**. You will need to set this value as the `SenderId` variable in the client app.

   	![](./media/mobile-services-enable-google-cloud-messaging/notification-hubs-utilities-and-more.png)


4. In the project dashboard, under **Mobile APIs**, click **Google Cloud Messaging**, then on the next page click **Enable API** and accept the terms of service.

	![Enabling GCM](./media/mobile-services-enable-google-cloud-messaging/enable-GCM.png)

	![Enabling GCM](./media/mobile-services-enable-google-cloud-messaging/enable-gcm-2.png)

5. In the project dashboard, Click **Credentials** > **Create Credential** > **API Key**.

   	![](./media/mobile-services-enable-google-cloud-messaging/mobile-services-google-create-server-key.png)

6. In **Create a new key**, click **Server key**, type a name for your key, then click **Create**.

7. Make a note of the **API KEY** value.

	You will use this API key value to enable Azure to authenticate with GCM and send push notifications on behalf of your app.


## Configure Mobile Services to send push requests


1. Log on to the [Azure classic portal](https://manage.windowsazure.com/), click **Mobile Services**, and then click your app.

2. Click the **Push** tab, enter the **API Key** value obtained from GCM in the previous procedure, then click **Save**.

   	![](./media/mobile-services-android-configure-push/mobile-push-tab-android.png)

    >[AZURE.NOTE]When you set your GCM credentials for enhanced push notifications in the Push tab in the portal, they are shared with Notification Hubs to configure the notification hub with your app.

Both your mobile service and your app are now configured to work with GCM and Notification Hubs.

## Add push notifications to your app



Your next step is to install Google Play services. Google Cloud Messaging has some minimum API level requirements for development and testing, which the **minSdkVersion** property in the Manifest must conform to.

If you will be testing with an older device, then consult [Set Up Google Play Services SDK] to determine how low you can set this value, and set it appropriately.

### Add Google Play Services to the project

1. Open the Android SDK Manager by clicking the icon on the toolbar of Android Studio or by clicking **Tools** -> **Android** -> **SDK Manager** on the menu. Locate the target version of the Android SDK that is used in your project , open it, and choose **Google APIs**, if it is not already installed.

2. Scroll down to **Extras**, expand it, and choose **Google Play Services**, as shown below. Click **Install Packages**. Note the SDK path, for use in the following step.

   	![](./media/notification-hubs-android-get-started/notification-hub-create-android-app4.png)


3. Open the **build.gradle** file in the app directory.

	![](./media/mobile-services-android-get-started-push/android-studio-push-build-gradle.png)

4. Add this line under *dependencies*:

   		compile 'com.google.android.gms:play-services-gcm:8.4.0'

5. Under *defaultConfig*, change *minSdkVersion* to 9.

6. Click the **Sync Project with Gradle Files** icon in the tool bar.

7. Open **AndroidManifest.xml** and add this tag to the *application* tag.

        <meta-data android:name="com.google.android.gms.version"
            android:value="@integer/google_play_services_version" />






### Add code

1. In your **app** project, open the file `AndroidManifest.xml`. In the code in the next two steps, replace _`**my_app_package**`_ with the name of the app package for your project, which is the value of the `package` attribute of the `manifest` tag.

2. Add the following new permissions after the existing `uses-permission` element:

        <permission android:name="**my_app_package**.permission.C2D_MESSAGE"
            android:protectionLevel="signature" />
        <uses-permission android:name="**my_app_package**.permission.C2D_MESSAGE" />
        <uses-permission android:name="com.google.android.c2dm.permission.RECEIVE" />
        <uses-permission android:name="android.permission.GET_ACCOUNTS" />
        <uses-permission android:name="android.permission.WAKE_LOCK" />

3. Add the following code after the `application` opening tag:

        <receiver android:name="com.microsoft.windowsazure.notifications.NotificationsBroadcastReceiver"
            						 	android:permission="com.google.android.c2dm.permission.SEND">
            <intent-filter>
                <action android:name="com.google.android.c2dm.intent.RECEIVE" />
                <category android:name="**my_app_package**" />
            </intent-filter>
        </receiver>


4. Add this line under *dependencies* in the **build.gradle** file in the app directory and re-sync gradle with the project:

	    compile(group: 'com.microsoft.azure', name: 'azure-notifications-handler', version: '1.0.1', ext: 'jar')


5. Open the file *ToDoItemActivity.java*, and add the following import statement:

		import com.microsoft.windowsazure.notifications.NotificationsManager;


6. Add the following private variable to the class: replace _`<PROJECT_NUMBER>`_ with the Project Number assigned by Google to your app in the preceding procedure:

		public static final String SENDER_ID = "<PROJECT_NUMBER>";

7. Change the definition of the *MobileServiceClient* from **private** to **public static**, so it now looks like this:

		public static MobileServiceClient mClient;



8. Next we need to add a new class to handle notifications. In the Project Explorer, open the **src** => **main** => **java** nodes, and right-click the  package name node: click **New**, then click **Java Class**.

9. In **Name** type `MyHandler`, then click **OK**.


	![](./media/mobile-services-android-get-started-push/android-studio-create-class.png)


10. In the MyHandler file, replace the class declaration with

		public class MyHandler extends NotificationsHandler {


11. Add the following import statements for the `MyHandler` class:

		import android.app.NotificationManager;
		import android.app.PendingIntent;
		import android.content.Context;
		import android.content.Intent;
		import android.os.AsyncTask;
		import android.os.Bundle;
		import android.support.v4.app.NotificationCompat;


12. Next add the following members for the `MyHandler` class:

		public static final int NOTIFICATION_ID = 1;
		private NotificationManager mNotificationManager;
		NotificationCompat.Builder builder;
		Context ctx;


13. In the `MyHandler` class, add the following code to override the **onRegistered** method, which registers your device with the mobile service Notification Hub.

		@Override
		public void onRegistered(Context context,  final String gcmRegistrationId) {
		    super.onRegistered(context, gcmRegistrationId);

		    new AsyncTask<Void, Void, Void>() {

		    	protected Void doInBackground(Void... params) {
		    		try {
		    		    ToDoActivity.mClient.getPush().register(gcmRegistrationId, null);
		    		    return null;
	    		    }
	    		    catch(Exception e) {
			    		// handle error    		    
	    		    }
					return null;  		    
	    		}
		    }.execute();
		}



14. In the `MyHandler` class, add the following code to override the **onReceive** method, which causes the notification to display when it is received.

		@Override
		public void onReceive(Context context, Bundle bundle) {
		    ctx = context;
		    String nhMessage = bundle.getString("message");

		    sendNotification(nhMessage);
		}

		private void sendNotification(String msg) {
			mNotificationManager = (NotificationManager)
		              ctx.getSystemService(Context.NOTIFICATION_SERVICE);

		    PendingIntent contentIntent = PendingIntent.getActivity(ctx, 0,
		          new Intent(ctx, ToDoActivity.class), 0);

		    NotificationCompat.Builder mBuilder =
		          new NotificationCompat.Builder(ctx)
		          .setSmallIcon(R.drawable.ic_launcher)
		          .setContentTitle("Notification Hub Demo")
		          .setStyle(new NotificationCompat.BigTextStyle()
		                     .bigText(msg))
		          .setContentText(msg);

		     mBuilder.setContentIntent(contentIntent);
		     mNotificationManager.notify(NOTIFICATION_ID, mBuilder.build());
		}


15. Back in the TodoActivity.java file, update the **onCreate** method of the *ToDoActivity* class to register the notification handler class. Make sure to add this code after the *MobileServiceClient* is instantiated.


		NotificationsManager.handleNotifications(this, SENDER_ID, MyHandler.class);

    Your app is now updated to support push notifications.

<!-- URLs. -->
[Mobile Services Android SDK]: http://aka.ms/Iajk6q



## Update the registered insert script in the Azure classic portal


1. In the [Azure classic portal](https://manage.windowsazure.com/), click the **Data** tab and then click the **TodoItem** table.

2. In **todoitem**, click the **Script** tab and select **Insert**.

   	This displays the function that is invoked when an insert occurs in the **TodoItem** table.

3. Replace the insert function with the following code, and then click **Save**:

		function insert(item, user, request) {
		// Define a simple payload for a GCM notification.
	    var payload = {
	        "data": {
	            "message": item.text
	        }
	    };		
		request.execute({
		    success: function() {
		        // If the insert succeeds, send a notification.
		        push.gcm.send(null, payload, {
		            success: function(pushResponse) {
		                console.log("Sent push:", pushResponse, payload);
		                request.respond();
		                },              
		            error: function (pushResponse) {
		                console.log("Error Sending push:", pushResponse);
		                request.respond(500, { error: pushResponse });
		                }
		            });
		        },
		    error: function(err) {
		        console.log("request.execute error", err)
		        request.respond();
		    }
		  });
		}

   	This registers a new insert script, which uses the [gcm object](http://go.microsoft.com/fwlink/p/?LinkId=282645) to send a push notification to all registered devices after the insert succeeds.


## Test push notifications in your app

You can test the app by directly attaching an Android phone with a USB cable, or by using a virtual device in the emulator.

### Setting up the Android emulator for testing

When you run this app in the emulator, make sure that you use an Android Virtual Device (AVD) that supports Google APIs.

1. From the right end of the toolbar, select the Android Virtual Device Manager, select your device, click the edit icon on the right.

	![](./media/mobile-services-javascript-backend-android-get-started-push/mobile-services-android-virtual-device-manager.png)

2. Select **Change** on the device description line, select **Google APIs**,  then click OK.

   	![](./media/mobile-services-javascript-backend-android-get-started-push/mobile-services-android-virtual-device-manager-edit.png)

	This targets the AVD to use Google APIs.

### Running the test

1. From the **Run** menu item, click **Run app** to start the app.

2. In the app, type meaningful text, such as _A new Mobile Services task_ and then click the **Add** button.

  	![](./media/mobile-services-javascript-backend-android-get-started-push/mobile-quickstart-push1-android.png)

3. Swipe down from the top of the screen to open the device's Notification Drawer to see the notification.


You have successfully completed this tutorial.

## Troubleshooting

### Verify Android SDK Version

Because of ongoing development, the Android SDK version installed in Android Studio might not match the version in the code. The Android SDK referenced in this tutorial is version 21, the latest at the time of writing. The version number may increase as new releases of the SDK appear, and we recomend using the latest version available.

Two symptoms of version mismatch are:

1. When you Build or Rebuild the project, you may get Gradle error messages like "**failed to find target Google Inc.:Google APIs:n**".

2. Standard Android objects in code that should resolve based on `import` statements may be generating error messages.

If either of these appear, the version of the Android SDK installed in Android Studio might not match the SDK target of the downloaded project.  To verify the version, make the following changes:


1. In Android Studio, click **Tools** => **Android** => **SDK Manager**. If you have not installed the latest version of the SDK Platform, then click to install it. Make a note of the version number.

2. In the Project Explorer tab, under **Gradle Scripts**, open the file **build.gradle (modeule: app)**. Ensure that the **compileSdkVersion** and **buildToolsVersion** are set to the latest  SDK version installed. The tags might look like this:

	 	    compileSdkVersion 'Google Inc.:Google APIs:21'
    		buildToolsVersion "21.1.2"

3. In the Android Studio Project Explorer right-click the project node, choose **Properties**, and in the left column choose **Android**. Ensure that the **Project Build Target** is set to the same SDK version as the **targetSdkVersion**.

4. In Android Studio, the manifest file is no longer used to specify the target SDK and minimum SDK version, unlike the case with Eclipse.

## Next steps

<!---This tutorial demonstrated the basics of enabling an Android app to use Mobile Services and Notification Hubs to send push notifications. Next, consider completing the next tutorial, [Send push notifications to authenticated users], which shows how to use tags to send push notifications from a Mobile Service to only an authenticated user.

+ [Send broadcast notifications to subscribers]
	<br/>Learn how users can register and receive push notifications for categories they're interested in.

+ [Send template-based notifications to subscribers]
	<br/>Learn how to use templates to send push notifications from a Mobile Service, without having to craft platform-specific payloads in your back-end.
-->

Learn more about Mobile Services and Notification Hubs in the following topics:

* [Get started with authentication]
  <br/>Learn how to authenticate users of your app with different account types using mobile services.

* [What are Notification Hubs?]
  <br/>Learn more about how Notification Hubs works to deliver notifications to your apps across all major client platforms.

* [Debug Notification Hubs applications](http://go.microsoft.com/fwlink/p/?linkid=386630)
  </br>Get guidance troubleshooting and debugging Notification Hubs solutions.

* [How to use the Android client library for Mobile Services]
  <br/>Learn more about how to use Mobile Services with Android.

* [Mobile Services server script reference]
  <br/>Learn more about how to implement business logic in your mobile service.


<!-- Anchors. -->
[Register your app for push notifications and configure Mobile Services]: #register
[Update the generated push notification code]: #update-scripts
[Insert data to receive notifications]: #test
[Next Steps]:#next-steps

<!-- Images. -->
[13]: ./media/mobile-services-windows-store-javascript-get-started-push/mobile-quickstart-push1.png
[14]: ./media/mobile-services-windows-store-javascript-get-started-push/mobile-quickstart-push2.png


<!-- URLs. -->
[Submit an app page]: http://go.microsoft.com/fwlink/p/?LinkID=266582
[My Applications]: http://go.microsoft.com/fwlink/p/?LinkId=262039
[Get started with Mobile Services]: mobile-services-android-get-started.md
[Get started with authentication]: mobile-services-android-get-started-users.md
[Get started with push notifications]: https://azure.microsoft.com/develop/mobile/tutorials/get-started-with-push-js
[Push notifications to app users]: https://azure.microsoft.com/develop/mobile/tutorials/push-notifications-to-users-js
[Authorize users with scripts]: https://azure.microsoft.com/develop/mobile/tutorials/authorize-users-in-scripts-js
[JavaScript and HTML]: https://azure.microsoft.com/develop/mobile/tutorials/get-started-with-push-js
[Set Up Google Play Services SDK]: http://go.microsoft.com/fwlink/?LinkId=389801
[Azure classic portal]: https://manage.windowsazure.com/
[How to use the Android client library for Mobile Services]: mobile-services-android-how-to-use-client-library.md

[gcm object]: http://go.microsoft.com/fwlink/p/?LinkId=282645

[Mobile Services server script reference]: http://go.microsoft.com/fwlink/?LinkId=262293
[What are Notification Hubs?]: https://azure.microsoft.com/en-us/documentation/articles/notification-hubs-push-notification-overview/
[Send broadcast notifications to subscribers]: https://azure.microsoft.com/en-us/documentation/articles/notification-hubs-aspnet-backend-android-xplat-segmented-gcm-push-notification/
[Send template-based notifications to subscribers]: https://azure.microsoft.com/en-us/documentation/articles/notification-hubs-aspnet-backend-android-xplat-segmented-gcm-push-notification/
