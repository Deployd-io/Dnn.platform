const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to update the width when an attribute is updated by taking the width of the nearest parent scrollpanel.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug('>>> Entering define()');
    'use strict';

    var setWidthFromParentScrollPaneWhen = function (element, valueAccessor) {
		logger.debug(`>>> Entering setWidthFromParentScrollPaneWhen(element=${element},valueAccessor=${valueAccessor})`);
        var attribute = valueAccessor();

		logger.debug(`setWidthFromParentScrollPaneWhen(element=${element},valueAccessor=${valueAccessor}): attribute → ${attribute}`);
        if (attribute) {
			logger.debug(`<<< Exiting setWidthFromParentScrollPaneWhen(element=${element},valueAccessor=${valueAccessor})`);
            attribute.subscribe(function () {
                var scroll = null;
                var parent = $(element);
				logger.debug(`subscribe(scroll=${scroll}): parent → ${parent}`);
                while (parent) {
                    scroll = parent.data("jsp");
                    if (scroll) {
                        $(element).width(scroll.getContentWidth());
                        break;
                    }
                    parent = parent.parent();
					logger.debug(`subscribe(scroll=${scroll}): scroll → ${scroll}`);
                }
            });
			logger.debug(`<<< Exiting subscribe(scroll=${scroll})`);
        }
    }

	logger.debug('<<< Exiting define()');
    ko.bindingHandlers.setWidthFromParentScrollPaneWhen = {
        init: setWidthFromParentScrollPaneWhen,
    };
});