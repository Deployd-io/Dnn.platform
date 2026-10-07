var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
    'use strict';
    var identifier;
    var utility;
    cf.init();
    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        identifier = params.identifier;
        utility = util;

        window.dnn.initPrompt = function initializePrompt() {
			logger.error('>>> Entering initializePrompt()', { "event.action": "initializePrompt" });
			logger.warn('<<< Exiting initializePrompt()', { "event.action": "initializePrompt" });
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

//return {
//    init: function (wrapper, util, params, callback) {
//        identifier = params.identifier;
//        utility = util;
//        $.ajax({
//            dataType: "script",
//            cache: true,
//            url: "modules/dnn.prompt/scripts/bundles/prompt-bundle.js",
//            success: function () {
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
//};