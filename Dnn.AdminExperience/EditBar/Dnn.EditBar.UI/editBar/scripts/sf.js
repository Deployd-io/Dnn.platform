var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
	
	return {
	    init: function (siteRoot, tabId, antiForgeryToken) {
		    return {
				moduleRoot: 'editBar/common',
				controller: '',
				antiForgeryToken: antiForgeryToken,
				setHeaders: function(xhr){
					logger.error(`>>> Entering setHeaders(xhr=${xhr})`, { "event.action": "setHeaders", "logrite.xhr": xhr });
					if(tabId){
						xhr.setRequestHeader('TabId', tabId);
					}
					
					if(antiForgeryToken){
						xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
						logger.warn(`<<< Exiting setHeaders(xhr=${xhr})`, { "event.action": "setHeaders", "logrite.xhr": xhr });
					}
				},		
				getServiceRoot: function(){
					logger.error('>>> Entering getServiceRoot()', { "event.action": "getServiceRoot" });
					var self = this;
					logger.warn('<<< Exiting getServiceRoot()', { "event.action": "getServiceRoot" });
					return siteRoot + 'DesktopModules/' + self.moduleRoot +'/API/';
				},		
				getSiteRoot: function(){
					logger.error('>>> Entering getSiteRoot()', { "event.action": "getSiteRoot" });
					logger.warn('<<< Exiting getSiteRoot()', { "event.action": "getSiteRoot" });
					return siteRoot;
				},
				rawCall: function(httpMethod, url, params, success, failure, loading, beforeSend, sync, silence){
					var beforeCallback;
					var self = this;
					if(typeof beforeSend === 'function'){
						beforeCallback = function(xhr){
							logger.error(`>>> Entering beforeCallback(xhr=${xhr})`, { "event.action": "beforeCallback", "logrite.xhr": xhr });
							self.setHeaders(xhr);
							logger.warn(`<<< Exiting beforeCallback(xhr=${xhr})`, { "event.action": "beforeCallback", "logrite.xhr": xhr });
							return beforeSend(xhr);
						};
					}
					else{
						beforeCallback = self.setHeaders;
					}
					
					var options = {
						url: url,
						beforeSend: beforeCallback,
						type: httpMethod,
						async: sync === false,
						success: function(d){
							logger.error(`>>> Entering success(d=${d})`, { "event.action": "success", "logrite.d": d });
							if(typeof loading === 'function'){
								loading(false);
							}
							
							if(typeof success === 'function'){
								success(d || {});
								logger.warn(`<<< Exiting success(d=${d})`, { "event.action": "success", "logrite.d": d });
							}
						},
						error: function(xhr, status, err){
							logger.error(`>>> Entering error(xhr=${xhr},status=${status},err=${err})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.err": err });
							if(typeof loading === 'function'){
								loading(false);
							}
							
							if(typeof failure === 'function'){
								if(xhr){
									failure(xhr, err);
								}
								else{
									failure(null, 'Unknown error');
									logger.warn(`<<< Exiting error(xhr=${xhr},status=${status},err=${err})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.err": err });
								}
							}
						}
					};
					
					if (httpMethod === 'GET') {
						options.data = params;
					} else {
						options.contentType = 'application/json; charset=UTF-8';
						options.data = JSON.stringify(params);
						options.dataType = 'json';
					}

					if (typeof loading === 'function') {
						loading(true);
					}
				    
					logger.warn(`<<< Exiting rawCall(httpMethod=${httpMethod},url=${url},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend},sync=${sync},silence=${silence})`, { "event.action": "rawCall", "logrite.httpMethod": httpMethod, "logrite.url": url, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend, "logrite.sync": sync, "logrite.silence": silence });
					return $.ajax(options);
				},
				
				call: function(httpMethod, method, params, success, failure, loading, beforeSend, sync, silence){
					var self = this;
					var url = self.getServiceRoot() + self.controller + '/' + method;

                    // Reset url default values
					self.moduleRoot = 'editBar/common';
					self.controller = '';

					logger.warn(`<<< Exiting call(httpMethod=${httpMethod},method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend},sync=${sync},silence=${silence})`, { "event.action": "call", "logrite.httpMethod": httpMethod, "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend, "logrite.sync": sync, "logrite.silence": silence });
					return this.rawCall(httpMethod, url, params, success, failure, loading, beforeSend, sync, silence);
				},
				
				post: function (method, params, success, failure, loading, beforeSend) {
					logger.error(`>>> Entering post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "post", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					logger.warn(`<<< Exiting post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "post", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, false);
				},
				
				postsilence: function (method, params, success, failure, loading, beforeSend) {
					logger.error(`>>> Entering postsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "postsilence", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					logger.warn(`<<< Exiting postsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "postsilence", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
				    return this.call('POST', method, params, success, failure, loading, beforeSend, false, true);
				},

				get: function (method, params, success, failure, loading, beforeSend) {
					logger.error(`>>> Entering get(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "get", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					logger.warn(`<<< Exiting get(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "get", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					return this.call('GET', method, params, success, failure, loading, beforeSend, false, false);
				},
				
				getsilence: function (method, params, success, failure, loading, beforeSend) {
					logger.error(`>>> Entering getsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`, { "event.action": "getsilence", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading, "logrite.beforeSend": beforeSend });
					logger.warn(`<<< Exiting init(siteRoot=${siteRoot},tabId=${tabId},antiForgeryToken=<redacted>)`, { "event.action": "init", "logrite.siteRoot": siteRoot, "logrite.tabId": tabId });
				    return this.call('GET', method, params, success, failure, loading, beforeSend, false, true);
				}	
			};
		}
	};	
});