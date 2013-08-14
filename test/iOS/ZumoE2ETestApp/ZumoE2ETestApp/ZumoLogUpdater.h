// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface ZumoLogUpdater : NSObject

-(void)uploadLogs:(NSString *)logText toUrl:(NSString *)url allTests:(BOOL) allTests;

@end
