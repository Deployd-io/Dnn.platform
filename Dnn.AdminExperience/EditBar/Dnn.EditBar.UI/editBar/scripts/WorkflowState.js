var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery'], function ($) {
    'use strict';
    var menuItem, util;

    var init = function (menu, utility, params, callback) {
		logger.error(`>>> Entering init(menu=${menu},utility=${utility},params=${params},callback=${callback})`, { "event.action": "init", "logrite.menu": menu, "logrite.utility": utility, "logrite.params": params, "logrite.callback": callback });
        menuItem = menu;
        util = utility;

        if (typeof callback === 'function') {
            callback();
			logger.warn(`<<< Exiting init(menu=${menu},utility=${utility},params=${params},callback=${callback})`, { "event.action": "init", "logrite.menu": menu, "logrite.utility": utility, "logrite.params": params, "logrite.callback": callback });
        }
    };

    var onClick = function () {
	logger.error('>>> Entering onClick()', { "event.action": "onClick" });

	logger.warn('<<< Exiting onClick()', { "event.action": "onClick" });
    }

    return {
        init: init,
        onClick: onClick
    };
});
