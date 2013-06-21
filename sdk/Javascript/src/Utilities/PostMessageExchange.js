// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// window.postMessage does not have a concept of responses, so this class associates messages
// with IDs so that we can identify which message a reply refers to.

var Promises = require('Promises'),
    messageTimeoutDuration = 5 * 60 * 1000; // If no reply after 5 mins, surely there will be no reply

function PostMessageExchange() {
    var self = this;
    self._lastMessageId = 0;
    self._hasListener = false;
    self._pendingMessages = {};
}

PostMessageExchange.prototype.request = function (targetWindow, messageData, origin) {
    /// <summary>
    /// Issues a request to the target window via postMessage
    /// </summary>
    /// <param name="targetWindow" type="Object">
    /// The window object (on an actual window, or iframe) to send the request to
    /// </param>
    /// <param name="messageData" type="Object">
    /// A JSON-serializable object to pass to the target
    /// </param>
    /// <param name="origin" type="String">
    /// The expected origin (e.g., "http://example.com:81") of the recipient window.
    /// If at runtime the origin does not match, the request will not be issued.
    /// </param>
    /// <returns type="Object">
    /// A promise that completes once the target window sends back a reply, with
    /// value equal to that reply.
    /// </returns>

    var self = this,
        messageId = ++self._lastMessageId,
        envelope = { messageId: messageId, contents: messageData };

    self._ensureHasListener();

    return new Promises.Promise(function (complete, error) {
        // Track callbacks and origin data so we can complete the promise only for valid replies
        self._pendingMessages[messageId] = {
            messageId: messageId,
            complete: complete,
            error: error,
            targetWindow: targetWindow,
            origin: origin
        };

        // Don't want to leak memory, so if there's no reply, forget about it eventually
        self._pendingMessages[messageId].timeoutId = global.setTimeout(function () {
            var pendingMessage = self._pendingMessages[messageId];
            if (pendingMessage) {
                delete self._pendingMessages[messageId];
                pendingMessage.error({ status: 0, statusText: "Timeout", responseText: null });
            }
        }, messageTimeoutDuration);

        targetWindow.postMessage(JSON.stringify(envelope), origin);
    });
};

PostMessageExchange.prototype._ensureHasListener = function () {
    if (this._hasListener) {
        return;
    }
    this._hasListener = true;

    var self = this,
        boundHandleMessage = function () {
            self._handleMessage.apply(self, arguments);
        };

    if (window.addEventListener) {
        window.addEventListener('message', boundHandleMessage, false);
    } else {
        // For IE8
        window.attachEvent('onmessage', boundHandleMessage);
    }
};

PostMessageExchange.prototype._handleMessage = function (evt) {
    var envelope = this._tryDeserializeMessage(evt.data),
        messageId = envelope && envelope.messageId,
        pendingMessage = messageId && this._pendingMessages[messageId],
        isValidReply = pendingMessage && pendingMessage.targetWindow === evt.source &&
                       pendingMessage.origin === getOriginRoot(evt.origin);
    
    if (isValidReply) {
        global.clearTimeout(pendingMessage.timeoutId); // No point holding this in memory until the timeout expires
        delete this._pendingMessages[messageId];
        pendingMessage.complete(envelope.contents);
    }
};

PostMessageExchange.prototype._tryDeserializeMessage = function (messageString) {
    if (!messageString || typeof messageString !== 'string') {
        return null;
    }

    try {
        return JSON.parse(messageString);
    } catch (ex) {
        // It's not JSON, so it's not a message for us. Ignore it.
        return null;
    }
};

function getOriginRoot(url) {
    // Returns the proto/host/port part of a URL, i.e., the part that defines the access boundary
    // for same-origin policy. This is of the form "protocol://host:port", where ":port" is omitted
    // if it is the default port for that protocol.
    var parsedUrl = parseUrl(url),
        portString = parsedUrl.port ? parsedUrl.port.toString() : null,
        isDefaultPort = (parsedUrl.protocol === 'http:' && portString === '80') ||
                        (parsedUrl.protocol === 'https:' && portString === '443'),
        portSuffix = (portString && !isDefaultPort) ? ':' + portString : '';
    return parsedUrl.protocol + '//' + parsedUrl.hostname + portSuffix;
}

function parseUrl(url) {
    // https://gist.github.com/2428561 - works on IE8+. Could switch to a more manual, less magic
    // parser in the future if we need to support IE < 8.
    var elem = global.document.createElement('a');
    elem.href = url;
    return elem;
}

exports.instance = new PostMessageExchange();
exports.getOriginRoot = getOriginRoot;