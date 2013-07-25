// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import "ZumoTestGroup.h"
#import "ZumoTestGroupCallbacks.h"

@interface ZumoTestGroupTableViewController : UITableViewController <UITextFieldDelegate, ZumoTestGroupCallbacks>
{
    IBOutlet UIView *headerView;
}

@property (nonatomic, strong) ZumoTestGroup *testGroup;
@property (nonatomic, strong) NSString *logUploadUrl;

- (IBAction)runTests:(id)sender;
- (IBAction)resetTests:(id)sender;
- (IBAction)uploadLogs:(id)sender;

- (UIView *)headerView;

@end
