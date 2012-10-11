// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSClient.h"
#import "MSTable.h"
#import "MSClientConnection.h"
#import "MSUser.h"


#pragma mark * MSClient Private Interface


@interface MSClient ()

// Public readonly, private readwrite properties
@property (nonatomic, strong, readwrite)         NSArray *filters;

@end


#pragma mark * MSClient Implementation


@implementation MSClient

@synthesize applicationURL = applicationURL_;
@synthesize applicationKey = applicationKey_;
@synthesize currentUser = currentUser_;

MSClientConnection *connection;


#pragma mark * Public Static Constructor Methods


+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
{
    return [MSClient clientWithApplicationURLString:urlString
                                 withApplicationKey:nil];
}

+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
                           withApplicationKey:(NSString *)key
{
    // NSURL will be nil for non-percent escaped url strings so we have to
    // percent escape here
    NSString  *urlStringEncoded =
    [urlString stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSURL *url = [NSURL URLWithString:urlStringEncoded];
    return [MSClient clientWithApplicationURL:url withApplicationKey:key];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
{
    return [MSClient clientWithApplicationURL:url withApplicationKey:nil];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
                    withApplicationKey:(NSString *)key
{
    return [[MSClient alloc] initWithApplicationURL:url withApplicationKey:key];    
}


#pragma mark * Public Initializer Methods


-(id) initWithApplicationURL:(NSURL *)url
{
    return [self initWithApplicationURL:url withApplicationKey:nil];
}

-(id) initWithApplicationURL:(NSURL *)url withApplicationKey:(NSString *)key
{
    self = [super init];
    if(self)
    {
        applicationURL_ = url;
        applicationKey_ = [key copy];
    }
    return self;
}


#pragma mark * Public Filter Methods


-(MSClient *) clientwithFilter:(id<MSFilter>)filter
{
    // Create a deep copy of the client (except for the filters)
    MSClient *newClient = [self copy];
    
    // Either copy or create a new filters array
    NSMutableArray *filters = [self.filters mutableCopy];
    if (!filters) {
        filters = [NSMutableArray arrayWithCapacity:1];
    }
    
    // Add the filter to the filters array
    [filters addObject:filter];
    
    // Set the new filters on the copied client
    newClient.filters = filters;
    
    return newClient;
}


#pragma mark * Public Authentication Methods


-(UINavigationController *) loginViewControllerWithProvider:(NSString *)provider
                                completion:(MSClientLoginBlock)completion
{
    NSURL* startUrl = [self.applicationURL URLByAppendingPathComponent:
                       [NSString stringWithFormat:@"login/%@", provider]];
    NSURL* endUrl = [self.applicationURL URLByAppendingPathComponent:@"login/done"];
    
    __block MSEndUrlNavigatedTo endURLCompletion =
    
    ^(NSURL* url, NSError *error)
    {
        if (completion) {
            if (!error) {
                
                // The endUrl has been reached
                NSInteger match = [url.absoluteString rangeOfString:@"#token="].location;
                if (match > 0) {
                    
                    // Process returned token
                    NSString* jsonToken = [[url.absoluteString substringFromIndex:(match + 7)]
                                           stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
                    [self parseLoginResponse:[jsonToken dataUsingEncoding:NSUTF8StringEncoding]
                                  completion:completion];
                }
                else {
                    match = [url.absoluteString rangeOfString:@"#error="].location;
                    if (match > 0) {
                        
                        NSString * errorDescription = [[url.absoluteString
                                            substringFromIndex:(match + 7)]
                                            stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
                        NSDictionary *userInfo = @{
                            NSLocalizedDescriptionKey:errorDescription
                        };
                        
                        error = [NSError errorWithDomain:MSErrorDomain
                                                    code:MSLoginFailed
                                                userInfo:userInfo];
                    }
                    else {
                        
                        // Unrecognized response
                        error = [NSError errorWithDomain:MSErrorDomain
                                                    code:MSLoginInvalidResponseSyntax
                                                userInfo:@{@"error": url.absoluteString}];
                    }
                }
            }
        }
        
        if (error) {
            completion(nil, error);
        }
    };
        
    MSLoginViewController* lvc = [[MSLoginViewController alloc]
                                  initWithStartUrl:startUrl
                                  endUrl:endUrl
                                  completion:endURLCompletion];
    
    UINavigationController* nav = [[UINavigationController alloc]
                                   initWithRootViewController:lvc];
    
    return nav;
}

-(void) loginWithProvider:(NSString *)provider
                withToken:(NSDictionary *)token
                completion:(MSClientLoginBlock)completion;
{
    NSError *error = nil;
    
    if (connection) {
        if (completion) {
            NSDictionary *userInfo = @{
                NSLocalizedDescriptionKey:NSLocalizedString(@"Cannot start a login operation while another login operation is in progress.", nil)
            };
            
            error = [NSError errorWithDomain:MSErrorDomain
                                        code:MSLoginAlreadyInProgress
                                    userInfo:userInfo];
        }
    }
    else {
        NSURL *requestURL = [self.applicationURL URLByAppendingPathComponent:
                             [NSString stringWithFormat:@"login/%@", provider]];
        NSMutableURLRequest *request = [NSMutableURLRequest
                                        requestWithURL:requestURL];
        request.HTTPMethod = @"POST";
        request.HTTPBody = [NSJSONSerialization dataWithJSONObject:token options:(0) error:&error];
        if (!error) {
            
            __block MSResponseBlock responseCompletion = nil;
            if (completion) {
            
                responseCompletion =
                ^(NSHTTPURLResponse *response, NSData *data, NSError *responseError) {
                    connection = nil;
                    if (responseError) {
                        completion(nil, responseError);
                    }
                    [self parseLoginResponse:data completion:completion];
                };
            }
            
            connection = [[MSClientConnection alloc] initWithRequest:request
                                                          withClient:self
                                                          completion:responseCompletion];
            [connection start];
        }

    };
            
    if (error && completion) {
        completion(nil, error);
    }
}

-(void) logout
{
    self.currentUser = nil;
}


#pragma mark * Public Table Constructor Methods


-(MSTable *) getTable:(NSString *)tableName
{
    return [[MSTable alloc] initWithName:tableName andClient:self];
}


#pragma mark * NSCopying Methods


-(id) copyWithZone:(NSZone *)zone
{
    MSClient *client = [[MSClient allocWithZone:zone]
                            initWithApplicationURL:self.applicationURL
                                withApplicationKey:self.applicationKey];
                                                                            
    client.currentUser = [self.currentUser copyWithZone:zone];
    client.filters = [self.filters copyWithZone:zone];

    return client;
}


#pragma mark * Private Authentication Methods


-(void) parseLoginResponse:(NSData *)data
                 completion:(MSClientLoginBlock)completion
{
    if (completion) {
        NSError *error = nil;
        id json = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
        if (error || ![json isKindOfClass:[NSDictionary class]]) {
            
            // Token is not a JSON object
            NSDictionary *userInfo = @{
                @"token":[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding],
                NSLocalizedDescriptionKey:NSLocalizedString(@"The token in the login response must be a JSON object.", nil)
            };
            
            error = [NSError errorWithDomain:MSErrorDomain
                                            code:MSLoginInvalidResponseSyntax
                                        userInfo:userInfo];
        }
        else {
            id userId = [[json objectForKey:@"user"] objectForKey:@"userId"];
            id authenticationToken = [json objectForKey:@"authenticationToken"];
            if (![userId isKindOfClass:[NSString class]] ||
                ![authenticationToken isKindOfClass:[NSString class]]) {
                
                // userId or authenticationToken are not strings
                NSDictionary *userInfo = @{
                    @"token":[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding],
                    NSLocalizedDescriptionKey:NSLocalizedString(@"The token in the login response does not contain userId or authenticationToken.", nil)
                };
                
                error = [NSError errorWithDomain:MSErrorDomain
                                                code:MSLoginInvalidResponseSyntax
                                            userInfo:userInfo];
            }
            else {
                self.currentUser = [[MSUser alloc] initWithUserId:userId];
                self.currentUser.mobileServiceAuthenticationToken = authenticationToken;
                completion(self.currentUser, nil);
            }
        }
        
        if (error) {
            completion(nil, error);
        }
    }
}

@end
