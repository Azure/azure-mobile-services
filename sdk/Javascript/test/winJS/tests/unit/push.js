$testGroup('push',
    $test('register request URL includes installation ID')
        .checkAsync(function () {
            return client(function (req) {
                $assert.areNotEqual(req.url.indexOf(WindowsAzure.MobileServiceClient._applicationInstallationId), -1);
            }).push.register('channelUri');
        }),
    $test('unregister request URL includes installation ID')
        .checkAsync(function () {
            return client(function (req) {
                $assert.areNotEqual(req.url.indexOf(WindowsAzure.MobileServiceClient._applicationInstallationId), -1);
            }).push.unregister('channelUri');
        }),
    $test('register sets data properties')
        .checkAsync(function () {
            return client(function (req) {
                var data = JSON.parse(req.data);
                $assert.areEqual(data.installationId, WindowsAzure.MobileServiceClient._applicationInstallationId);
                $assert.areEqual(data.pushChannel, 'channelUri');
                $assert.areEqual(data.platform, 'wns');
                $assert.areEqual(data.templates, 'templates');
                $assert.areEqual(data.secondaryTiles, 'secondaryTiles');
            }).push.register('channelUri', 'templates', 'secondaryTiles');
        })
);

function client(tests) {
    return new WindowsAzure.MobileServiceClient("http://www.test.com", "http://www.gateway.com/", "applicationKey")
        .withFilter(function (req, next, callback) {
            tests(req);
            callback(null, { status: 200 });
        });
}
