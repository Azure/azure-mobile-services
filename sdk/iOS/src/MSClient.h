// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#if TARGET_OS_IPHONE
#import <UIKit/UIKit.h>
#endif
#import "MSError.h"
#import "BlockDefinitions.h"

@class MSTable;
@class MSUser;
@class MSSyncTable;
@class MSPush;
@class MSSyncContext;
@class MSLoginController;

@protocol MSFilter;

#pragma  mark * MSClient Public Interface

/// The MSClient class is the starting point for working with a Microsoft Azure
/// Mobile Service on a client device. An instance of the *MSClient* class is
/// created with a URL pointing to a Microsoft Azure Mobile Service. The *MSClient*
/// allows the developer to get MSTable instances, which are used to work with
/// the data of the Microsoft Azure Mobile Service, as well as login and logout an
/// end user.
@interface MSClient : NSObject <NSCopying>

#pragma mark * Public Readonly Properties

/// @name Properties
/// @{

/// The URL of the Microsoft Azure Mobile App
@property (nonatomic, strong, readonly, nonnull) NSURL *applicationURL;

/// The URL of the gateway associated with the mobile app
@property (nonatomic, strong, readonly, nullable) NSURL *gatewayURL;

/// The application key for the Microsoft Azure Mobile Service associated with
/// the client if one was provided in the creation of the client and nil
/// otherwise. If non-nil, the application key will be included in all requests
/// made to the Microsoft Azure Mobile Service, allowing the client to perform
/// all actions on the Microsoft Azure Mobile Service that require application-key
/// level permissions.
@property (nonatomic, copy, readonly, nullable) NSString *applicationKey;

/// A collection of MSFilter instances to apply to use with the requests and
/// responses sent and received by the client. The property is readonly and the
/// array is not-mutable. To apply a filter to a client, use the withFilter:
/// method.
@property (nonatomic, strong, readonly, nullable) NSArray *filters;

/// A sync context that defines how offline data is synced and allows for manually
/// syncing data on demand
@property (nonatomic, strong, nullable)     MSSyncContext *syncContext;

/// @name Registering and unregistering for push notifications

/// The property to use for registering and unregistering for notifications via *MSPush*.
@property (nonatomic, strong, readonly, nullable) MSPush *push;

/// @}

#pragma mark * Public ReadWrite Properties


/// The currently logged in user. While the currentUser property can be set
/// directly, the login* and logout methods are more convenient and
/// recommended for non-testing use.
@property (nonatomic, strong, nullable) MSUser *currentUser;

/// @}

#pragma  mark * Public Static Constructor Methods

/// @name Initializing the MSClient Object
/// @{

/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(nonnull MSClient *)clientWithApplicationURLString:(nonnull NSString *)urlString;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(nonnull MSClient *)clientWithApplicationURLString:(nonnull NSString *)urlString
                         applicationKey:(nullable NSString *)key;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(nonnull MSClient *)clientWithApplicationURLString:(nonnull NSString *)applicationUrlString
                           gatewayURLString:(nullable NSString *)gatewayUrlString
                             applicationKey:(nullable NSString *)key;


/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(nonnull MSClient *)clientWithApplicationURL:(nonnull NSURL *)url;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(nonnull MSClient *)clientWithApplicationURL:(nonnull NSURL *)url
                       applicationKey:(nullable NSString *)key;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(nonnull MSClient *)clientWithApplicationURL:(nonnull NSURL *)applicationUrl
                           gatewayURL:(nullable NSURL *)gatewayUrl
                       applicationKey:(nullable NSString *)key;


#pragma  mark * Public Initializer Methods

/// Initializes a client with the given URL for the Microsoft Azure Mobile Service.
-(nonnull instancetype)initWithApplicationURL:(nonnull NSURL *)url;

/// Initializes a client with the given URL and application key for the Windows
/// Azure Mobile Service.
-(nonnull instancetype)initWithApplicationURL:(nonnull NSURL *)url applicationKey:(nullable NSString *)key;

/// Initializes a client with the given URL and application key for the Windows
/// Azure Mobile Service.
-(nonnull instancetype)initWithApplicationURL:(nonnull NSURL *)applicationUrl
                 gatewayURL:(nullable NSURL *)gatewayUrl
             applicationKey:(nullable NSString *)key;


#pragma mark * Public Filter Methods

/// Creates a clone of the client with the given filter applied to the new client.
-(nonnull MSClient *)clientWithFilter:(nonnull id<MSFilter>)filter;

///@}

/// @name Authenticating Users
/// @{

#pragma mark * Public Login and Logout Methods

#if TARGET_OS_IPHONE
/// Logs in the current end user with the given provider by presenting the
/// MSLoginController with the given controller.
-(void)loginWithProvider:(nonnull NSString *)provider
              controller:(nonnull UIViewController *)controller
                animated:(BOOL)animated
              completion:(nonnull MSClientLoginBlock)completion;

/// Returns an MSLoginController that can be used to log in the current
/// end user with the given provider.
-(nonnull MSLoginController *)loginViewControllerWithProvider:(nonnull NSString *)provider
                                 completion:(nonnull MSClientLoginBlock)completion;
#endif

/// Logs in the current end user with the given provider and the given token for
/// the provider.
-(void)loginWithProvider:(nonnull NSString *)provider
                   token:(nonnull NSDictionary *)token
              completion:(nonnull MSClientLoginBlock)completion;

/// Logs out the current end user.
-(void)logout;

/// @}

#pragma mark * Public Table Methods

/// @name Querying Tables
/// @{

/// Returns an MSTable instance for a table with the given name.
-(nonnull MSTable *)tableWithName:(nonnull NSString *)tableName;

/// Old method to return an MSTable instance for a table with the given name.
/// This has been deprecated. Use tableWithName:
-(nonnull MSTable *)getTable:(nonnull NSString *)tableName __deprecated;

/// Returns an MSSyncTable instance for a table with the given name.
-(nonnull MSSyncTable *)syncTableWithName:(nonnull NSString *)tableName;

/// @}

#pragma mark * Public invokeAPI Methods

/// @name Invoking Custom APIs
/// @{

/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
/// response content will be treated as JSON.
-(void)invokeAPI:(nonnull NSString *)APIName
            body:(nullable id)body
      HTTPMethod:(nonnull NSString *)method
      parameters:(nullable NSDictionary *)parameters
         headers:(nullable NSDictionary *)headers
      completion:(nullable MSAPIBlock)completion;

/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
/// response content can be of any media type.
-(void)invokeAPI:(nonnull NSString *)APIName
            data:(nullable NSData *)data
      HTTPMethod:(nonnull NSString *)method
      parameters:(nullable NSDictionary *)parameters
         headers:(nullable NSDictionary *)headers
      completion:(nullable MSAPIDataBlock)completion;

/// @}


#pragma mark * Public Connection Methods


/// @name Controlling connections to the server
/// @{

/// Determines where connections made to the mobile service are run. If set, connection related
/// logic will occur on this queue. Otherwise, the thread that made the call will be used.
@property (nonatomic, strong, nullable) NSOperationQueue *connectionDelegateQueue;

/// @}

@end
