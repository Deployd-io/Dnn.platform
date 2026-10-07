var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'knockout', 'main/extension', 'main/config', 'jquery-ui.min', 'dnn.jquery'], function ($, ko, ext, cf) {
    'use strict';
    window.ko = ko;

    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        window.dnn.initPages = function initializePages() {
			logger.error('>>> Entering initializePages()', { "event.action": "initializePages" });
			logger.warn('<<< Exiting initializePages()', { "event.action": "initializePages" });
            return {
                utilities: util,
                moduleName: "Pages",
                config: config,
                viewName: params.viewName,
                viewParams: params.viewParams,
                settings: params.settings
            };
        };

        util.loadBundleScript('modules/dnn.pages/scripts/bundles/pages-bundle.js');

        if (typeof callback === 'function') {
            callback();
        }
    };

    var load = function (params, callback) {
		logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
        window.dnn.pages.load({
            viewName: params.viewName,
            viewParams: params.viewParams,
            settings: params.settings
        });
        if (window.dnn.pages.pageHierarchyManager &&
            window.dnn.pages.pageHierarchyManager._initialized) {
            window.dnn.pages.pageHierarchyManager._resizeContentContainer(true);
        }
        if (typeof callback === 'function') {
            callback();
			logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
        }
    };

    return {
        init: init,
        load: load
    };
});
