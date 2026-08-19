const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug('>>> Entering define()');
    'use strict';
    ko.bindingHandlers.slideToggleVisible = {
        init: function (element, valueAccessor) {
			logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`);
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).show() : $(element).hide();
			logger.debug(`init(element=${element},valueAccessor=${valueAccessor}): value → ${value}`);
			logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`);
        },
        update: function (element, valueAccessor) {
			logger.debug(`>>> Entering update(element=${element},valueAccessor=${valueAccessor})`);
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).slideDown() : $(element).slideUp();
			logger.debug(`update(element=${element},valueAccessor=${valueAccessor}): value → ${value}`);
			logger.debug(`<<< Exiting update(element=${element},valueAccessor=${valueAccessor})`);
        }
    };
	logger.debug('<<< Exiting define()');
});