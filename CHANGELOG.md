# Windows Azure Mobile Services Change Log

### Version 1.1.2
**iOS SDK**
- Supports the arm64 architecture
- Now requires iOS 6 or newer to use 

**Javascript SDK**
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns

**Managed SDK**
- Fix [#192](https://github.com/WindowsAzure/azure-mobile-services/issues/192) - Serialized query is ambiguous if double literal has no fractional part

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
