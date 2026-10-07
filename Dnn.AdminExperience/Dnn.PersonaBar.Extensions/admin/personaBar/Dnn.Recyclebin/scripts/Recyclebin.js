var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to manage the Recycle Bin
*/
define(['jquery',
    'knockout',
    'knockout.mapping',
    './RecycleBin.ViewModel',
	'dnn.jquery',
	'dnn.extensions',
	'dnn.jquery.extensions',
	'jquery.tokeninput',
	'dnn.jScrollBar',
    'jquery-ui.min'],
    function ($, ko, koMapping, DnnPageRecycleBin) {
        'use strict';

        var dnnPageRecycleBin;

        var init, load,
            initRecycleBin, viewRecycleBin;

        var utility = null;

        ko.mapping = koMapping;

        init = function (wrapper, util, params, callback) {
			logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            utility = util;

            dnnPageRecycleBin = new DnnPageRecycleBin(utility.resx.Recyclebin, utility.sf, utility, null, params.settings);

            initRecycleBin(wrapper);

            if (typeof callback === 'function') {
                callback();
				logger.warn(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            }
        };

        load = function (params, callback) {
			logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
            viewRecycleBin();

            if (dnn && dnn.dnnPageHierarchy) {
                dnn.dnnPageHierarchy.load();
				logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
            }
        };

        initRecycleBin = function (wrapper) {
			logger.error(`>>> Entering initRecycleBin(wrapper=${wrapper})`, { "event.action": "initRecycleBin", "logrite.wrapper": wrapper });
            dnnPageRecycleBin.init(wrapper);
            viewRecycleBin();
			logger.warn(`<<< Exiting initRecycleBin(wrapper=${wrapper})`, { "event.action": "initRecycleBin", "logrite.wrapper": wrapper });
        };

        viewRecycleBin = function () {
			logger.error('>>> Entering viewRecycleBin()', { "event.action": "viewRecycleBin" });
            if (typeof dnn.dnnPageHierarchy != "undefined" && dnn.dnnPageHierarchy.hasPendingChanges()) {
				logger.warn('<<< Exiting viewRecycleBin()', { "event.action": "viewRecycleBin" });
                return dnn.dnnPageHierarchy.handlePendingChanges();
            }
            dnnPageRecycleBin.show();
        };

        return {
            init: init,
            load: load
        };
    });
