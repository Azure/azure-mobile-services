// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSPushRequest : NSMutableURLRequest
- (MSPushRequest *)initPushRequest:(NSURL *)url
                              data:(NSData *)data
                        HTTPMethod:(NSString *)method;
@end
