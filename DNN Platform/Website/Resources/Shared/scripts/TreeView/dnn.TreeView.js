var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

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


    // The actual plugin constructor
    var TreeView = this.TreeView = function (element, options) {
        this.$this = $(this);
        this.$element = $(element);
        this.options = options;
        this.init();
    };

    TreeView.prototype = {

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend(TreeView.defaults(), this.options);
            this._onRedrawHandler = $.proxy(this._onRedraw, this);
            this._onShowChildrenHandler = $.proxy(this._onShowChildren, this);
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        _createChildrenNodeElement: function (childrenContext, isRoot) {
			logger.error(`>>> Entering _createChildrenNodeElement(childrenContext=${childrenContext},isRoot=${isRoot})`, { "event.action": "_createChildrenNodeElement", "logrite.childrenContext": childrenContext, "logrite.isRoot": isRoot });
            if (!childrenContext || childrenContext.length === 0) {
                return $([]);
            }
            var childContext;
            var $childNodeElement;
            var $childrenNodeElement = $("<ul/>");

            $childrenNodeElement.addClass(this.options.nodeListCss);
            if (isRoot) {
                $childrenNodeElement.addClass(this.options.rootCss);
            }
            for (var i = 0, count = childrenContext.length; i < count; i++) {
                childContext = childrenContext[i];
                if (childContext) {
					logger.debug(`_createChildrenNodeElement(childrenContext=${childrenContext},isRoot=${isRoot}): childContext → ${childContext}`, { "event.action": "_createChildrenNodeElement", "logrite.childrenContext": childrenContext, "logrite.isRoot": isRoot });
                    $childNodeElement = this._createNodeElement(childContext);
                    $childNodeElement.append(this._createChildrenNodeElement(childContext.children));
                    $childrenNodeElement.append($childNodeElement);
					logger.warn(`<<< Exiting _createChildrenNodeElement(childrenContext=${childrenContext},isRoot=${isRoot})`, { "event.action": "_createChildrenNodeElement", "logrite.childrenContext": childrenContext, "logrite.isRoot": isRoot });
                }
            }
            return $childrenNodeElement;
        },

        showTree: function (nodeContext) {
            if (!nodeContext) {
                return;
            }
            var nodes = Object.IsArray(nodeContext) ? nodeContext : [nodeContext];
            this._$treeElement = this._createChildrenNodeElement(nodes, true).hide();
            this.$element.empty().append(this._$treeElement);
            this._$treeElement.slideDown({ duration: 0, complete: this._onRedrawHandler });
			logger.warn(`<<< Exiting showTree(nodeContext=${nodeContext})`, { "event.action": "showTree", "logrite.nodeContext": nodeContext });
        },

        collapseTree: function (onComplete) {
            var self = this;
            if (this._$treeElement && this._$treeElement.length) {
                this._$treeElement.slideUp({
                    duration: 0,
                    complete: function() {
						logger.error('>>> Entering complete()', { "event.action": "complete" });
                        $(this).remove();
                        self._onRedraw();
                        typeof onComplete === "function" && onComplete.apply(self);
						logger.warn('<<< Exiting complete()', { "event.action": "complete" });
                    }
                });
            }
            else {
                typeof onComplete === "function" && onComplete.apply(self);
            }
			logger.warn(`<<< Exiting collapseTree(onComplete=${onComplete})`, { "event.action": "collapseTree", "logrite.onComplete": onComplete });
        },

        showChildren: function (nodeContext) {
            if (!nodeContext) {
                return null;
            }
            var $childrenNodeElement = this._createChildrenNodeElement(nodeContext.children).hide();
            var $nodeElement = this.getNodeElement(nodeContext);
            $nodeElement.children('ul').remove();
            $nodeElement.append($childrenNodeElement);
            $childrenNodeElement.slideDown({ duration: this.options.expandSpeed, easing: this.options.expandEasing, complete: this._onShowChildrenHandler });
			logger.warn(`<<< Exiting showChildren(nodeContext=${nodeContext})`, { "event.action": "showChildren", "logrite.nodeContext": nodeContext });
            return $childrenNodeElement;
        },

        removeChildren: function (nodeElement) {
            var self = this;
            var nodeContext = this.nodeContext(nodeElement);
            $(nodeElement).find('ul').slideUp(this.options.collapseSpeed, this.options.collapseEasing, function () { $(this).remove(); self._onRedraw(nodeContext); });
			logger.warn(`<<< Exiting removeChildren(nodeElement=${nodeElement})`, { "event.action": "removeChildren", "logrite.nodeElement": nodeElement });
        },

        _onRedraw: function (nodeContext) {
			logger.error(`>>> Entering _onRedraw(nodeContext=${nodeContext})`, { "event.action": "_onRedraw", "logrite.nodeContext": nodeContext });
            var e = $.Event('onredrawtree');
            this.$this.trigger(e, [nodeContext]);
			logger.error(`_onRedraw(nodeContext=${nodeContext}): e → ${e}`, { "event.action": "_onRedraw", "logrite.nodeContext": nodeContext });
			logger.warn(`<<< Exiting nodeContext(element=${element},context=${context})`, { "event.action": "nodeContext", "logrite.element": element, "logrite.context": context });
        },
        
        _onShowChildren: function (nodeContext) {
			logger.error(`>>> Entering _onShowChildren(nodeContext=${nodeContext})`, { "event.action": "_onShowChildren", "logrite.nodeContext": nodeContext });
            var e = $.Event('onredrawtree');
            this.$this.trigger(e, [nodeContext]);
			logger.error(`_onShowChildren(nodeContext=${nodeContext}): e → ${e}`, { "event.action": "_onShowChildren", "logrite.nodeContext": nodeContext });

            this.$this.trigger('onshowchildren', [nodeContext]);
			logger.warn(`<<< Exiting _onShowChildren(nodeContext=${nodeContext})`, { "event.action": "_onShowChildren", "logrite.nodeContext": nodeContext });
        },

        getNodeElement: function (nodeContext) {
			logger.error(`>>> Entering getNodeElement(nodeContext=${nodeContext})`, { "event.action": "getNodeElement", "logrite.nodeContext": nodeContext });
            var rootElement = this.getRootElement();
            var self = this;
			logger.error(`getNodeElement(nodeContext=${nodeContext}): rootElement → ${rootElement}`, { "event.action": "getNodeElement", "logrite.nodeContext": nodeContext });
            var filterFunction = function (i) {
				logger.error(`>>> Entering filterFunction(i=${i})`, { "event.action": "filterFunction", "logrite.i": i });
                var context = self.nodeContext(this);
				logger.error(`filterFunction(i=${i}): context → ${context}`, { "event.action": "filterFunction", "logrite.i": i });
                return context === nodeContext;
            };
            //var list = $(rootElement).find('li').andSelf().filter(filterFunction);
            var list = $(rootElement).find('li').filter(filterFunction);
			logger.warn(`<<< Exiting getNodeElement(nodeContext=${nodeContext})`, { "event.action": "getNodeElement", "logrite.nodeContext": nodeContext });
			logger.error(`getNodeElement(nodeContext=${nodeContext}): list → ${list}`, { "event.action": "getNodeElement", "logrite.nodeContext": nodeContext });
            return list.first();
        },

        _createNodeElement: function (nodeContext) {
			logger.error(`>>> Entering _createNodeElement(nodeContext=${nodeContext})`, { "event.action": "_createNodeElement", "logrite.nodeContext": nodeContext });
            if (!nodeContext) {
                return null;
            }
            var $nodeElement = $("<li/>").addClass(this.options.nodeCss);
            this.nodeContext($nodeElement[0], nodeContext);
            this.$this.trigger("oncreatenode", [$nodeElement[0], nodeContext ]);
			logger.warn(`<<< Exiting _createNodeElement(nodeContext=${nodeContext})`, { "event.action": "_createNodeElement", "logrite.nodeContext": nodeContext });
            return $nodeElement;
        },

        /*_removeSelections: function() {
			var rootElement = this.getRootElement();
			$(rootElement).find('li').andSelf().each(function (index, element) { $(element).children().eq(1).removeClass("selected"); });
		},*/

        addNodeClass: function (nodeContext, add, className) {
            if (!nodeContext) {
                return;
            }
            var $nodeElement = this.getNodeElement(nodeContext);
            var addClass = typeof (add) === "undefined" || add;
            var element = $nodeElement.children().eq(1);
            if (addClass) {
                element.addClass(className);
            }
            else {
                element.removeClass(className);
            }
        },

        getRootElement: function () {
			logger.error('>>> Entering getRootElement()', { "event.action": "getRootElement" });
            var $node = this._$treeElement; //this.$element.find("ul li:first");
			logger.warn('<<< Exiting getRootElement()', { "event.action": "getRootElement" });
            return ($node && $node.length > 0) ? $node[0] : null;
        },

//        _getParentNodeElement: function (node) {
//            if (node && node !== this.getRootElement()) {
//                var $parent = $(node).parent().parent();
//                if ($parent && $parent.length > 0) {
//                    return $parent[0];
//                }
//            }
//            return null;
//        },

        nodeContext: function (element, context) {
            var nodeElement = (element) ? $(element).closest("li")[0] : this.getRootElement();
            if (!nodeElement) {
                return null;
            }
            if (typeof context === "undefined") {
                return $.data(nodeElement, "tree-node");
            }
            else {
                $.data(nodeElement, "tree-node", context);
            }
            return context;
        }

    };

    TreeView._defaults = {
        expandSpeed: 200, // default = 200 (ms); use -1 for no animation
        collapseSpeed: 200, // default = 200 (ms); use -1 for no animation
        expandEasing: null, // easing function to use on expand (optional)
        collapseEasing: null, //easing function to use on collapse (optional)
        nodeListCss: "tv-nodes",
        nodeCss: "tv-node",
        rootCss: "tv-root"
    };

    TreeView.defaults = function (settings, dataAndMethods) {
		logger.error(`>>> Entering defaults(settings=${settings},dataAndMethods=${dataAndMethods})`, { "event.action": "defaults", "logrite.settings": settings, "logrite.dataAndMethods": dataAndMethods });
        if (typeof settings !== "undefined") {
            $.extend(TreeView._defaults, settings);
        }
        if (typeof dataAndMethods !== "undefined") {
            $.extend(TreeView._defaults, dataAndMethods);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings},dataAndMethods=${dataAndMethods})`, { "event.action": "defaults", "logrite.settings": settings, "logrite.dataAndMethods": dataAndMethods });
        return TreeView._defaults;
    };

}).apply(dnn, [jQuery, window, document]);

