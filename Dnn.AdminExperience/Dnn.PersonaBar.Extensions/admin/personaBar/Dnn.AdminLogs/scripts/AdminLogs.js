var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

        var init = function (wrapper, util, params, callback) {
			logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            identifier = params.identifier;
            utility = util;

            window.dnn.initAdminLogs = function initializeAdminLogs() {
				logger.error('>>> Entering initializeAdminLogs()', { "event.action": "initializeAdminLogs" });
				logger.warn('<<< Exiting initializeAdminLogs()', { "event.action": "initializeAdminLogs" });
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

        var load = function (params, callback) {
			logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
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
