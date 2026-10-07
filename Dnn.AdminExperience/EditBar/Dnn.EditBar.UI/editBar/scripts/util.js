var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery', 'purify.min'], function ($, DOMPurify) {
    return {
        init: function (config) {
			logger.error(`>>> Entering init(config=${config})`, { "event.action": "init", "logrite.config": config });
            return {
                loadResx: function(cb) {
					logger.error(`>>> Entering loadResx(cb=${cb})`, { "event.action": "loadResx", "logrite.cb": cb });
                    var self = this;
					logger.warn(`<<< Exiting init(config=${config})`, { "event.action": "init", "logrite.config": config });
                    
					self.sf.moduleRoot = 'editBar/common';
                    self.sf.controller = 'localization';
                    self.sf.getsilence('gettable', { culture: config.culture }, function (d) {
                        self.resx = d;
						logger.debug(`init(config=${config}): self.resx → ${self.resx}`, { "event.action": "init", "logrite.config": config });
                        if (typeof cb === 'function') cb();
                    });
					logger.debug(`loadResx(cb=${cb}): self.resx → ${self.resx}`, { "event.action": "loadResx", "logrite.cb": cb });
                },

                getResx: function (moduleName, key) {
					logger.error(`>>> Entering getResx(moduleName=${moduleName},key=${key})`, { "event.action": "getResx", "logrite.moduleName": moduleName, "logrite.key": key });
                    if (this.resx[moduleName] && this.resx[moduleName].hasOwnProperty(key)) {
                        return this.resx[moduleName][key];
                    }

					logger.warn(`<<< Exiting getResx(moduleName=${moduleName},key=${key})`, { "event.action": "getResx", "logrite.moduleName": moduleName, "logrite.key": key });
                    return key;
                },

                getModuleNameByParams: function(params) {
					logger.error(`>>> Entering getModuleNameByParams(params=${params})`, { "event.action": "getModuleNameByParams", "logrite.params": params });
					logger.warn(`<<< Exiting getModuleNameByParams(params=${params})`, { "event.action": "getModuleNameByParams", "logrite.params": params });
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
					logger.error(`>>> Entering getIdentifierByParams(params=${params})`, { "event.action": "getIdentifierByParams", "logrite.params": params });
					logger.warn(`<<< Exiting getIdentifierByParams(params=${params})`, { "event.action": "getIdentifierByParams", "logrite.params": params });
                    return params ? (params.identifier || '') : '';
                },

                asyncParallel: function(deferreds, callback) {
					logger.error(`>>> Entering asyncParallel(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncParallel", "logrite.deferreds": deferreds, "logrite.callback": callback });
                    var i = deferreds.length;
                    if (i === 0) callback();
                    var call = function() {
						logger.error('>>> Entering call()', { "event.action": "call" });
                        i--;
                        if (i === 0) {
                            callback();
							logger.warn('<<< Exiting call()', { "event.action": "call" });
                        }
                    };

                    $.each(deferreds, function(ii, d) {
                        d(call);
                    });
					logger.warn(`<<< Exiting asyncParallel(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncParallel", "logrite.deferreds": deferreds, "logrite.callback": callback });
                },

                asyncWaterfall: function(deferreds, callback) {
					logger.error(`>>> Entering asyncWaterfall(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncWaterfall", "logrite.deferreds": deferreds, "logrite.callback": callback });
                    var call = function() {
                        var deferred = deferreds.shift();
                        if (!deferred) {
                            callback();
                            return;
                        }
                        deferred(call);
						logger.error(`call(): deferred → ${deferred}`, { "event.action": "call" });
                    };
                    call();
					logger.warn(`<<< Exiting asyncWaterfall(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncWaterfall", "logrite.deferreds": deferreds, "logrite.callback": callback });
                },

                confirm: function(text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
					logger.error(`>>> Entering confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtn": confirmBtn, "logrite.cancelBtn": cancelBtn, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
                    $('#confirmation-dialog > p').html(DOMPurify.sanitize(text));
                    $('#confirmation-dialog a#confirmbtn').html(DOMPurify.sanitize(confirmBtn)).unbind('click').bind('click', function() {
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#confirmation-dialog a#cancelbtn').html(DOMPurify.sanitize(cancelBtn)).unbind('click').bind('click', function() {
                        if (typeof cancelHandler === 'function') cancelHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#mask').show();
                    $('#confirmation-dialog').fadeIn(200, 'linear');

                    $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {

                        if (evt.keyCode === 27) {
                            $(window).off('keydown.confirmDialog');
                            $('#confirmation-dialog a#cancelbtn').trigger('click');
							logger.warn(`<<< Exiting confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtn": confirmBtn, "logrite.cancelBtn": cancelBtn, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
                        }
                    });
                },

                notify: function(text) {
					logger.error(`>>> Entering notify(text=${text})`, { "event.action": "notify", "logrite.text": text });
                    $('#notification-dialog > p').removeClass().html(DOMPurify.sanitize(text));
                    $('#notification-dialog').fadeIn(200, 'linear', function() {
                        setTimeout(function() {
                            $('#notification-dialog').fadeOut(200, 'linear');
                        }, 2000);
                    });
					logger.warn(`<<< Exiting notify(text=${text})`, { "event.action": "notify", "logrite.text": text });
                },

                notifyError: function(text) {
					logger.error(`>>> Entering notifyError(text=${text})`, { "event.action": "notifyError", "logrite.text": text });
                    $('#notification-dialog > p').removeClass().addClass('errorMessage').html(DOMPurify.sanitize(text));
                    $('#notification-dialog').fadeIn(200, 'linear', function () {
                        setTimeout(function() {
                            $('#notification-dialog').fadeOut(200, 'linear');
                        }, 2000);
                    });
					logger.warn(`<<< Exiting notifyError(text=${text})`, { "event.action": "notifyError", "logrite.text": text });
                },

                trimContentToFit: function(content, width) {
					logger.error(`>>> Entering trimContentToFit(content=${content},width=${width})`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    if (!content || !width) return '';
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);
					logger.error(`trimContentToFit(content=${content},width=${width}): max → ${max}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });

                    var arr = content.split(' ');
					logger.error(`trimContentToFit(content=${content},width=${width}): arr → ${arr}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    var trimmed = '', count = 0;
					logger.error(`trimContentToFit(content=${content},width=${width}): trimmed → ${trimmed}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    $.each(arr, function(i, v) {
                        count += v.length;
                        if (count < max) {
                            if (trimmed) trimmed += ' ';
                            trimmed += v;
                            count++;
                        } else {
                            trimmed += '...';
							logger.warn(`<<< Exiting trimContentToFit(content=${content},width=${width})`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                            return false;
                        }
                    });
                    return trimmed;
                },

                getApplicationRootPath: function getApplicationRootPath() {
					logger.error('>>> Entering getApplicationRootPath()', { "event.action": "getApplicationRootPath" });
                    var rootPath = location.protocol + '//' + location.host + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
					logger.warn('<<< Exiting getApplicationRootPath()', { "event.action": "getApplicationRootPath" });
                    return rootPath;
                }
            };
        }
    };
});

define('css',{
    load: function (name, require, load, config) {
		function inject(filename)
		{
			logger.error(`>>> Entering inject(filename=${filename})`, { "event.action": "inject", "logrite.filename": filename });
			var head = document.getElementsByTagName('head')[0];
			var link = document.createElement('link');
			logger.error(`inject(filename=${filename}): head → ${head}`, { "event.action": "inject", "logrite.filename": filename });
			logger.error(`inject(filename=${filename}): link → ${link}`, { "event.action": "inject", "logrite.filename": filename });
			link.href = filename;
			logger.debug(`inject(filename=${filename}): link.href → ${link.href}`, { "event.action": "inject", "logrite.filename": filename });
			link.rel = 'stylesheet';
			logger.debug(`inject(filename=${filename}): link.rel → ${link.rel}`, { "event.action": "inject", "logrite.filename": filename });
			link.type = 'text/css';
			head.appendChild(link);
			logger.warn(`<<< Exiting inject(filename=${filename})`, { "event.action": "inject", "logrite.filename": filename });
		}

		var path = name;
        for (var i in config.paths) {
            if (path.indexOf(i) === 0) {
                path = path.replace(i, config.paths[i]);
                break;
            }
        }

        if (path.indexOf('://') === -1) {
            path = config.baseUrl + path;
        }

        if (typeof config.urlArgs === 'string') {
            path = path + '?' + config.urlArgs;
        }
        inject(path);

		load(true);
	},
	pluginBuilder: './css-build'
});;
