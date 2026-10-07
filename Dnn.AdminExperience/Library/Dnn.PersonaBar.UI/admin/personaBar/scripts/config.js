var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            var inIframe = window.parent && typeof window.parent.dnn !== "undefined";

			logger.error(`init(): inIframe → ${inIframe}`, { "event.action": "init" });
            var tabId = inIframe ? window.parent.dnn.getVar('sf_tabId') : '';
            var siteRoot = inIframe ? window.parent.dnn.getVar('sf_siteRoot') : '';
			logger.error(`init(): tabId → ${tabId}`, { "event.action": "init" });
            var antiForgeryToken = inIframe ? window.parent.document.getElementsByName('__RequestVerificationToken')[0].value : '';
			logger.warn('<<< Exiting init()', { "event.action": "init" });

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['personaBarSettings'] : {});

            return config;   
       } 
    };
});