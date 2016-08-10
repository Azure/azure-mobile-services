<properties
	pageTitle="How to define a custom API in a JavaScript backend mobile service | Azure Mobile Services"
	description="Learn how to define a custom API endpoint in a JavaScript backend mobile service."
	services="mobile-services"
	documentationCenter=""
	authors="ggailey777"
	manager="erikre"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-multiple"
	ms.devlang="javascript"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>


# How to: define a custom API endpoint in a JavaScript backend mobile service

> [AZURE.SELECTOR]
- [JavaScript backend](./mobile-services-javascript-backend-define-custom-api.md)
- [.NET backend](./mobile-services-dotnet-backend-define-custom-api.md)

&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).
> For the equivalent Mobile Apps version of this topic, see [How to: Define a custom API controller](../app-service-mobile/app-service-mobile-node-backend-how-to-use-server-sdk.md#CustomAPI).

This topic shows you how to define a custom API endpoint in a JavaScript backend mobile service. A custom API lets you define custom endpoints with server functionality, but it does not map to a database insert, update, delete, or read operation. By using a custom API, you have more control over messaging, including HTTP headers and body format.



1. Log into the [Azure classic portal](https://manage.windowsazure.com/), click **Mobile Services**, and then select your mobile service.

2. Click the **API** tab, and then click **Create**. This displays the **Create a new custom API** dialog.

3. Type _completeall_ in **API name**, and then click the check button to create the new API.

	> [AZURE.TIP] With default permissions, anyone with the app key may call the custom API. However, the application key is not considered a secure credential because it may not be distributed or stored securely. Consider restricting access to only authenticated users for additional security.

4. Click on **completeall** in the API table.

5. Click the **Script** tab, replace the existing code with the following code, then click **Save**. 	This code uses the [mssql object] to access the **todoitem** table directly to set the `complete` flag on all items. Because the **exports.post** function is used, clients send a POST request to perform the operation. The number of changed rows is returned to the client as an integer value.


		exports.post = function(request, response) {
			var mssql = request.service.mssql;
			var sql = "UPDATE todoitem SET complete = 1 " +
                "WHERE complete = 0; SELECT @@ROWCOUNT as count";
			mssql.query(sql, {
				success: function(results) {
					if(results.length == 1)
						response.send(200, results[0]);
				}
			})
		};


> [AZURE.NOTE] The [request](http://msdn.microsoft.com/library/windowsazure/jj554218.aspx) and [response](http://msdn.microsoft.com/library/windowsazure/dn303373.aspx) object supplied to custom API functions are implemented by the [Express.js library](http://go.microsoft.com/fwlink/p/?LinkId=309046). 

<!-- Anchors. -->

<!-- Images. -->

<!-- URLs. -->
[mssql object]: http://msdn.microsoft.com/library/windowsazure/jj554212.aspx

For information on how to invoke a custom API in your app using a Mobile Services client library, see [Call a custom API](mobile-services-windows-dotnet-how-to-use-client-library.md#custom-api) in the client SDK reference.


<!-- Anchors. -->

<!-- Images. -->

<!-- URLs. -->

