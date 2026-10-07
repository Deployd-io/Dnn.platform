var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            var inIframe = window !== window.top && typeof window.top.dnn !== "undefined";

			logger.error(`init(): inIframe → ${inIframe}`, { "event.action": "init" });
            var tabId = inIframe ? window.top.dnn.getVar('sf_tabId') : '';
            var siteRoot = inIframe ? window.top.dnn.getVar('sf_siteRoot') : '';
			logger.error(`init(): tabId → ${tabId}`, { "event.action": "init" });
            var antiForgeryToken = inIframe ? window.top.document.getElementsByName('__RequestVerificationToken')[0].value : '';
			logger.warn('<<< Exiting init()', { "event.action": "init" });

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['editBarSettings'] : {});

            return config;   
       } 
    };
});