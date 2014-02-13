//
//  ZumoTestRunSetup.h
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 2/6/14.
//  Copyright (c) 2014 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ZumoTestRunSetup : NSObject

+ (NSArray *)createTests;
+ (NSString *)groupDescription;

@end
