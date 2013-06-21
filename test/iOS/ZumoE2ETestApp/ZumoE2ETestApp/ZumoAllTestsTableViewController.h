// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import "ZumoTestGroupCallbacks.h"

@interface ZumoAllTestsTableViewController : UITableViewController <ZumoTestGroupCallbacks>
{
    NSArray *allTestObjects;
    NSArray *allTestGroups;
    int currentTestGroup;
    NSString *uploadLogsUrl;
    id<ZumoTestGroupCallbacks> previousGroupDelegate;
}

- (id)initWithTests:(NSArray *)testGroups uploadLogsTo:(NSString *)uploadUrl;
- (id)initWithStyle:(UITableViewStyle)style tests:(NSArray *)testGroups uploadLogsTo:(NSString *)uploadUrl;

@end
