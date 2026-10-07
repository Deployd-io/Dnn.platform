(function () {
    'use strict';

    var debugMode = window.parent['editBarSettings']['debugMode'] === true;
    var cdv = window.parent['editBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css/'
        },
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : '')
    });
    requirejs.onError = function (err) {
		logger.error(`>>> Entering onError(err=${err})`, { "event.action": "onError", "logrite.err": err });
        // If requireJs throws a timeout reload the page
        if (err.requireType === 'timeout') {
            console.error(err);
            location.reload();
        }
        else {
            console.error(err);
			logger.warn(`<<< Exiting onError(err=${err})`, { "event.action": "onError", "logrite.err": err });
            throw err;
        }
    };
})();

require(['jquery', 'knockout', '../util', '../sf', '../config', '../eventEmitter', '../gateway'],
    function ($, ko, ut, sf, cf, eventEmitter, Gateway) {
        var iframe = window.parent.document.getElementById("editBar-iframe");
        if (!iframe) return;

        var config = cf.init();
        var utility = ut.init(config);

        var $editBar = $("#edit-bar");

        var menuLoaders = {}, callAction;

        window.requirejs.config({
            paths: {
                'rootPath': utility.getApplicationRootPath()
            }
        });

        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
            switchMode: function(mode) {
				logger.error(`>>> Entering switchMode(mode=${mode})`, { "event.action": "switchMode", "logrite.mode": mode });
                window.top.$('#editBar-iframe').removeClass('small middle').addClass(mode);
				logger.warn(`<<< Exiting switchMode(mode=${mode})`, { "event.action": "switchMode", "logrite.mode": mode });
            }
        };
        util = $.extend(util, utility);
        // end define util

        var getMenuItem = function(name) {
			logger.error(`>>> Entering getMenuItem(name=${name})`, { "event.action": "getMenuItem", "logrite.name": name });
            for (var i = 0; i < config.items.length; i++) {
                if (config.items[i].name === name) {
                    return config.items[i];
                }
            }

			logger.warn(`<<< Exiting getMenuItem(name=${name})`, { "event.action": "getMenuItem", "logrite.name": name });
            return null;
        }

        var getMenuLoader = function (item, callback) {
			logger.error(`>>> Entering getMenuLoader(item=${item},callback=${callback})`, { "event.action": "getMenuLoader", "logrite.item": item, "logrite.callback": callback });
            if (!item.loader && !item.group) {
                return;
            }

            var loaderName = item.loader || item.group;
            if (typeof menuLoaders[loaderName] !== "undefined") {
                if (typeof callback === "function") {
                    callback(menuLoaders[loaderName]);
                }
                return;
            }

            
            var initMethod = 'init';
			logger.error(`getMenuLoader(item=${item},callback=${callback}): initMethod → ${initMethod}`, { "event.action": "getMenuLoader", "logrite.item": item, "logrite.callback": callback });
            var requiredArray = ['../' + loaderName];
			logger.error(`getMenuLoader(item=${item},callback=${callback}): requiredArray → ${requiredArray}`, { "event.action": "getMenuLoader", "logrite.item": item, "logrite.callback": callback });
            if (item.customLayout) {
                var templateSuffix = '.html';
				logger.error(`getMenuLoader(item=${item},callback=${callback}): templateSuffix → ${templateSuffix}`, { "event.action": "getMenuLoader", "logrite.item": item, "logrite.callback": callback });
                requiredArray.push('text!../../' + loaderName + templateSuffix);
            }

            window.require(requiredArray, function (loader, html) {
                if (typeof loader === "undefined") {
                    return;
                }

                if (html) {
                    html = html.replace(/\[resx:(.+?)\]/gi, function (matches, key) {
                        return item.resx[key] || key;
                    });
                }
                var params = { html: html };

                menuLoaders[loaderName] = loader;

                loader[initMethod].call(loader, item, util, params, function() {
                    if (typeof callback === "function") {
                        callback(loader);
                    }
                });
            });
			logger.warn(`<<< Exiting getMenuLoader(item=${item},callback=${callback})`, { "event.action": "getMenuLoader", "logrite.item": item, "logrite.callback": callback });
        }

        util.callAction = function (menuName, actionName, params) {
			logger.error(`>>> Entering callAction(menuName=${menuName},actionName=${actionName},params=${params})`, { "event.action": "callAction", "logrite.menuName": menuName, "logrite.actionName": actionName, "logrite.params": params });
            var menuItem = getMenuItem(menuName);
            if (menuItem) {
				logger.error(`callAction(menuName=${menuName},actionName=${actionName},params=${params}): menuItem → ${menuItem}`, { "event.action": "callAction", "logrite.menuName": menuName, "logrite.actionName": actionName, "logrite.params": params });
                getMenuLoader(menuItem, function(loader) {
                    if (typeof loader[actionName] === "function") {
                        loader[actionName].apply(menuLoaders[menuName], params);
						logger.warn(`<<< Exiting callAction(menuName=${menuName},actionName=${actionName},params=${params})`, { "event.action": "callAction", "logrite.menuName": menuName, "logrite.actionName": actionName, "logrite.params": params });
                    }
                });
            }
        }

        var loadMenu = function (item) {
			logger.error(`>>> Entering loadMenu(item=${item})`, { "event.action": "loadMenu", "logrite.item": item });
            if (!item.loader && !item.group) {
                return;
            }

            var cssSuffix = '.css';
			logger.error(`loadMenu(item=${item}): cssSuffix → ${cssSuffix}`, { "event.action": "loadMenu", "logrite.item": item });
            var requiredArray = ['css!../../css/' + (item.loader || item.group) + cssSuffix];
			logger.error(`loadMenu(item=${item}): requiredArray → ${requiredArray}`, { "event.action": "loadMenu", "logrite.item": item });

            window.require(requiredArray);

            if (item.customLayout) {
                getMenuLoader(item);
            }
			logger.warn(`<<< Exiting loadMenu(item=${item})`, { "event.action": "loadMenu", "logrite.item": item });
        }

        var renderMenu = function (menuItem) {
			logger.error(`>>> Entering renderMenu(menuItem=${menuItem})`, { "event.action": "renderMenu", "logrite.menuItem": menuItem });
            if (menuItem.template) {
                return menuItem.template;
            } else {
                var text = menuItem.resx[menuItem.text] || menuItem.text;
                var menu = '<button href="javascript:void(0);">' + text + '</button>';
				logger.error(`renderMenu(menuItem=${menuItem}): menu → ${menu}`, { "event.action": "renderMenu", "logrite.menuItem": menuItem });

                var tooltip = menuItem.resx[menuItem.name + ".Tooltip"];
				logger.error(`renderMenu(menuItem=${menuItem}): tooltip → ${tooltip}`, { "event.action": "renderMenu", "logrite.menuItem": menuItem });
                if (tooltip) {
                    menu += '<div class="submenuEditBar">' + tooltip + '</div>';
                }

				logger.warn(`<<< Exiting renderMenu(menuItem=${menuItem})`, { "event.action": "renderMenu", "logrite.menuItem": menuItem });
                return menu;
            }
        }

        var authorizeCheck = function (callback) {
			logger.error(`>>> Entering authorizeCheck(callback=${callback})`, { "event.action": "authorizeCheck", "logrite.callback": callback });
            util.sf.moduleRoot = 'editBar/common';
            util.sf.controller = 'Common';
            util.sf.getsilence('CheckAuthorized', {}, function (data) {
                if (data.success) {
                    if (typeof callback === "function") {
                        callback();
                    }
                } else {
                    var loginUrl = config.loginUrl;
                    if (typeof window.parent.dnnModal != "undefined") {
                        window.parent.dnnModal.show(loginUrl + (loginUrl.indexOf('?') === -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
                    } else {
                        location.href = loginUrl;
						logger.warn(`<<< Exiting authorizeCheck(callback=${callback})`, { "event.action": "authorizeCheck", "logrite.callback": callback });
                    }
                }
            });
        };

        var menuItemClick = function (menuItem) {
			logger.error(`>>> Entering menuItemClick(menuItem=${menuItem})`, { "event.action": "menuItemClick", "logrite.menuItem": menuItem });
            for (var name in menuLoaders) {
                if (menuLoaders.hasOwnProperty(name) && name !== menuItem.name) {
                    var loader = menuLoaders[name];
                    if (typeof loader["onBlur"] === "function") {
                        loader["onBlur"].call(loader);
                    }
                }
            }
            authorizeCheck(function() {
                getMenuLoader(menuItem, function(loader) {
                    if (typeof loader["onClick"] === "function") {
                        loader["onClick"].call(loader, menuItem);
						logger.warn(`<<< Exiting menuItemClick(menuItem=${menuItem})`, { "event.action": "menuItemClick", "logrite.menuItem": menuItem });
                    }
                });
            });
        }

        var buildViewModel = function() {
			logger.error('>>> Entering buildViewModel()', { "event.action": "buildViewModel" });
            var viewModel = {};
            viewModel.leftMenus = ko.observableArray([]);
			logger.debug(`buildViewModel(): viewModel.renderMenu → ${viewModel.renderMenu}`, { "event.action": "buildViewModel" });
			logger.debug(`buildViewModel(): viewModel.menuItemClick → ${viewModel.menuItemClick}`, { "event.action": "buildViewModel" });
            viewModel.rightMenus = ko.observableArray([]);

            for (var i = 0; i < config.items.length; i++) {
                var menuItem = config.items[i];
                menuItem.resx = util.resx[menuItem.name] || (menuItem.group ? util.resx[menuItem.group] : null) || util.resx.Common;
                loadMenu(menuItem);

                switch (menuItem.parent.toLowerCase()) {
                    case "leftmenu":
                        viewModel.leftMenus.push(menuItem);
                        break;
                    case "rightmenu":
                        viewModel.rightMenus.push(menuItem);
                        break;
                }
            }

            viewModel.renderMenu = renderMenu;
            viewModel.menuItemClick = menuItemClick;

			logger.warn('<<< Exiting buildViewModel()', { "event.action": "buildViewModel" });
            return viewModel;
        }

        var loadMenus = function() {
			logger.error('>>> Entering loadMenus()', { "event.action": "loadMenus" });
            var viewModel = buildViewModel();
            ko.applyBindings(viewModel, $editBar[0]);
			logger.error(`loadMenus(): viewModel → ${viewModel}`, { "event.action": "loadMenus" });
        }

        util.loadResx(function() {
            loadMenus();
        });
        
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.EditBar = new Gateway(util);
    });
