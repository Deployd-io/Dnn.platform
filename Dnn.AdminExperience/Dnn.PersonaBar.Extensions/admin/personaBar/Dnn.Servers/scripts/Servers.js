var logger = (typeof logger !== 'undefined' && logger) || console;
define(['main/config'], function (cf) {
    'use strict';
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        params.handleTabViewInModule = true;

		logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): params.handleTabViewInModule → ${params.handleTabViewInModule}`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        window.dnn.initServers = function initServers() {
			logger.error('>>> Entering initServers()', { "event.action": "initServers" });
            if (typeof callback === 'function') {
                callback();
            }
			logger.warn('<<< Exiting initServers()', { "event.action": "initServers" });
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
