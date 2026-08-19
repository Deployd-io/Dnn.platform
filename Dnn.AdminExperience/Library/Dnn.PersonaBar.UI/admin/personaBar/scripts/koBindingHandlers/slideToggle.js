const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function($, ko) {
	logger.debug(`>>> Entering define(slideToogle=${slideToogle})`);
    'use strict';
    
    var slideToogle = function (element, valueAccessor) {
		logger.debug(`>>> Entering slideToogle(element=${element},valueAccessor=${valueAccessor})`);
        var value = valueAccessor();
        $(element).slideToggle(ko.unwrap(value));
		logger.debug(`slideToogle(element=${element},valueAccessor=${valueAccessor}): value → ${value}`);
		logger.debug(`<<< Exiting slideToogle(element=${element},valueAccessor=${valueAccessor})`);
    }

	logger.debug(`<<< Exiting define(slideToogle=${slideToogle})`);
    ko.bindingHandlers.slideToggle = {
        init: slideToogle,
        update: slideToogle
    };
});