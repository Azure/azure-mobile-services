﻿/// <reference path="../testFramework.js" />

function definePushTestsNamespace() {
    var tests = [];
    var i;
    var tableName = 'w8PushTest';
    var pushChannel;
    var pushNotifications = Windows.Networking.PushNotifications;
    var pushNotificationQueue = [];
    var onPushNotificationReceived = function (e) {
        var notificationPayload;
        switch (e.notificationType) {
            case pushNotifications.PushNotificationType.toast:
                notificationPayload = e.toastNotification.content.getXml();
                break;

            case pushNotifications.PushNotificationType.tile:
                notificationPayload = e.tileNotification.content.getXml();
                break;

            case pushNotifications.PushNotificationType.badge:
                notificationPayload = e.badgeNotification.content.getXml();
                break;

            case pushNotifications.PushNotificationType.raw:
                notificationPayload = e.rawNotification.content;
                break;
        }
        pushNotificationQueue.push({ type: e.notificationType, content: notificationPayload });
    }

    function waitForNotification(timeout, timeAfterPush, continuation) {
        /// <param name="timeout" type="Number">Time to wait for push notification in milliseconds</param>
        /// <param name="timeAfterPush" type="Number">Time to sleep after a push is received. Used to prevent
        ///            blasting the push notification service.</param>
        /// <param name="continuation" type="function(Object)">Function called when the timeout expires.
        ///            If there was a push notification, it will be passed; otherwise null will be passed
        ///            to the function.</param>
        if (typeof timeAfterPush === 'function') {
            continuation = timeAfterPush;
            timeAfterPush = 3000; // default to 3 seconds
        }
        var start = Date.now();
        var waitForPush = function () {
            var now = Date.now();
            if (pushNotificationQueue.length) {
                var notification = pushNotificationQueue.pop();
                setTimeout(function () {
                    continuation(notification);
                }, timeAfterPush);
            } else {
                if ((now - start) > timeout) {
                    continuation(null); // Timed out
                } else {
                    setTimeout(waitForPush, 500); // try it again in 500ms
                }
            }
        }

        waitForPush();
    }

    tests.push(new zumo.Test('Register push channel', function (test, done) {
        var pushManager = Windows.Networking.PushNotifications.PushNotificationChannelManager;
        pushManager.createPushNotificationChannelForApplicationAsync().done(function (channel) {
            test.addLog('Created push channel: ', { uri: channel.uri, expirationTime: channel.expirationTime });
            channel.onpushnotificationreceived = onPushNotificationReceived;
            pushChannel = channel;
            done(true);
        }, function (error) {
            test.addLog('Error creating push channel: ', error);
            done(false);
        });
    }));

    var imageUrl = 'http://zumotestserver.azurewebsites.net/content/zumo2.png';
    var wideImageUrl = 'http://zumotestserver.azurewebsites.net/content/zumo1.png';

    tests.push(createPushTest('sendRaw', 'hello world', 'hello world'));
    tests.push(createPushTest('sendRaw', 'non-ASCII áéíóú', 'non-ASCII áéíóú'));

    tests.push(createPushTest('sendToastText01',
        { text1: 'hello world' },
        '<toast><visual><binding template="ToastText01"><text id="1">hello world</text></binding></visual></toast>'));
    tests.push(createPushTest('sendToastImageAndText03',
        { text1: 'ts-iat3-1', text2: 'ts-iat3-2', image1src: imageUrl, image1alt: 'zumo' },
        '<toast><visual><binding template="ToastImageAndText03"><image id="1" src="' + imageUrl + '" alt="zumo"/><text id="1">ts-iat3-1</text><text id="2">ts-iat3-2</text></binding></visual></toast>'));
    tests.push(createPushTest('sendToastImageAndText04',
        { text1: 'ts-iat4-1', text2: 'ts-iat4-2', text3: 'ts-iat4-3', image1src: imageUrl, image1alt: 'zumo' },
        '<toast><visual><binding template="ToastImageAndText04"><image id="1" src="' + imageUrl + '" alt="zumo"/><text id="1">ts-iat4-1</text><text id="2">ts-iat4-2</text><text id="3">ts-iat4-3</text></binding></visual></toast>'));

    tests.push(createPushTest('sendBadge', 4, '<badge value="4" version="1"/>'));
    tests.push(createPushTest('sendBadge', 'playing', '<badge value="playing" version="1"/>'));

    tests.push(createPushTest('sendTileWideImageAndText02',
        { text1: 'tl-wiat2-1', text2: 'tl-wiat2-2', image1src: wideImageUrl, image1alt: 'zumowide' },
        '<tile><visual><binding template="TileWideImageAndText02"><image id="1" src="' + wideImageUrl + '" alt="zumowide"/><text id="1">tl-wiat2-1</text><text id="2">tl-wiat2-2</text></binding></visual></tile>'));
    tests.push(createPushTest('sendTileWideImageCollection',
        {
            image1src: wideImageUrl, image2src: imageUrl, image3src: imageUrl, image4src: imageUrl, image5src: imageUrl,
            image1alt: 'widezumo', image2alt: 'zumo', image3alt: 'zumo', image4alt: 'zumo', image5alt: 'zumo'
        },
        '<tile><visual><binding template="TileWideImageCollection">' +
            '<image id="1" src="' + wideImageUrl + '" alt="widezumo"/>' +
            '<image id="2" src="' + imageUrl + '" alt="zumo"/>' +
            '<image id="3" src="' + imageUrl + '" alt="zumo"/>' +
            '<image id="4" src="' + imageUrl + '" alt="zumo"/>' +
            '<image id="5" src="' + imageUrl + '" alt="zumo"/>' +
            '</binding></visual></tile>'));
    tests.push(createPushTest('sendTileWideText02',
        {
            text1: 'tl-wt02-caption',
            text2: 'tl-wt02-1', text3: 'tl-wt02-2', text4: 'tl-wt02-3', text5: 'tl-wt02-4',
            text6: 'tl-wt02-5', text7: 'tl-wt02-6', text8: 'tl-wt02-7', text9: 'tl-wt02-8'
        },
        '<tile><visual><binding template="TileWideText02">' +
            '<text id="1">tl-wt02-caption</text>' +
            '<text id="2">tl-wt02-1</text>' +
            '<text id="3">tl-wt02-2</text>' +
            '<text id="4">tl-wt02-3</text>' +
            '<text id="5">tl-wt02-4</text>' +
            '<text id="6">tl-wt02-5</text>' +
            '<text id="7">tl-wt02-6</text>' +
            '<text id="8">tl-wt02-7</text>' +
            '<text id="9">tl-wt02-8</text>' +
            '</binding></visual></tile>'));
    tests.push(createPushTest('sendTileSquarePeekImageAndText01',
        {
            text1: 'tl-spiat1-1',
            text2: 'tl-spiat1-2',
            text3: 'tl-spiat1-3',
            text4: 'tl-spiat1-4',
            image1src: imageUrl,
            image1alt: 'zumo'
        },
        '<tile><visual><binding template="TileSquarePeekImageAndText01">' +
            '<image id="1" src="' + imageUrl + '" alt="zumo"/>' +
            '<text id="1">tl-spiat1-1</text>' +
            '<text id="2">tl-spiat1-2</text>' +
            '<text id="3">tl-spiat1-3</text>' +
            '<text id="4">tl-spiat1-4</text>' +
            '</binding></visual></tile>'));
        tests.push(createPushTest('sendTileSquareBlock',
        { text1: '24', text2: 'aliquam' },
        '<tile><visual><binding template="TileSquareBlock"><text id="1">24</text><text id="2">aliquam</text></binding></visual></tile>'));

    tests.push(new zumo.Test('Unregister push channel', function (test, done) {
        if (pushChannel) {
            pushChannel.close();
            done(true);
        } else {
            test.addLog('Error, push channel needs to be registered.');
            done(false);
        }

        pushChannel = null;
    }));

    function createPushTest(wnsMethod, payload, expectedPushPayload) {
        /// <param name="wnsMethod" type="String">The method on the WNS module</param>
        /// <param name="payload" type="object">The payload to be sent to WNS</param>
        /// <param name="expectedPushPayload" type="String">The result which will be returned on the callback</param>
        var testName = 'Test for ' + wnsMethod + ': ';
        var payloadString = JSON.stringify(payload);
        testName += payloadString.length < 15 ? payloadString : (payloadString.substring(0, 15) + "...");

        var expectedNotificationType;
        if (wnsMethod.indexOf('Badge') >= 0) {
            expectedNotificationType = pushNotifications.PushNotificationType.badge;
        } else if (wnsMethod.indexOf('Raw') >= 0) {
            expectedNotificationType = pushNotifications.PushNotificationType.raw;
        } else if (wnsMethod.indexOf('Tile') >= 0) {
            expectedNotificationType = pushNotifications.PushNotificationType.tile;
        } else if (wnsMethod.indexOf('Toast') >= 0) {
            expectedNotificationType = pushNotifications.PushNotificationType.toast;
        } else {
            throw "Unknown wnsMethod";
        }

        if (typeof expectedPushPayload === 'object') {
            expectedPushPayload = JSON.stringify(expectedPushPayload);
        }

        return new zumo.Test(testName, function (test, done) {
            test.addLog('Test for method ', wnsMethod, ' with payload ', payload);
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            var item = {
                method: wnsMethod,
                channelUri: pushChannel.uri,
                payload: payload
            };
            table.insert(item).done(function (inserted) {
                if (inserted.response) {
                    delete inserted.response.channel;
                }
                test.addLog('Push request: ', inserted);
                waitForNotification(10000, function (notification) {
                    if (notification) {
                        test.addLog('Notification received: ', notification);
                        if (notification.type !== expectedNotificationType) {
                            test.addLog('Error, notification type (', notification.type, ') is not the expected (', expectedNotificationType, ')');
                            done(false);
                        } else {
                            if (notification.content !== expectedPushPayload) {
                                test.addLog('Error, notification payload (', notification.content, ') is not the expected (', expectedPushPayload, ')');
                                done(false);
                            } else {
                                test.addLog('Push notification received successfully');
                                done(true);
                            }
                        }
                    } else {
                        test.addLog('Error, push not received on the allowed timeout');
                        done(false);
                    }
                });
            }, function (err) {
                test.addLog('Error requesting push notification: ', err);
                done(false);
            });
        });
    }

    return {
        name: 'Push',
        tests: tests
    };
}

zumo.tests.push = definePushTestsNamespace();
