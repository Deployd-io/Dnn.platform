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
        * Subscribes to the panel open event
        *
        * @method onPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelOpen = function onPanelOpen(callback) {
			logger.error(`>>> Entering onPanelOpen(callback=${callback})`, { "event.action": "onPanelOpen", "logrite.callback": callback });
            eventEmitter.addPanelOpenEventListener(callback);
			logger.warn(`<<< Exiting onPanelOpen(callback=${callback})`, { "event.action": "onPanelOpen", "logrite.callback": callback });
        };

        /**
        * Unsubscribes to the panel open event
        *
        * @method offPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelOpen = function offPanelOpen(callback) {
			logger.error(`>>> Entering offPanelOpen(callback=${callback})`, { "event.action": "offPanelOpen", "logrite.callback": callback });
            eventEmitter.removePanelOpenEventListener(callback);
			logger.warn(`<<< Exiting offPanelOpen(callback=${callback})`, { "event.action": "offPanelOpen", "logrite.callback": callback });
        };

        /**
        * Subscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelClose = function onPanelClose(callback) {
			logger.error(`>>> Entering onPanelClose(callback=${callback})`, { "event.action": "onPanelClose", "logrite.callback": callback });
            eventEmitter.addPanelCloseEventListener(callback);
			logger.warn(`<<< Exiting onPanelClose(callback=${callback})`, { "event.action": "onPanelClose", "logrite.callback": callback });
        };

        /**
        * Unsubscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelClose = function offPanelClose(callback) {
			logger.error(`>>> Entering offPanelClose(callback=${callback})`, { "event.action": "offPanelClose", "logrite.callback": callback });
            eventEmitter.removePanelCloseEventListener(callback);
			logger.warn(`<<< Exiting offPanelClose(callback=${callback})`, { "event.action": "offPanelClose", "logrite.callback": callback });
        };

        /**
        * Opens the persona bar panel
        *
        * @method openPanel
        * @param {String} path identifier of the path
        * @param {Object} params parameters for the panel
        */
        this.openPanel = function openPanel(identifier, params) {
			logger.error(`>>> Entering openPanel(identifier=${identifier},params=${params})`, { "event.action": "openPanel", "logrite.identifier": identifier, "logrite.params": params });
            util.loadPanel(identifier, params);
			logger.warn(`<<< Exiting openPanel(identifier=${identifier},params=${params})`, { "event.action": "openPanel", "logrite.identifier": identifier, "logrite.params": params });
        };

        /**
        * Closes the persona bar panel
        *
        * @method closePanel
        */
        this.closePanel = function closePanel() {
			logger.error('>>> Entering closePanel()', { "event.action": "closePanel" });
            util.closePersonaBar();
			logger.warn('<<< Exiting closePanel()', { "event.action": "closePanel" });
        };
    };
});