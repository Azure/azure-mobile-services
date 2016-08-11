<properties
	pageTitle="How to Use iOS Client Library for Azure Mobile Services"
	description="How to Use iOS Client Library for Mobile Services"
	services="mobile-services"
	documentationCenter="ios"
	authors="krisragh"
	manager="dwrede"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-ios"
	ms.devlang="objective-c"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="krisragh"/>

# How to Use iOS Client Library for Azure Mobile Services

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

&nbsp;


> [AZURE.SELECTOR]
- [Android](mobile-services-android-how-to-use-client-library.md)
- [HTML/JavaScript](mobile-services-html-how-to-use-client-library.md)
- [iOS](mobile-services-ios-how-to-use-client-library.md)
- [Managed (Windows/Xamarin)](mobile-services-dotnet-how-to-use-client-library.md)

This guide teaches you to perform common scenarios using the Azure Mobile Services [iOS SDK]. If you are new to Mobile Services, first complete [Mobile Services Quick Start] to configure your account, create a table, and create a mobile service.

> [AZURE.NOTE] This guide uses the latest [iOS Mobile Services SDK](https://go.microsoft.com/fwLink/?LinkID=266533&clcid=0x409). If your project uses an older version of the SDK, first upgrade the framework in Xcode.

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

For more information, see [Mobile Services Concepts](./
mobile-services-concepts-links.md).

##<a name="Setup"></a>Setup and Prerequisites

This guide assumes that you have created a mobile service with a table. For more information see [Create a table], or reuse the `TodoItem` table created in [Mobile Services Quick Start]. This guide assumes that the table has the same schema as the tables in those tutorials. This guide also assumes that your Xcode references `WindowsAzureMobileServices.framework` and imports `WindowsAzureMobileServices/WindowsAzureMobileServices.h`.

##<a name="create-client"></a>How to: Create Mobile Services Client

To access an Azure mobile service in your project, create an `MSClient` client object. Replace `AppUrl` and `AppKey` with the mobile service URL and the application key Dashboard values, respectively.

```
MSClient *client = [MSClient clientWithApplicationURLString:@"AppUrl" applicationKey:@"AppKey"];
```

##<a name="table-reference"></a>How to: Create Table Reference

To access or update data for your Azure mobile service, create a reference to the table. Replace `TodoItem` with the name of your table.

```
	MSTable *table = [client tableWithName:@"TodoItem"];
```

##<a name="querying"></a>How to: Query Data

To create a database query, query the `MSTable` object. The following query gets all the items in `TodoItem` and logs the text of each item.

```
[table readWithCompletion:^(MSQueryResult *result, NSError *error) {
		if(error) { // error is nil if no error occured
				NSLog(@"ERROR %@", error);
		} else {
				for(NSDictionary *item in result.items) { // items is NSArray of records that match query
						NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
				}
		}
}];
```

##<a name="filtering"></a>How to: Filter Returned Data

To filter results, there are many available options.

To filter using a predicate, use an `NSPredicate` and `readWithPredicate`. The following filters returned data to find only incomplete Todo items.

```
// Create a predicate that finds items where complete is false
NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
// Query the TodoItem table and update the items property with the results from the service
[table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
		if(error) {
				NSLog(@"ERROR %@", error);
		} else {
				for(NSDictionary *item in result.items) {
						NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
				}
		}
}];
```

##<a name="query-object"></a>How to: Use MSQuery

To perform a complex query (including sorting and paging), create an `MSQuery` object, directly or by using a predicate:

```
    MSQuery *query = [table query];
    MSQuery *query = [table queryWithPredicate: [NSPredicate predicateWithFormat:@"complete == NO"]];
```

`MSQuery` lets you control several query behaviors, including the following. Execute an `MSQuery` query by calling `readWithCompletion` on it, as shown in the next example.
* Specify order of results
* Limit which fields to return
* Limit how many records to return
* Specify total count in response
* Specify custom query string parameters in request
* Apply additional functions


## <a name="sorting"></a>How to: Sort Data with MSQuery

To sort results, let's look at an example. To first ascendingly by field `text` and then descendingly by field `completion`, invoke `MSQuery` like so:

```
[query orderByAscending:@"text"];
[query orderByDescending:@"complete"];
[query readWithCompletion:^(MSQueryResult *result, NSError *error) {
		if(error) {
				NSLog(@"ERROR %@", error);
		} else {
				for(NSDictionary *item in result.items) {
						NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
				}
		}
}];
```

## <a name="paging"></a>How to: Return Data in Pages with MSQuery

Mobile Services limits the amount of records that are returned in a single response. To control the number of records displayed to your users you must implement a paging system.  Paging is performed by using the following three properties of the **MSQuery** object:

```
+	`BOOL includeTotalCount`
+	`NSInteger fetchLimit`
+	`NSInteger fetchOffset`
```

In the following example, a simple function requests 5 records from the server and then appends them to the local collection of previously loaded records:

```
// Create and initialize these properties
@property (nonatomic, strong)   NSMutableArray *loadedItems; // Init via [[NSMutableArray alloc] init]
@property (nonatomic)   				BOOL moreResults;
```

```
-(void)loadResults
{
    MSQuery *query = [self.table query];

    query.includeTotalCount = YES;
    query.fetchLimit = 5;
    query.fetchOffset = self.loadedItems.count;


    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        if(!error) {
            // Add the items to our local copy
            [self.loadedItems addObjectsFromArray:result.items];

            // Set a flag to keep track if there are any additional records we need to load
            self.moreResults = (self.loadedItems.count <= result.totalCount);
        }
    }];
}

```

## <a name="selecting"></a><a name="parameters"></a>How to: Limit Fields and Expand Query String Parameters with MSQuery

To limit fields to be returned in a query, specify the names of the fields in the **selectFields** property. This returns only the text and completed fields:

```
	query.selectFields = @[@"text", @"completed"];
```

To include additional query string parameters in the server request (for example, because a custom server-side script uses them), populate `query.parameters` like so:

```
	query.parameters = @{
		@"myKey1" : @"value1",
		@"myKey2" : @"value2",
	};
```

##<a name="inserting"></a>How to: Insert Data

To insert a new table row, create a new `NSDictionary` and invoke `table insert`. Mobile Services automatically generates new columns based on the `NSDictionary` if [Dynamic Schema] is not disabled.

If `id` is not provided, the backend automatically generates a new unique ID. Provide your own `id` to use email addresses, usernames, or your own custom values as ID. Providing your own ID may ease joins and business-oriented database logic.

```
	NSDictionary *newItem = @{@"id": @"custom-id", @"text": @"my new item", @"complete" : @NO};
	[self.table insert:newItem completion:^(NSDictionary *result, NSError *error) {
		// The result contains the new item that was inserted,
		// depending on your server scripts it may have additional or modified
		// data compared to what was passed to the server.
		if(error) {
				NSLog(@"ERROR %@", error);
		} else {
						NSLog(@"Todo Item: %@", [result objectForKey:@"text"]);
		}
	}];
```

##<a name="modifying"></a>How to: Modify Data

To update an existing row, modify an item and call `update`:

```
	NSMutableDictionary *newItem = [oldItem mutableCopy]; // oldItem is NSDictionary
	[newItem setValue:@"Updated text" forKey:@"text"];
	[self.table update:newItem completion:^(NSDictionary *item, NSError *error) {
		// Handle error or perform additional logic as needed
	}];
```

Alternatively, supply the row ID and the updated field:

```
	[self.table update:@{@"id":@"37BBF396-11F0-4B39-85C8-B319C729AF6D", @"Complete":@YES} completion:^(NSDictionary *item, NSError *error) {
		// Handle error or perform additional logic as needed
	}];
```

At minimum, the `id` attribute must be set when making updates.

##<a name="deleting"></a>How to: Delete Data

To delete an item, invoke `delete` with the item:

```
	[self.table delete:item completion:^(id itemId, NSError *error) {
		// Handle error or perform additional logic as needed
	}];
```

Alternatively, delete by providing a row ID:

```
	[self.table deleteWithId:@"37BBF396-11F0-4B39-85C8-B319C729AF6D" completion:^(id itemId, NSError *error) {
		// Handle error or perform additional logic as needed
	}];
```

At minimum, the `id` attribute must be set when making deletes.

##<a name="#custom-api"></a>How to: Call a custom API

A custom API enables you to define custom endpoints that expose server functionality that does not map to an insert, update, delete, or read operation. By using a custom API, you can have more control over messaging, including reading and setting HTTP message headers and defining a message body format other than JSON. For an example of how to create a custom API in your mobile service, see [How to: define a custom API endpoint](mobile-services-dotnet-backend-define-custom-api.md).


### Call custom API from iOS app

To call this custom API from an iOS client, use the `MSClient invokeAPI` method. There are two versions of this method, one for JSON-formatted requests, and one for any data type:

	/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
	/// response content will be treated as JSON.
	-(void)invokeAPI:(NSString *)APIName
	            body:(id)body
	      HTTPMethod:(NSString *)method
	      parameters:(NSDictionary *)parameters
	         headers:(NSDictionary *)headers
	      completion:(MSAPIBlock)completion;

	/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
	/// response content can be of any media type.
	-(void)invokeAPI:(NSString *)APIName
	            data:(NSData *)data
	      HTTPMethod:(NSString *)method
	      parameters:(NSDictionary *)parameters
	         headers:(NSDictionary *)headers
	      completion:(MSAPIDataBlock)completion;


For example, to send a JSON request to a custom API named "sendEmail", pass an `NSDictionary` for the request parameters:

	NSDictionary *emailHeader = @{ @"to": @"email.com", @"subject" : @"value" };

	[self.client invokeAPI:@"sendEmail"
	     body:emailBody
	     HTTPMethod:@"POST"
	     parameters:emailHeader
	     headers:nil
	     completion:completion ];

If you need the data returned then you can use something like this:

	[self.client invokeAPI:apiName
                 body:yourBody
           HTTPMethod:httpMethod
           parameters:parameters
              headers:headers
           completion:  ^(NSData *result,
                          NSHTTPURLResponse *response,
                          NSError *error){
               // error is nil if no error occured
               if(error) {
                   NSLog(@"ERROR %@", error);
               } else {

		// do something with the result
               }


           }];





##<a name="authentication"></a>How to: Authenticate Users

Azure Mobile Services supports various identity providers. For a basic tutorial, see [Authentication].

Azure Mobile Services supports two authentication workflows:

- **Server-managed Login**: Azure Mobile Services manages the login process on behalf of your app. It displays a provider-specific login page and authenticates with the chosen provider.

- **Client-managed Login**: The _app_ requests a token from the identity provider and presents this token to Azure Mobile Services for authentication.

When authentication succeeds, you get back a user object with a user ID value and the auth token. To use this user ID to authorize users, see [Service-side Authorization]. To restrict table access to only authenticated users, see [Permissions].

### Server-managed Login

Here is how you can add server-managed login to the [Mobile Services Quick Start] project; you may use similar code for your other projects. For more information and to see an end-to-end example in action, see [Authentication].

* Open **QSTodoListViewController.m** and add the following method. Change _facebook_ to _microsoftaccount_, _twitter_, _google_, or _windowsazureactivedirectory_ if you're not using Facebook as your identity provider.

```
        - (void) loginAndGetData
        {
            MSClient *client = self.todoService.client;
            if (client.currentUser != nil) {
                return;
            }

            [client loginWithProvider:@"facebook" controller:self animated:YES completion:^(MSUser *user, NSError *error) {
                [self refresh];
            }];
        }
```

* Replace `[self refresh]` in `viewDidLoad` with the following:

```
        [self loginAndGetData];
```

* Press  **Run** to start the app, and then log in. When you are logged in, you should be able to view the Todo list and make updates.

### Client-managed Login (Single Sign-on)

You may do the login process outside the Mobile Services client, either to enable single sign-on or if your app contacts the identity provider directly. In such cases, you can log in to Mobile Services by providing a token obtained independently from a supported identity provider.

The following example uses the [Live Connect SDK] to enable single sign-on for iOS apps. It assumes that you have a **LiveConnectClient** instance named `liveClient` in the controller and the user is logged in.

```
	[client loginWithProvider:@"microsoftaccount"
		token:@{@"authenticationToken" : self.liveClient.session.authenticationToken}
		completion:^(MSUser *user, NSError *error) {
				// Handle success and errors
	}];
```

##<a name="caching-tokens"></a>How to: Cache Authentication Tokens

Let's see how you may cache tokens in the [Mobile Services Quick Start] project; you may apply similar steps to any project.

* The recommended way to encrypt and store authentication tokens on an iOS client is use the iOS Keychain. We'll use [SSKeychain](https://github.com/soffes/sskeychain) -- a simple wrapper around the iOS Keychain. Follow the instructions on the SSKeychain page and add it to your project. Verify that the **Enable Modules** setting is enabled in the project's **Build Settings** (section **Apple LLVM - Languages - Modules**.)


##<a name="errors"></a>How to: Handle Errors

When you call a mobile service, the completion block contains an `NSError *error` parameter. When an error occurs, this parameter is non-nil. In your code, you should check this parameter and handle the error as needed.

The file [`<WindowsAzureMobileServices/MSError.h>`](https://github.com/Azure/azure-mobile-services/blob/master/sdk/iOS/src/MSError.h) defines the constants `MSErrorResponseKey`, `MSErrorRequestKey`, and `MSErrorServerItemKey` to get more data related to the error. In addition, the file defines constants for each error code. For an example on how to use these constants, see [Conflict-Handler] for its usage of `MSErrorServerItemKey` and `MSErrorPreconditionFailed`.

<!-- Anchors. -->

[What is Mobile Services]: #what-is
[Concepts]: #concepts
[Setup and Prerequisites]: #Setup
[How to: Create the Mobile Services client]: #create-client
[How to: Create a table reference]: #table-reference
[How to: Query data from a mobile service]: #querying
[Filter returned data]: #filtering
[Sort returned data]: #sorting
[Return data in pages]: #paging
[Select specific columns]: #selecting
[How to: Bind data to the user interface]: #binding
[How to: Insert data into a mobile service]: #inserting
[How to: Modify data in a mobile service]: #modifying
[How to: Authenticate users]: #authentication
[Cache authentication tokens]: #caching-tokens
[How to: Upload images and large files]: #blobs
[How to: Handle errors]: #errors
[How to: Design unit tests]: #unit-testing
[How to: Customize the client]: #customizing
[Customize request headers]: #custom-headers
[Customize data type serialization]: #custom-serialization
[Next Steps]: #next-steps
[How to: Use MSQuery]: #query-object

<!-- Images. -->

<!-- URLs. -->
[Mobile Services Quick Start]: mobile-services-ios-get-started.md
[Get started with Mobile Services]: mobile-services-ios-get-started.md
[Mobile Services SDK]: https://go.microsoft.com/fwLink/p/?LinkID=266533
[Authentication]: /develop/mobile/tutorials/get-started-with-users-ios
[iOS SDK]: https://developer.apple.com/xcode

[Handling Expired Tokens]: http://go.microsoft.com/fwlink/p/?LinkId=301955
[Live Connect SDK]: http://go.microsoft.com/fwlink/p/?LinkId=301960
[Permissions]: http://msdn.microsoft.com/library/windowsazure/jj193161.aspx
[Service-side Authorization]: mobile-services-javascript-backend-service-side-authorization.md
[Dynamic Schema]: http://go.microsoft.com/fwlink/p/?LinkId=296271
[Create a table]: http://msdn.microsoft.com/library/windowsazure/jj193162.aspx
[NSDictionary object]: http://go.microsoft.com/fwlink/p/?LinkId=301965
[ASCII control codes C0 and C1]: http://en.wikipedia.org/wiki/Data_link_escape_character#C1_set
[CLI to manage Mobile Services tables]: https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-command-line-tools/#Mobile_Tables
[Conflict-Handler]: mobile-services-ios-handling-conflicts-offline-data.md#add-conflict-handling
