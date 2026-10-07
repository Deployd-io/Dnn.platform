var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
    return {
        init: function (cfg, sf) {
			logger.error(`>>> Entering init(cfg=${cfg},sf=${sf})`, { "event.action": "init", "logrite.cfg": cfg, "logrite.sf": sf });
            var defaultSettings = {
                expandPersonaBar: true
            };

            var settings = null;
            var serviceFramework = sf.init(cfg.siteRoot, cfg.tabId, cfg.antiForgeryToken);
			logger.warn(`<<< Exiting init(cfg=${cfg},sf=${sf})`, { "event.action": "init", "logrite.cfg": cfg, "logrite.sf": sf });
            var inIframe = window !== window.top;

            return {
                load: function () {
					logger.error('>>> Entering load()', { "event.action": "load" });
                    if (!settings) settings = $.extend(defaultSettings, cfg.userSettings);
					logger.warn('<<< Exiting load()', { "event.action": "load" });
                    return settings;
                },
                save: function (s, success, error) {
					logger.error(`>>> Entering save(s=${s},success=${success},error=${error})`, { "event.action": "save", "logrite.s": s, "logrite.success": success, "logrite.error": error });
                    if (!s) return;
                    settings = $.extend(defaultSettings, s);

                    serviceFramework.controller = "UserSettings";
                    serviceFramework.postsilence('UpdateUserSettings', settings, function () {
                        // Update settings in all locations
                        if (inIframe) {
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
                        }
                    }, error);
					logger.warn(`<<< Exiting save(s=${s},success=${success},error=${error})`, { "event.action": "save", "logrite.s": s, "logrite.success": success, "logrite.error": error });
                }
            };
        }
    };
});
