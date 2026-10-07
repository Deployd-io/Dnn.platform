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
    // DropDownList
    //
    var DropDownList = this.DropDownList = function (element, options, controller) {
        this.element = element;
        this.options = options;
        this._controller = controller;

        this.init();
    };

    DropDownList.prototype = {

        constructor: DropDownList,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, DropDownList.defaults(), this.options);

            this.$this = $(this);

            this.$element = this.element ? $(this.element) : this._createLayout();

            this._$selectedItemContainer = this.$element.find("." + this.options.selectedItemCss);
            this._$selectedItemCaption = this._$selectedItemContainer.find("." + this.options.selectedValueCss);

            this._onOpenItemListHandler = $.proxy(this._onOpenItemList, this);
            this._onCloseItemListHandler = $.proxy(this._onCloseItemList, this);
            this._onSelectedItemCaptionClickHandler = $.proxy(this._onSelectedItemCaptionClick, this);

            this._onDocumentClickHandler = $.proxy(this._onDocumentClick, this);

            this._$selectedItemCaption.on("click", this._onSelectedItemCaptionClickHandler);

            this._state = new StateField(this.options.initialState, this.options.internalStateFieldId);

            var stateValue = this._state.val();
			logger.error(`init(): stateValue → ${stateValue}`, { "event.action": "init" });
            this._setSelectedItemCaption(stateValue ? stateValue.selectedItem : null);

            this.disabled(this.options.disabled);

            this._id = dnn.guid();
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        _createLayout: function () {
			logger.error('>>> Entering _createLayout()', { "event.action": "_createLayout" });
            var layout = $("<div id='" + (this.options.containerId || dnn.uid()) + "' class='" + this.options.containerCss + "'/>")
                    .append($("<div class='" + this.options.selectedItemCss + "'/>")
                        .append($("<a href='javascript:void(0);' class='" + this.options.selectedValueCss + "'/>")));
			logger.error(`_createLayout(): layout → ${layout}`, { "event.action": "_createLayout" });
            return layout;
        },
        
        id: function() {
			logger.error('>>> Entering id()', { "event.action": "id" });
			logger.warn('<<< Exiting id()', { "event.action": "id" });
            return this.$element.attr('id');
        },

        selectedItem: function (item) {
			logger.error(`>>> Entering selectedItem(item=${item})`, { "event.action": "selectedItem", "logrite.item": item });
            if (typeof item === "undefined") {
                // getter:
                var state = this._state.val();
                return state ? state.selectedItem : null;
            }
            // setter:
            this._state.val({ selectedItem: item });
            this._setSelectedItemCaption(item);
            if (this._treeView) {
                this._treeView.selectedId(item ? item.key : null);
            }
            this.$element.trigger("selectedItemChanged", item);
            
			logger.warn(`<<< Exiting selectedItem(item=${item})`, { "event.action": "selectedItem", "logrite.item": item });
            return item;
        },

        selectedPath: function () {
			logger.error('>>> Entering selectedPath()', { "event.action": "selectedPath" });
			logger.warn('<<< Exiting selectedPath()', { "event.action": "selectedPath" });
            return this._treeView ? this._treeView.selectedPath() : [];
        },

        disabled: function (disable) {
			logger.error(`>>> Entering disabled(disable=${disable})`, { "event.action": "disabled", "logrite.disable": disable });
            if (typeof disable !== "undefined") {
                if (disable) {
                    this._selectedItemTitle = this._$selectedItemCaption.prop("title");
                    this._$selectedItemCaption.removeAttr("href").removeAttr("title");
                }
                else {
                    this._$selectedItemCaption.prop("href", "javascript:void(0);").prop("title", this._selectedItemTitle || "");
                }
                this._disabled = disable;
				logger.warn(`<<< Exiting disabled(disable=${disable})`, { "event.action": "disabled", "logrite.disable": disable });
            }
			logger.debug(`disabled(disable=${disable}): this._disabled → ${this._disabled}`, { "event.action": "disabled", "logrite.disable": disable });
            return this._disabled;
        },
        
        refresh: function (parentId) {
			logger.error(`>>> Entering refresh(parentId=${parentId})`, { "event.action": "refresh", "logrite.parentId": parentId });
            this.options.services.parameters.parentId = parentId;
            if (this._treeView) {
                this._treeView._showTree();
                this._treeView._dynamicTree.updateLayout();
				logger.warn(`<<< Exiting refresh(parentId=${parentId})`, { "event.action": "refresh", "logrite.parentId": parentId });
            }
        },

        _setSelectedItemCaption: function (item) {
			logger.error(`>>> Entering _setSelectedItemCaption(item=${item})`, { "event.action": "_setSelectedItemCaption", "logrite.item": item });
            var caption = item ? item.value : "";
            this._$selectedItemCaption.text(caption || this.options.selectItemDefaultText);
			logger.error(`_setSelectedItemCaption(item=${item}): caption → ${caption}`, { "event.action": "_setSelectedItemCaption", "logrite.item": item });
        },

        _onOpenItemList: function () {
			logger.error('>>> Entering _onOpenItemList()', { "event.action": "_onOpenItemList" });
            this._$selectedItemCaption.addClass(this.options.openedItemListCss);
            $(document).on("click." + this._id, this._onDocumentClickHandler);

            this._treeView._dynamicTree.scrollToSelectedNode();
			logger.warn('<<< Exiting _onOpenItemList()', { "event.action": "_onOpenItemList" });
        },

        _onCloseItemList: function () {
			logger.error('>>> Entering _onCloseItemList()', { "event.action": "_onCloseItemList" });
            this._$selectedItemCaption.removeClass(this.options.openedItemListCss);
            $(document).off("click." + this._id, this._onDocumentClickHandler);
            this._$treeViewLayout.detach(); // keeps all jQuery data associated with the removed element
			logger.warn('<<< Exiting _onCloseItemList()', { "event.action": "_onCloseItemList" });
        },

        _openItemList: function () {
			logger.error('>>> Entering _openItemList()', { "event.action": "_openItemList" });
            var showTreeView = false;
            if (!this._treeView) {
                this._treeView = this._createTreeView();
                this._$treeViewLayout = this._treeView.$element;
                showTreeView = true;
            }
            // append treeView layout to DOM before calling show
            if (!$.inContainer(this.$element, this._$treeViewLayout[0])) {
                this.$element[0].appendChild(this._$treeViewLayout[0]);
            }
            if (showTreeView) {
                this._treeView.show();
            }
            this._$treeViewLayout.slideDown(this.options.openItemListSpeed, this._onOpenItemListHandler);
			logger.warn('<<< Exiting _openItemList()', { "event.action": "_openItemList" });
        },

        _isItemListOpen: function() {
			logger.error('>>> Entering _isItemListOpen()', { "event.action": "_isItemListOpen" });
			logger.warn('<<< Exiting _isItemListOpen()', { "event.action": "_isItemListOpen" });
            return this._$treeViewLayout && this._$treeViewLayout.is(':visible');
        },

        _closeItemList: function () {
			logger.error('>>> Entering _closeItemList()', { "event.action": "_closeItemList" });
            this._$treeViewLayout.slideUp(this.options.closeItemListSpeed, this._onCloseItemListHandler);
			logger.warn('<<< Exiting _closeItemList()', { "event.action": "_closeItemList" });
        },

        _onSelectedItemCaptionClick: function (eventObject) {
			logger.error(`>>> Entering _onSelectedItemCaptionClick(eventObject=${eventObject})`, { "event.action": "_onSelectedItemCaptionClick", "logrite.eventObject": eventObject });
            if (this._disabled) {
                return;
            }
            this._isItemListOpen() ? this._closeItemList() : this._openItemList();
			logger.warn(`<<< Exiting _onSelectedItemCaptionClick(eventObject=${eventObject})`, { "event.action": "_onSelectedItemCaptionClick", "logrite.eventObject": eventObject });
        },

        _onDocumentClick: function (eventObject) {
			logger.error(`>>> Entering _onDocumentClick(eventObject=${eventObject})`, { "event.action": "_onDocumentClick", "logrite.eventObject": eventObject });
            // clicked outside of the dropdown list
            // when click on the element, the onMouseClick handler is executed
            var clickedElement = eventObject.target;
            if (!$.inContainer(this.$element, clickedElement) && this._isItemListOpen()) {
                this._closeItemList();
				logger.warn(`<<< Exiting _onDocumentClick(eventObject=${eventObject})`, { "event.action": "_onDocumentClick", "logrite.eventObject": eventObject });
            }
        },

        _createTreeView: function () {
			logger.error('>>> Entering _createTreeView()', { "event.action": "_createTreeView" });
            var controller = this._controller ? this._controller : new dnn.DynamicTreeViewController(this.options.services);
            var treeView = new dnn.SortableTreeView(null, this.options.itemList, controller);
			logger.error(`_createTreeView(): controller → ${controller}`, { "event.action": "_createTreeView" });
			logger.error(`_createTreeView(): treeView → ${treeView}`, { "event.action": "_createTreeView" });
            var onChangeNodeHandler = $.proxy(this._onChangeNode, this);
            var onSelectNodeHandler = $.proxy(this._onSelectNode, this);
			logger.error(`_createTreeView(): onChangeNodeHandler → ${onChangeNodeHandler}`, { "event.action": "_createTreeView" });
			logger.error(`_createTreeView(): onSelectNodeHandler → ${onSelectNodeHandler}`, { "event.action": "_createTreeView" });
            var onTreeLoadedHandler = $.proxy(this._onTreeLoaded, this);
            var onShowChildrenHandler = $.proxy(this._onShowChildren, this);
			logger.error(`_createTreeView(): onTreeLoadedHandler → ${onTreeLoadedHandler}`, { "event.action": "_createTreeView" });
			logger.error(`_createTreeView(): onShowChildrenHandler → ${onShowChildrenHandler}`, { "event.action": "_createTreeView" });
            var onRequestExpandHandler = $.proxy(this._onRequestExpand, this);
            $(treeView).on("onchangenode", onChangeNodeHandler)
                       .on("onselectnode", onSelectNodeHandler)
                       .on("ontreeloaded", onTreeLoadedHandler)
                       .on("onloadchildren", onTreeLoadedHandler)
                       .on("onshowchildren", onShowChildrenHandler)
                       .on("onrequestexpand", onRequestExpandHandler);
            var item = this.selectedItem();
			logger.error(`_createTreeView(): onRequestExpandHandler → ${onRequestExpandHandler}`, { "event.action": "_createTreeView" });
			logger.error(`_createTreeView(): item → ${item}`, { "event.action": "_createTreeView" });
            treeView.selectedId(item ? item.key : null);
			logger.warn('<<< Exiting _createTreeView()', { "event.action": "_createTreeView" });
            return treeView;
        },
        
        _onTreeLoaded: function () {
			logger.error('>>> Entering _onTreeLoaded()', { "event.action": "_onTreeLoaded" });
            var id = this.$element[0].id;
            if (!id) {
                return;
            }

            var expandPath = dnn.getVar(id + "_expandPath");
			logger.error(`_onTreeLoaded(): expandPath → ${expandPath}`, { "event.action": "_onTreeLoaded" });

            if (expandPath == null) {
                $(this._treeView).off("onshowchildren");
            }

            if (expandPath && expandPath.length > 0) {
                var position = expandPath.indexOf(',') > -1 ? expandPath.indexOf(',') : expandPath.length;
				logger.error(`_onTreeLoaded(): position → ${position}`, { "event.action": "_onTreeLoaded" });
                var nodeId = expandPath.substr(0, position);
				logger.error(`_onTreeLoaded(): nodeId → ${nodeId}`, { "event.action": "_onTreeLoaded" });
                var leftPath = expandPath.substr(position);
				logger.error(`_onTreeLoaded(): leftPath → ${leftPath}`, { "event.action": "_onTreeLoaded" });
                if (leftPath.length > 1 && leftPath[0] == ',') {
                    leftPath = leftPath.substr(1);
                }

                dnn.setVar(id + "_expandPath", leftPath);
                var dynamicTree = this._treeView._dynamicTree;
                var node = dynamicTree._getNodeById(nodeId);
				logger.error(`_onTreeLoaded(): node → ${node}`, { "event.action": "_onTreeLoaded" });
                if (node) {
                    var expandIcon = dynamicTree._tree.getNodeElement(node).children(":first");
					logger.error(`_onTreeLoaded(): expandIcon → ${expandIcon}`, { "event.action": "_onTreeLoaded" });
                    if (!expandIcon.hasClass(dynamicTree.options.expandedCss)) {
                        expandIcon.trigger('click');
                    } else {
                        this._onTreeLoaded();
						logger.warn('<<< Exiting _onTreeLoaded()', { "event.action": "_onTreeLoaded" });
                    }
                }
            }
        },
        
        _onShowChildren: function () {
			logger.error('>>> Entering _onShowChildren()', { "event.action": "_onShowChildren" });
            this._treeView._dynamicTree.scrollToSelectedNode();
            var id = this.$element[0].id;
            if (id) {
                var expandPath = dnn.getVar(id + "_expandPath");
				logger.error(`_onShowChildren(): expandPath → ${expandPath}`, { "event.action": "_onShowChildren" });
                if (!expandPath) {
                    $(this._treeView).off("onshowchildren");
					logger.warn('<<< Exiting _onShowChildren()', { "event.action": "_onShowChildren" });
                }
            }
        },
        
        _onRequestExpand: function() {
			logger.error('>>> Entering _onRequestExpand()', { "event.action": "_onRequestExpand" });
            this._onTreeLoaded();
			logger.warn('<<< Exiting _onRequestExpand()', { "event.action": "_onRequestExpand" });
        },

        _onSelectNode: function(eventObject, node) {
			logger.error(`>>> Entering _onSelectNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onSelectNode", "logrite.eventObject": eventObject, "logrite.node": node });
            this._closeItemList();
			logger.warn(`<<< Exiting _onSelectNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onSelectNode", "logrite.eventObject": eventObject, "logrite.node": node });
        },

        _onChangeNode: function(eventObject, node) {
			logger.error(`>>> Entering _onChangeNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onChangeNode", "logrite.eventObject": eventObject, "logrite.node": node });
            var item = node ? dnn.TreeNodeConverter.toKeyValue(node.data) : null;
            this.selectedItem(item);
			logger.error(`_onChangeNode(eventObject=${eventObject},node=${node}): item → ${item}`, { "event.action": "_onChangeNode", "logrite.eventObject": eventObject, "logrite.node": node });

            if (this.options.onSelectionChanged && this.options.onSelectionChanged.length) {
                for (var i = 0, size = this.options.onSelectionChanged.length; i < size; i++) {
                    dnn.executeFunctionByName(this.options.onSelectionChanged[i], window, item, eventObject.target.$element);
                }
            }
            if (typeof this.options.onSelectionChangedBackScript === "function") {
                this.options.onSelectionChangedBackScript.apply(this);
            }
            this.$this.trigger($.Event("onchangenode"), [item]);
			logger.warn(`<<< Exiting _onChangeNode(eventObject=${eventObject},node=${node})`, { "event.action": "_onChangeNode", "logrite.eventObject": eventObject, "logrite.node": node });
        }

    };

    DropDownList._defaults = {
        containerCss: "dnnDropDownList",
        selectedItemCss: "selected-item",
        selectedValueCss: "selected-value",
        openedItemListCss: "opened",
        openItemListSpeed: "fast",
        closeItemListSpeed: "fast"
    };

    DropDownList.defaults = function (settings) {
		logger.error(`>>> Entering defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        if (typeof settings !== "undefined") {
            $.extend(DropDownList._defaults, settings);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        return DropDownList._defaults;
    };

    var StateField = this.StateField = function (initialState, stateElementId) {
		logger.error(`>>> Entering StateField(initialState=${initialState},stateElementId=${stateElementId})`, { "event.action": "StateField", "logrite.initialState": initialState, "logrite.stateElementId": stateElementId });
        if (stateElementId) {
            this._stateElement = document.getElementById(stateElementId);
        }
        this.val(initialState);
		logger.warn(`<<< Exiting StateField(initialState=${initialState},stateElementId=${stateElementId})`, { "event.action": "StateField", "logrite.initialState": initialState, "logrite.stateElementId": stateElementId });
    };

    StateField.prototype = {
        constructor: StateField,

        val: function (stateObject) {
			logger.error(`>>> Entering val(stateObject=${stateObject})`, { "event.action": "val", "logrite.stateObject": stateObject });
            if (typeof stateObject === "undefined") {
                // getter:
                if (typeof this._state === "undefined") {
                    try {
                        this._state = (this._stateElement && this._stateElement.value) ?
                            JSON.parse(this._stateElement.value) : null;
                    }
                    catch (ex) {
					logger.error(`Exception in val(stateObject=${stateObject}): ${ex.message}`, ex, { "event.action": "val", "event.category": "process", "logrite.stateObject": stateObject });
                    }
                }
                return this._state;
            }
            else {
                // setter:
                if (this._stateElement) {
                    var stateAsJson = stateObject ? JSON.stringify(stateObject) : "";
					logger.error(`val(stateObject=${stateObject}): stateAsJson → ${stateAsJson}`, { "event.action": "val", "logrite.stateObject": stateObject });
                    this._stateElement.value = stateAsJson;
                }
                this._state = stateObject;
				logger.warn(`<<< Exiting val(stateObject=${stateObject})`, { "event.action": "val", "logrite.stateObject": stateObject });
            }
			logger.debug(`val(stateObject=${stateObject}): this._state → ${this._state}`, { "event.action": "val", "logrite.stateObject": stateObject });
            return this._state;
        }

    };

}).apply(dnn, [jQuery, window, document]);


dnn.createDropDownList = function (selector, options, methods) {
	logger.error(`>>> Entering createDropDownList(selector=${selector},options=${options},methods=${methods})`, { "event.action": "createDropDownList", "logrite.selector": selector, "logrite.options": options, "logrite.methods": methods });
    $(document).ready(function () {
        var $element = $(selector);
        // in case when control is excluded from rendering by server tags logic,
        // a check on the html element existence is performed here:
        if ($element.length === 0) {
            return;
        }
        $.extend(options, methods);
        var element = $element[0];
        var instance = new dnn.DropDownList(element, options);
        if (element.id) {
            dnn[element.id] = instance;
        }
    });
	logger.warn(`<<< Exiting createDropDownList(selector=${selector},options=${options},methods=${methods})`, { "event.action": "createDropDownList", "logrite.selector": selector, "logrite.options": options, "logrite.methods": methods });
};

