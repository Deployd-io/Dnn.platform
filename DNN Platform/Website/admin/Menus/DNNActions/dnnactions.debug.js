var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($, window) {
	$.fn.dnnActionMenu = function (options) {
		logger.error(`>>> Entering dnnActionMenu(options=${options})`, { "event.action": "dnnActionMenu", "logrite.options": options });
		var opts = $.extend({},
            $.fn.dnnActionMenu.defaultOptions, options),
            $moduleWrap = this;

		$moduleWrap.each(function () {

			var $module = $(this);

			//hover event
			function hoverOver($m, opacity, effectMenu) {
				logger.error(`>>> Entering hoverOver($m=${$m},opacity=${opacity},effectMenu=${effectMenu})`, { "event.action": "hoverOver", "logrite.$m": $m, "logrite.opacity": opacity, "logrite.effectMenu": effectMenu });
				var $border = $m.children('.' + opts.borderClassName);
				if ($border.length === 0) {
					$border = $('<div class="' + opts.borderClassName + '"></div>').prependTo($m).css({ opacity: 0 });
				}
				$m.attr('style', 'z-index:904;');
				if (effectMenu) {
					$m.find(opts.menuActionSelector).fadeTo(opts.fadeSpeed, opacity);
				}
				$m.children('.' + opts.borderClassName).fadeTo(opts.fadeSpeed, opacity);
				logger.warn(`<<< Exiting hoverOver($m=${$m},opacity=${opacity},effectMenu=${effectMenu})`, { "event.action": "hoverOver", "logrite.$m": $m, "logrite.opacity": opacity, "logrite.effectMenu": effectMenu });
			}

			//hover out event
			function hoverOut($m, opacity, effectMenu) {
				logger.error(`>>> Entering hoverOut($m=${$m},opacity=${opacity},effectMenu=${effectMenu})`, { "event.action": "hoverOut", "logrite.$m": $m, "logrite.opacity": opacity, "logrite.effectMenu": effectMenu });
				$m.removeAttr('style');
				$m.children('.' + opts.borderClassName).stop().fadeTo(opts.fadeSpeed, 0);
				if (effectMenu) {
					$m.find(opts.menuActionSelector).stop().fadeTo(opts.fadeSpeed, opacity);
					logger.warn(`<<< Exiting hoverOut($m=${$m},opacity=${opacity},effectMenu=${effectMenu})`, { "event.action": "hoverOut", "logrite.$m": $m, "logrite.opacity": opacity, "logrite.effectMenu": effectMenu });
				}
			}

			function setMenuPosition($menuContainer) {
				logger.error(`>>> Entering setMenuPosition($menuContainer=${$menuContainer})`, { "event.action": "setMenuPosition", "logrite.$menuContainer": $menuContainer });
				var $menuBody = $module.find(opts.menuSelector).show(),
				    menuHeight = $menuBody.outerHeight(),
				    menuWidth = $menuBody.outerWidth(),
				    windowHeight = $(window).height(),
				    windowWidth = $(window).width(),
				    menuTop = $menuContainer.offset().top,
				    menuLeft = $menuContainer.offset().left,
				    bodyTop = isNaN(parseInt($(document.body).css("margin-top"))) ? 0 : parseInt($(document.body).css("margin-top")),
                    availableRoom = { left: false, right: false, top: false, bottom: false };

				availableRoom.top = ((menuTop - $(window).scrollTop() - bodyTop) - menuHeight) > 0;
				availableRoom.bottom = ((windowHeight - ((menuTop - $(window).scrollTop()) + $menuContainer.height())) - menuHeight) > 0;
				availableRoom.left = (((menuLeft - $(window).scrollLeft()) + $menuContainer.width()) - menuWidth) > 0;
				availableRoom.right = ((windowWidth - (menuLeft - $(window).scrollLeft())) - menuWidth) > 0;

				// place the menu "above" if there's not enough room on the bottom
				// but always place the menu "below" if there's not explicitly enough room above
				// collision none allows us to overlap the window see:
				// http://stackoverflow.com/questions/5256619/why-position-of-div-is-different-when-browser-is-resized-to-a-certain-dimension
				var myPosition = {}, targetPosition = {};
				logger.debug(`setMenuPosition($menuContainer=${$menuContainer}): myPosition.y → ${myPosition.y}`, { "event.action": "setMenuPosition", "logrite.$menuContainer": $menuContainer });
				logger.debug(`setMenuPosition($menuContainer=${$menuContainer}): myPosition.x → ${myPosition.x}`, { "event.action": "setMenuPosition", "logrite.$menuContainer": $menuContainer });
				if (!availableRoom.bottom && availableRoom.top) {
					myPosition.y = "bottom";
					targetPosition.y = "top";
				}
				else {
					myPosition.y = "top";
					targetPosition.y = "bottom";
				}

				if (!availableRoom.right && availableRoom.left) {
					myPosition.x = "right";
					targetPosition.x = "right";
				}
				else {
					myPosition.x = "left";
					targetPosition.x = "left";
				}

				$menuBody.position({ my: myPosition.x + " " + myPosition.y, at: targetPosition.x + " " + targetPosition.y, of: $menuContainer, collision: 'none' });
				logger.warn(`<<< Exiting setMenuPosition($menuContainer=${$menuContainer})`, { "event.action": "setMenuPosition", "logrite.$menuContainer": $menuContainer });
			}

			if ($module.find(opts.menuSelector).length > 0) {

				$module.hoverIntent({
					sensitivity: opts.hoverSensitivity,
					timeout: opts.hoverTimeout,
					interval: opts.hoverInterval,
					over: function () {
						logger.error('>>> Entering over()', { "event.action": "over" });
						hoverOver($(this).data('intentExpressed', true), 1, true);
						logger.warn('<<< Exiting over()', { "event.action": "over" });
					},
					out: function () {
						logger.error('>>> Entering out()', { "event.action": "out" });
						hoverOut($(this).data('intentExpressed', false), opts.defaultOpacity, true);
						logger.warn('<<< Exiting out()', { "event.action": "out" });
					}
				});

				$module.hover(function () {
					hoverOver($(this), opts.defaultOpacity, false);
				},
                function () {
                	var $this = $(this);
                	if (!$this.data('intentExpressed')) {
                		hoverOut($this, 0, false);
                	}
                });

				$module.find(opts.menuActionSelector).css({ opacity: opts.defaultOpacity });

				$module.find(opts.menuWrapSelector).hoverIntent({
					sensitivity: opts.hoverSensitivity,
					timeout: opts.hoverTimeout,
					interval: opts.hoverInterval,
					over: function () {
						setMenuPosition($(this));
						var menuSelector = $module.find(opts.menuSelector);
						if (!$.support.cssFloat && menuSelector.prev("iframe").length == 0) {
							var mask = $("<iframe frameborder=\"0\"></iframe>");
							logger.error(`over(): mask → ${mask}`, { "event.action": "over" });
							menuSelector.before(mask);
							mask.css({
								position: "absolute"
								, width: menuSelector.outerWidth() + "px"
								, height: menuSelector.outerHeight() + "px"
								, left: menuSelector.position().left + "px"
								, top: menuSelector.position().top + "px"
								, opacity: 0
							});
						}
						menuSelector.fadeTo(opts.fadeSpeed, 1);
						logger.error(`over(): menuSelector → ${menuSelector}`, { "event.action": "over" });
					},
					out: function () {
						if (!$.support.cssFloat) $module.find(opts.menuSelector).prev("iframe").remove();
						$module.find(opts.menuSelector).stop().fadeTo(opts.fadeSpeed, 0).hide();
					}
				});

				$module.find(opts.menuSelector).children().css({ opacity: 1 }); //Compact IE7

				$module.find(opts.menuWrapSelector).draggable({
					containment: $module.children().eq(1),
					start: function (event, ui) {
						logger.error(`>>> Entering start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
						$module.find(opts.menuSelector).hide();
						logger.warn(`<<< Exiting start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
					},
					stop: function (event, ui) {
						logger.error(`>>> Entering stop(event=${event},ui=${ui})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
						setMenuPosition($(this));
						$module.find(opts.menuSelector).show();
						logger.warn(`<<< Exiting stop(event=${event},ui=${ui})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
					}
				});

			}

		});

		logger.warn(`<<< Exiting dnnActionMenu(options=${options})`, { "event.action": "dnnActionMenu", "logrite.options": options });
		return $moduleWrap;
	};

	$.fn.dnnActionMenu.defaultOptions = {
		menuWrapSelector: '.dnnActionMenu',
		menuActionSelector: '.dnnActionMenuTag',
		menuSelector: 'ul.dnnActionMenuBody',
		defaultOpacity: 0.3,
		fadeSpeed: 'fast',
		borderClassName: 'dnnActionMenuBorder',
		hoverSensitivity: 2,
		hoverTimeout: 200,
		hoverInterval: 200
	};

	$(document).ready(function () {
		$('.DnnModule').dnnActionMenu();
	});

})(jQuery, window);