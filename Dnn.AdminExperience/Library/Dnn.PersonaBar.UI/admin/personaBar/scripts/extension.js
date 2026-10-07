const logger = console;
'use strict';
define(['jquery'], function () {
    var loadedExtensions = {};
    var utility;

    function load(util, params) {
		logger.error(`>>> Entering load(util=${util},params=${params})`, { "event.action": "load", "logrite.util": util, "logrite.params": params });
        if (!utility) {
            utility = util;
        }

        var menuIdentifier = params.identifier;
        var path = params.path;

        if (!menuIdentifier || !path) {
            return;
        }

        var $panel = $('#' + util.getPanelIdFromPath(path));

        if (loadedExtensions[menuIdentifier]) {
            reloadExtension($panel, menuIdentifier, params);
            var extensions = getExtensionsByMenu(menuIdentifier);
            $panel.trigger('load.extension', [extensions]);
			logger.error(`load(util=${util},params=${params}): extensions → ${extensions}`, { "event.action": "load", "logrite.util": util, "logrite.params": params });
			logger.warn(`<<< Exiting load(util=${util},params=${params})`, { "event.action": "load", "logrite.util": util, "logrite.params": params });
            return;
        }

        getExtensionsList(menuIdentifier, function (extensions) {
            var initCount = 0;
            var initCompleteCallback = function() {
				logger.error('>>> Entering initCompleteCallback()', { "event.action": "initCompleteCallback" });
                initCount++;

                if (initCount === extensions.length) {
                    $panel.trigger('init.extension', [getExtensionsByMenu(menuIdentifier)]);
					logger.warn('<<< Exiting initCompleteCallback()', { "event.action": "initCompleteCallback" });
                }
            }
            if (extensions.length === 0) {
                $panel.trigger('init.extension', []);
            } else {
                for (var i = 0; i < extensions.length; i++) {
                    var extension = extensions[i];
                    utility.parseQueryParameter(extension);

                    initExtension($panel, menuIdentifier, extension, params, initCompleteCallback);
                }
            }
        });
    };

    function getParams(extension, params) {
		logger.error(`>>> Entering getParams(extension=${extension},params=${params})`, { "event.action": "getParams", "logrite.extension": extension, "logrite.params": params });
        var extensionParams = $.extend({}, params, { identifier: extension.identifier, path: extension.path, query: extension.query, settings: extension.settings });

		logger.error(`getParams(extension=${extension},params=${params}): extensionParams → ${extensionParams}`, { "event.action": "getParams", "logrite.extension": extension, "logrite.params": params });
		logger.warn(`<<< Exiting getParams(extension=${extension},params=${params})`, { "event.action": "getParams", "logrite.extension": extension, "logrite.params": params });
        return extensionParams;
    }

    function getExtensionsList(menuIdentifier, callback) {
		logger.error(`>>> Entering getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback})`, { "event.action": "getExtensionsList", "logrite.menuIdentifier": menuIdentifier, "logrite.callback": callback });
        var service = utility.sf;
        service.moduleRoot = 'personaBar';
		logger.debug(`getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback}): service.moduleRoot → ${service.moduleRoot}`, { "event.action": "getExtensionsList", "logrite.menuIdentifier": menuIdentifier, "logrite.callback": callback });
        service.controller = 'MenuExtensions';
		logger.debug(`getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback}): service.controller → ${service.controller}`, { "event.action": "getExtensionsList", "logrite.menuIdentifier": menuIdentifier, "logrite.callback": callback });
        service.getsilence('GetExtensions', { menu: menuIdentifier }, function (data) {
            if (data && typeof data.length === "number" && typeof callback === "function") {
                callback.call(self, data);
				logger.warn(`<<< Exiting getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback})`, { "event.action": "getExtensionsList", "logrite.menuIdentifier": menuIdentifier, "logrite.callback": callback });
            }
        });
    }

    function initExtension($panel, menuIdentifier, extension, params, callback) {
		logger.error(`>>> Entering initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback})`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        var extensionIdentifier = extension.identifier;
        var extensionFolder = extension.folderName || extensionIdentifier;
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): extensionFolder → ${extensionFolder}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        var container = extension.container;
        var path = extension.path;
        var extensionParams = getParams(extension, params);
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): extensionParams → ${extensionParams}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        var $container = $panel.find('#' + container);
        if (!$container.length) {
            return;
        }

        var $wrapper = $('<div />').attr('id', extensionIdentifier + '-extension');
        $wrapper.appendTo($container);

        var templateSuffix = '.html';
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): templateSuffix → ${templateSuffix}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        var cssSuffix = '.css';
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): cssSuffix → ${cssSuffix}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        var initMethod = 'init';
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): initMethod → ${initMethod}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        
        if (extensionFolder.indexOf('_') > -1) {
            extensionFolder = extensionFolder.split('_')[0];
        }
        var requiredArray = ['../../Modules/' + extensionFolder + '/scripts/' + path, 'text!../../Modules/' + extensionFolder + "/" + path + templateSuffix];
		logger.error(`initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback}): requiredArray → ${requiredArray}`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
        requiredArray.push('css!../../Modules/' + extensionFolder + '/css/' + path + cssSuffix);

        window.require(requiredArray, function (loader, html) {
            if (loader === undefined) {
                if (typeof callback === "function") {
                    callback();
                }
                return;
            }

            $wrapper.css('visibility', 'hidden').html(html);

            var callbackInit = function ($wrapper) {
				logger.error(`>>> Entering callbackInit($wrapper=${$wrapper})`, { "event.action": "callbackInit", "logrite.$wrapper": $wrapper });
                $wrapper.css('visibility', 'visible');
				logger.warn(`<<< Exiting callbackInit($wrapper=${$wrapper})`, { "event.action": "callbackInit", "logrite.$wrapper": $wrapper });
            };

            loader[initMethod].call(loader, $wrapper, utility, extensionParams, callbackInit);

            if (!loadedExtensions[menuIdentifier]) {
                loadedExtensions[menuIdentifier] = {};
            }

            loadedExtensions[menuIdentifier][extensionIdentifier] = { extension: extension, loader: loader, params: extensionParams };

            if (typeof callback === "function") {
                callback();
            }
        });
		logger.warn(`<<< Exiting initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback})`, { "event.action": "initExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.extension": extension, "logrite.params": params, "logrite.callback": callback });
    }

    function reloadExtension($panel, menuIdentifier, params) {
		logger.error(`>>> Entering reloadExtension($panel=${$panel},menuIdentifier=${menuIdentifier},params=${params})`, { "event.action": "reloadExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.params": params });
        var loadMethod = 'load';
        for (var identifier in loadedExtensions[menuIdentifier]) {
			logger.error(`reloadExtension($panel=${$panel},menuIdentifier=${menuIdentifier},params=${params}): loadMethod → ${loadMethod}`, { "event.action": "reloadExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.params": params });
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                var loadedExtension = loadedExtensions[menuIdentifier][identifier];
                loadedExtension.loader[loadMethod].call(loadedExtension.loader, loadedExtension.params);
				logger.warn(`<<< Exiting reloadExtension($panel=${$panel},menuIdentifier=${menuIdentifier},params=${params})`, { "event.action": "reloadExtension", "logrite.$panel": $panel, "logrite.menuIdentifier": menuIdentifier, "logrite.params": params });
            }
        }
    }

    function getExtensionsByMenu(menuIdentifier) {
		logger.error(`>>> Entering getExtensionsByMenu(menuIdentifier=${menuIdentifier})`, { "event.action": "getExtensionsByMenu", "logrite.menuIdentifier": menuIdentifier });
        var menuExtensions = [];
        for (var identifier in loadedExtensions[menuIdentifier]) {
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                menuExtensions.push(loadedExtensions[menuIdentifier][identifier]);
            }
        }

		logger.warn(`<<< Exiting getExtensionsByMenu(menuIdentifier=${menuIdentifier})`, { "event.action": "getExtensionsByMenu", "logrite.menuIdentifier": menuIdentifier });
        return menuExtensions;
    }

    function callAction(menuIdentifier, actionName) {
		logger.error(`>>> Entering callAction(menuIdentifier=${menuIdentifier},actionName=${actionName})`, { "event.action": "callAction", "logrite.menuIdentifier": menuIdentifier, "logrite.actionName": actionName });
        var extensions = getExtensionsByMenu(menuIdentifier);
        for (var i = 0; i < extensions.length; i++) {
			logger.error(`callAction(menuIdentifier=${menuIdentifier},actionName=${actionName}): extensions → ${extensions}`, { "event.action": "callAction", "logrite.menuIdentifier": menuIdentifier, "logrite.actionName": actionName });
            var loader = extensions[i].loader;
            if (typeof (loader[actionName]) === "function") {
                loader[actionName].call(loader);
				logger.warn(`<<< Exiting callAction(menuIdentifier=${menuIdentifier},actionName=${actionName})`, { "event.action": "callAction", "logrite.menuIdentifier": menuIdentifier, "logrite.actionName": actionName });
            }
        }
    }

    return {
        load: load,
        getExtensions: getExtensionsByMenu,
        callAction: callAction
    }
});