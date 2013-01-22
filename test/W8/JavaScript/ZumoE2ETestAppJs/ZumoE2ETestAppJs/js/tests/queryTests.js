/// <reference path="../testFramework.js" />

function defineQueryTestsNamespace() {
    var tests = [];

    tests.push(new zumo.Test('Populate table, if necessary', function (test, done) {
        test.addLog('Populating the table');
        done(true);
    }));

    tests.push(new zumo.Test('Populate table, if necessary - 2', function (test, done) {
        test.addLog('Populating the table');
        done(true);
    }));

    tests.push(new zumo.Test('Populate table, if necessary - 3', function (test, done) {
        test.addLog('Populating the table');
        done(true);
    }));

    return {
        name: 'Query',
        tests: tests
    };
}

zumo.tests.query = defineQueryTestsNamespace();
