const logger = console;
define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier;
    var utility;
    cf.init();
    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;
        utility = util;

        window.dnn.initPrompt = function initializePrompt() {
			logger.debug('>>> Entering initializePrompt()');
			logger.debug('<<< Exiting initializePrompt()');
            return {
                utility: utility,
                settings: params.settings,
                moduleName: 'Prompt'
            };
        };
        utility.loadBundleScript('modules/dnn.Prompt/scripts/bundles/prompt-bundle.js');

        if (typeof callback === 'function') {
            callback();
        }
    };

	logger.debug('<<< Exiting define()');
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

//return {
//    init: function (wrapper, util, params, callback) {
//        identifier = params.identifier;
//        utility = util;
//        $.ajax({
//            dataType: "script",
//            cache: true,
//            url: "modules/dnn.prompt/scripts/bundles/prompt-bundle.js",
//            success: function () {
logger.debug('>>> Entering success()');
//                window.dnnPrompt = new window.DnnPrompt(vsn, wrapper, util, params);
//            },
//        });
//        if (typeof callback === 'function') {
//            callback();
//        }
//    },
//    load: function (params, callback) {
//        if (typeof callback === 'function') {
//            callback();
//        }
//    }
logger.debug('<<< Exiting success()');
//};