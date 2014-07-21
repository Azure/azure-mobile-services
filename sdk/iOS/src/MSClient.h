// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "MSError.h"
#import "MSFilter.h"
#import "MSLoginController.h"
#import "MSSyncContext.h"

@class MSTable;
@class MSUser;
@class MSSyncTable;
@class MSPush;


#pragma mark * Block Type Definitions
/// Callback for method with no return other than error.
typedef void (^MSCompletionBlock)(NSError *error);

/// Callback for invokeAPI method that expects a JSON result.
typedef void (^MSAPIBlock)(id result, NSHTTPURLResponse *response, NSError *error);

/// Callback for the invokeAPI method that can return any media type.
typedef void (^MSAPIDataBlock)(NSData *result,
                               NSHTTPURLResponse *response,
                               NSError *error);

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

/// The URL of the Microsoft Azure Mobile Service associated with the client.
@property (nonatomic, strong, readonly)     NSURL *applicationURL;

/// The application key for the Microsoft Azure Mobile Service associated with
/// the client if one was provided in the creation of the client and nil
/// otherwise. If non-nil, the application key will be included in all requests
/// made to the Microsoft Azure Mobile Service, allowing the client to perform
/// all actions on the Microsoft Azure Mobile Service that require application-key
/// level permissions.
@property (nonatomic, copy, readonly)     NSString *applicationKey;

/// A collection of MSFilter instances to apply to use with the requests and
/// responses sent and received by the client. The property is readonly and the
/// array is not-mutable. To apply a filter to a client, use the withFilter:
/// method.
@property (nonatomic, strong, readonly)         NSArray *filters;

/// A sync context that defines how offline data is synced and allows for manually
/// syncing data on demand
@property (nonatomic, strong)     MSSyncContext *syncContext;

/// @name Registering and unregistering for push notifications

/// The property to use for registering and unregistering for notifications via *MSPush*.
@property (nonatomic, strong, readonly)     MSPush *push;

/// @}

#pragma mark * Public ReadWrite Properties


/// The currently logged in user. While the currentUser property can be set
/// directly, the login* and logout methods are more convenient and
/// recommended for non-testing use.
@property (nonatomic, strong)               MSUser *currentUser;

/// @}

#pragma  mark * Public Static Constructor Methods

/// @name Initializing the MSClient Object
/// @{

/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(MSClient *)clientWithApplicationURLString:(NSString *)urlString;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(MSClient *)clientWithApplicationURLString:(NSString *)urlString
                         applicationKey:(NSString *)key;

/// Old method to create a client with the given URL and application key for the
/// Microsoft Azure Mobile Service. This has been deprecated. Use
/// clientWithApplicationURLString:applicationKey:
/// @deprecated
+(MSClient *)clientWithApplicationURLString:(NSString *)urlString
                         withApplicationKey:(NSString *)key __deprecated;

/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(MSClient *)clientWithApplicationURL:(NSURL *)url;

/// Creates a client with the given URL and application key for the Microsoft Azure
/// Mobile Service.
+(MSClient *)clientWithApplicationURL:(NSURL *)url
                       applicationKey:(NSString *)key;

#pragma  mark * Public Initializer Methods

/// Initializes a client with the given URL for the Microsoft Azure Mobile Service.
-(id)initWithApplicationURL:(NSURL *)url;

/// Initializes a client with the given URL and application key for the Windows
/// Azure Mobile Service.
-(id)initWithApplicationURL:(NSURL *)url applicationKey:(NSString *)key;

#pragma mark * Public Filter Methods

/// Creates a clone of the client with the given filter applied to the new client.
-(MSClient *)clientWithFilter:(id<MSFilter>)filter;

///@}

/// @name Authenticating Users
/// @{

#pragma mark * Public Login and Logout Methods

/// Logs in the current end user with the given provider by presenting the
/// MSLoginController with the given controller.
-(void)loginWithProvider:(NSString *)provider
              controller:(UIViewController *)controller
                animated:(BOOL)animated
              completion:(MSClientLoginBlock)completion;

/// Returns an MSLoginController that can be used to log in the current
/// end user with the given provider.
-(MSLoginController *)loginViewControllerWithProvider:(NSString *)provider
                                 completion:(MSClientLoginBlock)completion;

/// Logs in the current end user with the given provider and the given token for
/// the provider.
-(void)loginWithProvider:(NSString *)provider
                   token:(NSDictionary *)token
              completion:(MSClientLoginBlock)completion;

/// Logs out the current end user.
-(void)logout;

/// @}

#pragma mark * Public Table Methods

/// @name Querying Tables
/// @{

/// Returns an MSTable instance for a table with the given name.
-(MSTable *)tableWithName:(NSString *)tableName;

/// Old method to return an MSTable instance for a table with the given name.
/// This has been deprecated. Use tableWithName:
-(MSTable *)getTable:(NSString *)tableName __deprecated;

/// Returns an MSSyncTable instance for a table with the given name.
-(MSSyncTable *)syncTableWithName:(NSString *)tableName;

/// @}

#pragma mark * Public invokeAPI Methods

/// @name Invoking Custom APIs
/// @{

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

/// @}


#pragma mark * Public Connection Methods


/// @name Controlling connections to the server
/// @{

/// Determines where connections made to the mobile service are run. If set, connection related
/// logic will occur on this queue. Otherwise, the thread that made the call will be used.
@property (nonatomic, strong) NSOperationQueue *connectionDelegateQueue;

/// @}

@end
