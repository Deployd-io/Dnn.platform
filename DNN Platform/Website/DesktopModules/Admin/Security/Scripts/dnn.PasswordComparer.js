var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };  //var dnn = dnn || {}; IE 8 doesn't respect var

// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var pluginName = 'inputComparer';

    // The actual plugin constructor
    var InputComparer = this.InputComparer = function (element, options) {
		logger.error(`>>> Entering InputComparer(element=${element},options=${options})`, { "event.action": "InputComparer", "logrite.element": element, "logrite.options": options });
        this.element = element;
        this.options = options;
		logger.debug(`InputComparer(element=${element},options=${options}): element → ${element}`, { "event.action": "InputComparer", "logrite.element": element, "logrite.options": options });
		logger.debug(`InputComparer(element=${element},options=${options}): this.element → ${this.element}`, { "event.action": "InputComparer", "logrite.element": element, "logrite.options": options });
		logger.debug(`InputComparer(element=${element},options=${options}): options → ${options}`, { "event.action": "InputComparer", "logrite.element": element, "logrite.options": options });

        this.valid = this.init();
    };

    InputComparer.prototype = {

        constructor: InputComparer,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, InputComparer.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);

            this._$firstElement = this.$element.find(this.options.firstElementSelector);
            if (this._$firstElement.length === 0) {
                return false;
            }
            this._$secondElement = this.$element.find(this.options.secondElementSelector);
            if (this._$secondElement.length === 0) {
                return false;
            }
            this._focusOutHandler = $.proxy(this._onFocusOut, this);
            this._$firstElement.on('focusout change keyup paste input propertychange', this._focusOutHandler);
            this._$secondElement.on('focusout change keyup paste input propertychange', this._focusOutHandler);

			logger.warn('<<< Exiting init()', { "event.action": "init" });
            return true;
        },

        _onFocusOut: function (eventObject) {
			logger.error(`>>> Entering _onFocusOut(eventObject=${eventObject})`, { "event.action": "_onFocusOut", "logrite.eventObject": eventObject });
            if (eventObject.type === 'propertychange' && eventObject.originalEvent.propertyName.toLowerCase() !== 'value') {
		        return;
            }

            if (eventObject.target === this._$firstElement[0]) {
                // leaving the first element
                this._isSecondElementVisited && this._compare(this._$firstElement, this._$secondElement);
            }
            else {
                // leaving the second element
                this._isSecondElementVisited = true;
                this._compare(this._$firstElement, this._$secondElement);
				logger.warn(`<<< Exiting _onFocusOut(eventObject=${eventObject})`, { "event.action": "_onFocusOut", "logrite.eventObject": eventObject });
				logger.debug(`_onFocusOut(eventObject=${eventObject}): this._isSecondElementVisited → ${this._isSecondElementVisited}`, { "event.action": "_onFocusOut", "logrite.eventObject": eventObject });
            }
        },

        _compare: function ($first, $second) {
			logger.error(`>>> Entering _compare($first=${$first},$second=${$second})`, { "event.action": "_compare", "logrite.$first": $first, "logrite.$second": $second });
            var firstText = $first.val();
            var secondText = $second.val();
			logger.error(`_compare($first=${$first},$second=${$second}): firstText → ${firstText}`, { "event.action": "_compare", "logrite.$first": $first, "logrite.$second": $second });
			logger.error(`_compare($first=${$first},$second=${$second}): secondText → ${secondText}`, { "event.action": "_compare", "logrite.$first": $first, "logrite.$second": $second });
            var e = $.Event('on-compare');
            this.$this.trigger(e, [firstText, secondText]);
			logger.error(`_compare($first=${$first},$second=${$second}): e → ${e}`, { "event.action": "_compare", "logrite.$first": $first, "logrite.$second": $second });
        },

        compare: function () {
			logger.error('>>> Entering compare()', { "event.action": "compare" });
            this._compare(this._$firstElement, this._$secondElement);
			logger.warn('<<< Exiting compare()', { "event.action": "compare" });
        }

    };

    InputComparer._defaults = {};

    InputComparer.defaults = function (settings) {
		logger.error(`>>> Entering defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        if (typeof settings !== "undefined") {
            $.extend(InputComparer._defaults, settings);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        return InputComparer._defaults;
    };

    $.fn[pluginName] = function (option) {

        return this.each(function () {

            // get the arguments
            var args = $.makeArray(arguments);
            var params = args.slice(1);

            return this.each(function () {
                var $this = $(this);
                var instance = $this.data(pluginName);
                if (!instance) {
                    var options = $.extend({}, typeof option === "object" && option);
                    $this.data(pluginName, (instance = new InputComparer(this, options)));
                }
                if (typeof option === "string") {
                    instance[option].apply(instance, params);
                }
            });
        });
    };


    var PasswordComparer = this.PasswordComparer = function (options) {
		logger.error(`>>> Entering PasswordComparer(options=${options})`, { "event.action": "PasswordComparer", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`PasswordComparer(options=${options}): this.options → ${this.options}`, { "event.action": "PasswordComparer", "logrite.options": options });
    };

    PasswordComparer.prototype = {

        constructor: PasswordComparer,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options

            this.$this = $(this);

            this._$container = $(this.options.containerSelector);
            this._comparer = new InputComparer(this._$container, this.options);
            
            if (!this._comparer.valid) {
                return;
            }

            var onCompareHandler = $.proxy(this._onCompare, this);
			logger.error(`init(): onCompareHandler → ${onCompareHandler}`, { "event.action": "init" });
            $(this._comparer).on("on-compare", onCompareHandler);

            this._$confirmElement = this._$container.find(this.options.secondElementSelector);

            this._$confirmElement.dnntooltip({
                getTooltipMarkup: $.proxy(this._getTooltipMarkup, this),
                cssClass: "confirm-password-tooltip",
                left: 214,
                top: -32,
                closeOnMouseLeave: true,
                disabled: true
            });

            this._comparer.compare();
        },

        _getTooltipMarkup: function (element) {
			logger.error(`>>> Entering _getTooltipMarkup(element=${element})`, { "event.action": "_getTooltipMarkup", "logrite.element": element });
            if (!this._$tooltipContent) {
                this._$tooltipContent = $("<label/>").addClass("confirm-password-tooltip-content");
                this._updateTooltipState(this._isMatched);
				logger.warn(`<<< Exiting _getTooltipMarkup(element=${element})`, { "event.action": "_getTooltipMarkup", "logrite.element": element });
            }
            return this._$tooltipContent;
        },

        _updateTooltipState: function (isMatched) {
			logger.error(`>>> Entering _updateTooltipState(isMatched=${isMatched})`, { "event.action": "_updateTooltipState", "logrite.isMatched": isMatched });
            if (!this._$tooltipContent) {
                return;
            }
            var text = isMatched ? this.options.confirmPasswordMatchedText : this.options.confirmPasswordUnmatchedText;
            this._$tooltipContent.text(text);
			logger.warn(`<<< Exiting _updateTooltipState(isMatched=${isMatched})`, { "event.action": "_updateTooltipState", "logrite.isMatched": isMatched });
        },

        _onCompare: function (eventObject, val1, val2) {
			logger.error(`>>> Entering _onCompare(eventObject=${eventObject},val1=${val1},val2=${val2})`, { "event.action": "_onCompare", "logrite.eventObject": eventObject, "logrite.val1": val1, "logrite.val2": val2 });
            this._$confirmElement.removeClass(this.options.unmatchedCssClass);
            this._$confirmElement.removeClass(this.options.matchedCssClass);

            this._isMatched = (val1 === val2);
            if (this._isMatched) {
                if (val1) {
                    // bith fields are not empty
                    this._$confirmElement.addClass(this.options.matchedCssClass);
                }
            }
            else {
                this._$confirmElement.addClass(this.options.unmatchedCssClass);
            }

            var disabled = !val1 || !val2;
            this._$confirmElement.dnntooltip("disabled", disabled);

            this._updateTooltipState(this._isMatched);
			logger.warn(`<<< Exiting _onCompare(eventObject=${eventObject},val1=${val1},val2=${val2})`, { "event.action": "_onCompare", "logrite.eventObject": eventObject, "logrite.val1": val1, "logrite.val2": val2 });
        }

    };

}).apply(dnn, [jQuery, window, document]);


dnn.initializePasswordComparer = function(options) {
	logger.error(`>>> Entering initializePasswordComparer(options=${options})`, { "event.action": "initializePasswordComparer", "logrite.options": options });
    $(document).ready(function() { var instance = new dnn.PasswordComparer(options); });
	logger.warn(`<<< Exiting initializePasswordComparer(options=${options})`, { "event.action": "initializePasswordComparer", "logrite.options": options });
};
