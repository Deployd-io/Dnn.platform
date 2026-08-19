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
        var mode = "View";

		logger.debug(`onClick(): mode → ${mode}`);
        util.sf.moduleRoot = 'internalservices';
        util.sf.controller = "controlBar";
		logger.debug('<<< Exiting onClick()');
        util.sf.post('ToggleUserMode', { UserMode: mode }, function handleToggleUserMode() {
            window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
        });
		logger.debug(`<<< Exiting handleToggleUserMode(mode=${mode})`);
    }

    return {
        init: init,
        onClick: onClick
    };
});
