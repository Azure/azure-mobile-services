// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSLoginController.h"
#import "MSLoginView.h"
#import "MSLoginSerializer.h"
#import "MSJSONSerializer.h"
#import "MSClient.h"
#import "MSClientInternal.h"

#pragma mark * MSLoginController Private Interface


@interface MSLoginController ()

// Private properties
@property (nonatomic, strong, readonly)     id<MSSerializer> serializer;
@property (nonatomic, strong, readwrite)    MSLoginView *loginView;
@property (nonatomic, copy,   readonly)     MSClientLoginBlock completion;

@end


#pragma mark * MSLoginController Implementation


@implementation MSLoginController

@synthesize client = client_;
@synthesize provider = provider_;
@synthesize completion = completion_;
@synthesize loginView = loginView_;


#pragma mark * Public Static Constructor Methods


-(id) initWithClient:(MSClient *)client
            provider:(NSString *)provider
          completion:(MSClientLoginBlock)completion
{
    self = [super initWithNibName:nil bundle:nil];
    if (self) {
        client_ = client;
        provider_ = [provider copy];
        completion_ = [completion copy];
    }
    
    return self;
}


#pragma mark * Public Toolbar Property Accessor Methods


-(UIToolbar *) toolbar
{
    UIToolbar *localToolbar = nil;
    
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localToolbar = localLoginView.toolbar;
    }
    
    return localToolbar;
}


#pragma mark * Public ActivityIndicator Property Accessor Methods


-(UIActivityIndicatorView *) activityIndicator
{
    UIActivityIndicatorView *localActivityIndicator = nil;
    
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localActivityIndicator = localLoginView.activityIndicator;
    }
    
    return localActivityIndicator;
}


#pragma mark * Public ShowToolbar Property Accessor Methods


-(BOOL) showToolbar
{
    BOOL localShowToolbar = YES;
    
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localShowToolbar = localLoginView.showToolbar;
    }
    
    return localShowToolbar;
}

-(void) setShowToolbar:(BOOL)showToolbar
{
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localLoginView.showToolbar = showToolbar;
    }
}


#pragma mark * Public ToolbarPosition Property Accessor Methods


-(UIToolbarPosition) toolbarPosition
{
    UIToolbarPosition localToolbarPosition = UIToolbarPositionBottom;
    
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localToolbarPosition = localLoginView.toolbarPosition;
    }
    
    return localToolbarPosition;
}

-(void) setToolbarPosition:(UIToolbarPosition)toolbarPosition
{
    MSLoginView *localLoginView = self.loginView;
    if (localLoginView) {
        localLoginView.toolbarPosition = toolbarPosition;
    }
}


#pragma mark * Public UIViewController Override Methods


-(void) loadView
{
    self.view = self.loginView;
}

-(void) viewDidUnload
{
    self.loginView = nil;
}


#pragma mark * Private LoginView Property Accessor Methods


-(MSLoginView *) loginView
{
    MSLoginView *loginView = loginView_;
    
    // If there is not an MSLoginView, create one
    if (!loginView) {
        
        // Ensure we are using HTTPS
        CGRect frame = [[UIScreen mainScreen] bounds];
        NSURL *start = [self.client.loginURL URLByAppendingPathComponent:self.provider];
        NSURL *end = [self.client.loginURL URLByAppendingPathComponent: @"done"];
        
        MSLoginViewBlock viewCompletion = nil;
        
        if (self.completion) {
            
            viewCompletion =
            ^(NSURL *endURL, NSError *error) {
                
                MSUser *user = nil;
                if (error) {
                    error = [self convertError:error];
                }
                else {
                    user = [self userFromURL:endURL orError:&error];
                
                    if (user) {
                        self.client.currentUser = user;
                    }
                }
                
                self.completion(user, error);
            };
        }

        loginView = [[MSLoginView alloc] initWithFrame:frame
                                                client:self.client
                                              startURL:start
                                                endURL:end
                                            completion:viewCompletion];
        
        loginView_ = loginView;
    }
    
    return loginView;
}


