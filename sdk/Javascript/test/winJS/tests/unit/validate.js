// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\Zumo.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global Validate:false */

$testGroup('Validate.js',

    $test('notNull')
    .description('Verify Validate.notNull works correctly across null/undefined/etc.')
    .check(function () {
        $assertThrows(function () { Validate.notNull(null); });
        $assertThrows(function () { Validate.notNull(undefined); });
        Validate.notNull('');
        Validate.notNull(0);
        Validate.notNull('foo');
        Validate.notNull([]);
        Validate.notNull({});
        $assertThrows(
            function () { Validate.notNull(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('notNullOrEmpty')
    .description('Verify Validate.notNullOrEmpty works correctly across null/undefined/empty/etc.')
    .check(function () {
        $assertThrows(function () { Validate.notNullOrEmpty(null); });
        $assertThrows(function () { Validate.notNullOrEmpty(undefined); });
        $assertThrows(function () { Validate.notNullOrEmpty(''); });
        $assertThrows(function () { Validate.notNullOrEmpty([]); });
        Validate.notNullOrEmpty(0);
        Validate.notNullOrEmpty('foo');
        Validate.notNullOrEmpty({});
        $assertThrows(
            function () { Validate.notNullOrEmpty(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isValidId')
    .description('Verify Validate.isValidId works correctly across null/undefined/empty/etc.')
    .check(function () {
        $assertThrows(function () { Validate.isValidId(null); });
        $assertThrows(function () { Validate.isValidId(undefined); });
        $assertThrows(function () { Validate.isValidId(''); });
        $assertThrows(function () { Validate.isValidId([]); });
        $assertThrows(function () { Validate.isValidId(0); });
        Validate.isValidId(1);
        Validate.isValidId(654);
        Validate.isValidId('foo');
        $assertThrows(
            function () { Validate.isValidId(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isDate')
    .description('Verify Validate.isDate correctly identifies Date instances.')
    .check(function () {
        Validate.isDate(new Date());
        Validate.isDate(new Date('04/28/06'));
        $assertThrows(function () { Validate.isDate(null); });
        $assertThrows(function () { Validate.isDate(''); });
        $assertThrows(function () { Validate.isDate('foo'); });
        $assertThrows(function () { Validate.isDate(123123); });
        $assertThrows(function () { Validate.isDate('1/1/12'); });
        $assertThrows(
            function () { Validate.isDate(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isNumber')
    .description('Verify Validate.isNumber correctly identifies numeric values.')
    .check(function () {
        Validate.isNumber(0);
        Validate.isNumber(1);
        Validate.isNumber(-1);
        Validate.isNumber(1.5);
        Validate.isNumber(10);
        Validate.isNumber(0.001);
        Validate.isNumber(NaN);
        $assertThrows(function () { Validate.isNumber(null); });
        $assertThrows(function () { Validate.isNumber(''); });
        $assertThrows(function () { Validate.isNumber('foo'); });
        $assertThrows(function () { Validate.isNumber('1'); });
        $assertThrows(
            function () { Validate.isNumber(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isValidParametersObject')
    .description('Verify Validate.isValidParametersObject correctly identifies valid parameter object instances.')
    .check(function () {
        Validate.isValidParametersObject({ state: 'WA' });
        Validate.isValidParametersObject({ code: 5 });
        Validate.isValidParametersObject({ x: { y: 3 } });
        Validate.isValidParametersObject({ });

        var obj = {};
        obj.$x = 5;
        $assertThrows(function () { Validate.isValidParametersObject(obj); });
        $assertThrows(function () { Validate.isValidParametersObject(null); });
        $assertThrows(function () { Validate.isValidParametersObject('foo'); });
        $assertThrows(function () { Validate.isValidParametersObject(1); });

        $assertThrows(
            function () { Validate.isValidParametersObject(obj, 'foo'); },
            function (ex) {
                $assert.contains(ex.toString(), 'foo');
                $assert.contains(ex.toString(), '$x');
            });
    }),

    $test('isInteger')
    .description('Verify Validate.isInteger correctly identifies integral values.')
    .check(function () {
        Validate.isInteger(0);
        Validate.isInteger(1);
        Validate.isInteger(-1);
        Validate.isInteger(10);
        $assertThrows(function () { Validate.isInteger(null); });
        $assertThrows(function () { Validate.isInteger(''); });
        $assertThrows(function () { Validate.isInteger('foo'); });
        $assertThrows(function () { Validate.isInteger('1'); });
        $assertThrows(function () { Validate.isInteger(1.5); });
        $assertThrows(function () { Validate.isInteger(0.001); });
        $assertThrows(function () { Validate.isInteger(NaN); });
        $assertThrows(
            function () { Validate.isInteger(null, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isString')
    .description('Verify Validate.isString correctly identifies string values.')
    .check(function () {
        Validate.isString(null);
        Validate.isString('');
        Validate.isString('foo');
        $assertThrows(function () { Validate.isString(0); });
        $assertThrows(function () { Validate.isString({}); });
        $assertThrows(function () { Validate.isString([]); });
        $assertThrows(
            function () { Validate.isString(10, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('isObject')
    .description('Verify Validate.isObject correctly identifies Object values.')
    .check(function () {
        Validate.isObject(null);
        Validate.isObject({});
        Validate.isObject({ id: 5 });
        $assertThrows(function () { Validate.isObject(0); });
        $assertThrows(function () { Validate.isObject('foo'); });
        $assertThrows(function () { Validate.isObject(new Date()); });
        $assertThrows(
            function () { Validate.isObject(10, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    }),

    $test('length')
    .description('Verify Validate.length correctly identifies strings of a given length')
    .check(function () {
        Validate.length('', 0);
        Validate.length('a', 1);
        Validate.length('ab', 2);
        $assertThrows(function () { Validate.length(null, 1); });
        $assertThrows(function () { Validate.length(null, null); });
        $assertThrows(function () { Validate.length('foo', 2); });
        $assertThrows(
            function () { Validate.length('test', 2, 'foo'); },
            function (ex) { $assert.contains(ex.toString(), 'foo'); });
    })
);