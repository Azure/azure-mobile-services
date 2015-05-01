# Azure Mobile Services iOS SDK Change Log

## SDK Downloads
- [iOS 2.0 SDK](http://aka.ms/gc6fex)
- [iOS 1.2.4 SDK](http://aka.ms/kymw2g)

### Version 2.1.0
- Fix cancelAndUpdate and cancelAndDiscard actions on the MSTableOperationError class
- Fix issues with sync operations not firing their completion blocks on the correct queue

### Version 2.0.0
- GA of offline sync changes from previous betas

### Version 2.0.0 beta2
- **[Breaking]** Changed MSReadQueryBlock to return MSQueryResult instead of items and totalCount. See [this blog post](http://azure.microsoft.com/blog/2014/10/07/mobile-services-beta-ios-sdk-released/) for more information.
- Added support for incremental sync
- Added support for query parameters in pull operations
- Added support for following link headers returned from the .NET backend
- Fixed issue with login controller completing before animation completes
- Added a method for force purge of local data
- Added a helper method to return an NSDictionary from an NSManagedObject
- Fixed issue with the __includeDeleted flag sending the wrong value

### Version 2.0.0 beta1

- Added support for incremental sync
- Added support for query parameters in pull operations
- Fixed issue with login controller completing before animation completes
- Added a method for force purge of local data
- Added a helper method to return an NSDictionary from an NSManagedObject
- Fixed issue with the __includeDeleted flag sending the wrong value

### Version 1.3 alpha1
- Added support for offline and sync

### Version 1.2.4
- Address bug where version property was returned to the caller even when not asked for
- Fixes Swift QS for syntax changes up to Xcode Beta 7

### Version 1.2.3
- Fix issue with const when using both Azure Messaging and Mobile Services frameworks
- Fix issue [#306](https://github.com/Azure/azure-mobile-services/issues/306) with how arrays passed as query string params to table and custom APIs are converted 
- Fix issue where system properties (__version, __updatedAt, etc) were returned to the caller when they were not requested

### Version 1.2.2
- Added support for APNS Azure Notification Hub integration
- Support for optimistic concurrency on delete

### iOS SDK
- - Fix issue [#218](https://github.com/WindowsAzure/azure-mobile-services/issues/218) in which some dates coming from the mobile services with the .NET runtime weren't parsed correctly

### Version 1.1.3
- Added a mapping in the authentication provider from WindowsAzureActiveDirectory to the value used in the REST API (`/login/aad`)

### Version 1.1.2
- Supports the arm64 architecture
- Now requires iOS 6 or newer to use 

### Version 1.1.1
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Fix bug with using arrays in invokeAPI

### Version 1.1.0
- Support for tables with string ids

