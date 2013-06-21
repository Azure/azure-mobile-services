// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

(function (global, qunit, undefined) {

    // This file allows our JavaScript client tests to be run in a browser via QUnit     
    
    // ------------------------------------------------------------------------------
    // Top-level test framework APIs invoked by our tests
    global.$test = function (testName) {
        return new Test(testName);
    };

    global.$testGroup = function (groupName /*, test1, test2, ... */) {
        var testsArray = arguments.length > 1 ? Array.prototype.slice.call(arguments, 1) : null;
        return new TestGroup(groupName, testsArray);
    };

    global.$assert = {
        isTrue: qunit.ok,
        isFalse: function (value, message) { qunit.ok(!value, message); },
        isNull: function (value, message) {
            // Our tests treat null and undefined as if they were the same, because part of the
            // WinJS test framework is written in .NET where there isn't a concept of undefined.
            qunit.ok(value === null || value === undefined, message);
        },
        isNotNull: function (value, message) {
            // As above, regard null and undefined as the same thing
            qunit.ok(value !== null && value !== undefined, message);
        },
        areEqual: qunit.equal,
        areNotEqual: qunit.notEqual,
        fail: function (message) { qunit.ok(false, message); },
        contains: function(str, substr, message) {
            message = message || (str + " should contain " + substr);
            qunit.ok((str || "").indexOf(substr) >= 0, message);
        }
    };

    global.$assertThrows = function (action, verify, message) {
        var thrown = true;
        try {
            action();
            thrown = false;
        } catch (ex) {
            if (verify) {
                verify(ex);
            }
        }

        if (!thrown) {
            $assert.fail(message || 'Should have failed!');
        }
    };

    global.$chain = function () {
        /// <summary>
        /// Given a list of functions that return promises, return a promise that
        /// will chain them all together sequentially.
        /// </summary>
        var actions = Array.prototype.slice.call(arguments);
        return new global.Promises.Promise(function (complete, err) {
            var error = function (ex) {
                err(ex.responseText || JSON.stringify(ex));
            };
            var exec = function (prev) {
                if (actions.length > 0) {
                    var next = actions.shift();
                    try {
                        var result = next(prev);
                        if (result) {
                            result.then(function (results) {
                                exec(results);
                            }, error);
                        } else if (actions.length === 0) {
                            complete();
                        } else {
                            error('$chain expects all actions except the last to return another Promise');
                        }
                    } catch (ex) {
                        error(ex);
                    }
                } else {
                    complete();
                }
            };
            exec();
        });
    };

    global.$log = function (message) {
        if (global.console) {
            global.console.log(message);
        }
    };

    global.$fmt = function (formatString /*, arg0, arg1, ... */) {
        var positionalArgs = Array.prototype.slice.call(arguments, 1),
            index;
        for (index = 0; index < positionalArgs.length; index++) {
            formatString = formatString.replace("{" + index + "}", positionalArgs[index]);
        }
        return formatString;
    };

    // ------------------------------------------------------------------------------
    // Test represents a single test
    function Test(testName) {
        this.testName = testName;
        this.tags = [];
    }

    Test.prototype.exclude = function () {
        this.isExcluded = true;
        return this;
    };

    Test.prototype.check = function (testFunc) {
        this.testFunc = testFunc;
        return this;
    };

    Test.prototype.checkAsync = function (testFunc) {
        this.isAsync = true;
        return this.check(testFunc);
    };

    Test.prototype.exec = function () {
        if (this._shouldExclude()) {
            return;
        }

        var self = this;
        qunit.test(this.testName, function () {
            qunit.ok(!!self.testFunc, "Test function was supplied");
            var result = self.testFunc();
            if (self.isAsync) {
                qunit.ok(typeof result.then === "function", "Async test returned a promise");
                qunit.stop();
                result
                    .then(qunit.start, function (err) {
                        qunit.start();
                        qunit.ok(false, err && err.exception || err);
                    });
            }
        });
    };

    Test.prototype.tag = function (tagText) {
        this.tags.push(tagText);
        return this;
    };

    Test.prototype._shouldExclude = function () {
        return this.isExcluded || this._hasTag("exclude-web");
    };

    Test.prototype._hasTag = function (tagText) {
        // Can't use Array.prototype.indexOf because old IE doesn't have it
        for (var i = 0; i < this.tags.length; i++) {
            if (this.tags[i] === tagText) {
                return true;
            }
        }
        return false;
    };

    // These functions are not used in our browser tests - they are ignored
    Test.prototype.functional = function () { return this; };
    Test.prototype.description = function () { return this; };

    // ------------------------------------------------------------------------------
    // TestGroup represents a collection of tests, or in QUnit terms, a "module"
    function TestGroup(groupName, testsArray) {
        this.groupName = groupName;
        if (testsArray) {
            this.tests.apply(this, testsArray);
        }
    }

    TestGroup.prototype.functional = function () { return this; }; // Not used in browser - ignored

    TestGroup.prototype.tests = function (/* test1, test2, ... */) {
        var testsArray = Array.prototype.slice.call(arguments, 0);
        qunit.module(this.groupName);
        for (var i = 0; i < testsArray.length; i++) {
            testsArray[i].exec();
        }
        return this;
    };

})(this, this.QUnit);