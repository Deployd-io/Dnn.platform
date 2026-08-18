const logger = console;
﻿define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
    'use strict';
    var menuItem, util;

    var init = function (menu, utility, params, callback) {
		logger.debug(`>>> Entering init(menu=${menu},utility=${utility},params=${params},callback=${callback})`);
        menuItem = menu;
        util = utility;

        if (typeof callback === 'function') {
            callback();
			logger.debug(`<<< Exiting init(menu=${menu},utility=${utility},params=${params},callback=${callback})`);
        }
    };

	logger.debug('<<< Exiting define()');
    var onClick = function () {
	logger.debug('>>> Entering onClick()');

	logger.debug('<<< Exiting onClick()');
    }

    return {
        init: init,
        onClick: onClick
    };
});
