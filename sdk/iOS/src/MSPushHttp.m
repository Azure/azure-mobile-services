// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPushRequest.h"
#import "MSPushHttp.h"
#import "MSClient.h"

@interface MSPushHttp ()
@property (nonatomic, weak, readonly) MSClient *client;
@end
    
@implementation MSPushHttp

- (MSPushHttp *)initWithClient:(MSClient *)client
{
    self = [super init];
    
    if (self) {
        _client = client;
    }
    
    return self;
}

- (void)createRegistrationId:(MSCreateRegistrationIdBlock)completion
{
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:@"push/registrationids"];
    MSPushRequest *request = [[MSPushRequest alloc] initWithURL:url
                                                               data:nil
                                                         HTTPMethod:@"POST"];

    __block MSClientConnection *connection = nil;
    
    MSResponseBlock responseCompletion = nil;
    if (completion) {
        
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            NSString *registrationId = nil;
            
            if (!error) {
                [connection isSuccessfulResponse:response
                                            data:data
                                         orError:&error];
                if (!error) {
                    registrationId = response.allHeaderFields[@"Location"];
                    NSRange stringLocation = [registrationId rangeOfString:@"/" options:(NSBackwardsSearch)];
                    registrationId = [registrationId substringFromIndex:stringLocation.location + 1];
                }
            } else {
                [connection addRequestAndResponse:response toError:&error];
            }
            
            connection = nil;
            
            if (completion) {
                completion(registrationId, error);
            }
        };
    }
    
    connection = [[MSClientConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

- (void)upsertRegistration:(NSMutableDictionary *)registration
                completion:(MSCompletionBlock)completion
{
    NSString *registrationId = registration[@"registrationId"];
    NSString *urlPath =[NSString stringWithFormat:@"push/registrations/%@", registrationId];
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:urlPath];
    [registration removeObjectForKey:@"registrationId"];
    
    NSString *name = registration[@"name"];
    [registration removeObjectForKey:@"name"];
    
    NSError *error = nil;
    NSData *data = [NSJSONSerialization dataWithJSONObject:registration options:0 error:&error];
    
    MSPushRequest *request = [[MSPushRequest alloc] initWithURL:url
                                                               data:data
                                                         HTTPMethod:@"PUT"];
    
    __block MSClientConnection *connection = nil;

    MSResponseBlock responseCompletion = nil;
    if (completion) {
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            [registration setValue:name forKey:@"name"];
            [registration setValue:registrationId forKey:@"registrationId"];
            
            if (!error) {
                [connection isSuccessfulResponse:response
                                            data:data
                                         orError:&error];
            } else {
                [connection addRequestAndResponse:response toError:&error];
            }
            
            connection = nil;
            
            if (completion) {
                completion(error);
            }
        };
    }
    
    connection = [[MSClientConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

- (void)registrationsForDeviceToken:(NSString *)deviceToken
               completion:(MSListRegistrationsBlock)completion
{
    NSString *queryAndUrlPath = [NSString stringWithFormat:@"push/registrations?deviceId=%@&platform=apns", deviceToken];
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:queryAndUrlPath];
    
    MSPushRequest *request = [[MSPushRequest alloc] initWithURL:url
                                                               data:nil
                                                         HTTPMethod:@"GET"];
    
    __block MSClientConnection *connection = nil;
    
    MSResponseBlock responseCompletion = nil;
    if (completion) {
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            NSArray *registrations = nil;
            
            if (!error) {
                [connection isSuccessfulResponse:response
                                            data:data
                                         orError:&error];
                
                if (!error) {
                    registrations = [self.client.serializer arrayFromData:data orError:&error];
                }
            } else {
                [connection addRequestAndResponse:response toError:&error];
            }
            
            completion(registrations, error);
            connection = nil;
        };
    }
    
    connection = [[MSClientConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

- (void)deleteRegistrationById:(NSString *)registrationId
                completion:(MSCompletionBlock)completion
{
    NSString *urlPath = [NSString stringWithFormat:@"push/registrations/%@", registrationId];
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:urlPath];
    
    MSPushRequest *request = [[MSPushRequest alloc] initWithURL:url
                                                               data:nil
                                                         HTTPMethod:@"DELETE"];
    
    __block MSClientConnection *connection = nil;
    
    MSResponseBlock responseCompletion = nil;
    if (completion) {
        responseCompletion =
        ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
        {
            if (!error) {
                [connection isSuccessfulResponse:response
                                            data:data
                                         orError:&error];
            } else {
                [connection addRequestAndResponse:response toError:&error];
            }
            
            completion(error);
            connection = nil;
        };
    }
    
    connection = [[MSClientConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

@end