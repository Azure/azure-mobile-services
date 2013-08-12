// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoLogUpdater.h"
#import "ZumoTestGlobals.h"

@interface ZumoLogUpdater () <NSURLConnectionDelegate>
{
    NSMutableData *receivedData;
    NSURLConnection *connection;
}

@end

@implementation ZumoLogUpdater

-(void)uploadLogs:(NSString *)logText toUrl:(NSString *)url allTests:(BOOL)allTests {
    NSString *urlWithPlatform = [url stringByAppendingString:@"?platform=iOS"];
    if (allTests) {
        urlWithPlatform = [urlWithPlatform stringByAppendingString:@"&allTests=true"];
    }
    
    NSString *runtimeVersion = [[[ZumoTestGlobals sharedInstance] globalTestParameters] objectForKey:RUNTIME_VERSION_KEY];
    NSString *clientVersion = [[[ZumoTestGlobals sharedInstance] globalTestParameters] objectForKey:CLIENT_VERSION_KEY];
    if (runtimeVersion) {
        urlWithPlatform = [urlWithPlatform stringByAppendingString:@"&runtimeVersion="];
        urlWithPlatform = [urlWithPlatform stringByAppendingString:runtimeVersion];
    }
    if (clientVersion) {
        urlWithPlatform = [urlWithPlatform stringByAppendingString:@"&clientVersion="];
        urlWithPlatform = [urlWithPlatform stringByAppendingString:clientVersion];
    }

    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:[NSURL URLWithString:urlWithPlatform]];
    [request setHTTPMethod:@"POST"];
    [request setHTTPBody:[logText dataUsingEncoding:NSUTF8StringEncoding]];
    [request setValue:@"text/plain" forHTTPHeaderField:@"Content-Type"];
    
    connection = [[NSURLConnection alloc] initWithRequest:request delegate:self startImmediately:YES];
}

- (void)connection:(NSURLConnection *)connection didFailWithError:(NSError *)error {
    UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error uploading logs" message:[error localizedDescription] delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
    [av show];
    receivedData = nil;
}

- (void)connection:(NSURLConnection *)connection didReceiveResponse:(NSURLResponse *)response {
    NSHTTPURLResponse *httpResponse = (NSHTTPURLResponse *)response;
    if ([httpResponse statusCode] == 200 || [httpResponse statusCode] == 201) {
        receivedData = [[NSMutableData alloc] init];
    } else {
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:[NSString stringWithFormat:@"Response from upload service: %d", [httpResponse statusCode]] delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
    }
}

- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data {
    [receivedData appendData:data];
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection {
    if (receivedData) {
        NSString *responseBody = [[NSString alloc] initWithData:receivedData encoding:NSUTF8StringEncoding];
        receivedData = nil;
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Logs uploaded" message:responseBody delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
    }
}

@end
