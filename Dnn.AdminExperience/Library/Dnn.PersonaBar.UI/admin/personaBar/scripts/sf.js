const logger = console;
'use strict';
define(['jquery', 'main/loader'], function ($, loader) {
logger.debug('>>> Entering define()');

	return {
		init: function (siteRoot, tabId, antiForgeryToken) {
			return {
				moduleRoot: 'personaBar',
				controller: '',
				antiForgeryToken: antiForgeryToken,
				setHeaders: function (xhr) {
					logger.debug(`>>> Entering setHeaders(xhr=${xhr})`);
					if (tabId) {
						xhr.setRequestHeader('TabId', tabId);
					}

					if (antiForgeryToken) {
						xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
						logger.debug(`<<< Exiting setHeaders(xhr=${xhr})`);
					}
				},
				getServiceRoot: function () {
					logger.debug('>>> Entering getServiceRoot()');
					var self = this;
					logger.debug('<<< Exiting getServiceRoot()');
					return siteRoot + 'API/' + self.moduleRoot + '/';
				},
				getSiteRoot: function () {
					logger.debug('>>> Entering getSiteRoot()');
					logger.debug('<<< Exiting getSiteRoot()');
					return siteRoot;
				},
				rawCall: function (httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile) {
					var beforeCallback;
					var self = this;
					if (typeof beforeSend === 'function') {
						beforeCallback = function (xhr) {
							logger.debug(`>>> Entering function(xhr=${xhr})`);
							self.setHeaders(xhr);
							logger.debug(`<<< Exiting function(xhr=${xhr})`);
							return beforeSend(xhr);
						};
					}
					else {
						beforeCallback = self.setHeaders;
					}

					var options = {
						url: url,
						beforeSend: beforeCallback,
						type: httpMethod,
						async: sync === false,
						success: function (d) {
							logger.debug(`>>> Entering success(d=${d})`);
							if (!silence) loader.stopLoading();
							if (typeof loading === 'function') {
								loading(false);
							}

							if (typeof success === 'function') {
								success(d || {});
								logger.debug(`<<< Exiting success(d=${d})`);
							}
						},
						error: function (xhr, status, err) {
							logger.debug(`>>> Entering error(xhr=${xhr},status=${status},err=${err})`);
							if (!silence) loader.stopLoading(!(httpMethod !== "GET")); //only show error if GET
							if (typeof loading === 'function') {
								loading(false);
							}

							if (typeof failure === 'function') {
								if (xhr) {
									failure(xhr, err);
								}
								else {
									failure(null, 'Unknown error');
									logger.debug(`<<< Exiting error(xhr=${xhr},status=${status},err=${err})`);
								}
							}
						}
					};

					if (httpMethod === 'GET') {
						options.data = params;
					} else if (postFile) {
						options.processData = false;
						logger.debug(`define(): options.processData → ${options.processData}`);
						options.contentType = false;
						logger.debug(`define(): options.contentType → ${options.contentType}`);
						options.data = params;
					}
					else {
						options.contentType = 'application/json; charset=UTF-8';
						options.data = JSON.stringify(params);
						options.dataType = 'json';
						logger.debug(`define(): options.dataType → ${options.dataType}`);
					}

					if (typeof loading === 'function') {
						loading(true);
					}

					if (!silence) loader.startLoading();
					logger.debug(`<<< Exiting rawCall(httpMethod=${httpMethod},url=${url},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend},sync=${sync},silence=${silence},postFile=${postFile})`);
					return $.ajax(options);
				},

				call: function (httpMethod, method, params, success, failure, loading, beforeSend, sync, silence, postFile) {
					var self = this;
					var url = self.getServiceRoot() + self.controller + '/' + method;

					// Reset url default values
					self.moduleRoot = 'personaBar';
					logger.debug(`define(): self.moduleRoot → ${self.moduleRoot}`);
					self.controller = '';
					logger.debug(`define(): self.controller → ${self.controller}`);

					logger.debug(`<<< Exiting call(httpMethod=${httpMethod},method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend},sync=${sync},silence=${silence},postFile=${postFile})`);
					return this.rawCall(httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile);
				},

				post: function (method, params, success, failure, loading, beforeSend) {
					logger.debug(`>>> Entering post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					logger.debug(`init(siteRoot=${siteRoot},tabId=${tabId},antiForgeryToken=<redacted>): self.controller → ${self.controller}`);
					logger.debug(`<<< Exiting post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, false);
				},

				postfile: function (method, params, success, failure, loading, beforeSend) {
					logger.debug(`>>> Entering postfile(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					logger.debug(`<<< Exiting postfile(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, false, true);
				},

				postsilence: function (method, params, success, failure, loading, beforeSend) {
					logger.debug(`>>> Entering postsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					logger.debug(`<<< Exiting postsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, true);
				},

				get: function (method, params, success, failure, loading, beforeSend) {
					logger.debug(`>>> Entering get(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					logger.debug(`<<< Exiting get(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					return this.call('GET', method, params, success, failure, loading, beforeSend, false, false);
				},

				getsilence: function (method, params, success, failure, loading, beforeSend) {
					logger.debug(`>>> Entering getsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					logger.debug(`<<< Exiting init(siteRoot=${siteRoot},tabId=${tabId},antiForgeryToken=<redacted>)`);
					logger.debug(`<<< Exiting getsilence(method=${method},params=${params},success=${success},failure=${failure},loading=${loading},beforeSend=${beforeSend})`);
					return this.call('GET', method, params, success, failure, loading, beforeSend, false, true);
				}
			};
		}
	};
	logger.debug('<<< Exiting define()');
});