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


#pragma mark * MSQueryDateTimeFormatter Public Interface


// The |MSQueryDateTimeFormatter| can format an |NSDate| as a string according
// to the normative OData protocol specification:
//
// datetimeUriLiteral = "datetime" SINGLEQUOTE dateTimeLiteral SINGLEQUOTE
// dateTimeLiteral = year "-" month "-" day "T" hour ":" minute [":" second
//     ["." nanoSeconds]]
// year = 4 *Digit;
// month = <any number between 1 and 12 inclusive>
// day = nonZeroDigit / ("0" nonZeroDigit) /("1" DIGIT) / ("2" DIGIT )
//     / "3" ("0" / "1")
// hour = nonZeroDigit / ("0" nonZeroDigit) / ("1" DIGIT) / ("2" zeroToFour)
// zeroToFour= <any nuumber between 0 and 4 inclusive>
// minute =doubleZeroToSixty
// second = doubleZeroToSixty
// nanoSeconds= 1*7Digit
//
// Example: datetime'2012-05-21T00:00:29'
@interface MSQueryDateTimeFormatter : NSObject

+(NSString *) stringFromDate:(NSDate *)date;

@end
