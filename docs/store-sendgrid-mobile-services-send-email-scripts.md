<properties
	pageTitle="Send email using SendGrid | Microsoft Azure"
	description="Learn how to use the SendGrid service to send email from your Azure Mobile Services app."
	services="mobile-services"
	documentationCenter=""
	authors="Erikre"
	manager="sendgrid"
	editor=""/>


<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="na"
	ms.devlang="multiple"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="glenga"/>


# Send email from Mobile Services with SendGrid

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

&nbsp;


This topic shows you how can add email functionality to your mobile service. In this tutorial you add server side scripts to send email using SendGrid. When complete, your mobile service will send an email each time a record is inserted.

SendGrid is a [cloud-based email service] that provides reliable [transactional email delivery], scalability, and real-time analytics along with flexible APIs that make custom integration easy. For more information, see <http://sendgrid.com>.

This tutorial walks you through these basic steps to enable email functionality:

1. [Create a SendGrid account]
2. [Add a script to send email]
3. [Insert data to receive email]

This tutorial is based on the Mobile Services quickstart. Before you start this tutorial, you must first complete [Get started with Mobile Services].

## <a name="sign-up"></a>Create a new SendGrid account

Azure customers can unlock 25,000 free emails each month. These 25,000 free monthly emails will give you access to advanced reporting and analytics and [all APIs][] (Web, SMTP, Event, Parse and more). For information about additional services provided by SendGrid, see the [SendGrid Features][] page.

### To sign up for a SendGrid account

1. Log in to the [Azure Management Portal][].

2. In the lower pane of the management portal, click **New**.

	![command-bar-new][command-bar-new]

3. Click **Marketplace**.

	![sendgrid-store][sendgrid-store]

4. In the **Choose an Application and Service** dialog, select **SendGrid** and click the right arrow.

5. In the **Personalize Application and Service** dialog select the **SendGrid** plan you want to sign up for.

6. Enter a name to identify your **SendGrid** service in your Azure settings, or use the default value of **SendGridEmailDelivery.Simplified.SMTPWebAPI**. Names must be between 1 and 100 characters in length and contain only alphanumeric characters, dashes, dots, and underscores. The name must be unique in your list of subscribed Azure Store Items.

	![store-screen-2][store-screen-2]

7. Choose a value for the region; for example, West US.

8. Click the right arrow.

9. On the **Review Purchase** tab, review the plan and pricing information, and review the legal terms. If you agree to the terms, click the check mark. After you click the check mark, your SendGrid account will begin the [SendGrid provisioning process].

	![store-screen-3][store-screen-3]

10. After confirming your purchase you are redirected to the add-ons dashboard and you will see the message **Purchasing SendGrid**.

	![sendgrid-purchasing-message][sendgrid-purchasing-message]

	Your SendGrid account is provisioned immediately and you will see the message **Successfully purchased Add-On SendGrid**. Your account and credentials are now created. You are ready to send emails at this point.

	To modify your subscription plan or see the SendGrid contact settings, click the name of your SendGrid service to open the SendGrid Marketplace dashboard.

	![sendgrid-add-on-dashboard][sendgrid-add-on-dashboard]

	To send an email using SendGrid, you must supply your  account credentials (username and password).

### To find your SendGrid credentials ###

1. Click **Connection Info**.

	![sendgrid-connection-info-button][sendgrid-connection-info-button]

2. In the *Connection info* dialog, copy the **Password** and Username to use later in this tutorial.

	![sendgrid-connection-info][sendgrid-connection-info]

	To set your email deliverability settings, click the **Manage** button. This will redirect to your SendGrid Control Panel.

	![sendgrid-control-panel][sendgrid-control-panel]

	For more information on getting started with SendGrid, see [SendGrid Getting Started][].

## <a name="add-script"></a>Register a new script that sends emails

1. Log on to the [Azure classic portal], click **Mobile Services**, and then click your mobile service.

2. In the Azure classic portal, click the **Data** tab and then click the **TodoItem** table.

	![][1]

3. In **todoitem**, click the **Script** tab and select **Insert**.

	![][2]

	This displays the function that is invoked when an insert occurs in the **TodoItem** table.

