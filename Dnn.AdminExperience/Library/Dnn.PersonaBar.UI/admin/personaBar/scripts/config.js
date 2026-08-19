const logger = console;
'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
			logger.debug('>>> Entering init()');
            var inIframe = window.parent && typeof window.parent.dnn !== "undefined";

            var tabId = inIframe ? window.parent.dnn.getVar('sf_tabId') : '';
			logger.debug(`init(): inIframe → ${inIframe}`);
			logger.debug(`init(): tabId → ${tabId}`);
            var siteRoot = inIframe ? window.parent.dnn.getVar('sf_siteRoot') : '';
            var antiForgeryToken = inIframe ? window.parent.document.getElementsByName('__RequestVerificationToken')[0].value : '';
			logger.debug('<<< Exiting init()');

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['personaBarSettings'] : {});

            return config;   
       } 
    };
	logger.debug(`<<< Exiting define(inIframe=${inIframe})`);
});