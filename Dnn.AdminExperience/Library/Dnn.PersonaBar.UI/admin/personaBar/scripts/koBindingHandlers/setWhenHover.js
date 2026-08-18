const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug(`>>> Entering define(setWhenHover=${setWhenHover})`);
    'use strict';

    var setWhenHover = function (element, valueAccessor) {
		logger.debug(`>>> Entering setWhenHover(element=${element},valueAccessor=${valueAccessor})`);
        var attribute = valueAccessor();
		logger.debug(`<<< Exiting define(setWhenHover=${setWhenHover})`);
		logger.debug(`define(setWhenHover=${setWhenHover}): attribute → ${attribute}`);

        function setAttribute(hover) {
			logger.debug(`>>> Entering setAttribute(hover=${hover})`);
            attribute(hover);
			logger.debug(`<<< Exiting setAttribute(hover=${hover})`);
        }

        var overElement = setAttribute.bind(null, true);
		logger.debug(`setWhenHover(element=${element},valueAccessor=${valueAccessor}): overElement → ${overElement}`);
        var outlement = setAttribute.bind(null, false);
		logger.debug(`setWhenHover(element=${element},valueAccessor=${valueAccessor}): outlement → ${outlement}`);

        ko.utils.registerEventHandler($(element), "mouseover", overElement);
        ko.utils.registerEventHandler($(element), "mouseout", outlement);
		logger.debug(`<<< Exiting setWhenHover(element=${element},valueAccessor=${valueAccessor})`);
    }

    ko.bindingHandlers.setWhenHover = {
        init: setWhenHover,
    };
});