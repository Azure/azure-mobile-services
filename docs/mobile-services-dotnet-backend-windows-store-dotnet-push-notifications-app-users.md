<properties
	pageTitle="Send push notifications to authenticated users (Universal Windows 8.1) | Azure Mobile Services"
	description="Learn how to use Azure Mobile Services to send push notifications to a specific authenticated user running your Universal Windows 8.1 app."
	services="mobile-services,notification-hubs"
	documentationCenter="windows"
	authors="ggailey777"
	manager="dwrede"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-windows"
	ms.devlang="dotnet"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>

# Send push notifications to authenticated users
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
> For the equivalent Mobile Apps version of this topic, see [How to: Send push notifications to an authenticated user](../app-service-mobile/app-service-mobile-dotnet-backend-how-to-use-server-sdk.md#push-user).

##Overview

This topic shows you how to send push notifications to an authenticate user on any registered device. Unlike the previous [push notification][Get started with push notifications] tutorial, this tutorial changes your mobile service to require that a user be authenticated before the client can register with the notification hub for push notifications. Registration is also modified to add a tag based on the assigned user ID. Finally, the server code is updated to send the notification only to the authenticated user instead of to all registrations.

This tutorial supports both Windows Store and Windows Phone Store apps.

##Prerequisites

Before you start this tutorial, you must have already completed these Mobile Services tutorials:

+ [Get started with authentication]
Adds a login requirement to the TodoList sample app.

+ [Get started with push notifications]
Configures the TodoList sample app for push notifications by using Notification Hubs.

After you have completed both tutorials, you can prevent unauthenticated users from registering for push notifications from your mobile service.

##<a name="register"></a>Update the service to require authentication to register


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
 

##<a name="update-app"></a>Update the app to log in before registration


Next, you need to change the way that push notifications are registered to make sure that the user is authenticated before registration is attempted. The client app updates depend on the way in which you implemented push notifications.

###Using the Add Push Notification Wizard in Visual Studio 2013 Update 2 or a later version

In this method, the wizard generated a new push.register.cs file in your project.

1. In Visual Studio in Solution Explorer, open the app.xaml.cs project file and in the **OnLaunched** event handler comment-out or delete the call to the **UploadChannel** method. 

2. Open the push.register.cs project file and replace the **UploadChannel** method, with the following code:

		public async static void UploadChannel()
		{
		    var channel = 
		        await Windows.Networking.PushNotifications.PushNotificationChannelManager
		        .CreatePushNotificationChannelForApplicationAsync();
		
		    try
		    {
		        // Create a native push notification registration.
		        await App.MobileService.GetPush().RegisterNativeAsync(channel.Uri);		        
		
		    }
		    catch (Exception exception)
		    {
		        HandleRegisterException(exception);
		    }
		}

	This makes sure that registration is done using the same client instance that has the authenticated user credentials. Otherwise, registration will fail with an Unauthorized (401) error.

3. Open the shared MainPage.cs project file, and replace the **ButtonLogin_Click** handler with the following:

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Login the user and then load data from the mobile service.
            await AuthenticateAsync();
			todolistPush.UploadChannel();

            // Hide the login button and load items from the mobile service.
            this.ButtonLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            await RefreshTodoItems();
        }

	This makes sure that authentication occurs before push registration is attempted.

4. 	In the previous code, replace the generated push class name (`todolistPush`) with the name of class generated by the wizard, usually in the format <code><em>mobile_service</em>Push</code>. 

###Manually enabled push notifications		

In this method, you added registration code from the tutorial directly to the app.xaml.cs project file.

1. In Visual Studio in Solution Explorer, open the app.xaml.cs project file and in the **OnLaunched** event handler comment-out or delete the call to **InitNotificationsAsync**. 
 
2. Change the accessibility of the **InitNotificationsAsync** method from `private` to `public` and add the `static` modifier. 

3. Open the shared MainPage.cs project file, and replace the **ButtonLogin_Click** handler with the following:

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Login the user and then load data from the mobile service.
            await AuthenticateAsync();
			App.InitNotificationsAsync();

            // Hide the login button and load items from the mobile service.
            this.ButtonLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            await RefreshTodoItems();
        }
	
	This makes sure that authentication occurs before push registration is attempted.

##<a name="test"></a>Test the app


1. In Visual Studio, press the F5 key to run the app.

2. Log in using the selected identity provider and verify that authentication succeeds. 

3. In the app, type text in **Insert a TodoItem**, and then click **Save**.

   	Note that after the insert completes, the app receives a push notification from WNS.

4. (Optional) Repeat steps 1-3 on a different client device and using a different account when logging in.  

	Verify that the notification is received only on this device, since the previous device was not tagged with the current user ID. 



<!-- Anchors. -->
[Updating the service to require authentication for registration]: #register
[Updating the app to log in before registration]: #update-app
[Testing the app]: #test
[Next Steps]:#next-steps


<!-- URLs. -->
[Get started with authentication]: mobile-services-dotnet-backend-windows-store-dotnet-get-started-users.md
[Get started with push notifications]: mobile-services-dotnet-backend-windows-universal-dotnet-get-started-push.md

