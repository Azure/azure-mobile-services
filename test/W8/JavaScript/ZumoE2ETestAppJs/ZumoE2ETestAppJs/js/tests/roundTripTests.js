/// <reference path="/MobileServicesJavaScriptClient/MobileServices.js" />
/// <reference path="../testFramework.js" />

function defineRoundTripTestsNamespace() {
    var tests = [];

    tests.push(new zumo.Test('Setup dynamic schema', function (test, done) {
        var table = zumo.getClient().getTable('w8jsRoundTripTable');
        var item = {
            string1: "test",
            date1: new Date(),
            bool1: true,
            number: -1,
            longnum: 0,
            intnum: 1
        };
        table.insert(item).done(function () {
            test.addLog('Successfully set up the dynamic schema: ' + JSON.stringify(item));
            done(true);
        }, function (err) {
            test.addLog('Error setting up dynamic schema: ' + JSON.stringify(err));
            done(false);
        });
    }));

    for (var i = 0; i < 10; i++) {
        tests.push(new zumo.Test('Test which takes some time ' + i, function (test, done) {
            var delay = 100 * ((i % 10) + 1);
            var willPass = i % 3;
            setTimeout(function () {
                test.addLog('Running my test');
                done(willPass);
            }, delay);
        }));
    }

    return {
        name: 'Round trip',
        tests: tests
    };
}

zumo.tests.roundTrip = defineRoundTripTestsNamespace();
