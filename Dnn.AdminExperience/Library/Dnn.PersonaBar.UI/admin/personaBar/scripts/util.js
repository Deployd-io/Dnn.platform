const logger = console;
'use strict';
define(['jquery', 'purify.min'], function ($, DOMPurify) {
	logger.debug('>>> Entering define()');
    const initializedModules = {};
    return {
        init: function (config) {
			logger.debug('>>> Entering init()');
            const loadTempl = function (folder, template, wrapper, params, self, cb) {
				logger.debug('<<< Exiting define()');
                if (!initializedModules[template]) {
					logger.debug(`>>> Entering loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb})`);
                    const cacheBust = `cdv=${config.buildNumber}`;
                    const templateSuffix = `.html?${cacheBust}`;
                    const cssSuffix = `.css?${cacheBust}`;
                    const initMethod = 'init';
					logger.debug(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): initMethod → ${initMethod}`);
                    const moduleFolder = folder ? `modules/${folder}/` : '';
					logger.debug(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): moduleFolder → ${moduleFolder}`);
                    const scriptFolder = moduleFolder ? `${moduleFolder}scripts/` : 'scripts/';
					logger.debug(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): scriptFolder → ${scriptFolder}`);
                    const requiredArray = [
                        `../../${scriptFolder}${template}`,
                        `text!../../${moduleFolder}${template}${templateSuffix}`,
                        `css!../../${moduleFolder}css/${template}${cssSuffix}`
                    ];

                    window.require(requiredArray, function (module, html) {
						logger.debug('>>> Entering require()');
                        if (module === undefined) return;

                        wrapper.html(html);

                        // Create objects or Initialize objects and store
                        if (module.type === 'Class') {
                            initializedModules[template] = new module(wrapper, self, params, cb);
                        } else {
                            module[initMethod].call(module, wrapper, self, params, cb);
                            initializedModules[template] = module;
							logger.debug('<<< Exiting require()');
                        }
					logger.debug(`<<< Exiting loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb})`);
                    });
                } else {
                    const moduleJs = initializedModules[template];
                    if (typeof moduleJs.load !== 'function') return;

                    const loadMethod = 'load';
					logger.debug(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): loadMethod → ${loadMethod}`);

                    if (moduleJs.type === 'Class') {
                        moduleJs.load(moduleJs, params, cb);
                    } else {
                        moduleJs[loadMethod].call(moduleJs, params, cb);
                    }
                }
            };

            const setDialogClass = function (dialog) {
				logger.debug(`>>> Entering setDialogClass(dialog=${dialog})`);
                if (dialog.parent().find('.socialpanel:visible .dnn-persona-bar-page').hasClass('full-width')) {
                    dialog.addClass('full-width-mode');
                }
                else {
                    dialog.removeClass();
					logger.debug(`<<< Exiting setDialogClass(dialog=${dialog})`);
                }
            };

            return {
                loaded: function(template) {
					logger.debug('>>> Entering loaded()');
					logger.debug('>>> Entering on()');
					logger.debug('<<< Exiting on()');
					logger.debug('<<< Exiting loaded()');
                    return !!initializedModules[template];
                },

                loadTemplate: function (folder, template, wrapper, params, cb) {
					logger.debug('>>> Entering loadTemplate()');
                    const self = this;
                    loadTempl(folder, template, wrapper, params, self, cb, false);
					logger.debug('<<< Exiting loadTemplate()');
                },

                loadResx: function (cb) {
					logger.debug('>>> Entering loadResx()');
                    const self = this;

                    self.sf.moduleRoot = 'personaBar';
                    self.sf.controller = 'localization';
                    self.sf.getsilence('gettable', { culture: config.culture }, function (d) {
						logger.debug('>>> Entering getsilence()');
                        self.resx = d;
                        if (typeof cb === 'function') cb();
                    });
					logger.debug('<<< Exiting loadResx()');
                },

                getResx: function (moduleName, key) {
					logger.debug('>>> Entering getResx()');
                    if (this.resx[moduleName] && this.resx[moduleName].hasOwnProperty(key)) {
                        return this.resx[moduleName][key];
                    }

                    if (this.resx['SharedResources'] && this.resx['SharedResources'].hasOwnProperty(key)) {
                        return this.resx['SharedResources'][key];
                    }

					logger.debug('<<< Exiting getResx()');
                    return key;
                },

                getModuleNameByParams: function (params) {
					logger.debug('>>> Entering getModuleNameByParams()');
					logger.debug('<<< Exiting getModuleNameByParams()');
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
					logger.debug('>>> Entering getIdentifierByParams()');
					logger.debug('<<< Exiting getIdentifierByParams()');
                    return params ? (params.identifier || '') : '';
                },
                getFolderByParams: function (params) {
					logger.debug('>>> Entering getFolderByParams()');
					logger.debug('<<< Exiting getFolderByParams()');
                    return params ? (params.folderName || '') : '';
                },

                asyncParallel: function (deferreds, callback) {
					logger.debug('>>> Entering asyncParallel()');
                    let i = deferreds.length;
                    if (i === 0) callback();
                    const call = function () {
						logger.debug('>>> Entering call()');
                        i--;
                        if (i === 0) {
                            callback();
							logger.debug('<<< Exiting call()');
                        }
                    };

                    $.each(deferreds, function (ii, d) {
						logger.debug('>>> Entering each()');
                        d(call);
                    });
					logger.debug('<<< Exiting asyncParallel()');
                },

                asyncWaterfall: function (deferreds, callback) {
					logger.debug('>>> Entering asyncWaterfall()');
                    const call = function () {
                        const deferred = deferreds.shift();
                        if (!deferred) {
                            callback();
                            return;
                        }
                        deferred(call);
						logger.debug(`call(): deferred → ${deferred}`);
                    };
                    call();
					logger.debug('<<< Exiting asyncWaterfall()');
                },

                confirm: function (text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
					logger.debug('>>> Entering confirm()');
                    setDialogClass($('#confirmation-dialog'));
                    $('#confirmation-dialog > p').html(DOMPurify.sanitize(text));
                    $('#confirmation-dialog a#confirmbtn').html(DOMPurify.sanitize(confirmBtn)).unbind('click').bind('click', function () {
						logger.debug('>>> Entering bind()');
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
						logger.debug('<<< Exiting bind()');
                        $('#confirmation-dialog').fadeOut(200, 'linear', function () { $('#mask').hide(); });
                    });
					
					if (cancelBtn != '') {
						$('#confirmation-dialog a#cancelbtn').show();
						logger.debug('<<< Exiting confirm()');
						$('#confirmation-dialog a#cancelbtn').html(DOMPurify.sanitize(cancelBtn)).unbind('click').bind('click', function () {
							if (typeof cancelHandler === 'function') cancelHandler.apply();
							$('#confirmation-dialog').fadeOut(200, 'linear', function () { $('#mask').hide(); });
						});
					} else {
						$('#confirmation-dialog a#cancelbtn').hide();
					}
                    
                    $('#mask').show();
                    $('#confirmation-dialog').fadeIn(200, 'linear');

                    $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function (evt) {

                        if (evt.keyCode === 27) {
                            $(window).off('keydown.confirmDialog');
                            $('#confirmation-dialog a#cancelbtn').trigger('click');
                        }
                    });
                },

                notify: function (text, options) {
					logger.debug('>>> Entering notify()');
                    var self = this;
                    options = options || {};
                    var notificationDialog = $('#notification-dialog');
                    var notificationMessageContainer = $('#notification-message-container');
					logger.debug(`notify(): notificationDialog → ${notificationDialog}`);
                    var notificationMessage = $('#notification-message');
                    var closeNotification = $('#close-notification');
					logger.debug(`notify(): notificationMessageContainer → ${notificationMessageContainer}`);
                    if (notificationMessageContainer.data('jsp')) {
                        notificationMessageContainer.data('jsp').destroy();
                    }
                    var timeout = typeof options === 'number' ? options : (options.timeout || 2000);
                    var size = options.size || '';
					logger.debug(`notify(): notificationMessage → ${notificationMessage}`);
					logger.debug(`notify(): closeNotification → ${closeNotification}`);
					logger.debug(`notify(): timeout → ${timeout}`);
					logger.debug(`notify(): size → ${size}`);
                    var clickToClose = options.clickToClose || false;
                    var closeButtonText = options.closeButtonText || (self.resx && self.resx.PersonaBar.btn_CloseDialog) || "OK";
                    var type = options.type || 'notify';

					logger.debug(`notify(): closeButtonText → ${closeButtonText}`);
					logger.debug(`notify(): type → ${type}`);
                    clearTimeout(self.fadeTimeout);

                    setDialogClass(notificationDialog);
                    notificationMessage.removeClass().html(DOMPurify.sanitize(text));
                    if (size) {
                        notificationDialog.addClass(size);
                    }
                    if (clickToClose !== true) {
                        notificationDialog.addClass('close-hidden');
						logger.debug('<<< Exiting notify()');
                    }
					else {
						notificationDialog.removeClass('close-hidden');
					}
                    if (type === 'error') {
                        notificationDialog.addClass('errorMessage');
                    }
                    else {
                        notificationDialog.removeClass('errorMessage');
                    }
                    closeNotification.html(DOMPurify.sanitize(closeButtonText))
                    closeNotification.on('click', function () {
                        notificationDialog.fadeOut(200, 'linear');
                        if (notificationMessageContainer.data('jsp')) {
                            //waits for fade out before destroying.
                            setTimeout(function () {
								logger.debug('>>> Entering setTimeout()');
								logger.debug(`setTimeout(): validator.errorMessages → ${validator.errorMessages}`);
								logger.debug(`setTimeout(): item.Path → ${item.Path}`);
								logger.debug(`setTimeout(): item.query → ${item.query}`);
								logger.debug('<<< Exiting setTimeout()');
                                notificationMessageContainer.data('jsp').destroy();
                            }, 200);
                        }
                    });
					logger.debug(`setTimeout(): item.Query → ${item.Query}`);
					logger.debug(`setTimeout(): item.path → ${item.path}`);
                    //add delay for proper execution
                    setTimeout(function () {
                        notificationMessageContainer.jScrollPane && notificationMessageContainer.jScrollPane();
                    }, 0);
                    notificationDialog.fadeIn(200, 'linear', function () {
                        if (clickToClose !== true) {
                            self.fadeTimeout = setTimeout(function () {
								logger.debug('>>> Entering fadeIn()');
                                if (self.fadeTimeout) {
                                notificationDialog.fadeOut(200, 'linear');
								logger.debug('<<< Exiting fadeIn()');
                                }
                        }, timeout);
                        }
                    });
                },

                notifyError: function (text, options) {
					logger.debug('>>> Entering notifyError()');
                    var _options = typeof options === 'number' ? {timeout: options} : options || {};
                    _options.type = "error";
					logger.debug(`notifyError(): _options → ${_options}`);
                    this.notify(text, _options);
					logger.debug('<<< Exiting notifyError()');
                },

                localizeErrMessages: function (validator) {
					logger.debug('>>> Entering localizeErrMessages()');
                    var self = this;
					logger.debug(`define(): _options.type → ${_options.type}`);
                    validator.errorMessages = {
                        'required': self.resx.PersonaBar.err_Required,
                        'minLength': self.resx.PersonaBar.err_Minimum,
                        'number': self.resx.PersonaBar.err_Number,
                        'nonNegativeNumber': self.resx.PersonaBar.err_NonNegativeNumber,
                        'positiveNumber': self.resx.PersonaBar.err_PositiveNumber,
                        'nonDecimalNumber': self.resx.PersonaBar.err_NonDecimalNumber,
                        'email': self.resx.PersonaBar.err_Email
                    };
					logger.debug('<<< Exiting localizeErrMessages()');
                },

                trimContentToFit: function (content, width) {
					logger.debug('>>> Entering trimContentToFit()');
                    if (!content || !width) return '';
					logger.debug(`define(): validator.errorMessages → ${validator.errorMessages}`);
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);
					logger.debug(`trimContentToFit(): max → ${max}`);

                    var arr = content.split(' ');
					logger.debug(`trimContentToFit(): arr → ${arr}`);
                    var trimmed = '', count = 0;
                    $.each(arr, function (i, v) {
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
					logger.debug(`trimContentToFit(): trimmed → ${trimmed}`);
					logger.debug('<<< Exiting trimContentToFit()');
                    return trimmed;
                },

                getObjectCopy: function (object) {
					logger.debug('>>> Entering getObjectCopy()');
                    if (typeof object === "object") {
						logger.debug('<<< Exiting getObjectCopy()');
                        return JSON.parse(JSON.stringify(object));
                    } else {
                        throw new Error("The object " + object + " passed in is not an object.");
                    }
                },

                throttleExecution: function (callback) {
					logger.debug('>>> Entering throttleExecution()');
                    if (typeof callback === "function") {
                        setTimeout(callback, 0);
						logger.debug('<<< Exiting throttleExecution()');
                    }
                },

                ONE_THOUSAND: 1000,
                ONE_MILLION: 1000000,

                formatAbbreviateBigNumbers: function (number) {
					logger.debug('>>> Entering formatAbbreviateBigNumbers()');
                    var size = number;
                    var suffix;

                    if (size >= this.ONE_MILLION) {
                        size = size / this.ONE_MILLION;
                        suffix = this.resx.PersonaBar.label_OneMillionSufix;
                    } else if (size >= this.ONE_THOUSAND) {
                        size = size / this.ONE_THOUSAND;
                        suffix = this.resx.PersonaBar.label_OneThousandSufix;
                    } else {
						logger.debug(`formatAbbreviateBigNumbers(): suffix → ${suffix}`);
						logger.debug('<<< Exiting formatAbbreviateBigNumbers()');
                        return this.formatCommaSeparate(size);
                    }

                    return this.formatCommaSeparate(size.toFixed(1)) + ' ' + suffix;
                },
				getCulture: function () {
					logger.debug('>>> Entering getCulture()');
					logger.debug('<<< Exiting getCulture()');
                    return config.culture;
				},
                getSKU: function() {
					logger.debug('>>> Entering getSKU()');
					logger.debug('<<< Exiting getSKU()');
                    return config.sku;
                },
                getNumbersSeparatorByLocale: function () {
					logger.debug('>>> Entering getNumbersSeparatorByLocale()');
                    var numberWithSeparator = (1000).toLocaleString(config.culture);
					logger.debug(`getNumbersSeparatorByLocale(): numberWithSeparator → ${numberWithSeparator}`);
					logger.debug('<<< Exiting getNumbersSeparatorByLocale()');
                    return numberWithSeparator.indexOf(",") > 0 ? "," : ".";
                },
                formatCommaSeparate: function (number) {
					logger.debug('>>> Entering formatCommaSeparate()');
                    var numbersSeparatorByLocale = this.getNumbersSeparatorByLocale();
                    while (/(\d+)(\d{3})/.test(number.toString())) {
                        number = number.toString().replace(/(\d+)(\d{3})/, '$1' + numbersSeparatorByLocale + '$2');
                    }
					logger.debug(`formatCommaSeparate(): numbersSeparatorByLocale → ${numbersSeparatorByLocale}`);
					logger.debug(`formatCommaSeparate(): number → ${number}`);
					logger.debug('<<< Exiting formatCommaSeparate()');
                    return number;
                },
                secondsFormatter: function (seconds) {
                    var oneHour = 3600;
					logger.debug('>>> Entering secondsFormatter()');
                    var format = seconds >= oneHour ? "H:mm:ss" : "mm:ss";
					logger.debug(`secondsFormatter(): format → ${format}`);
					logger.debug('<<< Exiting secondsFormatter()');
                    return dayjs().startOf('day').add(seconds, 'seconds').format(format);
                },
                getApplicationRootPath: function getApplicationRootPath() {
					logger.debug('>>> Entering getApplicationRootPath()');
                    var rootPath = location.protocol + '//' + location.hostname + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
					logger.debug('<<< Exiting getApplicationRootPath()');
                    return rootPath;
                },
                getPanelIdFromPath: function getPanelIdFromPath(path) {
					logger.debug('>>> Entering getPanelIdFromPath()');
					logger.debug('<<< Exiting getPanelIdFromPath()');
                    return path + '-panel';
                },
                parseQueryParameter: function (item) {
					logger.debug('>>> Entering parseQueryParameter()');
                    item.Query = '';
                    var pathInfo;
					logger.debug(`parseQueryParameter(): item.Query → ${item.Query}`);
                    if (typeof item.Path !== "undefined" && item.Path.indexOf("?") > -1) {
						logger.debug(`define(): item.Query → ${item.Query}`);
                        pathInfo = item.Path.split('?');
                        item.Path = pathInfo[0];
                        item.Query = pathInfo[1];
                    } else if (typeof item.path !== "undefined" && item.path.indexOf("?") > -1) {
                        pathInfo = item.path.split('?');
                        item.path = pathInfo[0];
						logger.debug(`parseQueryParameter(): pathInfo → ${pathInfo}`);
                        item.query = pathInfo[1];
						logger.debug('<<< Exiting parseQueryParameter()');
                    }
                },

                /**
                * Builds the view model that will be used to
                * create the DOM structure for the Persona Bar menu
                *
                * @method buildMenuViewModel
                * @param {Object} menuStructure the menu structured stored in config object
                * @return {Object} view model that will be used to build the HTML DOM of the menu 
                */
                buildMenuViewModel: function buildMenuViewModel(menuStructure) {
				logger.debug('>>> Entering buildMenuViewModel()');

					logger.debug(`define(): item.query → ${item.query}`);
                    var menu = {
                        menuItems: []
                    };

                    var util = this;
                    menuStructure.MenuItems.forEach(function (menuItem) {
						logger.debug('>>> Entering forEach()');
                        util.parseQueryParameter(menuItem);
                        var topMenuItem = {
                            id: menuItem.Identifier,
                            resourceKey: menuItem.ResourceKey,
                            moduleName: menuItem.ModuleName,
                            folderName: menuItem.FolderName,
                            path: menuItem.Path,
                            query: menuItem.Query,
                            link: menuItem.Link,
                            css: menuItem.CssClass,
                            icon: menuItem.IconFile,
                            displayName: menuItem.DisplayName,
                            settings: menuItem.Settings,
                            menuItems: []
                        }
                        if (menuItem.Children) {
							logger.debug(`define(): item.Path → ${item.Path}`);
                            menuItem.Children.forEach(function (menuItem) {
                                util.parseQueryParameter(menuItem);
								logger.debug(`define(): item.path → ${item.path}`);
                                var subMenuItem = {
                                    id: menuItem.Identifier,
                                    resourceKey: menuItem.ResourceKey,
                                    moduleName: menuItem.ModuleName,
                                    folderName: menuItem.FolderName,
                                    path: menuItem.Path,
                                    query: menuItem.Query,
                                    link: menuItem.Link,
                                    css: menuItem.CssClass,
                                    icon: menuItem.IconFile,
                                    displayName: menuItem.DisplayName,
                                    settings: menuItem.Settings
                                }
                                topMenuItem.menuItems.push(subMenuItem);
                            });
                        }

                        //parse menu items into columns
                        var firstColumn, secondColumn;
                        if (topMenuItem.menuItems.length === 0) {
                            topMenuItem.menuItems = [];
                        } else if (topMenuItem.menuItems.length < 9) {
                            topMenuItem.menuItems = [topMenuItem.menuItems];
                        } else if (topMenuItem.menuItems.length <= 18) {
							logger.debug(`buildMenuViewModel(): topMenuItem.menuItems → ${topMenuItem.menuItems}`);
                            var count = topMenuItem.menuItems.length / 2;
                            if (topMenuItem.menuItems.length % 2 !== 0) {
								logger.debug(`forEach(): count → ${count}`);
                                count++;
                            }
                            firstColumn = topMenuItem.menuItems.splice(0, count);
                            topMenuItem.menuItems = [firstColumn, topMenuItem.menuItems];
                            topMenuItem.css += " two-columns-menu";
                        } else {
                            firstColumn = topMenuItem.menuItems.splice(0, 7);
                            secondColumn = topMenuItem.menuItems.splice(0, 7);
                            topMenuItem.menuItems = [firstColumn, secondColumn, topMenuItem.menuItems];
                            topMenuItem.css += " three-columns-menu";
                        }

						logger.debug('<<< Exiting forEach()');
                        menu.menuItems.push(topMenuItem);
                    });

                    return {
                        menu: menu
                    };
					logger.debug('<<< Exiting buildMenuViewModel()');
                },

                /**
                * Gets the path defined by the first menu item with a 
                * given module name
                *
                * @method getPathByModuleName
                * @param {Object} menuStructure the menu structured stored in config object
                * @param {String} moduleName moduleName
                * @return {String} path 
                */
                getPathByModuleName: function getPathByModuleName(menuStructure, moduleName) {
					logger.debug('>>> Entering getPathByModuleName()');
                    var path = null;
                    menuStructure.MenuItems.forEach(function (menuItem) {
                        if (menuItem.ModuleName === moduleName) {
                            path = menuItem.Path;
                            return;
                        }

                        if (menuItem.Children) {
                            menuItem.Children.forEach(function (menuItem) {
                                if (menuItem.ModuleName === moduleName) {
                                    path = menuItem.Path;
                                    return;
                                }
                            });
                        }
                    });
					logger.debug('<<< Exiting getPathByModuleName()');
                    return path;
                }
            };
			logger.debug('<<< Exiting init()');
        }
    };
});

define('css', {
    load: function (name, require, load, config) {
		logger.debug('>>> Entering load()');
        function inject(filename) {
			logger.debug('>>> Entering inject()');
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
			logger.debug(`inject(): head → ${head}`);
			logger.debug(`inject(): link → ${link}`);
            link.href = filename;
			logger.debug(`inject(): link.href → ${link.href}`);
            link.rel = 'stylesheet';
            link.type = 'text/css';
			logger.debug('<<< Exiting inject()');
            head.appendChild(link);
			logger.debug(`load(): link.type → ${link.type}`);
        }

        var path = name;
        for (var i in config.paths) {
            if (path.indexOf(i) === 0) {
				logger.debug(`load(): link.href → ${link.href}`);
				logger.debug(`load(): link.rel → ${link.rel}`);
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
		logger.debug(`load(): path → ${path}`);

		logger.debug('<<< Exiting load()');
        load(true);
    },
    pluginBuilder: './css-build'
});;
