const logger = console;
﻿define(['jquery', 'main/extension', 'main/config', './exportables/Sites/SitesListView'], function ($, ext, cf, slv) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier;
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;
        window.dnn.initSites = function initializeSites() {
			logger.debug('>>> Entering initializeSites()');
            params.cultureCode = config.culture;
			logger.debug(`initializeSites(): params.cultureCode → ${params.cultureCode}`);
			logger.debug('<<< Exiting initializeSites()');
            return params;
        };
        util.loadBundleScript('modules/dnn.sites/scripts/bundles/sites-bundle.js');

		logger.debug(`define(): params.cultureCode → ${params.cultureCode}`);
        if (typeof callback === 'function') {
			logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): params.cultureCode → ${params.cultureCode}`);
            callback();
        }
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
