//
//  ZumoMainTableViewController.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/8/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoMainTableViewController.h"
#import "ZumoTestGroup.h"
#import "ZumoTestGlobals.h"
#import "ZumoTestGroupTableViewController.h"
#import "ZumoSavedAppsTableViewController.h"

@interface ZumoMainTableViewController ()

@end

@implementation ZumoMainTableViewController

@synthesize testGroups;

- (id)init
{
    self = [super initWithStyle:UITableViewStyleGrouped];
    if (self) {
        NSString *savedAppsPath = [self savedAppsArchivePath];
        savedApps = [NSKeyedUnarchiver unarchiveObjectWithFile:savedAppsPath];
        if (!savedApps) {
            savedApps = [[NSMutableArray alloc] init];
        }
    }
    
    return self;
}

- (id)initWithStyle:(UITableViewStyle)style
{
    return [self init];
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    [[self navigationItem] setTitle:@"Azure Mobile Service E2E Tests"];
}

- (NSString *)savedAppsArchivePath {
    NSArray *documentDirectories = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentDirectory = [documentDirectories objectAtIndex:0];
    return [documentDirectory stringByAppendingPathComponent:@"savedapps.archive"];
}

- (void)saveApps {
    [NSKeyedArchiver archiveRootObject:self->savedApps toFile:[self savedAppsArchivePath]];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (IBAction)resetClient:(id)sender {
    [appUrlField setText:@""];
    [appKeyField setText:@""];
    [appUrlField setEnabled:YES];
    [appKeyField setEnabled:YES];
}

- (IBAction)loadSavedApp:(id)sender {
    if ([savedApps count] > 0) {
        ZumoSavedAppsTableViewController *savedAppsController = [[ZumoSavedAppsTableViewController alloc] init];
        [savedAppsController setSavedApps:savedApps];
        void (^completion)(void) = ^(void) {
            NSString *appUrl = [savedAppsController selectedAppUrl];
            NSString *appKey = [savedAppsController selectedAppKey];
            if (appUrl && appKey) {
                [appUrlField setText:appUrl];
                [appKeyField setText:appKey];
            }
            
            if ([savedAppsController savedAppsChanged]) {
                NSArray *newApps = [savedAppsController savedApps];
                savedApps = [NSMutableArray arrayWithArray:newApps];
                [self saveApps];
            }
        };
        [savedAppsController setCompletion:completion];
        [self presentViewController:savedAppsController animated:YES completion:nil];
    } else {
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"No saved apps" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
    }
}

- (IBAction)saveAppInfo:(id)sender {
    NSString *appUrl = [appUrlField text];
    NSString *appKey = [appKeyField text];
    if ([self validateAppInfoForUrl:appUrl andKey:appKey]) {
        [savedApps addObject:[NSArray arrayWithObjects:appUrl, appKey, nil]];
        [self saveApps];
    }
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [testGroups count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }

    ZumoTestGroup *testGroup = [[self testGroups] objectAtIndex:[indexPath row]];
    [[cell textLabel] setText:[NSString stringWithFormat:@"%d. %@", [indexPath row] + 1, [testGroup name]]];
    
    return cell;
}

- (UIView *)headerView {
    if (!self->headerView) {
        [[NSBundle mainBundle] loadNibNamed:@"ZumoMainTableHeader" owner:self options:nil];
    }
    
    return self->headerView;
}

- (UIView *)tableView:(UITableView *)tableView viewForHeaderInSection:(NSInteger)section {
    return [self headerView];
}

- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section {
    return [[self headerView] bounds].size.height;
}

- (BOOL)validateAppInfoForUrl:(NSString *)appUrl andKey:(NSString *)appKey {
    if ([appUrl length] == 0 || [appKey length] == 0) {
        UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Please set the application URL and key before proceeding" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [alert show];
        return NO;
    } else {
        return YES;
    }
}

#pragma mark - Table view delegate

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    if (![[ZumoTestGlobals sharedInstance] client]) {
        NSString *appUrl = [appUrlField text];
        NSString *appKey = [appKeyField text];
        if (![self validateAppInfoForUrl:appUrl andKey:appKey]) {
            return;
        } else {
            [[ZumoTestGlobals sharedInstance] initializeClientWithAppUrl:appUrl andKey:appKey];
            [appUrlField setEnabled:NO];
            [appKeyField setEnabled:NO];
            [resetClientButton setEnabled:YES];
        }
    }

    ZumoTestGroup *subgroup = [[self testGroups] objectAtIndex:[indexPath row]];
    ZumoTestGroupTableViewController *subview = [[ZumoTestGroupTableViewController alloc] init];
    [subview setTests:subgroup];
    [[self navigationController] pushViewController:subview animated:YES];
}

#pragma mark - ZumoTestGroup delegate

- (void)zumoTestGroupStarted:(NSString *)groupName {
    NSLog(@"Tests for %@ started.", groupName);
}

- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests {
    NSLog(@"Tests for group %@ finished: pass: %d, fail: %d", groupName, passedTests, failedTests);
}

- (void)zumoTestGroupSingleTestStarted:(int)testIndex {
    
}

- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(BOOL)testPassed {
    
}

@end
