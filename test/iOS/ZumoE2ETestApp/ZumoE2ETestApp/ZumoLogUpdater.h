//
//  ZumoLogUpdater.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ZumoLogUpdater : NSObject

-(void)uploadLogs:(NSString *)logText toUrl:(NSString *)url;

@end
