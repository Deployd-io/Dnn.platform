const logger = console;
﻿'use strict';
define(['jquery'], function () {
	logger.debug('>>> Entering define()');
    var loadedExtensions = {};
    var utility;
	logger.debug('<<< Exiting define()');

    function load(util, params) {
		logger.debug(`>>> Entering load(util=${util},params=${params})`);
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
            return;
        }

        getExtensionsList(menuIdentifier, function (extensions) {
            var initCount = 0;
			logger.debug(`>>> Entering getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback})`);
            var initCompleteCallback = function() {
				logger.debug('>>> Entering initCompleteCallback()');
                initCount++;

                if (initCount === extensions.length) {
                    $panel.trigger('init.extension', [getExtensionsByMenu(menuIdentifier)]);
					logger.debug('<<< Exiting initCompleteCallback()');
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
			logger.debug(`<<< Exiting getExtensionsList(menuIdentifier=${menuIdentifier},callback=${callback})`);
            }
        });
		logger.debug(`<<< Exiting load(util=${util},params=${params})`);
    };

    function getParams(extension, params) {
		logger.debug(`>>> Entering getParams(extension=${extension},params=${params})`);
        var extensionParams = $.extend({}, params, { identifier: extension.identifier, path: extension.path, query: extension.query, settings: extension.settings });

		logger.debug(`<<< Exiting getParams(extension=${extension},params=${params})`);
        return extensionParams;
    }

    function getExtensionsList(menuIdentifier, callback) {
        var service = utility.sf;
        service.moduleRoot = 'personaBar';
        service.controller = 'MenuExtensions';
        service.getsilence('GetExtensions', { menu: menuIdentifier }, function (data) {
			logger.debug(`>>> Entering getsilence(menuIdentifier=${menuIdentifier})`);
            if (data && typeof data.length === "number" && typeof callback === "function") {
                callback.call(self, data);
				logger.debug(`<<< Exiting getsilence(menuIdentifier=${menuIdentifier})`);
            }
        });
    }

    function initExtension($panel, menuIdentifier, extension, params, callback) {
		logger.debug(`>>> Entering initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback})`);
        var extensionIdentifier = extension.identifier;
        var extensionFolder = extension.folderName || extensionIdentifier;
        var container = extension.container;
        var path = extension.path;
        var extensionParams = getParams(extension, params);
        var $container = $panel.find('#' + container);
        if (!$container.length) {
            return;
        }

        var $wrapper = $('<div />').attr('id', extensionIdentifier + '-extension');
        $wrapper.appendTo($container);

        var templateSuffix = '.html';
        var cssSuffix = '.css';
        var initMethod = 'init';
        
        if (extensionFolder.indexOf('_') > -1) {
            extensionFolder = extensionFolder.split('_')[0];
        }
        var requiredArray = ['../../Modules/' + extensionFolder + '/scripts/' + path, 'text!../../Modules/' + extensionFolder + "/" + path + templateSuffix];
        requiredArray.push('css!../../Modules/' + extensionFolder + '/css/' + path + cssSuffix);

        window.require(requiredArray, function (loader, html) {
			logger.debug('>>> Entering require()');
            if (loader === undefined) {
                if (typeof callback === "function") {
                    callback();
                }
                return;
            }

            $wrapper.css('visibility', 'hidden').html(html);

            var callbackInit = function ($wrapper) {
				logger.debug(`>>> Entering callbackInit($wrapper=${$wrapper})`);
                $wrapper.css('visibility', 'visible');
				logger.debug(`<<< Exiting callbackInit($wrapper=${$wrapper})`);
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
		logger.debug(`<<< Exiting initExtension($panel=${$panel},menuIdentifier=${menuIdentifier},extension=${extension},params=${params},callback=${callback})`);
    }

    function reloadExtension($panel, menuIdentifier, params) {
		logger.debug(`>>> Entering reloadExtension($panel=${$panel},menuIdentifier=${menuIdentifier},params=${params})`);
        var loadMethod = 'load';
        for (var identifier in loadedExtensions[menuIdentifier]) {
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                var loadedExtension = loadedExtensions[menuIdentifier][identifier];
                loadedExtension.loader[loadMethod].call(loadedExtension.loader, loadedExtension.params);
				logger.debug(`<<< Exiting reloadExtension($panel=${$panel},menuIdentifier=${menuIdentifier},params=${params})`);
            }
        }
    }

    function getExtensionsByMenu(menuIdentifier) {
		logger.debug(`>>> Entering getExtensionsByMenu(menuIdentifier=${menuIdentifier})`);
        var menuExtensions = [];
        for (var identifier in loadedExtensions[menuIdentifier]) {
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                menuExtensions.push(loadedExtensions[menuIdentifier][identifier]);
            }
        }

		logger.debug(`<<< Exiting getExtensionsByMenu(menuIdentifier=${menuIdentifier})`);
        return menuExtensions;
    }

    function callAction(menuIdentifier, actionName) {
		logger.debug(`>>> Entering callAction(menuIdentifier=${menuIdentifier},actionName=${actionName})`);
        var extensions = getExtensionsByMenu(menuIdentifier);
        for (var i = 0; i < extensions.length; i++) {
            var loader = extensions[i].loader;
            if (typeof (loader[actionName]) === "function") {
                loader[actionName].call(loader);
				logger.debug(`<<< Exiting callAction(menuIdentifier=${menuIdentifier},actionName=${actionName})`);
            }
        }
    }

    return {
        load: load,
        getExtensions: getExtensionsByMenu,
        callAction: callAction
    }
});