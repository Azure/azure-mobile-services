/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

/**
 * FunctionCallKind.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

/**
 * Enumeration of function calls.
 */
enum FunctionCallKind {
    /**
     * The year date function.
     */
    Year,

    /**
     * The month date function.
     */
    Month,

    /**
     * The day date function.
     */
    Day,

    /**
     * The hour date function.
     */
    Hour,

    /**
     * The minute date function.
     */
    Minute,

    /**
     * The second date function.
     */
    Second,

    /**
     * The floor math function.
     */
    Floor,

    /**
     * The ceiling math function.
     */
    Ceiling,

    /**
     * The round math function.
     */
    Round,

    /**
     * The to lower string function.
     */
    ToLower,

    /**
     * The to upper string function.
     */
    ToUpper,

    /**
     * The length string function.
     */
    Length,

    /**
     * The trim string function.
     */
    Trim,

    /**
     * The starts with string function.
     */
    StartsWith,

    /**
     * The ends with string function.
     */
    EndsWith,

    /**
     * The substring of string function.
     */
    SubstringOf,

    /**
     * The concat string function.
     */
    Concat,

    /**
     * The index of string function.
     */
    IndexOf,

    /**
     * The substring string function.
     */
    Substring,

    /**
     * The replace string function.
     */
    Replace
}