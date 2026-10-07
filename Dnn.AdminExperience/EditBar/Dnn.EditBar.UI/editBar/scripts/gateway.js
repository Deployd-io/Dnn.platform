var logger = (typeof logger !== 'undefined' && logger) || console;
define(['./eventEmitter'], function (eventEmitter) {
    'use strict';
    
    /*
     * This module is responsible to expose an Interface for the Persona Bar.
     * This interface is exposed to the parent window (the main site) in order
     * to allow easy interaction and integration between the PB and the site (i.e.: Edit Bar).
     * Consumers can subscribe/unsubscribe to events like open, close panels.
     */
    return function Gateway(util) {
	logger.error(`>>> Entering Gateway(util=${util})`, { "event.action": "Gateway", "logrite.util": util });
        /**
        * Subscribes to the edit bar open event
        *
        * @method onOpen
        * @param {Function} callback callback called when the event happens
        */
        this.onOpen = function (callback) {
			logger.error(`>>> Entering onOpen(callback=${callback})`, { "event.action": "onOpen", "logrite.callback": callback });
            eventEmitter.addOpenEventListener(callback);
			logger.warn(`<<< Exiting onOpen(callback=${callback})`, { "event.action": "onOpen", "logrite.callback": callback });
        };

        /**
        * Unsubscribes to the edit bar open event
        *
        * @method offOpen
        * @param {Function} callback callback called when the event happens
        */
        this.offOpen = function (callback) {
			logger.error(`>>> Entering offOpen(callback=${callback})`, { "event.action": "offOpen", "logrite.callback": callback });
            eventEmitter.removeOpenEventListener(callback);
			logger.warn(`<<< Exiting offOpen(callback=${callback})`, { "event.action": "offOpen", "logrite.callback": callback });
        };

        /**
        * Subscribes to the edit bar close event
        *
        * @method onClose
        * @param {Function} callback callback called when the event happens
        */
        this.onClose = function (callback) {
			logger.error(`>>> Entering onClose(callback=${callback})`, { "event.action": "onClose", "logrite.callback": callback });
            eventEmitter.addCloseEventListener(callback);
			logger.warn(`<<< Exiting onClose(callback=${callback})`, { "event.action": "onClose", "logrite.callback": callback });
        };

        /**
        * Unsubscribes to the edit bar close event
        *
        * @method offClose
        * @param {Function} callback callback called when the event happens
        */
        this.offClose = function (callback) {
			logger.error(`>>> Entering offClose(callback=${callback})`, { "event.action": "offClose", "logrite.callback": callback });
            eventEmitter.removeCloseEventListener(callback);
			logger.warn(`<<< Exiting offClose(callback=${callback})`, { "event.action": "offClose", "logrite.callback": callback });
        };

        /**
        * trigger menu item's action
        *
        * @method action
        * @param {String} path identifier of the path
        * @param {Object} params parameters for the panel
        */
        this.action = function (menuName, actionName, params) {
			logger.error(`>>> Entering action(menuName=${menuName},actionName=${actionName},params=${params})`, { "event.action": "action", "logrite.menuName": menuName, "logrite.actionName": actionName, "logrite.params": params });
            util.callAction(menuName, actionName, params);
			logger.warn(`<<< Exiting action(menuName=${menuName},actionName=${actionName},params=${params})`, { "event.action": "action", "logrite.menuName": menuName, "logrite.actionName": actionName, "logrite.params": params });
        };
    };
});