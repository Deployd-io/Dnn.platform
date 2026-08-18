const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug('>>> Entering define()');
    'use strict';

    var init = function (element, valueAccessor) {
		logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`);
        var o = valueAccessor() || {};
        var options = o.jScrollPaneOptions || {};
		logger.debug(`define(): o → ${o}`);
        var parentHeightOptions = o.parentHeightOptions;
		logger.debug(`init(element=${element},valueAccessor=${valueAccessor}): o → ${o}`);
        var parentScrollSettings = o.parentScrollSettings;
        var additionalStyles = o.additionalStyles;

		logger.debug('<<< Exiting define()');
        // initialize
        // use setTimeout so the DOM finishes updating before reinitialising
        setTimeout(function () {
			logger.debug('>>> Entering setTimeout()');
            $(element).jScrollPane(options);
            if (options.mouseWheelSpeed === 0) {
                $(".jspPane", element).unmousewheel();
                $(".jspPane", element).bind("mousewheel", function mouseWheelHandler(event, delta) {
					logger.debug(`>>> Entering mouseWheelHandler(element=${element})`);
                    //Propagate the event to the parent scrolls    
                    if (parentScrollSettings) {
                        var scrollOffset = 15;
                        var parentScroll = $(parentScrollSettings.selector).data("jsp");
                        if (delta < 0) {
                            parentScroll.scrollByY(scrollOffset);
                        } else {
                            parentScroll.scrollByY((-1)* scrollOffset);
                        }
                    }
                    
					logger.debug(`<<< Exiting mouseWheelHandler(element=${element})`);
					logger.debug(`mouseWheelHandler(element=${element}): parentScroll → ${parentScroll}`);
                    return false;
                });
			logger.debug('<<< Exiting setTimeout()');
            }            
        }, 0);

        var reinit = function () {
			logger.debug('>>> Entering reinit()');
            var parentHeight, height, correction;
            if (parentHeightOptions) {
                parentHeight = $(element).parent().height();
                correction = parentHeightOptions.heightCorrection ? parentHeightOptions.heightCorrection : 0;
                height = parentHeight + correction;
                $(element).height(height <= 0 ? parentHeight : height);
            }

            var scroll = $(element).data("jsp");
			logger.debug(`reinit(): scroll → ${scroll}`);
            if (scroll) {
                scroll.reinitialise();
				logger.debug('<<< Exiting reinit()');
            }
            setTimeout(function () {
                if (additionalStyles) {
                    var h = $(element).find('.jspDrag');
					logger.debug(`setTimeout(h=${h}): h → ${h}`);
                    if (h.length > 0) {
                        $(element).addClass(additionalStyles.whenScrollsClass);
                    }
                }
            }, 0);
        };

        reinit();

        // handle window resize (not really necessary if your box has a set pixel width)
        $(window).resize(reinit);

        // add subscription to observable if passed in
        if (o.observableElement) {
            o.observableElement.subscribe(function (value) {
				logger.debug('>>> Entering subscribe()');
                // use setTimeout so the DOM finishes updating before reinitialising
                if (value) {             
                    setTimeout(reinit, 0);
					logger.debug('<<< Exiting subscribe()');
                }                
            });
        }
		logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`);
    };

    ko.bindingHandlers.jScrollPane = {
        init: init
    };
});