const logger = console;
﻿define(['./eventEmitter'], function (eventEmitter) {
	logger.debug('>>> Entering define()');
    'use strict';
    
	logger.debug('<<< Exiting define()');
    /*
     * This module is responsible to expose an Interface for the Persona Bar.
     * This interface is exposed to the parent window (the main site) in order
     * to allow easy interaction and integration between the PB and the site (i.e.: Edit Bar).
     * Consumers can subscribe/unsubscribe to events like open, close panels.
     */
    return function Gateway(util) {
	logger.debug(`>>> Entering Gateway(util=${util})`);
        /**
		logger.debug(`<<< Exiting Gateway(util=${util})`);
        * Subscribes to the panel open event
        *
        * @method onPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelOpen = function onPanelOpen(callback) {
			logger.debug(`>>> Entering onPanelOpen(callback=${callback})`);
            eventEmitter.addPanelOpenEventListener(callback);
			logger.debug(`<<< Exiting onPanelOpen(callback=${callback})`);
        };

        /**
        * Unsubscribes to the panel open event
        *
        * @method offPanelOpen
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelOpen = function offPanelOpen(callback) {
			logger.debug(`>>> Entering offPanelOpen(callback=${callback})`);
            eventEmitter.removePanelOpenEventListener(callback);
			logger.debug(`<<< Exiting offPanelOpen(callback=${callback})`);
        };

        /**
        * Subscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.onPanelClose = function onPanelClose(callback) {
			logger.debug(`>>> Entering onPanelClose(callback=${callback})`);
            eventEmitter.addPanelCloseEventListener(callback);
			logger.debug(`<<< Exiting onPanelClose(callback=${callback})`);
        };

        /**
        * Unsubscribes to the panel close event
        *
        * @method onPanelClose
        * @param {Function} callback callback called when the event happens
        */
        this.offPanelClose = function offPanelClose(callback) {
			logger.debug(`>>> Entering offPanelClose(callback=${callback})`);
            eventEmitter.removePanelCloseEventListener(callback);
			logger.debug(`<<< Exiting offPanelClose(callback=${callback})`);
        };

        /**
        * Opens the persona bar panel
        *
        * @method openPanel
        * @param {String} path identifier of the path
        * @param {Object} params parameters for the panel
        */
        this.openPanel = function openPanel(identifier, params) {
			logger.debug(`>>> Entering openPanel(identifier=${identifier},params=${params})`);
            util.loadPanel(identifier, params);
			logger.debug(`<<< Exiting openPanel(identifier=${identifier},params=${params})`);
        };

        /**
        * Closes the persona bar panel
        *
        * @method closePanel
        */
        this.closePanel = function closePanel() {
			logger.debug('>>> Entering closePanel()');
            util.closePersonaBar();
			logger.debug('<<< Exiting closePanel()');
        };
    };
});