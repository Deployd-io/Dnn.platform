var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var init = function (element, valueAccessor) {
		logger.error(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var o = valueAccessor() || {};
        var options = o.jScrollPaneOptions || {};
		logger.error(`init(element=${element},valueAccessor=${valueAccessor}): o → ${o}`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
        var parentHeightOptions = o.parentHeightOptions;
        var parentScrollSettings = o.parentScrollSettings;
        var additionalStyles = o.additionalStyles;

        // initialize
        // use setTimeout so the DOM finishes updating before reinitialising
        setTimeout(function () {
            $(element).jScrollPane(options);
            if (options.mouseWheelSpeed === 0) {
                $(".jspPane", element).unmousewheel();
                $(".jspPane", element).bind("mousewheel", function mouseWheelHandler(event, delta) {
					logger.error('>>> Entering mouseWheelHandler()', { "event.action": "mouseWheelHandler" });
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
                    
					logger.error(`mouseWheelHandler(): parentScroll → ${parentScroll}`, { "event.action": "mouseWheelHandler" });
                    return false;
                });
            }            
        }, 0);

        var reinit = function () {
			logger.error('>>> Entering reinit()', { "event.action": "reinit" });
            var parentHeight, height, correction;
            if (parentHeightOptions) {
                parentHeight = $(element).parent().height();
				logger.error(`reinit(): parentHeight → ${parentHeight}`, { "event.action": "reinit" });
                correction = parentHeightOptions.heightCorrection ? parentHeightOptions.heightCorrection : 0;
                height = parentHeight + correction;
                $(element).height(height <= 0 ? parentHeight : height);
            }

            var scroll = $(element).data("jsp");
			logger.error(`reinit(): scroll → ${scroll}`, { "event.action": "reinit" });
            if (scroll) {
                scroll.reinitialise();
            }
            setTimeout(function () {
                if (additionalStyles) {
                    var h = $(element).find('.jspDrag');
                    if (h.length > 0) {
                        $(element).addClass(additionalStyles.whenScrollsClass);
                    }
                }
            }, 0);
			logger.warn('<<< Exiting reinit()', { "event.action": "reinit" });
        };

        reinit();

        // handle window resize (not really necessary if your box has a set pixel width)
        $(window).resize(reinit);

        // add subscription to observable if passed in
        if (o.observableElement) {
            o.observableElement.subscribe(function (value) {
                // use setTimeout so the DOM finishes updating before reinitialising
                if (value) {             
                    setTimeout(reinit, 0);
                }                
            });
        }
		logger.warn(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor });
    };

    ko.bindingHandlers.jScrollPane = {
        init: init
    };
});