// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSAPIRequest.h"
#import "MSURLBuilder.h"


#pragma mark * MSAPIRequest Implementation


@implementation MSAPIRequest


#pragma mark * Public Static Constructors


+(MSAPIRequest *) requestToinvokeAPI:(NSString *)APIName
                              client:(MSClient *)client
                                data:(NSData *)data
                          HTTPMethod:(NSString *)method
                          parameters:(NSDictionary *)parameters
                             headers:(NSDictionary *)headers
                          completion:(MSAPIDataBlock)completion
{
    MSAPIRequest *request = nil;
    NSError *error = nil;
    
    // Create the URL
    NSURL *url = [MSURLBuilder URLForApi:client
                                 APIName:APIName
                              parameters:parameters
                                 orError:&error];
    
    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        if (completion) {
            completion(nil, nil, error);
        }
    }
    else {
        // Create the request
        request = [[MSAPIRequest alloc] initWithURL:url];
        
        // Set the body
        request.HTTPBody = data;
        
        // Set the user-defined headers properties
        [request setAllHTTPHeaderFields:headers];
        
        // Set the method and headers
        request.HTTPMethod = [method uppercaseString];
        if (!request.HTTPMethod) {
            request.HTTPMethod = @"POST";
        }
    }
    
    return request;
}

+(MSAPIRequest *) requestToinvokeAPI:(NSString *)APIName
                              client:(MSClient *)client
                                body:(id)body
                          HTTPMethod:(NSString *)method
                          parameters:(NSDictionary *)parameters
                             headers:(NSDictionary *)headers
                          completion:(MSAPIBlock)completion
{
    MSAPIRequest *request = nil;
    NSError *error = nil;
    
    // Create the body or capture the error from serialization
    NSData *data = nil;
    if (body) {
        data = [client.serializer dataFromItem:body
                                     idAllowed:YES
                              ensureDictionary:NO
                        removeSystemProperties:NO
                                       orError:&error];
    }

    // If there was an error, call the completion and make sure
    // to return nil for the request
    if (error) {
        if (completion) {
            completion(nil, nil, error);
        }
    }
    else {
        request = [MSAPIRequest requestToinvokeAPI:APIName
                                            client:client
                                              data:data
                                        HTTPMethod:method
                                        parameters:parameters
                                           headers:headers
                                        completion:completion];
    }
    
    return request;
}


#pragma mark * Private Initializer Method


-(id) initWithURL:(NSURL *)url
{
    self = [super initWithURL:url];
    
    return self;
}

@end
