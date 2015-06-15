// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

/// Settings that control the pull behavior
@interface MSPullSettings : NSObject

#pragma mark * Properties

///@name Properties that control pull behavior
///@{

/// Size of pages requested from the server while performing a pull
@property (nonatomic) NSInteger pageSize;

///@}

@end
