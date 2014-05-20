// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPushHttp.h"
#import "MSPushRequest.h"
#import "MSPushConnection.h"
#import "MSClient.h"

@interface MSPushHttp ()
@property (nonatomic) MSClient *client;
@end
    
@implementation MSPushHttp

-(MSPushHttp*)init:(MSClient*)client {
    self = [super init];
    
    if (self) {
        self.client = client;
    }
    
    return self;
}

-(void)createRegistrationId:(MSCreateRegistrationBlock)completion {
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:@"push/registrationids"];
    MSPushRequest *request = [[MSPushRequest alloc] initPushRequest:url
                                                               data:nil
                                                         HTTPMethod:@"POST"];

    __block MSPushConnection *connection = nil;
    
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
                if (!error)
                {
                    registrationId = response.allHeaderFields[@"Location"];
                    NSRange stringLocation = [registrationId rangeOfString:@"/" options:(NSBackwardsSearch)];
                    registrationId = [registrationId substringFromIndex:stringLocation.location + 1];
                }
            } else {
                [connection addRequestAndResponse:response toError:&error];
            }
            
            completion(registrationId, error);
            connection = nil;
        };
    }
    
    connection = [[MSPushConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

-(void)createRegistration:(NSMutableDictionary *)registration
               completion:(MSCompletionBlock)completion {
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:@"push/registrations"];
    url = [url URLByAppendingPathComponent:registration[@"registrationId"]];
    [registration removeObjectForKey:@"registrationId"];
    
    NSError *error = nil;
    NSData *data = [NSJSONSerialization dataWithJSONObject:registration options:0 error:&error];
    
    MSPushRequest *request = [[MSPushRequest alloc] initPushRequest:url
                                                               data:data
                                                         HTTPMethod:@"PUT"];
    
    __block MSPushConnection *connection = nil;

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
    
    connection = [[MSPushConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

-(void)listRegistrations:(NSString *)deviceToken
              completion:(MSListRegistrationsBlock)completion {
    NSString *query = @"?deviceId=";
    query = [query stringByAppendingString:deviceToken];
    query = [query stringByAppendingString:@"&platform=apns"];
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:@"push/registrations"];
    url = [url URLByAppendingPathComponent:query];
    
    MSPushRequest *request = [[MSPushRequest alloc] initPushRequest:url
                                                               data:nil
                                                         HTTPMethod:@"GET"];
    
    __block MSPushConnection *connection = nil;
    
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
    
    connection = [[MSPushConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

-(void)deleteRegistration:(NSString *)registrationId
               completion:(MSCompletionBlock)completion {
    NSURL *url = [self.client.applicationURL URLByAppendingPathComponent:@"push/registrations"];
    url = [url URLByAppendingPathComponent:registrationId];
    
    MSPushRequest *request = [[MSPushRequest alloc] initPushRequest:url
                                                               data:nil
                                                         HTTPMethod:@"DELETE"];
    
    __block MSPushConnection *connection = nil;
    
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
    
    connection = [[MSPushConnection alloc]
                  initWithRequest:request
                  client:self.client
                  completion:responseCompletion];
    
    [connection start];
}

@end
