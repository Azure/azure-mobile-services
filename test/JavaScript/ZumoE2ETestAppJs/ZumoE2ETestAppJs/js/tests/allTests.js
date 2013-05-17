/// <reference path="../testFramework.js" />
/// <reference path="roundTripTests.js" />
/// <reference path="queryTests.js" />
/// <reference path="updateDeleteTests.js" />
/// <reference path="loginTests.js" />
/// <reference path="miscTests.js" />
/// <reference path="pushTests.js" />

(function () {
    zumo.testGroups.push(new zumo.Group(zumo.tests.roundTrip.name, zumo.tests.roundTrip.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.query.name, zumo.tests.query.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.query.name + '(server side)', zumo.tests.query.serverSideTests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.updateDelete.name, zumo.tests.updateDelete.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.login.name, zumo.tests.login.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.misc.name, zumo.tests.misc.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.push.name, zumo.tests.push.tests));
})();
