//
//  ZumoLogUpdater.h
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/17/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ZumoLogUpdater : NSObject <NSURLConnectionDelegate>
{
    NSMutableData *receivedData;
    NSURLConnection *connection;
}

-(void)uploadLogs:(NSString *)logText toUrl:(NSString *)url;

@end
