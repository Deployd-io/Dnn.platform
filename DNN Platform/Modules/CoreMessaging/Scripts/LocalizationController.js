var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


(function ($) {

    window.LocalizationController = function (settings, root) {
		logger.error(`>>> Entering LocalizationController(settings=${settings},root=${root})`, { "event.action": "LocalizationController", "logrite.settings": settings, "logrite.root": root });
        var that = this;

        this.showMissingKeys = settings.showMissingKeys || false;

		logger.debug(`LocalizationController(settings=${settings},root=${root}): this.showMissingKeys → ${this.showMissingKeys}`, { "event.action": "LocalizationController", "logrite.settings": settings, "logrite.root": root });
        this.servicesFramework = $.ServicesFramework(settings.moduleId);

        // Actual table of localized strings and their values.    
        this.stringTable = {};

		logger.debug(`LocalizationController(settings=${settings},root=${root}): this.stringTable → ${this.stringTable}`, { "event.action": "LocalizationController", "logrite.settings": settings, "logrite.root": root });
        // Load a localization string table for the specified view.
        this.loadTable = function () {
			logger.error('>>> Entering loadTable()', { "event.action": "loadTable" });
            var worked = false;

            var success = function (m) {
				logger.error(`>>> Entering success(m=${m})`, { "event.action": "success", "logrite.m": m });
                $.each(m.Table,
                    function (index, entry) {
                        if (that.stringTable.hasOwnProperty(index) == false) {
                            that.stringTable[index] = entry;
                        }
                    });

                worked = true;
				logger.warn(`<<< Exiting success(m=${m})`, { "event.action": "success", "logrite.m": m });
            };

            var failure = function (m) {
				logger.error(`>>> Entering failure(m=${m})`, { "event.action": "failure", "logrite.m": m });
                console.log('Unable to load string translations: ' + (m.ErrorMessage || 'Unknown error'));
				logger.warn(`<<< Exiting failure(m=${m})`, { "event.action": "failure", "logrite.m": m });
            };

            // This is a synchronous call instead of async so that we can guarantee the translations are loaded
            var params = {
                culture: settings.culture
            };
            
            $.ajax({
                type: 'get',
                url: root.servicesFramework.getServiceRoot('CoreMessaging') + 'Subscriptions/GetLocalizationTable',
                beforeSend: root.servicesFramework.setModuleHeaders,
                data: params,
                cache: false,
                async: false
            }).done(success).fail(failure);

			logger.warn('<<< Exiting loadTable()', { "event.action": "loadTable" });
            return worked;
        };

        this.getString = function (key) {
			logger.error(`>>> Entering getString(key=${key})`, { "event.action": "getString", "logrite.key": key });
            var value = that.stringTable[key];

            if ((value || '').length == 0) {
                if (that.showMissingKeys) {
                    return '[L]{0}'.replace("{0}", key);
                }

                return key;
            }

			logger.warn(`<<< Exiting getString(key=${key})`, { "event.action": "getString", "logrite.key": key });
            return value;
        };
	    
		this.loadTable();
		logger.warn(`<<< Exiting LocalizationController(settings=${settings},root=${root})`, { "event.action": "LocalizationController", "logrite.settings": settings, "logrite.root": root });
    };
})(window.jQuery);