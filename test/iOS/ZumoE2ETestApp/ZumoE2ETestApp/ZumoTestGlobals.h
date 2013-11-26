// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

typedef void (^ZumoHttpRequestCompletion)(NSHTTPURLResponse *response, NSData *responseBody, NSError *error);

extern NSString *const RUNTIME_VERSION_KEY;
extern NSString *const CLIENT_VERSION_KEY;

@protocol PushNotificationReceiver <NSObject>

@required
- (void)pushReceived:(NSDictionary *)userInfo;

@end

@interface ZumoTestGlobals : NSObject
{
    NSMutableDictionary *globalTestParameters;
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
- (NSMutableDictionary *)globalTestParameters;

// Helper methods
+ (NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day;
+ (BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2;
+ (BOOL)compareObjects:(NSDictionary *)obj1 with:(NSDictionary *)obj2 log:(NSMutableArray *)errors;
+ (BOOL)compareJson:(id)json1 with:(id)json2 log:(NSMutableArray *)errors;
+ (NSString *)dateToString:(NSDate *)date;

@end
