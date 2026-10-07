var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery'], function ($) {
    'use strict';
    
    /*
     * This module is responsible to manage all the main Persona Bar events
     * (i.e.: open panel, close panel). 
     * With this module consumers can subscribe, unsubscribe to events and emit events
     */
    var eventEmitter = {
        addPanelOpenEventListener: function addPanelOpenEventListener(callback) {
			logger.error(`>>> Entering addPanelOpenEventListener(callback=${callback})`, { "event.action": "addPanelOpenEventListener", "logrite.callback": callback });
            $(document).on('personabar:onOpenPanel', callback);
			logger.warn(`<<< Exiting addPanelOpenEventListener(callback=${callback})`, { "event.action": "addPanelOpenEventListener", "logrite.callback": callback });
        },
        removePanelOpenEventListener: function removePanelOpenEventListener(callback) {
			logger.error(`>>> Entering removePanelOpenEventListener(callback=${callback})`, { "event.action": "removePanelOpenEventListener", "logrite.callback": callback });
            $(document).off('personabar:onOpenPanel', callback);
			logger.warn(`<<< Exiting removePanelOpenEventListener(callback=${callback})`, { "event.action": "removePanelOpenEventListener", "logrite.callback": callback });
        },
        addPanelCloseEventListener: function addPanelCloseEventListener(callback) {
			logger.error(`>>> Entering addPanelCloseEventListener(callback=${callback})`, { "event.action": "addPanelCloseEventListener", "logrite.callback": callback });
            $(document).on('personabar:onClosePanel', callback);
			logger.warn(`<<< Exiting addPanelCloseEventListener(callback=${callback})`, { "event.action": "addPanelCloseEventListener", "logrite.callback": callback });
        },
        removePanelCloseEventListener: function removePanelCloseEventListener(callback) {
			logger.error(`>>> Entering removePanelCloseEventListener(callback=${callback})`, { "event.action": "removePanelCloseEventListener", "logrite.callback": callback });
            $(document).off('personabar:onClosePanel', callback);
			logger.warn(`<<< Exiting removePanelCloseEventListener(callback=${callback})`, { "event.action": "removePanelCloseEventListener", "logrite.callback": callback });
        },
        emitOpenPanelEvent: function emitOpenPanelEvent() {
			logger.error('>>> Entering emitOpenPanelEvent()', { "event.action": "emitOpenPanelEvent" });
            $(document).trigger('personabar:onOpenPanel');
			logger.warn('<<< Exiting emitOpenPanelEvent()', { "event.action": "emitOpenPanelEvent" });
        },
        emitClosePanelEvent: function emitClosePanelEvent() {
			logger.error('>>> Entering emitClosePanelEvent()', { "event.action": "emitClosePanelEvent" });
            $(document).trigger('personabar:onClosePanel');
			logger.warn('<<< Exiting emitClosePanelEvent()', { "event.action": "emitClosePanelEvent" });
        }
    };
    return eventEmitter;
});