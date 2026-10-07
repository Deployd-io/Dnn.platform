var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    //
    // DynamicTreeView
    //
    var DynamicTreeView = this.DynamicTreeView = function (element, options, controller) {
		logger.error(`>>> Entering DynamicTreeView(element=${element},options=${options},controller=${controller})`, { "event.action": "DynamicTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
        this.element = element;
        this.options = options;
		logger.debug(`DynamicTreeView(element=${element},options=${options},controller=${controller}): element → ${element}`, { "event.action": "DynamicTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`DynamicTreeView(element=${element},options=${options},controller=${controller}): this.element → ${this.element}`, { "event.action": "DynamicTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`DynamicTreeView(element=${element},options=${options},controller=${controller}): options → ${options}`, { "event.action": "DynamicTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`DynamicTreeView(element=${element},options=${options},controller=${controller}): controller → ${controller}`, { "event.action": "DynamicTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
        this.controller = controller;

        this.init();
    };

    DynamicTreeView.prototype = {

        constructor: DynamicTreeView,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, DynamicTreeView.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);

            if (this.options.scroll) {
                this.$element.jScrollbar();
            }

            this._numberOfNodes = 0;

			logger.debug(`init(): this._numberOfNodes → ${this._numberOfNodes}`, { "event.action": "init" });
            var $tree = this.$element.find("." + this.options.treeContainerCss);
            this._tree = new dnn.TreeView($tree[0], { showRoot: true });

            var onRedrawTreeHandler = $.proxy(this._onRedrawTree, this);
			logger.error(`init(): onRedrawTreeHandler → ${onRedrawTreeHandler}`, { "event.action": "init" });
            $(this._tree).on("onredrawtree", onRedrawTreeHandler);

            var onCreateNodeElementHandler = $.proxy(this._onCreateNodeElement, this);
			logger.error(`init(): onCreateNodeElementHandler → ${onCreateNodeElementHandler}`, { "event.action": "init" });
            $(this._tree).on("oncreatenode", onCreateNodeElementHandler);
            
            var onShowChildrenHandler = $.proxy(this._onShowChildren, this);
			logger.error(`init(): onShowChildrenHandler → ${onShowChildrenHandler}`, { "event.action": "init" });
            $(this._tree).on("onshowchildren", onShowChildrenHandler);

            this._onNodeIconClickHandler = $.proxy(this._onNodeIconClick, this);
            this._onNodeTextClickHandler = $.proxy(this._onNodeTextClick, this);

            this._firstNode = this.options.firstNode;

		logger.debug(`init(): this._firstNode → ${this._firstNode}`, { "event.action": "init" });
        },

        _onCreateNodeElement: function (eventObject, nodeElement, nodeContext) {
            if (!nodeContext) {
                return;
            }
            var $nodeElement = $(nodeElement);
            var data = nodeContext.data || { name: "", selectable: false };
            var textNode = $("<a>" + $.htmlEncode(data.name) + "</a>").addClass(this.options.textCss);
            if (data.selectable) {
                textNode.prop("href", "javascript:void(0);").on("click", this._onNodeTextClickHandler);
            }
            else {
                textNode.addClass(this.options.unselectableCss);
            }
            $nodeElement
                .append($("<a href='javascript:;' aria-label=\"Node\"/>").addClass(this.options.iconCss).on("click", this._onNodeIconClickHandler))
                .append(textNode);
            this._initializeNodeIcon(nodeContext, $nodeElement);
            $nodeElement.children().eq(1).prop("title", data.name);
			logger.warn(`<<< Exiting _onCreateNodeElement(eventObject=${eventObject},nodeElement=${nodeElement},nodeContext=${nodeContext})`, { "event.action": "_onCreateNodeElement", "logrite.eventObject": eventObject, "logrite.nodeElement": nodeElement, "logrite.nodeContext": nodeContext });
        },

        _initializeNodeIcon: function (nodeContext, $nodeElement) {
			logger.error(`>>> Entering _initializeNodeIcon(nodeContext=${nodeContext},$nodeElement=${$nodeElement})`, { "event.action": "_initializeNodeIcon", "logrite.nodeContext": nodeContext, "logrite.$nodeElement": $nodeElement });
            var iconClass;
            var iconTooltip = "";
            if (nodeContext.hasChildren()) {
                iconClass = this.options.expandedCss;
                iconTooltip = this.options.folderCollapseTooltip;
				logger.debug(`_initializeNodeIcon(nodeContext=${nodeContext},$nodeElement=${$nodeElement}): iconClass → ${iconClass}`, { "event.action": "_initializeNodeIcon", "logrite.nodeContext": nodeContext, "logrite.$nodeElement": $nodeElement });
            }
            else if (nodeContext.data.hasChildren) {
                iconClass = this.options.collapsedCss;
                iconTooltip = this.options.folderExpandTooltip;
            }
            else {
                iconClass = this.options.emptyCss;
            }
            $nodeElement = $nodeElement || this._tree.getNodeElement(nodeContext);
            var $iconElement = $nodeElement.children(":first");
            $iconElement.removeClass(this.options.expandingCss).addClass(iconClass);
            $iconElement.prop("title", iconTooltip);
        },

        _onNodeTextClick: function (eventObject) {
			logger.error(`>>> Entering _onNodeTextClick(eventObject=${eventObject})`, { "event.action": "_onNodeTextClick", "logrite.eventObject": eventObject });
            eventObject.preventDefault();
            eventObject.stopPropagation();
            var clickedElement = eventObject.currentTarget;
            var nodeContext = this._tree.nodeContext(clickedElement);
			logger.error(`_onNodeTextClick(eventObject=${eventObject}): nodeContext → ${nodeContext}`, { "event.action": "_onNodeTextClick", "logrite.eventObject": eventObject });
            this._onChangeNode(nodeContext);
            $(this).trigger($.Event("onselectnode"), [nodeContext]);
			logger.warn(`<<< Exiting _onNodeTextClick(eventObject=${eventObject})`, { "event.action": "_onNodeTextClick", "logrite.eventObject": eventObject });
        },

        _onNodeIconClick: function (eventObject) {
			logger.error(`>>> Entering _onNodeIconClick(eventObject=${eventObject})`, { "event.action": "_onNodeIconClick", "logrite.eventObject": eventObject });
            eventObject.preventDefault();
            eventObject.stopPropagation();
            var clickedElement = eventObject.currentTarget;
            var $clickedElement = $(clickedElement);
            var $node = $clickedElement.parent();
            var nodeContext = this._tree.nodeContext(clickedElement);
			logger.error(`_onNodeIconClick(eventObject=${eventObject}): nodeContext → ${nodeContext}`, { "event.action": "_onNodeIconClick", "logrite.eventObject": eventObject });

            var tooltip;
            if ($clickedElement.hasClass(this.options.collapsedCss)) {
                // expand
                $clickedElement.removeClass(this.options.collapsedCss).addClass(this.options.expandingCss);
                this._onExpandNode(nodeContext);
                tooltip = this.options.folderLoadingTooltip;
            }
            else {
                // collapse: slide up and remove
                this._tree.removeChildren($node[0]);
                $clickedElement.removeClass(this.options.expandedCss).addClass(this.options.collapsedCss);
                this._onCollapseNode(nodeContext);
                tooltip = this.options.folderExpandTooltip;
            }
            $clickedElement.prop("title", tooltip);
			logger.debug(`_onNodeIconClick(eventObject=${eventObject}): tooltip → ${tooltip}`, { "event.action": "_onNodeIconClick", "logrite.eventObject": eventObject });
        },

        showChildren: function (nodeContext, children) {
			logger.error(`>>> Entering showChildren(nodeContext=${nodeContext},children=${children})`, { "event.action": "showChildren", "logrite.nodeContext": nodeContext, "logrite.children": children });
            nodeContext.data.hasChildren = children.length > 0;
            nodeContext.removeChildren().addChildNodes(children);
            this._tree.showChildren(nodeContext);
            this._initializeNodeIcon(nodeContext, this._tree.getNodeElement(nodeContext));
            this._ensureSelectedNode();
            this._numberOfNodes += children.length;
			logger.warn(`<<< Exiting showChildren(nodeContext=${nodeContext},children=${children})`, { "event.action": "showChildren", "logrite.nodeContext": nodeContext, "logrite.children": children });
        },

        _onCollapseNode: function (nodeContext) {
			logger.error(`>>> Entering _onCollapseNode(nodeContext=${nodeContext})`, { "event.action": "_onCollapseNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
            this._numberOfNodes -= nodeContext.getNumberOfChildren(true);
            if (this._selectedNode !== nodeContext && nodeContext.hasNode(this._selectedNode)) {
                this._selectedNode = null;
            }
            nodeContext.removeChildren();
            $(this).trigger($.Event("oncollapsenode"), [nodeContext]);
			logger.warn(`<<< Exiting _onCollapseNode(nodeContext=${nodeContext})`, { "event.action": "_onCollapseNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
        },

        _onExpandNode: function (nodeContext) {
			logger.error(`>>> Entering _onExpandNode(nodeContext=${nodeContext})`, { "event.action": "_onExpandNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
            $(this).trigger($.Event("onexpandnode"), [nodeContext]);
			logger.warn(`<<< Exiting _onExpandNode(nodeContext=${nodeContext})`, { "event.action": "_onExpandNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
        },

        count: function () {
			logger.error('>>> Entering count()', { "event.action": "count" });
			logger.warn('<<< Exiting count()', { "event.action": "count" });
            return this._numberOfNodes;
        },

        _selectNodeElement: function (nodeContext, selected) {
			logger.error(`>>> Entering _selectNodeElement(nodeContext=${nodeContext},selected=${selected})`, { "event.action": "_selectNodeElement", "logrite.nodeContext": nodeContext, "logrite.selected": selected });
            if (this._tree) {
                this._tree.addNodeClass(nodeContext, selected, this.options.selectedCss);
				logger.warn(`<<< Exiting _selectNodeElement(nodeContext=${nodeContext},selected=${selected})`, { "event.action": "_selectNodeElement", "logrite.nodeContext": nodeContext, "logrite.selected": selected });
            }
        },

        _onRedrawTree: function () {
			logger.error('>>> Entering _onRedrawTree()', { "event.action": "_onRedrawTree" });
            this.updateLayout();
			logger.warn('<<< Exiting _onRedrawTree()', { "event.action": "_onRedrawTree" });
            $(this).trigger($.Event("onredrawtree"));
        },
        
        _onShowChildren: function () {
			logger.error('>>> Entering _onShowChildren()', { "event.action": "_onShowChildren", "logrite.nodeContext": nodeContext });
            $(this).trigger($.Event("onshowchildren"));
			logger.warn('<<< Exiting _onShowChildren()', { "event.action": "_onShowChildren", "logrite.nodeContext": nodeContext });
        },

        updateLayout: function () {
			logger.error('>>> Entering updateLayout()', { "event.action": "updateLayout" });
            if (this.options.scroll) {
                this.$element.jScrollbar("update");
				logger.warn('<<< Exiting updateLayout()', { "event.action": "updateLayout" });
            }
        },

        _ensureSelectedNode: function () {
			logger.error('>>> Entering _ensureSelectedNode()', { "event.action": "_ensureSelectedNode" });
            this.selectedId(this._selectedNodeId);
			logger.warn('<<< Exiting _ensureSelectedNode()', { "event.action": "_ensureSelectedNode" });
        },

        _getNodeById: function (id) {
			logger.error(`>>> Entering _getNodeById(id=${id})`, { "event.action": "_getNodeById", "logrite.id": id });
            var predicate = function(data) { return data && data.id === id; };
			logger.error(`>>> Entering predicate(data=${data})`, { "event.action": "predicate", "logrite.data": data });
            var path = dnn.NTree.getPath(this._rootNode, predicate);
			logger.warn(`<<< Exiting predicate(data=${data})`, { "event.action": "predicate", "logrite.data": data });
			logger.error(`_getNodeById(id=${id}): path → ${path}`, { "event.action": "_getNodeById", "logrite.id": id });
            return path.length > 0 ? path.pop() : null;
        },

        selectedId: function (id) {
            if (typeof id === "undefined") {
                return this._selectedNodeId;
            }
            
            if (id !== this._selectedNodeId || !this.selectedNode()) {
                var node = this._getNodeById(id);
                this.selectedNode(node);
            }
            return this._selectedNodeId = id;
        },
        
        scrollToSelectedNode: function() {               
			logger.error('>>> Entering scrollToSelectedNode()', { "event.action": "scrollToSelectedNode" });
            //if node selected, we need scoll tree to show the selected node.
            var node = this._getNodeById(this.selectedId());
			logger.error(`scrollToSelectedNode(): node → ${node}`, { "event.action": "scrollToSelectedNode" });
            if (!node) {
                //if node is not exist, we should pop a node expand request to make sure its load by expand parents nodes.
                $(this).trigger($.Event("onrequestexpand"));
                return;
            }
            if (this.options.scroll && node) {
                var $nodeElement = this._tree.getNodeElement(node);
                var offset = $nodeElement.position().top - $nodeElement.parentsUntil('div', 'ul[class*="' + this._tree.options.nodeListCss + '"][class*="' + this._tree.options.rootCss + '"]').position().top;
				logger.error(`scrollToSelectedNode(): offset → ${offset}`, { "event.action": "scrollToSelectedNode" });
                this.$element.scrollTop(offset);
                this.updateLayout();
				logger.warn('<<< Exiting scrollToSelectedNode()', { "event.action": "scrollToSelectedNode" });
            }
        },

        selectedPath: function () {
			logger.error('>>> Entering selectedPath()', { "event.action": "selectedPath" });
            var dataPath = [];
            var id = this.selectedId();
			logger.error(`selectedPath(): dataPath → ${dataPath}`, { "event.action": "selectedPath" });
            var predicate = function(data) { return data && data.id === id; };
            var path = dnn.NTree.getPath(this._rootNode, predicate);
			logger.error(`selectedPath(): id → ${id}`, { "event.action": "selectedPath" });
			logger.error(`selectedPath(): path → ${path}`, { "event.action": "selectedPath" });
            if (path.length > 1) {
                path.shift(); // remove the first root node;
                for (var i = 0, size = path.length; i < size; i++) {
                    dataPath.push(path[i].data);
					logger.warn('<<< Exiting selectedPath()', { "event.action": "selectedPath" });
                }
            }
            return dataPath;
        },

        selectedNode: function (node) {
			logger.error(`>>> Entering selectedNode(node=${node})`, { "event.action": "selectedNode", "logrite.node": node });
            if (typeof node === "undefined") {
                return this._selectedNode;
            }
            this._selectNodeElement(this._selectedNode, false);
            this._selectNodeElement(node, true);
            this._selectedNodeId = node && node.data ? node.data.id : null;
			logger.warn(`<<< Exiting selectedNode(node=${node})`, { "event.action": "selectedNode", "logrite.node": node });
			logger.debug(`selectedNode(node=${node}): this._selectedNodeId → ${this._selectedNodeId}`, { "event.action": "selectedNode", "logrite.node": node });
            return this._selectedNode = node;
        },

        rootNode: function (root) {
			logger.error(`>>> Entering rootNode(root=${root})`, { "event.action": "rootNode", "logrite.root": root });
            if (typeof root !== "undefined") {
                this._tree.collapseTree($.proxy(this._showTree, this, root));
				logger.warn(`<<< Exiting rootNode(root=${root})`, { "event.action": "rootNode", "logrite.root": root });
            }
            return this._rootNode;
        },

        _showTree: function (rootNode) {
            this._rootNode = rootNode;
            if (this._firstNode) {
                this._rootNode.children.insertAt(0, this._firstNode);
            }
            this._tree.showTree(this._rootNode.children);
            this._selectedNode = null;
            this._ensureSelectedNode();
            this._tree.addNodeClass(this._firstNode, true, this.options.firstItemCss);

            this._numberOfNodes = this._rootNode.getNumberOfChildren(true) - (this._firstNode ? 1 : 0);
        },

        _onChangeNode: function (node) {
			logger.error(`>>> Entering _onChangeNode(node=${node})`, { "event.action": "_onChangeNode", "logrite.eventObject": eventObject, "logrite.node": node });
            var nodeComparer = function(one, another) { return one && another && one.id === another.id; };
			logger.error(`>>> Entering nodeComparer(one=${one},another=${another})`, { "event.action": "nodeComparer", "logrite.one": one, "logrite.another": another });
            if (!dnn.NTree.equals(node, this._selectedNode, nodeComparer)) {
				logger.warn(`<<< Exiting nodeComparer(one=${one},another=${another})`, { "event.action": "nodeComparer", "logrite.one": one, "logrite.another": another });
                this.selectedNode(node);
                $(this).trigger($.Event("onchangenode"), [node]);
				logger.warn(`<<< Exiting _onChangeNode(node=${node})`, { "event.action": "_onChangeNode", "logrite.eventObject": eventObject, "logrite.node": node });
            }
        }

    };

    DynamicTreeView._defaults = {
        selectedCss: "selected",
        unselectableCss: "unselectable",
        expandedCss: "expanded",
        expandingCss: "expanding",
        collapsedCss: "collapsed",
        emptyCss: "empty",
        iconCss: "icon",
        textCss: "text",
        searchOnCss: "searchOn",
        firstItemCss: "first-item",
        treeContainerCss: "dt-tree",
        scroll: true
    };

    DynamicTreeView.defaults = function (settings) {
		logger.error(`>>> Entering defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        if (typeof settings !== "undefined") {
            $.extend(DynamicTreeView._defaults, settings);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        return DynamicTreeView._defaults;
    };


    //
    // SortableTreeView
    //
    var SortableTreeView = this.SortableTreeView = function (element, options, controller) {
		logger.error(`>>> Entering SortableTreeView(element=${element},options=${options},controller=${controller})`, { "event.action": "SortableTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
        this.element = element;
        this.options = options;
		logger.debug(`SortableTreeView(element=${element},options=${options},controller=${controller}): element → ${element}`, { "event.action": "SortableTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`SortableTreeView(element=${element},options=${options},controller=${controller}): this.element → ${this.element}`, { "event.action": "SortableTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`SortableTreeView(element=${element},options=${options},controller=${controller}): options → ${options}`, { "event.action": "SortableTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
		logger.debug(`SortableTreeView(element=${element},options=${options},controller=${controller}): controller → ${controller}`, { "event.action": "SortableTreeView", "logrite.element": element, "logrite.options": options, "logrite.controller": controller });
        this.controller = controller;

        this.init();
    };

    SortableTreeView.prototype = {

        constructor: SortableTreeView,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, SortableTreeView.defaults(), this.options);

            this.$this = $(this);

            this.$element = this.element ? $(this.element) : this._createLayout();

            this._$itemListHeaderElement = this.$element.find("." + this.options.headerCss);
            this._$clearButton = this._$itemListHeaderElement.find("." + this.options.clearButtonCss);
            this._$clearButton.on("click.drop-down-list", $.proxy(this._onClearClick, this));
            this._$searchButton = this._$itemListHeaderElement.find("." + this.options.searchButtonCss);
            this._$searchButton.on("click.drop-down-list", $.proxy(this._onSearchClick, this));
            var onSortHandler = $.proxy(this._onSortClick, this);
			logger.error(`init(): onSortHandler → ${onSortHandler}`, { "event.action": "init" });
            var onSearchHandler = $.proxy(this._onSearchClick, this);
			logger.error(`init(): onSearchHandler → ${onSearchHandler}`, { "event.action": "init" });
            this._$searchContainer = this._$itemListHeaderElement.find("." + this.options.searchContainerCss);
            this._$searchInput = this._$itemListHeaderElement.find("." + this.options.searchInputCss).onEnter(onSearchHandler).on("keyup.drop-down-list", $.proxy(this._displayClearButton, this));
            this._displayClearButton();
            this._$sortButton = this._$itemListHeaderElement.find("." + this.options.sortButtonCss);
            this._$sortButton.on("click.drop-down-list", onSortHandler);

            this._$itemListFooterElement = this.$element.find("." + this.options.footerCss);
            this._$resultValue = this._$itemListFooterElement.find("." + this.options.resultElementCss).children(":first");

            this._$itemListContentElement = this.$element.find("." + this.options.contentCss);

            this._onLoadTreeHandler = $.proxy(this._onLoadTree, this);

            this._firstNode = this.options.firstItem ? TreeNodeConverter.createNodeFromModel(this.options.firstItem) : null;
            this.options.firstNode = this._firstNode;

            this._dynamicTree = new DynamicTreeView(this._$itemListContentElement[0], this.options);
            var $dynamicTree = $(this._dynamicTree);
            $dynamicTree.on("onredrawtree", $.proxy(this._onRedrawTree, this));
            $dynamicTree.on("onshowchildren", $.proxy(this._onShowChildren, this));
            $dynamicTree.on("onexpandnode", $.proxy(this._onExpandNode, this));
            $dynamicTree.on("oncollapsenode", $.proxy(this._onCollapseNode, this));
            $dynamicTree.on("onselectnode", $.proxy(this._onSelectNode, this));
            $dynamicTree.on("onchangenode", $.proxy(this._onChangeNode, this));
            $dynamicTree.on("onrequestexpand", $.proxy(this._onRequestExpand, this));

            this._sortOrder(dnn.SortOrder.unspecified);

		logger.debug(`init(): this.parameters → ${this.parameters}`, { "event.action": "init" });
        },

        _createLayout: function () {
			logger.error('>>> Entering _createLayout()', { "event.action": "_createLayout" });
            var layout = $("<div class='" + this.options.containerCss + "'/>")
                    .append($("<div class='" + this.options.headerCss + "'/>")
                        .append($("<a href='javascript:void(0);' class='" + this.options.sortButtonCss + "'><span>" + this.options.sortAscendingButtonTitle + "</span></a>"))
                        .append($("<div class='" + this.options.searchContainerCss + "'/>")
                            .append($("<div class='" + this.options.searchInputContainerCss + "'/>")
                                .append($("<input type='text' placeholder='" + this.options.searchInputPlaceHolder + "' maxlength='200' autocomplete='off' class='" + this.options.searchInputCss + "' aria-label=\"Search\">")))
                            .append($("<a href='javascript:void(0);' title='" + this.options.clearButtonTooltip + "' style='display:none;' class='" + this.options.clearButtonCss + "' aria-label=\"Clear\"/>"))
                            .append($("<a href='javascript:void(0);' title='" + this.options.searchButtonTooltip + "' class='" + this.options.searchButtonCss + "' aria-label=\"Search\"/>"))))
                    .append($("<div class='" + this.options.contentCss + "'/>")
                        .append($("<div class='" + this.options.treeContainerCss + "'/>")))
                    .append($("<div class='" + this.options.footerCss + "'/>")
                        .append($("<span class='" + this.options.resultElementCss + "'><b></b> " + this.options.resultsText + "</span>"))
                        .append($("<div class='" + this.options.resizerElementCss + "'/>")));
			logger.error(`_createLayout(): layout → ${layout}`, { "event.action": "_createLayout" });
            return layout;
        },

        _onExpandNode: function(eventObject, nodeContext) {
			logger.error(`>>> Entering _onExpandNode(eventObject=${eventObject},nodeContext=${nodeContext})`, { "event.action": "_onExpandNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
            var onLoadChildNodesHandler = $.proxy(this._onLoadChildren, this, nodeContext);
            var searchText = this._searchInput();
			logger.error(`_onExpandNode(eventObject=${eventObject},nodeContext=${nodeContext}): onLoadChildNodesHandler → ${onLoadChildNodesHandler}`, { "event.action": "_onExpandNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
			logger.error(`_onExpandNode(eventObject=${eventObject},nodeContext=${nodeContext}): searchText → ${searchText}`, { "event.action": "_onExpandNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
            this.controller.getChildren(nodeContext.data.id, this._sortOrder(), searchText, onLoadChildNodesHandler);
        },

        _onCollapseNode: function (eventObject, nodeContext) {
			logger.error(`>>> Entering _onCollapseNode(eventObject=${eventObject},nodeContext=${nodeContext})`, { "event.action": "_onCollapseNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
            this._updateResult(this._dynamicTree.count());
			logger.warn(`<<< Exiting _onCollapseNode(eventObject=${eventObject},nodeContext=${nodeContext})`, { "event.action": "_onCollapseNode", "logrite.eventObject": eventObject, "logrite.nodeContext": nodeContext });
        },

        _onLoadChildren: function (nodeContext, children) {
			logger.error(`>>> Entering _onLoadChildren(nodeContext=${nodeContext},children=${children})`, { "event.action": "_onLoadChildren", "logrite.nodeContext": nodeContext, "logrite.children": children });
            this._dynamicTree.showChildren(nodeContext, children);
            this._updateResult(this._dynamicTree.count());
            
            $(this).trigger($.Event('onloadchildren'), [nodeContext]);
			logger.warn(`<<< Exiting _onLoadChildren(nodeContext=${nodeContext},children=${children})`, { "event.action": "_onLoadChildren", "logrite.nodeContext": nodeContext, "logrite.children": children });
        },
        _onShowChildren: function(nodeContext) {
            $(this).trigger($.Event('onshowchildren'), [nodeContext]);
        },
        _onRequestExpand: function(nodeContext) {
			logger.error(`>>> Entering _onRequestExpand(nodeContext=${nodeContext})`, { "event.action": "_onRequestExpand", "logrite.nodeContext": nodeContext });
            $(this).trigger($.Event('onrequestexpand'), [nodeContext]);
			logger.warn(`<<< Exiting _onRequestExpand(nodeContext=${nodeContext})`, { "event.action": "_onRequestExpand", "logrite.nodeContext": nodeContext });
        },

        _updateResult: function (resultText) {
			logger.error(`>>> Entering _updateResult(resultText=${resultText})`, { "event.action": "_updateResult", "logrite.resultText": resultText });
            this._$resultValue.text(resultText);
			logger.warn(`<<< Exiting _updateResult(resultText=${resultText})`, { "event.action": "_updateResult", "logrite.resultText": resultText });
        },

        _searchInput: function(val) {
			logger.error(`>>> Entering _searchInput(val=${val})`, { "event.action": "_searchInput", "logrite.val": val });
            if (typeof val === "undefined") {
                return this._$searchInput.val().trim();
            }
            this._$searchInput.val(val);
			logger.warn(`<<< Exiting _searchInput(val=${val})`, { "event.action": "_searchInput", "logrite.val": val });
            return val;
        },

        _displayClearButton: function () {
			logger.error('>>> Entering _displayClearButton()', { "event.action": "_displayClearButton" });
            var input = this._$searchInput.val();
            if (input && input.length > 0) {
				logger.error(`_displayClearButton(): input → ${input}`, { "event.action": "_displayClearButton" });
                this._$clearButton.show();
            }
            else {
                this._$clearButton.hide();
				logger.warn('<<< Exiting _displayClearButton()', { "event.action": "_displayClearButton" });
            }
        },

        _onRedrawTree: function () {
        },

        selectedId: function (id) {
            return this._dynamicTree.selectedId(id);
        },

        selectedPath: function () {
            return this._dynamicTree.selectedPath();
        },

        selectedNode: function (node) {
            return this._dynamicTree.selectedNode(node);
        },

        _onChangeNode: function (eventObject, node) {
            $(this).trigger($.Event('onchangenode'), [node]);
        },

        _onSelectNode: function (eventObject, node) {
			logger.error(`>>> Entering _onSelectNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onSelectNode", "logrite.eventObject": eventObject, "logrite.node": node });
            $(this).trigger($.Event('onselectnode'), [node]);
			logger.warn(`<<< Exiting _onSelectNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onSelectNode", "logrite.eventObject": eventObject, "logrite.node": node });
        },

        _onResize: function (eventObject, containerSize) {
			logger.error(`>>> Entering _onResize(eventObject=${eventObject},containerSize=${containerSize})`, { "event.action": "_onResize", "logrite.eventObject": eventObject, "logrite.containerSize": containerSize });
            this._$itemListContentElement.height(containerSize.height - this._itemListHeaderHeight - this._itemListFooterHeight);
            this._dynamicTree.updateLayout();
			logger.warn(`<<< Exiting _onResize(eventObject=${eventObject},containerSize=${containerSize})`, { "event.action": "_onResize", "logrite.eventObject": eventObject, "logrite.containerSize": containerSize });
        },

        _onLoadTree: function (rootNode) {
            this._dynamicTree.rootNode(rootNode);
            this._updateResult(this._dynamicTree.count());
            this._loading(false);
            
            $(this).trigger($.Event('ontreeloaded'));
			logger.warn(`<<< Exiting _onLoadTree(rootNode=${rootNode})`, { "event.action": "_onLoadTree", "logrite.rootNode": rootNode });
        },

        show: function () {
			logger.error('>>> Entering show()', { "event.action": "show" });
            /* the component is a part of DOM, the dimension is known at this point */
            this._itemListHeaderHeight = this._$itemListHeaderElement.outerHeight(true);
            this._itemListFooterHeight = this._$itemListFooterElement.outerHeight(true);
            this._$itemListContentElement.height(this.$element.height() - this._itemListHeaderHeight - this._itemListFooterHeight);

            if (!this._resizer) {
                var $resizerElement = this._$itemListFooterElement.find("." + this.options.resizerElementCss);
                this._resizer = new dnn.Resizer($resizerElement[0], { container: this.$element });
                $(this._resizer).on("resized", $.proxy(this._onResize, this));
            }

            this._showTree(this._selectedNodeId);
			logger.warn('<<< Exiting show()', { "event.action": "show" });
        },

        _onClearClick: function () {
			logger.error('>>> Entering _onClearClick()', { "event.action": "_onClearClick" });
            this._$searchInput.val("");
            this._$clearButton.hide();
            if (this._searchHasBeenApplied) {
                this._search();
				logger.warn('<<< Exiting _onClearClick()', { "event.action": "_onClearClick" });
            }
        },

        _loading: function (loading) {
			logger.error(`>>> Entering _loading(loading=${loading})`, { "event.action": "_loading", "logrite.loading": loading });
            if (loading) {
                this._$itemListContentElement.addClass(this.options.loadingCss);
            }
            else {
                this._$itemListContentElement.removeClass(this.options.loadingCss);
				logger.warn(`<<< Exiting _loading(loading=${loading})`, { "event.action": "_loading", "logrite.loading": loading });
            }
        },

        _showTree: function (selectedItemId) {
			logger.error(`>>> Entering _showTree(selectedItemId=${selectedItemId})`, { "event.action": "_showTree", "logrite.selectedItemId": selectedItemId });
            this._loading(true);
            if (selectedItemId && (!this._firstNode || (this._firstNode && this._firstNode.data.id !== selectedItemId))) {
                this._searchInput("");
                this.controller.getTreeWithItem(selectedItemId, this._sortOrder(), this._onLoadTreeHandler);
            }
            else {
                this.controller.getTree(this._sortOrder(), this._onLoadTreeHandler);
            }
            this._$searchContainer.removeClass(this.options.searchOnCss);
            this._searchHasBeenApplied = false;
			logger.warn(`<<< Exiting _showTree(rootNode=${rootNode})`, { "event.action": "_showTree", "logrite.selectedItemId": selectedItemId });
			logger.debug(`_showTree(selectedItemId=${selectedItemId}): this._searchHasBeenApplied → ${this._searchHasBeenApplied}`, { "event.action": "_showTree", "logrite.selectedItemId": selectedItemId });
        },

        _onSearchClick: function () {
			logger.error('>>> Entering _onSearchClick()', { "event.action": "_onSearchClick" });
            this._search();
			logger.warn('<<< Exiting _onSearchClick()', { "event.action": "_onSearchClick" });
        },

        _onSortClick: function () {
			logger.error('>>> Entering _onSortClick()', { "event.action": "_onSortClick" });
            this._sort();
			logger.warn('<<< Exiting _onSortClick()', { "event.action": "_onSortClick" });
        },

        _search: function() {
			logger.error('>>> Entering _search()', { "event.action": "_search" });
            this._loading(true);
            var searchText = this._searchInput();
			logger.error(`_search(): searchText → ${searchText}`, { "event.action": "_search" });
            if (searchText) {
                this._$searchContainer.addClass(this.options.searchOnCss);
                this.controller.search(searchText, this._sortOrder(), this._onLoadTreeHandler);
                this._searchHasBeenApplied = true;
            }
            else {
				logger.debug(`_search(): this._searchHasBeenApplied → ${this._searchHasBeenApplied}`, { "event.action": "_search" });
                this._$searchContainer.removeClass(this.options.searchOnCss);
                this.controller.getTree(this._sortOrder(), this._onLoadTreeHandler);
                this._searchHasBeenApplied = false;
				logger.warn('<<< Exiting _search()', { "event.action": "_search" });
            }
        },

        _sortOrder: function (order) {
            if (typeof order === "undefined") {
                return this._sortOrderValue;
            }
            if (order === dnn.SortOrder.ascending) {
                this._$sortButton.removeClass("desc").addClass("asc").prop("title", this.options.sortDescendingButtonTooltip);
            }
            else if (order === dnn.SortOrder.descending) {
                this._$sortButton.removeClass("asc").addClass("desc").prop("title", this.options.unsortedOrderButtonTooltip);
            }
            else { // order === dnn.SortOrder.unspecified
                this._$sortButton.removeClass("desc").removeClass("asc").prop("title", this.options.sortAscendingButtonTooltip);
            }
			logger.warn(`<<< Exiting _sortOrder(order=${order})`, { "event.action": "_sortOrder", "logrite.order": order });
            return this._sortOrderValue = order;
        },

        _getNextSortOrder: function() {
			logger.error('>>> Entering _getNextSortOrder()', { "event.action": "_getNextSortOrder" });
            var order = this._sortOrder();
            if (this.options.disableUnspecifiedOrder) {
				logger.error(`_getNextSortOrder(): order → ${order}`, { "event.action": "_getNextSortOrder" });
                if (order === dnn.SortOrder.ascending) {
                    return dnn.SortOrder.descending;
                }
                
                return dnn.SortOrder.ascending;
            }
            if (order === dnn.SortOrder.unspecified) {
                return dnn.SortOrder.ascending;
            }
            if (order === dnn.SortOrder.ascending) {
				logger.warn('<<< Exiting _getNextSortOrder()', { "event.action": "_getNextSortOrder" });
                return dnn.SortOrder.descending;
            }
            return dnn.SortOrder.unspecified;
        },

        _sort: function () {
			logger.error('>>> Entering _sort()', { "event.action": "_sort" });
            this._loading(true);
            this._sortOrder(this._getNextSortOrder());
            var searchText = "";
            if (this._searchHasBeenApplied) {
                searchText = this._searchInput();
                if (searchText === "") {
                    this._$searchContainer.removeClass(this.options.searchOnCss);
                    this._searchHasBeenApplied = false;
                }
            }
            this.controller.sortTree(this._sortOrder(), this._dynamicTree.rootNode(), searchText, this._onLoadTreeHandler);
			logger.error(`_sort(): searchText → ${searchText}`, { "event.action": "_sort" });
        }

    };

    SortableTreeView._defaults = {
        searchOnCss: "searchOn",
        containerCss: "dt-container",
        headerCss: "dt-header",
        footerCss: "dt-footer",
        contentCss: "dt-content",
        treeContainerCss: "dt-tree",
        clearButtonCss: "clear-button",
        searchButtonCss: "search-button",
        searchContainerCss: "search-container",
        searchInputContainerCss: "search-input-container",
        searchInputCss: "search-input",
        sortButtonCss: "sort-button",
        resultElementCss: "result",
        resizerElementCss: "resizer",
        loadingCss: "loading-items"
    };

    SortableTreeView.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(SortableTreeView._defaults, settings);
        }
        return SortableTreeView._defaults;
    };


    var TreeNodeConverter = this.TreeNodeConverter = function () {
	logger.error('>>> Entering TreeNodeConverter()', { "event.action": "TreeNodeConverter" });
    };

    TreeNodeConverter.castNodes = function (nodes) {
        var castedNodes = [];
        for (var i = 0, count = nodes.length; i < count; i++) {
            castedNodes.push(TreeNodeConverter.castNode(nodes[i]));
        }
        return castedNodes;
    };

    TreeNodeConverter.castNode = function(model) {
		logger.error(`>>> Entering castNode(model=${model})`, { "event.action": "castNode", "logrite.model": model });
        if (!model) {
            return null;
        }
        var castedNode = TreeNodeConverter.createNodeFromModel(model.data);
        for (var i = 0, count = model.children.length; i < count; i++) {
            castedNode.children.push(TreeNodeConverter.castNode(model.children[i]));
        }
        return castedNode;
    };

    TreeNodeConverter.createNodesFromData = function (model) {
		logger.error(`>>> Entering createNodesFromData(model=${model})`, { "event.action": "createNodesFromData", "logrite.model": model });
        var castedNodes = [];
        if (model) {
            for (var i = 0, count = model.length; i < count; i++) {
                castedNodes.push(TreeNodeConverter.createNodeFromModel(model[i]));
            }
        }
		logger.warn(`<<< Exiting createNodesFromData(model=${model})`, { "event.action": "createNodesFromData", "logrite.model": model });
        return castedNodes;
    };

    TreeNodeConverter.createNodeFromModel = function(data) {
		logger.error(`>>> Entering createNodeFromModel(data=${data})`, { "event.action": "createNodeFromModel", "logrite.data": data });
        var node = new dnn.NTree();
        node.data = TreeNodeConverter.castModelData(data);
		logger.error(`createNodeFromModel(data=${data}): node → ${node}`, { "event.action": "createNodeFromModel", "logrite.data": data });
		logger.warn(`<<< Exiting createNodeFromModel(data=${data})`, { "event.action": "createNodeFromModel", "logrite.data": data });
        return node;
    };

    TreeNodeConverter.castModelData = function(model) {
		logger.error(`>>> Entering castModelData(model=${model})`, { "event.action": "castModelData", "logrite.model": model });
		logger.warn(`<<< Exiting castModelData(model=${model})`, { "event.action": "castModelData", "logrite.model": model });
        return model ? { hasChildren: model.hasChildren, name: model.value, id: model.key, selectable: typeof (model.selectable) === "undefined" || model.selectable } : null;
    };

    TreeNodeConverter.toKeyValue = function (data) {
		logger.error(`>>> Entering toKeyValue(data=${data})`, { "event.action": "toKeyValue", "logrite.data": data });
		logger.warn(`<<< Exiting toKeyValue(data=${data})`, { "event.action": "toKeyValue", "logrite.data": data });
        return data ? { key: data.id, value: data.name } : null;
    };

    TreeNodeConverter.toTreeOfIds = function (tree) {
		logger.error(`>>> Entering toTreeOfIds(tree=${tree})`, { "event.action": "toTreeOfIds", "logrite.tree": tree });
        if (!tree) {
            return null;
        }
        var node = new dnn.NTree({ id: tree.data.id });
        for (var i = 0, count = tree.children.length; i < count; i++) {
            if (tree.children[i].hasChildren()) {
                node.children.push(TreeNodeConverter.toTreeOfIds(tree.children[i]));
            }
        }
        return node;
    };

    var DynamicTreeViewController = this.DynamicTreeViewController = function (options) {
		logger.error(`>>> Entering DynamicTreeViewController(options=${options})`, { "event.action": "DynamicTreeViewController", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`DynamicTreeViewController(options=${options}): this.options → ${this.options}`, { "event.action": "DynamicTreeViewController", "logrite.options": options });
    };

    DynamicTreeViewController.prototype = {
        constructor: DynamicTreeViewController,

        init: function() {
            this.options = $.extend({}, DynamicTreeViewController.defaults(), this.options);
            this._serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
            this.parameters = this.options.parameters;
        },

        _callGet: function (data, onLoadHandler, method) {
			logger.error(`>>> Entering _callGet(data=${data},onLoadHandler=${onLoadHandler},method=${method})`, { "event.action": "_callGet", "logrite.data": data, "logrite.onLoadHandler": onLoadHandler, "logrite.method": method });
            $.extend(data, this.parameters);
            var serviceSettings = {
                url: this._serviceUrl + method,
                beforeSend: $.dnnSF(this.options.moduleId).setModuleHeaders,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: data,
                type: "GET",
                async: true,
                success: onLoadHandler,
                error: $.onAjaxError
            };
            $.ajax(serviceSettings);
			logger.warn(`<<< Exiting _callGet(data=${data},onLoadHandler=${onLoadHandler},method=${method})`, { "event.action": "_callGet", "logrite.data": data, "logrite.onLoadHandler": onLoadHandler, "logrite.method": method });
        },

        getTreeWithItem: function (itemId, sortOrder, onGetTreeCallback) {
			logger.error(`>>> Entering getTreeWithItem(itemId=${itemId},sortOrder=${sortOrder},onGetTreeCallback=${onGetTreeCallback})`, { "event.action": "getTreeWithItem", "logrite.itemId": itemId, "logrite.sortOrder": sortOrder, "logrite.onGetTreeCallback": onGetTreeCallback });
            var onGetTreeHandler = $.proxy(this._onGetTree, this, onGetTreeCallback);
            this._callGet({ itemId: itemId, sortOrder: sortOrder }, onGetTreeHandler, this.options.getTreeWithNodeMethod);
			logger.error(`getTreeWithItem(itemId=${itemId},sortOrder=${sortOrder},onGetTreeCallback=${onGetTreeCallback}): onGetTreeHandler → ${onGetTreeHandler}`, { "event.action": "getTreeWithItem", "logrite.itemId": itemId, "logrite.sortOrder": sortOrder, "logrite.onGetTreeCallback": onGetTreeCallback });
        },

        _onGetTree: function (onGetTreeCallback, data, textStatus, jqXhr) {
			logger.error(`>>> Entering _onGetTree(onGetTreeCallback=${onGetTreeCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr})`, { "event.action": "_onGetTree", "logrite.onGetTreeCallback": onGetTreeCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
            var rootNode = null;
            var castedRootNode = TreeNodeConverter.castNode(data.Tree);
			logger.error(`_onGetTree(onGetTreeCallback=${onGetTreeCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr}): castedRootNode → ${castedRootNode}`, { "event.action": "_onGetTree", "logrite.onGetTreeCallback": onGetTreeCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
			logger.debug(`_onGetTree(onGetTreeCallback=${onGetTreeCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr}): castedRootNode.data → ${castedRootNode.data}`, { "event.action": "_onGetTree", "logrite.onGetTreeCallback": onGetTreeCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
            if (data.IgnoreRoot) {
                if (dnn.NTree.isEmpty(castedRootNode)) {
                    castedRootNode.data = { hasChildren: false, name: "", id: this.options.rootId };
                }
                rootNode = castedRootNode;
            }
            else {
                rootNode = new dnn.NTree({ hasChildren: true, name: "", id: this.options.rootId });
                if (!dnn.NTree.isEmpty(castedRootNode)) {
                    rootNode.children.push(castedRootNode);
                }
            }

            if (rootNode.children && rootNode.children.length > 0 && this.options.rootNodeName) {
                if (rootNode.children[0].data.id === this.options.rootNodeId) { // Check if first node is the configured root node. Search or Sort could change that
                    rootNode.children[0].data.name = this.options.rootNodeName;
                }
            }


            onGetTreeCallback.apply(this, [rootNode]);
			logger.warn(`<<< Exiting _onGetTree(onGetTreeCallback=${onGetTreeCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr})`, { "event.action": "_onGetTree", "logrite.onGetTreeCallback": onGetTreeCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
        },

        getChildren: function (parentId, sortOrder, searchText, onGetChildrenCallback) {
			logger.error(`>>> Entering getChildren(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},onGetChildrenCallback=${onGetChildrenCallback})`, { "event.action": "getChildren", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.onGetChildrenCallback": onGetChildrenCallback });
            var onGetChildrenHandler = $.proxy(this._onGetChildren, this, onGetChildrenCallback);
            var callGetParams = { parentId: parentId, sortOrder: sortOrder, searchText: searchText, includeAllTypes: true };
			logger.error(`getChildren(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},onGetChildrenCallback=${onGetChildrenCallback}): onGetChildrenHandler → ${onGetChildrenHandler}`, { "event.action": "getChildren", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.onGetChildrenCallback": onGetChildrenCallback });
			logger.debug(`getChildren(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},onGetChildrenCallback=${onGetChildrenCallback}): callGetParams.includeDisabled → ${callGetParams.includeDisabled}`, { "event.action": "getChildren", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.onGetChildrenCallback": onGetChildrenCallback });
            if (this.options.includeDisabled) {
                callGetParams.includeDisabled = this.options.includeDisabled;
            }
            this._callGet(callGetParams, onGetChildrenHandler, this.options.getNodeDescendantsMethod);
			logger.warn(`<<< Exiting getChildren(parentId=${parentId},sortOrder=${sortOrder},searchText=${searchText},onGetChildrenCallback=${onGetChildrenCallback})`, { "event.action": "getChildren", "logrite.parentId": parentId, "logrite.sortOrder": sortOrder, "logrite.searchText": searchText, "logrite.onGetChildrenCallback": onGetChildrenCallback });
        },

        _onGetChildren: function(onGetChildrenCallback, data, textStatus, jqXhr) {
			logger.error(`>>> Entering _onGetChildren(onGetChildrenCallback=${onGetChildrenCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr})`, { "event.action": "_onGetChildren", "logrite.onGetChildrenCallback": onGetChildrenCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
            var nodes = TreeNodeConverter.createNodesFromData(data.Items);
            onGetChildrenCallback.apply(this, [nodes]);
			logger.error(`_onGetChildren(onGetChildrenCallback=${onGetChildrenCallback},data=${data},textStatus=${textStatus},jqXhr=${jqXhr}): nodes → ${nodes}`, { "event.action": "_onGetChildren", "logrite.onGetChildrenCallback": onGetChildrenCallback, "logrite.data": data, "logrite.textStatus": textStatus, "logrite.jqXhr": jqXhr });
        },

        search: function(searchText, sortOrder, onSearchCallback) {
			logger.error(`>>> Entering search(searchText=${searchText},sortOrder=${sortOrder},onSearchCallback=${onSearchCallback})`, { "event.action": "search", "logrite.searchText": searchText, "logrite.sortOrder": sortOrder, "logrite.onSearchCallback": onSearchCallback });
            var onSearchHandler = $.proxy(this._onGetTree, this, onSearchCallback);
            var callGetParams = { searchText: searchText, sortOrder: sortOrder, includeAllTypes: true };
			logger.error(`search(searchText=${searchText},sortOrder=${sortOrder},onSearchCallback=${onSearchCallback}): onSearchHandler → ${onSearchHandler}`, { "event.action": "search", "logrite.searchText": searchText, "logrite.sortOrder": sortOrder, "logrite.onSearchCallback": onSearchCallback });
			logger.debug(`search(searchText=${searchText},sortOrder=${sortOrder},onSearchCallback=${onSearchCallback}): callGetParams.includeDisabled → ${callGetParams.includeDisabled}`, { "event.action": "search", "logrite.searchText": searchText, "logrite.sortOrder": sortOrder, "logrite.onSearchCallback": onSearchCallback });
            if (this.options.includeDisabled) {
                callGetParams.includeDisabled = this.options.includeDisabled;
            }
            this._callGet(callGetParams, onSearchHandler, this.options.searchTreeMethod);
			logger.warn(`<<< Exiting search(searchText=${searchText},sortOrder=${sortOrder},onSearchCallback=${onSearchCallback})`, { "event.action": "search", "logrite.searchText": searchText, "logrite.sortOrder": sortOrder, "logrite.onSearchCallback": onSearchCallback });
        },

        getTree: function (sortOrder, onGetFirstLevelItemsCallback) {
			logger.error(`>>> Entering getTree(sortOrder=${sortOrder},onGetFirstLevelItemsCallback=${onGetFirstLevelItemsCallback})`, { "event.action": "getTree", "logrite.sortOrder": sortOrder, "logrite.onGetFirstLevelItemsCallback": onGetFirstLevelItemsCallback });
            var onGetFirstLevelItemsHandler = $.proxy(this._onGetTree, this, onGetFirstLevelItemsCallback);
            var callGetParams = { sortOrder: sortOrder, includeAllTypes: true };
			logger.error(`getTree(sortOrder=${sortOrder},onGetFirstLevelItemsCallback=${onGetFirstLevelItemsCallback}): onGetFirstLevelItemsHandler → ${onGetFirstLevelItemsHandler}`, { "event.action": "getTree", "logrite.sortOrder": sortOrder, "logrite.onGetFirstLevelItemsCallback": onGetFirstLevelItemsCallback });
			logger.debug(`getTree(sortOrder=${sortOrder},onGetFirstLevelItemsCallback=${onGetFirstLevelItemsCallback}): callGetParams.includeDisabled → ${callGetParams.includeDisabled}`, { "event.action": "getTree", "logrite.sortOrder": sortOrder, "logrite.onGetFirstLevelItemsCallback": onGetFirstLevelItemsCallback });
            if (this.options.includeDisabled) {
                callGetParams.includeDisabled = this.options.includeDisabled;
            }
            this._callGet(callGetParams, onGetFirstLevelItemsHandler, this.options.getTreeMethod);
			logger.warn(`<<< Exiting getTree(sortOrder=${sortOrder},onGetFirstLevelItemsCallback=${onGetFirstLevelItemsCallback})`, { "event.action": "getTree", "logrite.sortOrder": sortOrder, "logrite.onGetFirstLevelItemsCallback": onGetFirstLevelItemsCallback });
        },

        sortTree: function (sortOrder, rootNode, searchText, onSortTreeCallback) {
            var onSortTreeHandler = $.proxy(this._onGetTree, this, onSortTreeCallback);
            this._callGet({ treeAsJson: JSON.stringify(TreeNodeConverter.toTreeOfIds(rootNode)), sortOrder: sortOrder, searchText: searchText }, onSortTreeHandler, this.options.sortTreeMethod);
        }

    };

    DynamicTreeViewController._defaults = {
        serviceRoot: "InternalServices",
        sortTreeMethod: "ItemListService/SortPages",
        getTreeMethod: "ItemListService/GetPages",
        searchTreeMethod: "ItemListService/SearchPages",
        getNodeDescendantsMethod: "ItemListService/GetPageDescendants",
        getTreeWithNodeMethod: "ItemListService/GetTreePathForPage",
        rootId: "Root"
    };

    DynamicTreeViewController.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DynamicTreeViewController._defaults, settings);
        }
        return DynamicTreeViewController._defaults;
    };

}).apply(dnn, [jQuery, window, document]);
