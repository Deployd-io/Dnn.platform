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
        var dialog = window.top.dnn.ContentEditorManager.getModuleDialog();
        var contentPane = dialog.getDefaultPane();
		logger.debug(`onClick(): dialog → ${dialog}`);
		logger.debug(`onClick(): contentPane → ${contentPane}`);
        var moduleManager = contentPane.data('dnnModuleManager');
        if (moduleManager) {
			logger.debug(`onClick(): moduleManager → ${moduleManager}`);
            moduleManager.getHandler().click();
			logger.debug('<<< Exiting onClick()');
        }
    }

    return {
        init: init,
        onClick: onClick
    };
});
