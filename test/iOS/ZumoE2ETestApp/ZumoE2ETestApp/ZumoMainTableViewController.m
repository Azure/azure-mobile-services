//
//  ZumoMainTableViewController.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoMainTableViewController.h"
#import "ZumoTestGroup.h"
#import "ZumoTestGlobals.h"
#import "ZumoTestGroupTableViewController.h"
#import "ZumoSavedAppsTableViewController.h"
#import "ZumoTestHelpViewController.h"

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

- (IBAction)loadSavedApp:(id)sender {
    if ([savedApps count] > 0) {
        __block ZumoSavedAppsTableViewController *savedAppsController = [[ZumoSavedAppsTableViewController alloc] init];
        ZumoSavedAppsTableViewController *weakControllerRef = savedAppsController;
        [savedAppsController setSavedApps:savedApps];
        [savedAppsController setCompletion:^(void) {
            NSString *appUrl = [weakControllerRef selectedAppUrl];
            NSString *appKey = [weakControllerRef selectedAppKey];
            if (appUrl && appKey) {
                [appUrlField setText:appUrl];
                [appKeyField setText:appKey];
            }
            
            if ([weakControllerRef savedAppsChanged]) {
                NSArray *newApps = [weakControllerRef savedApps];
                savedApps = [NSMutableArray arrayWithArray:newApps];
                [self saveApps];
                UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Saved" message:@"The modifications in the saved apps were saved to the local storage." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
                [av show];
            }
        }];
        [self presentViewController:savedAppsController animated:YES completion:nil];
    } else {
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"No saved apps." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
    }
}

- (IBAction)saveAppInfo:(id)sender {
    NSString *appUrl = [appUrlField text];
    NSString *appKey = [appKeyField text];
    if ([self validateAppInfoForUrl:appUrl andKey:appKey]) {
        [savedApps addObject:[NSArray arrayWithObjects:appUrl, appKey, nil]];
        [self saveApps];
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Saved" message:@"The application URL and key were saved to the local storage." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [av show];
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
    MSClient *currentClient = [[ZumoTestGlobals sharedInstance] client];
    NSString *appUrl = [appUrlField text];
    NSString *appKey = [appKeyField text];
    BOOL needRefreshClient;
    if (!currentClient) {
        // client not yet set
        needRefreshClient = YES;
    } else if ([[[currentClient applicationURL] absoluteString] isEqualToString:appUrl] && [[currentClient applicationKey] isEqualToString:appKey]) {
        // Same application, no need to reinitialize the client
        needRefreshClient = NO;
    } else {
        needRefreshClient = YES;
    }

    if (needRefreshClient) {
        if (![self validateAppInfoForUrl:appUrl andKey:appKey]) {
            return;
        } else {
            [[ZumoTestGlobals sharedInstance] initializeClientWithAppUrl:appUrl andKey:appKey];
        }
    }

    ZumoTestGroup *subgroup = [[self testGroups] objectAtIndex:[indexPath row]];
    ZumoTestGroupTableViewController *subview = [[ZumoTestGroupTableViewController alloc] init];
    [subview setTests:subgroup];
    [[self navigationController] pushViewController:subview animated:YES];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (IBAction)displayHelp:(id)sender {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"To run this test application:",
                      @"1. Create (or reuse) an application using either the portal or the CLI",
                      @"2. On the application, create the following tables:",
                      @"2.1. iosRoundTripTable (used for round-trip, update and delete tests), no special setting",
                      @"2.2. iosMovies (used for query tests), with the appropriate script",
                      @"2.3. iosApplication (used for login tests), set permissions to 'Application Key'",
                      @"2.4. iosAuthenticated (used for login tests), set permissions to 'Authenticated Users'",
                      @"2.5. iosAdmin (used for login tests), set permissions to 'Admin and Scripts'",
                      @"3. Create applications in all supported identity providers (for login tests)",
                      @"4. Configure the identity tab of the Zumo app to point to the providers (for login tests)",
                      @"5. Run the desired tests by selecing the test group, then tapping 'Run Tests'",
                      @"6. Make sure all the scenarios pass.", nil];
    NSString *helpText = [lines componentsJoinedByString:@"\n"];
    ZumoTestHelpViewController *hvc = [[ZumoTestHelpViewController alloc] init];
    [hvc setTitle:@"General E2E Test App Help" andHelpText:helpText];
    [self presentViewController:hvc animated:YES completion:nil];
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
