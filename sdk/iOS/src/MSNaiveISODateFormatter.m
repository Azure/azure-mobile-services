// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSNaiveISODateFormatter.h"


#pragma mark * DateTime Format

NSString *const format = @"yyyy-MM-dd'T'HH:mm:ss.SSS'Z'";


#pragma mark * MSDateFormatter Implementation


@implementation MSNaiveISODateFormatter


static MSNaiveISODateFormatter *staticDateFormatterSingleton;


#pragma mark * Public Static Singleton Constructor


+(MSNaiveISODateFormatter *) naiveISODateFormatter
{
    if (staticDateFormatterSingleton == nil) {
        staticDateFormatterSingleton = [[MSNaiveISODateFormatter alloc] init];
    }
    
    return  staticDateFormatterSingleton;
}


#pragma mark * Public Initializer Methods


-(id) init
{
    self = [super init];
    if (self) {

        // To ensure we ignore user locale and preferences we use the
        // following locale
        NSLocale *locale = [[NSLocale alloc]
                            initWithLocaleIdentifier:@"en_US_POSIX"];
        [self setLocale:locale];
        
        // Set the date format
        [self setDateFormat:format];
        
        // Set the time zone to GMT
        [self setTimeZone:[NSTimeZone timeZoneWithAbbreviation:@"GMT"]];
    }
    return self;
}

@end
