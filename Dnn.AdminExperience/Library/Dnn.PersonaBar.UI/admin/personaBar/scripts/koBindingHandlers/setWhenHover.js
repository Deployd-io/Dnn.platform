var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var setWhenHover = function (element, valueAccessor) {
		logger.error(`>>> Entering setWhenHover(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "setWhenHover", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var attribute = valueAccessor();

		logger.error(`setWhenHover(element=${element},valueAccessor=${valueAccessor}): attribute → ${attribute}`, { "event.action": "setWhenHover", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        function setAttribute(hover) {
			logger.error(`>>> Entering setAttribute(hover=${hover})`, { "event.action": "setAttribute", "logrite.hover": hover });
            attribute(hover);
			logger.warn(`<<< Exiting setAttribute(hover=${hover})`, { "event.action": "setAttribute", "logrite.hover": hover });
        }

        var overElement = setAttribute.bind(null, true);
		logger.error(`setWhenHover(element=${element},valueAccessor=${valueAccessor}): overElement → ${overElement}`, { "event.action": "setWhenHover", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var outlement = setAttribute.bind(null, false);
		logger.error(`setWhenHover(element=${element},valueAccessor=${valueAccessor}): outlement → ${outlement}`, { "event.action": "setWhenHover", "logrite.element": element, "logrite.valueAccessor": valueAccessor });

        ko.utils.registerEventHandler($(element), "mouseover", overElement);
        ko.utils.registerEventHandler($(element), "mouseout", outlement);
		logger.warn(`<<< Exiting setWhenHover(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "setWhenHover", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
    }

    ko.bindingHandlers.setWhenHover = {
        init: setWhenHover,
    };
});