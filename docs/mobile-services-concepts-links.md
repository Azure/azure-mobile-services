<properties
	pageTitle="Mobile Services Concepts"
	description="Links to Mobile Services concepts topics found in the Help Drawer in the Azure classic portal."
	services="mobile-services"
	documentationCenter="na"
	authors="ggailey777"
	manager="dwrede"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-multiple"
	ms.devlang="na"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>

# Mobile Services concepts

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

The topics linked below provide information about specific behaviors of Microsoft Azure Mobile Services. These same topics are available as help in the Azure classic portal.

## <a name="what-is"></a>What is Mobile Services

Azure Mobile Services is a highly scalable mobile application development platform that lets you add enhanced functionality to your mobile device apps by using Azure. 

With Mobile Services you can: 

+ **Build native and cross platform apps** - Connect your iOS, Android, Windows, or cross-platform Xamarin or Cordova (Phonegap) apps to your backend mobile service using native SDKs.  
+ **Send push notifications to your users** - Send push notifications to your users of your app.
+ **Authenticate your users** - Leverage popular identity providers like Facebook and Twitter to authenticate your app users.
+ **Store data in the cloud** - Store user data in a SQL Database (by default) or in Mongo DB, DocumentDB, Azure Tables, or Azure Blobs. 
+ **Build offline-ready apps with sync** - Make your apps work offline and use Mobile Services to sync data in the background.
+ **Monitor and scale your apps** - Monitor app usage and scale your backend as demand grows. 

## <a name="concepts"> </a>Mobile Services Concepts

The following are important features and concepts in the Mobile Services:

+ **Application key:** a unique value that is used to limit access to your mobile service from random clients; this "key" is not a security token and is not used to authenticate users of your app.    
+ **Backend:** the mobile service instance that supports your app. A mobile service is implemented either as an ASP.NET Web API project (*.NET backend* ) or as a Node.js project (*JavaScript backend*).
+ **Identity provider:** an external service, trusted by Mobile Services, that authenticates your app's users. Supported providers include: Facebook, Twitter, Google, Microsoft Account, and Azure Active Directory. 
+ **Push notification:** Service-initiated message that is sent to a registered device or user using Azure Notification Hubs.
+ **Scale:** The ability to add, for an additional cost, more processing power, performance, and storage as your app becomes more popular.
+ **Scheduled job:** Custom code that is run either on a pre-determined schedule or on-demand.

For more information, see [Mobile Services Concepts](../articles/mobile-services/mobile-services-concepts-links.md).

The [overview topic](https://msdn.microsoft.com/library/azure/jj193167.aspx) describes the benefits of using Mobile Services and what tasks can be performed in the Azure classic portal. For more general information about Mobile Services and examples of how to use Mobile Services in your apps, see [Mobile Services Tutorials and Resources](https://azure.microsoft.com/documentation/services/mobile-services/).

##Configuration
The following topics provide information about creating, deleting and configuring Mobile Services:

- [Create a mobile service](https://msdn.microsoft.com/library/azure/jj193169.aspx)
- [Delete a mobile service](https://msdn.microsoft.com/library/azure/jj193173.aspx)
- [Manage keys](https://msdn.microsoft.com/library/azure/jj193164.aspx)
- [Change the database](https://msdn.microsoft.com/library/azure/jj193170.aspx)
- [Scaling a mobile service](https://msdn.microsoft.com/library/azure/jj193178.aspx)
- [Configure identity](https://msdn.microsoft.com/library/azure/jj591527.aspx)

##Data access
The following topics provide information about accessing and changing app data stored in Mobile Services:

- [Work with data](https://msdn.microsoft.com/library/azure/jj631634.aspx)
- [Create a table](https://msdn.microsoft.com/library/azure/jj193162.aspx)
- [Permissions](https://msdn.microsoft.com/library/azure/jj193161.aspx)
- [Dynamic schema](https://msdn.microsoft.com/library/azure/jj193175.aspx)
- [Browse records](https://msdn.microsoft.com/library/azure/jj193171.aspx)
- [Delete data](https://msdn.microsoft.com/library/azure/jj908633.aspx)
- [Manage columns](https://msdn.microsoft.com/library/azure/jj193177.aspx)
- [System columns](https://msdn.microsoft.com/library/azure/dn518225.aspx)

##Push notifications
The following topics provide information about configuring push notifications in Mobile Services:

- [Configure push notifications](https://msdn.microsoft.com/library/azure/jj591526.aspx)
- [Register endpoint](https://msdn.microsoft.com/library/azure/dn771685.aspx)
- [Send push notifications](https://msdn.microsoft.com/library/azure/jj631630.aspx)

##Scheduled jobs
The following topics provide information about working with scheduled jobs:

- [Schedule jobs](https://msdn.microsoft.com/library/azure/jj860528.aspx)
- [Configure jobs](https://msdn.microsoft.com/library/azure/jj899833.aspx)
- [Register job scripts](https://msdn.microsoft.com/library/azure/jj899832.aspx)

##Custom APIs
The following topics provide information about defining custom HTTP endpoints in Mobile Services:

- [Custom API](https://msdn.microsoft.com/library/azure/dn280974.aspx)
- [Content types and headers](https://msdn.microsoft.com/library/azure/dn303369.aspx)

##JavaScript backend server scripts
The following topics provide examples of how to perform tasks using server scripts in a JavaScript backend mobile services:

- [Using scripts](https://msdn.microsoft.com/library/azure/jj193174.aspx)
- [Script debugging](https://msdn.microsoft.com/library/azure/jj631636.aspx)
- [Read and write data](https://msdn.microsoft.com/library/azure/jj631640.aspx)
- [Modify the request](https://msdn.microsoft.com/library/azure/jj631635.aspx)
- [Modify the response](https://msdn.microsoft.com/library/azure/jj631631.aspx)
- [Validate data](https://msdn.microsoft.com/library/azure/jj631638.aspx)
- [Send push notification](https://msdn.microsoft.com/library/azure/jj631630.aspx)
- [Send HTTP request](https://msdn.microsoft.com/library/azure/jj631641.aspx)
- [Authorize user](https://msdn.microsoft.com/library/azure/jj631637.aspx)
- [Restrict access to admins](https://msdn.microsoft.com/library/azure/jj712649.aspx)
- [Error handling](https://msdn.microsoft.com/library/azure/jj631632.aspx)
- [Shortcut keys](https://msdn.microsoft.com/library/azure/jj552469.aspx)







