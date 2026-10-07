var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        return {
            init: function (wrapper, util, params, callback) {
				logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                utility = util;

                window.dnn.initExtensions = function initializeExtensions() {
					logger.error('>>> Entering initializeExtensions()', { "event.action": "initializeExtensions" });
					logger.warn('<<< Exiting initializeExtensions()', { "event.action": "initializeExtensions" });
                    return {
                        utility: utility,
                        siteRoot: config.siteRoot,
                        settings: params.settings,
                        moduleName: 'Extensions'
                    };
                };
                utility.loadBundleScript('modules/dnn.extensions/scripts/bundles/extensions-bundle.js');

                if (typeof callback === "function") {
                    callback();
					logger.warn(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                }
            },

            load: function (params, callback) {
				logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
                if (typeof callback === "function") {
                    callback();
					logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
                }
            }
        };
    });


