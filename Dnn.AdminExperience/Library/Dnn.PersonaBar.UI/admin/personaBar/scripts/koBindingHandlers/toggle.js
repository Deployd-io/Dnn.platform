var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var toggle = function (element, valueAccessor) {
		logger.error(`>>> Entering toggle(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "toggle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var options = ko.utils.unwrapObservable(valueAccessor());
        var visible = ko.unwrap(options.value);
		logger.error(`toggle(element=${element},valueAccessor=${valueAccessor}): options → ${options}`, { "event.action": "toggle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
		logger.error(`toggle(element=${element},valueAccessor=${valueAccessor}): visible → ${visible}`, { "event.action": "toggle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var duration = options.duration;
        var complete = options.complete;
        if (visible) {
            $(element).show(duration, complete);
        } else {
            $(element).hide(duration, complete);
			logger.warn(`<<< Exiting toggle(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "toggle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        }
    }

    ko.bindingHandlers.toggle = {
        init: toggle,
        update: toggle
    };
});