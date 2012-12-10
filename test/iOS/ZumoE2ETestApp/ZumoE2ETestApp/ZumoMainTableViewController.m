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

@interface ZumoMainTableViewController ()

@end

@implementation ZumoMainTableViewController

@synthesize testGroups;

- (id)initWithStyle:(UITableViewStyle)style
{
    self = [super initWithStyle:style];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    [[self navigationItem] setTitle:@"Azure Mobile Service E2E Tests"];
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

- (IBAction)managedSavedApps:(id)sender {
    UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Not implemented yet" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
    [av show];
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

#pragma mark - Table view delegate

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    if (![[ZumoTestGlobals sharedInstance] client]) {
        NSString *appUrl = [appUrlField text];
        NSString *appKey = [appKeyField text];
        if ([appUrl length] == 0 || [appKey length] == 0) {
            UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Please set the application URL and key before proceeding" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
            [alert show];
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
