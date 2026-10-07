var logger = (typeof logger !== 'undefined' && logger) || console;
var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettingsDispatcher = (function () {
    "use strict";
    var addSubcriber, notify;
    var subcribers;

    var EVENT_TYPES = {
        SAVE: "SAVE",
        CANCEL: "CANCEL"
    };
    
    subcribers = {}

    addSubcriber = function addSubcriberHandler(moduleId, eventType, callback) {
		logger.error(`>>> Entering addSubcriberHandler(moduleId=${moduleId},eventType=${eventType},callback=${callback})`, { "event.action": "addSubcriberHandler", "logrite.moduleId": moduleId, "logrite.eventType": eventType, "logrite.callback": callback });
        if (!EVENT_TYPES[eventType]) {
            throw "QuickSettingsDispatcher - Subcritions to event type '" + eventType + "' not supported.";
        }

        if (!subcribers[moduleId]) {
            subcribers[moduleId] = {};
        }

        subcribers[moduleId][eventType] = callback;
		logger.warn(`<<< Exiting addSubcriberHandler(moduleId=${moduleId},eventType=${eventType},callback=${callback})`, { "event.action": "addSubcriberHandler", "logrite.moduleId": moduleId, "logrite.eventType": eventType, "logrite.callback": callback });
    };

    notify = function notifyHandler(moduleId, eventType, params) {
		logger.error(`>>> Entering notifyHandler(moduleId=${moduleId},eventType=${eventType},params=${params})`, { "event.action": "notifyHandler", "logrite.moduleId": moduleId, "logrite.eventType": eventType, "logrite.params": params });
        if (!EVENT_TYPES[eventType]) {
            throw "QuickSettingsDispatcher - Notifications to event type '" + eventType + "' not supported.";
        }

        if (subcribers[moduleId] && subcribers[moduleId][eventType]
            && typeof subcribers[moduleId][eventType] == "function") {
            subcribers[moduleId][eventType](params);
			logger.warn(`<<< Exiting notifyHandler(moduleId=${moduleId},eventType=${eventType},params=${params})`, { "event.action": "notifyHandler", "logrite.moduleId": moduleId, "logrite.eventType": eventType, "logrite.params": params });
        }
    };

    return {
        addSubcriber: addSubcriber,
        notify: notify,
        eventTypes: EVENT_TYPES
    };
}());
