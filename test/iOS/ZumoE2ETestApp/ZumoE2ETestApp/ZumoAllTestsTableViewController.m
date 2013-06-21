// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoAllTestsTableViewController.h"
#import "ZumoTest.h"
#import "ZumoTestGroup.h"
#import "ZumoTestGlobals.h"
#import "ZumoLogUpdater.h"

@interface AggregateTestObject : NSObject

@property (nonatomic) int index;
@property (nonatomic) BOOL isGroup;
@property (nonatomic, strong) ZumoTest *test;
@property (nonatomic, strong) ZumoTestGroup *group;

- (id)initForGroup:(ZumoTestGroup *)group atIndex:(int)index;
- (id)initForTest:(ZumoTest *)test atIndex:(int)testIndex fromGroup:(ZumoTestGroup *)group;

@end

@implementation AggregateTestObject

@synthesize isGroup = _isGroup, test = _test, group = _group, index = _index;

- (id)initForGroup:(ZumoTestGroup *)group atIndex:(int)index {
    self = [super init];
    if (self) {
        self->_index = index;
        self->_isGroup = YES;
        self->_group = group;
        self->_test = nil;
    }
    
    return self;
}

- (id)initForTest:(ZumoTest *)test atIndex:(int)testIndex fromGroup:(ZumoTestGroup *)group {
    self = [super init];
    if (self) {
        self->_index = testIndex;
        self->_isGroup = NO;
        self->_test = test;
        self->_group = group;
    }
    
    return self;
}

@end

@interface ZumoAllTestsTableViewController ()

@end

@implementation ZumoAllTestsTableViewController

- (id)initWithTests:(NSArray *)testGroups uploadLogsTo:(NSString *)uploadUrl
{
    self = [super initWithStyle:UITableViewStyleGrouped];
    if (self) {
        self->currentTestGroup = -1;
        self->uploadLogsUrl = uploadUrl;
        self->allTestGroups = [[NSArray alloc] initWithArray:testGroups];
        NSMutableArray *objects = [[NSMutableArray alloc] init];
        self->allTestObjects = objects;
        int groupIndex = 0;
        for (ZumoTestGroup *group in testGroups) {
            [objects addObject:[[AggregateTestObject alloc] initForGroup:group atIndex:groupIndex++]];
            int testIndex = 0;
            for (ZumoTest *test in [group tests]) {
                [objects addObject:[[AggregateTestObject alloc] initForTest:test atIndex:testIndex++ fromGroup:group]];
            }
        }
    }
    
    return self;
}

- (id)initWithStyle:(UITableViewStyle)style tests:(NSArray *)testGroups uploadLogsTo:(NSString *)uploadUrl
{
    return [self initWithTests:testGroups uploadLogsTo:uploadUrl];
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    [self runNextTestGroup];
}

- (void)runNextTestGroup {
    if (self->currentTestGroup >= 0) {
        [[allTestGroups objectAtIndex:currentTestGroup] setDelegate:previousGroupDelegate];
    }
    
    self->currentTestGroup++;

    if (self->currentTestGroup >= [allTestGroups count]) {
        [self uploadLogs];
    } else {
        ZumoTestGroup *testGroup = [allTestGroups objectAtIndex:currentTestGroup];
        previousGroupDelegate = [testGroup delegate];
        [testGroup setDelegate:self];
        [testGroup startExecutingFrom:self];
    }
}

