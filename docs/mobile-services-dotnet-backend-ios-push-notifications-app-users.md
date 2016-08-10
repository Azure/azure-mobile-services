<properties
	pageTitle="Send Push Notifications to Authenticated Users (.NET Backend)"
	description="Learn how to send push notifications to specific"
	services="mobile-services,notification-hubs"
	documentationCenter="ios"
	authors="krisragh"
	manager="dwrede"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-ios"
	ms.devlang="objective-c"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="krisragh"/>

# Send push notifications to authenticated users
> [AZURE.SELECTOR-LIST (Platform | Backend)]
- [(iOS | JavaScript)](../articles/mobile-services-javascript-backend-ios-push-notifications-app-users.md)
- [(Windows 8.x Store C# | .NET)](../articles/mobile-services-dotnet-backend-windows-store-dotnet-push-notifications-app-users.md)
- [(Windows 8.x Store C# | JavaScript)](../articles/mobile-services-javascript-backend-windows-store-dotnet-push-notifications-app-users.md)

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).
> For the equivalent Mobile Apps version of this topic, see [How to: Send push notifications to an authenticated user](../app-service-mobile/app-service-mobile-dotnet-backend-how-to-use-server-sdk.md#push-user).

In this topic, you learn how to send push notifications to an authenticated user on iOS. Before starting this tutorial, complete [Get started with authentication] and [Get started with push notifications] first.

In this tutorial, you require users to authenticate first, register with the notification hub for push notifications, and update server scripts to send those notifications to only authenticated users.

##<a name="register"></a>Update service to require authentication to register

[AZURE.INCLUDE [mobile-services-dotnet-backend-push-notifications-app-users](../../includes/mobile-services-dotnet-backend-push-notifications-app-users.md)]

##<a name="update-app"></a>Update app to sign in before registration


Next, you need to change the way that push notifications are registered so that a user is authenticated before registration is attempted.

1. In **QSAppDelegate.m**, remove the implementation of **didFinishLaunchingWithOptions** altogether.

2. Open **QSTodoListViewController.m** and add the following code to the end of the **viewDidLoad** method:

```
// Register for remote notifications
[[UIApplication sharedApplication] registerForRemoteNotificationTypes:
UIRemoteNotificationTypeAlert | UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeSound];
```

##<a name="test"></a>Test app


1. Press **Run** to start the app on a physical iOS device. In the app, add a new item, such as _A new Mobile Services task_, to the todo list.

2. Verify that a notification is received. Additionally -- and optionally -- repeat the above steps on a different physical iOS device, once using the same log-in account and another time using a different log-in account. Verify that notifications are received only by devices authenticating with the same user account.

<!-- Anchors. -->
[Updating the service to require authentication for registration]: #register
[Updating the app to log in before registration]: #update-app
[Testing the app]: #test
[Next Steps]:#next-steps


<!-- URLs. -->
[Get started with authentication]: mobile-services-dotnet-backend-ios-get-started-users.md
[Get started with push notifications]: mobile-services-dotnet-backend-ios-get-started-push.md
[Mobile Services .NET How-to Conceptual Reference]: /develop/mobile/how-to-guides/work-with-net-client-library
