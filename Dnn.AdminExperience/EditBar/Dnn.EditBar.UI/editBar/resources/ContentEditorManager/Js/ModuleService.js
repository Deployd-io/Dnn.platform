var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    ///dnnModuleService

    var dnnModuleService = dnn.dnnModuleService = function (options) {
		logger.error(`>>> Entering dnnModuleService(options=${options})`, { "event.action": "dnnModuleService", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`dnnModuleService(options=${options}): this.options → ${this.options}`, { "event.action": "dnnModuleService", "logrite.options": options });
    };

    dnnModuleService.prototype = {
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, dnnModuleService.defaultOptions, this.options);

            this._service = $.dnnSF(this.options.moduleId);
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        isLoading: function () {
			logger.error('>>> Entering isLoading()', { "event.action": "isLoading" });
			logger.warn('<<< Exiting isLoading()', { "event.action": "isLoading" });
            return this._loading;
        },

        request: function (method, type, params, successCallback, errorCallback) {
			logger.error(`>>> Entering request(method=${method},type=${type},params=${params},successCallback=${successCallback},errorCallback=${errorCallback})`, { "event.action": "request", "logrite.method": method, "logrite.type": type, "logrite.params": params, "logrite.successCallback": successCallback, "logrite.errorCallback": errorCallback });
            var handler = this;
            $.ajax({
                url: this._getServiceUrl() + method,
                type: type,
                data: params,
                async: this.options.async,
                beforeSend: $.proxy(this._beforeSend, this),
                complete: $.proxy(this._completeRequest, this),
                success: function (data) {
					logger.error(`>>> Entering success(data=${data})`, { "event.action": "success", "logrite.data": data });
                    if (typeof successCallback == "function") {
                        successCallback(data);
						logger.warn(`<<< Exiting success(data=${data})`, { "event.action": "success", "logrite.data": data });
                    }
                },
                error: function (xhr) {
					logger.error(`>>> Entering error(xhr=${xhr})`, { "event.action": "error", "logrite.xhr": xhr });
                    if (xhr && xhr.status == '401') {
                        var loginUrl = dnn.getVar('cem_loginurl');
						logger.error(`error(xhr=${xhr}): loginUrl → ${loginUrl}`, { "event.action": "error", "logrite.xhr": xhr });
                        if (typeof window.dnnModal != "undefined") {
                            window.dnnModal.show(loginUrl + (loginUrl.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
                        } else {
                            location.href = loginUrl;
                        }

                        return;
                    }

                    if (typeof errorCallback == "function") {
                        errorCallback(xhr);
                        return;
                    }

                    if (xhr != null &&
                        xhr.responseJSON != null &&
                        xhr.responseJSON.message != null) {
                        $.dnnAlert({
                            title: 'Error',
                            text: xhr.responseJSON.message
                        });

						logger.warn(`<<< Exiting error(xhr=${xhr})`, { "event.action": "error", "logrite.xhr": xhr });
                        return;
                    }

                    $.dnnAlert({
                        title: 'Error',
                        text: 'Error occurred when request service \'' + method + '\'.'
                    });
                }
            });
			logger.warn(`<<< Exiting request(method=${method},type=${type},params=${params},successCallback=${successCallback},errorCallback=${errorCallback})`, { "event.action": "request", "logrite.method": method, "logrite.type": type, "logrite.params": params, "logrite.successCallback": successCallback, "logrite.errorCallback": errorCallback });
        },

        getTabId: function() {
			logger.error('>>> Entering getTabId()', { "event.action": "getTabId" });
			logger.warn('<<< Exiting getTabId()', { "event.action": "getTabId" });
            return this._service.getTabId();
        },

        _beforeSend: function (xhr) {
			logger.error(`>>> Entering _beforeSend(xhr=${xhr})`, { "event.action": "_beforeSend", "logrite.xhr": xhr });
            this._service.setModuleHeaders(xhr);
            this._loading = true;
			logger.debug(`_beforeSend(xhr=${xhr}): this._loading → ${this._loading}`, { "event.action": "_beforeSend", "logrite.xhr": xhr });
        },

        _completeRequest: function (xhr, status) {
			logger.error(`>>> Entering _completeRequest(xhr=${xhr},status=${status})`, { "event.action": "_completeRequest", "logrite.xhr": xhr, "logrite.status": status });
            this._loading = false;
			logger.debug(`_completeRequest(xhr=${xhr},status=${status}): this._loading → ${this._loading}`, { "event.action": "_completeRequest", "logrite.xhr": xhr, "logrite.status": status });
        },

        _getServiceUrl: function () {
			logger.error('>>> Entering _getServiceUrl()', { "event.action": "_getServiceUrl" });
			logger.warn('<<< Exiting _getServiceUrl()', { "event.action": "_getServiceUrl" });
            return this._service.getServiceRoot(this.options.service) + this.options.controller + '/';
        },
    };

    dnnModuleService.defaultOptions = {
        async: true
    };

	///dnnModuleService END
}(jQuery));
