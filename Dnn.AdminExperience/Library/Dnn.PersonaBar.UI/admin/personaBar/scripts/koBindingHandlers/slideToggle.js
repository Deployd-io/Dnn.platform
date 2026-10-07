var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function($, ko) {
    'use strict';
    
    var slideToogle = function (element, valueAccessor) {
		logger.error(`>>> Entering slideToogle(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "slideToogle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var value = valueAccessor();
        $(element).slideToggle(ko.unwrap(value));
		logger.error(`slideToogle(element=${element},valueAccessor=${valueAccessor}): value → ${value}`, { "event.action": "slideToogle", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
    }

    ko.bindingHandlers.slideToggle = {
        init: slideToogle,
        update: slideToogle
    };
});