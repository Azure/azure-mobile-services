##Data
Mobile Services gives you an easy way to store data in the cloud.  

When you create a new Mobile Service, you'll be prompted to either create a new SQL database or connect to an existing one.  To reduce latency, be sure to deploy your Mobile Service and SQL database to the same data center.

![](WAMS-SQLdb1.png)

![](WAMS-SQLdb2.png)

Once you've either created a new SQL database for your Mobile Service or connected your Mobile Service to an database, declare your data in your app and insert it using the following syntax:

```csharp
public class Item {
	public int Id { get; set; }
	public string Text { get; set; }
}

Item item = new Item { Text = "Awesome item" };
App.MobileService.GetTable<Item>().InsertAsync (item)
	.ContinueWith (t => { /* success or failure */});
```

Then use familiar LINQ syntax to query data.

You can find the full Getting Started with Data tutorial [here](http://go.microsoft.com/fwlink/?LinkId=282375).

##Auth
You can authenticate users through their Facebook, Twitter, Microsoft, or Google credentials. (A single Mobile Service can simultaneously support multiple forms of identity so you can of course offer your users a choice of how to login.) 

Copy the Client ID and Client  Secret to the appropriate place in the Identity tab. 

![](WAMS-userauth.png)

To allow your users to login with their Facebook credentials, for example, you'd use this code: 

```csharp
App.MobileService.LoginAsync (MobileServiceAuthenticationProvider.Facebook)
	.ContinueWith (t => { /* t.Result is user */ });
```

You can find the full Getting Started with Authentication tutorial [here](http://go.microsoft.com/fwlink/?LinkId=282376).

##Push
To send push notifications, upload your developer certificate under the authentication tab in the Windows Azure portal.

![](WAMS-push1.png)

Mobile Services allows you to easily send push notifications via Apple Push Notification Services (APNS)

```js
push.apns.send (devicetoken, { alert: "Hello to Apple World from Mobile Services!"});
```

You can find the full Getting Started with Push Notifications tutorial [here](http://go.microsoft.com/fwlink/?LinkId=282377).

##Scripts
Mobile Services allows you to add business logic to CRUD operations through secure server-side scripts.  Currently, server-side scripts must be written in JavaScript even though client side code is written in C#.

To add a script, navigate to the 'DATA' tab on the dashbaord and select a table.

![](WAMS-Script1.png)

Then, under the 'SCRIPT' tab, choose either Insert, Update, Delete, or Read from the dropdown menu and copy in your script.  You can find samples for common scripts at http://msdn.microsoft.com/en-us/library/windowsazure/jj591477.aspx.

Learn how to validate data with scripts [here](http://go.microsoft.com/fwlink/?LinkId=282378) and how to authenticate users with scripts [here](http://go.microsoft.com/fwlink/?LinkId=282379).

![](WAMS-Script2.png)

If you'd like to schedule a script to run periodically (rather than when triggerd by a particular event), visit the 'SCHEDULER' tab on the main dashboard and click 'Create a Scheduled Job.'  Then, set the interval at which you would like the script to run.

![](WAMS-Scheduler2.png)

Once you write the script, click 'Save' then 'Run Once.'  Check the 'LOGS' tab on the main dashboard for any errors.  If you're error-free, be sure to return to the 'SCHEDULER' tab and click 'Enable.'

You can find the full tutorial for scheduling recurring jobs [here]( http://go.microsoft.com/fwlink/?LinkId=282380).


To learn about more Mobile Services, visit the [Windows Azure Mobile Developer Center](https://www.windowsazure.com/en-us/develop/mobile/).

