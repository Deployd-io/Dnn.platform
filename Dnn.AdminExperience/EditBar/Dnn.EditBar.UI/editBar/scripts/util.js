const logger = console;
'use strict';
define(['jquery', 'purify.min'], function ($, DOMPurify) {
	logger.debug('>>> Entering define()');
    return {
        init: function (config) {
			logger.debug(`>>> Entering init(config=${config})`);
			logger.debug('<<< Exiting define()');
            return {
                loadResx: function(cb) {
					logger.debug(`>>> Entering loadResx(cb=${cb})`);
                    var self = this;
					logger.debug(`<<< Exiting init(config=${config})`);
                    
					self.sf.moduleRoot = 'editBar/common';
                    self.sf.controller = 'localization';
					logger.debug(`<<< Exiting loadResx(cb=${cb})`);
                    self.sf.getsilence('gettable', { culture: config.culture }, function (d) {
						logger.debug('>>> Entering getsilence()');
                        self.resx = d;
						logger.debug('<<< Exiting getsilence()');
                        if (typeof cb === 'function') cb();
                    });
                },

                getResx: function (moduleName, key) {
					logger.debug(`>>> Entering getResx(moduleName=${moduleName},key=${key})`);
                    if (this.resx[moduleName] && this.resx[moduleName].hasOwnProperty(key)) {
						logger.debug(`define(): self.resx → ${self.resx}`);
						logger.debug(`init(config=${config}): self.resx → ${self.resx}`);
					logger.debug(`<<< Exiting getResx(moduleName=${moduleName},key=${key})`);
                        return this.resx[moduleName][key];
                    }

                    return key;
                },

                getModuleNameByParams: function(params) {
					logger.debug(`>>> Entering getModuleNameByParams(params=${params})`);
					logger.debug(`<<< Exiting getModuleNameByParams(params=${params})`);
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
					logger.debug(`>>> Entering getIdentifierByParams(params=${params})`);
					logger.debug(`<<< Exiting getIdentifierByParams(params=${params})`);
                    return params ? (params.identifier || '') : '';
                },

                asyncParallel: function(deferreds, callback) {
					logger.debug(`>>> Entering asyncParallel(deferreds=${deferreds},callback=${callback})`);
                    var i = deferreds.length;
                    if (i === 0) callback();
                    var call = function() {
						logger.debug('>>> Entering call()');
                        i--;
                        if (i === 0) {
                            callback();
							logger.debug('<<< Exiting call()');
                        }
                    };

                    $.each(deferreds, function(ii, d) {
						logger.debug(`>>> Entering each(deferreds=${deferreds})`);
						logger.debug(`<<< Exiting each(deferreds=${deferreds})`);
						logger.debug(`<<< Exiting each(arr=${arr})`);
                        d(call);
                    });
					logger.debug(`<<< Exiting asyncParallel(deferreds=${deferreds},callback=${callback})`);
                },

                asyncWaterfall: function(deferreds, callback) {
					logger.debug(`>>> Entering asyncWaterfall(deferreds=${deferreds},callback=${callback})`);
                    var call = function() {
                        var deferred = deferreds.shift();
                        if (!deferred) {
                            callback();
                            return;
                        }
                        deferred(call);
						logger.debug(`call(): deferred → ${deferred}`);
                    };
                    call();
					logger.debug(`<<< Exiting asyncWaterfall(deferreds=${deferreds},callback=${callback})`);
                },

                confirm: function(text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
					logger.debug(`>>> Entering confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
                    $('#confirmation-dialog > p').html(DOMPurify.sanitize(text));
                    $('#confirmation-dialog a#confirmbtn').html(DOMPurify.sanitize(confirmBtn)).unbind('click').bind('click', function() {
						logger.debug('>>> Entering bind()');
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
						logger.debug('<<< Exiting bind()');
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#confirmation-dialog a#cancelbtn').html(DOMPurify.sanitize(cancelBtn)).unbind('click').bind('click', function() {
                        if (typeof cancelHandler === 'function') cancelHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#mask').show();
                    $('#confirmation-dialog').fadeIn(200, 'linear');

					logger.debug(`<<< Exiting confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
                    $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {
					logger.debug(`>>> Entering on(window=${window})`);

                        if (evt.keyCode === 27) {
                            $(window).off('keydown.confirmDialog');
                            $('#confirmation-dialog a#cancelbtn').trigger('click');
							logger.debug(`<<< Exiting on(window=${window})`);
                        }
                    });
                },

                notify: function(text) {
					logger.debug(`>>> Entering notify(text=${text})`);
                    $('#notification-dialog > p').removeClass().html(DOMPurify.sanitize(text));
                    $('#notification-dialog').fadeIn(200, 'linear', function() {
						logger.debug('>>> Entering fadeIn()');
                        setTimeout(function() {
							logger.debug('>>> Entering setTimeout()');
							logger.debug('<<< Exiting setTimeout()');
                            $('#notification-dialog').fadeOut(200, 'linear');
						logger.debug('<<< Exiting fadeIn()');
                        }, 2000);
                    });
					logger.debug(`<<< Exiting notify(text=${text})`);
                },

                notifyError: function(text) {
                    $('#notification-dialog > p').removeClass().addClass('errorMessage').html(DOMPurify.sanitize(text));
                    $('#notification-dialog').fadeIn(200, 'linear', function () {
						logger.debug(`>>> Entering notifyError(text=${text})`);
                        setTimeout(function() {
                            $('#notification-dialog').fadeOut(200, 'linear');
						logger.debug(`<<< Exiting notifyError(text=${text})`);
                        }, 2000);
                    });
                },

                trimContentToFit: function(content, width) {
					logger.debug(`>>> Entering trimContentToFit(content=${content},width=${width})`);
                    if (!content || !width) return '';
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);

					logger.debug(`trimContentToFit(content=${content},width=${width}): max → ${max}`);
                    var arr = content.split(' ');
                    var trimmed = '', count = 0;
					logger.debug(`trimContentToFit(content=${content},width=${width}): arr → ${arr}`);
                    $.each(arr, function(i, v) {
						logger.debug(`<<< Exiting trimContentToFit(content=${content},width=${width})`);
                        count += v.length;
                        if (count < max) {
                            if (trimmed) trimmed += ' ';
                            trimmed += v;
                            count++;
                        } else {
                            trimmed += '...';
                            return false;
                        }
                    });
                    return trimmed;
                },

                getApplicationRootPath: function getApplicationRootPath() {
					logger.debug('>>> Entering getApplicationRootPath()');
                    var rootPath = location.protocol + '//' + location.host + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
					logger.debug('<<< Exiting getApplicationRootPath()');
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
			logger.debug(`>>> Entering inject(filename=${filename})`);
			var head = document.getElementsByTagName('head')[0];
			logger.debug(`inject(filename=${filename}): head → ${head}`);
			var link = document.createElement('link');
			logger.debug(`inject(filename=${filename}): link → ${link}`);
			link.href = filename;
			link.rel = 'stylesheet';
			logger.debug(`inject(filename=${filename}): link.href → ${link.href}`);
			logger.debug(`inject(filename=${filename}): link.rel → ${link.rel}`);
			link.type = 'text/css';
			logger.debug(`inject(filename=${filename}): link.type → ${link.type}`);
			head.appendChild(link);
			logger.debug(`<<< Exiting inject(filename=${filename})`);
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
