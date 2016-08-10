<properties
	pageTitle="Get started with authentication (Xamarin.Android) - Mobile Services"
	description="Learn how to use authentication in your Azure Mobile Services app for Xamarin.Android."
	services="mobile-services"
	documentationCenter="xamarin"
	manager="dwrede"
	authors="lindydonna"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-xamarin-android"
	ms.devlang="dotnet"
	ms.topic="article"
	ms.date="07/21/2016" 
	ms.author="donnam"/>

# Add authentication to your Mobile Services app

[AZURE.INCLUDE [mobile-services-selector-get-started-users](../../includes/mobile-services-selector-get-started-users.md)]

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).
> For the equivalent Mobile Apps version of this topic, see [Add authentication to your Xamarin.Android app](../app-service-mobile/app-service-mobile-xamarin-android-get-started-users.md).

<p>This topic shows you how to authenticate users in Azure Mobile Services from your Xamarin.Android app. In this tutorial, you add authentication to the quickstart project using an identity provider that is supported by Mobile Services. After being successfully authenticated and authorized by Mobile Services, the user ID value is displayed.</p>

This tutorial walks you through these basic steps to enable authentication in your app:

1. [Register your app for authentication and configure Mobile Services]
2. [Restrict table permissions to authenticated users]
3. [Add authentication to the app]

This tutorial is based on the Mobile Services quickstart. You must also first complete the tutorial [Get started with Mobile Services].

Completing this tutorial requires Visual Studio with Xamarin on Windows or Xamarin Studio on Mac OS X. Complete installation instructions are on [Setup and Install for Visual Studio and Xamarin](https://msdn.microsoft.com/library/mt613162.aspx). 

##<a name="register"></a>Register your app for authentication and configure Mobile Services

[AZURE.INCLUDE [mobile-services-register-authentication](../../includes/mobile-services-register-authentication.md)]

##<a name="permissions"></a>Restrict permissions to authenticated users


[AZURE.INCLUDE [mobile-services-restrict-permissions-javascript-backend](../../includes/mobile-services-restrict-permissions-javascript-backend.md)]


3. In Xamarin Studio, open the project that you created when you completed the tutorial [Get started with Mobile Services].

4. From the **Run** menu, click **Start debugging** to start the app; verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

	 This happens because the app attempts to access Mobile Services as an unauthenticated user, but the _TodoItem_ table now requires authentication.

Next, you will update the app to authenticate users before requesting resources from the mobile service.

##<a name="add-authentication"></a>Add authentication to the app

1. Add the following property to the **ToDoActivity** class:

		private MobileServiceUser user;

2. Add the following method to the **ToDoActivity** class:

        private async Task Authenticate()
        {
            try
            {
                user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.MicrosoftAccount);
                CreateAndShowDialog(string.Format("you are now logged in - {0}", user.UserId), "Logged in!");
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
        }

    This creates a new method to handle the authentication process. The user is authenticated by using a Microsoft Account login. A dialog is displayed which displays the ID of the authenticated user. You cannot proceed without a positive authentication.

    > [AZURE.NOTE] If you are using an identity provider other than Microsoft, change the value passed to the **login** method above to one of the following: _Facebook_, _Google_, _Twitter_, or _WindowsAzureActiveDirectory_.

3. In the **OnCreate** method, add the following line of code after the code that instantiates the `MobileServiceClient` object.

		await Authenticate();

	This call starts the authentication process and awaits it asynchronously.

4. Move the remaining code after `await Authenticate();` in the **OnCreate** method to a new **CreateTable** method, which looks like this:

        private async Task CreateTable()
        {

            await InitLocalStoreAsync();

            // Get the Mobile Service Table instance to use
            toDoTable = client.GetSyncTable<ToDoItem>();

            textNewToDo = FindViewById<EditText>(Resource.Id.textNewToDo);

            // Create an adapter to bind the items with the view
            adapter = new ToDoItemAdapter(this, Resource.Layout.Row_List_To_Do);
            var listViewTodo = FindViewById<ListView>(Resource.Id.listViewToDo);
            listViewTodo.Adapter = adapter;

            // Load the items from the Mobile Service
            await RefreshItemsFromTableAsync();
        }

5. Then call the new **CreateTable** method in **OnCreate** after the **Authenticate** call added in step 2:

		await CreateTable();


6. From the **Run** menu, click **Start debugging** to start the app and sign in with your chosen identity provider.

   	When you are successfully logged-in, the app should run without errors, and you should be able to query Mobile Services and make updates to data.

## Get completed example
Download the [completed example project]. Be sure to update the **applicationURL** and **applicationKey** variables with your own Azure settings.

## <a name="next-steps"></a>Next steps

In the next tutorial, [Authorize users with scripts], you will take the user ID value provided by Mobile Services based on an authenticated user and use it to filter the data returned by Mobile Services.

<!-- Anchors. -->
[Register your app for authentication and configure Mobile Services]: #register
[Restrict table permissions to authenticated users]: #permissions
[Add authentication to the app]: #add-authentication
[Next Steps]:#next-steps

<!-- Images. -->
[4]: ./media/partner-xamarin-mobile-services-android-get-started-users/mobile-services-selection.png
[5]: ./media/partner-xamarin-mobile-services-android-get-started-users/mobile-service-uri.png

[13]: ./media/partner-xamarin-mobile-services-android-get-started-users/mobile-identity-tab.png
[14]: ./media/partner-xamarin-mobile-services-android-get-started-users/mobile-portal-data-tables.png
[15]: ./media/partner-xamarin-mobile-services-android-get-started-users/mobile-portal-change-table-perms.png

<!-- URLs. -->
[Get started with Mobile Services]: partner-xamarin-mobile-services-android-get-started.md
[Authorize users with scripts]: mobile-services-javascript-backend-service-side-authorization.md
[completed example project]: http://go.microsoft.com/fwlink/p/?LinkId=331328
