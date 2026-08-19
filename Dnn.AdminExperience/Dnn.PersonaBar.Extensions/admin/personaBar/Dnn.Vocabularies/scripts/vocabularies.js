const logger = console;
'use strict';
define(['jquery',
    'main/config'
],
    function ($, cf) {
		logger.debug(`>>> Entering function(cf=${cf})`);
        var utility;
        var config = cf.init();

        return {
            init: function (wrapper, util, params, callback) {
				logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
                utility = util;

				logger.debug(`function(cf=${cf}): utility → ${utility}`);
                window.dnn.initVocabularies = function initializeVocabularies() {
					logger.debug('>>> Entering initializeVocabularies()');
					logger.debug('<<< Exiting initializeVocabularies()');
                    return {
                        utility: utility,
                        settings: params.settings,
                        moduleName: 'Vocabularies'
                    };
                };
                utility.loadBundleScript('modules/dnn.vocabularies/scripts/bundles/vocabulary-bundle.js');

                if (typeof callback === 'function') {
                    callback();
					logger.debug(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
                }
            },

            load: function (params, callback) {
				logger.debug(`>>> Entering load(params=${params},callback=${callback})`);
                if (typeof callback === 'function') {
					logger.debug(`<<< Exiting function(cf=${cf})`);
                    callback();
					logger.debug(`<<< Exiting load(params=${params},callback=${callback})`);
                }
            }
        };
    });


