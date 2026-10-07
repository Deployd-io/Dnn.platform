var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery', 'main/config'], function ($, cf) {
    var config = cf.init();
    var loadImage = function(wrapper, path, alt) {
		logger.error(`>>> Entering loadImage(wrapper=${wrapper},path=${path},alt=${alt})`, { "event.action": "loadImage", "logrite.wrapper": wrapper, "logrite.path": path, "logrite.alt": alt });
        wrapper.append($('<img />').attr({
            'src': path,
            'alt': alt
        }));
		logger.warn(`<<< Exiting loadImage(wrapper=${wrapper},path=${path},alt=${alt})`, { "event.action": "loadImage", "logrite.wrapper": wrapper, "logrite.path": path, "logrite.alt": alt });
    }

    var loadSvg = function (wrapper, path, alt, iconId) {
		logger.error(`>>> Entering loadSvg(wrapper=${wrapper},path=${path},alt=${alt},iconId=${iconId})`, { "event.action": "loadSvg", "logrite.wrapper": wrapper, "logrite.path": path, "logrite.alt": alt, "logrite.iconId": iconId });
        $.get(path, {}, function (data) {
            wrapper.html(function () {
                /* Add a <title> element to the SVG, and an aria-labelledby attribute to help assistive technology */
                var titleId = iconId + '_title';
                var $titleEl = $('<title id="' + titleId + '">' + alt + '</title>');
                var svgIcon = $.parseHTML($.trim(data), null, false);

                /* Make sure there isn't already a <title> */
                if ($(svgIcon).has('title') === true) {
                    $(svgIcon)
                        .attr('aria-labelledby', titleId)
                        .find('title')
                        .replaceWith($titleEl);
                }
                else {
                    $(svgIcon)
                        .attr('aria-labelledby', titleId)
                        .prepend($titleEl);
                }
                return $(svgIcon);
            });

        }, 'text');
		logger.warn(`<<< Exiting loadSvg(wrapper=${wrapper},path=${path},alt=${alt},iconId=${iconId})`, { "event.action": "loadSvg", "logrite.wrapper": wrapper, "logrite.path": path, "logrite.alt": alt, "logrite.iconId": iconId });
    }

    var loadIcons = function() {
		logger.error('>>> Entering loadIcons()', { "event.action": "loadIcons" });
        $('#personabar .personabarnav span.icon-loader').each(function() {
            var $this = $(this);
            var path = $this.data('path');
            var alt = $this.data('name');
            var iconId = $this.data('id');
            if (!path) {
                return;
            }

            if (path.indexOf('cdv=') === -1) {
                path += (path.indexOf('?') > -1 ? '&' : '?') + 'cdv=' + config.buildNumber;
            }

            var ext = path.split('?')[0].split('.').pop().toLowerCase();
            if (ext === "svg") {
                loadSvg($this, path, alt, iconId);
            } else {
                loadImage($this, path, alt);
            }

            $this.data('path', null);
        });
		logger.warn('<<< Exiting loadIcons()', { "event.action": "loadIcons" });
    }

    return {
        load: function () {
			logger.error('>>> Entering load()', { "event.action": "load" });
            loadIcons();
			logger.warn('<<< Exiting load()', { "event.action": "load" });
        } 
    };
});
