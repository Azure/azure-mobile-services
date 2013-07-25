// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface ZumoTestStore : NSObject

+ (NSArray *)createTests;

extern NSString * const ALL_TESTS_GROUP_NAME;
extern NSString * const ALL_UNATTENDED_TESTS_GROUP_NAME;

@end
