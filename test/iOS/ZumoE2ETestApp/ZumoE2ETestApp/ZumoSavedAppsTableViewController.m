// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoSavedAppsTableViewController.h"

@interface ZumoSavedAppsTableViewController ()

@end

@implementation ZumoSavedAppsTableViewController

@synthesize selectedAppUrl, selectedAppKey, completion, savedAppsChanged;
@synthesize savedApps = _savedApps;

- (id)initWithStyle:(UITableViewStyle)style
{
    self = [super initWithStyle:style];
    if (self) {
        [self setSavedAppsChanged:NO];
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    self.clearsSelectionOnViewWillAppear = YES;
    self.navigationItem.rightBarButtonItem = self.editButtonItem;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)setSavedApps:(NSMutableArray *)savedApps {
    _savedApps = [NSMutableArray arrayWithArray:savedApps];
}

- (IBAction)editApps:(id)sender {
    if ([self isEditing]) {
        [editButton setTitle:@"Edit apps" forState:UIControlStateNormal];
        [self setEditing:NO animated:YES];
    } else {
        [editButton setTitle:@"Done editing" forState:UIControlStateNormal];
        [self setEditing:YES animated:YES];
    }
}

- (IBAction)cancel:(id)sender {
    [self setSavedAppsChanged:NO];
    [self setSavedApps:nil];
    [self setSelectedAppUrl:nil];
    [self setSelectedAppKey:nil];
    [self dismissAndCallCompletion];
}

- (IBAction)selectApp:(id)sender {
    if (selectedAppUrl && selectedAppKey) {
        [self dismissAndCallCompletion];
    } else if ([_savedApps count] == 0) {
        // No saved apps, can return
        [self dismissAndCallCompletion];
    } else {
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Please select an app" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
    }
}

- (void)dismissAndCallCompletion {
    [[self presentingViewController] dismissViewControllerAnimated:YES completion:[self completion]];
}

#pragma mark - Table view data source

- (UIView *)headerView {
    if (!headerView) {
        [[NSBundle mainBundle] loadNibNamed:@"ZumoSavedAppsHeader" owner:self options:nil];
    }
    
    return headerView;
}

- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section {
    return [[self headerView] bounds].size.height;
}

- (UIView *)tableView:(UITableView *)tableView viewForHeaderInSection:(NSInteger)section {
    return [self headerView];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [_savedApps count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    NSArray *appData = [[self savedApps] objectAtIndex:[indexPath row]];
    NSString *appUrl = [appData objectAtIndex:0];
    [[cell textLabel] setText:appUrl];
    
    return cell;
}

- (BOOL)tableView:(UITableView *)tableView canEditRowAtIndexPath:(NSIndexPath *)indexPath
{
    return YES;
}

- (void)tableView:(UITableView *)tableView commitEditingStyle:(UITableViewCellEditingStyle)editingStyle forRowAtIndexPath:(NSIndexPath *)indexPath
{
    if (editingStyle == UITableViewCellEditingStyleDelete) {
        [self setSavedAppsChanged:YES];
        [_savedApps removeObjectAtIndex:[indexPath row]];
        [tableView deleteRowsAtIndexPaths:@[indexPath] withRowAnimation:UITableViewRowAnimationFade];
    }
}

- (void)tableView:(UITableView *)tableView moveRowAtIndexPath:(NSIndexPath *)fromIndexPath toIndexPath:(NSIndexPath *)toIndexPath
{
    int from = [fromIndexPath row];
    int to = [toIndexPath row];
    if (from == to) return;
    [self setSavedAppsChanged:YES];
    NSArray *item = [_savedApps objectAtIndex:from];
    [_savedApps removeObjectAtIndex:from];
    [_savedApps insertObject:item atIndex:to];
}

- (BOOL)tableView:(UITableView *)tableView canMoveRowAtIndexPath:(NSIndexPath *)indexPath
{
    return YES;
}

#pragma mark - Table view delegate

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    NSArray *appData = [_savedApps objectAtIndex:[indexPath row]];
    [self setSelectedAppUrl:[appData objectAtIndex:0]];
    [self setSelectedAppKey:[appData objectAtIndex:1]];
}

@end
