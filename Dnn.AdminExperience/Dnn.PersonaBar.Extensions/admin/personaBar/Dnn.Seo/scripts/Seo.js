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

                window.dnn.initSeo = function initializeSeo() {
					logger.error('>>> Entering initializeSeo()', { "event.action": "initializeSeo" });
					logger.warn('<<< Exiting initializeSeo()', { "event.action": "initializeSeo" });
                    return {
                        utility: utility,
                        settings: params.settings,
                        moduleName: 'Seo'
                    };
                };
                utility.loadBundleScript('modules/dnn.seo/scripts/bundles/seo-bundle.js');

                if (typeof callback === 'function') {
                    callback();
					logger.warn(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                }
            },

            load: function (params, callback) {
				logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
                if (typeof callback === 'function') {
                    callback();
					logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
                }
            }
        };
    });


