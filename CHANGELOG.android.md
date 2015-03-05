# Azure Mobile Services Android SDK Change Log

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
 