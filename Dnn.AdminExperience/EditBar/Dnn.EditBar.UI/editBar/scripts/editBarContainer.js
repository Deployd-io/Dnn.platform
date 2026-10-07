var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    $(document.body).ready(function () {
        var loadEditBar = function () {
			logger.error('>>> Entering loadEditBar()', { "event.action": "loadEditBar" });
            var w = window,
                d = document,
                e = d.documentElement,
                g = d.getElementsByTagName('body')[0],
                x = w.innerWidth || e.clientWidth || g.clientWidth,
                settings = window.top.editBarSettings || {},
                appPath = settings.applicationPath;

            if (typeof settings.items == "undefined" || settings.items.length === 0) {
                return;
            }

            var debugMode = settings.debugMode === true;
            var src = appPath + '/DesktopModules/admin/Dnn.EditBar/index.html';
			logger.error(`loadEditBar(): src → ${src}`, { "event.action": "loadEditBar" });
            src += '?cdv=' + settings.buildNumber + (debugMode ? '&t=' + Math.random() : '');
            var $container = $('<div class="editBarFrameContainer"></div>');
            if ($('#personaBar-iframe').length) {
                $container.addClass('personabar-shown');
            }
            var $iframe = $('<iframe id="editBar-iframe" allowTransparency="true" frameBorder="0" scrolling="false"></iframe>');

            $container.append($iframe).appendTo(document.body);

            $iframe.attr('src', src).mouseenter(function () {
                    $iframe.addClass("small");
                }).mouseleave(function () {
                    $iframe.removeClass("small middle");
                });
				logger.warn('<<< Exiting loadEditBar()', { "event.action": "loadEditBar" });
        }

        loadEditBar();
    });
})(jQuery);