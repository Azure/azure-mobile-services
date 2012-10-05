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
#import "MSFilter.h"

typedef NSURLRequest *(^MSInspectRequestBlock)(NSURLRequest *request);

@interface MSTestFilter : NSObject <MSFilter>

@property (nonatomic, strong)   NSURLRequest *requestToUse;
@property (nonatomic, copy)     MSInspectRequestBlock onInspectRequest;
@property (nonatomic, strong)   NSHTTPURLResponse *responseToUse;
@property (nonatomic, strong)   NSData *dataToUse;
@property (nonatomic, strong)   NSError *errorToUse;
@property (nonatomic)           BOOL ignoreNextFilter;

@end
