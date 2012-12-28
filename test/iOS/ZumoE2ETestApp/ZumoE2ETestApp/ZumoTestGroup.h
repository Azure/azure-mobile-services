//
//  ZumoTestGroup.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ZumoTest.h"
#import "ZumoTestGroupCallbacks.h"

@interface ZumoTestGroup : NSObject <ZumoTestCallbacks>
{
    int testsPassed;
    int testsFailed;
    BOOL runningTests;
    UIViewController *associatedViewController;
}

@property (nonatomic, strong) NSMutableArray *tests;
@property (nonatomic, copy) NSString *name;
@property (nonatomic, copy) NSString *helpText;
@property (nonatomic, weak) id<ZumoTestGroupCallbacks> delegate;

- (void)addTest:(ZumoTest *)test;
- (void)startExecutingFrom:(UIViewController *)viewController;

@end
