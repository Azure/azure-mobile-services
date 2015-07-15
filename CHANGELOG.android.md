# Azure Mobile Services Android SDK Change Log

### Android SDK: Version 2.0.3
- Fix in the incremental sync query building logic [#720](https://github.com/Azure/azure-mobile-services/issues/720) to use correct parentheses and skip logic [c58d4e8](https://github.com/Azure/azure-mobile-services/commit/c58d4e8) 
- Fix for the deserialization bug [#718](https://github.com/Azure/azure-mobile-services/issues/718) on OperationErrorList table with datatype mismatch [15e9f9b](https://github.com/Azure/azure-mobile-services/commit/15e9f9b)

### Android SDK: Version 2.0.2
- Support for operation state tracking
- Fix for type incompatibility from datetime to datetimeoffset 
- Methods added for CancelAndDiscard and CancelAndUpdate for customized conflict handling 
- Fix for the local store database connection issue caused due to race condition in asynchronous operations
- Updated end to end test
- Support for binary data type on queries (for instance to query using the __version column)
- Improvements on the DateSerializer

### Android SDK: Version 2.0.2-Beta2
- Fix for the pull action with SQLite

### Android SDK: Version 2.0.2-Beta
- Mobile Services SDK version indicator with user-agent
- Introduced conflict exception to facilitate handling of 409 conflict
- Fix for the cropped UI for Facebook authentication
- Fix for SQL ordering issue
- Support for Incremental Sync to pull data with flexibility
- Support for Soft Delete
- Improved SQLite insert performance on pull
- Purge no longer pushes, instead it throws an exception as user expects
- Local item deletion exception is handled properly according to server state
- Support for continuation tokens in queries for paging
- InsertAsync throws expected exception on duplicate insert
- Fix for Local store & SQLite missing/mismatch table columns (i.e. “_deleted”)
- Support for Android Studio
- Send custom query string parameters with loginAsync Android

### Android SDK: Version 1.1.5
- Added support for Windows Azure Notification Hub integration

### Android SDK: Version 1.1.3
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Added support for the Windows Azure Active Directory authentication in the `MobileServiceAuthenticationProvider` enum.
- Also added a mapping from that name to the value used in the service REST API (`/login/aad`)

### Android SDK: Version 1.1.0
- Support for tables with string ids
- Overload for log in which takes the provider as a string, in addition to the one with enums
 