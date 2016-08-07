// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.2.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.2.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

// Declare JSHint globals
/*global Extensions:false */

$testGroup('Extensions.js',

    $test('isNull')
    .description('Verify Extensions.isNull works correctly across null/undefined/etc.')
    .check(function () {
        $assert.isTrue(Extensions.isNull(null), 'null should be null.');
        $assert.isTrue(Extensions.isNull(undefined), 'undefined should be null.');
        $assert.isFalse(Extensions.isNull(''), 'empty string is not null.');
        $assert.isFalse(Extensions.isNull(0), '0 is not null.');
        $assert.isFalse(Extensions.isNull('Foo'), "'Foo' is not null.");
    }),

    $test('isNullOrEmpty')
    .description('Verify Extensions.isNullOrEmpty works correctly across null/undefined/empty/etc.')
    .check(function () {
        $assert.isTrue(Extensions.isNullOrEmpty(null), 'null should be null or empty.');
        $assert.isTrue(Extensions.isNullOrEmpty(undefined), 'undefined should be null or empty.');
        $assert.isTrue(Extensions.isNullOrEmpty(''), 'empty string should be null or empty.');
        $assert.isTrue(Extensions.isNullOrEmpty([]), 'empty array should be null or empty.');
        $assert.isFalse(Extensions.isNullOrEmpty(0), '0 is not null or empty.');
        $assert.isFalse(Extensions.isNullOrEmpty('Foo'), "'Foo' is not null empty.");
        $assert.isFalse(Extensions.isNullOrEmpty({ }), '{ } is not null or empty.');
    }),

    $test('isValidId')
    .description('Verify isValidId works correctly across id types')
    .check(function() {
        $assert.isFalse(Extensions.isValidId(null), 'null is an invalid id');
        $assert.isFalse(Extensions.isValidId(undefined), 'undefined is an invalid id');
        $assert.isFalse(Extensions.isValidId(''), 'empty string is an invalid id');
        $assert.isFalse(Extensions.isValidId([]), 'empty array can not be an id');
        $assert.isFalse(Extensions.isValidId({ }), '{ } is an invalid id');
        $assert.isFalse(Extensions.isValidId(0), '0 is an invalid id');
        $assert.isFalse(Extensions.isValidId(new Array(257).join('A')), 'length of 256 is invalid');
        $assert.isFalse(Extensions.isValidId('a+b'), 'id can not contain a +');        
        $assert.isFalse(Extensions.isValidId('a"b'), 'id can not contain a "');
        $assert.isFalse(Extensions.isValidId('a/b'), 'id can not contain a /');
        $assert.isFalse(Extensions.isValidId('a?b'), 'id can not contain a ?');
        $assert.isFalse(Extensions.isValidId('a\\b'), 'id can not contain a \\');
        $assert.isFalse(Extensions.isValidId('a`b'), 'id can not contain a `');
        $assert.isFalse(Extensions.isValidId('.'), 'id can not be .');
        $assert.isFalse(Extensions.isValidId('..'), 'id can not be ..');
        $assert.isFalse(Extensions.isValidId('A\u0000C'), 'id can not contain control character u0000');
        $assert.isFalse(Extensions.isValidId('A__\u0008C'), 'id can not contain control character u0008');

        $assert.isTrue(Extensions.isValidId(10), '10 is a valid id');
        $assert.isTrue(Extensions.isValidId('id'), 'id is a valid id');
        $assert.isTrue(Extensions.isValidId('12.0'), 'id can be a string number');
        $assert.isTrue(Extensions.isValidId('true'), 'id can be a string respresentation of a boolean');
        $assert.isTrue(Extensions.isValidId('false'), 'id can be a string respresentation of a boolean (false)');
        $assert.isTrue(Extensions.isValidId('aa4da0b5-308c-4877-a5d2-03f274632636'), 'id can contain a guid');
        $assert.isTrue(Extensions.isValidId('69C8BE62-A09F-4638-9A9C-6B448E9ED4E7'), 'id can contain another guid');
        $assert.isTrue(Extensions.isValidId('{EC26F57E-1E65-4A90-B949-0661159D0546}'), 'id can contain brackets and guids');
        $assert.isTrue(Extensions.isValidId('id with Russian Где моя машина'), 'id can contain other language characters');
    }),

    $test('format')
    .description('Verify Extensions.format')
    .check(function () {
        $assert.areEqual(Extensions.format(null), null, 'null messages should not be formatted.');
        $assert.areEqual(Extensions.format(null, 1, 'foo'), null, 'null messages should not be formatted even with arguments.');
        $assert.areEqual(Extensions.format('literal'), 'literal');
        $assert.areEqual(Extensions.format('literal', 1, 'foo'), 'literal');
        $assert.areEqual(Extensions.format('test {0}', 'basic'), 'test basic');
        $assert.areEqual(Extensions.format('test {0}', 1), 'test 1');
        $assert.areEqual(Extensions.format('X{0}Z', 'Y'), 'XYZ');
        $assert.areEqual(Extensions.format('X {0} Z', 'Y'), 'X Y Z');
        $assert.areEqual(Extensions.format('{0} Z', 'Y'), 'Y Z');
        $assert.areEqual(Extensions.format('X {0}', 'Y'), 'X Y');
        $assert.areEqual(Extensions.format('{0}{1}', 0, 1), '01');
        $assert.areEqual(Extensions.format('{0}{1}{2}', 0, 1, 2), '012');
        $assert.areEqual(Extensions.format('{0}{1}{2}{3}', 0, 1, 2, 3), '0123');
        $assertThrows(
            function () { Extensions.format(12); },
            function (ex) { $assert.contains(ex.toString(), 'message'); },
            'format did not throw for non-string message value.');

        // Ideally these would behave differently...  see the comment in the
        // method
        $assert.areEqual(Extensions.format('{0}{1}', 0), '0{1}'); // error
        $assert.areEqual(Extensions.format('{0}{1}', '{1}', 'X'), 'XX'); // {1}X
        $assert.areEqual(Extensions.format('{{0}}', '1', 'X'), 'X'); // {0}
    }),

    $test('has')
    .description('Verify Extensions.has')
    .check(function () {
        $assertThrows(function () { Extensions.has(null, null); });
        $assertThrows(function () { Extensions.has(null, 2); });
        Extensions.has(null, 'key');

        var obj = { foo: 1, bar: 2, qux : [1, 2, 3] };
        $assert.isTrue(Extensions.has(obj, 'foo'));
        $assert.isTrue(Extensions.has(obj, 'bar'));
        $assert.isTrue(Extensions.has(obj, 'qux'));
        $assert.isFalse(Extensions.has(obj, 'baz'));
    }),

    $test('isObject')
    .description('Verify Extensions.isObject')
    .check(function () {
        $assert.isTrue(Extensions.isObject(), 'undefined');
        $assert.isFalse(Extensions.isObject(12), 'number');
        $assert.isFalse(Extensions.isObject('test'), 'string');
        $assert.isFalse(Extensions.isObject(true), 'bool');
        $assert.isFalse(Extensions.isObject(function () { }), 'function');
        $assert.isFalse(Extensions.isObject(new Date()), 'date');
        $assert.isTrue(Extensions.isObject(null), 'null');
        $assert.isTrue(Extensions.isObject({}), 'obj');
    }),

    $test('isString')
    .description('Verify Extensions.isString')
    .check(function () {
        $assert.isTrue(Extensions.isString(null), 'null');
        $assert.isTrue(Extensions.isString(), 'undefined');
        $assert.isFalse(Extensions.isString(12), 'number');
        $assert.isTrue(Extensions.isString('test'), 'string');
        $assert.isFalse(Extensions.isObject(true), 'bool');
        $assert.isFalse(Extensions.isString(function () { }), 'function');
        $assert.isFalse(Extensions.isString(new Date()), 'date');
        $assert.isFalse(Extensions.isString({}), 'obj');
    }),

    $test('isNumber')
    .description('Verify Extensions.isNumber')
    .check(function () {
        $assert.isFalse(Extensions.isNumber(null), 'null');
        $assert.isFalse(Extensions.isNumber(), 'undefined');
        $assert.isTrue(Extensions.isNumber(12), 'int');
        $assert.isTrue(Extensions.isNumber(12.5), 'float');
        $assert.isFalse(Extensions.isNumber('test'), 'string');
        $assert.isFalse(Extensions.isNumber(true), 'bool');
        $assert.isFalse(Extensions.isNumber(function () { }), 'function');
        $assert.isFalse(Extensions.isNumber(new Date()), 'date');
        $assert.isFalse(Extensions.isNumber({}), 'obj');
    }),

    $test('isBool')
    .description('Verify Extensions.isBool')
    .check(function () {
        $assert.isFalse(Extensions.isBool(null), 'null');
        $assert.isFalse(Extensions.isBool(), 'undefined');
        $assert.isFalse(Extensions.isBool(12), 'number');
        $assert.isFalse(Extensions.isBool('test'), 'string');
        $assert.isTrue(Extensions.isBool(true), 'bool');
        $assert.isFalse(Extensions.isBool(function () { }), 'function');
        $assert.isFalse(Extensions.isBool(new Date()), 'date');
        $assert.isFalse(Extensions.isBool({}), 'obj');
    }),

    $test('isDate')
    .description('Verify Extensions.isDate')
    .check(function () {
        $assert.isFalse(Extensions.isDate(null), 'null');
        $assert.isFalse(Extensions.isDate(), 'undefined');
        $assert.isFalse(Extensions.isDate(12), 'number');
        $assert.isFalse(Extensions.isDate('test'), 'string');
        $assert.isFalse(Extensions.isDate(true), 'bool');
        $assert.isFalse(Extensions.isDate(function () { }), 'function');
        $assert.isTrue(Extensions.isDate(new Date()), 'date');
        $assert.isFalse(Extensions.isDate({}), 'obj');
    }),

    $test('toJson')
    .description('Verify Extensions.toJson')
    .check(function () {
        $assert.areEqual('{"a":1}', Extensions.toJson({ a: 1 }));
    }),

    $test('fromJson')
    .description('Verify Extensions.fromJson')
    .check(function () {
        $assert.areEqual('foo', Extensions.fromJson('{"test":"foo"}').test);
        $assert.areEqual(2009, Extensions.fromJson('{"test":"2009-11-21T22:22:59.860Z"}').test.getFullYear());
    }),

    $test('createUniqueInstallationId')
    .description('Verify Extensions.createUniqueInstallationId')
    .check(function () {
        $assert.areNotEqual(Extensions.createUniqueInstallationId(), Extensions.createUniqueInstallationId());
    }),

    $test('mapProperties')
    .description('Verify Extensions.mapProperties')
    .check(function () {
        var obj = { foo: 1, bar: 2 };
        var results = Extensions.mapProperties(obj, function (k, v) { return k + '=' + v; }).join(',');
        $assert.areEqual(results, 'foo=1,bar=2');
    }),

    $test('pad')
    .description('Verify Extensions.pad')
    .check(function () {
        $assertThrows(function () { Extensions.pad(null, 1, '0'); });
        $assertThrows(function () { Extensions.pad(1, 'fasdfas', '0'); });
        $assertThrows(function () { Extensions.pad(1, 1, 1); });
        $assertThrows(function () { Extensions.pad(1, 1, null); });
        $assertThrows(function () { Extensions.pad(1, 1, 'abc'); });

        $assert.areEqual('01', Extensions.pad(1, 2, '0'));
        $assert.areEqual('001', Extensions.pad(1, 3, '0'));
        $assert.areEqual('12', Extensions.pad(12, 2, '0'));
        $assert.areEqual('012', Extensions.pad(12, 3, '0'));
        $assert.areEqual('??1', Extensions.pad(1, 3, '?'));
    }),

    $test('trimEnd')
    .description('Verify Extensions.trimEnd')
    .check(function () {
        $assertThrows(function () { Extensions.trimEnd(null, '1'); });
        $assertThrows(function () { Extensions.trimEnd(1, '1'); });
        $assertThrows(function () { Extensions.trimEnd('foo', null); });
        $assertThrows(function () { Extensions.trimEnd('foo', 1); });
        $assertThrows(function () { Extensions.trimEnd('foo', 'abc'); });

        $assert.areEqual('123', Extensions.trimEnd('123', '/'));
        $assert.areEqual('123', Extensions.trimEnd('123/', '/'));
        $assert.areEqual('/123', Extensions.trimEnd('/123', '/'));
        $assert.areEqual('/123', Extensions.trimEnd('/123/', '/'));
        $assert.areEqual('/123', Extensions.trimEnd('/123//', '/'));
        $assert.areEqual('', Extensions.trimEnd('/', '/'));
        $assert.areEqual('', Extensions.trimEnd('///', '/'));
        $assert.areEqual('', Extensions.trimEnd('', '/'));
    }),

    $test('trimStart')
    .description('Verify Extensions.trimStart')
    .check(function () {
        $assertThrows(function () { Extensions.trimStart(null, '1'); });
        $assertThrows(function () { Extensions.trimStart(1, '1'); });
        $assertThrows(function () { Extensions.trimStart('foo', null); });
        $assertThrows(function () { Extensions.trimStart('foo', 1); });
        $assertThrows(function () { Extensions.trimStart('foo', 'abc'); });

        $assert.areEqual('123', Extensions.trimStart('123', '/'));
        $assert.areEqual('123/', Extensions.trimStart('123/', '/'));
        $assert.areEqual('123', Extensions.trimStart('/123', '/'));
        $assert.areEqual('123/', Extensions.trimStart('/123/', '/'));
        $assert.areEqual('123/', Extensions.trimStart('//123/', '/'));
        $assert.areEqual('', Extensions.trimStart('/', '/'));
        $assert.areEqual('', Extensions.trimStart('///', '/'));
        $assert.areEqual('', Extensions.trimStart('', '/'));
    }),

    $test('compareCaseInsensitive')
    .description('Verify Extensions.compareCaseInsensitive')
    .check(function () {
        $assert.isTrue(Extensions.compareCaseInsensitive('abc', 'abc'));
        $assert.isTrue(Extensions.compareCaseInsensitive('ABC', 'ABC'));
        $assert.isTrue(Extensions.compareCaseInsensitive('', ''));
        $assert.isTrue(Extensions.compareCaseInsensitive(null, null));
        $assert.isTrue(Extensions.compareCaseInsensitive('ABC', 'abc'));
        $assert.isTrue(Extensions.compareCaseInsensitive('aBC', 'abc'));
        $assert.isTrue(Extensions.compareCaseInsensitive('aBc', 'abc'));

        $assert.isFalse(Extensions.compareCaseInsensitive('', 'abc'));
        $assert.isFalse(Extensions.compareCaseInsensitive(' ABC', 'abc'));
    }),

    $test('url.combinePathSegments')
    .description('Verify Extensions.url.combinePathSegments')
    .check(function () {
        $assertThrows(function () { Extensions.url.combinePathSegments(); });
        $assertThrows(function () { Extensions.url.combinePathSegments(1); });
        $assertThrows(function () { Extensions.url.combinePathSegments('foo', 1); });

        $assert.areEqual('foo/bar', Extensions.url.combinePathSegments('foo', 'bar'));
        $assert.areEqual('foo', Extensions.url.combinePathSegments('foo'));
        $assert.areEqual('/foo/bar/', Extensions.url.combinePathSegments('/foo', 'bar/'));
        $assert.areEqual('foo/bar', Extensions.url.combinePathSegments('foo', '/bar'));
        $assert.areEqual('foo/bar', Extensions.url.combinePathSegments('foo/', 'bar'));
        $assert.areEqual('foo/bar', Extensions.url.combinePathSegments('foo/', '/bar'));
        $assert.areEqual('foo/bar/baz', Extensions.url.combinePathSegments('foo/', '/bar/', '/baz'));
    }),

   $test('url.getQueryString')
    .description('Verify Extensions.url.getQueryString')
    .check(function () {
        $assertThrows(function () { Extensions.url.getQueryString(); });
        $assertThrows(function () { Extensions.url.getQueryString(1); });
        $assertThrows(function () { Extensions.url.getQueryString('foo'); });
   
        $assert.areEqual('state=WA', Extensions.url.getQueryString({ state: 'WA' }));
        $assert.areEqual('state=WA&code=5', Extensions.url.getQueryString({ state: 'WA', code: 5 }));
        $assert.areEqual('questions=What%20is%20your%20favorite%20food%3F', Extensions.url.getQueryString({ questions: 'What is your favorite food?' }));
        $assert.areEqual('x=%7B%22y%22%3A%5B5%2C7%2C%22Hello!%22%5D%7D', Extensions.url.getQueryString({ x: { y: [5, 7, 'Hello!'] } }));
    }),

    $test('url.combinePathAndQuery')
    .description('Verify Extensions.url.combinePathAndQuery')
    .check(function () {
        $assertThrows(function () { Extensions.url.combinePathAndQuery(); });
        $assertThrows(function () { Extensions.url.combinePathAndQuery(1); });
        $assertThrows(function () { Extensions.url.combinePathAndQuery('foo', 1); });

        $assert.areEqual('\\somePath?someQuery=true', Extensions.url.combinePathAndQuery('\\somePath', 'someQuery=true'));
        $assert.areEqual('\\somePath?someQuery=true', Extensions.url.combinePathAndQuery('\\somePath', '?someQuery=true'));
    }),

    $test('url.isAbsoluteUrl')
    .description('Verify Extensions.url.isAbsoluteUrl')
    .check(function () {
        $assert.areEqual(Extensions.url.isAbsoluteUrl('ms-appx://www.test.com/'), false);
        $assert.areEqual(Extensions.url.isAbsoluteUrl('www.test.com/'), false);
        $assert.areEqual(Extensions.url.isAbsoluteUrl('$filter=(id eq 6)'), false);

        $assert.areEqual(Extensions.url.isAbsoluteUrl('https://www.test.com/'), true);
        $assert.areEqual(Extensions.url.isAbsoluteUrl('http://www.test.com/'), true);
    }),

    $test('tryParseIsoDateString')
    .description('Verify Extensions.tryParseIsoDateString')
    .check(function () {
        $assertThrows(function () { Extensions.tryParseIsoDateString(1); });
        $assertThrows(function () { Extensions.tryParseIsoDateString(new Date()); });

        // Note: we convert to UTC first so the test will be independent of the
        // machine's current time zone
        var date = new Date(Date.UTC(1999, 1, 12));
        $assert.areEqual(date.valueOf(), Extensions.tryParseIsoDateString("1999-02-12T00:00:00.000Z").valueOf());
        $assert.isNull(Extensions.tryParseIsoDateString("1999-02-12T00:00:00.000"));
        $assert.isNull(Extensions.tryParseIsoDateString("1999-02-12T00:00:00.00"));
        $assert.isNull(Extensions.tryParseIsoDateString("1999-02-12T00:00:00"));
        $assert.isNull(Extensions.tryParseIsoDateString("1999-02-12T00:00"));
        $assert.isNull(Extensions.tryParseIsoDateString('rasfoaksdfasdf'));

        // Verify different formats for seconds and milliseconds
        var milliseconds = { "": 0, ".1": 100, ".22": 220, ".333": 333 };
        for (var millisecondString in milliseconds) {
            date = new Date(Date.UTC(2013, 1, 23, 15, 26, 37, milliseconds[millisecondString]));
            var dateString = "2013-02-23T15:26:37" + millisecondString + "Z";
            $assert.areEqual(date.valueOf(), Extensions.tryParseIsoDateString(dateString).valueOf());
        }
    }),

    $test('createError')
    .description('Verify the creation of error messages')
    .check(function () {
        // Default
        var error = Extensions.createError();
        $assert.areEqual(error.message, 'Unexpected failure.');
        $assert.isNull(error.request);
        $assert.isNull(error.exception);

        // String
        error = Extensions.createError('BOOM');
        $assert.areEqual(error.message, 'BOOM');
        $assert.isNull(error.request);
        $assert.isNull(error.exception);

        // Object
        error = Extensions.createError({ x: 123 });
        $assert.areEqual(error.message, 'Unexpected failure.');
        $assert.isNull(error.request);
        $assert.areEqual(error.exception.x, 123);

        // Failing request (error in body)
        error = Extensions.createError(null, { status: 400, responseText: '{"error":"BOOM"}'});
        $assert.areEqual(error.message, 'BOOM');
        $assert.areEqual(error.request.status, 400);
        $assert.isNull(error.exception);

        // Failing request (description in body)
        error = Extensions.createError(null, { status: 400, responseText: '{"description":"BOOM"}' });
        $assert.areEqual(error.message, 'BOOM');
        $assert.areEqual(error.request.status, 400);
        $assert.isNull(error.exception);

        // Failing request (statusText)
        error = Extensions.createError(null, { status: 400, responseText: '{"other":"BOOM"}', statusText: 'EXPLODED' });
        $assert.areEqual(error.message, 'EXPLODED');
        $assert.areEqual(error.request.status, 400);
        $assert.isNull(error.exception);

        // Failing connection
        error = Extensions.createError(null, { status: 0, responseText: '{"description":"BOOM"}' });
        $assert.areEqual(error.message, 'Unexpected connection failure.');
        $assert.areEqual(error.request.status, 0);
        $assert.isNull(error.exception);
    }),

    $test('installIdFormat')
    .description('Verify the format of the installation ids')
    .check(function () {
        // Check to make sure that our installtion id format 
        // has the same format as a GUID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

        // Generate the id
    var id = Extensions.createUniqueInstallationId();

        // Make sure the hyphens are in the right places
    $assert.areEqual(id[8], '-');
    $assert.areEqual(id[13], '-');
    $assert.areEqual(id[18], '-');
    $assert.areEqual(id[23], '-');

        // Ensure that we only have hyphens and alpha-numeric characters
    $assert.isNotNull(id.match(/^[a-z0-9\-]+$/i));
    })
);
