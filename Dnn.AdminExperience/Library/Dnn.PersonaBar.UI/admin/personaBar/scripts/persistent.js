const logger = console;
'use strict';
define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
    return {
        init: function (cfg, sf) {
			logger.debug(`>>> Entering init(cfg=${cfg},sf=${sf})`);
            var defaultSettings = {
                expandPersonaBar: true
            };

            var settings = null;
            var serviceFramework = sf.init(cfg.siteRoot, cfg.tabId, cfg.antiForgeryToken);
			logger.debug(`<<< Exiting init(cfg=${cfg},sf=${sf})`);
            var inIframe = window !== window.top;

            return {
                load: function () {
					logger.debug('>>> Entering load()');
                    if (!settings) settings = $.extend(defaultSettings, cfg.userSettings);
					logger.debug('<<< Exiting load()');
                    return settings;
                },
                save: function (s, success, error) {
					logger.debug(`>>> Entering save(s=${s},success=${success},error=${error})`);
                    if (!s) return;
                    settings = $.extend(defaultSettings, s);

                    serviceFramework.controller = "UserSettings";
					logger.debug(`<<< Exiting save(s=${s},success=${success},error=${error})`);
                    serviceFramework.postsilence('UpdateUserSettings', settings, function () {
						logger.debug(`>>> Entering postsilence(settings=${settings},success=${success},error=${error})`);
                        // Update settings in all locations
						logger.debug(`define(): serviceFramework.controller → ${serviceFramework.controller}`);
                        if (inIframe) {
							logger.debug(`init(cfg=${cfg},sf=${sf}): serviceFramework.controller → ${serviceFramework.controller}`);
                            if (window.parent['personaBarSettings']['personaBarUserSettings']) {
                                $.extend(window.parent['personaBarSettings']['personaBarUserSettings'], settings);
                            }
                        } else {
                            if (window['personaBarSettings']['personaBarUserSettings']) {
                                $.extend(window['personaBarSettings']['personaBarUserSettings'], settings);
                            }
                        }

                        $.extend(cfg.userSettings, settings);

                        if (typeof success === 'function') {
                            success();
							logger.debug(`<<< Exiting postsilence(settings=${settings},success=${success},error=${error})`);
                        }
                    }, error);
                }
            };
        }
    };
	logger.debug('<<< Exiting define()');
});
