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

            window.dnn.initSiteImportExport = function initializeSiteImportExport() {
				logger.debug('>>> Entering initializeSiteImportExport()');
                if (typeof callback === 'function') {
                    callback();
                }

				logger.debug('<<< Exiting initializeSiteImportExport()');
                return {
                    utility: utility,
                    settings: params.settings,
                    moduleName: 'SiteImportExport'
                };
            };
            utility.loadBundleScript('modules/dnn.siteimportexport/scripts/bundles/siteimportexport-bundle.js');
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
