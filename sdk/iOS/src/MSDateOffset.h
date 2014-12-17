// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

@interface MSDateOffset : NSObject

@property (nonatomic, strong) NSDate *date;

-(id)initWithDate:(NSDate *)date;

+(id)offsetFromDate:(NSDate *)date;

@end
