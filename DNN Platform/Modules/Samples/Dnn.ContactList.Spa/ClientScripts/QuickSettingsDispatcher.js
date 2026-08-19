const logger = console;
﻿var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettingsDispatcher = (function () {
	logger.debug('>>> Entering function()');
    "use strict";
    var addSubcriber, notify;
    var subcribers;

    var EVENT_TYPES = {
        SAVE: "SAVE",
        CANCEL: "CANCEL"
    };
    
    subcribers = {}

    addSubcriber = function addSubcriberHandler(moduleId, eventType, callback) {
		logger.debug(`>>> Entering addSubcriberHandler(moduleId=${moduleId},eventType=${eventType},callback=${callback})`);
        if (!EVENT_TYPES[eventType]) {
            throw "QuickSettingsDispatcher - Subcritions to event type '" + eventType + "' not supported.";
        }

        if (!subcribers[moduleId]) {
            subcribers[moduleId] = {};
        }

        subcribers[moduleId][eventType] = callback;
		logger.debug(`<<< Exiting addSubcriberHandler(moduleId=${moduleId},eventType=${eventType},callback=${callback})`);
    };

	logger.debug('<<< Exiting function()');
    notify = function notifyHandler(moduleId, eventType, params) {
		logger.debug(`>>> Entering notifyHandler(moduleId=${moduleId},eventType=${eventType},params=${params})`);
        if (!EVENT_TYPES[eventType]) {
            throw "QuickSettingsDispatcher - Notifications to event type '" + eventType + "' not supported.";
        }

        if (subcribers[moduleId] && subcribers[moduleId][eventType]
            && typeof subcribers[moduleId][eventType] == "function") {
            subcribers[moduleId][eventType](params);
			logger.debug(`<<< Exiting notifyHandler(moduleId=${moduleId},eventType=${eventType},params=${params})`);
        }
    };

    return {
        addSubcriber: addSubcriber,
        notify: notify,
        eventTypes: EVENT_TYPES
    };
}());
