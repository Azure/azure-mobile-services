//
//  ZumoTestGlobals.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

@interface ZumoTestGlobals : NSObject
{
    
}

@property (nonatomic, strong) MSClient *client;

+(ZumoTestGlobals *)sharedInstance;
-(void)initializeClientWithAppUrl:(NSString *)url andKey:(NSString *)appKey;

// Helper methods
+(NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day;
+(BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2;

// Data used in multiple tests
+(NSMutableDictionary *)propertyBag;
extern NSString * const ZumoKeyStringValue;

@end
