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

#import <Foundation/Foundation.h>
#import "MSError.h"

typedef void (^MSFilterResponseBlock)(NSHTTPURLResponse *response, NSData *data);

typedef void (^MSFilterNextBlock)(NSURLRequest *request,
                                  MSFilterResponseBlock onResponse,
                                  MSErrorBlock onError);

@protocol MSFilter <NSObject>

-(void) handleRequest:(NSURLRequest *)request
               onNext:(MSFilterNextBlock)onNext
           onResponse:(MSFilterResponseBlock)onResponse
              onError:(MSErrorBlock)onError;
@end
