const logger = console;
﻿define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier;
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;
        window.dnn.initThemes = function () {
			logger.debug('>>> Entering function()');
            return {
                utility: util,
                params: params,
                moduleName: "Themes"
            };
			logger.debug('<<< Exiting function()');
        };

        util.loadBundleScript('modules/dnn.themes/scripts/bundles/themes-bundle.js');

        if (typeof callback === 'function') {
            callback();
        }
		logger.debug(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
    };

	logger.debug('<<< Exiting define()');
    var load = function (params, callback) {
		logger.debug(`>>> Entering load(params=${params},callback=${callback})`);
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
