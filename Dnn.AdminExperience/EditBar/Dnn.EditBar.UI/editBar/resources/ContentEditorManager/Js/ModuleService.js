const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
	logger.debug('>>> Entering function()');
    ///dnnModuleService

    var dnnModuleService = dnn.dnnModuleService = function (options) {
        this.options = options;
        this.init();
    };

    dnnModuleService.prototype = {
        init: function () {
			logger.debug('>>> Entering init()');
            this.options = $.extend({}, dnnModuleService.defaultOptions, this.options);

			logger.debug(`function(): this.options → ${this.options}`);
            this._service = $.dnnSF(this.options.moduleId);
			logger.debug('<<< Exiting init()');
        },

        isLoading: function () {
			logger.debug('>>> Entering isLoading()');
			logger.debug(`function(): dnnModuleService.prototype → ${dnnModuleService.prototype}`);
			logger.debug('<<< Exiting isLoading()');
            return this._loading;
        },

        request: function (method, type, params, successCallback, errorCallback) {
			logger.debug(`>>> Entering request(method=${method},type=${type},params=${params},successCallback=${successCallback},errorCallback=${errorCallback})`);
            var handler = this;
            $.ajax({
                url: this._getServiceUrl() + method,
                type: type,
                data: params,
                async: this.options.async,
                beforeSend: $.proxy(this._beforeSend, this),
                complete: $.proxy(this._completeRequest, this),
                success: function (data) {
					logger.debug(`>>> Entering success(data=${data})`);
                    if (typeof successCallback == "function") {
                        successCallback(data);
						logger.debug(`<<< Exiting success(data=${data})`);
                    }
                },
                error: function (xhr) {
					logger.debug(`>>> Entering error(xhr=${xhr})`);
                    if (xhr && xhr.status == '401') {
                        var loginUrl = dnn.getVar('cem_loginurl');
                        if (typeof window.dnnModal != "undefined") {
							logger.debug(`error(xhr=${xhr}): loginUrl → ${loginUrl}`);
                            window.dnnModal.show(loginUrl + (loginUrl.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
							logger.debug(`function(options=${options}): this.options → ${this.options}`);
                        } else {
                            location.href = loginUrl;
							logger.debug(`function(): location.href → ${location.href}`);
                        }

						logger.debug(`request(method=${method},type=${type},params=${params},successCallback=${successCallback},errorCallback=${errorCallback}): location.href → ${location.href}`);
					logger.debug(`error(xhr=${xhr}): location.href → ${location.href}`);
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

						logger.debug(`<<< Exiting error(xhr=${xhr})`);
                        return;
                    }

                    $.dnnAlert({
                        title: 'Error',
                        text: 'Error occurred when request service \'' + method + '\'.'
                    });
                }
            });
			logger.debug(`<<< Exiting request(method=${method},type=${type},params=${params},successCallback=${successCallback},errorCallback=${errorCallback})`);
        },

        getTabId: function() {
			logger.debug('>>> Entering getTabId()');
			logger.debug(`function(): this._loading → ${this._loading}`);
			logger.debug(`function(): dnnModuleService.defaultOptions → ${dnnModuleService.defaultOptions}`);
			logger.debug(`<<< Exiting function(options=${options})`);
			logger.debug('<<< Exiting getTabId()');
            return this._service.getTabId();
        },

        _beforeSend: function (xhr) {
			logger.debug(`>>> Entering _beforeSend(xhr=${xhr})`);
            this._service.setModuleHeaders(xhr);
            this._loading = true;
			logger.debug(`<<< Exiting _beforeSend(xhr=${xhr})`);
        },

        _completeRequest: function (xhr, status) {
			logger.debug(`>>> Entering _completeRequest(xhr=${xhr},status=${status})`);
            this._loading = false;
			logger.debug(`_completeRequest(xhr=${xhr},status=${status}): this._loading → ${this._loading}`);
			logger.debug(`<<< Exiting _completeRequest(xhr=${xhr},status=${status})`);
        },

        _getServiceUrl: function () {
			logger.debug('>>> Entering _getServiceUrl()');
			logger.debug('<<< Exiting _getServiceUrl()');
            return this._service.getServiceRoot(this.options.service) + this.options.controller + '/';
        },
    };

    dnnModuleService.defaultOptions = {
        async: true
    };

	///dnnModuleService END
}(jQuery));
