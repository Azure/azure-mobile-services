// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>

typedef void (^EmptyCompletionBlock)(void);

@interface ZumoSavedAppsTableViewController : UITableViewController
{
    IBOutlet UIButton *editButton;
    IBOutlet UIView *headerView;
}

@property (nonatomic) BOOL savedAppsChanged;
@property (nonatomic, copy) NSString *selectedAppUrl;
@property (nonatomic, copy) NSString *selectedAppKey;
@property (nonatomic, copy) NSMutableArray *savedApps;
@property (nonatomic, strong) EmptyCompletionBlock completion;

- (IBAction)editApps:(id)sender;
- (IBAction)cancel:(id)sender;
- (IBAction)selectApp:(id)sender;

@end
