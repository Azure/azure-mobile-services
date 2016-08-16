<properties
	pageTitle="What happened to my .NET project after adding Mobile Services by using Visual Studio Connected Services | Microsoft Azure"
	description="Describes what happened in your Visual Studio .NET project after adding Azure Mobile Services by using Connected Services "
	services="mobile-services"
	documentationCenter=""
	authors="mlhoop"
	manager="douge"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="na"
	ms.devlang="dotnet"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="mlearned"/>

# What happened to my Visual Studio .NET project after adding Azure Mobile Services by using Connected Services?

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

## References Added

The Azure Mobile Services NuGet package was added to your project. As a result, the following .NET references were added to your project:

- **Microsoft.WindowsAzure.Mobile**
- **Microsoft.WindowsAzure.Mobile.Ext**
- **Newtonsoft.Json**
- **System.Net.Http.Extensions**
- **System.Net.Http.Primitives**

## Connection string values for Mobile Services

In your App.xaml.cs file, a **MobileServiceClient** object was created with the selected mobile service’s application URL and application key.

## Mobile Services project added

If a .NET mobile service is created in the Connected Service Provider, then a mobile services project is created and added to the solution.


[Learn more about mobile services](https://azure.microsoft.com/documentation/services/mobile-services/)
