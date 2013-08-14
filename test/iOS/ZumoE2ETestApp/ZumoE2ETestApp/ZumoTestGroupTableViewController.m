// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGroupTableViewController.h"
#import "ZumoTestHelpViewController.h"
#import "ZumoLogUpdater.h"
#import "ZumoTestGlobals.h"
#import "ZumoTestStore.h"

@interface ZumoTestGroupTableViewController ()

@end

@implementation ZumoTestGroupTableViewController

@synthesize testGroup, logUploadUrl;

- (id)init
{
    self = [super initWithStyle:UITableViewStyleGrouped];
    if (self) {
        
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
    NSString *groupName = [[self testGroup] name];
    [[self navigationItem] setTitle:groupName];
    if ([groupName hasPrefix:ALL_TESTS_GROUP_NAME]) {
        // Start running the tests
        [self runTests:nil];
    }
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

#pragma mark - Table view data source

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [[[self testGroup] tests] count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    ZumoTest *test = [[[self testGroup] tests] objectAtIndex:[indexPath row]];
    UIColor *textColor;
    if ([test testStatus] == TSFailed) {
        textColor = [UIColor redColor];
    } else if ([test testStatus] == TSPassed) {
        textColor = [UIColor greenColor];
    } else if ([test testStatus] == TSRunning) {
        textColor = [UIColor grayColor];
    } else if ([test testStatus] == TSSkipped) {
        textColor = [UIColor magentaColor];
    } else {
        textColor = [UIColor blackColor];
    }
    [[cell textLabel] setTextColor:textColor];
    [[cell textLabel] setText:[NSString stringWithFormat:@"%d. %@", [indexPath row] + 1, [test description]]];
    
    return cell;
}

- (UIView *)headerView {
    if (!headerView) {
        [[NSBundle mainBundle] loadNibNamed:@"ZumoTestGroupTableHeader" owner:self options:nil];
    }
    
    return headerView;
}

- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section {
    return [[self headerView] bounds].size.height;
}

- (UIView *)tableView:(UITableView *)tableView viewForHeaderInSection:(NSInteger)section {
    return [self headerView];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (IBAction)runTests:(id)sender {
    NSLog(@"Start running tests!");
    [[self testGroup] setDelegate:self];
    __weak UIViewController *weakSelf = self;
    [[self testGroup] startExecutingFrom:weakSelf];
}

- (IBAction)resetTests:(id)sender {
    ZumoTest *test;
    for (test in [[self testGroup] tests]) {
        [test resetStatus];
    }
    
    [[self tableView] reloadData];
}

- (IBAction)uploadLogs:(id)sender {
    ZumoTestHelpViewController *helpController = [[ZumoTestHelpViewController alloc] init];
    NSMutableArray *arr = [[NSMutableArray alloc] init];
    ZumoTest *test;
    for (test in [[self testGroup] tests]) {
        NSString *testStatus = [ZumoTest testStatusToString:[test testStatus]];
        NSString *testStartTime = [ZumoTestGlobals dateToString:[test startTime]];
        [arr addObject:[NSString stringWithFormat:@"[%@] Logs for test %@ (%@)", testStartTime, [test testName], testStatus]];
        NSString *logLine;
        for (logLine in [test getLogs]) {
            [arr addObject:logLine];
        }
        [arr addObject:[NSString stringWithFormat:@"[%@] -*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-", [ZumoTestGlobals dateToString:[test endTime]]]];
    }
    
    NSString *allLogs = [arr componentsJoinedByString:@"\n"];
    [helpController setTitle:@"Test logs" andHelpText:allLogs];
    [helpController setModalPresentationStyle:UIModalPresentationFullScreen];
    NSString *urlToUpload = [self logUploadUrl];
    if (urlToUpload && [urlToUpload length] && [[[self testGroup] name] hasPrefix:ALL_TESTS_GROUP_NAME]) {
        ZumoLogUpdater *updater = [[ZumoLogUpdater alloc] init];
        [updater uploadLogs:allLogs toUrl:urlToUpload allTests:YES];
    } else {
        [self presentViewController:helpController animated:YES completion:^(void) {
            if (urlToUpload && [urlToUpload length]) {
                ZumoLogUpdater *updater = [[ZumoLogUpdater alloc] init];
                [updater uploadLogs:allLogs toUrl:urlToUpload allTests:NO];
            }
        }];
    }
}

- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests andSkipped:(int)skippedTests {
    if ([groupName hasPrefix:ALL_TESTS_GROUP_NAME] && [[self logUploadUrl] length] > 0) {
        [self uploadLogs:nil];
    }
}

- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(TestStatus)testStatus {
    [[[[self testGroup] tests] objectAtIndex:testIndex] setTestStatus:testStatus];
    [[self tableView] reloadData];
}

- (void)zumoTestGroupSingleTestStarted:(int)testIndex {
    [[[[self testGroup] tests] objectAtIndex:testIndex] setTestStatus:TSRunning];
}

- (void)zumoTestGroupStarted:(NSString *)groupName {
    NSLog(@"Test group started: %@", groupName);
}

@end
