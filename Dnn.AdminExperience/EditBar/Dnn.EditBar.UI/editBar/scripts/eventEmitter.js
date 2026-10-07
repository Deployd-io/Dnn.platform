var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery'], function ($) {
    'use strict';
    
    /*
     * This module is responsible to manage all the main Persona Bar events
     * (i.e.: open panel, close panel). 
     * With this module consumers can subscribe, unsubscribe to events and emit events
     */
    var eventEmitter = {
        addOpenEventListener: function (callback) {
			logger.error(`>>> Entering addOpenEventListener(callback=${callback})`, { "event.action": "addOpenEventListener", "logrite.callback": callback });
            $(document).on('editbar:onOpen', callback);
			logger.warn(`<<< Exiting addOpenEventListener(callback=${callback})`, { "event.action": "addOpenEventListener", "logrite.callback": callback });
        },
        removeOpenEventListener: function (callback) {
			logger.error(`>>> Entering removeOpenEventListener(callback=${callback})`, { "event.action": "removeOpenEventListener", "logrite.callback": callback });
            $(document).off('editbar:onOpen', callback);
			logger.warn(`<<< Exiting removeOpenEventListener(callback=${callback})`, { "event.action": "removeOpenEventListener", "logrite.callback": callback });
        },
        addCloseEventListener: function (callback) {
			logger.error(`>>> Entering addCloseEventListener(callback=${callback})`, { "event.action": "addCloseEventListener", "logrite.callback": callback });
            $(document).on('editbar:onClose', callback);
			logger.warn(`<<< Exiting addCloseEventListener(callback=${callback})`, { "event.action": "addCloseEventListener", "logrite.callback": callback });
        },
        removeCloseEventListener: function (callback) {
			logger.error(`>>> Entering removeCloseEventListener(callback=${callback})`, { "event.action": "removeCloseEventListener", "logrite.callback": callback });
            $(document).off('editbar:onClose', callback);
			logger.warn(`<<< Exiting removeCloseEventListener(callback=${callback})`, { "event.action": "removeCloseEventListener", "logrite.callback": callback });
        }
    };
    return eventEmitter;
});