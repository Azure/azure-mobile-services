//
//  ZumoMainTableViewController.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ZumoTestGroupCallbacks.h"

@interface ZumoMainTableViewController : UITableViewController <ZumoTestGroupCallbacks, UITextFieldDelegate>
{
    IBOutlet UITextField *appUrlField;
    IBOutlet UITextField *appKeyField;
    IBOutlet UIView *headerView;
    IBOutlet UIButton *resetClientButton;
    NSMutableArray *savedApps;
}

@property (nonatomic, copy) NSArray *testGroups;

- (UIView *)headerView;
- (IBAction)resetClient:(id)sender;
- (IBAction)loadSavedApp:(id)sender;
- (IBAction)saveAppInfo:(id)sender;

@end
