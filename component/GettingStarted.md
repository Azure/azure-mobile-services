##Getting Started

Mobile Services offers an easy way to store data in the cloud, authenticate users, and send push notifications.


To use Mobile Services with your iOS app, you will need a Windows Azure account.  If you already have an account, login to the [Windows Azure management portal](https://manage.windowsazure.com/).  If you are new to Windows Azure, you can sign up for a 90-day free trial [here](https://www.windowsazure.com/en-us/pricing/free-trial/).

To create a new Mobile Service after you've logged into the [management portal](https://manage.windowsazure.com/), select 'New' --> 'Compute' --> 'Mobile Service' --> 'Create.'  

![](WAMS-New.PNG)

Even though you will write the majority of your application in your preferred IDE, the management portal provides an easy way to work with three key Mobile Services features: storing data in the cloud, settuing up user authentication via third party services like Facebook, and sending push notifications.

You can find the full Getting Started with Mobile Services tutorial [here]( http://go.microsoft.com/fwlink/?LinkId=282374).

## Connect a Mobile Service to your Xamarin app

After you've created a Mobile Service, use the following to connect your project:

```csharp
using Microsoft.WindowsAzure.MobileServices;
...

public static MobileServiceClient MobileService = new MobileServiceClient(
"https://yourMobileServiceName.azure-mobile.net/", 
"YOUR_APPLICATION_KEY"
);
```

You can find value of your Mobile Service URL on the right-hand side of Dashboard and the value of your app key by clicking 'Manage Keys' at the bottom of the Dashboard.

![](WAMS-Keys.png)

##Store Data in the Cloud

When you create a Mobile Service, you'll be prompted to either create a new SQL database for that Mobile Service or connect your Mobile Service to an existing one. 

You then add a table to that SQL database by going to the 'Data' tab and hitting 'Create.'

![](WAMS-EmptyData.png)

You'll then be prompted to set permissions for the table.

![](WAMS-DataPermissions.png)

To store data in that table, use the following code snippet (originally from the [September 2012 announcement](http://blog.xamarin.com/xamarin-partners-with-microsoft-to-support-azure-mobile-services-on-android-and-ios/) of the Xamarin and Windows Azure partnership):

```csharp 
public class TodoItem
{
	public int Id { get; set; }
	[DataMember (Name = "text")]
	public string Text { get; set; }
	[DataMember (Name = "complete")]
	public bool Complete { get; set; }
}

...

this.table = MobileService.GetTable<TodoItem>();
this.table.Where (ti => !ti.Complete).ToListAsync()
	.ContinueWith (t => { this.items = t.Result; }, scheduler);
```

### Documentation

- Tutorials: https://www.windowsazure.com/en-us/develop/mobile/resources/
- Developer Center: http://www.windowsazure.com/mobile
- API Library: http://msdn.microsoft.com/en-us/library/windowsazure/jj710108.aspx
- Mobile Services GitHub Repo: https://github.com/WindowsAzure/azure-mobile-services
- Xamarin Mobile Services client framework GitHub Repo: https://github.com/xamarin/azure-mobile-services

### Contact

- Developer Forum: http://social.msdn.microsoft.com/Forums/en-US/azuremobile/threads
- Feature Requests: http://mobileservices.uservoice.com
- Contact: mobileservices@microsoft.com
- Twitter: @joshtwist @cloudnick @chrisrisner @mlunes90

###Legal 

- Terms & Conditions: http://www.windowsazure.com/en-us/support/legal/
