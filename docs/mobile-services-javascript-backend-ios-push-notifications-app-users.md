<properties
	pageTitle="Send Push Notifications to Authenticated Users in iOS (JavaScript backend)"
	description="Learn how to send push notifications to specific users"
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

# Send Push Notifications to Authenticated Users

> [AZURE.SELECTOR-LIST (Platform | Backend)]
- [(iOS | JavaScript)](mobile-services-javascript-backend-ios-push-notifications-app-users.md)
- [(Windows 8.x Store C# | .NET)](mobile-services-dotnet-backend-windows-store-dotnet-push-notifications-app-users.md)
- [(Windows 8.x Store C# | JavaScript)](mobile-services-javascript-backend-windows-store-dotnet-push-notifications-app-users.md)

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

In this topic, you learn how to send push notifications to an authenticated user on iOS. Before starting this tutorial, complete [Get started with authentication] and [Get started with push notifications] first.

In this tutorial, you require users to authenticate first, register with the notification hub for push notifications, and update server scripts to send those notifications to only authenticated users.


##<a name="register"></a>Update Service to Require Authentication to Register


1. Log on to the [Azure classic portal](https://manage.windowsazure.com/), click **Mobile Services**, and then click your mobile service.

2. Click the **Push** tab, select **Only Authenticated Users** for **Permissions**, click **Save**, and then click **Edit Script**.

	This allows you to customize the push notification registration callback function. If you use Git to edit your source code, this same registration function is found in `.\service\extensions\push.js`.

3. Replace the existing **register** function with the following code and then click **Save**:

		exports.register = function (registration, registrationContext, done) {   
		    // Get the ID of the logged-in user.
			var userId = registrationContext.user.userId;    

			// Perform a check here for any disallowed tags.
			if (!validateTags(registration))
			{
				// Return a service error when the client tries
		        // to set a user ID tag, which is not allowed.		
				done("You cannot supply a tag that is a user ID");		
			}
			else{
				// Add a new tag that is the user ID.
				registration.tags.push(userId);

				// Complete the callback as normal.
				done();
			}
		};

		function validateTags(registration){
		    for(var i = 0; i < registration.tags.length; i++) {
		        console.log(registration.tags[i]);           
				if (registration.tags[i]
				.search(/facebook:|twitter:|google:|microsoft:/i) !== -1){
					return false;
				}
				return true;
			}
		}

	This adds a tag to the registration that is the ID of the logged-in user. The supplied tags are validated to prevent a user from registering for another user's ID. When a notification is sent to this user, it is received on this and any other device registered by the user.

4. Click the back arrow, click the **Data** tab, click **TodoItem**, click **Script**, and then select **Insert**.

Replace the `insert` function with the following code, then click **Save**. This insert script uses the user ID tag to send a push notification to all iOS app registrations from the logged-in user:

```
// Get the ID of the logged-in user.
var userId = user.userId;

function insert(item, user, request) {
    request.execute();
    setTimeout(function() {
        push.apns.send(userId, {
            alert: "Alert: " + item.text,
            payload: {
                "Hey, a new item arrived: '" + item.text + "'"
            }
        });
    }, 2500);
}
```

##<a name="update-app"></a>Update App to Login Before Registration


Next, you need to change the way that push notifications are registered so that a user is authenticated before registration is attempted.

1. In **QSAppDelegate.m**, remove the implementation of **didFinishLaunchingWithOptions** altogether.

2. Open **QSTodoListViewController.m** and add the following code to the end of the **viewDidLoad** method:

```
// Register for remote notifications
[[UIApplication sharedApplication] registerForRemoteNotificationTypes:
UIRemoteNotificationTypeAlert | UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeSound];
```

##<a name="test"></a>Test App


1. Press **Run** to start the app on a physical iOS device. In the app, add a new item, such as _A new Mobile Services task_, to the todo list.

2. Verify that a notification is received. Additionally -- and optionally -- repeat the above steps on a different physical iOS device, once using the same log-in account and another time using a different log-in account. Verify that notifications are received only by devices authenticating with the same user account.



<!-- Anchors. -->
[Updating the service to require authentication for registration]: #register
[Updating the app to log in before registration]: #update-app
[Testing the app]: #test
[Next Steps]:#next-steps


<!-- URLs. -->
[Get started with authentication]: mobile-services-ios-get-started-users.md
[Get started with push notifications]: mobile-services-javascript-backend-ios-get-started-push.md
[Mobile Services .NET How-to Conceptual Reference]: mobile-services-ios-how-to-use-client-library.md
