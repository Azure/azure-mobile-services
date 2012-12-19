//
//  ZumoMiscTests.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/11/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoMiscTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

// Private class
@interface UserAgentGrabberFilter : NSObject <MSFilter>

@property (nonatomic, copy) NSString *userAgent;

@end

@implementation UserAgentGrabberFilter

@synthesize userAgent;

- (void)handleRequest:(NSURLRequest *)request onNext:(MSFilterNextBlock)onNext onResponse:(MSFilterResponseBlock)onResponse {
    userAgent = request.allHTTPHeaderFields[@"User-Agent"];
    onNext(request, onResponse);
}

@end

// Main implementation
@implementation ZumoMiscTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createUserAgentTest]];
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create a table called 'iOSTodoItem'.",
                      @"3. Click on the 'Misc Tests' button.",
                      @"4. Make sure all the tests pass.",
                      nil];
    return [lines componentsJoinedByString:@"\n"];
}

+ (ZumoTest *)createUserAgentTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"User-Agent test" andExecution:nil];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        UserAgentGrabberFilter *filter = [[UserAgentGrabberFilter alloc] init];
        client = [client clientwithFilter:filter];
        MSTable *table = [client getTable:@"iosTodoItem"];
        NSDictionary *item = @{@"name":@"john doe"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [weakRef addLog:[NSString stringWithFormat:@"Error: %@", error]];
            } else {
                NSNumber *itemId = inserted[@"id"];
                [table deleteWithId:itemId completion:nil]; // clean-up after this test
                NSString *userAgent = [filter userAgent];
                if ([userAgent rangeOfString:@"objective-c"].location == NSNotFound) {
                    [weakRef addLog:[NSString stringWithFormat:@"Error, user-agent does not contain 'objective-c': %@", userAgent]];
                } else {
                    passed = YES;
                    [weakRef addLog:[NSString stringWithFormat:@"User-Agent: %@", userAgent]];
                }
            }

            [weakRef setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
    
    return result;
}

@end
