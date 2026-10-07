var logger = (typeof logger !== 'undefined' && logger) || console;
if (!window.DDR)
	window.DDR = {};
if (!DDR.Menu)
	DDR.Menu = {};
if (!DDR.Menu.Providers)
	DDR.Menu.Providers = {};

DDRjQuery(function ($) {
	DDR.Menu.Providers.BaseRenderer = function () {
	logger.error('>>> Entering BaseRenderer()', { "event.action": "BaseRenderer" });
	}

	DDR.Menu.Providers.BaseRenderer.prototype.baseConstructor = function (jqContainer, dnnNavParams) {
		var me = this;

		me.jqContainer = jqContainer;
		me.dnnNavParams = dnnNavParams;
		logger.debug(`baseConstructor(jqContainer=${jqContainer},dnnNavParams=${dnnNavParams}): me.dnnNavParams → ${me.dnnNavParams}`, { "event.action": "baseConstructor", "logrite.jqContainer": jqContainer, "logrite.dnnNavParams": dnnNavParams });
		me.menus = jqContainer.find("ul").toDDRObjectArray();
		me.items = jqContainer.find("li").toDDRObjectArray();
		me.subMenus = me.menus.filter(function (m) { return (m.level > 0); });
		me.rootItems = me.items.filter(function (i) { return (i.level == 0); });

		me.jqRootMenu = jqContainer.children("ul");
		me.rootMenu = me.jqRootMenu.toDDRObject();
		me.clientID = jqContainer[0].id;
		logger.debug(`baseConstructor(jqContainer=${jqContainer},dnnNavParams=${dnnNavParams}): me.clientID → ${me.clientID}`, { "event.action": "baseConstructor", "logrite.jqContainer": jqContainer, "logrite.dnnNavParams": dnnNavParams });

		me.showEffect = dnnNavParams.effect || "slide";
		logger.debug(`baseConstructor(jqContainer=${jqContainer},dnnNavParams=${dnnNavParams}): me.showEffect → ${me.showEffect}`, { "event.action": "baseConstructor", "logrite.jqContainer": jqContainer, "logrite.dnnNavParams": dnnNavParams });
		me.showEffectOptions = JSON.parse(dnnNavParams.effectOptions) || {};
		me.showEffectSpeed = dnnNavParams.effectSpeed || 200;
		logger.debug(`baseConstructor(jqContainer=${jqContainer},dnnNavParams=${dnnNavParams}): me.showEffectSpeed → ${me.showEffectSpeed}`, { "event.action": "baseConstructor", "logrite.jqContainer": jqContainer, "logrite.dnnNavParams": dnnNavParams });
		me.orientHorizontal = (dnnNavParams.ControlOrientation != "Vertical");

		me.useShim = $.browser.msie && dnnNavParams.shim;
		logger.debug(`baseConstructor(jqContainer=${jqContainer},dnnNavParams=${dnnNavParams}): me.useShim → ${me.useShim}`, { "event.action": "baseConstructor", "logrite.jqContainer": jqContainer, "logrite.dnnNavParams": dnnNavParams });
	}

	DDR.Menu.Providers.BaseRenderer.prototype.mapToRendered = function (jq) {
		logger.error(`>>> Entering mapToRendered(jq=${jq})`, { "event.action": "mapToRendered", "logrite.jq": jq });
		return $($.map(jq.get(), function (elt) { return $(elt).ddrData().rendered.get(0); }));
	}

	DDR.Menu.Providers.BaseRenderer.prototype.addCovering = function () {
		logger.error('>>> Entering addCovering()', { "event.action": "addCovering" });
		var me = this;

		var resizeCoverings = new Array();
		logger.error(`addCovering(): resizeCoverings → ${resizeCoverings}`, { "event.action": "addCovering" });
		var coveringBase = $("<a style='position:absolute;display:block;border:0;padding:0;margin:0;top:0;left:0;bottom:0;right:0;text-decoration:none'><span style='visibility:hidden'/></a>").css("background", $.browser.msie ? "url(" + me.dnnNavParams.PathSystemImage + "spacer.gif)" : "transparent");
		logger.error(`addCovering(): coveringBase → ${coveringBase}`, { "event.action": "addCovering" });
		var overrideDimensions = ($.browser.msie && $.browser.version.startsWith("6.")) || !$.support.boxModel;
		logger.error(`addCovering(): overrideDimensions → ${overrideDimensions}`, { "event.action": "addCovering" });
		me.menus.each(function (m) {
			m.coverings = new Array();
			m.childItems.each(function (i) {
				if (i.coveringHere) {
					var itemText = i.rendered.text();
					i.coveringHere().each(function () {
						var coveringHere = $(this);
						coveringHere.css("position", "relative");
						var covering = coveringBase.clone();
						covering.attr("href", i.href || "javascript:void(0)").children("span").text(itemText);
						coveringHere.prepend(covering);
						covering = coveringHere.children("a:first");
						var parent = covering.offsetParent();
						var chElt = coveringHere[0];
						logger.debug(`addCovering(): chElt.moveCovering → ${chElt.moveCovering}`, { "event.action": "addCovering" });
						logger.debug(`addCovering(): chElt.covering → ${chElt.covering}`, { "event.action": "addCovering" });
						logger.debug(`addCovering(): chElt.coveringHere → ${chElt.coveringHere}`, { "event.action": "addCovering" });

						if (chElt === parent[0] || chElt.offsetParent === null) {
							if (overrideDimensions) {
								covering.css({
									"left": "",
									"right": "",
									"width": coveringHere.outerWidth(false) + "px",
									"height": coveringHere.outerHeight(false) + "px"
								});
							}
						} else {
							chElt.moveCovering = true;
							chElt.covering = covering;
							chElt.coveringHere = coveringHere;
							covering.css({ "width": "0", "height": "0" });
							coveringHere.on("mouseenter", function () {
								m.coverings.each(function (chElt) {
									if (chElt.moveCovering) {
										var chOffset = chElt.coveringHere.offset();
										chElt.covering.css({ "top": "0", "left": "0" });
										var cOffset = chElt.covering.offset();
										chElt.covering.css({
											"top": (chOffset.top - cOffset.top) + "px",
											"left": (chOffset.left - cOffset.left) + "px",
											"width": chElt.coveringHere.outerWidth(false) + "px",
											"height": chElt.coveringHere.outerHeight(false) + "px"
										});
										chElt.moveCovering = false;
									}
								});
							});
							m.coverings.push(chElt);
							resizeCoverings.push(chElt);
						}
					});
				}
			});
		});

		if (resizeCoverings.length > 0) {
			$(window).resize(function () {
				resizeCoverings.each(function (c) {
					c.covering.css({ width: "0", height: "0" });
					c.moveCovering = true;
				});
			});
			logger.warn('<<< Exiting addCovering()', { "event.action": "addCovering" });
		}
	}

	DDR.Menu.Providers.BaseRenderer.prototype.prepareHideAndShow = function () {
		logger.error('>>> Entering prepareHideAndShow()', { "event.action": "prepareHideAndShow" });
		var me = this;

		logger.error(`prepareHideAndShow(): me → ${me}`, { "event.action": "prepareHideAndShow" });
		me.hideAllMenus = me.menus.filter(function (m) { return m.flyout; });
		me.setItemsHideAndShow();
		me.attachEvents();
		me.closeUp();
		logger.warn('<<< Exiting prepareHideAndShow()', { "event.action": "prepareHideAndShow" });
	}

	DDR.Menu.Providers.BaseRenderer.prototype.setItemsHideAndShow = function () {
		logger.error('>>> Entering setItemsHideAndShow()', { "event.action": "setItemsHideAndShow" });
		var me = this;

		me.items.each(function (i) {
			var allParents = i.allParentMenus;
			var allChildren = i.allChildMenus;
			var childMenu = i.childMenu;

			var hideThese = me.subMenus.filter(function (m) {
			logger.debug(`setItemsHideAndShow(): i.hideThese → ${i.hideThese}`, { "event.action": "setItemsHideAndShow" });
				return (m.flyout && !(allParents.contains(m) || allChildren.contains(m)));
			});

			i.hideThese = hideThese;
			i.showThese = new Array();
			if (childMenu && childMenu.flyout)
				i.showThese[0] = childMenu;
		});
	}

	DDR.Menu.Providers.BaseRenderer.prototype.attachEvents = function () {
		logger.error('>>> Entering attachEvents()', { "event.action": "attachEvents" });
		var me = this;

		logger.debug(`attachEvents(): me.timeoutID → ${me.timeoutID}`, { "event.action": "attachEvents" });
		me.menus.each(function (menu) {
			if (!menu.flyout) {
				menu.childItems.each(function (i) {
					if (me.menuItemHover) {
						me.menuItemHover(i);
					}
					i.rendered.hover(
							function () {
								i.hideThese.allRendered().each(function () { this.hideMenu() });
								i.showThese.allRendered().each(function () { this.showMenu() });
							},
							function () {
							}
						);
					i.rendered.focus(function () {
						i.hideThese.allRendered().each(function () { this.hideMenu() });
						i.showThese.allRendered().each(function () { this.showMenu(true) });
					});
				});
			} else {
				var jqMenu = menu.rendered;
				var menuElt = jqMenu[0];

				menuElt.hideMenu = function () {
					logger.error('>>> Entering hideMenu()', { "event.action": "hideMenu" });
					jqMenu.stop(true, true);
					if (this.style.display != "none") {
						jqMenu.hide();
						if (menu.shim)
							menu.shim.hide();
					}
				};
				menuElt.showMenu = function (instant) {
					logger.error(`>>> Entering showMenu(instant=${instant})`, { "event.action": "showMenu", "logrite.instant": instant });
					if (this.style.display == "none") {
						jqMenu.stop(true, true);
						menu.childItems.allRendered().stop(true, true).off('mouseenter mouseleave');
						me.positionMenu(menu);
						if (instant || (me.showEffectSpeed == 0)) {
							jqMenu.show();
						} else {
							if (me.showEffect == "none") {
								jqMenu.queue(function () {
									setTimeout(function () { jqMenu.dequeue(); }, me.showEffectSpeed);
								});
								jqMenu.show(1);
							} else if (me.showEffect == "fade") {
								jqMenu.fadeIn(me.showEffectSpeed);
							} else {
								if ((me.showEffect == "slide") || (me.showEffect == "drop")) {
									me.showEffectOptions.direction = menu.slideDirection;
								}
								else {
									me.showEffectOptions.direction = menu.blindDirection;
								}
								jqMenu.show(me.showEffect, me.showEffectOptions, me.showEffectSpeed);
							}
						}
						jqMenu.queue(function () {
							jqMenu.css("display", "none").css("display", "block");
							menu.childItems.each(function (i) {
								if (me.menuItemHover) {
									me.menuItemHover(i);
								}
								i.rendered.hover(
										function () {
											i.hideThese.allRendered().each(function () { this.hideMenu() });
											i.showThese.allRendered().each(function () { this.showMenu() });
										},
										function () {
										}
									);
								i.rendered.focus(function () {
									i.hideThese.allRendered().each(function () { this.hideMenu() });
									i.showThese.allRendered().each(function () { this.showMenu(true) });
								});
							});
							$(this).dequeue();
						});
						logger.warn(`<<< Exiting showMenu(instant=${instant})`, { "event.action": "showMenu", "logrite.instant": instant });
					}
				};
			}

			menu.rendered.mouseover(function () {
				if (me.timeoutID) {
					window.clearTimeout(me.timeoutID);
					me.timeoutID = null;
				}
			});
			menu.rendered.mouseout(function () {
				if (!me.timeoutID) {
					me.timeoutID = window.setTimeout(function () { me.closeUp(); }, 400);
				}
			});
			menu.rendered.mouseover(function () {
				if (me.timeoutID) {
					window.clearTimeout(me.timeoutID);
					me.timeoutID = null;
				}
			});
			menu.rendered.mouseout(function () {
				if (!me.timeoutID) {
					me.timeoutID = window.setTimeout(function () { me.closeUp(); }, 400);
					logger.warn('<<< Exiting attachEvents()', { "event.action": "attachEvents" });
				}
			});
		});
	}

	DDR.Menu.Providers.BaseRenderer.prototype.positionMenu = function (menu) {
		logger.error(`>>> Entering positionMenu(menu=${menu})`, { "event.action": "positionMenu", "logrite.menu": menu });
		if (menu.childItems && menu.childItems.length > 0) {
			var me = this;

			var level = menu.level;
			var parentMenu = menu.parentItem.parentMenu;
			var parentItem = menu.parentItem;
			var parentRendered = menu.layout.match(/,menu$/) ? parentMenu.rendered : parentItem.rendered;
			logger.error(`positionMenu(menu=${menu}): parentRendered → ${parentRendered}`, { "event.action": "positionMenu", "logrite.menu": menu });
			var r = menu.rendered;

			var windowObj = $(window);
			logger.error(`positionMenu(menu=${menu}): windowObj → ${windowObj}`, { "event.action": "positionMenu", "logrite.menu": menu });
			var windowLeft = windowObj.scrollLeft();
			logger.error(`positionMenu(menu=${menu}): windowLeft → ${windowLeft}`, { "event.action": "positionMenu", "logrite.menu": menu });
			var windowRight = windowLeft + windowObj.width();
			logger.error(`positionMenu(menu=${menu}): windowRight → ${windowRight}`, { "event.action": "positionMenu", "logrite.menu": menu });
			var windowTop = windowObj.scrollTop();
			logger.error(`positionMenu(menu=${menu}): windowTop → ${windowTop}`, { "event.action": "positionMenu", "logrite.menu": menu });
			var windowBottom = windowTop + windowObj.height();
			logger.error(`positionMenu(menu=${menu}): windowBottom → ${windowBottom}`, { "event.action": "positionMenu", "logrite.menu": menu });

			var prevDisplay = r.css("display");
			r.css({
				"display": "block",
				"width": "auto",
				"height": "auto",
				"overflow-x": "visible",
				"overflow-y": "visible",
				"z-index": 10000 + level * 3
			});
			r.width(r.width());
			logger.error(`positionMenu(menu=${menu}): prevDisplay → ${prevDisplay}`, { "event.action": "positionMenu", "logrite.menu": menu });
			r.height(r.height());

			if (parentMenu.layout.match(/^horizontal/)) {
				menu.slideDirection = "up";
				logger.debug(`positionMenu(menu=${menu}): menu.slideDirection → ${menu.slideDirection}`, { "event.action": "positionMenu", "logrite.menu": menu });
				menu.blindDirection = "vertical";

				r.alignElement(
					function () { return menu.childItems[0].rendered.getLeft(2); }, parentRendered.getLeft(2),
					function () { return r.getTop(1); }, parentRendered.getBottom(1)
				);
				if (r.getRight(3) > windowRight) {
					logger.debug(`positionMenu(menu=${menu}): menu.blindDirection → ${menu.blindDirection}`, { "event.action": "positionMenu", "logrite.menu": menu });
					r.alignHorizontal(
						function () { return r.getRight(3); }, windowRight
					);
				}
				if (r.getBottom(3) > windowBottom) {
					r.alignVertical(
						function () { return r.getBottom(1); }, parentRendered.getTop(1)
					);
					menu.slideDirection = "down";
				}
				if (r.getTop(3) < windowTop) {
					r.alignVertical(
						function () { return r.getTop(1); }, parentRendered.getBottom(1)
					);
					r.css({
						"height": windowBottom - r.getTop(4),
						"overflow-x": "visible",
						"overflow-y": "scroll"
					});
					if (r.getRight(3) > windowRight) {
						r.alignHorizontal(
							function () { return r.getRight(3); }, windowRight
						);
					}
					menu.slideDirection = "up";
				}
				else {
					r.alignElement(
						function () { return menu.childItems[0].rendered.getLeft(2); }, parentRendered.getLeft(2),
						function () { return r.getTop(1); }, parentRendered.getBottom(1)
					);
					if (r.getRight(3) > windowRight) {
						r.alignHorizontal(
							function () { return r.getRight(3); }, windowRight
						);
					}
					if (r.getBottom(3) > windowBottom) {
						r.alignVertical(
							function () { return r.getBottom(1); }, parentRendered.getTop(1)
						);
						menu.slideDirection = "down";
					}
					if (r.getTop(3) < windowTop) {
						r.alignVertical(
							function () { return r.getTop(1); }, parentRendered.getBottom(1)
						);
						r.css({
							"height": windowBottom - r.getTop(4),
							"overflow-x": "visible",
							"overflow-y": "scroll"
						});
						if (r.getRight(3) > windowRight) {
							r.alignHorizontal(
								function () { return r.getRight(3); }, windowRight
							);
						}
						menu.slideDirection = "up";
					}
				}
			}
			else {
				menu.slideDirection = "left";
				menu.blindDirection = "horizontal";
				r.alignElement(
					function () { return r.getLeft(1); }, parentRendered.getRight(1),
					function () { return menu.childItems[0].rendered.getTop(2); }, parentRendered.getTop(2)
				);
				if (r.getBottom(3) > windowBottom) {
					r.alignVertical(
						function () { return r.getBottom(3); }, windowBottom
					);
				}
				if (r.getRight(3) > windowRight) {
					r.alignHorizontal(
						function () { return r.getRight(1); }, parentRendered.getLeft(1)
					);
					menu.slideDirection = "right";
				}
				if (r.getLeft(3) < windowLeft) {
					r.alignHorizontal(
						function () { return r.getLeft(1); }, parentRendered.getRight(1)
					);
					menu.slideDirection = "left";
				}
			}

			if (me.useShim) {
				if (!menu.shim) {
					menu.shim = $("<iframe src='javascript:false' frameBorder='0' scrolling='no' />").css({
						"position": "absolute",
						"z-index": 9999 + level * 3,
						"background-color": "transparent",
						"filter": "progid:DXImageTransform.Microsoft.Alpha(style=0,opacity=0)"
					}).appendTo($(document.body));
				}

				menu.shim.css({
					"top": r.css("top"),
					"left": r.css("left"),
					"width": r.outerWidth(true) + "px",
					"height": r.outerHeight(true) + "px",
					"display": "block"
				});
			}

			r.css("display", prevDisplay);
			logger.warn(`<<< Exiting positionMenu(menu=${menu})`, { "event.action": "positionMenu", "logrite.menu": menu });
		}
	}

	DDR.Menu.Providers.BaseRenderer.prototype.closeUp = function () {
		logger.error('>>> Entering closeUp()', { "event.action": "closeUp" });
		var me = this;
		me.hideAllMenus.allRendered().each(function () { this.hideMenu(); });
		logger.warn('<<< Exiting closeUp()', { "event.action": "closeUp" });
	}

	DDR.Menu.getCSS = function (elt, css) {
		logger.error(`>>> Entering getCSS(elt=${elt},css=${css})`, { "event.action": "getCSS", "logrite.elt": elt, "logrite.css": css });
		logger.warn(`<<< Exiting getCSS(elt=${elt},css=${css})`, { "event.action": "getCSS", "logrite.elt": elt, "logrite.css": css });
	    return parseFloat("0" + $(elt).css(css));
	};

	if (!Array.prototype.each)
		Array.prototype.each = function (fn) {
			logger.error(`>>> Entering each(fn=${fn})`, { "event.action": "each", "logrite.fn": fn });
			for (var n = 0; n < this.length; n++)
				fn(this[n]);
				logger.warn(`<<< Exiting each(fn=${fn})`, { "event.action": "each", "logrite.fn": fn });
		}

	if (!Array.prototype.filter)
		Array.prototype.filter = function (fn) {
			var result = new Array();
			for (var n = 0; n < this.length; n++) {
				if (fn(this[n])) {
					result[result.length] = this[n];
				}
			}
			return result;
		}

	if (!Array.prototype.contains)
		Array.prototype.contains = function (o) {
			for (var n = 0; n < this.length; n++) {
				if (this[n] == o) {
					return true;
				}
			}
			return false;
		}

	if (!Array.prototype.allRendered)
		Array.prototype.allRendered = function () {
			logger.error('>>> Entering allRendered()', { "event.action": "allRendered" });
			logger.warn('<<< Exiting allRendered()', { "event.action": "allRendered" });
			return $($.map(this, function (o) { return o.rendered.get(0); }));
		}

	$.fn.extend({
		getLeft: function (index) {
			logger.error(`>>> Entering getLeft(index=${index})`, { "event.action": "getLeft", "logrite.index": index });
			var o = this;
			//            if (this[0].tagName == "TR")
			//                o = o.children("td:first");
			var result = o.offset().left;
			logger.error(`getLeft(index=${index}): result → ${result}`, { "event.action": "getLeft", "logrite.index": index });
			var htmlThis = o[0];

			if (index == 1)
				result -= DDR.Menu.getCSS(htmlThis, "marginLeft");
			if (index >= 3)
				result += DDR.Menu.getCSS(htmlThis, "borderLeftWidth");
			if (index >= 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingLeft");
			return result;
		},
		getTop: function (index) {
			logger.error(`>>> Entering getTop(index=${index})`, { "event.action": "getTop", "logrite.index": index });
			var o = this;
			//            if (this[0].tagName == "TR")
			//                o = o.children("td:first");
			var result = o.offset().top;
			logger.error(`getTop(index=${index}): result → ${result}`, { "event.action": "getTop", "logrite.index": index });
			var htmlThis = o[0];

			if (index == 1)
				result -= DDR.Menu.getCSS(htmlThis, "marginTop");
			if (index >= 3)
				result += DDR.Menu.getCSS(htmlThis, "borderTopWidth");
			if (index >= 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingTop");
			return result;
		},
		getRight: function (index) {
			logger.error(`>>> Entering getRight(index=${index})`, { "event.action": "getRight", "logrite.index": index });
			var result = this.getLeft(4) + this.width();
			var htmlThis = this[0];
			logger.error(`getRight(index=${index}): result → ${result}`, { "event.action": "getRight", "logrite.index": index });
			if (index < 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingRight");
			if (index < 3)
				result += DDR.Menu.getCSS(htmlThis, "borderRightWidth");
			if (index < 2)
				result += DDR.Menu.getCSS(htmlThis, "marginRight");
			return result;
		},
		getBottom: function (index) {
			logger.error(`>>> Entering getBottom(index=${index})`, { "event.action": "getBottom", "logrite.index": index });
			var result = this.getTop(4) + this.height();
			var htmlThis = this[0];
			logger.error(`getBottom(index=${index}): result → ${result}`, { "event.action": "getBottom", "logrite.index": index });
			if (index < 4)
				result += DDR.Menu.getCSS(htmlThis, "paddingBottom");
			if (index < 3)
				result += DDR.Menu.getCSS(htmlThis, "borderBottomWidth");
			if (index < 2)
				result += DDR.Menu.getCSS(htmlThis, "marginBottom");
			return result;
		},
		alignElement: function (horizontalFn, horizontalVal, verticalFn, verticalVal) {
			logger.error(`>>> Entering alignElement(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal},verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "alignElement", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal, "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "left": "-999px", "top": "-999px", "position": "absolute", "display": "block" });
				horizontalVal -= (999 + horizontalFn());
				verticalVal -= (999 + verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "left": horizontalVal + "px", "top": verticalVal + "px" });
			});
			logger.warn(`<<< Exiting alignElement(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal},verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "alignElement", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal, "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
		},
		alignHorizontal: function (horizontalFn, horizontalVal) {
			logger.error(`>>> Entering alignHorizontal(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal})`, { "event.action": "alignHorizontal", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal });
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "left": "-999px", "position": "absolute", "display": "block" });
				horizontalVal -= (999 + horizontalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "left": horizontalVal + "px" });
			});
			logger.warn(`<<< Exiting alignHorizontal(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal})`, { "event.action": "alignHorizontal", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal });
		},
		alignVertical: function (verticalFn, verticalVal) {
			logger.error(`>>> Entering alignVertical(verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "alignVertical", "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "top": "-999px", "position": "absolute", "display": "block" });
				verticalVal -= (999 + verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "top": verticalVal + "px" });
			});
			logger.warn(`<<< Exiting alignVertical(verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "alignVertical", "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
		},
		sizeElement: function (horizontalFn, horizontalVal, verticalFn, verticalVal) {
			logger.error(`>>> Entering sizeElement(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal},verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "sizeElement", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal, "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
			return this.each(function () {
				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "width": "9999px", "height": "9999px", "display": "block" });
				horizontalVal += (9999 - horizontalFn());
				verticalVal += (9999 - verticalFn());
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
				jqThis.css({ "width": horizontalVal + "px", "height": verticalVal + "px" });
			});
			logger.warn(`<<< Exiting sizeElement(horizontalFn=${horizontalFn},horizontalVal=${horizontalVal},verticalFn=${verticalFn},verticalVal=${verticalVal})`, { "event.action": "sizeElement", "logrite.horizontalFn": horizontalFn, "logrite.horizontalVal": horizontalVal, "logrite.verticalFn": verticalFn, "logrite.verticalVal": verticalVal });
		},
		setContentWidth: function (width) {
			logger.error(`>>> Entering setContentWidth(width=${width})`, { "event.action": "setContentWidth", "logrite.width": width });
			return this.each(function () {
				var jqThis = $(this);
				jqThis.width($.support.boxModel ? width : width + jqThis.fullWidth() - jqThis.width());
			});
			logger.warn(`<<< Exiting setContentWidth(width=${width})`, { "event.action": "setContentWidth", "logrite.width": width });
		},
		setContentHeight: function (height) {
			logger.error(`>>> Entering setContentHeight(height=${height})`, { "event.action": "setContentHeight", "logrite.height": height });
			return this.each(function () {
				var jqThis = $(this);
				jqThis.height($.support.boxModel ? height : height + jqThis.fullHeight() - jqThis.height());
			});
			logger.warn(`<<< Exiting setContentHeight(height=${height})`, { "event.action": "setContentHeight", "logrite.height": height });
		},
		setFullWidth: function (width) {
			logger.error(`>>> Entering setFullWidth(width=${width})`, { "event.action": "setFullWidth", "logrite.width": width });
			return this.each(function () {
				var jqThis = $(this);
				var delta = jqThis.width() - jqThis.fullWidth();
				jqThis.width(width + delta);
				for (var n = 0; n < 100; n++) {
					var curWidth = jqThis.fullWidth();
					if (curWidth == width)
						return;
					delta += (width - curWidth);
					jqThis.width(width + delta);
					logger.warn(`<<< Exiting setFullWidth(width=${width})`, { "event.action": "setFullWidth", "logrite.width": width });
				}
			});
		},
		setFullHeight: function (height) {
			logger.error(`>>> Entering setFullHeight(height=${height})`, { "event.action": "setFullHeight", "logrite.height": height });
			return this.each(function () {
				var jqThis = $(this);
				var delta = jqThis.height() - jqThis.fullHeight();
				jqThis.height(height + delta);
				for (var n = 0; n < 100; n++) {
					var curHeight = jqThis.fullHeight();
					if (curHeight == height)
						return;
					delta += (height - curHeight);
					jqThis.height(height + delta);
					logger.warn(`<<< Exiting setFullHeight(height=${height})`, { "event.action": "setFullHeight", "logrite.height": height });
				}
			});
		},
		fullWidth: function () {
			logger.error('>>> Entering fullWidth()', { "event.action": "fullWidth" });
			var result = 0;
			this.each(function () {
				var max = $(this).outerWidth(true);
				if (max > result)
					result = max;
			});
			return result;
		},
		fullHeight: function () {
			logger.error('>>> Entering fullHeight()', { "event.action": "fullHeight" });
			var result = 0;
			this.each(function () {
				var max = $(this).outerHeight(true);
				if (max > result)
					result = max;
			});
			return result;
		},
		totalWidth: function () {
			logger.error('>>> Entering totalWidth()', { "event.action": "totalWidth" });
			var result = 0;
			this.each(function () {
				logger.error(`totalWidth(): result → ${result}`, { "event.action": "totalWidth" });
				result += $(this).outerWidth(true);
			});
			logger.warn('<<< Exiting totalWidth()', { "event.action": "totalWidth" });
			return result;
		},
		totalHeight: function () {
			logger.error('>>> Entering totalHeight()', { "event.action": "totalHeight" });
			var result = 0;
			this.each(function () {
				logger.error(`totalHeight(): result → ${result}`, { "event.action": "totalHeight" });
				result += $(this).outerHeight(true);
			});
			logger.warn('<<< Exiting totalHeight()', { "event.action": "totalHeight" });
			return result;
		},
		matchWidths: function () {
			logger.error('>>> Entering matchWidths()', { "event.action": "matchWidths" });
			logger.warn('<<< Exiting matchWidths()', { "event.action": "matchWidths" });
			return this.setFullWidth(this.fullWidth());
		},
		matchHeights: function () {
			logger.error('>>> Entering matchHeights()', { "event.action": "matchHeights" });
			logger.warn('<<< Exiting matchHeights()', { "event.action": "matchHeights" });
			return this.setFullHeight(this.fullHeight());
		},
		lineUpHorizontal: function (columns) {
			logger.error(`>>> Entering lineUpHorizontal(columns=${columns})`, { "event.action": "lineUpHorizontal", "logrite.columns": columns });
			var firstElt = $(this[0]);
			var left0 = left = firstElt.getLeft(1);
			logger.error(`lineUpHorizontal(columns=${columns}): firstElt → ${firstElt}`, { "event.action": "lineUpHorizontal", "logrite.columns": columns });
			logger.error(`lineUpHorizontal(columns=${columns}): left0 → ${left0}`, { "event.action": "lineUpHorizontal", "logrite.columns": columns });
			var top = firstElt.getTop(2);
			var column = 0;
			logger.error(`lineUpHorizontal(columns=${columns}): top → ${top}`, { "event.action": "lineUpHorizontal", "logrite.columns": columns });
			logger.warn(`<<< Exiting lineUpHorizontal(columns=${columns})`, { "event.action": "lineUpHorizontal", "logrite.columns": columns });
			return this.each(function () {
				var jqThis = $(this);
				jqThis.alignElement(
						function () { return jqThis.getLeft(1); }, left,
						function () { return jqThis.getTop(2); }, top
					);
				left += jqThis.fullWidth();
				column++;
				if (column == columns) {
					column = 0;
					left = left0;
					top += jqThis.fullHeight();
				}
			});
		},
		lineUpVertical: function () {
			logger.error('>>> Entering lineUpVertical()', { "event.action": "lineUpVertical" });
			var firstElt = $(this[0]);
			var left = firstElt.getLeft(2);
			logger.error(`lineUpVertical(): firstElt → ${firstElt}`, { "event.action": "lineUpVertical" });
			logger.error(`lineUpVertical(): left → ${left}`, { "event.action": "lineUpVertical" });
			var top = firstElt.getTop(1);
			logger.warn('<<< Exiting lineUpVertical()', { "event.action": "lineUpVertical" });
			return this.each(function () {
				var jqThis = $(this);
				jqThis.alignElement(
						function () { return jqThis.getLeft(2); }, left,
						function () { return jqThis.getTop(1); }, top
					);
				top += jqThis.fullHeight();
			});
		},
		fitToContent: function () {
			logger.error('>>> Entering fitToContent()', { "event.action": "fitToContent" });
			return this.each(function () {
				var maxRight = -999999;
				var maxBottom = -999999;

				var jqThis = $(this);
				var prevDisplay = jqThis.css("display");
				jqThis.css({ "display": "block" });
				jqThis.children().each(function () {
					logger.error(`fitToContent(): prevDisplay → ${prevDisplay}`, { "event.action": "fitToContent" });
					var jqThis = $(this);
					var right = jqThis.getRight(1);
					var bottom = jqThis.getBottom(1);
					if (right > maxRight)
						maxRight = right;
					if (bottom > maxBottom)
						maxBottom = bottom;
				});

				jqThis.sizeElement(
						function () { return jqThis.getRight(4); }, maxRight,
						function () { return jqThis.getBottom(4); }, maxBottom
					);
				if (prevDisplay != "block")
					jqThis.css("display", prevDisplay);
			});
		},
		stretchBlockHyperlinks: function () {
			logger.error('>>> Entering stretchBlockHyperlinks()', { "event.action": "stretchBlockHyperlinks" });
			this.find("a").each(function () {
				var jqThis = $(this);
				if (jqThis.css("display") == "block") {
					jqThis.setFullWidth(jqThis.parent().width());
					jqThis.setFullHeight(jqThis.parent().height());
					logger.warn('<<< Exiting stretchBlockHyperlinks()', { "event.action": "stretchBlockHyperlinks" });
				}
			});
		},
		setHoverClass: function (cssClass, alter, prevClass) {
			logger.error(`>>> Entering setHoverClass(cssClass=${cssClass},alter=${alter},prevClass=${prevClass})`, { "event.action": "setHoverClass", "logrite.cssClass": cssClass, "logrite.alter": alter, "logrite.prevClass": prevClass });
			return this.each(function () {
				if (cssClass) {
					var jqThis = $(this);
					logger.warn(`<<< Exiting setHoverClass(cssClass=${cssClass},alter=${alter},prevClass=${prevClass})`, { "event.action": "setHoverClass", "logrite.cssClass": cssClass, "logrite.alter": alter, "logrite.prevClass": prevClass });
					var jqAlter = alter || jqThis;
					jqThis.hover(
									function () {
										jqAlter.each(function () {
											if (!this.hoverClass) {
												this.hoverClass = this.className;
											}
										});
										jqAlter.addClass(cssClass);
										if (prevClass)
											jqAlter.removeClass(prevClass);
									},
									function () {
										jqAlter.each(function () {
											if (this.hoverClass) {
												this.className = this.hoverClass;
											}
										});
									}
								);
				}
			});
		},
		ddrData: function () {
			logger.error('>>> Entering ddrData()', { "event.action": "ddrData" });
			if (!this.data("ddrData"))
				this.data("ddrData", {});
			logger.warn('<<< Exiting ddrData()', { "event.action": "ddrData" });
			return this.data("ddrData");
		},
		setMenuData: function (level, parentItem) {
			logger.error(`>>> Entering setMenuData(level=${level},parentItem=${parentItem})`, { "event.action": "setMenuData", "logrite.level": level, "logrite.parentItem": parentItem });
			return this.each(function () {
				var jqThis = $(this);
				var childItems = jqThis.children("li");
				var id = jqThis.attr("nid");
				var path = "";
				if (parentItem != null) {
					path = parentItem.data("ddrData").path + "-";
				}

				jqThis.data("ddrData", {
					isMenu: true,
					id: id,
					level: level,
					path: path,
					childItems: childItems,
					parentItem: parentItem,
					rendered: jqThis,
					itemIndex: 0
				});
				childItems.setItemData(level, jqThis);
				if (childItems.length > 0) {
					jqThis.children("li:first").ddrData().first = true;
					jqThis.children("li:last").ddrData().last = true;
					logger.warn(`<<< Exiting setMenuData(level=${level},parentItem=${parentItem})`, { "event.action": "setMenuData", "logrite.level": level, "logrite.parentItem": parentItem });
				}
			});
		},
		setItemData: function (level, parentMenu) {
			this.each(function () {
				var jqThis = $(this);
				var childMenu = jqThis.children("ul");
				var allChildMenus = jqThis.find("ul");
				var allParentMenus = jqThis.parents("ul");
				var jqLink = jqThis.children("a");
				var jqParent = jqLink.length ? jqLink : jqThis;
				var jqImage = jqParent.children("img");
				var jqText = jqParent.children("span");
				var id = jqThis.attr("nid");
				var path = parentMenu.data("ddrData").path + parentMenu.data("ddrData").itemIndex++;

				jqThis.data("ddrData", {
					isItem: true,
					id: id,
					level: level,
					path: path,
					first: false,
					last: false,
					href: jqLink.attr("href"),
					image: jqImage.attr("src"),
					title: jqText.text(),
					isBreadcrumb: jqThis.hasClass("breadcrumb"),
					isSelected: jqThis.hasClass("selected"),
					isSeparator: jqThis.hasClass("separator"),
					childMenu: childMenu,
					parentMenu: parentMenu,
					allChildMenus: allChildMenus,
					allParentMenus: allParentMenus,
					rendered: jqThis
				});
				childMenu.setMenuData(level + 1, jqThis);
			});
			logger.warn(`<<< Exiting setItemData(level=${level},parentMenu=${parentMenu})`, { "event.action": "setItemData", "logrite.level": level, "logrite.parentMenu": parentMenu });
		},
		toDDRObject: function () {
			logger.error('>>> Entering toDDRObject()', { "event.action": "toDDRObject" });
			if (this.data("ddrObject"))
				return this.data("ddrObject");

			var data = this.ddrData();
			logger.error(`toDDRObject(): data → ${data}`, { "event.action": "toDDRObject" });
			var result = {};
			logger.error(`toDDRObject(): result → ${result}`, { "event.action": "toDDRObject" });
			this.data("ddrObject", result);
			var n = 0;
			for (var member in data) {
				if ((member != "rendered") && data[member] && data[member].jquery) {
					if (member.substr(member.length - 1) == "s") {
						result[member] = data[member].toDDRObjectArray();
					}
					else if (data[member].length == 0) {
						result[member] = null;
					}
					else {
						result[member] = data[member].toDDRObject();
					}
				}
				else {
					result[member] = data[member];
				}
				n++;
			}

			return result;
		},
		toDDRObjectArray: function () {
			logger.error('>>> Entering toDDRObjectArray()', { "event.action": "toDDRObjectArray" });
			logger.warn('<<< Exiting toDDRObjectArray()', { "event.action": "toDDRObjectArray" });
			return $.map(this, function (elt) { return $(elt).toDDRObject(); });
		}
	});

	DDR.Menu.createTable = function () {
		logger.error('>>> Entering createTable()', { "event.action": "createTable" });
		logger.warn('<<< Exiting createTable()', { "event.action": "createTable" });
		return $("<table />").attr("cellpadding", 0).attr("cellspacing", 0).attr("border", 0);
	}
	DDR.Menu.addTableCell = function (table, tdContents) {
		logger.error(`>>> Entering addTableCell(table=${table},tdContents=${tdContents})`, { "event.action": "addTableCell", "logrite.table": table, "logrite.tdContents": tdContents });
		var tr = table.find("tr:first");
		if (tr.length == 0)
			table.append(tr = $("<tr />"));
		var result = $("<td />").css({ "padding": 0, "margin": 0 }).append(tdContents);
		logger.error(`addTableCell(table=${table},tdContents=${tdContents}): result → ${result}`, { "event.action": "addTableCell", "logrite.table": table, "logrite.tdContents": tdContents });
		tr.append(result);
		logger.warn(`<<< Exiting addTableCell(table=${table},tdContents=${tdContents})`, { "event.action": "addTableCell", "logrite.table": table, "logrite.tdContents": tdContents });
		return result;
	}
	DDR.Menu.setTableColumns = function (table, columns) {
		logger.error(`>>> Entering setTableColumns(table=${table},columns=${columns})`, { "event.action": "setTableColumns", "logrite.table": table, "logrite.columns": columns });
		var tbody = table.children("tbody");
		if (tbody.length == 0)
			tbody = table;
		var trs = tbody.children("tr");
		logger.error(`setTableColumns(table=${table},columns=${columns}): trs → ${trs}`, { "event.action": "setTableColumns", "logrite.table": table, "logrite.columns": columns });
		var tds = trs.children("td");
		logger.error(`setTableColumns(table=${table},columns=${columns}): tds → ${tds}`, { "event.action": "setTableColumns", "logrite.table": table, "logrite.columns": columns });
		var tr = $("<tr />");
		tbody.append(tr);
		tds.each(function () {
			var td = $(this);
			tr.append(td);
			if (tr.children("td").length == columns) {
				tr = $("<tr />");
				tbody.append(tr);
			}
		});
		if (tr.children("td").length == 0) {
			tr.remove();
		}
		trs.remove();
		logger.warn(`<<< Exiting setTableColumns(table=${table},columns=${columns})`, { "event.action": "setTableColumns", "logrite.table": table, "logrite.columns": columns });
	}
});


DDR.Menu.registerMenu = function (clientID, dnnNavParams) {
	logger.error(`>>> Entering registerMenu(clientID=${clientID},dnnNavParams=${dnnNavParams})`, { "event.action": "registerMenu", "logrite.clientID": clientID, "logrite.dnnNavParams": dnnNavParams });
	document.write(
		"<style type='text/css'>" +
		"	#" + clientID + " {" +
		"		display: none;" +
		"	}" +
		"</style>"
		);

	DDRjQuery(function ($) {
		var jqContainer = $("#" + clientID);
		var rootMenu = jqContainer.children("ul");

		jqContainer.hide();
		while ((rootMenu.children("li").length == 0) && (rootMenu.children().length > 0)) {
			rootMenu.html(rootMenu.children().html());
		}
		if (rootMenu.children().length > 0) {
			rootMenu.setMenuData(0, null);

			new DDR.Menu.Providers[dnnNavParams.MenuStyle](jqContainer, dnnNavParams).createRootMenu();

			jqContainer.css({ "display": "block" });
			logger.warn(`<<< Exiting registerMenu(clientID=${clientID},dnnNavParams=${dnnNavParams})`, { "event.action": "registerMenu", "logrite.clientID": clientID, "logrite.dnnNavParams": dnnNavParams });
		}
	});
}
