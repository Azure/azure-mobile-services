//
//  ZumoTestGlobals.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoTestGlobals.h"

@interface ZumoAPMRequest : NSObject <NSURLConnectionDelegate>
{
    NSURLConnection *connection;
    NSMutableData *responseData;
    NSHTTPURLResponse *response;
    ZumoHttpRequestCompletion responseCallback;
}

- (id)initWithMethod:(NSString *)method url:(NSString *)url headers:(NSDictionary *)headers body:(NSString *)body withResponse:(ZumoHttpRequestCompletion)responseblock;
- (void)sendRequest;

@end

@implementation ZumoAPMRequest

- (id)initWithMethod:(NSString *)method url:(NSString *)url headers:(NSDictionary *)headers body:(NSString *)body withResponse:(ZumoHttpRequestCompletion)responseBlock {
    self = [super init];
    if (self) {
        responseData = [[NSMutableData alloc] init];
        NSURL *theUrl = [NSURL URLWithString:url];
        NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:theUrl];
        [request setHTTPMethod:method];
        if (headers) {
            [request setValuesForKeysWithDictionary:headers];
        }
        
        NSData *requestBody = [body dataUsingEncoding:NSUTF8StringEncoding];
        [request setHTTPBody:requestBody];
        
        connection = [[NSURLConnection alloc] initWithRequest:request delegate:self startImmediately:NO];
        responseCallback = responseBlock;
        response = nil;
    }
    
    return self;
}

- (void)sendRequest {
    [connection start];
}

- (void)connection:(NSURLConnection *)connection didFailWithError:(NSError *)error {
    responseCallback(nil, nil, error);
}

- (void)connection:(NSURLConnection *)connection didReceiveResponse:(NSURLResponse *)theResponse {
    response = (NSHTTPURLResponse *)theResponse;
    [responseData setLength:0];
}

- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data {
    [responseData appendData:data];
}

- (void)connectionDidFinishLoading:(NSURLConnection *)conn {
    NSData *responseBody = [responseData copy];
    responseCallback(response, responseBody, nil);
}

@end

@implementation ZumoTestGlobals

@synthesize client;

+(ZumoTestGlobals *)sharedInstance {
    static ZumoTestGlobals *instance = nil;
    if (!instance) {
        instance = [[ZumoTestGlobals alloc] init];
    }
    
    return instance;
}

- (void)initializeClientWithAppUrl:(NSString *)url andKey:(NSString *)appKey {
    [self setClient:[MSClient clientWithApplicationURLString:url withApplicationKey:appKey]];
}

+(NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day {
    NSCalendar *calendar = [[NSCalendar alloc] initWithCalendarIdentifier:NSGregorianCalendar];
    NSDateComponents *components = [[NSDateComponents alloc] init];
    [components setYear:year];
    [components setMonth:month];
    [components setDay:day];
    [components setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    return [calendar dateFromComponents:components];
}

+(BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2 {
    if ([date1 isKindOfClass:[NSNull class]] && [date2 isKindOfClass:[NSNull class]]) {
        // both are null, ok
        return YES;
    }
    NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
    [formatter setDateFormat:@"yyyy-MM-ddTHH:mm:ss"];
    NSString *str1 = [formatter stringFromDate:date1];
    NSString *str2 = [formatter stringFromDate:date2];
    return [str1 isEqualToString:str2];
}

+(NSMutableDictionary *)propertyBag {
    static NSMutableDictionary *dict = nil;
    if (!dict) {
        dict = [[NSMutableDictionary alloc] init];
    }
    
    return dict;
}

+(void)sendAsyncRequest:(NSString *)method url:(NSString *)url headers:(NSDictionary *)headers body:(NSString *)body completion:(ZumoHttpRequestCompletion)completion {
    ZumoAPMRequest *apmRequest = [[ZumoAPMRequest alloc] initWithMethod:method url:url headers:headers body:body withResponse:completion];
    [apmRequest sendRequest];
}

@end
