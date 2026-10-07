var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
    'use strict';
    var identifier, utility;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) { 
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        identifier = params.identifier;
        utility = util;

        window.dnn.initRoles = function initializeRoles() {
			logger.error('>>> Entering initializeRoles()', { "event.action": "initializeRoles" });
			logger.warn('<<< Exiting initializeRoles()', { "event.action": "initializeRoles" });
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
