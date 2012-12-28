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
    NSMutableArray *savedApps;
}

@property (nonatomic, copy) NSArray *testGroups;

- (UIView *)headerView;
- (IBAction)loadSavedApp:(id)sender;
- (IBAction)saveAppInfo:(id)sender;
- (IBAction)displayHelp:(id)sender;

@end
