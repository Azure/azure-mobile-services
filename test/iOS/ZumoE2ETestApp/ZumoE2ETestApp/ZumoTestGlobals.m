// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGlobals.h"

NSString *const UserDefaultApplicationUrl = @"ZumoE2ETest_AppUrl";
NSString *const UserDefaultApplicationKey = @"ZumoE2ETest_AppKey";
NSString *const UserDefaultUploadLogsUrl = @"ZumoE2ETest_UploadLogUrl";
NSString *const RUNTIME_VERSION_KEY = @"client-version";
NSString *const CLIENT_VERSION_KEY = @"server-version";

@implementation ZumoTestGlobals

@synthesize client, deviceToken, remoteNotificationRegistrationStatus, pushNotificationDelegate;

+(ZumoTestGlobals *)sharedInstance {
    static ZumoTestGlobals *instance = nil;
    if (!instance) {
        instance = [[ZumoTestGlobals alloc] init];
    }
    
    return instance;
}

- (id)init {
    self = [super init];
    if (self) {
        self->globalTestParameters = [[NSMutableDictionary alloc] init];
    }
    return self;
}

- (void)initializeClientWithAppUrl:(NSString *)url andKey:(NSString *)appKey {
    [self setClient:[MSClient clientWithApplicationURLString:url applicationKey:appKey]];
}

- (NSMutableDictionary *)globalTestParameters {
    return self->globalTestParameters;
}

- (void)saveAppInfo:(NSString *)appUrl key:(NSString *)appKey {
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    [defaults setObject:appUrl forKey:UserDefaultApplicationUrl];
    [defaults setObject:appKey forKey:UserDefaultApplicationKey];
    [defaults synchronize];
}

- (NSArray *)loadAppInfo {
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *appUrl = [defaults objectForKey:UserDefaultApplicationUrl];
    NSString *appKey = [defaults objectForKey:UserDefaultApplicationKey];
    if (appUrl && appKey) {
        return [NSArray arrayWithObjects:appUrl, appKey, nil];
    } else {
        return nil;
    }
}

- (void)saveUploadLogsUrl:(NSString *)url {
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    [defaults setObject:url forKey:UserDefaultUploadLogsUrl];
    [defaults synchronize];
}

