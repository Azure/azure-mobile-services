//
//  ZumoTestGlobals.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/8/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoTestGlobals.h"

NSString * const ZumoKeyStringValue = @"sharedStringValue";

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

@end
