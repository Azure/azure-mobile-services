// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSError.h"


#pragma mark * Block Type Definitions


/// Callback that the filter should invoke once an HTTP response (with or
/// without data) or an error has been received by the filter.
typedef void (^MSFilterResponseBlock)(NSHTTPURLResponse *response, NSData *data, NSError *error);

/// Callback that the filter should invoke to allow the next filter to handle
/// the given request.
typedef void (^MSFilterNextBlock)(NSURLRequest *request,
                                  MSFilterResponseBlock onResponse);


#pragma  mark * MSFilter Public Protocol


/// The *MSFilter* protocol allows developers to implement a class that can
/// inspect and/or replace HTTP request and HTTP response messages being sent
/// and received by an *MSClient* instance.
@protocol MSFilter <NSObject>

/// @name Modify the request
/// @{

/// Allows for the inspection and/or modification of the HTTP request and HTTP response messages
/// being sent and received by an *MSClient* instance.
-(void)handleRequest:(NSURLRequest *)request
                next:(MSFilterNextBlock)next
            response:(MSFilterResponseBlock)response;

///@}
@end