- (NSString *)loadUploadLogsUrl {
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *uploadLogsUrl = [defaults objectForKey:UserDefaultUploadLogsUrl];
    return uploadLogsUrl;
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

+(NSString *)dateToString:(NSDate *)date{
    static NSDateFormatter *formatter;
    if (!formatter) {
        formatter = [[NSDateFormatter alloc] init];
        [formatter setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
        [formatter setDateFormat:@"yyyy-MM-dd HH:mm:ss.SSS"];
    }
    
    return [formatter stringFromDate:date];
}

+(NSMutableDictionary *)propertyBag {
    static NSMutableDictionary *dict = nil;
    if (!dict) {
        dict = [[NSMutableDictionary alloc] init];
    }
    
    return dict;
}

+(BOOL)compareObjects:(NSDictionary *)obj1 with:(NSDictionary *)obj2 log:(NSMutableArray *)errors {
    NSDictionary *first = [self cloneAndRemoveId:obj1];
    NSDictionary *second = [self cloneAndRemoveId:obj2];
    return [self compareJson:first with:second log:errors];
}

+(NSDictionary *)cloneAndRemoveId:(NSDictionary *)dic {
    NSMutableDictionary *result = [[NSMutableDictionary alloc] init];
    for (NSString *key in [dic allKeys]) {
        if (![key isEqualToString:@"id"]) {
            [result setValue:[dic objectForKey:key] forKey:key];
        }
    }
    return result;
}

+(BOOL)compareJson:(id)json1 with:(id)json2 log:(NSMutableArray *)errors {
    if (!json1 && !json2) return YES;
    if ([json1 isKindOfClass:[NSNull class]] && [json2 isKindOfClass:[NSNull class]]) return YES;
    if (!json1 || !json2 || [json1 isKindOfClass:[NSNull class]] || [json2 isKindOfClass:[NSNull class]]) {
        [errors addObject:[NSString stringWithFormat:@"Only one is null - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }

    BOOL firstIsString = [json1 isKindOfClass:[NSString class]];
    BOOL secondIsString = [json2 isKindOfClass:[NSString class]];
    if (firstIsString && secondIsString) {
        NSString *str1 = json1;
        NSString *str2 = json2;
        if ([str1 isEqualToString:str2]) {
            return YES;
        } else {
            [errors addObject:[NSString stringWithFormat:@"Different strings - json1 = %@, json2 = %@", str1, str2]];
            return NO;
        }
    } else if (firstIsString || secondIsString) {
        [errors addObject:[NSString stringWithFormat:@"Only one is a string - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }

    BOOL firstIsNumber = [json1 isKindOfClass:[NSNumber class]];
    BOOL secondIsNumber = [json2 isKindOfClass:[NSNumber class]];
    if (firstIsNumber && secondIsNumber) {
        NSNumber *num1 = json1;
        NSNumber *num2 = json2;
        if ([num1 isEqualToNumber:num2]) {
            return YES;
        } else {
            [errors addObject:[NSString stringWithFormat:@"Different number/bool - json1 = %@, json2 = %@", num1, num2]];
            return NO;
        }
    } else if (firstIsNumber || secondIsNumber) {
        [errors addObject:[NSString stringWithFormat:@"Only one is a number/bool - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }
    
    BOOL firstIsDate = [json1 isKindOfClass:[NSDate class]];
    BOOL secondIsDate = [json2 isKindOfClass:[NSDate class]];
    if (firstIsDate && secondIsDate) {
        NSDate *date1 = json1;
        NSDate *date2 = json2;
        if ([self compareDate:date1 withDate:date2]) {
            return YES;
        } else {
            [errors addObject:[NSString stringWithFormat:@"Different date - json1 = %@, json2 = %@", date1, date2]];
            return NO;
        }
    } else if (firstIsDate || secondIsDate) {
        [errors addObject:[NSString stringWithFormat:@"Only one is a date - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }

    BOOL firstIsArray = [json1 isKindOfClass:[NSArray class]];
    BOOL secondIsArray = [json2 isKindOfClass:[NSArray class]];
    if (firstIsArray && secondIsArray) {
        NSArray *arr1 = json1;
        NSArray *arr2 = json2;
        if ([arr1 count] == [arr2 count]) {
            for (int i = 0; i < [arr1 count]; i++) {
                id value1 = [arr1 objectAtIndex:i];
                id value2 = [arr2 objectAtIndex:i];
                if (![self compareJson:value1 with:value2 log:errors]) {
                    [errors addObject:[NSString stringWithFormat:@"Error comparing element %d of the array", i]];
                    return NO;
                }
            }
            return YES;
        } else {
            [errors addObject:[NSString stringWithFormat:@"Different array sizes - json1.len = %d, json2.len = %d", [arr1 count], [arr2 count]]];
            return NO;
        }
    } else if (firstIsArray || secondIsArray) {
        [errors addObject:[NSString stringWithFormat:@"Only one is an array - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }
    
    BOOL firstIsObject = [json1 isKindOfClass:[NSDictionary class]];
    BOOL secondIsObject = [json2 isKindOfClass:[NSDictionary class]];
    if (firstIsObject && secondIsObject) {
        NSDictionary *dic1 = json1;
        NSDictionary *dic2 = json2;
        if ([dic1 count] == [dic2 count]) {
            for (NSString *key in [dic1 allKeys]) {
                id value1 = dic1[key];
                id value2 = dic2[key];
                if (![self compareJson:value1 with:value2 log:errors]) {
                    [errors addObject:[NSString stringWithFormat:@"Error comparing element with key '%@ 'of the dictionary", key]];
                    return NO;
                }
            }
            return YES;
        } else {
            [errors addObject:[NSString stringWithFormat:@"Different dictionary sizes - json1.len = %d, json2.len = %d", [dic1 count], [dic2 count]]];
            return NO;
        }
    } else if (firstIsObject || secondIsObject) {
        [errors addObject:[NSString stringWithFormat:@"Only one is an array - json1 = %@, json2 = %@", json1, json2]];
        return NO;
    }
    
    [errors addObject:[NSString stringWithFormat:@"Unknown types. json1 = %@, json2 = %@", json1, json2]];
    return NO;
}

@end
