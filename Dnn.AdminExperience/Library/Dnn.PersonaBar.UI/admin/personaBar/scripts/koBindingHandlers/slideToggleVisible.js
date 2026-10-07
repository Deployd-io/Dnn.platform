var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';
    ko.bindingHandlers.slideToggleVisible = {
        init: function (element, valueAccessor) {
			logger.error(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).show() : $(element).hide();
			logger.error(`init(element=${element},valueAccessor=${valueAccessor}): value → ${value}`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        },
        update: function (element, valueAccessor) {
			logger.error(`>>> Entering update(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "update", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).slideDown() : $(element).slideUp();
			logger.error(`update(element=${element},valueAccessor=${valueAccessor}): value → ${value}`, { "event.action": "update", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        }
    };
});