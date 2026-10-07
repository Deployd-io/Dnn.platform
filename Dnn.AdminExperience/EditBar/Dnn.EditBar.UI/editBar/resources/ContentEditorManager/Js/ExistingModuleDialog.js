var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    var moduleAlias = { 'Content Layout': 'Grids' };

    ///dnnExistingModuleDialog Plugin

    var dnnExistingModuleDialog = dnn.dnnExistingModuleDialog = function (options) {
		logger.error(`>>> Entering dnnExistingModuleDialog(options=${options})`, { "event.action": "dnnExistingModuleDialog", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`dnnExistingModuleDialog(options=${options}): this.options → ${this.options}`, { "event.action": "dnnExistingModuleDialog", "logrite.options": options });
    };

    dnnExistingModuleDialog.prototype = {
        constructor: dnnExistingModuleDialog,
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, dnnExistingModuleDialog.defaultOptions, this.options);

            var layout = this._generateLayout();
			logger.error(`init(): layout → ${layout}`, { "event.action": "init" });
            $(document.body).append(layout);
            layout.hide();

            this._moduleDialog = dnn.ContentEditorManager.getModuleDialog();
            this._addCloseButton();

            this._initUI();
            this._attachEvents();
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        apply: function (moduleManager) {
			logger.error(`>>> Entering apply(moduleManager=${moduleManager})`, { "event.action": "apply", "logrite.moduleManager": moduleManager });
            this._moduleDialog.apply(moduleManager, true);
			logger.warn(`<<< Exiting apply(moduleManager=${moduleManager})`, { "event.action": "apply", "logrite.moduleManager": moduleManager });
            return this;
        },

        open: function () {
			logger.error('>>> Entering open()', { "event.action": "open" });
            this._showDialog();
            this._isOpen = true;
            this._noFloat = false;
			logger.debug(`open(): this._isOpen → ${this._isOpen}`, { "event.action": "open" });
			logger.warn('<<< Exiting open()', { "event.action": "open" });
            return this;
        },

        close: function (callback) {
			logger.error(`>>> Entering close(callback=${callback})`, { "event.action": "close", "logrite.callback": callback });
            this._isOpen = false;
            this._hideDialog(callback);
			logger.warn(`<<< Exiting close(callback=${callback})`, { "event.action": "close", "logrite.callback": callback });
			logger.debug(`close(callback=${callback}): this._isOpen → ${this._isOpen}`, { "event.action": "close", "logrite.callback": callback });
            return this;
        },

        getModuleDialog: function () {
			logger.error('>>> Entering getModuleDialog()', { "event.action": "getModuleDialog" });
            if (!this._moduleDialog) {
                this._moduleDialog = dnn.ContentEditorManager.getModuleDialog();
            }

			logger.warn('<<< Exiting getModuleDialog()', { "event.action": "getModuleDialog" });
            return this._moduleDialog;
        },

        getModuleManager: function () {
			logger.error('>>> Entering getModuleManager()', { "event.action": "getModuleManager" });
			logger.warn('<<< Exiting getModuleManager()', { "event.action": "getModuleManager" });
            return this.getModuleDialog().getModuleManager();
        },

        getDefaultPane: function () {
			logger.error('>>> Entering getDefaultPane()', { "event.action": "getDefaultPane" });
			logger.warn('<<< Exiting getDefaultPane()', { "event.action": "getDefaultPane" });
            return this.getModuleDialog().getDefaultPane.apply(this.getModuleDialog(), arguments);
        },

        getPaneById: function () {
			logger.error('>>> Entering getPaneById()', { "event.action": "getPaneById" });
			logger.warn('<<< Exiting getPaneById()', { "event.action": "getPaneById" });
            return this.getModuleDialog().getPaneById.apply(this.getModuleDialog(), arguments);
        },

        isOpen: function () {
			logger.error('>>> Entering isOpen()', { "event.action": "isOpen" });
			logger.warn('<<< Exiting isOpen()', { "event.action": "isOpen" });
            return this._isOpen;
        },

        refreshPane: function () {
			logger.error('>>> Entering refreshPane()', { "event.action": "refreshPane" });
			logger.warn('<<< Exiting refreshPane()', { "event.action": "refreshPane" });
            return this.getModuleDialog().refreshPane.apply(this.getModuleDialog(), arguments);
        },

        addModule: function (moduleId, callback, callbackBeforeRefresh) {
			logger.error(`>>> Entering addModule(moduleId=${moduleId},callback=${callback},callbackBeforeRefresh=${callbackBeforeRefresh})`, { "event.action": "addModule", "logrite.moduleId": moduleId, "logrite.callback": callback, "logrite.callbackBeforeRefresh": callbackBeforeRefresh });
            if (this.getModuleDialog()._working) {
                return false;
            }

            this.getModuleDialog()._working = true;

            var params = {
                Visibility: 0,
                Position: -1,
                Page: this._selectedPageId,
                Module: moduleId,
                Pane: this.getModuleDialog().options.paneName,
                AddExistingModule: true,
                CopyModule: this._dialogLayout.find('input.copy-module').prop('checked'),
                Sort: -1
            };

            this._moduleDialog._addingDesktopModuleId = moduleId;
            var handler = this;

            this._moduleDialog.showProgressBar();

            this.getModuleDialog()._getService().request('AddModule', 'POST', params, function (data) {
                if (callbackBeforeRefresh && typeof callback === "function") {
                    callback.call(handler.getModuleManager().getPane(), [data.TabModuleID]);
                    callback = null;
                }

                handler.getModuleDialog()._addModuleComplete(data, callback);
            });

			logger.warn(`<<< Exiting addModule(moduleId=${moduleId},callback=${callback},callbackBeforeRefresh=${callbackBeforeRefresh})`, { "event.action": "addModule", "logrite.moduleId": moduleId, "logrite.callback": callback, "logrite.callbackBeforeRefresh": callbackBeforeRefresh });
            return false;
        },

        _onSelectPageChanged: function (value) {
			logger.error(`>>> Entering _onSelectPageChanged(value=${value})`, { "event.action": "_onSelectPageChanged", "logrite.value": value });
            this._selectedPageId = parseInt(value.key);
            this._loadModuleList();
			logger.warn(`<<< Exiting _onSelectPageChanged(value=${value})`, { "event.action": "_onSelectPageChanged", "logrite.value": value });
        },

        _createDropdown: function (element, options) {
			logger.error(`>>> Entering _createDropdown(element=${element},options=${options})`, { "event.action": "_createDropdown", "logrite.element": element, "logrite.options": options });
            var selectPageCallback = options.selectPageCallback;
            var id = "#{0}".replace(/\{0\}/g, element.id);
			logger.error(`_createDropdown(element=${element},options=${options}): id → ${id}`, { "event.action": "_createDropdown", "logrite.element": element, "logrite.options": options });

            var selectPageProxyCallback = function () {
				logger.error('>>> Entering selectPageProxyCallback()', { "event.action": "selectPageProxyCallback" });
                selectPageCallback(this.selectedItem());
				logger.warn('<<< Exiting selectPageProxyCallback()', { "event.action": "selectPageProxyCallback" });
            };

            var defaultOptions = {
                disabled: false,
                initialState: {
                    selectedItem: options.selectedFolder
                },
                services: {
                    moduleId: '',
                    serviceRoot: 'InternalServices',
                    rootId: 'Root',
                    parameters: { includeAllTypes: "true", portalId: options.portalId },
                    includeDisabled: true
                },
                onSelectionChangedBackScript: selectPageProxyCallback
            };

            $.extend(true, defaultOptions, options);

            dnn.createDropDownList(id, defaultOptions, {});
			logger.warn(`<<< Exiting _createDropdown(element=${element},options=${options})`, { "event.action": "_createDropdown", "logrite.element": element, "logrite.options": options });
        },

        _createPagesDropdown: function (portalId) {
			logger.error(`>>> Entering _createPagesDropdown(portalId=${portalId})`, { "event.action": "_createPagesDropdown", "logrite.portalId": portalId });
            var handler = this;
            var resx = dnn.ContentEditorManagerResources;
            var pagePickerOptions = {
                selectPageCallback: $.proxy(handler._onSelectPageChanged, handler),
                portalId: portalId,
                selectedFolder: -1,
                selectedItemCss: 'selected-item',
                internalStateFieldId: 'AddExistingModule_FolderPicker_State',
                selectItemDefaultText: resx.pagePicker_selectItemDefaultText,
                itemList: {
                    sortAscendingButtonTitle: resx.pagePicker_sortAscendingButtonTitle,
                    unsortedOrderButtonTooltip: resx.pagePicker_unsortedOrderButtonTooltip,
                    sortAscendingButtonTooltip: resx.pagePicker_sortAscendingButtonTooltip,
                    sortDescendingButtonTooltip: resx.pagePicker_sortDescendingButtonTooltip,
                    selectedItemExpandTooltip: resx.pagePicker_selectedItemExpandTooltip,
                    selectedItemCollapseTooltip: resx.pagePicker_selectedItemCollapseTooltip,
                    searchInputPlaceHolder: resx.pagePicker_searchInputPlaceHolder,
                    clearButtonTooltip: resx.pagePicker_clearButtonTooltip,
                    searchButtonTooltip: resx.pagePicker_searchButtonTooltip,
                    loadingResultText: resx.pagePicker_loadingResultText,
                    resultsText: resx.pagePicker_resultsText,
                    firstItem: null,
                    disableUnspecifiedOrder: true
                },
                services: {
                    rootNodeName: resx.pagePicker_selectItemDefaultText
                }
            };

            handler._createDropdown($('#AddExistingModule_Pages')[0], pagePickerOptions);
			logger.warn(`<<< Exiting _createPagesDropdown(portalId=${portalId})`, { "event.action": "_createPagesDropdown", "logrite.portalId": portalId });
        },

        _calcPosition: function (handler) {
			logger.error(`>>> Entering _calcPosition(handler=${handler})`, { "event.action": "_calcPosition", "logrite.handler": handler });
            var left, top;
            var dialogWidth = this._dialogLayout.outerWidth();
			logger.error(`_calcPosition(handler=${handler}): dialogWidth → ${dialogWidth}`, { "event.action": "_calcPosition", "logrite.handler": handler });
            var dialogHeight = this._dialogLayout.outerHeight();
			logger.error(`_calcPosition(handler=${handler}): dialogHeight → ${dialogHeight}`, { "event.action": "_calcPosition", "logrite.handler": handler });
            var personaBarWidth = $('#personaBar-iframe').outerWidth();
            left = $(document).scrollLeft() + personaBarWidth / 2 + ($(window).width() - dialogWidth) / 2;
			logger.error(`_calcPosition(handler=${handler}): left → ${left}`, { "event.action": "_calcPosition", "logrite.handler": handler });
			logger.error(`_calcPosition(handler=${handler}): personaBarWidth → ${personaBarWidth}`, { "event.action": "_calcPosition", "logrite.handler": handler });
            top = $(document).scrollTop() + ($(window).height() - dialogHeight) / 2;
            this._dialogLayout.css({
                left: left,
                top: top
            });
        },

        _generateLayout: function () {
			logger.error('>>> Entering _generateLayout()', { "event.action": "_generateLayout" });
            var layout = this._dialogLayout = $('' +
                '<div class="dnnModuleDialog dnnExistingModuleDialog">' +
                    '<div class="dnnPageContainer">' +
                        '<div class="dnnPage">' +
                            '<div class="dnnDialogTitle">' +
                                '<span class="title">' + dnn.ContentEditorManagerResources.addExistingModule + '</span>' +
                            '</div>' +
                            '<div class="dnnDialogBody dnnModuleList">' +
                                '<div class="dnnModuleHeader">' +
                                    '<ul>' +
                                        '<li class="sites">' +
                                            '<label>' + dnn.ContentEditorManagerResources.site + '</label>' +
                                            '<select id="AddExistingModule_Sites" class="dnnDropDownList site">' +
                                            '</select>' +
                                        '</li>' +
                                        '<li>' +
                                            '<label>' + dnn.ContentEditorManagerResources.page + '</label>' +
                                            '<div id="AddExistingModule_Pages" class="dnnDropDownList page">' +
                                                '<div class="selected-item">' +
                                                    '<a href="javascript:void(0);" title="Click to expand" class="selected-value">Select Page</a>' +
                                                '</div>' +
                                            '</div>' +
                                            '<input type="hidden" id="AddExistingModule_FolderPicker_State" />' +
                                        '</li>' +
                                        '<li>' +
                                            '<input type="checkbox" class="copy-module" checked="true" aria-label="Copy" /><label>' + dnn.ContentEditorManagerResources.makeCopy + '</label>' +
                                        '</li>' +
                                        '<div class="clear"></div>' +
                                    '</ul>' +
                                '</div>' +
                                '<div class="listContainer listAll"><ul></ul></div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
					'<div class="dnnProgressBar"><span /></div>' +
                '</div>');

			logger.warn('<<< Exiting _generateLayout()', { "event.action": "_generateLayout" });
            return layout;
        },

        _addCloseButton: function () {
			logger.error('>>> Entering _addCloseButton()', { "event.action": "_addCloseButton" });
            var closeButton = this._closeButton = $('<span class="btn-close" />');
            $(".dnnExistingModuleDialog .dnnDialogTitle .title").after(closeButton);
			logger.error(`_addCloseButton(): closeButton → ${closeButton}`, { "event.action": "_addCloseButton" });

			logger.warn('<<< Exiting _addCloseButton()', { "event.action": "_addCloseButton" });
            return closeButton;
        },

        _showDialog: function () {
			logger.error('>>> Entering _showDialog()', { "event.action": "_showDialog" });
            this._createMask();
            this._dialogLayout.show('fast', $.proxy(this._dialogOpened, this));
            $(document).on('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
			logger.warn('<<< Exiting _showDialog()', { "event.action": "_showDialog" });
        },

        _hideDialog: function (callback) {
			logger.error(`>>> Entering _hideDialog(callback=${callback})`, { "event.action": "_hideDialog", "logrite.callback": callback });
            this._destroyMask();
            this._dialogLayout.hide('fast', function () {
                if (typeof callback == "function") {
                    callback.call(this);
                }
            });

            $(document).off('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
			logger.warn(`<<< Exiting _hideDialog(callback=${callback})`, { "event.action": "_hideDialog", "logrite.callback": callback });
        },

        _createMask: function () {
			logger.error('>>> Entering _createMask()', { "event.action": "_createMask" });
            var mask = $('<div class="dnnDialogMask"></div>');
            mask.css({
                left: 0,
                top: 0,
                width: '100%',
                height: $(document).outerHeight()
            });

			logger.error(`_createMask(): mask → ${mask}`, { "event.action": "_createMask" });
            $(document.body).append(mask).css('overflow', 'hidden');

            this._calcPosition(this.getModuleManager().getExistingModuleHandler());
			logger.warn('<<< Exiting _createMask()', { "event.action": "_createMask" });
        },

        _destroyMask: function () {
			logger.error('>>> Entering _destroyMask()', { "event.action": "_destroyMask" });
            $(document.body).css('overflow', '').find('.dnnDialogMask').remove();
			logger.warn('<<< Exiting _destroyMask()', { "event.action": "_destroyMask" });
        },

        _dialogOpened: function () {
			logger.error('>>> Entering _dialogOpened()', { "event.action": "_dialogOpened" });
            this._calcPosition(this.getModuleManager().getExistingModuleHandler());

            //when dialog first open, raise dialog initial event
            if (!this._initialized) {
                this._dialogLayout.trigger('dialoginit');
                this._initialized = true;

				logger.debug(`_dialogOpened(): this._initialized → ${this._initialized}`, { "event.action": "_dialogOpened" });
                this._loadModuleList();
            }

            this._dialogLayout.trigger('dialogopen');
			logger.warn('<<< Exiting _dialogOpened()', { "event.action": "_dialogOpened" });
        },

        _loadSiteList: function() {
			logger.error('>>> Entering _loadSiteList()', { "event.action": "_loadSiteList" });
            if (!this.getModuleDialog()._getItemListService().isLoading()) {
                this.getModuleDialog()._getItemListService().request('GetPortalsInGroup', 'GET', {
                    sortOrder: 0
                }, $.proxy(this._renderSiteList, this));
				logger.warn('<<< Exiting _loadSiteList()', { "event.action": "_loadSiteList" });
            }
        },

        _loadModuleList: function () {
			logger.error('>>> Entering _loadModuleList()', { "event.action": "_loadModuleList" });
            this._dialogLayout.find('li.dnnModuleItem').remove();
            this._dialogLayout.find('div.dnnModuleDialog_ModuleListMessage').remove();

            if (!this._selectedPageId) {
                var container = this._dialogLayout.find(".dnnModuleList .listContainer.listAll ul");
				logger.error(`_loadModuleList(): container → ${container}`, { "event.action": "_loadModuleList" });
                container.after(this._getNoResultTemplate());
                return;
            }

            if (!this.getModuleDialog()._getService().isLoading()) {
				logger.warn('<<< Exiting _loadModuleList()', { "event.action": "_loadModuleList" });
                this.getModuleDialog()._getService().request('GetTabModules', 'GET', {
                    tab: this._selectedPageId
                }, $.proxy(this._renderModuleList, this));
            }
        },

        _renderSiteList: function (data) {
			logger.error(`>>> Entering _renderSiteList(data=${data})`, { "event.action": "_renderSiteList", "logrite.data": data });
            var select = document.querySelector('#AddExistingModule_Sites');
            data.sites.forEach((siteInfo, index) => {
				logger.error(`_renderSiteList(data=${data}): select → ${select}`, { "event.action": "_renderSiteList", "logrite.data": data });
                var portalId = parseInt(siteInfo.key.substr(2));
                var option = document.createElement('option');
				logger.debug(`_renderSiteList(data=${data}): option.value → ${option.value}`, { "event.action": "_renderSiteList", "logrite.data": data });
                option.innerText = siteInfo.value;
				logger.debug(`_renderSiteList(data=${data}): option.innerText → ${option.innerText}`, { "event.action": "_renderSiteList", "logrite.data": data });
                option.value = portalId;
                select.appendChild(option);
                if(data.portalId == portalId) {
                    select.options[index].setAttribute('selected', '');
                }
            });
            select.addEventListener('change', e => {
                var portalId = parseInt(e.target.value);
                this._createPagesDropdown(portalId);
            });
			logger.warn(`<<< Exiting _renderSiteList(data=${data})`, { "event.action": "_renderSiteList", "logrite.data": data });
        },

        _renderModuleList: function (data) {
			logger.error(`>>> Entering _renderModuleList(data=${data})`, { "event.action": "_renderModuleList", "logrite.data": data });
            var container = this._dialogLayout.find(".dnnModuleList .listContainer.listAll ul");

			logger.error(`_renderModuleList(data=${data}): container → ${container}`, { "event.action": "_renderModuleList", "logrite.data": data });
            if (data.length > 0) {
                container.next().remove();
                //render rearranged list
                for (var i = 0; i < data.length; i++) {
                    var itemData = data[i];

                    var $item = $(this._renderItem(itemData));
                    container.append($item);
                }

                this._dialogLayout.trigger('moduleloaded');

                this._initScrollView();
            }
            else {
                if (container.has("li").length === 0) {
                    $(".jspVerticalBar").hide();
                    //scroll top to render no modules message
                    $(".jspDrag").css("top", "0px");
                    $(".jspPane").css("top", "0px");
                    container.after(this._getNoResultTemplate());
					logger.warn(`<<< Exiting _renderModuleList(data=${data})`, { "event.action": "_renderModuleList", "logrite.data": data });
                }
            }
        },

        _getItemTemplate: function () {
			logger.error('>>> Entering _getItemTemplate()', { "event.action": "_getItemTemplate" });
            return '<li class="dnnModuleItem" data-moduleid="[$ModuleID$]">' +
                '<span class="icon [$ModuleName|css]"><img src="[$ModuleImage$]" alt="[$ModuleName$]" /></span>' +
                '<span class="title {0}">[$ModuleName$]</span>' +
                '</li>';
				logger.warn('<<< Exiting _getItemTemplate()', { "event.action": "_getItemTemplate" });
        },

        _getNoResultTemplate: function () {
			logger.error('>>> Entering _getNoResultTemplate()', { "event.action": "_getNoResultTemplate" });
			logger.warn('<<< Exiting _getNoResultTemplate()', { "event.action": "_getNoResultTemplate" });
            return '<div class="dnnModuleDialog_ModuleListMessage"><span>' + dnn.ContentEditorManagerResources.nomodules + '</span></div>';
        },

        _renderItem: function (item) {
			logger.error(`>>> Entering _renderItem(item=${item})`, { "event.action": "_renderItem", "logrite.item": item });
            var extraclass;
            var template = this._getItemTemplate();
            var renderItem = this._cloneItem(item);
			logger.error(`_renderItem(item=${item}): renderItem → ${renderItem}`, { "event.action": "_renderItem", "logrite.item": item });
			logger.debug(`_renderItem(item=${item}): renderItem.ModuleName → ${renderItem.ModuleName}`, { "event.action": "_renderItem", "logrite.item": item });
            if (moduleAlias[renderItem.ModuleName]) {
                renderItem.ModuleName = moduleAlias[renderItem.ModuleName];
            }

            for (var a in renderItem) {
                if (renderItem.hasOwnProperty(a)) {
                    var shortMatchRegex = new RegExp('\\[\\$' + a + '\\|(\\d+)\\$\\]', 'g');
					logger.error(`_renderItem(item=${item}): shortMatchRegex → ${shortMatchRegex}`, { "event.action": "_renderItem", "logrite.item": item });
                    var shortMatch = shortMatchRegex.exec(template);
					logger.error(`_renderItem(item=${item}): shortMatch → ${shortMatch}`, { "event.action": "_renderItem", "logrite.item": item });
                    while (shortMatch) {
                        var val = renderItem[a].toString();
						logger.error(`_renderItem(item=${item}): val → ${val}`, { "event.action": "_renderItem", "logrite.item": item });
                        var length = parseInt(shortMatch[1], 10);
						logger.error(`_renderItem(item=${item}): length → ${length}`, { "event.action": "_renderItem", "logrite.item": item });
                        if (val.length > length) {
                            val = val.substr(0, length) + "...";
                        }

                        template = template.replace(shortMatch[0], val);
                        shortMatch = shortMatchRegex.exec(template);

                    };

                    template = this._replaceAll(template, '[$' + a + '|css]', this._replaceAll(renderItem[a].toString().toLowerCase(), ' ', '-'));
                    template = this._replaceAll(template, '[$' + a + '$]', renderItem[a]);

                    if (a === 'ModuleName') {
                        extraclass = renderItem[a].length > 20 ? 'longTitle' : '';
                        template = template.replace('{0}', extraclass);
						logger.error(`_renderItem(item=${item}): extraclass → ${extraclass}`, { "event.action": "_renderItem", "logrite.item": item });
                    }
                }
            }

			logger.error(`_renderItem(item=${item}): template → ${template}`, { "event.action": "_renderItem", "logrite.item": item });
			logger.warn(`<<< Exiting _renderItem(item=${item})`, { "event.action": "_renderItem", "logrite.item": item });
            return template;
        },

        _cloneItem: function () {
			logger.error('>>> Entering _cloneItem()', { "event.action": "_cloneItem" });
			logger.warn('<<< Exiting _cloneItem()', { "event.action": "_cloneItem" });
            return this.getModuleDialog()._cloneItem.apply(this.getModuleDialog(), arguments);
        },

        _initScrollView: function () {
			logger.error('>>> Entering _initScrollView()', { "event.action": "_initScrollView" });
            var container = this._dialogLayout.find(".dnnModuleList");
            if (container.data('jsp')) {
				logger.error(`_initScrollView(): container → ${container}`, { "event.action": "_initScrollView" });
                container.data('jsp').reinitialise();
            } else {
                container.jScrollPane();
            }

            $(".jspVerticalBar").show();
			logger.warn('<<< Exiting _initScrollView()', { "event.action": "_initScrollView" });
        },

        _attachEvents: function () {
			logger.error('>>> Entering _attachEvents()', { "event.action": "_attachEvents" });
            this._dialogLayout.on('click', '.dnnModuleItem', $.proxy(this._doAddModule, this));
            $(window).on('resize', '', $.proxy(this._layoutResized, this));
            this._dialogLayout.on('click', 'span.btn-close', $.proxy(this._closeButtonHandler, this));
            $(window).on('beforeunload', $.proxy(this._windowBeforeUnload, this));
			logger.warn('<<< Exiting _attachEvents()', { "event.action": "_attachEvents" });
        },

        _initUI: function () {
			logger.error('>>> Entering _initUI()', { "event.action": "_initUI" });
            this._createPagesDropdown(undefined);
            this._dialogLayout.find('input[type="checkbox"]').dnnCheckbox();
            this._loadSiteList();
			logger.warn('<<< Exiting _initUI()', { "event.action": "_initUI" });
        },

        _closeButtonHandler: function () {
			logger.error('>>> Entering _closeButtonHandler()', { "event.action": "_closeButtonHandler" });
            this.getModuleManager().getExistingModuleHandler().click();
			logger.warn('<<< Exiting _closeButtonHandler()', { "event.action": "_closeButtonHandler" });
        },

        _handleKeyEvent: function (e) {
			logger.error(`>>> Entering _handleKeyEvent(e=${e})`, { "event.action": "_handleKeyEvent", "logrite.e": e });
            if (e.keyCode === 27) {
                this.getModuleManager().getExistingModuleHandler().click();
				logger.warn(`<<< Exiting _handleKeyEvent(e=${e})`, { "event.action": "_handleKeyEvent", "logrite.e": e });
            }
        },

        _doAddModule: function (e) {
			logger.error(`>>> Entering _doAddModule(e=${e})`, { "event.action": "_doAddModule", "logrite.e": e });
            var $item = $(e.target);
            $item = $item.hasClass('dnnModuleItem') ? $item : $item.parents('.dnnModuleItem');
            var handler = this;
            var moduleId = $item.data("moduleid");
            if (!moduleId || moduleId === -1) {
                return false;
            }
            this.addModule(moduleId, function () {
                handler._hideDialog();
            }, true);

			logger.error(`_doAddModule(e=${e}): moduleId → ${moduleId}`, { "event.action": "_doAddModule", "logrite.e": e });
			logger.warn(`<<< Exiting _doAddModule(e=${e})`, { "event.action": "_doAddModule", "logrite.e": e });
            return false;
        },

        _layoutResized: function (e) {
			logger.error(`>>> Entering _layoutResized(e=${e})`, { "event.action": "_layoutResized", "logrite.e": e });
            if (this.isOpen()) {
                this._calcPosition(this.getModuleManager().getExistingModuleHandler());
				logger.warn(`<<< Exiting _layoutResized(e=${e})`, { "event.action": "_layoutResized", "logrite.e": e });
            }
        },

        _replaceAll: function () {
			logger.error('>>> Entering _replaceAll()', { "event.action": "_replaceAll" });
			logger.warn('<<< Exiting _replaceAll()', { "event.action": "_replaceAll" });
            return this.getModuleDialog()._replaceAll.apply(this.getModuleDialog(), arguments);
        }
    };

    dnnExistingModuleDialog.defaultOptions = {};

    ///dnnModuleDialog Plugin END
}(jQuery));