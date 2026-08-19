const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


(function ($) {
logger.debug('>>> Entering function()');

    window.LocalizationController = function (settings, root) {
        var that = this;

        this.showMissingKeys = settings.showMissingKeys || false;

		logger.debug(`function(): this.showMissingKeys → ${this.showMissingKeys}`);
        this.servicesFramework = $.ServicesFramework(settings.moduleId);

        // Actual table of localized strings and their values.    
        this.stringTable = {};
		logger.debug(`function(settings=${settings},root=${root}): this.showMissingKeys → ${this.showMissingKeys}`);

        // Load a localization string table for the specified view.
        this.loadTable = function () {
            var worked = false;
			logger.debug(`function(): this.stringTable → ${this.stringTable}`);

            var success = function (m) {
				logger.debug(`>>> Entering success(m=${m})`);
                $.each(m.Table,
                    function (index, entry) {
					logger.debug('<<< Exiting function()');
					logger.debug(`function(settings=${settings},root=${root}): this.stringTable → ${this.stringTable}`);
                        if (that.stringTable.hasOwnProperty(index) == false) {
                            that.stringTable[index] = entry;
                        }
                    });

                worked = true;
				logger.debug(`<<< Exiting success(m=${m})`);
            };

            var failure = function (m) {
				logger.debug(`>>> Entering failure(m=${m})`);
                console.log('Unable to load string translations: ' + (m.ErrorMessage || 'Unknown error'));
				logger.debug(`<<< Exiting failure(m=${m})`);
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

            return worked;
        };

        this.getString = function (key) {
            var value = that.stringTable[key];

            if ((value || '').length == 0) {
                if (that.showMissingKeys) {
                    return '[L]{0}'.replace("{0}", key);
                }

                return key;
            }

			logger.debug(`<<< Exiting function(key=${key})`);
            return value;
        };
	    
		this.loadTable();
    };
})(window.jQuery);