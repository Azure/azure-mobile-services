//
//  ZumoMainTableViewController.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/8/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoMainTableViewController.h"
#import "ZumoTestGroup.h"

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

    // Uncomment the following line to preserve selection between presentations.
    // self.clearsSelectionOnViewWillAppear = NO;
 
    // Uncomment the following line to display an Edit button in the navigation bar for this view controller.
    // self.navigationItem.rightBarButtonItem = self.editButtonItem;
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
    [[cell textLabel] setText:[NSString stringWithFormat:@"%d. %@", [indexPath row] + 1, [testGroup description]]];
    
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
    UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Not implemented yet" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
    [av show];

    // Navigation logic may go here. Create and push another view controller.
    /*
     <#DetailViewController#> *detailViewController = [[<#DetailViewController#> alloc] initWithNibName:@"<#Nib name#>" bundle:nil];
     // ...
     // Pass the selected object to the new view controller.
     [self.navigationController pushViewController:detailViewController animated:YES];
     */
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
