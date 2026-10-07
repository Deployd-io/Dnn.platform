var logger = (typeof logger !== 'undefined' && logger) || console;
define(['jquery'], function ($) {
    var loadingbarId = '#personaBar-loadingbar';
    var loaded = false;
    var error = true;

    var defaultOptions = {
        slowOperationMessage: "We are processing your content, please wait..."
    };

    function loading(options) {
		logger.error(`>>> Entering loading(options=${options})`, { "event.action": "loading", "logrite.options": options });
        if (window.dnn.loading) return;

        options = $.extend(defaultOptions, options);
        var loadingbar = $(loadingbarId);
		logger.error(`loading(options=${options}): options → ${options}`, { "event.action": "loading", "logrite.options": options });
		logger.error(`loading(options=${options}): loadingbar → ${loadingbar}`, { "event.action": "loading", "logrite.options": options });
        var progressbar = $(loadingbarId + ' > div');
        var width = loadingbar.width();
		logger.error(`loading(options=${options}): progressbar → ${progressbar}`, { "event.action": "loading", "logrite.options": options });
		logger.error(`loading(options=${options}): width → ${width}`, { "event.action": "loading", "logrite.options": options });
        var closeLoadingBar = $("#close-load-error");
        var loadingBarMessage = loadingbar.children('span');
		logger.error(`loading(options=${options}): closeLoadingBar → ${closeLoadingBar}`, { "event.action": "loading", "logrite.options": options });
		logger.error(`loading(options=${options}): loadingBarMessage → ${loadingBarMessage}`, { "event.action": "loading", "logrite.options": options });
        var slowConnectionTimeout = 4000;

        loadingbar.show();
        loadingBarMessage.html("");
        window.dnn.loading = true;
        closeLoadingBar.hide();
		logger.warn(`<<< Exiting loading(options=${options})`, { "event.action": "loading", "logrite.options": options });

        function hideLoadingBar() {
			logger.error('>>> Entering hideLoadingBar()', { "event.action": "hideLoadingBar" });
            loadingbar.hide();
			logger.warn('<<< Exiting hideLoadingBar()', { "event.action": "hideLoadingBar" });
        }

        progressbar.removeClass().css({ width: 0, height: 5 }).stop(true, false).animate({ width: 0.75 * width }, 300, 'linear', function () {
            var pingDuration = 0;
            function checkloaded() {
				logger.error('>>> Entering checkloaded()', { "event.action": "checkloaded" });
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
					logger.warn('<<< Exiting checkloaded()', { "event.action": "checkloaded" });
                }
            };
            checkloaded();
        });
    };

    return {
        startLoading: function (options) {
			logger.error(`>>> Entering startLoading(options=${options})`, { "event.action": "startLoading", "logrite.options": options });
            loading(options);
			logger.warn(`<<< Exiting startLoading(options=${options})`, { "event.action": "startLoading", "logrite.options": options });
        },
        stopLoading: function stopLoading(_error) {
			logger.error(`>>> Entering stopLoading(_error=${_error})`, { "event.action": "stopLoading", "logrite._error": _error });
            if (window.dnn.loading) {
                loaded = true;
                error = _error;
				logger.warn(`<<< Exiting stopLoading(_error=${_error})`, { "event.action": "stopLoading", "logrite._error": _error });
            }
        }
    }
});