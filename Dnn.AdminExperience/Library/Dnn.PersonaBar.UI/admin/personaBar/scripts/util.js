const logger = console;
'use strict';
define(['jquery', 'purify.min'], function ($, DOMPurify) {
    const initializedModules = {};
    return {
        init: function (config) {
			logger.error(`>>> Entering init(config=${config})`, { "event.action": "init", "logrite.config": config });
            const loadTempl = function (folder, template, wrapper, params, self, cb) {
				logger.error(`>>> Entering loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb})`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                if (!initializedModules[template]) {
                    const cacheBust = `cdv=${config.buildNumber}`;
                    const templateSuffix = `.html?${cacheBust}`;
                    const cssSuffix = `.css?${cacheBust}`;
                    const initMethod = 'init';
					logger.error(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): initMethod → ${initMethod}`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                    const moduleFolder = folder ? `modules/${folder}/` : '';
					logger.error(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): moduleFolder → ${moduleFolder}`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                    const scriptFolder = moduleFolder ? `${moduleFolder}scripts/` : 'scripts/';
					logger.error(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): scriptFolder → ${scriptFolder}`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                    const requiredArray = [
                        `../../${scriptFolder}${template}`,
                        `text!../../${moduleFolder}${template}${templateSuffix}`,
                        `css!../../${moduleFolder}css/${template}${cssSuffix}`
                    ];

                    window.require(requiredArray, function (module, html) {
                        if (module === undefined) return;

                        wrapper.html(html);

                        // Create objects or Initialize objects and store
                        if (module.type === 'Class') {
                            initializedModules[template] = new module(wrapper, self, params, cb);
                        } else {
                            module[initMethod].call(module, wrapper, self, params, cb);
                            initializedModules[template] = module;
                        }
                    });
                } else {
                    const moduleJs = initializedModules[template];
                    if (typeof moduleJs.load !== 'function') return;

                    const loadMethod = 'load';
					logger.error(`loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb}): loadMethod → ${loadMethod}`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });

                    if (moduleJs.type === 'Class') {
                        moduleJs.load(moduleJs, params, cb);
                    } else {
                        moduleJs[loadMethod].call(moduleJs, params, cb);
						logger.warn(`<<< Exiting loadTempl(folder=${folder},template=${template},wrapper=${wrapper},params=${params},self=${self},cb=${cb})`, { "event.action": "loadTempl", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                    }
                }
            };

            const setDialogClass = function (dialog) {
				logger.error(`>>> Entering setDialogClass(dialog=${dialog})`, { "event.action": "setDialogClass", "logrite.dialog": dialog });
                if (dialog.parent().find('.socialpanel:visible .dnn-persona-bar-page').hasClass('full-width')) {
                    dialog.addClass('full-width-mode');
                }
                else {
                    dialog.removeClass();
					logger.warn(`<<< Exiting setDialogClass(dialog=${dialog})`, { "event.action": "setDialogClass", "logrite.dialog": dialog });
                }
            };

            return {
                loaded: function(template) {
					logger.error(`>>> Entering loaded(template=${template})`, { "event.action": "loaded", "logrite.template": template });
					logger.warn(`<<< Exiting init(config=${config})`, { "event.action": "init", "logrite.config": config });
                    return !!initializedModules[template];
                },

                loadTemplate: function (folder, template, wrapper, params, cb) {
					logger.error(`>>> Entering loadTemplate(folder=${folder},template=${template},wrapper=${wrapper},params=${params},cb=${cb})`, { "event.action": "loadTemplate", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                    const self = this;
                    loadTempl(folder, template, wrapper, params, self, cb, false);
					logger.warn(`<<< Exiting loadTemplate(folder=${folder},template=${template},wrapper=${wrapper},params=${params},cb=${cb})`, { "event.action": "loadTemplate", "logrite.folder": folder, "logrite.template": template, "logrite.wrapper": wrapper, "logrite.params": params, "logrite.cb": cb });
                },

                loadResx: function (cb) {
					logger.error(`>>> Entering loadResx(cb=${cb})`, { "event.action": "loadResx", "logrite.cb": cb });
                    const self = this;

                    self.sf.moduleRoot = 'personaBar';
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

                    if (this.resx['SharedResources'] && this.resx['SharedResources'].hasOwnProperty(key)) {
                        return this.resx['SharedResources'][key];
                    }

					logger.warn(`<<< Exiting getResx(moduleName=${moduleName},key=${key})`, { "event.action": "getResx", "logrite.moduleName": moduleName, "logrite.key": key });
                    return key;
                },

                getModuleNameByParams: function (params) {
					logger.error(`>>> Entering getModuleNameByParams(params=${params})`, { "event.action": "getModuleNameByParams", "logrite.params": params });
					logger.warn(`<<< Exiting getModuleNameByParams(params=${params})`, { "event.action": "getModuleNameByParams", "logrite.params": params });
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
					logger.error(`>>> Entering getIdentifierByParams(params=${params})`, { "event.action": "getIdentifierByParams", "logrite.params": params });
					logger.warn(`<<< Exiting getIdentifierByParams(params=${params})`, { "event.action": "getIdentifierByParams", "logrite.params": params });
                    return params ? (params.identifier || '') : '';
                },
                getFolderByParams: function (params) {
					logger.error(`>>> Entering getFolderByParams(params=${params})`, { "event.action": "getFolderByParams", "logrite.params": params });
					logger.warn(`<<< Exiting getFolderByParams(params=${params})`, { "event.action": "getFolderByParams", "logrite.params": params });
                    return params ? (params.folderName || '') : '';
                },

                asyncParallel: function (deferreds, callback) {
					logger.error(`>>> Entering asyncParallel(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncParallel", "logrite.deferreds": deferreds, "logrite.callback": callback });
                    let i = deferreds.length;
                    if (i === 0) callback();
                    const call = function () {
						logger.error('>>> Entering call()', { "event.action": "call" });
                        i--;
                        if (i === 0) {
                            callback();
							logger.warn('<<< Exiting call()', { "event.action": "call" });
                        }
                    };

                    $.each(deferreds, function (ii, d) {
                        d(call);
                    });
					logger.warn(`<<< Exiting asyncParallel(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncParallel", "logrite.deferreds": deferreds, "logrite.callback": callback });
                },

                asyncWaterfall: function (deferreds, callback) {
					logger.error(`>>> Entering asyncWaterfall(deferreds=${deferreds},callback=${callback})`, { "event.action": "asyncWaterfall", "logrite.deferreds": deferreds, "logrite.callback": callback });
                    const call = function () {
                        const deferred = deferreds.shift();
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

                confirm: function (text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
					logger.error(`>>> Entering confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtn": confirmBtn, "logrite.cancelBtn": cancelBtn, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
                    setDialogClass($('#confirmation-dialog'));
                    $('#confirmation-dialog > p').html(DOMPurify.sanitize(text));
                    $('#confirmation-dialog a#confirmbtn').html(DOMPurify.sanitize(confirmBtn)).unbind('click').bind('click', function () {
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function () { $('#mask').hide(); });
                    });
					
					if (cancelBtn != '') {
						$('#confirmation-dialog a#cancelbtn').show();
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
							logger.warn(`<<< Exiting confirm(text=${text},confirmBtn=${confirmBtn},cancelBtn=${cancelBtn},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtn": confirmBtn, "logrite.cancelBtn": cancelBtn, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
                        }
                    });
                },

                notify: function (text, options) {
					logger.error(`>>> Entering notify(text=${text},options=${options})`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var self = this;
                    options = options || {};
                    var notificationDialog = $('#notification-dialog');
					logger.error(`notify(text=${text},options=${options}): notificationDialog → ${notificationDialog}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var notificationMessageContainer = $('#notification-message-container');
					logger.error(`notify(text=${text},options=${options}): notificationMessageContainer → ${notificationMessageContainer}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var notificationMessage = $('#notification-message');
					logger.error(`notify(text=${text},options=${options}): notificationMessage → ${notificationMessage}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var closeNotification = $('#close-notification');
					logger.error(`notify(text=${text},options=${options}): closeNotification → ${closeNotification}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    if (notificationMessageContainer.data('jsp')) {
                        notificationMessageContainer.data('jsp').destroy();
                    }
                    var timeout = typeof options === 'number' ? options : (options.timeout || 2000);
					logger.error(`notify(text=${text},options=${options}): timeout → ${timeout}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var size = options.size || '';
					logger.error(`notify(text=${text},options=${options}): size → ${size}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var clickToClose = options.clickToClose || false;
                    var closeButtonText = options.closeButtonText || (self.resx && self.resx.PersonaBar.btn_CloseDialog) || "OK";
					logger.error(`notify(text=${text},options=${options}): closeButtonText → ${closeButtonText}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                    var type = options.type || 'notify';
					logger.error(`notify(text=${text},options=${options}): type → ${type}`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });

                    clearTimeout(self.fadeTimeout);

                    setDialogClass(notificationDialog);
                    notificationMessage.removeClass().html(DOMPurify.sanitize(text));
                    if (size) {
                        notificationDialog.addClass(size);
                    }
                    if (clickToClose !== true) {
                        notificationDialog.addClass('close-hidden');
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
							logger.warn(`<<< Exiting notify(text=${text},options=${options})`, { "event.action": "notify", "logrite.text": text, "logrite.options": options });
                            //waits for fade out before destroying.
                            setTimeout(function () {
                                notificationMessageContainer.data('jsp').destroy();
                            }, 200);
                        }
                    });
                    //add delay for proper execution
                    setTimeout(function () {
                        notificationMessageContainer.jScrollPane && notificationMessageContainer.jScrollPane();
                    }, 0);
                    notificationDialog.fadeIn(200, 'linear', function () {
                        if (clickToClose !== true) {
                            self.fadeTimeout = setTimeout(function () {
                                if (self.fadeTimeout) {
                                notificationDialog.fadeOut(200, 'linear');
                                }
                        }, timeout);
                        }
                    });
                },

                notifyError: function (text, options) {
					logger.error(`>>> Entering notifyError(text=${text},options=${options})`, { "event.action": "notifyError", "logrite.text": text, "logrite.options": options });
                    var _options = typeof options === 'number' ? {timeout: options} : options || {};
                    _options.type = "error";
					logger.error(`notifyError(text=${text},options=${options}): _options → ${_options}`, { "event.action": "notifyError", "logrite.text": text, "logrite.options": options });
                    this.notify(text, _options);
					logger.warn(`<<< Exiting notifyError(text=${text},options=${options})`, { "event.action": "notifyError", "logrite.text": text, "logrite.options": options });
                },

                localizeErrMessages: function (validator) {
					logger.error(`>>> Entering localizeErrMessages(validator=${validator})`, { "event.action": "localizeErrMessages", "logrite.validator": validator });
                    var self = this;
                    validator.errorMessages = {
                        'required': self.resx.PersonaBar.err_Required,
                        'minLength': self.resx.PersonaBar.err_Minimum,
                        'number': self.resx.PersonaBar.err_Number,
                        'nonNegativeNumber': self.resx.PersonaBar.err_NonNegativeNumber,
                        'positiveNumber': self.resx.PersonaBar.err_PositiveNumber,
                        'nonDecimalNumber': self.resx.PersonaBar.err_NonDecimalNumber,
                        'email': self.resx.PersonaBar.err_Email
                    };
					logger.warn(`<<< Exiting localizeErrMessages(validator=${validator})`, { "event.action": "localizeErrMessages", "logrite.validator": validator });
                },

                trimContentToFit: function (content, width) {
					logger.error(`>>> Entering trimContentToFit(content=${content},width=${width})`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    if (!content || !width) return '';
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);
					logger.error(`trimContentToFit(content=${content},width=${width}): max → ${max}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });

                    var arr = content.split(' ');
					logger.error(`trimContentToFit(content=${content},width=${width}): arr → ${arr}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    var trimmed = '', count = 0;
					logger.error(`trimContentToFit(content=${content},width=${width}): trimmed → ${trimmed}`, { "event.action": "trimContentToFit", "logrite.content": content, "logrite.width": width });
                    $.each(arr, function (i, v) {
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

                getObjectCopy: function (object) {
					logger.error(`>>> Entering getObjectCopy(object=${object})`, { "event.action": "getObjectCopy", "logrite.object": object });
                    if (typeof object === "object") {
                        return JSON.parse(JSON.stringify(object));
                    } else {
						logger.warn(`<<< Exiting getObjectCopy(object=${object})`, { "event.action": "getObjectCopy", "logrite.object": object });
                        throw new Error("The object " + object + " passed in is not an object.");
                    }
                },

                throttleExecution: function (callback) {
					logger.error(`>>> Entering throttleExecution(callback=${callback})`, { "event.action": "throttleExecution", "logrite.callback": callback });
                    if (typeof callback === "function") {
                        setTimeout(callback, 0);
						logger.warn(`<<< Exiting throttleExecution(callback=${callback})`, { "event.action": "throttleExecution", "logrite.callback": callback });
                    }
                },

                ONE_THOUSAND: 1000,
                ONE_MILLION: 1000000,

                formatAbbreviateBigNumbers: function (number) {
					logger.error(`>>> Entering formatAbbreviateBigNumbers(number=${number})`, { "event.action": "formatAbbreviateBigNumbers", "logrite.number": number });
                    var size = number;
                    var suffix;

                    if (size >= this.ONE_MILLION) {
                        size = size / this.ONE_MILLION;
                        suffix = this.resx.PersonaBar.label_OneMillionSufix;
                    } else if (size >= this.ONE_THOUSAND) {
                        size = size / this.ONE_THOUSAND;
                        suffix = this.resx.PersonaBar.label_OneThousandSufix;
                    } else {
						logger.debug(`formatAbbreviateBigNumbers(number=${number}): suffix → ${suffix}`, { "event.action": "formatAbbreviateBigNumbers", "logrite.number": number });
					logger.warn(`<<< Exiting formatAbbreviateBigNumbers(number=${number})`, { "event.action": "formatAbbreviateBigNumbers", "logrite.number": number });
                        return this.formatCommaSeparate(size);
                    }

                    return this.formatCommaSeparate(size.toFixed(1)) + ' ' + suffix;
                },
				getCulture: function () {
					logger.error('>>> Entering getCulture()', { "event.action": "getCulture" });
					logger.warn('<<< Exiting getCulture()', { "event.action": "getCulture" });
                    return config.culture;
				},
                getSKU: function() {
					logger.error('>>> Entering getSKU()', { "event.action": "getSKU" });
					logger.warn('<<< Exiting getSKU()', { "event.action": "getSKU" });
                    return config.sku;
                },
                getNumbersSeparatorByLocale: function () {
					logger.error('>>> Entering getNumbersSeparatorByLocale()', { "event.action": "getNumbersSeparatorByLocale" });
                    var numberWithSeparator = (1000).toLocaleString(config.culture);
					logger.error(`getNumbersSeparatorByLocale(): numberWithSeparator → ${numberWithSeparator}`, { "event.action": "getNumbersSeparatorByLocale" });
                    return numberWithSeparator.indexOf(",") > 0 ? "," : ".";
                },
                formatCommaSeparate: function (number) {
					logger.error(`>>> Entering formatCommaSeparate(number=${number})`, { "event.action": "formatCommaSeparate", "logrite.number": number });
                    var numbersSeparatorByLocale = this.getNumbersSeparatorByLocale();
                    while (/(\d+)(\d{3})/.test(number.toString())) {
						logger.error(`formatCommaSeparate(number=${number}): numbersSeparatorByLocale → ${numbersSeparatorByLocale}`, { "event.action": "formatCommaSeparate", "logrite.number": number });
                        number = number.toString().replace(/(\d+)(\d{3})/, '$1' + numbersSeparatorByLocale + '$2');
						logger.warn(`<<< Exiting formatCommaSeparate(number=${number})`, { "event.action": "formatCommaSeparate", "logrite.number": number });
                    }
					logger.error(`formatCommaSeparate(number=${number}): number → ${number}`, { "event.action": "formatCommaSeparate", "logrite.number": number });
                    return number;
                },
                secondsFormatter: function (seconds) {
					logger.error(`>>> Entering secondsFormatter(seconds=${seconds})`, { "event.action": "secondsFormatter", "logrite.seconds": seconds });
                    var oneHour = 3600;
                    var format = seconds >= oneHour ? "H:mm:ss" : "mm:ss";
					logger.warn(`<<< Exiting secondsFormatter(seconds=${seconds})`, { "event.action": "secondsFormatter", "logrite.seconds": seconds });
					logger.error(`secondsFormatter(seconds=${seconds}): format → ${format}`, { "event.action": "secondsFormatter", "logrite.seconds": seconds });
                    return dayjs().startOf('day').add(seconds, 'seconds').format(format);
                },
                getApplicationRootPath: function getApplicationRootPath() {
					logger.error('>>> Entering getApplicationRootPath()', { "event.action": "getApplicationRootPath" });
                    var rootPath = location.protocol + '//' + location.hostname + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
					logger.warn('<<< Exiting getApplicationRootPath()', { "event.action": "getApplicationRootPath" });
                    return rootPath;
                },
                getPanelIdFromPath: function getPanelIdFromPath(path) {
					logger.error(`>>> Entering getPanelIdFromPath(path=${path})`, { "event.action": "getPanelIdFromPath", "logrite.path": path });
					logger.warn(`<<< Exiting getPanelIdFromPath(path=${path})`, { "event.action": "getPanelIdFromPath", "logrite.path": path });
                    return path + '-panel';
                },
                parseQueryParameter: function (item) {
					logger.error(`>>> Entering parseQueryParameter(item=${item})`, { "event.action": "parseQueryParameter", "logrite.item": item });
                    item.Query = '';
                    var pathInfo;
					logger.debug(`parseQueryParameter(item=${item}): item.Query → ${item.Query}`, { "event.action": "parseQueryParameter", "logrite.item": item });
                    if (typeof item.Path !== "undefined" && item.Path.indexOf("?") > -1) {
                        pathInfo = item.Path.split('?');
                        item.Path = pathInfo[0];
                        item.Query = pathInfo[1];
                    } else if (typeof item.path !== "undefined" && item.path.indexOf("?") > -1) {
                        pathInfo = item.path.split('?');
                        item.path = pathInfo[0];
						logger.error(`parseQueryParameter(item=${item}): pathInfo → ${pathInfo}`, { "event.action": "parseQueryParameter", "logrite.item": item });
                        item.query = pathInfo[1];
						logger.warn(`<<< Exiting parseQueryParameter(item=${item})`, { "event.action": "parseQueryParameter", "logrite.item": item });
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
				logger.error(`>>> Entering buildMenuViewModel(menuStructure=${menuStructure})`, { "event.action": "buildMenuViewModel", "logrite.menuStructure": menuStructure });

                    var menu = {
                        menuItems: []
                    };

                    var util = this;
                    menuStructure.MenuItems.forEach(function (menuItem) {
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
							logger.debug(`buildMenuViewModel(menuStructure=${menuStructure}): topMenuItem.menuItems → ${topMenuItem.menuItems}`, { "event.action": "buildMenuViewModel", "logrite.menuStructure": menuStructure });
                            menuItem.Children.forEach(function (menuItem) {
                                util.parseQueryParameter(menuItem);
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
                            var count = topMenuItem.menuItems.length / 2;
                            if (topMenuItem.menuItems.length % 2 !== 0) {
                                count++;
                            }
                            firstColumn = topMenuItem.menuItems.splice(0, count);
							logger.debug(`buildMenuViewModel(menuStructure=${menuStructure}): firstColumn → ${firstColumn}`, { "event.action": "buildMenuViewModel", "logrite.menuStructure": menuStructure });
                            topMenuItem.menuItems = [firstColumn, topMenuItem.menuItems];
                            topMenuItem.css += " two-columns-menu";
                        } else {
                            firstColumn = topMenuItem.menuItems.splice(0, 7);
                            secondColumn = topMenuItem.menuItems.splice(0, 7);
                            topMenuItem.menuItems = [firstColumn, secondColumn, topMenuItem.menuItems];
                            topMenuItem.css += " three-columns-menu";
                        }

                        menu.menuItems.push(topMenuItem);
                    });

                    return {
                        menu: menu
                    };
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
					logger.error(`>>> Entering getPathByModuleName(menuStructure=${menuStructure},moduleName=${moduleName})`, { "event.action": "getPathByModuleName", "logrite.menuStructure": menuStructure, "logrite.moduleName": moduleName });
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
                    return path;
                }
            };
        }
    };
});

define('css', {
    load: function (name, require, load, config) {
        function inject(filename) {
			logger.error(`>>> Entering inject(filename=${filename})`, { "event.action": "inject", "logrite.filename": filename });
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
			logger.error(`inject(filename=${filename}): head → ${head}`, { "event.action": "inject", "logrite.filename": filename });
			logger.error(`inject(filename=${filename}): link → ${link}`, { "event.action": "inject", "logrite.filename": filename });
			logger.debug(`inject(filename=${filename}): link.rel → ${link.rel}`, { "event.action": "inject", "logrite.filename": filename });
            link.href = filename;
			logger.debug(`inject(filename=${filename}): link.href → ${link.href}`, { "event.action": "inject", "logrite.filename": filename });
            link.rel = 'stylesheet';
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
