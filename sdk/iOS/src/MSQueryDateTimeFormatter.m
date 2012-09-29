// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSQueryDateTimeFormatter.h"


#pragma mark * Query String DateTime Formats


NSString *const fullFormat = @"'datetime'''yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'SSSSSSS''";
NSString *const sansFractionalSecondsFormat = @"'datetime'''yyyy'-'MM'-'dd'T'HH':'mm':'ss''";
NSString *const sansSecondsFormat = @"'datetime'''yyyy'-'MM'-'dd'T'HH':'mm''";


#pragma mark * MSQueryDateTimeFormatter Private Interface


@interface MSQueryDateTimeFormatter ()

// Private properties
@property (nonatomic, strong) NSDateFormatter *fullFormatter;
@property (nonatomic, strong) NSDateFormatter *sansFractionalSecondsFormatter;
@property (nonatomic, strong) NSDateFormatter *sansSecondsFormatter;

@end


#pragma mark * MSQueryDateTimeFormatter Implementation


@implementation MSQueryDateTimeFormatter

static MSQueryDateTimeFormatter *staticFormatter;

@synthesize fullFormatter = fullFormatter_;
@synthesize sansFractionalSecondsFormatter = sansFractionalSecondsFormatter_;
@synthesize sansSecondsFormatter = sansSecondsFormatter_;


#pragma mark * Private Initializer Methods


-(id) init
{
    self = [super init];
    if (self) {
        
        // Initialize each formatter
        fullFormatter_ = [[NSDateFormatter alloc] init];
        sansFractionalSecondsFormatter_ = [[NSDateFormatter alloc] init];
        sansSecondsFormatter_ = [[NSDateFormatter alloc] init];
        
        // To ensure we ignore user locale and preferences we use the
        // following locale
        NSLocale *locale = [[NSLocale alloc]
                            initWithLocaleIdentifier:@"en_US_POSIX"];
        
        // Set the locale on each
        [fullFormatter_ setLocale:locale];
        [sansFractionalSecondsFormatter_ setLocale:locale];
        [sansSecondsFormatter_ setLocale:locale];
        
        // Set the date formats for each
        [fullFormatter_ setDateFormat:fullFormat];
        [sansFractionalSecondsFormatter_ setDateFormat:sansFractionalSecondsFormat];
        [sansSecondsFormatter_ setDateFormat:sansSecondsFormat];
    }
    return self;
}


#pragma mark * Public Static Methods


+(NSString *) stringFromDate:(NSDate *)date
{
    NSString *formattedDate = nil;
    
    if (date) {
        
        if (!staticFormatter) {
            staticFormatter = [[MSQueryDateTimeFormatter alloc] init];
        }
        
        NSDateFormatter *formatter;
        
        // Determine if the date has fractional seconds and then seconds
        // because we use different NSDateFormatters since fractional seconds
        // and seconds are optional and we shouldn't include them if they are 0
        NSTimeInterval timeInterval = [date timeIntervalSinceReferenceDate];
        if (trunc(timeInterval) != timeInterval) {
            
            // Has fractional seconds
            formatter = staticFormatter.fullFormatter;
        }
        else if (fmod(timeInterval, 60.0) != 0.0) {
            
            // Doesn't have fractional seconds, but does have seconds
            formatter = staticFormatter.sansFractionalSecondsFormatter;
        } else
        {
            formatter = staticFormatter.sansSecondsFormatter;        
        }
        
        formattedDate = [formatter stringFromDate:date];
    }

    return formattedDate;
}

@end
