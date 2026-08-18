const logger = console;
﻿define(['jquery', 'main/extension', 'main/config', './exportables/Users/UsersCommon'], function ($, ext, cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier;
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;
        window.dnn.initUsers = function initializeUsers() {
			logger.debug('>>> Entering initializeUsers()');
			logger.debug('<<< Exiting initializeUsers()');
            return {
                utility: util,
                settings: params.settings,
                moduleName: 'Users'
            };
        };
        util.loadBundleScript('modules/dnn.users/scripts/bundles/users-bundle.js');

        if (typeof callback === 'function') {
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
