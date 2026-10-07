var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof dnn === "undefined") { dnn = {}; }; //var dnn = dnn || {};

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($, window, document, undefined) {
    "use strict";

    var pluginName = "jScrollbar";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var Scrollbar = this.Scrollbar = function (element, options) {
		logger.error(`>>> Entering Scrollbar(element=${element},options=${options})`, { "event.action": "Scrollbar", "logrite.element": element, "logrite.options": options });
        this.element = element;
        this.options = options;
		logger.debug(`Scrollbar(element=${element},options=${options}): element → ${element}`, { "event.action": "Scrollbar", "logrite.element": element, "logrite.options": options });
		logger.debug(`Scrollbar(element=${element},options=${options}): this.element → ${this.element}`, { "event.action": "Scrollbar", "logrite.element": element, "logrite.options": options });
        this.init();
    };

    Scrollbar.prototype = {
        constructor: Scrollbar,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, Scrollbar.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element).addClass('ps-container');

            this._onMouseMoveXHandler = $.proxy(this._onMouseMoveX, this);
            this._onMouseMoveYHandler = $.proxy(this._onMouseMoveY, this);
            this._onMouseUpXHandler = $.proxy(this._onMouseUpX, this);
            this._onMouseUpYHandler = $.proxy(this._onMouseUpY, this);

            this._$content = this.$element.children();
            this._$scrollbarX = $("<div class='ps-scrollbar-x'></div>").appendTo(this.$element);
            this._$scrollbarX.on('mousedown.perfect-scroll', $.proxy(this._onMouseDownX, this));

            this._$scrollbarY = $("<div class='ps-scrollbar-y'></div>").appendTo(this.$element);
            this._$scrollbarY.on('mousedown.perfect-scroll', $.proxy(this._onMouseDownY, this));

            this._scrollbarXBottom = parseInt(this._$scrollbarX.css('bottom'), 10);
            this._scrollbarYRight = parseInt(this._$scrollbarY.css('right'), 10);

            if (this.$element.mousewheel) {
                this.$element.mousewheel($.proxy(this._onMouseWheel, this));
            }
            this.update();
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        destroy: function () {
			logger.error('>>> Entering destroy()', { "event.action": "destroy" });
            this._$scrollbarX.remove();
            this._$scrollbarY.remove();
            this.$element.off("mousewheel");
			logger.warn('<<< Exiting destroy()', { "event.action": "destroy" });
        },

        _scrollY: function () {
			logger.error('>>> Entering _scrollY()', { "event.action": "_scrollY" });
            var scrollTop = parseInt(this._scrollbarYTop * this._contentHeight / this._containerHeight, 10);
            this.$element.scrollTop(scrollTop);
			logger.error(`_scrollY(): scrollTop → ${scrollTop}`, { "event.action": "_scrollY" });
            this._$scrollbarX.css({ bottom: this._scrollbarXBottom - scrollTop });
			logger.warn('<<< Exiting _scrollY()', { "event.action": "_scrollY" });
        },

        _scrollX: function () {
			logger.error('>>> Entering _scrollX()', { "event.action": "_scrollX" });
            var scrollLeft = parseInt(this._scrollbarXLeft * this._contentWidth / this._containerWidth, 10);
            this.$element.scrollLeft(scrollLeft);
			logger.error(`_scrollX(): scrollLeft → ${scrollLeft}`, { "event.action": "_scrollX" });
            this._$scrollbarY.css({ right: this._scrollbarYRight - scrollLeft });
			logger.warn('<<< Exiting _scrollX()', { "event.action": "_scrollX" });
        },

        update: function () {
			logger.error('>>> Entering update()', { "event.action": "update" });
            this._containerWidth = this.$element.width();
            this._containerHeight = this.$element.height();
            this._contentWidth = this._$content.outerWidth(false);
            this._contentHeight = this._$content.outerHeight(false);
            if (this._containerWidth < this._contentWidth) {
                this._scrollbarXWidth = parseInt(this._containerWidth * this._containerWidth / this._contentWidth, 10);
                this._scrollbarXLeft = parseInt(this.$element.scrollLeft() * this._containerWidth / this._contentWidth, 10);
            }
            else {
                this._scrollbarXWidth = 0;
                this._scrollbarXLeft = 0;
				logger.debug(`update(): this._scrollbarXWidth → ${this._scrollbarXWidth}`, { "event.action": "update" });
                this.$element.scrollLeft(0);
            }
            if (this._containerHeight < this._contentHeight) {
				logger.debug(`update(): this._scrollbarXLeft → ${this._scrollbarXLeft}`, { "event.action": "update" });
                this._scrollbarYHeight = parseInt(this._containerHeight * this._containerHeight / this._contentHeight, 10);
                this._scrollbarYTop = parseInt(this.$element.scrollTop() * this._containerHeight / this._contentHeight, 10);
            }
            else {
                this._scrollbarYHeight = 0;
                this._scrollbarYTop = 0;
				logger.debug(`update(): this._scrollbarYHeight → ${this._scrollbarYHeight}`, { "event.action": "update" });
                this.$element.scrollTop(0);
            }

			logger.debug(`update(): this._scrollbarYTop → ${this._scrollbarYTop}`, { "event.action": "update" });
            if (this._scrollbarYTop >= this._containerHeight - this._scrollbarYHeight) {
                this._scrollbarYTop = this._containerHeight - this._scrollbarYHeight;
            }
            if (this._scrollbarXLeft >= this._containerWidth - this._scrollbarXWidth) {
                this._scrollbarXLeft = this._containerWidth - this._scrollbarXWidth;
            }

            this._$scrollbarX.css({ left: this._scrollbarXLeft + this.$element.scrollLeft(), bottom: this._scrollbarXBottom - this.$element.scrollTop(), width: this._scrollbarXWidth });
            this._$scrollbarY.css({ top: this._scrollbarYTop + this.$element.scrollTop(), right: this._scrollbarYRight - this.$element.scrollLeft(), height: this._scrollbarYHeight });
			logger.warn('<<< Exiting update()', { "event.action": "update" });
        },

        _moveBarX: function (currentLeft, deltaX) {
			logger.error(`>>> Entering _moveBarX(currentLeft=${currentLeft},deltaX=${deltaX})`, { "event.action": "_moveBarX", "logrite.currentLeft": currentLeft, "logrite.deltaX": deltaX });
            var newLeft = currentLeft + deltaX;
            var maxLeft = this._containerWidth - this._scrollbarXWidth;

            if (newLeft < 0) {
                this._scrollbarXLeft = 0;
				logger.debug(`_moveBarX(currentLeft=${currentLeft},deltaX=${deltaX}): this._scrollbarXLeft → ${this._scrollbarXLeft}`, { "event.action": "_moveBarX", "logrite.currentLeft": currentLeft, "logrite.deltaX": deltaX });
            }
            else if (newLeft > maxLeft) {
                this._scrollbarXLeft = maxLeft;
            }
            else {
                this._scrollbarXLeft = newLeft;
            }
            this._$scrollbarX.css({ left: this._scrollbarXLeft + this.$element.scrollLeft() });
        },

        _moveBarY: function (currentTop, deltaY) {
			logger.error(`>>> Entering _moveBarY(currentTop=${currentTop},deltaY=${deltaY})`, { "event.action": "_moveBarY", "logrite.currentTop": currentTop, "logrite.deltaY": deltaY });
            var newTop = currentTop + deltaY;
            var maxTop = this._containerHeight - this._scrollbarYHeight;

            if (newTop < 0) {
                this._scrollbarYTop = 0;
				logger.debug(`_moveBarY(currentTop=${currentTop},deltaY=${deltaY}): this._scrollbarYTop → ${this._scrollbarYTop}`, { "event.action": "_moveBarY", "logrite.currentTop": currentTop, "logrite.deltaY": deltaY });
            }
            else if (newTop > maxTop) {
                this._scrollbarYTop = maxTop;
            }
            else {
                this._scrollbarYTop = newTop;
            }
            this._$scrollbarY.css({ top: this._scrollbarYTop + this.$element.scrollTop() });
        },

        _getDocument: function() {
			logger.error('>>> Entering _getDocument()', { "event.action": "_getDocument" });
			logger.warn('<<< Exiting _getDocument()', { "event.action": "_getDocument" });
            return this.options.document || document;
        },

        _onMouseDownX: function (e) {
			logger.error(`>>> Entering _onMouseDownX(e=${e})`, { "event.action": "_onMouseDownX", "logrite.e": e });
            this._currentPageX = e.pageX;
            this._currentLeft = this._$scrollbarX.position().left;
            this._$scrollbarX.addClass('in-scrolling');
			logger.debug(`_onMouseDownX(e=${e}): this._currentPageX → ${this._currentPageX}`, { "event.action": "_onMouseDownX", "logrite.e": e });
            $(this._getDocument()).on('mousemove.perfect-scroll', this._onMouseMoveXHandler);
            $(this._getDocument()).on('mouseup.perfect-scroll', this._onMouseUpXHandler);
            e.stopPropagation();
            e.preventDefault();
			logger.warn(`<<< Exiting _onMouseDownX(e=${e})`, { "event.action": "_onMouseDownX", "logrite.e": e });
        },

        _onMouseDownY: function (e) {
			logger.error(`>>> Entering _onMouseDownY(e=${e})`, { "event.action": "_onMouseDownY", "logrite.e": e });
            this._currentPageY = e.pageY;
            this._currentTop = this._$scrollbarY.position().top;
            this._$scrollbarY.addClass('in-scrolling');
			logger.debug(`_onMouseDownY(e=${e}): this._currentPageY → ${this._currentPageY}`, { "event.action": "_onMouseDownY", "logrite.e": e });
            $(this._getDocument()).on('mousemove.perfect-scroll', this._onMouseMoveYHandler);
            $(this._getDocument()).on('mouseup.perfect-scroll', this._onMouseUpYHandler);
            e.stopPropagation();
            e.preventDefault();
			logger.warn(`<<< Exiting _onMouseDownY(e=${e})`, { "event.action": "_onMouseDownY", "logrite.e": e });
        },

        _onMouseMoveX: function (e) {
			logger.error(`>>> Entering _onMouseMoveX(e=${e})`, { "event.action": "_onMouseMoveX", "logrite.e": e });
            if (this._$scrollbarX.hasClass('in-scrolling')) {
                this._moveBarX(this._currentLeft, e.pageX - this._currentPageX);
                this._scrollX();
                e.stopPropagation();
                e.preventDefault();
				logger.warn(`<<< Exiting _onMouseMoveX(e=${e})`, { "event.action": "_onMouseMoveX", "logrite.e": e });
            }
        },

        _onMouseMoveY: function (e) {
			logger.error(`>>> Entering _onMouseMoveY(e=${e})`, { "event.action": "_onMouseMoveY", "logrite.e": e });
            if (this._$scrollbarY.hasClass('in-scrolling')) {
                this._moveBarY(this._currentTop, e.pageY - this._currentPageY);
                this._scrollY();
                e.stopPropagation();
                e.preventDefault();
				logger.warn(`<<< Exiting _onMouseMoveY(e=${e})`, { "event.action": "_onMouseMoveY", "logrite.e": e });
            }
        },

        _onMouseUpX: function (e) {
			logger.error(`>>> Entering _onMouseUpX(e=${e})`, { "event.action": "_onMouseUpX", "logrite.e": e });
            if (this._$scrollbarX.hasClass('in-scrolling')) {
                this._$scrollbarX.removeClass('in-scrolling');
            }
            $(this._getDocument()).off('mousemove.perfect-scroll', this._onMouseMoveXHandler);
            $(this._getDocument()).off('mouseup.perfect-scroll', this._onMouseUpXHandler);
			logger.warn(`<<< Exiting _onMouseUpX(e=${e})`, { "event.action": "_onMouseUpX", "logrite.e": e });
        },

        _onMouseUpY: function (e) {
			logger.error(`>>> Entering _onMouseUpY(e=${e})`, { "event.action": "_onMouseUpY", "logrite.e": e });
            if (this._$scrollbarY.hasClass('in-scrolling')) {
                this._$scrollbarY.removeClass('in-scrolling');
            }
            $(this._getDocument()).off('mousemove.perfect-scroll', this._onMouseMoveYHandler);
            $(this._getDocument()).off('mouseup.perfect-scroll', this._onMouseUpYHandler);
			logger.warn(`<<< Exiting _onMouseUpY(e=${e})`, { "event.action": "_onMouseUpY", "logrite.e": e });
        },

        _onMouseWheel: function (e, delta, deltaX, deltaY) {
			logger.error(`>>> Entering _onMouseWheel(e=${e},delta=${delta},deltaX=${deltaX},deltaY=${deltaY})`, { "event.action": "_onMouseWheel", "logrite.e": e, "logrite.delta": delta, "logrite.deltaX": deltaX, "logrite.deltaY": deltaY });
            this.$element.scrollTop(this.$element.scrollTop() - (deltaY * this.options.wheelSpeed));
            this.$element.scrollLeft(this.$element.scrollLeft() + (deltaX * this.options.wheelSpeed));

            // update bar position
            this.update();

            if (this._shouldPreventDefault(deltaX, deltaY)) {
                e.preventDefault();
				logger.warn(`<<< Exiting _onMouseWheel(e=${e},delta=${delta},deltaX=${deltaX},deltaY=${deltaY})`, { "event.action": "_onMouseWheel", "logrite.e": e, "logrite.delta": delta, "logrite.deltaX": deltaX, "logrite.deltaY": deltaY });
            }
        },

        _shouldPreventDefault: function (deltaX, deltaY) {
			logger.error(`>>> Entering _shouldPreventDefault(deltaX=${deltaX},deltaY=${deltaY})`, { "event.action": "_shouldPreventDefault", "logrite.deltaX": deltaX, "logrite.deltaY": deltaY });
            var scrollTop = this.$element.scrollTop();
            if (scrollTop === 0 && deltaY > 0 && deltaX === 0) {
				logger.warn(`<<< Exiting _shouldPreventDefault(deltaX=${deltaX},deltaY=${deltaY})`, { "event.action": "_shouldPreventDefault", "logrite.deltaX": deltaX, "logrite.deltaY": deltaY });
				logger.error(`_shouldPreventDefault(deltaX=${deltaX},deltaY=${deltaY}): scrollTop → ${scrollTop}`, { "event.action": "_shouldPreventDefault", "logrite.deltaX": deltaX, "logrite.deltaY": deltaY });
                return !this.options.wheelPropagation;
            }
            else if (scrollTop >= this._contentHeight - this._containerHeight && deltaY < 0 && deltaX === 0) {
                return !this.options.wheelPropagation;
            }

            var scrollLeft = this.$element.scrollLeft();
            if (scrollLeft === 0 && deltaX < 0 && deltaY === 0) {
                return !this.options.wheelPropagation;
            }
            else if (scrollLeft >= this._contentWidth - this._containerWidth && deltaX > 0 && deltaY === 0) {
                return !this.options.wheelPropagation;
            }
            return true;
        }

    };

    Scrollbar._defaults = { wheelSpeed: 10, wheelPropagation: false };

    Scrollbar.defaults = function (settings) {
		logger.error(`>>> Entering defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        if (typeof settings !== "undefined") {
            $.extend(Scrollbar._defaults, settings);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        return Scrollbar._defaults;
    };

    $.fn[pluginName] = function (option) {

        // get the arguments
        var args = $.makeArray(arguments);
        var params = args.slice(1);

        return this.each(function () {
            var $this = $(this);
            var instance = $this.data(pluginName);
            if (!instance) {
                var options = $.extend({}, typeof option === "object" && option);
                $this.data(pluginName, (instance = new Scrollbar(this, options)));
            }
            if (typeof option === "string") {
                instance[option].apply(instance, params);
            }
        });

    };

}).apply(dnn, [jQuery, window, document]);

