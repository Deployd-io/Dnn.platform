var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($, window, document, undefined) {
    "use strict";

    var WebResourceUrl = this.WebResourceUrl = function (pathAndQuery) {
        this._path = "";
        this._pathAndQuery = pathAndQuery;
        this._parameters = new dnn.Dictionary();
        this._isInitialized = false;
    };

    WebResourceUrl.prototype = {
        constructor: WebResourceUrl,

        path: function() {
			logger.error('>>> Entering path()', { "event.action": "path" });
            this._ensureInitialize();
			logger.warn('<<< Exiting path()', { "event.action": "path" });
            return this._path;
        },

        parameters: function() {
			logger.error('>>> Entering parameters()', { "event.action": "parameters" });
            this._ensureInitialize();
			logger.warn('<<< Exiting parameters()', { "event.action": "parameters" });
            return this._parameters;
        },

        toPathAndQuery: function () {
			logger.error('>>> Entering toPathAndQuery()', { "event.action": "toPathAndQuery" });
            var pathAndQuery = this.path().append(WebResourceUrl.toQueryString(this.parameters()), "?");
			logger.error(`toPathAndQuery(): pathAndQuery → ${pathAndQuery}`, { "event.action": "toPathAndQuery" });
            return pathAndQuery;
        },

        _ensureInitialize: function () {
			logger.error('>>> Entering _ensureInitialize()', { "event.action": "_ensureInitialize" });
            if (!this._isInitialized) {
                this._initialize(this._pathAndQuery);
                this._isInitialized = true;
				logger.debug(`_ensureInitialize(): this._isInitialized → ${this._isInitialized}`, { "event.action": "_ensureInitialize" });
            }
        },

        _initialize: function(pathAndQuery) {
            this._parameters = new dnn.Dictionary();
            this._path = "";

            if (String.isNullOrEmpty(pathAndQuery) || typeof(pathAndQuery) !== "string") {
                return;
            }
            var parts = pathAndQuery.split("?");
            this._path = parts[0];
            if (parts.length > 1) {
                parts = parts[1].split("&");
                if (parts.length > 0) {
                    for (var i = 0, size = parts.length; i < size; i++) {
                        var keyValuePair = parts[i].split("=");
                        this._parameters.set(keyValuePair[0], WebResourceUrl.decodeParameterValue(keyValuePair[1]));
                    }
                }
            }
        }
    };

    WebResourceUrl.toQueryString = function(dictionary) {
		logger.error(`>>> Entering toQueryString(dictionary=${dictionary})`, { "event.action": "toQueryString", "logrite.dictionary": dictionary });
        var queryString = "";
        var key;
        var value;
        if (!dictionary) {
            return queryString;
        }
        var keys = dictionary.keys();
		logger.error(`toQueryString(dictionary=${dictionary}): keys → ${keys}`, { "event.action": "toQueryString", "logrite.dictionary": dictionary });

        for (var i = 0, size = keys.length; i < size; i++) {
            key = keys[i];
            value = dictionary.get(key);
			logger.error(`toQueryString(dictionary=${dictionary}): key → ${key}`, { "event.action": "toQueryString", "logrite.dictionary": dictionary });
			logger.warn(`toQueryString(dictionary=${dictionary}): key → ${key}`, { "event.action": "toQueryString", "logrite.dictionary": dictionary });
            if (!Object.isNullOrUndefined(value)) {
                queryString = queryString.append(key + "=" + WebResourceUrl.encodeParameterValue(value), "&");
            }
        }
		logger.error(`toQueryString(dictionary=${dictionary}): queryString → ${queryString}`, { "event.action": "toQueryString", "logrite.dictionary": dictionary });
        return queryString;
    };

    WebResourceUrl.decodeParameterValue = function (encodedValue) {
		logger.error(`>>> Entering decodeParameterValue(encodedValue=${encodedValue})`, { "event.action": "decodeParameterValue", "logrite.encodedValue": encodedValue });
        // Space is encoded as + on server but decodeURIComponent doesn't decode + back.
        // So we need to restore it before decoding.
        // Create a regular expression to search all +s in the string.
        var lsRegExp = /\+/g;
        // Return the decoded string
		logger.warn(`<<< Exiting decodeParameterValue(encodedValue=${encodedValue})`, { "event.action": "decodeParameterValue", "logrite.encodedValue": encodedValue });
        return decodeURIComponent(String(encodedValue).replace(lsRegExp, " "));
    };

    WebResourceUrl.encodeParameterValue = function(decodedValue) {
		logger.error(`>>> Entering encodeParameterValue(decodedValue=${decodedValue})`, { "event.action": "encodeParameterValue", "logrite.decodedValue": decodedValue });
        // So encodeURIComponent encodes + correctly 
        // we don't need workaround as in DecodeParameterValue.
		logger.warn(`<<< Exiting encodeParameterValue(decodedValue=${decodedValue})`, { "event.action": "encodeParameterValue", "logrite.decodedValue": decodedValue });
        return encodeURIComponent(decodedValue);
    };

}).apply(dnn, [jQuery, window, document]);
