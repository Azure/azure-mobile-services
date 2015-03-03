# Azure Mobile Services Change Log

### Version 1.3.1
**Managed SDK**
- Update to latest version of sqlite pcl [ce1aa67](https://github.com/Azure/azure-mobile-services/commit/ce1aa67)
- Fix iOS classic compilation issues [316a57a](https://github.com/Azure/azure-mobile-services/commit/316a57a)
- Update Xamarin unified support for Xamarin.iOS 8.6
[da537b1](https://github.com/Azure/azure-mobile-services/commit/da537b1)
- Xamarin.iOS Unified API Support [d778c60](https://github.com/Azure/azure-mobile-services/commit/d778c60)
- Relax queryId restrictions #521 [offline] 
[3e2f645](https://github.com/Azure/azure-mobile-services/commit/3e2f645)
- Work around for resource missing error on windows phone [offline]

### Version 2.0.0 beta2
**iOS SDK**
- Added support for incremental sync
- Added support for query parameters in pull operations
- Fixed issue with login controller completing before animation completes
- Added a method for force purge of local data
- Added a helper method to return an NSDictionary from an NSManagedObject
- Fixed issue with the __includeDeleted flag sending the wrong value

### Version 2.0.0 beta1

**iOS SDK**
- Added support for following link headers returned from the .NET backend
- **[Breaking]** Changed MSReadQueryBlock to return MSQueryResult instead of items and totalCount

### Version 1.3 
- allow underscore and hyphen in queryId [7d192a3](https://github.com/Azure/azure-mobile-services/commit/7d192a3)
- added force option to purge data and pending operations on data [aa51d9f](https://github.com/Azure/azure-mobile-services/commit/aa51d9f)
- delete errors with operation on cancel and collapse [372ba61](https://github.com/Azure/azure-mobile-services/commit/372ba61)
- rename queryKey to queryId [93e59f7](https://github.com/Azure/azure-mobile-services/commit/93e59f7)
- insert should throw if the item already exists [#491](https://github.com/Azure/azure-mobile-services/issues/491) [fc13891](https://github.com/Azure/azure-mobile-services/commit/fc13891)
- **[Breaking]** Removed PullAsync overloads that do not take queryId [88cac8c](https://github.com/Azure/azure-mobile-services/commit/88cac8c)

### Version 1.3 beta3
**Managed SDK**
- Improved the push failure error message [d49a72e](https://github.com/Azure/azure-mobile-services/commit/d49a72e)
- Implement true upsert [c5b0b38](https://github.com/Azure/azure-mobile-services/commit/c5b0b38)
- Use more fine grained types in sqlite store [de49712](https://github.com/Azure/azure-mobile-services/commit/de49712)
- Speedup store table creation [eb7cc8d](https://github.com/Azure/azure-mobile-services/commit/eb7cc8d)
- Allow query on member name datetime [7d831cd](https://github.com/Azure/azure-mobile-services/commit/7d831cd)
- Make the sync handler optional as there is alternate way for handling sync errors [edc04e5](https://github.com/Azure/azure-mobile-services/commit/edc04e5)
- Drop the unused createdat column in operations table [8a30df4](https://github.com/Azure/azure-mobile-services/commit/8a30df4)
- Remove redundant overloads in interface and move them to extensions [d0a46b6](https://github.com/Azure/azure-mobile-services/commit/d0a46b6)
- Support relative and absolute uri in pull same as table.read [c9d8e39](https://github.com/Azure/azure-mobile-services/commit/c9d8e39)
- Allow relative URI in invokeapi [5b3c6b3](https://github.com/Azure/azure-mobile-services/commit/5b3c6b3)
- Fixed the like implementation in sqlite store [77a0180](https://github.com/Azure/azure-mobile-services/commit/77a0180)
- Purge should forget the deltatoken [18f1803](https://github.com/Azure/azure-mobile-services/commit/18f1803)
- Renamed fromServer to ignoreMissingColumns [8b047eb](https://github.com/Azure/azure-mobile-services/commit/8b047eb)
- **[Breaking]** Removed PullAsync overloads that do not take queryKey [d4ff784](https://github.com/Azure/azure-mobile-services/commit/d4ff784)
- Save tableKind in the errors table [23f2ef0](https://github.com/Azure/azure-mobile-services/commit/23f2ef0)

### Version 1.3 beta2
**Managed SDK**
- Updated Nuget references
- Request __deleted system property for sync
- Default delta token set to 1970-01-01 for compatibility with Table Storage 
- Expose protected methods from the MobileServiceSQLiteStore for intercepting sql
- **[Breaking]** Expose a ReadOnlyCollection instead of IEnumerable from MobileServiceTableOperationError

### Version 1.3 beta
**Managed SDK**
- Added support for incremental sync for .NET backend
- Added support for byte[] properties in offline
- Fixed issue with timezone roundtripping in incremental sync
- Improved exception handling for 409 conflicts
- Improved error handling for timeout errors during sync
- Follow link headers returned from .NET backend and use skip and top for PullAsync()
- Introduced the SupportedOptions enum on IMobileServiceSyncTable to configure the pull strategy
- **[Breaking]** Do not Push changes on PurgeAsync() instead throw an exception
- **[Breaking]** Renamed ToQueryString method to ToODataString on MobileServiceTableQueryDescription class

### Version 1.3 alpha4
**Managed SDK**
- Added support for incremental sync (currently, for Mobile Services JavaScript backend only)
- Added client support for soft delete
- Added support for offline pull with query string

### Version 1.3 alpha1
**iOS SDK**
- Added support for offline and sync

**Managed SDK** 
- Added support for offline and sync
- Added support for soft delete


### Version 1.2.6
**Managed SDK**
- Fixed an issue on Xamarin.iOS and Xamarin.Android where UI popups occur during failed user authentication flows. These popups are now suppressed so that the developer can handle the error however they want.

### Version 1.2.5
**Managed SDK**
- Updated to use a modified build of Xamarin.Auth that will not conflict with any user-included version of Xamarin.Auth

**Javascript SDK**
- Added support for sending provider specific query string parameters in login using new loginWithOptions method
- Added support for registering devices with notification hubs for apns and gcm
- Fixed issue with InAppBrowser on iOS devices during auth workflows when using Cordova/PhoneGap

### Version 1.2.4
**Managed SDK**
- Added support for following link headers returned from the .NET backend
- Added a MobileServiceConflictException to detect duplicate inserts
- Added support for datetimeoffsets in queries
- Added support for sending provider specific query string parameters in LoginAsync()
- Fixed an issue causing duplicate registrations in Xamarin.iOS against .NET backends

**Javascript SDK**
- Fixed crash when server response did not have a Content-Type header

**iOS SDK**
- Address bug where version property was returned to the caller even when not asked for
- Fixes Swift QS for syntax changes up to Xcode Beta 7

**Quickstarts**
- Converted Windows Phone and Windows Store quickstarts to a univeral app quickstart
- Converted WinJS Windows Store quickstart to a universal app quickstart
- Fix syntax issues in iOS Swift quickstart

### Version 1.2.3
**Managed SDK** 
- Added support for Xamarin iOS Azure Notification Hub integration

**iOS SDK**
- Fix issue with const when using both Azure Messaging and Mobile Services frameworks
- Fix issue [#306](https://github.com/Azure/azure-mobile-services/issues/306) with how arrays passed as query string params to table and custom APIs are converted 
- Fix issue where system properties (__version, __updatedAt, etc) were returned to the caller when they were not requested

### Version 1.2.2
**iOS SDK**
- Added support for APNS Azure Notification Hub integration
- Support for optimistic concurrency on delete

**Managed SDK** 
- Support for optimistic concurrency on delete
- Update to Push surface area with minor object model changes. Added Registration base class in PCL and changed name within each extension to match the push notifcation surface. Example: WnsRegistration, WnsTemplateRegistration
- Added support for Xamarin Android Azure Notification Hub integration

**Javascript SDK** 
- Support for optimistic concurrency on delete

### Version 1.2.1
**Managed SDK**
- Added support for Windows Phone 8.1, requires using Visual Studio 2013 Update 2 RC

### Version 1.1.5
**Managed SDK**
- Added support for Xamarin (iOS / Android)
- Clean-up id validation on insert operations

**Javascript SDK**
- Fix issue [#218](https://github.com/WindowsAzure/azure-mobile-services/issues/218) in which some dates coming from the mobile services with the .NET runtime weren't parsed correctly
- [WinJS only] Fix race condition on notification hub integration initialization when storage was corrupted

**iOS SDK**
- - Fix issue [#218](https://github.com/WindowsAzure/azure-mobile-services/issues/218) in which some dates coming from the mobile services with the .NET runtime weren't parsed correctly

**Android SDK**
- Added support for Windows Azure Notification Hub integration

### Version 1.1.4
**Managed SDK**
- Added support for Windows Azure Notification Hub integration.

**Javascript SDK**
- Added support for Windows Azure Notification Hub integration for WinJS.

### Version 1.1.3
**Managed SDK**
- Added support for the Windows Azure Active Directory authentication in the `MobileServiceAuthenticationProvider` enumeration.
- Also added a mapping from that name to the value used in the service REST API (`/login/aad`)
- Fixed a issue [#213](https://github.com/WindowsAzure/azure-mobile-services/issues/213) in which SDK prevented calls to custom APIs with query string parameters starting with `$`

**iOS SDK** / **Javascript SDK**
- Added a mapping in the authentication provider from WindowsAzureActiveDirectory to the value used in the REST API (`/login/aad`)

**Android SDK**
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Added support for the Windows Azure Active Directory authentication in the `MobileServiceAuthenticationProvider` enum.
- Also added a mapping from that name to the value used in the service REST API (`/login/aad`)

### Version 1.1.2
**iOS SDK**
- Supports the arm64 architecture
- Now requires iOS 6 or newer to use 

**Javascript SDK**
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns

**Managed SDK**
- Fix [#192](https://github.com/WindowsAzure/azure-mobile-services/issues/192) - Serialized query is ambiguous if double literal has no fractional part
- Fixed Nuget support for Windows Phone 8

### Version 1.1.1
**iOS SDK**
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Fix bug with using arrays in invokeAPI

**Managed SDK**
- Fix bug when inserting a derived type
- Dropped support for Windows Phone 7.x clients (WP7.5 can still use the client version 1.1.0)

### Version 1.1.0

**All SDKS**
- Support for tables with string ids

**Android SDK**
- Overload for log in which takes the provider as a string, in addition to the one with enums
 
**Javascript SDK**
- Removed client restriction on valid providers for login
- JS SDK files are now served from http://ajax.aspnetcdn.com/ajax/mobileservices/MobileServices.Web-[version].min.js (or [version].js for the non minified copy)

**Managed SDK**
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Overload for log in which takes the provider as a string, in addition to the one with enums
- Fix [#121](https://github.com/WindowsAzure/azure-mobile-services/issues/121) - exceptions in `MobileServiceIncrementalLoadingCollection.LoadMoreItemsAsync` causes the app to crash

### Version 1.0.3:

**Managed SDK**
- Fixed query issues in Visual Basic expressions

**Javascript SDK**
- Added support for `String.substr` inside functions on `where` clauses
- Fix [#152](https://github.com/WindowsAzure/azure-mobile-services/issues/152) - InvokeApi method crashes on IE9 and IE8
- Fixed issue with login popup not being closed when using IE11
