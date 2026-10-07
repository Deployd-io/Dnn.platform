var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define([
        'jquery',
        'knockout',
        'dnn.jquery',
        'css!cssPath/tags-input.css'
],

function($, ko) {
    ko.bindingHandlers.tagsInput = {
        init: function (element, valueAccessor, allBindingsAccessor) {
			logger.error(`>>> Entering init(element=${element},valueAccessor=${valueAccessor},allBindingsAccessor=${allBindingsAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor, "logrite.allBindingsAccessor": allBindingsAccessor });
            var settings = allBindingsAccessor().settings || {};

            $(element).dnnTagsInput({
                width: settings.width || '100%',
                minInputWidth: settings.minInputWidth || '100%',
                defaultText: settings.placeholder || '',
                onAddTag: function () {
					logger.error('>>> Entering onAddTag()', { "event.action": "onAddTag" });
                    valueAccessor()($(element).val());
					logger.warn('<<< Exiting onAddTag()', { "event.action": "onAddTag" });
                },
                onRemoveTag: function () {
					logger.error('>>> Entering onRemoveTag()', { "event.action": "onRemoveTag" });
                    valueAccessor()($(element).val());
					logger.warn('<<< Exiting onRemoveTag()', { "event.action": "onRemoveTag" });
                }
            });
			logger.warn(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor},allBindingsAccessor=${allBindingsAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor, "logrite.allBindingsAccessor": allBindingsAccessor });
        },
        update: function (element, valueAccessor) {
			logger.error(`>>> Entering update(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "update", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
            var value = ko.unwrap(valueAccessor());
            if ($(element).val() != value) {
				logger.error(`update(element=${element},valueAccessor=${valueAccessor}): value → ${value}`, { "event.action": "update", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
                $(element).dnnImportTags(!value ? '' : value);    
				logger.warn(`<<< Exiting update(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "update", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
            }            
        }
    };
});
