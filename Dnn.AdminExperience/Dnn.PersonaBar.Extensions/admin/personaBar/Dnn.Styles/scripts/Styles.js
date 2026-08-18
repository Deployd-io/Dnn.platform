const logger = console;
﻿define(['main/config'], function (cf) {
	logger.debug('>>> Entering define()');
    'use strict';
    var identifier;
    var config = cf.init();
	logger.debug(`define(): config → ${config}`);

    var init = function (wrapper, util, params, callback) {
		logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
        identifier = params.identifier;

        window.dnn.initStyles = function () {
			logger.debug('>>> Entering function()');
            return {
                utility: util,
                params: params,
                moduleName: "Styles"
            };
			logger.debug('<<< Exiting function()');
        };

        if (typeof callback === 'function') {
            callback();
        }
		logger.debug(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
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
