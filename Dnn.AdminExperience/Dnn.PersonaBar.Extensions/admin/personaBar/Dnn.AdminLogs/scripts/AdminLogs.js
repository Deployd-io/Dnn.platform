const logger = console;
﻿define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
		logger.debug(`>>> Entering function(ext=${ext},cf=${cf})`);
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

        var init = function (wrapper, util, params, callback) {
			logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
            identifier = params.identifier;
            utility = util;

            window.dnn.initAdminLogs = function initializeAdminLogs() {
				logger.debug('>>> Entering initializeAdminLogs()');
				logger.debug('<<< Exiting initializeAdminLogs()');
                return {
                    utility: utility,
                    settings: params.settings,
                    moduleName: 'AdminLogs'
                };
            };
            utility.loadBundleScript('modules/dnn.adminlogs/scripts/bundles/adminLogs-bundle.js');

            if (typeof callback === 'function') {
                callback();
            }
        };

		logger.debug(`<<< Exiting function(ext=${ext},cf=${cf})`);
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
