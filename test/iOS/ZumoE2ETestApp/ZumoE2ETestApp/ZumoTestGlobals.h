// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "ZumoTest.h"

typedef void (^ZumoHttpRequestCompletion)(NSHTTPURLResponse *response, NSData *responseBody, NSError *error);


@protocol PushNotificationReceiver <NSObject>

@required
- (void)pushReceived:(NSDictionary *)userInfo;

@end

@interface ZumoTestGlobals : NSObject
{
    
}

@property (nonatomic, strong) MSClient *client;
@property (nonatomic, copy) NSString *deviceToken;
@property (nonatomic, copy) NSString *remoteNotificationRegistrationStatus;
@property (nonatomic, weak) id<PushNotificationReceiver> pushNotificationDelegate;

+ (ZumoTestGlobals *)sharedInstance;
- (void)initializeClientWithAppUrl:(NSString *)url andKey:(NSString *)appKey;
- (void)saveAppInfo:(NSString *)appUrl key:(NSString *)appKey;
- (NSArray *)loadAppInfo;
- (void)saveUploadLogsUrl:(NSString *)url;
- (NSString *)loadUploadLogsUrl;

// Helper methods
+ (NSString *)testStatusToString:(TestStatus)status;
+ (NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day;
+ (BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2;
+ (BOOL)compareJson:(id)json1 with:(id)json2 log:(NSMutableArray *)errors;

@end
