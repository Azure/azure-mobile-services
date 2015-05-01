# Azure Mobile Services Managed SDK Change Log

### Managed SDK: Version 1.3.2
- Added workaround for WinRT issue [#658](https://github.com/WindowsAzure/azure-mobile-services/issues/658) by removing localization in SQLiteStore and in the SDK  [6af8b30](https://github.com/Azure/azure-mobile-services/commit/6af8b30) [58c5a44](https://github.com/Azure/azure-mobile-services/commit/58c5a44)
- Added partial fix for issue [#615](https://github.com/WindowsAzure/azure-mobile-services/issues/615), by removing operations from the queue before releasing the operation's lock. [a28ae32](https://github.com/Azure/azure-mobile-services/commit/a28ae32)

### Managed SDK: Version 1.3.1
- Update to latest version of sqlite pcl [ce1aa67](https://github.com/Azure/azure-mobile-services/commit/ce1aa67)
- Fix iOS classic compilation issues [316a57a](https://github.com/Azure/azure-mobile-services/commit/316a57a)
- Update Xamarin unified support for Xamarin.iOS 8.6
[da537b1](https://github.com/Azure/azure-mobile-services/commit/da537b1)
- Xamarin.iOS Unified API Support [d778c60](https://github.com/Azure/azure-mobile-services/commit/d778c60)
- Relax queryId restrictions #521 [offline] 
[3e2f645](https://github.com/Azure/azure-mobile-services/commit/3e2f645)
- Work around for resource missing error on windows phone [offline]

### Managed SDK: Version 1.3 

- allow underscore and hyphen in queryId [7d192a3](https://github.com/Azure/azure-mobile-services/commit/7d192a3)
- added force option to purge data and pending operations on data [aa51d9f](https://github.com/Azure/azure-mobile-services/commit/aa51d9f)
- delete errors with operation on cancel and collapse [372ba61](https://github.com/Azure/azure-mobile-services/commit/372ba61)
- rename queryKey to queryId [93e59f7](https://github.com/Azure/azure-mobile-services/commit/93e59f7)
- insert should throw if the item already exists [#491](https://github.com/Azure/azure-mobile-services/issues/491) [fc13891](https://github.com/Azure/azure-mobile-services/commit/fc13891)
- **[Breaking]** Removed PullAsync overloads that do not take queryId [88cac8c](https://github.com/Azure/azure-mobile-services/commit/88cac8c)

### Managed SDK: Version 1.3 beta3
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

### Managed SDK: Version 1.3 beta2
- Updated Nuget references
- Request __deleted system property for sync
- Default delta token set to 1970-01-01 for compatibility with Table Storage 
- Expose protected methods from the MobileServiceSQLiteStore for intercepting sql
- **[Breaking]** Expose a ReadOnlyCollection instead of IEnumerable from MobileServiceTableOperationError

### Managed SDK: Version 1.3 beta
- Added support for incremental sync for .NET backend
- Added support for byte[] properties in offline
- Fixed issue with timezone roundtripping in incremental sync
- Improved exception handling for 409 conflicts
- Improved error handling for timeout errors during sync
- Follow link headers returned from .NET backend and use skip and top for PullAsync()
- Introduced the SupportedOptions enum on IMobileServiceSyncTable to configure the pull strategy
- **[Breaking]** Do not Push changes on PurgeAsync() instead throw an exception
- **[Breaking]** Renamed ToQueryString method to ToODataString on MobileServiceTableQueryDescription class

### Managed SDK: Version 1.3 alpha2
- Added support for incremental sync (currently, for Mobile Services JavaScript backend only)
- Added client support for soft delete
- Added support for offline pull with query string

### Managed SDK: Version 1.3 alpha2
- Added support for offline and sync
- Added support for soft delete

### Managed SDK: Version 1.2.6
- Fixed an issue on Xamarin.iOS and Xamarin.Android where UI popups occur during failed user authentication flows. These popups are now suppressed so that the developer can handle the error however they want.

### Managed SDK: Version 1.2.5
- Updated to use a modified build of Xamarin.Auth that will not conflict with any user-included version of Xamarin.Auth

### Managed SDK: Version 1.2.4
- Added support for following link headers returned from the .NET backend
- Added a MobileServiceConflictException to detect duplicate inserts
- Added support for datetimeoffsets in queries
- Added support for sending provider specific query string parameters in LoginAsync()
- Fixed an issue causing duplicate registrations in Xamarin.iOS against .NET backends

### Managed SDK: Version 1.2.3
- Added support for Xamarin iOS Azure Notification Hub integration

### Managed SDK: Version 1.2.2
- Support for optimistic concurrency on delete
- Update to Push surface area with minor object model changes. Added Registration base class in PCL and changed name within each extension to match the push notifcation surface. Example: WnsRegistration, WnsTemplateRegistration
- Added support for Xamarin Android Azure Notification Hub integration

### Managed SDK: Version 1.2.1
- Added support for Windows Phone 8.1, requires using Visual Studio 2013 Update 2 RC

### Managed SDK: Version 1.1.5
- Added support for Xamarin (iOS / Android)
- Clean-up id validation on insert operations

### Managed SDK: Version 1.1.4
- Added support for Windows Azure Notification Hub integration.

### Managed SDK: Version 1.1.3
- Added support for the Windows Azure Active Directory authentication in the `MobileServiceAuthenticationProvider` enumeration.
- Also added a mapping from that name to the value used in the service REST API (`/login/aad`)
- Fixed a issue [#213](https://github.com/WindowsAzure/azure-mobile-services/issues/213) in which SDK prevented calls to custom APIs with query string parameters starting with `$`

### Managed SDK: Version 1.1.2
- Fix [#192](https://github.com/WindowsAzure/azure-mobile-services/issues/192) - Serialized query is ambiguous if double literal has no fractional part
- Fixed Nuget support for Windows Phone 8

### Managed SDK: Version 1.1.1
- Fix bug when inserting a derived type
- Dropped support for Windows Phone 7.x clients (WP7.5 can still use the client version 1.1.0)

### Managed SDK: Version 1.1.0
- Support for tables with string ids
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Overload for log in which takes the provider as a string, in addition to the one with enums
- Fix [#121](https://github.com/WindowsAzure/azure-mobile-services/issues/121) - exceptions in `MobileServiceIncrementalLoadingCollection.LoadMoreItemsAsync` causes the app to crash

### Managed SDK: Version 1.0.3:
- Fixed query issues in Visual Basic expressions

