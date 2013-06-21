exports.get = handleRequest;
exports.post = handleRequest;
exports.put = handleRequest;
exports.patch = handleRequest;
exports.del = handleRequest;

exports.handleRequest = handleRequest;
function handleRequest(req, res) {
    var format = req.query.format || 'json';
    var status = req.query.status || 200;
    var output = { method: req.method };
    for (var q in req.query) {
        if (q !== 'format' && q !== 'status' && req.query.hasOwnProperty(q)) {
            var val = req.query[q];
            if (typeof val === 'string') {
                if (!output.query) output.query = {};
                output.query[q] = req.query[q];
            }
        }
    }
    var reqHeaders = req.headers;
    for (var reqHeader in reqHeaders) {
        if (reqHeaders.hasOwnProperty(reqHeader) && reqHeader.indexOf('x-test-zumo-') === 0) {
            res.set(reqHeader, reqHeaders[reqHeader]);
        }
    }

    if (req.body) {
        output.body = req.body;
    }

    output.user = JSON.parse(JSON.stringify(req.user)); // remove functions

    switch (format) {
        case 'json':
            break; // nothing to do
        case 'xml':
            res.set('Content-Type', 'text/xml');
            output = objToXml(output);
            break;
        default:
            res.set('Content-Type', 'text/plain');
            output = JSON.stringify(output)
                         .replace(/{/g, '__{__')
                         .replace(/}/g, '__}__')
                         .replace(/\[/g, '__[__')
                         .replace(/\]/g, '__]__');
            break;
    }
    
    res.send(status, output);
    res.end();
}

function objToXml(obj) {
    return '<root>' + jsToXml(obj) + '</root>';
}

function jsToXml(value) {
    if (value === null) return 'null';
    var type = typeof value;
    var result = '';
    var i = 0;
    switch (type.toLowerCase()) {
        case 'string':
        case 'boolean':
        case 'number':
            return value.toString();
        case 'function':
        case 'object':
            if (Object.prototype.toString.call( value ) === '[object Array]') {
                result = result + '<array>';
                for (i = 0; i < value.length; i++) {
                    result = result + '<item>' + jsToXml(value[i]) + '</item>';
                }
                result = result + '</array>';
            } else {
                var k;
                var keys = [];
                for (k in value) {
                    if (value.hasOwnProperty(k)) {
                        if (typeof value[k] !== 'function') {
                            keys.push(k);
                        }
                    }
                }
                keys.sort();
                for (i = 0; i < keys.length; i++) {
                    k = keys[i];
                    result = result + '<' + k + '>' + jsToXml(value[k]) + '</' + k + '>';
                }
            }
    }
    return result;
}
