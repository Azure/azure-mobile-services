# Azure Mobile Services JavaScript SDK Change Log

### JavaScript SDK: Version 1.2.7
- Added support for phonegap/cordova with [plugin repo](https://github.com/Azure/azure-mobile-services-cordova)

### JavaScript SDK: Version 1.2.5
- Added support for sending provider specific query string parameters in login using new loginWithOptions method
- Added support for registering devices with notification hubs for apns and gcm
- Fixed issue with InAppBrowser on iOS devices during auth workflows when using Cordova/PhoneGap

### JavaScript SDK: Version 1.2.4
- Fixed crash when server response did not have a Content-Type header

### JavaScript SDK: Version 1.2.2 
- Support for optimistic concurrency on delete

### JavaScript SDK: Version 1.1.5
- Fix issue [#218](https://github.com/WindowsAzure/azure-mobile-services/issues/218) in which some dates coming from the mobile services with the .NET runtime weren't parsed correctly
- [WinJS only] Fix race condition on notification hub integration initialization when storage was corrupted

### JavaScript SDK: Version 1.1.4
- Added support for Windows Azure Notification Hub integration for WinJS.

### JavaScript SDK: Version 1.1.3
- Added a mapping in the authentication provider from WindowsAzureActiveDirectory to the value used in the REST API (`/login/aad`)

### JavaScript SDK: Version 1.1.2
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns

### JavaScript SDK: Version 1.1.0
- Support for tables with string ids
- Removed client restriction on valid providers for login
- Files are now served from http://ajax.aspnetcdn.com/ajax/mobileservices/MobileServices.Web-[version].min.js (or [version].js for the non minified copy)

### JavaScript SDK: Version 1.0.3:
- Added support for `String.substr` inside functions on `where` clauses
- Fix [#152](https://github.com/WindowsAzure/azure-mobile-services/issues/152) - InvokeApi method crashes on IE9 and IE8
- Fixed issue with login popup not being closed when using IE11