- (void)uploadLogs {
    NSMutableString *logs = [[NSMutableString alloc] init];
    [logs appendString:@"Logs for all test groups - iOS\n"];
    [logs appendString:@"------------------------------\n"];
    int totalPass = 0, totalFail = 0;
    for (ZumoTestGroup *group in allTestGroups) {
        NSArray *tests = [group tests];
        int passed = [group testsPassed];
        int failed = [group testsFailed];
        [logs appendFormat:@"Tests in group: %@ (%d passed, %d failed)\n", [group name], passed, failed];
        totalPass += passed;
        totalFail += failed;
        for (ZumoTest *test in tests) {
            NSString *testStatus = [ZumoTestGlobals testStatusToString:[test testStatus]];
            [logs appendFormat:@"Logs for test %@ (status = %@)", [test testName], testStatus];
            for (NSString *logLine in [test getLogs]) {
                [logs appendFormat:@"%@\n", logLine];
            }
            [logs appendString:@"---------------------\n"];
        }

        [logs appendFormat:@"End of group: %@\n", [group name]];
        [logs appendString:@"------------------------------\n"];
    }

    ZumoLogUpdater *uploader = [[ZumoLogUpdater alloc] init];
    [uploader uploadLogs:logs toUrl:self->uploadLogsUrl];
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
    return [allTestObjects count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    AggregateTestObject *testObject = [allTestObjects objectAtIndex:[indexPath row]];
    NSString *textLabel;
    UIColor *textColor;
    ZumoTestGroup *group = [testObject group];
    int groupIndex = [allTestGroups indexOfObject:[testObject group]];
    if ([testObject isGroup]) {
        textColor = [UIColor blackColor];
        textLabel = [NSString stringWithFormat:@"%d. %@", groupIndex + 1, [group name]];
    } else {
        ZumoTest *test = [testObject test];
        if ([test testStatus] == TSFailed) {
            textColor = [UIColor redColor];
        } else if ([test testStatus] == TSPassed) {
            textColor = [UIColor greenColor];
        } else if ([test testStatus] == TSRunning) {
            textColor = [UIColor grayColor];
        } else {
            textColor = [UIColor blackColor];
        }
        
        textLabel = [NSString stringWithFormat:@"%d.%d %@", groupIndex + 1, [testObject index] + 1, [test description]];
    }

    [[cell textLabel] setTextColor:textColor];
    [[cell textLabel] setText:textLabel];
    
    return cell;

}

/*
// Override to support conditional editing of the table view.
- (BOOL)tableView:(UITableView *)tableView canEditRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Return NO if you do not want the specified item to be editable.
    return YES;
}
*/

/*
// Override to support editing the table view.
- (void)tableView:(UITableView *)tableView commitEditingStyle:(UITableViewCellEditingStyle)editingStyle forRowAtIndexPath:(NSIndexPath *)indexPath
{
    if (editingStyle == UITableViewCellEditingStyleDelete) {
        // Delete the row from the data source
        [tableView deleteRowsAtIndexPaths:@[indexPath] withRowAnimation:UITableViewRowAnimationFade];
    }   
    else if (editingStyle == UITableViewCellEditingStyleInsert) {
        // Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view
    }   
}
*/

/*
// Override to support rearranging the table view.
- (void)tableView:(UITableView *)tableView moveRowAtIndexPath:(NSIndexPath *)fromIndexPath toIndexPath:(NSIndexPath *)toIndexPath
{
}
*/

/*
// Override to support conditional rearranging of the table view.
- (BOOL)tableView:(UITableView *)tableView canMoveRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Return NO if you do not want the item to be re-orderable.
    return YES;
}
*/

#pragma mark - Table view delegate

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Navigation logic may go here. Create and push another view controller.
    /*
     <#DetailViewController#> *detailViewController = [[<#DetailViewController#> alloc] initWithNibName:@"<#Nib name#>" bundle:nil];
     // ...
     // Pass the selected object to the new view controller.
     [self.navigationController pushViewController:detailViewController animated:YES];
     */
}

#pragma mark - Zumo test group delegate
- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests {
    NSLog(@"Test group finished: %@", groupName);
    [self runNextTestGroup];
}

- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(BOOL)testPassed {
    ZumoTest *currentTest = [allTestGroups[self->currentTestGroup] tests][testIndex];
    [currentTest setTestStatus:(testPassed ? TSPassed : TSFailed)];
    [[self tableView] reloadData];
}

- (void)zumoTestGroupSingleTestStarted:(int)testIndex {
    ZumoTest *currentTest = [allTestGroups[self->currentTestGroup] tests][testIndex];
    [currentTest setTestStatus:TSRunning];
    [[self tableView] reloadData];
}

- (void)zumoTestGroupStarted:(NSString *)groupName {
    NSLog(@"Test group started: %@", groupName);
}


@end
