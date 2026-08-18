const logger = console;
﻿define(['jquery', 'knockout', 'main/extension', 'main/config', 'jquery-ui.min', 'dnn.jquery'], function ($, ko, ext, cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    window.ko = ko;
	logger.debug(`define(): window.ko → ${window.ko}`);

    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        window.dnn.initPages = function initializePages() {
			logger.debug('>>> Entering initializePages()');
			logger.debug('<<< Exiting initializePages()');
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

	logger.debug('<<< Exiting define()');
    var load = function (params, callback) {
		logger.debug(`>>> Entering load(params=${params},callback=${callback})`);
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
			logger.debug(`<<< Exiting load(params=${params},callback=${callback})`);
        }
    };

    return {
        init: init,
        load: load
    };
});
