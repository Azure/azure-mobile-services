$testGroup('push',
    $test('register makes PUT HTTP request with URL containing installation ID')
        .checkAsync(function () {
            return client(function (req) {
                $assert.areEqual(req.type, 'PUT');
                $assert.areNotEqual(req.url.indexOf(encodeURIComponent(WindowsAzure.MobileServiceClient._applicationInstallationId)), -1);
                $assert.areEqual(req.headers['ZUMO-API-VERSION'], '2.0.0');
            }).push.register('wns', 'channelUri');
        }),
    $test('unregister makes DELETE HTTP request with URL containing installation ID')
        .checkAsync(function () {
            return client(function (req) {
                $assert.areEqual(req.type, 'DELETE');
                $assert.areNotEqual(req.url.indexOf(encodeURIComponent(WindowsAzure.MobileServiceClient._applicationInstallationId)), -1);
            }).push.unregister('channelUri');
        }),
    $test('register sets data properties')
        .checkAsync(function () {
            var templates = { templateName: {} };
            return client(function (req) {
                var data = JSON.parse(req.data);
                $assert.areEqual(data.installationId, WindowsAzure.MobileServiceClient._applicationInstallationId);
                $assert.areEqual(data.pushChannel, 'channelUri');
                $assert.areEqual(data.platform, 'wns');
                $assert.areEqual(JSON.stringify(data.templates), JSON.stringify(templates));
                $assert.areEqual(JSON.stringify(data.secondaryTiles), JSON.stringify(templates));
                $assert.areEqual(req.headers['ZUMO-API-VERSION'], '2.0.0');
            }).push.register('wns', 'channelUri', templates, templates);
        }),
    $test('templates bodies are stringified if not already a string')
        .checkAsync(function () {
            var body = { text: 'test' };

            return client(function (req) {
                var data = JSON.parse(req.data);
                $assert.areEqual(data.templates.templateName.body, JSON.stringify(body));
                $assert.areEqual(req.headers['ZUMO-API-VERSION'], '2.0.0');
            }).push.register('wns', 'channelUri', { templateName: { body: body } }, 'secondaryTiles');
        })
);

function client(tests) {
    return new WindowsAzure.MobileServiceClient("http://www.test.com", "http://www.gateway.com/", "applicationKey")
        .withFilter(function (req, next, callback) {
            tests(req);
            callback(null, { status: 200 });
        });
}
