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
 * BinaryOperatorKind.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

/**
 * Enumeration of binary operators.
 */
enum BinaryOperatorKind {
    /**
     * The logical or operator.
     */
    Or,

    /**
     * The logical and operator.
     */
    And,

    /**
     * The equal to operator.
     */
    Eq,

    /**
     * The not equal to operator.
     */
    Ne,

    /**
     * The greater than operator.
     */
    Gt,

    /**
     * The greater than or equal to operator.
     */
    Ge,

    /**
     * The less than operator.
     */
    Lt,

    /**
     * The less than or equal to operator.
     */
    Le,

    /**
     * The add operator.
     */
    Add,

    /**
     * The subtract operator.
     */
    Sub,

    /**
     * The multiply operator.
     */
    Mul,

    /**
     * The divide operator.
     */
    Div,

    /**
     * The modulo operator.
     */
    Mod
}