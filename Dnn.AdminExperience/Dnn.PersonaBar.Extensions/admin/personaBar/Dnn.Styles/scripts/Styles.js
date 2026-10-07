var logger = (typeof logger !== 'undefined' && logger) || console;
define(['main/config'], function (cf) {
    'use strict';
    var identifier;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
		logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
        identifier = params.identifier;

        window.dnn.initStyles = function () {
			logger.error('>>> Entering initStyles()', { "event.action": "initStyles" });
			logger.warn('<<< Exiting initStyles()', { "event.action": "initStyles" });
            return {
                utility: util,
                params: params,
                moduleName: "Styles"
            };
        };

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
