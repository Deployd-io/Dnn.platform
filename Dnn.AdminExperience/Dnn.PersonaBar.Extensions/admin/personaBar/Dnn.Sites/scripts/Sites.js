var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'main/extension', 'main/config', './exportables/Sites/SitesListView'], function ($, ext, cf, slv) {
    'use strict';
    var identifier;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        identifier = params.identifier;
        window.dnn.initSites = function initializeSites() {
			logger.error('>>> Entering initializeSites()', { "event.action": "initializeSites" });
            params.cultureCode = config.culture;
			logger.warn('<<< Exiting initializeSites()', { "event.action": "initializeSites" });
            return params;
        };
        util.loadBundleScript('modules/dnn.sites/scripts/bundles/sites-bundle.js');
		logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): params.cultureCode → ${params.cultureCode}`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });

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
