// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPushRequest.h"
#import "MSURLBuilder.h"

#pragma mark * MSPushRequest Implementation
@implementation MSPushRequest

#pragma mark * Private Static Constructors
- (MSPushRequest *)initWithURL:(NSURL *)url
                          data:(NSData *)data
                    HTTPMethod:(NSString *)method
{
    // Create the request
    MSPushRequest *request = [[MSPushRequest alloc] initWithURL:url];
    
    // Set the method and headers
    request.HTTPMethod = [method uppercaseString];

    request.HTTPBody = data;
    
    return request;
}

#pragma mark * Private Initializer Method

- (id)initWithURL:(NSURL *)url
{
    self = [super initWithURL:url];
    
    return self;
}

@end
