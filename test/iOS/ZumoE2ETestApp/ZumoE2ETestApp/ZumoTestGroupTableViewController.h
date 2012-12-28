//
//  ZumoTestGroupTableViewController.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ZumoTestGroup.h"
#import "ZumoTestGroupCallbacks.h"

@interface ZumoTestGroupTableViewController : UITableViewController <UITextFieldDelegate, ZumoTestGroupCallbacks>
{
    IBOutlet UITextField *uploadUrl;
    IBOutlet UIView *headerView;
}

@property (nonatomic, strong) ZumoTestGroup *tests;

- (IBAction)runTests:(id)sender;
- (IBAction)resetTests:(id)sender;
- (IBAction)uploadLogs:(id)sender;
- (IBAction)showHelp:(id)sender;

- (UIView *)headerView;

@end
