//
//  ZumoMainTableViewController.h
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/8/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ZumoTestGroupCallbacks.h"

@interface ZumoMainTableViewController : UITableViewController <ZumoTestGroupCallbacks>
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
