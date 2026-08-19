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
        addPanelOpenEventListener: function addPanelOpenEventListener(callback) {
			logger.debug(`>>> Entering addPanelOpenEventListener(callback=${callback})`);
            $(document).on('personabar:onOpenPanel', callback);
			logger.debug(`<<< Exiting addPanelOpenEventListener(callback=${callback})`);
        },
        removePanelOpenEventListener: function removePanelOpenEventListener(callback) {
			logger.debug(`>>> Entering removePanelOpenEventListener(callback=${callback})`);
            $(document).off('personabar:onOpenPanel', callback);
			logger.debug(`<<< Exiting removePanelOpenEventListener(callback=${callback})`);
        },
        addPanelCloseEventListener: function addPanelCloseEventListener(callback) {
			logger.debug(`>>> Entering addPanelCloseEventListener(callback=${callback})`);
            $(document).on('personabar:onClosePanel', callback);
			logger.debug(`<<< Exiting addPanelCloseEventListener(callback=${callback})`);
        },
        removePanelCloseEventListener: function removePanelCloseEventListener(callback) {
			logger.debug(`>>> Entering removePanelCloseEventListener(callback=${callback})`);
            $(document).off('personabar:onClosePanel', callback);
			logger.debug(`<<< Exiting removePanelCloseEventListener(callback=${callback})`);
        },
        emitOpenPanelEvent: function emitOpenPanelEvent() {
			logger.debug('>>> Entering emitOpenPanelEvent()');
            $(document).trigger('personabar:onOpenPanel');
			logger.debug('<<< Exiting emitOpenPanelEvent()');
        },
        emitClosePanelEvent: function emitClosePanelEvent() {
			logger.debug('>>> Entering emitClosePanelEvent()');
            $(document).trigger('personabar:onClosePanel');
			logger.debug('<<< Exiting emitClosePanelEvent()');
        }
    };
    return eventEmitter;
});