const logger = console;
'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
			logger.debug('>>> Entering init()');
            var inIframe = window !== window.top && typeof window.top.dnn !== "undefined";

            var tabId = inIframe ? window.top.dnn.getVar('sf_tabId') : '';
			logger.debug(`init(): inIframe → ${inIframe}`);
			logger.debug(`init(): tabId → ${tabId}`);
            var siteRoot = inIframe ? window.top.dnn.getVar('sf_siteRoot') : '';
            var antiForgeryToken = inIframe ? window.top.document.getElementsByName('__RequestVerificationToken')[0].value : '';
			logger.debug('<<< Exiting init()');

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['editBarSettings'] : {});

            return config;   
       } 
    };
	logger.debug(`<<< Exiting define(inIframe=${inIframe})`);
});