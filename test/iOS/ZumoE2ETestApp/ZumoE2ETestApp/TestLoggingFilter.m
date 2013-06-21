// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "TestLoggingFilter.h"

@implementation TestLoggingFilter

- (void)handleRequest:(NSURLRequest *)request next:(MSFilterNextBlock)onNext response:(MSFilterResponseBlock)onResponse {
    NSDictionary *headers = [request allHTTPHeaderFields];
    NSLog(@"All request headers: %@", headers);
    NSString *reqBody = [[NSString alloc] initWithData:[request HTTPBody] encoding:NSUTF8StringEncoding];
    NSLog(@"Request body: %@", reqBody);
    onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        NSDictionary *respHeaders = [response allHeaderFields];
        NSLog(@"All response headers: %@", respHeaders);
        NSString *respBody = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        NSLog(@"Response body: %@", respBody);
        onResponse(response, data, error);
    });
}

@end
