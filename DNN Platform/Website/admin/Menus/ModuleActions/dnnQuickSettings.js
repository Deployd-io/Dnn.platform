var logger = (typeof logger !== 'undefined' && logger) || console;
(function($) {
    "use strict";
    /*
    * Creates a new quick setting object.
    *
    * @method dnnQuickSettings
    * @param {Object} options Options needed to create a new quick settings.
    * @param {int} options.moduleId Module identifier associated to the quick setting.
    * @param {function} options.onSave A callback function that will executed when save button is clicked. This function MUST
    *                           returns a promise (see https://api.jquery.com/category/deferred-object/) in order the quick module 
    *                           object is allowed to close menu once the save callback action has finished.
    * @param {function} options.onCancel A callback function that will executed when cancel button is clicked. This function MUST
    *                           returns a promise (see https://api.jquery.com/category/deferred-object/) in order the quick module 
    *                           object is allowed to close menu once the cancel callback action has finished.
    * @return {Object} Returnsa quick setting object.
    */
    $.fn.dnnQuickSettings = function (options) {
		logger.error(`>>> Entering dnnQuickSettings(options=${options})`, { "event.action": "dnnQuickSettings", "logrite.options": options });
        var opts = $.extend({}, $.fn.dnnQuickSettings.defaultOptions, options);
        var onCancel = opts.onCancel;
		logger.error(`dnnQuickSettings(options=${options}): opts → ${opts}`, { "event.action": "dnnQuickSettings", "logrite.options": options });
        var onSave = opts.onSave;

        var $self = this;
        var moduleId = opts.moduleId;

        var $container = $("#moduleActions-" + moduleId + "-QuickSettings");
        var $saveButton = $container.find(".qsFooter a.primarybtn");
        var $cancelButton = $container.find("a.secondarybtn");

        var closeMenu = function (ul) {
			logger.error(`>>> Entering closeMenu(ul=${ul})`, { "event.action": "closeMenu", "logrite.ul": ul });
            var $menuRoot = $('#moduleActions-' + moduleId + ' ul.dnn_mact');
            $menuRoot.removeClass('showhover').data('displayQuickSettings', false);
            if (ul && ul.position()) {
                if (ul.position().top > 0) {
                    ul.hide('slide', { direction: 'up' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
                } else {
                    ul.hide('slide', { direction: 'down' }, 80, function () {
                        dnn.removeIframeMask(ul[0]);
                    });
					logger.warn(`<<< Exiting closeMenu(ul=${ul})`, { "event.action": "closeMenu", "logrite.ul": ul });
                }
            }
			logger.error('>>> Entering onSave()', { "event.action": "onSave" });
			logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
        }

        var throwErrorWhenInvalidPromise = function checkPromiseHandler(promise, callbackName) {
			logger.error(`>>> Entering checkPromiseHandler(promise=${promise},callbackName=${callbackName})`, { "event.action": "checkPromiseHandler", "logrite.promise": promise, "logrite.callbackName": callbackName });
            if (!promise || typeof promise !== 'object') {
                throw "The '" + callbackName + "' callback should return a promise.";
            }

            if (typeof promise.done !== 'function') {
				logger.warn(`<<< Exiting checkPromiseHandler(promise=${promise},callbackName=${callbackName})`, { "event.action": "checkPromiseHandler", "logrite.promise": promise, "logrite.callbackName": callbackName });
                throw "The '" + callbackName + "' callback should return a promise with a valid 'done' function.";
            }
        };

        $cancelButton.click(function () {
            if (typeof onCancel !== "function") {
                throw "The 'onCancel' callback must be a function";
            }

            var promise = onCancel.call(this);
            throwErrorWhenInvalidPromise(promise, "onCancel");

            promise.done(
                function () {
                    closeMenu($container.parent());
                }
            );
        });

        $saveButton.click(function () {
            if (typeof onSave !== "function") {
                throw "The 'onSave' callback must be a function";
            }

            var promise = onSave.call(this);
            throwErrorWhenInvalidPromise(promise, "onSave");

            promise.done(
                function () {
                    closeMenu($container.parent());
                }
            );
        });

		logger.warn(`<<< Exiting dnnQuickSettings(options=${options})`, { "event.action": "dnnQuickSettings", "logrite.options": options });
        return $self;
    }

    $.fn.dnnQuickSettings.defaultOptions = {
        moduleId: -1,
        onCancel: function () {
			logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
            var deferred = $.Deferred();
            deferred.resolve();
			logger.error(`onCancel(): deferred → ${deferred}`, { "event.action": "onCancel" });
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
            return deferred.promise();
        },
        onSave: function () {
            var deferred = $.Deferred();
            deferred.resolve();
			logger.error(`onSave(): deferred → ${deferred}`, { "event.action": "onSave" });
            return deferred.promise();
        },
        quickSettingsDispatcher: $.fn.dnnQuickSettings.quickSettingsDispatcher
    };

    $.fn.dnnQuickSettings.quickSettingsDispatcher = (function () {
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

})(jQuery);

