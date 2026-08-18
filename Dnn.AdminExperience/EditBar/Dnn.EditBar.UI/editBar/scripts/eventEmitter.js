const logger = console;
﻿define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
    'use strict';
    
	logger.debug('<<< Exiting define()');
    /*
     * This module is responsible to manage all the main Persona Bar events
     * (i.e.: open panel, close panel). 
     * With this module consumers can subscribe, unsubscribe to events and emit events
     */
    var eventEmitter = {
        addOpenEventListener: function (callback) {
			logger.debug(`>>> Entering addOpenEventListener(callback=${callback})`);
            $(document).on('editbar:onOpen', callback);
			logger.debug(`<<< Exiting addOpenEventListener(callback=${callback})`);
        },
        removeOpenEventListener: function (callback) {
			logger.debug(`>>> Entering removeOpenEventListener(callback=${callback})`);
            $(document).off('editbar:onOpen', callback);
			logger.debug(`<<< Exiting removeOpenEventListener(callback=${callback})`);
        },
        addCloseEventListener: function (callback) {
			logger.debug(`>>> Entering addCloseEventListener(callback=${callback})`);
            $(document).on('editbar:onClose', callback);
			logger.debug(`<<< Exiting addCloseEventListener(callback=${callback})`);
        },
        removeCloseEventListener: function (callback) {
			logger.debug(`>>> Entering removeCloseEventListener(callback=${callback})`);
            $(document).off('editbar:onClose', callback);
			logger.debug(`<<< Exiting removeCloseEventListener(callback=${callback})`);
        }
    };
    return eventEmitter;
});