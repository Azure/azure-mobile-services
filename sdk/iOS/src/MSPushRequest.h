// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSPushRequest : NSMutableURLRequest

/// Initialize an MSPushRequest with url, data and method.
- (MSPushRequest *)initWithURL:(NSURL *)url
                          data:(NSData *)data
                    HTTPMethod:(NSString *)method;

@end
