// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSLogin.h"
#import "MSLoginSerializer.h"
#import "MSJSONSerializer.h"
#import "MSClientConnection.h"
#import "MSClient.h"
#import "WindowsAzureMobileServices.h"

#pragma mark * MSLogin Private Interface


@interface MSLogin ()

// Private properties
@property (nonatomic, strong, readonly)     id<MSSerializer> serializer;

@end


#pragma mark * MSLogin Implementation


@implementation MSLogin

@synthesize client = client_;


#pragma  mark * Public Initializer Methods


-(id) initWithClient:(MSClient *)client
{
    self = [super init];
    if (self) {
        client_ = client;
    }
    
    return self;
}


#pragma  mark * Public Login Methods

#if TARGET_OS_IPHONE
-(void) loginWithProvider:(NSString *)provider
             controller:(UIViewController *)controller
                 animated:(BOOL)animated
               completion:(MSClientLoginBlock)completion
{
    __block MSLoginController *loginController = nil;
    __block MSUser *localUser = nil;
    __block NSError *localError = nil;
    __block int allDoneCount = 0;
  
    // Verify we have a gateway URL
    
    if (![self verifyGatewayURL:self.client.gatewayURL error:&localError]) {
        if (completion) {
            completion(nil, localError);
        }
        return;
    }

    
    void (^callCompletionIfAllDone)() = ^{
        allDoneCount++;
        if (allDoneCount == 2) {
            // Its possible for this to be triggered to close in the completion block of the present
            // controller call, for example when there is no network connection.
            // In order to avoid an error with the presentation animation still finishing, put the
            // dismiss call onto the main queue and let this block finish running.
            dispatch_async(dispatch_get_main_queue(), ^{
                [controller dismissViewControllerAnimated:animated completion:^{
                    if (completion) {
                        completion(localUser, localError);
                    }
                    localUser = nil;
                    localError = nil;
                    loginController = nil;
                }];
            });
        }
    };
    
    // Create a completion block that will dismiss the controller, and then
    // in the controller dismissal completion, call the completion that was
    // passed in by the caller.  This ensures that if the dismissal is animated
    // the LoginViewController has fuly disappeared from view before the
    // final completion is called.
    MSClientLoginBlock loginCompletion = ^(MSUser *user, NSError *error){
        localUser = user;
        localError = error;
        callCompletionIfAllDone();
    };
    
    provider = [self normalizeProvider:provider];
    loginController = [self loginViewControllerWithProvider:provider
                                                 completion:loginCompletion];
    
    // On iPhone this will do nothing, but on iPad it will present a smaller
    // view that looks much better for login
    loginController.modalPresentationStyle = UIModalPresentationFormSheet;
    
    dispatch_async(dispatch_get_main_queue(),^{
        [controller presentViewController:loginController
                                 animated:animated
                               completion:callCompletionIfAllDone];
    });
}

-(MSLoginController *) loginViewControllerWithProvider:(NSString *)provider
                                            completion:(MSClientLoginBlock)completion
{
    NSAssert(self.client.gatewayURL != nil, @"The gateway URL is required for login");
    
    provider = [self normalizeProvider:provider];
    return [[MSLoginController alloc] initWithClient:self.client
                                            provider:provider
                                          completion:completion];
}

#endif

-(void) loginWithProvider:(NSString *)provider
                token:(NSDictionary *)token
               completion:(MSClientLoginBlock)completion
{
    // Create the request
    NSError *error = nil;
    provider = [self normalizeProvider:provider];
    NSURLRequest *request = [self requestForProvider:provider
                                            andToken:token
                                             orError:&error];
    
    // If creating the request failed, call the completion block,
    // otherwise, send the login request
    if (error) {
        if (completion) {
            completion(nil, error);
        }
    }
    else {
        
        // Create the response completion block
        MSResponseBlock responseCompletion = nil;
        if (completion) {
            
            responseCompletion = 
            ^(NSHTTPURLResponse *response, NSData *data, NSError *responseError)
            {
                MSUser *user = nil;
                
                if (!responseError) {
                    if (response.statusCode >= 400) {
                        responseError = [self.serializer errorFromData:data
                                                              MIMEType:response.MIMEType];
                    }
                    else {
                        user = [[MSLoginSerializer loginSerializer]
                                userFromData:data
                                orError:&responseError];
                        
                        if (user) {
                            self.client.currentUser = user;
                        }
                    }
                }
                
                completion(user, responseError);
            };
        }
        
        // Create the connection and start it
        MSClientConnection *connection = [[MSClientConnection alloc]
                                                initWithRequest:request
                                                client:self.client
                                                completion:responseCompletion];
        [connection startWithoutFilters];
    }
}


#pragma mark * Private Serializer Property Accessor Methods
    
    
-(id<MSSerializer>) serializer
{
    // Just use a hard coded reference to MSJSONSerializer
    return [MSJSONSerializer JSONSerializer];
}


#pragma mark * Private Methods

- (BOOL) verifyGatewayURL:(NSURL *)url error:(NSError **)error
{
    if (!url) {
        *error = [NSError errorWithDomain:MSErrorDomain
                                     code:MSLoginInvalidURL
                                 userInfo:@{ NSLocalizedDescriptionKey :@"The gateway URL is required for login" }];
        return NO;
    }
    
    return YES;
}

-(NSString *) normalizeProvider:(NSString *)provider {
    // Microsoft Azure Active Directory can be specified either in
    // full or with the 'aad' abbreviation. The service REST API
    // expects 'aad' only.
    if ([[provider lowercaseString] isEqualToString:@"windowsazureactivedirectory"]) {
        return @"aad";
    } else {
        return provider;
    }
}

-(NSURLRequest *) requestForProvider:(NSString *)provider
                            andToken:(NSDictionary *)token
                             orError:(NSError **)error
{
    NSMutableURLRequest *request = nil;
    
    if (![self verifyGatewayURL:self.client.gatewayURL error:error]) {
        return nil;
    }

    NSData *requestBody = [[MSLoginSerializer loginSerializer] dataFromToken:token
                                                                     orError:error];
    if (requestBody) {
    
        NSURL *URL = [self.client.gatewayURL URLByAppendingPathComponent:
                             [NSString stringWithFormat:@"login/%@", provider]];
        request = [NSMutableURLRequest requestWithURL:URL];
        request.HTTPMethod = @"POST";
        request.HTTPBody = requestBody;
    }
    
    return request;
}

@end
