<properties
	pageTitle="Configure IIS Express for local mobile service testing | Azure Mobile Services"
	description="Learn how to configure IIS Express to allow connections to a local mobile service project for testing."
	authors="ggailey777"
	manager="dwrede"
	services="mobile-services"
	documentationCenter=""
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="na"
	ms.devlang="multiple"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>

# Configure the local web server to allow connections to a local mobile service

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
> 
> Learn about [migrating your site to Azure App Service](../articles/app-service-mobile/app-service-mobile-migrating-from-mobile-services.md).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

&nbsp;

Azure Mobile Services enables you create your mobile service in Visual Studio using one of the supported .NET languages and then publish it to Azure. One of the major benefits of using a .NET backend for your mobile service is the ability to run, test, and debug the mobile service on your local computer or virtual machine before publishing it to Azure.

To be able to test a mobile service locally with clients running on an emulator, virtual machine or on a separate workstation, you have to configure the local Web server and host computer to allow connections to the workstation's IP address and port. This topic shows you how to configure IIS Express to enable connections to your locally hosted mobile service.

[AZURE.INCLUDE [mobile-services-how-to-configure-iis-express](../../includes/mobile-services-how-to-configure-iis-express.md)]
