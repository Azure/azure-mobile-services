//
//  ZumoTestGlobals.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

typedef void (^ZumoHttpRequestCompletion)(NSHTTPURLResponse *response, NSData *responseBody, NSError *error);

@interface ZumoTestGlobals : NSObject
{
    
}

@property (nonatomic, strong) MSClient *client;

+(ZumoTestGlobals *)sharedInstance;
-(void)initializeClientWithAppUrl:(NSString *)url andKey:(NSString *)appKey;

// Helper methods
+(NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day;
+(BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2;
+(void)sendAsyncRequest:(NSString *)method url:(NSString *)url headers:(NSDictionary *)headers body:(NSString *)body completion:(ZumoHttpRequestCompletion)completion;

@end
