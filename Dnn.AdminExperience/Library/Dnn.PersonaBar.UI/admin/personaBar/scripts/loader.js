const logger = console;
﻿define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
    var loadingbarId = '#personaBar-loadingbar';
    var loaded = false;
	logger.debug(`define(): loadingbarId → ${loadingbarId}`);
    var error = true;

	logger.debug('<<< Exiting define()');
    var defaultOptions = {
        slowOperationMessage: "We are processing your content, please wait..."
    };

    function loading(options) {
		logger.debug(`>>> Entering loading(options=${options})`);
        if (window.dnn.loading) return;

        options = $.extend(defaultOptions, options);
        var loadingbar = $(loadingbarId);
		logger.debug(`loading(options=${options}): options → ${options}`);
		logger.debug(`loading(options=${options}): loadingbar → ${loadingbar}`);
        var progressbar = $(loadingbarId + ' > div');
        var width = loadingbar.width();
		logger.debug(`loading(options=${options}): progressbar → ${progressbar}`);
		logger.debug(`loading(options=${options}): width → ${width}`);
        var closeLoadingBar = $("#close-load-error");
        var loadingBarMessage = loadingbar.children('span');
		logger.debug(`loading(options=${options}): closeLoadingBar → ${closeLoadingBar}`);
		logger.debug(`loading(options=${options}): loadingBarMessage → ${loadingBarMessage}`);
        var slowConnectionTimeout = 4000;

        loadingbar.show();
        loadingBarMessage.html("");
        window.dnn.loading = true;
        closeLoadingBar.hide();
		logger.debug(`<<< Exiting loading(options=${options})`);

        function hideLoadingBar() {
			logger.debug('>>> Entering hideLoadingBar()');
            loadingbar.hide();
			logger.debug('<<< Exiting hideLoadingBar()');
        }

        progressbar.removeClass().css({ width: 0, height: 5 }).stop(true, false).animate({ width: 0.75 * width }, 300, 'linear', function () {
			logger.debug('>>> Entering animate()');
            var pingDuration = 0;
            function checkloaded() {
				logger.debug('>>> Entering checkloaded()');
                if (loaded) {
                    loaded = false;
                    window.dnn.loading = false;

                    clearTimeout(checkloaded);

                    checkloaded = null;
                    pingDuration = 0;

                    if (!error) {
                        progressbar.stop(true, false).animate({ width: "100%" }, 100, 'linear', hideLoadingBar);
                    } else {
                        loadingBarMessage.show().html("There was an error retrieving your content. Please check your internet connection.");
                        progressbar.stop(true, false).addClass("load-error").animate({ width: "100%" }, 100, 'linear').animate({ height: 25 }, 100, 'linear', function () {
                            closeLoadingBar.show().click(hideLoadingBar);
                        });
						logger.debug('<<< Exiting animate()');
                    }
                }
                else {
                    pingDuration += 20;
                    if (pingDuration >= slowConnectionTimeout) {
                        var message = options.slowOperationMessage;
                        loadingBarMessage.show().html(message);
                        progressbar.animate({
                            height: 25
                        }, 'linear');
                    }
                    setTimeout(checkloaded, 20);
					logger.debug('<<< Exiting checkloaded()');
                }
            };
            checkloaded();
        });
    };

    return {
        startLoading: function (options) {
			logger.debug(`>>> Entering startLoading(options=${options})`);
            loading(options);
			logger.debug(`<<< Exiting startLoading(options=${options})`);
        },
        stopLoading: function stopLoading(_error) {
			logger.debug(`>>> Entering stopLoading(_error=${_error})`);
            if (window.dnn.loading) {
                loaded = true;
                error = _error;
				logger.debug(`stopLoading(_error=${_error}): loaded → ${loaded}`);
            }
			logger.debug(`<<< Exiting stopLoading(_error=${_error})`);
        }
    }
});