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

    var pageEditComplete = function () {
		logger.debug('>>> Entering pageEditComplete()');
        var personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;

        if (personaBar) {
            personaBar.offPanelClose(pageEditComplete);
        }

        if (window.top.dnn.EditBar) {
            window.top.dnn.EditBar.action('PrivatePage', 'update');
			logger.debug('<<< Exiting pageEditComplete()');
        }
    }

	logger.debug('<<< Exiting define()');
    var onClick = function () {
		logger.debug('>>> Entering onClick()');
        var personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel('Dnn.Pages', { viewName: 'edit', viewParams: null });
            personaBar.onPanelClose(pageEditComplete);
			logger.debug('<<< Exiting onClick()');
        }
    }

    return {
        init: init,
        onClick: onClick
    };
});
