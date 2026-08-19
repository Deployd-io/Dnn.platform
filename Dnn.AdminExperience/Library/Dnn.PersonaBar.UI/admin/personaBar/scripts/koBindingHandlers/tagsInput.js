const logger = console;
﻿'use strict';
define([
        'jquery',
        'knockout',
        'dnn.jquery',
        'css!cssPath/tags-input.css'
],

function($, ko) {
	logger.debug(`>>> Entering function(ko=${ko})`);
    ko.bindingHandlers.tagsInput = {
        init: function (element, valueAccessor, allBindingsAccessor) {
			logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor},allBindingsAccessor=${allBindingsAccessor})`);
            var settings = allBindingsAccessor().settings || {};

            $(element).dnnTagsInput({
                width: settings.width || '100%',
                minInputWidth: settings.minInputWidth || '100%',
                defaultText: settings.placeholder || '',
                onAddTag: function () {
					logger.debug('>>> Entering onAddTag()');
                    valueAccessor()($(element).val());
					logger.debug('<<< Exiting onAddTag()');
                },
                onRemoveTag: function () {
					logger.debug('>>> Entering onRemoveTag()');
                    valueAccessor()($(element).val());
					logger.debug('<<< Exiting onRemoveTag()');
                }
            });
			logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor},allBindingsAccessor=${allBindingsAccessor})`);
        },
        update: function (element, valueAccessor) {
			logger.debug(`>>> Entering update(element=${element},valueAccessor=${valueAccessor})`);
            var value = ko.unwrap(valueAccessor());
            if ($(element).val() != value) {
				logger.debug(`update(element=${element},valueAccessor=${valueAccessor}): value → ${value}`);
                $(element).dnnImportTags(!value ? '' : value);    
				logger.debug(`<<< Exiting update(element=${element},valueAccessor=${valueAccessor})`);
            }            
        }
    };
	logger.debug(`<<< Exiting function(ko=${ko})`);
});
