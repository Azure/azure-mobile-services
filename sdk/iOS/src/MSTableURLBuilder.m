// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSTableURLBuilder.h"


#pragma mark * MSTableURLBuilder Implementation


@implementation MSTableURLBuilder


#pragma mark * Public URL Builder Methods


+(NSURL *) URLForTable:(MSTable *)table
{
    // Create the table path
    NSString *tablePath = [NSString stringWithFormat:@"tables/%@", table.name];
    
    // Percent encode it
    NSString *tablePathEncoded =
    [MSTableURLBuilder stringPercentEncoded:tablePath];
    
    // Append it to the application URL
    return [table.client.applicationURL
                 URLByAppendingPathComponent:tablePathEncoded];
}

+(NSURL *) URLForTable:(MSTable *)table withItemIdString:(NSString *)itemId
{    
    // Percent encode it
    NSString *itemIdEncoded =
    [MSTableURLBuilder stringPercentEncoded:itemId];
    
    // Append it to the table URL
    return [[self URLForTable:table]
            URLByAppendingPathComponent:itemIdEncoded];
}

+(NSURL *) URLForTable:(MSTable *)table withQuery:(NSString *)query
{
    NSURL *tableURL = [self URLForTable:table];
    
    // Recreate the URL with the query appended
    NSString *urlString = [NSString stringWithFormat:@"%@%@%@",
                           tableURL.absoluteString,
                           tableURL.query ? @"&" : @"?",
                           query];
    
    // Percent encode it
    NSString *urlStringEncoded =
    [MSTableURLBuilder stringPercentEncoded:urlString];
    
    // Return the URL
    return[NSURL URLWithString:urlStringEncoded];
}


#pragma mark * Private Methods


+(NSString *) stringPercentEncoded:(NSString *)string
{
    return [string stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
}

@end
