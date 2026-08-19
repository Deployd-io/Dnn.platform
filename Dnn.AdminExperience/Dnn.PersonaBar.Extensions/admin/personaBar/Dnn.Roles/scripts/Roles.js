const logger = console;
﻿define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier, utility;
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) { 
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;
        utility = util;

        window.dnn.initRoles = function initializeRoles() {
			logger.debug('>>> Entering initializeRoles()');
			logger.debug('<<< Exiting initializeRoles()');
            return {
                utility: utility,
                settings: params.settings,
                moduleName: 'Roles'
            };
        };
        utility.loadBundleScript('modules/dnn.roles/scripts/bundles/roles-bundle.js');

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
