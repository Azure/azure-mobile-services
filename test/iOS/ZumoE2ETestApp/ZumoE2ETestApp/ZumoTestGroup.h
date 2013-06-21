// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "ZumoTest.h"
#import "ZumoTestGroupCallbacks.h"

@interface ZumoTestGroup : NSObject <ZumoTestCallbacks>
{
    BOOL runningTests;
    UIViewController *associatedViewController;
}

@property (nonatomic) int testsPassed;
@property (nonatomic) int testsFailed;
@property (nonatomic, strong) NSMutableArray *tests;
@property (nonatomic, copy) NSString *name;
@property (nonatomic, copy) NSString *groupDescription;
@property (nonatomic, weak) id<ZumoTestGroupCallbacks> delegate;

- (void)addTest:(ZumoTest *)test;
- (void)startExecutingFrom:(UIViewController *)viewController;

@end
