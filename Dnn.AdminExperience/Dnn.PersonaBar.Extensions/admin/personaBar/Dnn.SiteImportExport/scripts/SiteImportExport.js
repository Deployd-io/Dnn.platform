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

            window.dnn.initSiteImportExport = function initializeSiteImportExport() {
				logger.error('>>> Entering initializeSiteImportExport()', { "event.action": "initializeSiteImportExport" });
                if (typeof callback === 'function') {
                    callback();
                }

				logger.warn('<<< Exiting initializeSiteImportExport()', { "event.action": "initializeSiteImportExport" });
                return {
                    utility: utility,
                    settings: params.settings,
                    moduleName: 'SiteImportExport'
                };
            };
            utility.loadBundleScript('modules/dnn.siteimportexport/scripts/bundles/siteimportexport-bundle.js');
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