4. Replace the insert function with the following code:

        var SendGrid = require('sendgrid').SendGrid;

        function insert(item, user, request) {
            request.execute({
                success: function() {
                    // After the record has been inserted, send the response immediately to the client
                    request.respond();
                    // Send the email in the background
                    sendEmail(item);
                }
            });

            function sendEmail(item) {
                var sendgrid = new SendGrid('**username**', '**password**');

                sendgrid.send({
                    to: '**email-address**',
                    from: '**from-address**',
                    subject: 'New to-do item',
                    text: 'A new to-do was added: ' + item.text
                }, function(success, message) {
                    // If the email failed to send, log it as an error so we can investigate
                    if (!success) {
                        console.error(message);
                    }
                });
            }
        }

5. Replace the placeholders in the above script with the correct values:

	- **_username_ and _password_**: the SendGrid credentials you identified in [Create a SendGrid account].

	- **_email-address_**: the address that the email is sent to. In a real-world app, you can use tables to store and retrieve email addresses. When testing your app, just use your own email address.

	- **_from-address_**: the address from which the email originates. Consider using a registered domain address that belongs to your organization.

     > [AZURE.NOTE] If you do not have a registered domain, you can instead use the domain of your Mobile Service, in the format *notifications@_your-mobile-service_.azure-mobile.net*. However, messages sent to your mobile service domain are ignored.

6. Click the **Save** button. You have now configured a script to send an email each time a record is inserted into the **TodoItem** table.

## <a name="insert-data"></a>Insert test data to receive email

1. In the client app project, run the quickstart application.

	This topic shows the Windows Store version of the quickstart,

2. In the app, type text in **Insert a TodoItem**, and then click **Save**.

	![][3]

3. Notice that you receive an email, such as one shown in the notification below.

	![][4]

	Congratulations, you have successfully configured your mobile service to send email by using SendGrid.

## <a name="nextsteps"> </a>Next Steps

Now that you've seen how easy it is to use the SendGrid email service with Mobile Services, follow
these links to learn more about SendGrid.

-   SendGrid API documentation:
    <https://sendgrid.com/docs>
-   SendGrid special offer for Azure customers:
    <https://sendgrid.com/windowsazure.html>

<!--images-->

[command-bar-new]: ./media/sendgrid-sign-up/sendgrid_BAR_NEW.PNG
[sendgrid-store]: ./media/sendgrid-sign-up/sendgrid_offerings_store.png
[store-screen-2]: ./media/sendgrid-sign-up/sendgrid_store_scrn2.png
[store-screen-3]: ./media/sendgrid-sign-up/sendgrid_store_scrn3.png
[sendgrid-purchasing-message]: ./media/sendgrid-sign-up/sendgrid_purchasing_message.png
[sendgrid-add-on-dashboard]: ./media/sendgrid-sign-up/sendgrid_add-on_dashboard.png
[sendgrid-connection-info]: ./media/sendgrid-sign-up/sendgrid_connection_info.png
[sendgrid-connection-info-button]: ./media/sendgrid-sign-up/sendgrid_connection_info_button.png
[sendgrid-control-panel]: ./media/sendgrid-sign-up/sendgrid_control_panel.png

<!--Links-->

[SendGrid Features]: http://sendgrid.com/features
[Azure Management Portal]: https://manage.windowsazure.com
[SendGrid Getting Started]: http://sendgrid.com/docs
[SendGrid Provisioning Process]: https://support.sendgrid.com/hc/articles/200181628-Why-is-my-account-being-provisioned-
[all APIs]: https://sendgrid.com/docs/API_Reference/index.html

<!-- Anchors. -->
[Create a SendGrid account]: #sign-up
[Add a script to send email]: #add-script
[Insert data to receive email]: #insert-data

<!-- Images. -->
[1]: ./media/store-sendgrid-mobile-services-send-email-scripts/mobile-portal-data-tables.png
[2]: ./media/store-sendgrid-mobile-services-send-email-scripts/mobile-insert-script-push2.png
[3]: ./media/store-sendgrid-mobile-services-send-email-scripts/mobile-quickstart-push1.png
[4]: ./media/store-sendgrid-mobile-services-send-email-scripts/mobile-receive-email.png

<!-- URLs. -->
[Get started with Mobile Services]: https://azure.microsoft.com/develop/mobile/tutorials/get-started
[sign up page]: https://sendgrid.com/windowsazure.html
[Multiple User Credentials page]: https://sendgrid.com/credentials
[Azure classic portal]: https://manage.windowsazure.com/
[cloud-based email service]: https://sendgrid.com/email-solutions
[transactional email delivery]: https://sendgrid.com/transactional-email
