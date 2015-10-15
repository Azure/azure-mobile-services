// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSClientConnection.h"
#import "MSUserAgentBuilder.h"
#import "MSFilter.h"
#import "MSUser.h"
#import "MSClientInternal.h"

#pragma mark * HTTP Header String Constants


static NSString *const xApplicationHeader = @"X-ZUMO-APPLICATION";
static NSString *const contentTypeHeader = @"Content-Type";
static NSString *const userAgentHeader = @"User-Agent";
static NSString *const zumoVersionHeader = @"X-ZUMO-VERSION";
static NSString *const jsonContentType = @"application/json";
static NSString *const xZumoAuth = @"X-ZUMO-AUTH";
static NSString *const xZumoInstallId = @"X-ZUMO-INSTALLATION-ID";


#pragma mark * MSConnectionDelegate Private Interface


// The |MSConnectionDelegate| is a private class that implements the
// |NSURLSessionDataDelegate| and surfaces success and error blocks. It
// is used only by the |MSClientConnection|.
@interface MSConnectionDelegate : NSObject <NSURLSessionDataDelegate>
		
@property (nonatomic, strong)               MSClient *client;
@property (nonatomic, strong)               NSData *data;
@property (nonatomic, copy)                 MSResponseBlock completion;

-(id) initWithClient:(MSClient *)client
          completion:(MSResponseBlock)completion;

@end


#pragma mark * MSClientConnection Implementation


@implementation MSClientConnection

static NSOperationQueue *delegateQueue;

@synthesize client = client_;
@synthesize request = request_;
@synthesize completion = completion_;

+(NSURLSession *)sessionWithDelegate:(id<NSURLSessionDelegate>)delegate delegateQueue:(NSOperationQueue *)queue
{
	NSURLSessionConfiguration *configuration = [NSURLSessionConfiguration defaultSessionConfiguration];
    
	NSURLSession *session = [NSURLSession sessionWithConfiguration:configuration
											 delegate:delegate
										delegateQueue:queue];
	return session;
}

# pragma mark * Public Initializer Methods


-(id) initWithRequest:(NSURLRequest *)request
               client:(MSClient *)client
           completion:(MSResponseBlock)completion
{
    self = [super init];
    if (self) {
        client_ = client;
        request_ = [MSClientConnection configureHeadersOnRequest:request
                                                      withClient:client];
        completion_ = [completion copy];
    }
    
    return self;
}


#pragma mark * Public Start Methods


-(void) start
{
    [MSClientConnection invokeNextFilter:self.client.filters
                              withClient:self.client
                             withRequest:self.request
                              completion:self.completion];
}

-(void) startWithoutFilters
{
    [MSClientConnection invokeNextFilter:nil
                              withClient:self.client
                             withRequest:self.request
                              completion:self.completion];
}


#pragma mark * Public Response Handling Methods


-(BOOL) isSuccessfulResponse:(NSHTTPURLResponse *)response
                        data:(NSData *)data
                     orError:(NSError **)error
{
    // Success is determined just by the HTTP status code
    BOOL isSuccessful = response.statusCode < 400;
    
    if (!isSuccessful && self.completion && error) {
        // Read the error message from the response body
        *error =[self.client.serializer errorFromData:data
                                             MIMEType:response.MIMEType];
        [self addRequestAndResponse:response toError:error];
    }
    
    return isSuccessful;
}

-(id) itemFromData:(NSData *)data
          response:(NSHTTPURLResponse *)response
          ensureDictionary:(BOOL)ensureDictionary
          orError:(NSError **)error
{
    // Try to deserialize the data
    id item = [self.client.serializer itemFromData:data
                                  withOriginalItem:nil
                                  ensureDictionary:ensureDictionary
                                           orError:error];
    
    // If there was an error, add the request and response
    if (error && *error) {
        [self addRequestAndResponse:response toError:error];
    }
    
    return item;
}


-(void) addRequestAndResponse:(NSHTTPURLResponse *)response
                      toError:(NSError **)error
{
    if (error && *error) {
        // Create a new error with request and the response in the userInfo...
        NSMutableDictionary *userInfo = [(*error).userInfo mutableCopy];
        [userInfo setObject:self.request forKey:MSErrorRequestKey];
        
        if (response) {
            [userInfo setObject:response forKey:MSErrorResponseKey];
        }
        
        *error = [NSError errorWithDomain:(*error).domain
                                     code:(*error).code
                                 userInfo:userInfo];
    }
}


# pragma mark * Private Static Methods


