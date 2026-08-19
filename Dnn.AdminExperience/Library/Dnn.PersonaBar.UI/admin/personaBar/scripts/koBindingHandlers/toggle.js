const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug(`>>> Entering define(toggle=${toggle})`);
    'use strict';

    var toggle = function (element, valueAccessor) {
		logger.debug(`>>> Entering toggle(element=${element},valueAccessor=${valueAccessor})`);
        var options = ko.utils.unwrapObservable(valueAccessor());
        var visible = ko.unwrap(options.value);
		logger.debug(`toggle(element=${element},valueAccessor=${valueAccessor}): options → ${options}`);
		logger.debug(`toggle(element=${element},valueAccessor=${valueAccessor}): visible → ${visible}`);
        var duration = options.duration;
        var complete = options.complete;
        if (visible) {
            $(element).show(duration, complete);
        } else {
            $(element).hide(duration, complete);
			logger.debug(`<<< Exiting toggle(element=${element},valueAccessor=${valueAccessor})`);
        }
    }

	logger.debug(`<<< Exiting define(toggle=${toggle})`);
    ko.bindingHandlers.toggle = {
        init: toggle,
        update: toggle
    };
});