var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'main/extension', 'main/config', './exportables/Users/UsersCommon'], function ($, ext, cf) {
    'use strict';
    var identifier;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        identifier = params.identifier;
        window.dnn.initUsers = function initializeUsers() {
			logger.error('>>> Entering initializeUsers()', { "event.action": "initializeUsers" });
			logger.warn('<<< Exiting initializeUsers()', { "event.action": "initializeUsers" });
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
