// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// The *MSDateOffset* class is used to represent an NSDate that is stored as a
/// DateTimeOffset in the Mobile Service. This class is used when building
/// NSPredicates for an MSQuery.
@interface MSDateOffset : NSObject

/// @name Properties
/// @{

/// The date represented by tne MSDateOffset instance.
@property (nonatomic, strong) NSDate *date;

/// @}

///@name Initializing the MSDateOffset Object
///@{

/// Initializes an *MSDateOffset* instance with the given date.
-(id)initWithDate:(NSDate *)date;

/// Creates an *MSDateOffset* instance with the given date.
+(id)offsetFromDate:(NSDate *)date;

/// @}

@end