+(void) invokeNextFilter:(NSArray *)filters
              withClient:(MSClient *)client
             withRequest:(NSURLRequest *)request
               completion:(MSFilterResponseBlock)completion
{
    if (!filters || filters.count == 0) {
		// No filters to invoke so use |NSURLSessionDataTask | to actually
		// send the request.
		
		NSOperationQueue *taskQueue = nil;
        if (client.connectionDelegateQueue) {
			taskQueue = client.connectionDelegateQueue;
        } else {
			taskQueue = [NSOperationQueue mainQueue];
		}
		
		MSConnectionDelegate *delegate = [[MSConnectionDelegate alloc]
										  initWithClient:client
										  completion:completion];
		
		NSURLSession *session = [self sessionWithDelegate:delegate delegateQueue:taskQueue];
		NSURLSessionDataTask *task = [session dataTaskWithRequest:request];
		[task resume];
        
        [session finishTasksAndInvalidate];
    }
    else {
        
        // Since we have at least one more filter, construct the nextBlock
        // for it and then invoke the filter
        id<MSFilter> nextFilter = [filters objectAtIndex:0];
        NSArray *nextFilters = [filters subarrayWithRange:
                                NSMakeRange(1, filters.count - 1)];
    
        MSFilterNextBlock onNext =
        [^(NSURLRequest *onNextRequest,
           MSFilterResponseBlock onNextResponse)
        {
            [MSClientConnection invokeNextFilter:nextFilters
                                      withClient:client
                                     withRequest:onNextRequest
                                      completion:onNextResponse];                                    
        } copy];
        
        [nextFilter handleRequest:request
                           next:onNext
                       response:completion];
    }
}

+(NSURLRequest *) configureHeadersOnRequest:(NSURLRequest *)request
                                 withClient:(MSClient *)client
{
    NSMutableURLRequest *mutableRequest = [request mutableCopy];
    
    NSString *requestHost = request.URL.host;
    NSString *applicationHost = client.applicationURL.host;
    if ([applicationHost isEqualToString:requestHost])
    {
        // Add the authentication header if the user is logged in
        if (client.currentUser &&
            client.currentUser.mobileServiceAuthenticationToken) {
            [mutableRequest
             setValue:client.currentUser.mobileServiceAuthenticationToken
             forHTTPHeaderField:xZumoAuth];
        }
        
        // Set the User Agent header
        NSString *userAgentValue = [MSUserAgentBuilder userAgent];
        [mutableRequest setValue:userAgentValue
              forHTTPHeaderField:userAgentHeader];
        
        //Set the Zumo Version Header
        [mutableRequest setValue:userAgentValue
              forHTTPHeaderField:zumoVersionHeader];
        
        // Set the special Application key header
        NSString *appKey = client.applicationKey;
        if (appKey != nil) {
            [mutableRequest setValue:appKey
                  forHTTPHeaderField:xApplicationHeader];
        }
        
        // Set the installation id header
        [mutableRequest setValue:client.installId forHTTPHeaderField:xZumoInstallId];
        
        if ([request HTTPBody] &&
             ![request valueForHTTPHeaderField:contentTypeHeader]) {
            // Set the content type header
            [mutableRequest setValue:jsonContentType
                  forHTTPHeaderField:contentTypeHeader];
        }
    }
    
    return mutableRequest;
}




@end


#pragma mark * MSConnectionDelegate Private Implementation


@implementation MSConnectionDelegate

@synthesize client = client_;
@synthesize completion = completion_;
@synthesize data = data_;


# pragma mark * Public Initializer Methods


-(id) initWithClient:(MSClient *)client
          completion:(MSResponseBlock)completion
{
    self = [super init];
    if (self) {
        client_ = client;
        completion_ = [completion copy];
    }
    
    return self;
}

# pragma mark * NSURLSessionDataDelegate Methods

-(void)URLSession:(NSURLSession *)session dataTask:(NSURLSessionDataTask *)dataTask willCacheResponse:(NSCachedURLResponse *)proposedResponse completionHandler:(void (^)(NSCachedURLResponse *cachedResponse))completionHandler
{
	// We don't want to cache anything
	completionHandler(nil);
}

-(void)URLSession:(NSURLSession *)session dataTask:(NSURLSessionDataTask *)dataTask didReceiveData:(NSData *)data
{
	// If we haven't received any data before, just take this data instance
	if (!self.data) {
		self.data = data;
	}
	else {
		
		// Otherwise, append this data to what we have
		NSMutableData *newData = [NSMutableData dataWithData:self.data];
		[newData appendData:data];
		self.data = newData;
	}
}

-(void)URLSession:(NSURLSession *)session task:(NSURLSessionTask *)task willPerformHTTPRedirection:(NSHTTPURLResponse *)response newRequest:(NSURLRequest *)request completionHandler:(void (^)(NSURLRequest *))completionHandler
{
	NSURLRequest *newRequest = nil;
	
	// Only follow redirects to the Microsoft Azure Mobile Service and not
	// to other hosts
	NSString *requestHost = request.URL.host;
	NSString *applicationHost = self.client.applicationURL.host;
	if ([applicationHost isEqualToString:requestHost])
	{
		newRequest = request;
	}
	
	completionHandler(newRequest);
}

-(void)URLSession:(NSURLSession *)session task:(NSURLSessionTask *)task didCompleteWithError:(NSError *)error
{
	if (self.completion) {
		self.completion((NSHTTPURLResponse *)task.response, self.data, error);
		[self cleanup];
	}
}

-(void) cleanup
{
	self.client = nil;
	self.data = nil;
	self.completion = nil;
}

@end

