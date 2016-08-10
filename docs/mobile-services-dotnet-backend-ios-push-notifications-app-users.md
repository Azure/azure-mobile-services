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


1. In Solution Explorer in Visual Studio, expand the App_Start folder and open the WebApiConfig.cs project file.

2. Add the following line of code to the Register method after the **ConfigOptions** definition:

        options.PushAuthorization = 
            Microsoft.WindowsAzure.Mobile.Service.Security.AuthorizationLevel.User;
 
	This enforces user authentication before registering for push notifications. 

2. Right-click the project, click **Add** then click **Class...**.

3. Name the new empty class `PushRegistrationHandler` then click **Add**.

4. At the top of the code page, add the following **using** statements:

		using System.Threading.Tasks; 
		using System.Web.Http; 
		using System.Web.Http.Controllers; 
		using Microsoft.WindowsAzure.Mobile.Service; 
		using Microsoft.WindowsAzure.Mobile.Service.Notifications; 
		using Microsoft.WindowsAzure.Mobile.Service.Security; 

5. Replace the existing **PushRegistrationHandler** class with the following code:
 
	    public class PushRegistrationHandler : INotificationHandler
	    {
	        public Task Register(ApiServices services, HttpRequestContext context,
            NotificationRegistration registration)
	        {
	            try
	            {
	                // Perform a check here for user ID tags, which are not allowed.
	                if(!ValidateTags(registration))
	                {
	                    throw new InvalidOperationException(
	                        "You cannot supply a tag that is a user ID.");                    
	                }
	
	                // Get the logged-in user.
	                var currentUser = context.Principal as ServiceUser;
	
	                // Add a new tag that is the user ID.
	                registration.Tags.Add(currentUser.Id);
	
	                services.Log.Info("Registered tag for userId: " + currentUser.Id);
	            }
	            catch(Exception ex)
	            {
	                services.Log.Error(ex.ToString());
	            }
	                return Task.FromResult(true);
	        }
	
	        private bool ValidateTags(NotificationRegistration registration)
	        {
	            // Create a regex to search for disallowed tags.
	            System.Text.RegularExpressions.Regex searchTerm =
	            new System.Text.RegularExpressions.Regex(@"facebook:|google:|twitter:|microsoftaccount:",
	                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
	
	            foreach (string tag in registration.Tags)
	            {
	                if (searchTerm.IsMatch(tag))
	                {
	                    return false;
	                }
	            }
	            return true;
	        }
		
	        public Task Unregister(ApiServices services, HttpRequestContext context, 
	            string deviceId)
	        {
	            // This is where you can hook into registration deletion.
	            return Task.FromResult(true);
	        }
	    }

	The **Register** method is called during registration. This lets you add a tag to the registration that is the ID of the logged-in user. The supplied tags are validated to prevent a user from registering for another user's ID. When a notification is sent to this user, it is received on this and any other device registered by the user. 

6. Expand the Controllers folder, open the TodoItemController.cs project file, locate the **PostTodoItem** method and replace the line of code that calls **SendAsync** with the following code:

        // Get the logged-in user.
		var currentUser = this.User as ServiceUser;
		
		// Use a tag to only send the notification to the logged-in user.
        var result = await Services.Push.SendAsync(message, currentUser.Id);

7. Republish the mobile service project.

Now, the service uses the user ID tag to send a push notification (with the text of the inserted item) to all registrations created by the logged-in user.
 

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
