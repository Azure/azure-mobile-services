# Microsoft Azure Mobile Services Change Log
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
