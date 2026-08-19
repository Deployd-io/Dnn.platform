const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug(`>>> Entering define(init=${init})`);
    'use strict';

    var init = function (element, valueAccessor) {
		logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`);
        var options = valueAccessor();

		logger.debug(`init(element=${element},valueAccessor=${valueAccessor}): options → ${options}`);
        // initialize
        $(element).tabs(options);
		logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`);
    }


	logger.debug(`<<< Exiting define(init=${init})`);
    ko.bindingHandlers.tabs = {
        init: init,
    };
});