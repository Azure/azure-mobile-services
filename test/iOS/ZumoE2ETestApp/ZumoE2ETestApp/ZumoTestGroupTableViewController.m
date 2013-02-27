//
//  ZumoTestGroupTableViewController.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoTestGroupTableViewController.h"
#import "ZumoTestHelpViewController.h"
#import "ZumoLogUpdater.h"

@interface ZumoTestGroupTableViewController ()

@end

@implementation ZumoTestGroupTableViewController

@synthesize tests;

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

    [[self navigationItem] setTitle:[[self tests] name]];
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
    return [[[self tests] tests] count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    ZumoTest *test = [[[self tests] tests] objectAtIndex:[indexPath row]];
    UIColor *textColor;
    if ([test testStatus] == TSFailed) {
        textColor = [UIColor redColor];
    } else if ([test testStatus] == TSPassed) {
        textColor = [UIColor greenColor];
    } else if ([test testStatus] == TSRunning) {
        textColor = [UIColor grayColor];
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
    [[self tests] setDelegate:self];
    __weak UIViewController *weakSelf = self;
    [[self tests] startExecutingFrom:weakSelf];
}

- (IBAction)resetTests:(id)sender {
    ZumoTest *test;
    for (test in [[self tests] tests]) {
        [test resetStatus];
    }
    
    [[self tableView] reloadData];
}

- (IBAction)uploadLogs:(id)sender {
    ZumoTestHelpViewController *helpController = [[ZumoTestHelpViewController alloc] init];
    NSMutableArray *arr = [[NSMutableArray alloc] init];
    ZumoTest *test;
    for (test in [[self tests] tests]) {
        NSString *testStatus;
        switch ([test testStatus]) {
            case TSFailed:
                testStatus = @"Failed";
                break;
                
            case TSPassed:
                testStatus = @"Passed";
                break;
                
            case TSNotRun:
                testStatus = @"NotRun";
                break;
                
            case TSRunning:
                testStatus = @"Running";
                break;
                
            default:
                testStatus = @"Unkonwn";
                break;
        }
        [arr addObject:[NSString stringWithFormat:@"Logs for test %@ (status = %@)", [test testName], testStatus]];
        NSString *logLine;
        for (logLine in [test getLogs]) {
            [arr addObject:logLine];
        }
        [arr addObject:@"---------------------"];
    }
    
    NSString *allLogs = [arr componentsJoinedByString:@"\n"];
    [helpController setTitle:@"Test logs" andHelpText:allLogs];
    [helpController setModalPresentationStyle:UIModalPresentationFullScreen];
    NSString *urlToUpload = [uploadUrl text];
    [self presentViewController:helpController animated:YES completion:^(void) {
        if (urlToUpload && [urlToUpload length]) {
            ZumoLogUpdater *updater = [[ZumoLogUpdater alloc] init];
            [updater uploadLogs:allLogs toUrl:urlToUpload];
        }
    }];
}

- (IBAction)showHelp:(id)sender {
    ZumoTestHelpViewController *helpController = [[ZumoTestHelpViewController alloc] init];
    [helpController setTitle:[[self tests] name] andHelpText:[[self tests] helpText]];
    [helpController setModalPresentationStyle:UIModalPresentationFullScreen];
    [self presentViewController:helpController animated:YES completion:nil];
}

- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests {
    
}

- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(BOOL)testPassed {
    [[[[self tests] tests] objectAtIndex:testIndex] setTestStatus:(testPassed ? TSPassed : TSFailed)];
    [[self tableView] reloadData];
}

- (void)zumoTestGroupSingleTestStarted:(int)testIndex {
    [[[[self tests] tests] objectAtIndex:testIndex] setTestStatus:TSRunning];
}

- (void)zumoTestGroupStarted:(NSString *)groupName {
    NSLog(@"Test group started: %@", groupName);
}

@end
