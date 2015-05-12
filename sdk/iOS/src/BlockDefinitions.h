// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#ifndef WindowsAzureMobileServices_BlockDefinitions_h
#define WindowsAzureMobileServices_BlockDefinitions_h

#import <Foundation/Foundation.h>

@class MSUser;
@class MSQueryResult;

#pragma	mark * MSSyncContext

/// Callback for updates and deletes. If there was an error, the *error* will be non-nil.
typedef void (^MSSyncBlock)(NSError *error);

/// Callback for inserts. If there was an error, the *error* will be non-nil.
typedef void (^MSSyncItemBlock)(NSDictionary *item, NSError *error);

/// Callback for push operations
typedef void (^MSSyncPushCompletionBlock)(void);

#pragma	mark * MSLoginController
/// Callback logging in an end user. If there was an error or the login was
/// cancelled, *error* will be non-nil.
typedef void (^MSClientLoginBlock)(MSUser *user, NSError *error);

#pragma mark * MSClient
/// Callback for method with no return other than error.
typedef void (^MSCompletionBlock)(NSError *error);

/// Callback for invokeAPI method that expects a JSON result.
typedef void (^MSAPIBlock)(id result, NSHTTPURLResponse *response, NSError *error);

/// Callback for the invokeAPI method that can return any media type.
typedef void (^MSAPIDataBlock)(NSData *result,
							   NSHTTPURLResponse *response,
							   NSError *error);

#pragma mark * MSTable

/// Callback for updates, inserts or readWithId requests. If there was an
/// error, the *error* will be non-nil.
typedef void (^MSItemBlock)(NSDictionary *item, NSError *error);

/// Callback for deletes. If there was an error, the *error* will be non-nil.
typedef void (^MSDeleteBlock)(id itemId, NSError *error);

/// Callback for reads. If there was an error, the *error* will be non-nil. If
/// there was not an error, then the result will always be non-nil
/// but but items may be empty if the query returned no results. If the query included a
/// request for the total count of items on the server (not just those returned
/// in *items* array), the *totalCount* in the result will have this value; otherwise
/// *totalCount* will be -1.
/// if the server returned a link to next page of results then
/// nextLink will be non-nil.
typedef void (^MSReadQueryBlock)(MSQueryResult *result,
								 NSError *error);

#pragma mark * MSClientConnection


// Callback for connections. If there was an error, the |error| will be non-nil.
// If there was not an error, the |response| will be non-nil, but
// the |data| may or may not be nil depending on if the response had content.
typedef void (^MSResponseBlock)(NSHTTPURLResponse *response,
								NSData *data,
								NSError *error);

#endif
