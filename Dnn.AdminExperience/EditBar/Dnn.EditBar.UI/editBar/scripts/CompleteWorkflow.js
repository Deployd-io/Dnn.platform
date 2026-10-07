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
        util.sf.moduleRoot = 'internalservices';
        util.sf.controller = "contentWorkflowService";
		logger.warn('<<< Exiting onClick()', { "event.action": "onClick" });
        util.sf.post('CompleteWorkflow', {}, function done() {
			logger.error('>>> Entering done()', { "event.action": "done" });
            window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
        });
    }

    return {
        init: init,
        onClick: onClick
    };
});
