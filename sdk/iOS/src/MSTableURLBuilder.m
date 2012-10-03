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

    // Append it to the application URL; Don't percent encode the tablePath
    // because URLByAppending will percent encode for us
    return [table.client.applicationURL
                 URLByAppendingPathComponent:tablePath];
}

+(NSURL *) URLForTable:(MSTable *)table withItemIdString:(NSString *)itemId
{        
    // Append it to the table URL; Don't percent encode the tablePath
    // because URLByAppending will percent encode for us
    return [[self URLForTable:table]
                URLByAppendingPathComponent:itemId];
}

+(NSURL *) URLForTable:(MSTable *)table withQuery:(NSString *)query
{
    NSURL *url = [self URLForTable:table];
    
    if (query) {
        
        // Percent encode just the query... the table URL is already
        // percent encoded.
        NSString *queryEncoded = [MSTableURLBuilder stringPercentEncoded:query];
        
        // Recreate the URL with the query appended
        NSString *urlString = [NSString stringWithFormat:@"%@%@%@",
                               url.absoluteString,
                               url.query ? @"&" : @"?",
                               queryEncoded];
        
        // Get the URL
        url = [NSURL URLWithString:urlString];
    }
    
    return url;
}


#pragma mark * Private Methods


+(NSString *) stringPercentEncoded:(NSString *)string
{
    return [string stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
}

@end
