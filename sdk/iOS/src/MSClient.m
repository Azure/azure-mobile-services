// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSClientInternal.h"
#import "MSTable.h"
#import "MSClientConnection.h"
#import "MSLogin.h"
#import "MSUser.h"
#import "MSAPIRequest.h"
#import "MSAPIConnection.h"
#import "MSJSONSerializer.h"
#import "MSPush.h"

#pragma mark * MSClient Private Interface


@interface MSClient ()

// Public readonly, private readwrite properties
@property (nonatomic, strong, readwrite)         NSArray *filters;

// Private properties
@property (nonatomic, strong, readonly)         MSLogin *login;

@end


#pragma mark * MSClient Implementation


@implementation MSClient

@synthesize applicationURL = applicationURL_;
@synthesize applicationKey = applicationKey_;
@synthesize currentUser = currentUser_;
@synthesize login = login_;
@synthesize serializer = serializer_;

@synthesize installId = installId_;
-(NSString *) installId
{
    if(!installId_) {
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        installId_ = [defaults stringForKey:@"WindowsAzureMobileServicesInstallationId"];
        if(!installId_) {
            CFUUIDRef newUUID = CFUUIDCreate(kCFAllocatorDefault);
            installId_ = (__bridge_transfer NSString *)CFUUIDCreateString(kCFAllocatorDefault, newUUID);
            CFRelease(newUUID);
        
            //store the install ID so we don't generate a new one next time
            [defaults setObject:installId_ forKey:@"WindowsAzureMobileServicesInstallationId"];
            [defaults synchronize];
        }
    }
    
    return installId_;
}

#pragma mark * Public Static Constructor Methods


+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
{
    return [MSClient clientWithApplicationURLString:urlString
                                 applicationKey:nil];
}

+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
                           applicationKey:(NSString *)key
{
    // NSURL will be nil for non-percent escaped url strings so we have to
    // percent escape here
    NSString  *urlStringEncoded =
    [urlString stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSURL *url = [NSURL URLWithString:urlStringEncoded];
    return [MSClient clientWithApplicationURL:url applicationKey:key];
}

+(MSClient *) clientWithApplicationURLString:(NSString *)urlString
                          withApplicationKey:(NSString *)key
{
    return [MSClient clientWithApplicationURLString:urlString
                                     applicationKey:key];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
{
    return [MSClient clientWithApplicationURL:url applicationKey:nil];
}

+(MSClient *) clientWithApplicationURL:(NSURL *)url
                    applicationKey:(NSString *)key
{
    return [[MSClient alloc] initWithApplicationURL:url applicationKey:key];    
}


#pragma mark * Public Initializer Methods


-(id) initWithApplicationURL:(NSURL *)url
{
    return [self initWithApplicationURL:url applicationKey:nil];
}

-(id) initWithApplicationURL:(NSURL *)url applicationKey:(NSString *)key
{
    self = [super init];
    if(self)
    {
        applicationURL_ = url;
        applicationKey_ = [key copy];
        login_ = [[MSLogin alloc] initWithClient:self];
        _push = [[MSPush alloc] initWithClient:self];
    }
    return self;
}


#pragma mark * Public Filter Methods


-(MSClient *) clientWithFilter:(id<MSFilter>)filter
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
    
    newClient.connectionDelegateQueue = self.connectionDelegateQueue;
    
    return newClient;
}


#pragma mark * Public Authentication Methods


-(void) loginWithProvider:(NSString *)provider
             controller:(UIViewController *)controller
                 animated:(BOOL)animated
               completion:(MSClientLoginBlock)completion
{
    return [self.login loginWithProvider:provider
                            controller:controller
                                animated:animated
                              completion:completion];
}

-(MSLoginController *) loginViewControllerWithProvider:(NSString *)provider
                                      completion:(MSClientLoginBlock)completion
{
    return [self.login loginViewControllerWithProvider:provider
                                            completion:completion];
}

-(void) loginWithProvider:(NSString *)provider
                token:(NSDictionary *)token
               completion:(MSClientLoginBlock)completion;
{
   return [self.login loginWithProvider:provider
                              token:token
                             completion:completion];
}

-(void) logout
{
    self.currentUser = nil;
}


#pragma mark * Public Table Constructor Methods


-(MSTable *) tableWithName:(NSString *)tableName
{
    return [[MSTable alloc] initWithName:tableName client:self];
}

-(MSTable *) getTable:(NSString *)tableName
{
    return [self tableWithName:tableName];
}


#pragma mark * Public invokeAPI Methods


-(void)invokeAPI:(NSString *)APIName
            body:(id)body
      HTTPMethod:(NSString *)method
      parameters:(NSDictionary *)parameters
         headers:(NSDictionary *)headers
      completion:(MSAPIBlock)completion
{
    // Create the request
    MSAPIRequest *request = [MSAPIRequest requestToinvokeAPI:APIName
                                                      client:self
                                                        body:body
                                                  HTTPMethod:method
                                                  parameters:parameters
                                                     headers:headers
                                                  completion:completion];
    // Send the request
    if (request) {
        MSAPIConnection *connection =
            [MSAPIConnection connectionWithApiRequest:request
                                               client:self
                                               completion:completion];
        [connection start];
    }
}

-(void)invokeAPI:(NSString *)APIName
            data:(NSData *)data
      HTTPMethod:(NSString *)method
      parameters:(NSDictionary *)parameters
         headers:(NSDictionary *)headers
      completion:(MSAPIDataBlock)completion
{
    // Create the request
    MSAPIRequest *request = [MSAPIRequest requestToinvokeAPI:APIName
                                                      client:self
                                                        data:data
                                                  HTTPMethod:method
                                                  parameters:parameters
                                                     headers:headers
                                                  completion:completion];
    // Send the request
    if (request) {
        MSAPIConnection *connection =
        [MSAPIConnection connectionWithApiDataRequest:request
                                               client:self
                                       completion:completion];
        [connection start];
    }
}


#pragma mark * NSCopying Methods


-(id) copyWithZone:(NSZone *)zone
{
    MSClient *client = [[MSClient allocWithZone:zone]
                            initWithApplicationURL:self.applicationURL
                                applicationKey:self.applicationKey];
                                                                            
    client.currentUser = [self.currentUser copyWithZone:zone];
    client.filters = [self.filters copyWithZone:zone];

    return client;
}


#pragma mark * Private Methods


-(id<MSSerializer>) serializer
{
    // Just use a hard coded reference to MSJSONSerializer
    return [MSJSONSerializer JSONSerializer];
}

@end