#pragma mark * Private Serializer Property Accessor Methods


-(id<MSSerializer>) serializer
{
    // Just use a hard coded reference to MSJSONSerializer
    return [MSJSONSerializer JSONSerializer];
}


#pragma mark * Private Methods


-(NSError *) convertError:(NSError *)error
{
    NSError *newError = nil;
    
    if (error) {
        
        // Convert LoginView errors into general Login errors
        if ([error.domain isEqualToString:MSLoginViewErrorDomain]) {
            if (error.code == MSLoginViewCanceled) {
                newError = [self errorForLoginCanceled];
            }
            else {
                NSDictionary *userInfo = error.userInfo;
                NSData *data = [userInfo valueForKey:MSLoginViewErrorResponseData];
                NSURLResponse *response = [userInfo valueForKey:MSErrorResponseKey];
                newError = [self.serializer errorFromData:data
                                                 MIMEType:response.MIMEType];
            }
        }
        else {
            newError = error;
        }
    }
    
    return newError;
}

-(MSUser *) userFromURL:(NSURL *)URL orError:(NSError **)error
{
    MSUser *user = nil;
    NSError *localError = nil;
    
    // Ensure there is a URL
    if (URL) {
        
        // Parse the URL for the token string or an error string
        NSString *tokenString = nil;
        NSString *errorString = nil;
        
        NSString *URLString = URL.absoluteString;
        NSInteger tokenMatch = [URLString rangeOfString:@"#token="].location;
        if (tokenMatch != NSNotFound) {
            tokenString = [URLString substringFromIndex:(tokenMatch + 7)];
        }
        else {
            NSInteger errorMatch = [URLString rangeOfString:@"#error="].location;
            if (errorMatch != NSNotFound) {
                errorString = [URLString substringFromIndex:(errorMatch + 7)];
            }
        }
        
        // If there was a token string, deserialize it into a user or if
        // there was an error string, read the error message from it
        if (tokenString) {
            NSString *unencodedTokenString = [tokenString stringByRemovingPercentEncoding];
            
            NSData *tokenData = [unencodedTokenString
                                 dataUsingEncoding:NSUTF8StringEncoding];
            
            user = [[MSLoginSerializer loginSerializer] userFromData:tokenData
                                                             orError:&localError];
        }
        else if (errorString) {
            NSString *unencodedErrorString = [errorString stringByRemovingPercentEncoding];
            
            localError = [self errorWithDescription:unencodedErrorString
                                       andErrorCode:MSLoginFailed];
        }
    }
    
    // If we could not read a user from the URL, return either the known error
    // or a generic one
    if (!user && error) {
        if (!localError) {
            localError = [self errorForInvalidUserJson];
        }
        *error = localError;
    }
    
    return user;
}


#pragma mark * Private NSError Generation Methods


-(NSError *) errorForLoginCanceled
{
    return [self errorWithDescriptionKey:@"The login operation was canceled."
                            andErrorCode:MSLoginCanceled];
}

-(NSError *) errorForLoginFailed
{
    return [self errorWithDescriptionKey:@"The login operation failed."
                            andErrorCode:MSLoginFailed];
}

-(NSError *) errorForInvalidUserJson
{
    return [self errorWithDescriptionKey:@"The token in the login response was invalid. The token must be a JSON object with both a userId and an authenticationToken."
                            andErrorCode:MSLoginInvalidResponseSyntax];
}

-(NSError *) errorWithDescriptionKey:(NSString *)descriptionKey
                        andErrorCode:(NSInteger)errorCode
{
    NSString *description = NSLocalizedString(descriptionKey, nil);
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

-(NSError *) errorWithDescription:(NSString *)description
                     andErrorCode:(NSInteger)errorCode
{
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

@end