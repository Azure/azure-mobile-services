<properties 
	pageTitle="Send push notifications to authenticated universal Windows app users." 
	description="Learn how to send push notifications from Azure Mobile Services to specific users of your universal Windows C# app." 
	services="mobile-services,notification-hubs" 
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
> For the equivalent Mobile Apps version of this topic, see [How to: Send push notifications to an authenticated user using tags](../app-service-mobile/app-service-mobile-node-backend-how-to-use-server-sdk.md#push-user).

##Overview
This topic shows you how to send push notifications to an authenticated user on any registered device. Unlike the previous [Add push notifications to your app] tutorial, this tutorial changes your mobile service to require that a user be authenticated before the client can register with the notification hub for push notifications. Registration is also modified to add a tag based on the assigned user ID. Finally, the server script is updated to send the notification only to the authenticated user instead of to all registrations.

This tutorial walks you through the following process:

1. [Updating the service to require authentication for registration]
2. [Updating the app to log in before registration]
3. [Testing the app]
 
This tutorial supports both Windows Store and Windows Phone Store apps.

##Prerequisites 

Before you start this tutorial, you must have already completed these Mobile Services tutorials:

+ [Add authentication to your app]<br/>Adds a login requirement to the TodoList sample app.

+ [Add push notifications to your app]<br/>Configures the TodoList sample app for push notifications by using Notification Hubs. 

After you have completed both tutorials, you can prevent unauthenticated users from registering for push notifications from your mobile service.

##<a name="register"></a>Update the service to require authentication to register


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

&nbsp;&nbsp;5. Replace the insert function with the following code, then click **Save**:

	function insert(item, user, request) {
    // Define a payload for the Windows Store toast notification.
    var payload = '<?xml version="1.0" encoding="utf-8"?><toast><visual>' +    
    '<binding template="ToastText01"><text id="1">' +
    item.text + '</text></binding></visual></toast>';

    // Get the ID of the logged-in user.
    var userId = user.userId;		

    request.execute({
        success: function() {
            // If the insert succeeds, send a notification to all devices 
	    	// registered to the logged-in user as a tag.
            	push.wns.send(userId, payload, 'wns/toast', {
                success: function(pushResponse) {
                    console.log("Sent push:", pushResponse);
	    			request.respond();
                    },              
                    error: function (pushResponse) {
                            console.log("Error Sending push:", pushResponse);
	    				request.respond(500, { error: pushResponse });
                        }
                    });
                }
            });
	}

&nbsp;&nbsp;This insert script uses the user ID tag to send a push notification (with the text of the inserted item) to all Windows Store app registrations created by the logged-in user.

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
[Add authentication to your app]: ../mobile-services-windows-store-dotnet-get-started-users.md
[Add push notifications to your app]: ../mobile-services-javascript-backend-windows-store-dotnet-get-started-push.md 