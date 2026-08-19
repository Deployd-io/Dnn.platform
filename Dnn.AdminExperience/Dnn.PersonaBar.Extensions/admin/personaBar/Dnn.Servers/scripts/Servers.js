const logger = console;
﻿define(['main/config'], function (cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        params.handleTabViewInModule = true;

		logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): params.handleTabViewInModule → ${params.handleTabViewInModule}`);
        window.dnn.initServers = function initServers() {
			logger.debug('>>> Entering initServers()');
            if (typeof callback === 'function') {
				logger.debug(`define(): params.handleTabViewInModule → ${params.handleTabViewInModule}`);
                callback();
            }
			logger.debug('<<< Exiting initServers()');
            return {
                utilities: util,
                moduleName: "Servers",
                path: params.path,
                config: config,
                settings: params.settings
            };
        };
        util.loadBundleScript('modules/dnn.servers/scripts/bundles/servers-bundle.js');
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
