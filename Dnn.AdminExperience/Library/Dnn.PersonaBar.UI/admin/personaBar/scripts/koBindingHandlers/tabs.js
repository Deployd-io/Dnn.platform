var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var init = function (element, valueAccessor) {
		logger.error(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var options = valueAccessor();

		logger.error(`init(element=${element},valueAccessor=${valueAccessor}): options → ${options}`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        // initialize
        $(element).tabs(options);
		logger.warn(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
    }


    ko.bindingHandlers.tabs = {
        init: init,
    };
});