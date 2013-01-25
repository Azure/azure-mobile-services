/// <reference path="roundTripTests.js" />
/// <reference path="updateDeleteTests.js" />
/// <reference path="loginTests.js" />
/// <reference path="queryTests.js" />
/// <reference path="../testFramework.js" />

(function () {
    zumo.testGroups.push(new zumo.Group(zumo.tests.roundTrip.name, zumo.tests.roundTrip.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.query.name, zumo.tests.query.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.updateDelete.name, zumo.tests.updateDelete.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.login.name, zumo.tests.login.tests));
})();
