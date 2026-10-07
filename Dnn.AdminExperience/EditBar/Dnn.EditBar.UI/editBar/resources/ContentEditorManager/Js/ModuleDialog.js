var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    var htmlModuleName = 'HTML';
    var htmlModuleId = null;
    var recommendedList = [];
    var moduleAlias = [];
    var listMode = 'category';

    ///dnnModuleDialog Plugin

    var dnnModuleDialog = dnn.dnnModuleDialog = function (options) {
		logger.error(`>>> Entering dnnModuleDialog(options=${options})`, { "event.action": "dnnModuleDialog", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`dnnModuleDialog(options=${options}): this.options → ${this.options}`, { "event.action": "dnnModuleDialog", "logrite.options": options });
    };

    dnnModuleDialog.prototype = {
        constructor: dnnModuleDialog,
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, dnnModuleDialog.defaultOptions, this.options);

            var layout = this._generateLayout();
			logger.error(`init(): layout → ${layout}`, { "event.action": "init" });
            $(document.body).append(layout);
            layout.hide();

            this._addCloseButton();
            this._addSearchBox();

            this._loadMore = true;
            this._startIndex = 0;
			logger.debug(`init(): this._loadMore → ${this._loadMore}`, { "event.action": "init" });
            this._pageSize = 10;
            this._bookmarkedModuleList = [];
			logger.debug(`init(): this._startIndex → ${this._startIndex}`, { "event.action": "init" });
            this._minInputLength = 2;
            this._inputDelay = 400;
			logger.debug(`init(): this._pageSize → ${this._pageSize}`, { "event.action": "init" });
            this._lastVal = '';
            this._supportCategory = true;
			logger.debug(`init(): this._bookmarkedModuleList → ${this._bookmarkedModuleList}`, { "event.action": "init" });
            this._callFromExistingModule = false;

			logger.debug(`init(): this._minInputLength → ${this._minInputLength}`, { "event.action": "init" });
            this._syncCompleteHandler = $.proxy(this._syncComplete, this);
            this._refreshCompleteHandler = $.proxy(this._refreshComplete, this);
			logger.debug(`init(): this._inputDelay → ${this._inputDelay}`, { "event.action": "init" });

            this._attachEvents();
			logger.debug(`init(): this._lastVal → ${this._lastVal}`, { "event.action": "init" });
        },

        apply: function (moduleManager, callFromExistingModule) {
            this._moduleManager = moduleManager;
            this._callFromExistingModule = callFromExistingModule;
            this.options = $.extend({}, this.options, {
                paneName: moduleManager.options.pane
            });
			logger.warn(`<<< Exiting apply(moduleManager=${moduleManager},callFromExistingModule=${callFromExistingModule})`, { "event.action": "apply", "logrite.moduleManager": moduleManager, "logrite.callFromExistingModule": callFromExistingModule });
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
            this._isOpen = false;
            this._hideDialog(callback);
			logger.warn(`<<< Exiting close(callback=${callback})`, { "event.action": "close", "logrite.callback": callback });
            return this;
        },

        getElement: function () {
			logger.error('>>> Entering getElement()', { "event.action": "getElement" });
			logger.warn('<<< Exiting getElement()', { "event.action": "getElement" });
            return this._dialogLayout;
        },

        getModuleManager: function () {
			logger.error('>>> Entering getModuleManager()', { "event.action": "getModuleManager" });
            if (!this._moduleManager) {
                this._moduleManager = this.getDefaultPane().data('dnnModuleManager');
				logger.warn('<<< Exiting getModuleManager()', { "event.action": "getModuleManager" });
            }
            return this._moduleManager;
        },

        getDefaultPane: function() {
			logger.error('>>> Entering getDefaultPane()', { "event.action": "getDefaultPane" });
			logger.warn('<<< Exiting getDefaultPane()', { "event.action": "getDefaultPane" });
            return this.getPaneById();
        },

        getPaneById: function (paneName) {
			logger.error(`>>> Entering getPaneById(paneName=${paneName})`, { "event.action": "getPaneById", "logrite.paneName": paneName });
            var defaultPane = null;
            if (!paneName) {
                paneName = 'contentpane';
            } else {
                paneName = paneName.toLowerCase();
            }
            $('.dnnSortable[id]').each(function () {
                var $pane = $(this);
                var paneId = $pane.attr('id').toLowerCase();
                var thisPaneName;
                if ($pane.data('name')) {
                    thisPaneName = $pane.data('name').toLowerCase();
                } else {
                    thisPaneName = paneId.replace(/^dnn_/, '').toLowerCase();
                }
                
				logger.debug(`getPaneById(paneName=${paneName}): thisPaneName → ${thisPaneName}`, { "event.action": "getPaneById", "logrite.paneName": paneName });
                if (paneName === paneId || paneName === thisPaneName) {
                    defaultPane = $pane;
                    return false;
                }

			logger.warn(`<<< Exiting getPaneById(paneName=${paneName})`, { "event.action": "getPaneById", "logrite.paneName": paneName });
                return true;
            });

            return defaultPane;
        },

        isOpen: function () {
			logger.error('>>> Entering isOpen()', { "event.action": "isOpen" });
			logger.warn('<<< Exiting isOpen()', { "event.action": "isOpen" });
            return this._isOpen;
        },

        addPage: function (id, content) {
			if ($('#' + id).length) {
				return;
			}

            var pageContainer = this._dialogLayout.find('.dnnPageContainer');
            var newPage = $('<div class="dnnPage ' + id + '" id="' + id + '"></div>');
			logger.error(`addPage(id=${id},content=${content}): newPage → ${newPage}`, { "event.action": "addPage", "logrite.id": id, "logrite.content": content });
            newPage.append(content);
            pageContainer.append(newPage);
			logger.warn(`<<< Exiting addPage(id=${id},content=${content})`, { "event.action": "addPage", "logrite.id": id, "logrite.content": content });
        },

        showPage: function (id, immediate) {
			logger.error(`>>> Entering showPage(id=${id},immediate=${immediate})`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
            var pageContainer = this._dialogLayout.find('.dnnPageContainer');
            var page;
			logger.error(`showPage(id=${id},immediate=${immediate}): pageContainer → ${pageContainer}`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
            if (typeof id == "number") {
                page = pageContainer.find('.dnnPage').eq(id);
            } else {
                page = pageContainer.find('.dnnPage.' + id);
            }

			logger.error(`showPage(id=${id},immediate=${immediate}): page → ${page}`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
			logger.debug(`showPage(id=${id},immediate=${immediate}): page → ${page}`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
            var left = page.position().left;
			logger.error(`showPage(id=${id},immediate=${immediate}): left → ${left}`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
            if (!immediate) {
                var handler = this;
                pageContainer.animate({ marginLeft: 0 - left }, 'fast', function () {
                    handler._dialogLayout.trigger('pageChanged', [page]);
                });
            } else {
                pageContainer.css({ marginLeft: 0 - left });
				logger.warn(`<<< Exiting showPage(id=${id},immediate=${immediate})`, { "event.action": "showPage", "logrite.id": id, "logrite.immediate": immediate });
            }
        },

        refreshPane: function (paneName, args, callback, callOnReload) {
			logger.error(`>>> Entering refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload})`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
            var paneId;
            if (!paneName) {
                paneId = this.getModuleManager().getPane().attr('id');
            } else {
                paneId = this.getPaneById(paneName).attr('id');
            }

			logger.error(`refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload}): parentPane → ${parentPane}`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
            var pane = $('#' + paneId);
			logger.error(`refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload}): pane → ${pane}`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
            var parentPane = pane.data('parentpane');
            if (parentPane) {
                this.refreshPane(parentPane, args, callback);
                return;
            }
            //set module manager to current refresh pane.
            this._moduleManager = pane.data('dnnModuleManager');
            var ajaxPanel = $('#' + paneId + "_SyncPanel");
			logger.error(`refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload}): ajaxPanel → ${ajaxPanel}`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
            if (ajaxPanel.length) {
                //remove action menus from DOM bbefore fresh pane.
                var handler = this;
                pane.find('div.DnnModule').each(function () {
                    var moduleId = handler._moduleManager._findModuleId($(this));
                    $('#moduleActions-' + moduleId).remove();
                });

                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._refreshCompleteHandler);
                this._refreshPaneId = paneId;
                this._refreshCallback = callback;
				logger.debug(`refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload}): this._refreshPaneId → ${this._refreshPaneId}`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
                window.setTimeout(function () { __doPostBack(ajaxPanel.attr('id'), args); }, 100);
            } else {
				logger.debug(`refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload}): this._refreshCallback → ${this._refreshCallback}`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
                //save the args into cookie, after page reload then catch the cookie
                //and float the module for drag
                if (args && !this._noFloat) {
                    this._setCookie('CEM_CallbackData', args);
                }

                if (callOnReload && typeof callback == "function") {
                    callback.call($('#' + paneId), [true]);
                }

                location.reload();
				logger.warn(`<<< Exiting refreshPane(paneName=${paneName},args=${args},callback=${callback},callOnReload=${callOnReload})`, { "event.action": "refreshPane", "logrite.paneName": paneName, "logrite.args": args, "logrite.callback": callback, "logrite.callOnReload": callOnReload });
            }
        },

        noFloat: function() {
			logger.error('>>> Entering noFloat()', { "event.action": "noFloat" });
            this._noFloat = true;
			logger.debug(`noFloat(): this._noFloat → ${this._noFloat}`, { "event.action": "noFloat" });
        },

        addModule: function (moduleId, callback, callbackBeforeRefresh) {
            if (this._working) {
                return false;
            }

            this._working = true;

            var params = {
                Visibility: 0,
                Position: -1,
                Module: moduleId,
                Pane: this.options.paneName,
                AddExistingModule: false,
                CopyModule: false,
                Sort: -1
            };

            this._addingDesktopModuleId = moduleId;
            var handler = this;

            this.showProgressBar();

            this._getService().request('AddModule', 'POST', params, function (data) {
				if (callbackBeforeRefresh && typeof callback === "function") {
					callback.call(handler.getModuleManager().getPane(), [data.TabModuleID]);
					callback = null;
				}

                handler._addModuleComplete(data, callback);
            });

			logger.error('>>> Entering _getService()', { "event.action": "_getService" });
			logger.warn('<<< Exiting _getService()', { "event.action": "_getService" });
			logger.warn(`<<< Exiting addModule(moduleId=${moduleId},callback=${callback},callbackBeforeRefresh=${callbackBeforeRefresh})`, { "event.action": "addModule", "logrite.moduleId": moduleId, "logrite.callback": callback, "logrite.callbackBeforeRefresh": callbackBeforeRefresh });
            return false;
        },

        setModuleId: function (moduleId) {
			logger.error(`>>> Entering setModuleId(moduleId=${moduleId})`, { "event.action": "setModuleId", "logrite.moduleId": moduleId });
            if (moduleId <= 0) {
                this._removeCookie('CEM_NewModuleId');
                this._removeCookie('CEM_ExistingModule');
            } else {
                this._setCookie('CEM_NewModuleId', moduleId);
                this._setCookie('CEM_ExistingModule', this._callFromExistingModule);
				logger.warn(`<<< Exiting setModuleId(moduleId=${moduleId})`, { "event.action": "setModuleId", "logrite.moduleId": moduleId });
            }
        },

        getModuleId: function () {
			logger.error('>>> Entering getModuleId()', { "event.action": "getModuleId" });
			logger.warn('<<< Exiting getModuleId()', { "event.action": "getModuleId" });
            return this._getCookie('CEM_NewModuleId');
        },

        getSiteRoot: function () {
			logger.error('>>> Entering getSiteRoot()', { "event.action": "getSiteRoot" });
			logger.warn('<<< Exiting getSiteRoot()', { "event.action": "getSiteRoot" });
            return dnn.getVar("sf_siteRoot", "/").replace(/\/$/, "");
        },

		showProgressBar: function() {
			logger.error('>>> Entering showProgressBar()', { "event.action": "showProgressBar" });
			var $progressBar = this._dialogLayout.find('.dnnProgressBar');
			var $bar = $progressBar.find('> span');
			$bar.width(0);
			logger.warn('<<< Exiting showProgressBar()', { "event.action": "showProgressBar" });
			$progressBar.show();
			$bar.animate({
				width: $progressBar.width() * 0.8
			}, 5000);
		},

		hideProgressBar: function() {
			logger.error('>>> Entering hideProgressBar()', { "event.action": "hideProgressBar" });
			var $progressBar = this._dialogLayout.find('.dnnProgressBar');
			var $bar = $progressBar.find('> span');
			$bar.stop().width($progressBar.width());
			$progressBar.hide();
			logger.warn('<<< Exiting hideProgressBar()', { "event.action": "hideProgressBar" });
		},

		_refreshComplete: function (sender, args) {
			logger.error(`>>> Entering _refreshComplete(sender=${sender},args=${args})`, { "event.action": "_refreshComplete", "logrite.sender": sender, "logrite.args": args });
			this.hideProgressBar();
            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(this._refreshCompleteHandler);

			if (args.get_error() != undefined){
				args.set_errorHandled(true);
				location.reload();
			}

            var handler = this;
            var moduleManager = this.getModuleManager();
			logger.warn(`<<< Exiting _refreshComplete(sender=${sender},args=${args})`, { "event.action": "_refreshComplete", "logrite.sender": sender, "logrite.args": args });
            //run inline script execute
            setTimeout(function () {
                if (typeof window.dnnLoadScriptsInAjaxMode === "undefined" || window.dnnLoadScriptsInAjaxMode.length == 0) {
                    handler._executeModuleScripts();
                } else {
                    $(window).one('dnnScriptLoadComplete', function() {
                        handler._executeModuleScripts();
                    });
                }
            }, 50);

            var callback = this._refreshCallback;
            var paneId = this._refreshPaneId;
            var $pane = $('#' + paneId);
            this._refreshPaneId = this._refreshCallback = null;

            if (typeof callback == "function") {
                callback.call($pane);
            }
        },

        _executeModuleScripts: function () {
			logger.error('>>> Entering _executeModuleScripts()', { "event.action": "_executeModuleScripts" });
            $(window).off('load');
            var handler = this;
            var moduleManager = this.getModuleManager();
			logger.error(`_executeModuleScripts(): moduleManager → ${moduleManager}`, { "event.action": "_executeModuleScripts" });
            moduleManager.getPane().find('div.DnnModule').not('[class~="floating"]').find('script').each(function() {
                var script = $(this).html();
                if (script) {
                    handler._executeScript(script);
                }
            });

            //trigger window load event
            $(window).trigger('load').trigger('resize');
			logger.warn('<<< Exiting _executeModuleScripts()', { "event.action": "_executeModuleScripts" });
        },

        _executeScript: function (script) {
			logger.error(`>>> Entering _executeScript(script=${script})`, { "event.action": "_executeScript", "logrite.script": script });
            try {
                if (window.eval) {
                    return (function () {
					logger.warn(`<<< Exiting _executeScript(script=${script})`, { "event.action": "_executeScript", "logrite.script": script });
                        return window.eval.call(window, script);
                    })();
                }
		        else if (window.execScript) {
			        return window.execScript(script);
		        } else {
                    return null;
                }
	        } catch (ex) {
		        return null;
	        }
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
                '<div class="dnnModuleDialog">' +
                    '<div class="dnnPageContainer">' +
                        '<div class="dnnPage">' +
                            '<div class="dnnDialogTitle">' +
                                '<span class="title">' + dnn.ContentEditorManagerResources.title + '</span>' +
                            '</div>' +
                            '<div class="dnnDialogBody dnnModuleList">' +
                                '<h2>' + dnn.ContentEditorManagerResources.categoryRecommended + '</h2>' +
                                '<div class="listContainer listRecommended"><ul></ul></div>' +
                                '<h2>' + dnn.ContentEditorManagerResources.categoryAll + '</h2>' +
                                '<div class="listContainer listAll"><ul></ul></div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
					'<div class="dnnProgressBar"><span /></div>' +
                '</div>');

			logger.warn('<<< Exiting _generateLayout()', { "event.action": "_generateLayout" });
            return layout;
        },

        _addCloseButton: function() {
			logger.error('>>> Entering _addCloseButton()', { "event.action": "_addCloseButton" });
            var closeButton = this._closeButton = $('<span class="btn-close" />');
            $(".dnnModuleDialog .dnnDialogTitle .title").after(closeButton);
			logger.error(`_addCloseButton(): closeButton → ${closeButton}`, { "event.action": "_addCloseButton" });

			logger.warn('<<< Exiting _addCloseButton()', { "event.action": "_addCloseButton" });
            return closeButton;
        },

        _addSearchBox: function () {
			logger.error('>>> Entering _addSearchBox()', { "event.action": "_addSearchBox" });
            var searchBox = $(
                    '<div class="search-container"><div class="search-input-container">' +
                    '<input type="text" id="AddModule_SearchModulesInput" placeholder="' + dnn.ContentEditorManagerResources.searchPlaceHolder + '" class="search-input" aria-label="Search"/></div>' +
                    '<a href="javascript:void(0);" title="Search" class="search-button" aria-label="Search"></a>' +
                    '<a href="javascript:void(0);" title="Clear" class="clear-button" aria-label="Clear"></a></div>' +
                    '</div>');

            $(".dnnModuleDialog .dnnDialogTitle .title").after(searchBox);

			logger.warn('<<< Exiting _addSearchBox()', { "event.action": "_addSearchBox" });
            return searchBox;
        },

        _showDialog: function () {
			logger.error('>>> Entering _showDialog()', { "event.action": "_showDialog" });
            this._createMask();
            this.showPage(0, true);
            this._dialogLayout.show('fast', $.proxy(this._dialogOpened, this));
            $(document).on('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
			logger.warn('<<< Exiting _showDialog()', { "event.action": "_showDialog" });
        },

        _hideDialog: function (callback) {
			logger.error(`>>> Entering _hideDialog(callback=${callback})`, { "event.action": "_hideDialog", "logrite.callback": callback });
            this._destroyMask();
            this._dialogLayout.hide('fast', function() {
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

            this._calcPosition(this._moduleManager.getHandler());
			logger.warn('<<< Exiting _createMask()', { "event.action": "_createMask" });
        },

        _destroyMask: function () {
			logger.error('>>> Entering _destroyMask()', { "event.action": "_destroyMask" });
            $(document.body).css('overflow', '').find('.dnnDialogMask').remove();
			logger.warn('<<< Exiting _destroyMask()', { "event.action": "_destroyMask" });
        },

        _dialogOpened: function () {
			logger.error('>>> Entering _dialogOpened()', { "event.action": "_dialogOpened" });
            this._calcPosition(this._moduleManager.getHandler());

            //when dialog first open, raise dialog initial event
            if (!this._initialized) {
                this._dialogLayout.trigger('dialoginit');
                this._initialized = true;

				logger.debug(`_dialogOpened(): this._initialized → ${this._initialized}`, { "event.action": "_dialogOpened" });
                this._loadRecommendedList(function () {
                    this._loadModuleList('', false);
                });
            }

            this._dialogLayout.trigger('dialogopen');
			logger.warn('<<< Exiting _dialogOpened()', { "event.action": "_dialogOpened" });
        },

        _loadModuleList: function (val, isSearch) {
			logger.error(`>>> Entering _loadModuleList(val=${val},isSearch=${isSearch})`, { "event.action": "_loadModuleList", "logrite.val": val, "logrite.isSearch": isSearch });
            if ((this._loadMore || isSearch) && !this._getService().isLoading()) {
                if (isSearch && this._startIndex == 0) {
                    $('.listContainer.listAll ul li.dnnModuleItem').remove();
                    $('.listContainer.listAll div.dnnModuleDialog_ModuleListMessage').remove();
                }

                if (val === '' && this._supportCategory) {
                    this._switchToCategoryMode();
                } else {
                    this._switchToSimpleMode();
                }

                if (this._loadMore) {
					logger.warn(`<<< Exiting _loadModuleList(val=${val},isSearch=${isSearch})`, { "event.action": "_loadModuleList", "logrite.val": val, "logrite.isSearch": isSearch });
                    var isAdmin = dnn.getVar('editbar_isAdmin') === "True";
                    this._getService().request('GetPortalDesktopModules', 'GET', {
                        category: 'All',
                        loadingStartIndex: this._startIndex,
                        loadingPageSize: this._pageSize,
                        searchTerm: val,
                        excludeCategories: isAdmin ? '' : 'Admin,Professional',
                        sortBookmarks: true,
                        topModule: htmlModuleName
                    }, $.proxy(this._renderModuleList, this));
                }
            }
        },

        _loadRecommendedList: function (callback) {
            var handler = this;
            this._getEditorService().request('GetRecommendedModules', 'GET', {
            }, function (data) {
                handler._supportCategory = data.length > 0;
                if (!handler._supportCategory) {
                    handler._switchToSimpleMode();
                }

                handler._renderModuleList(data, 'listRecommended');
                recommendedList = data;
                if (callback && typeof callback == "function") {
                    callback.call(handler);
                }
            });
			logger.warn(`<<< Exiting _loadRecommendedList(callback=${callback})`, { "event.action": "_loadRecommendedList", "logrite.callback": callback });
        },

        _renderModuleList: function (data, list) {
			logger.error(`>>> Entering _renderModuleList(data=${data},list=${list})`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });
            if (!list) {
                list = 'listAll';
            }

			logger.error(`_renderModuleList(data=${data},list=${list}): list → ${list}`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });
            for (var i = 0; i < data.length; i++) {
                var moduleName = data[i].ModuleName;
                if (moduleAlias[moduleName]) {
                    var recommendExist = false;
                    for (var j = 0; j < recommendedList.length; j++) {
                        if (recommendedList[j].ModuleName === moduleName) {
                            recommendExist = true;
                            break;
                        }
                    }

                    if (!recommendExist) {
                        recommendedList.push(this._cloneItem(data[i]));
                    }
                }
            }

            var container = this._dialogLayout.find(".dnnModuleList .listContainer." + list + " ul");
			logger.error(`_renderModuleList(data=${data},list=${list}): container → ${container}`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });

            var searchWord = $("#AddModule_SearchModulesInput").val().toLowerCase();
            if (listMode === 'search' && searchWord) {
                for (var name in moduleAlias) {
                    if (moduleAlias.hasOwnProperty(name) && moduleAlias[name].toLowerCase().indexOf(searchWord) > -1) {
                        for (var i = 0; i < recommendedList.length; i++) {
                            if (recommendedList[i].ModuleName === name) {
                                var cloneItem = this._cloneItem(recommendedList[i]);
								logger.error(`_renderModuleList(data=${data},list=${list}): cloneItem → ${cloneItem}`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });
                                data.push(cloneItem);
                            }
                        }
                    }
                }
            }
            if (data.length > 0) {
                //render rearranged list
				for (var i = 0; i < data.length; i++) {
				    var itemData = data[i];
				    if (list === 'listAll' && listMode === 'category' && this._isRecommendModule(itemData)) {
                        continue;
                    }

					var $item = $(this._renderItem(itemData));
					container.append($item);
					this._injectModuleScript($item, itemData);
				}

				if (list === "listAll") {
				    if (data.length < this._pageSize) {
				        this._loadMore = false;
				    } else {
						logger.debug(`_renderModuleList(data=${data},list=${list}): this._loadMore → ${this._loadMore}`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });
				        this._startIndex += data.length;
				    }

				    if (!htmlModuleId) {
				        for (var i = 0; i < data.length; i++) {
				            if (data[i].ModuleName === htmlModuleName) {
				                htmlModuleId = data[i].ModuleID;
				            }
				        }
				    }
				    if (htmlModuleId > -1) {
				        var $htmlItem = this._dialogLayout.find('.dnnModuleItem[data-moduleid="' + htmlModuleId + '"]');
				        $htmlItem.parent().prepend($htmlItem);
				    }

                    this._dialogLayout.trigger('moduleloaded');

                    this._initScrollView();
                }
            }
            else {
                if (list == "listAll") {
                if (container.has("li").length == 0 && this._startIndex == 0) {
                    $(".jspVerticalBar").hide();
                    //scroll top to render no modules message
                    $(".jspDrag").css("top", "0px");
                    $(".jspPane").css("top", "0px");
                    this._loadMore = false;
                    container.after(this._getNoResultTemplate());
                }
}
            }
            $("#AddModule_SearchModulesInput").focus();
			logger.error(`_renderModuleList(data=${data},list=${list}): searchWord → ${searchWord}`, { "event.action": "_renderModuleList", "logrite.data": data, "logrite.list": list });
        },

        _isRecommendModule: function(item) {
			logger.error(`>>> Entering _isRecommendModule(item=${item})`, { "event.action": "_isRecommendModule", "logrite.item": item });
            for (var i = 0; i < recommendedList.length; i++) {
                if (recommendedList[i].ModuleName == item.ModuleName) {
                    return true;
                }
            }

			logger.warn(`<<< Exiting _isRecommendModule(item=${item})`, { "event.action": "_isRecommendModule", "logrite.item": item });
            return false;
        },

        _switchToSimpleMode: function () {
			logger.error('>>> Entering _switchToSimpleMode()', { "event.action": "_switchToSimpleMode" });
            this._dialogLayout.find(".dnnModuleList .listContainer.listRecommended").hide();
            this._dialogLayout.find("h2").hide();

            this._dialogLayout.find(".search-container .search-button").hide();
            this._dialogLayout.find(".search-container .clear-button").show();

            listMode = 'search';
			logger.warn('<<< Exiting _switchToSimpleMode()', { "event.action": "_switchToSimpleMode" });
        },

        _switchToCategoryMode: function () {
			logger.error('>>> Entering _switchToCategoryMode()', { "event.action": "_switchToCategoryMode" });
            this._dialogLayout.find(".dnnModuleList .listContainer.listRecommended").show();
            this._dialogLayout.find("h2").show();

            this._dialogLayout.find(".search-container .search-button").show();
            this._dialogLayout.find(".search-container .clear-button").hide();

            listMode = 'category';
			logger.warn('<<< Exiting _switchToCategoryMode()', { "event.action": "_switchToCategoryMode" });
        },

        _getItemTemplate: function () {
			logger.error('>>> Entering _getItemTemplate()', { "event.action": "_getItemTemplate" });
            return '<li class="dnnModuleItem" data-moduleid="[$ModuleID$]">' +
                '<span class="icon [$ModuleName|css]"><img src="[$ModuleImage$]" alt="[$ModuleName$]" /></span>' +
                '<span class="title {0}">[$ModuleName$]</span>' +
                '<span class="actions">' +
                    '<a href="#" class="button bookmarkModule" data-moduleid="[$ModuleID$]" aria-label="Bookmark"></a>' +
                '</span>' +
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

					logger.debug(`_renderItem(item=${item}): extraclass → ${extraclass}`, { "event.action": "_renderItem", "logrite.item": item });
                    if (renderItem.ModuleName === htmlModuleName) {
                        template = this._replaceAll(template, 'bookmarkModule', 'topModule');
                    } else if (renderItem.Bookmarked) {
                        template = this._replaceAll(template, 'bookmarkModule', 'bookmarkedModule');
                    }
                }
            }

			logger.error(`_renderItem(item=${item}): template → ${template}`, { "event.action": "_renderItem", "logrite.item": item });
			logger.warn(`<<< Exiting _renderItem(item=${item})`, { "event.action": "_renderItem", "logrite.item": item });
            return template;
        },

        _cloneItem: function(item) {
			logger.error(`>>> Entering _cloneItem(item=${item})`, { "event.action": "_cloneItem", "logrite.item": item });
            var renderItem = {};
            for (var i in item) {
                if (item.hasOwnProperty(i)) {
                    renderItem[i] = item[i];
					logger.warn(`<<< Exiting _cloneItem(item=${item})`, { "event.action": "_cloneItem", "logrite.item": item });
                }
            }
            return renderItem;
        },

        _injectModuleScript: function (item, data) {
			logger.error(`>>> Entering _injectModuleScript(item=${item},data=${data})`, { "event.action": "_injectModuleScript", "logrite.item": item, "logrite.data": data });
            if (data.ModuleID == -1) return;
	        var handler = this;
			this._getEditorService().request('LoadModuleScript', 'GET', {
				desktopModuleId: data.ModuleID
			}, function (returnData) {
				if (returnData.Script) {
					var model = handler._executeScript(returnData.Script);
					if (model && model.addModuleHandler && typeof model.addModuleHandler === "function") {
						model.addModuleHandler.call(window, data.ModuleID, data.ModuleName);
					}
				}

				if (returnData.StyleFile) {
					$(document.body).append('<link href="' + returnData.StyleFile + '" type="text/css" rel="stylesheet" />');
				}

				item.data('modulejs', true);
			}, function() {
				item.data('modulejs', true);
			});
			logger.warn(`<<< Exiting _injectModuleScript(item=${item},data=${data})`, { "event.action": "_injectModuleScript", "logrite.item": item, "logrite.data": data });
		},

        _initScrollView: function () {
			logger.error('>>> Entering _initScrollView()', { "event.action": "_initScrollView" });
            var container = this._dialogLayout.find(".dnnModuleList");
            if (container.data('jsp')) {
				logger.error(`_initScrollView(): container → ${container}`, { "event.action": "_initScrollView" });
                container.data('jsp').reinitialise();
            } else {
                container.jScrollPane();
                var $this = this;
                container.bind('jsp-scroll-y', function (e, y, atTop, atBottom) {
                    if (atBottom) {
                        if ($("#AddModule_SearchModulesInput").val()) {
                            $this._loadModuleList($("#AddModule_SearchModulesInput").val(), true);
                        } else {
                            $this._loadModuleList('', false);
                        }
                    }
                });
            }

            $(".jspVerticalBar").show();
			logger.warn('<<< Exiting _initScrollView()', { "event.action": "_initScrollView" });
        },

        // SEARCH
        _getSearchModules: function () {
			logger.error('>>> Entering _getSearchModules()', { "event.action": "_getSearchModules" });
            var $searchInput = $("#AddModule_SearchModulesInput");
            this._startIndex = 0;
            this._loadMore = true;
			logger.debug(`_getSearchModules(): this._startIndex → ${this._startIndex}`, { "event.action": "_getSearchModules" });
			logger.debug(`_getSearchModules(): this._loadMore → ${this._loadMore}`, { "event.action": "_getSearchModules" });
            this._loadModuleList($searchInput.val(), true);
        },

        _resetModuleSearch: function () {
			logger.error('>>> Entering _resetModuleSearch()', { "event.action": "_resetModuleSearch" });
            var $searchInput = $("#AddModule_SearchModulesInput");
            $searchInput.val('').focus();
            $('.listContainer.listAll ul li.dnnModuleItem:not([class~="dnnLayoutItem"])').remove();
            $('.listContainer.listAll div.dnnModuleDialog_ModuleListMessage').remove();
            this._loadMore = true;
            this._getService()._loading = false;
			logger.debug(`_resetModuleSearch(): this._loadMore → ${this._loadMore}`, { "event.action": "_resetModuleSearch" });
            this._startIndex = 0;
            this._loadModuleList('', true);
			logger.debug(`_resetModuleSearch(): this._startIndex → ${this._startIndex}`, { "event.action": "_resetModuleSearch" });
        },

        _searchKeyPress: function (e) {
			logger.error(`>>> Entering _searchKeyPress(e=${e})`, { "event.action": "_searchKeyPress", "logrite.e": e });
            if (e.keyCode == 13) {
                var val = $("#AddModule_SearchModulesInput").val();
				logger.error(`_searchKeyPress(e=${e}): val → ${val}`, { "event.action": "_searchKeyPress", "logrite.e": e });
                clearTimeout(this._searchTimeout);
                this._startIndex = 0;
                this._loadMore = true;
				logger.debug(`_searchKeyPress(e=${e}): this._startIndex → ${this._startIndex}`, { "event.action": "_searchKeyPress", "logrite.e": e });
                this._loadModuleList(val, true);
				logger.warn(`<<< Exiting _searchKeyPress(e=${e})`, { "event.action": "_searchKeyPress", "logrite.e": e });
			logger.debug(`_searchKeyPress(e=${e}): this._loadMore → ${this._loadMore}`, { "event.action": "_searchKeyPress", "logrite.e": e });
                return false;
            }
        },

        _searchKeyUp: function () {
			logger.error('>>> Entering _searchKeyUp()', { "event.action": "_searchKeyUp" });
            var val = $("#AddModule_SearchModulesInput").val();
            if (this._lastVal.length != val.length &&
                (val.length == 0 || val.length >= this._minInputLength)) {
                this._startIndex = 0;
                this._loadMore = true;
				logger.debug(`_searchKeyUp(): this._startIndex → ${this._startIndex}`, { "event.action": "_searchKeyUp" });
                this._loadModuleList(val, true);
            }
            this._lastVal = val;
			logger.error(`_searchKeyUp(): val → ${val}`, { "event.action": "_searchKeyUp" });
        },
        // END - SEARCH

        _attachEvents: function () {
			logger.error('>>> Entering _attachEvents()', { "event.action": "_attachEvents" });
            this._dialogLayout.on('click', '.dnnModuleItem', $.proxy(this._doAddModule, this));
            $(window).on('resize', '', $.proxy(this._layoutResized, this));
            this._dialogLayout.on('click', 'span.actions .button.bookmarkModule, span.actions .button.bookmarkedModule', $.proxy(this._toggleBookmark, this));
            this._dialogLayout.on('click', 'span.btn-close', $.proxy(this._closeButtonHandler, this));
            var $searchButton = $(".dnnModuleDialog .search-container .search-button");
            $searchButton.on('click', $.proxy(this._getSearchModules, this));

            var $clearButton = $(".dnnModuleDialog .search-container .clear-button");
            $clearButton.on('click', $.proxy(this._resetModuleSearch, this));

            var $searchInput = $(".dnnModuleDialog .search-container .search-input");
            $searchInput.on('mouseup', function () { return false; });
            $searchInput.on('keypress', $.proxy(this._searchKeyPress, this));
            $searchInput.on('keyup', $.proxy(this._searchKeyUp, this));

            $(window).on('beforeunload', $.proxy(this._windowBeforeUnload, this));
			logger.warn('<<< Exiting _attachEvents()', { "event.action": "_attachEvents" });
        },

        _closeButtonHandler: function() {
			logger.error('>>> Entering _closeButtonHandler()', { "event.action": "_closeButtonHandler" });
            this.getModuleManager().getHandler().click();
			logger.warn('<<< Exiting _closeButtonHandler()', { "event.action": "_closeButtonHandler" });
        },

        _handleKeyEvent: function (e) {
			logger.error(`>>> Entering _handleKeyEvent(e=${e})`, { "event.action": "_handleKeyEvent", "logrite.e": e });
            if (e.keyCode == 27) {
                this._moduleManager.getHandler().click();
				logger.warn(`<<< Exiting _handleKeyEvent(e=${e})`, { "event.action": "_handleKeyEvent", "logrite.e": e });
            }
        },

        _doAddModule: function (e) {
			logger.error(`>>> Entering _doAddModule(e=${e})`, { "event.action": "_doAddModule", "logrite.e": e });
        	var $item = $(e.target);
	        $item = $item.hasClass('dnnModuleItem') ? $item : $item.parents('.dnnModuleItem');
	        var handler = this;
			logger.warn(`<<< Exiting _doAddModule(e=${e})`, { "event.action": "_doAddModule", "logrite.e": e });
			if (!$item.data('modulejs')) {
				setTimeout(function() {
					handler._doAddModule(e);
				}, 500);

				return false;
			}

            var moduleId = $item.data("moduleid");
            if (!moduleId || moduleId == -1) {
                return false;
            }
            this.addModule(moduleId);

            return false;
        },

        _addModuleComplete: function (data, callback) {
			logger.error(`>>> Entering _addModuleComplete(data=${data},callback=${callback})`, { "event.action": "_addModuleComplete", "logrite.data": data, "logrite.callback": callback });
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._syncCompleteHandler);

            if (this._noFloat) {
                this._setCookie('noFloat', 'true');
                dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
            }

            this.setModuleId(data.TabModuleID);
            this.refreshPane('', 'module-' + data.TabModuleID, callback, true);
			logger.warn(`<<< Exiting _addModuleComplete(data=${data},callback=${callback})`, { "event.action": "_addModuleComplete", "logrite.data": data, "logrite.callback": callback });
        },

        // BOOKMARK
        _bookmarkUpdated: function (data) {
		logger.error(`>>> Entering _bookmarkUpdated(data=${data})`, { "event.action": "_bookmarkUpdated", "logrite.data": data });
        },

        _saveBookmark: function (item, removeBookmark) {
			logger.error(`>>> Entering _saveBookmark(item=${item},removeBookmark=${removeBookmark})`, { "event.action": "_saveBookmark", "logrite.item": item, "logrite.removeBookmark": removeBookmark });
            var params = {
                Title: 'module',
                Bookmark: this._bookmarkedModuleList.join(',')
            };
            this._getService().request('SaveBookmark', 'POST', params, $.proxy(this._bookmarkUpdated, this));
            if (removeBookmark) {
                $((item).parent()).parent().find("span.bookmarkholder a.button.bookmarkedModule").removeClass("bookmarkedModule").addClass("bookmarkModule").unbind("click");
                $((item).parent()).parent().find("span.actions a.button.bookmarkedModule").removeClass("bookmarkedModule").addClass("bookmarkModule").bind("click");
            }
            else {
                $((item).parent()).parent().find("span.bookmarkholder a.button.bookmarkModule").removeClass("bookmarkModule").addClass("bookmarkedModule").bind("click");
                $((item).parent()).parent().find("span.actions a.button.bookmarkModule").removeClass("bookmarkModule").addClass("bookmarkedModule").unbind("click");
            }

			logger.warn(`<<< Exiting _saveBookmark(item=${item},removeBookmark=${removeBookmark})`, { "event.action": "_saveBookmark", "logrite.item": item, "logrite.removeBookmark": removeBookmark });
            return false;
        },

        _toggleBookmark: function(e) {
			logger.error(`>>> Entering _toggleBookmark(e=${e})`, { "event.action": "_toggleBookmark", "logrite.e": e });
            if ($(e.target).hasClass('bookmarkedModule')) {
                this._removeBookmarkModule(e);
            } else {
                this._addBookmarkModule(e);
            }

            e.stopImmediatePropagation();
			logger.warn(`<<< Exiting _toggleBookmark(e=${e})`, { "event.action": "_toggleBookmark", "logrite.e": e });
            return false;
        },

        _addBookmarkModule: function (e) {
			logger.error(`>>> Entering _addBookmarkModule(e=${e})`, { "event.action": "_addBookmarkModule", "logrite.e": e });
            var moduleId = $(e.target).data("moduleid");
            if (this._bookmarkedModuleList.indexOf(moduleId) < 0) {
                this._bookmarkedModuleList.push(moduleId);
            }
            this._saveBookmark($(e.target), false);
			logger.error(`_addBookmarkModule(e=${e}): moduleId → ${moduleId}`, { "event.action": "_addBookmarkModule", "logrite.e": e });
        },

        _removeBookmarkModule: function (e) {
			logger.error(`>>> Entering _removeBookmarkModule(e=${e})`, { "event.action": "_removeBookmarkModule", "logrite.e": e });
            var moduleId = $(e.target).data("moduleid");
            if (moduleId == 84) {
				logger.error(`_removeBookmarkModule(e=${e}): moduleId → ${moduleId}`, { "event.action": "_removeBookmarkModule", "logrite.e": e });
                return;
            }
            var index = this._bookmarkedModuleList.indexOf(moduleId);
            if (index >= 0) {
                this._bookmarkedModuleList.splice(index, 1);
            }
            this._saveBookmark($(e.target), true);
			logger.warn(`<<< Exiting _removeBookmarkModule(e=${e})`, { "event.action": "_removeBookmarkModule", "logrite.e": e });
        },
        // END - BOOKMARK

        _cancelAddModule: function(e) {
			logger.error(`>>> Entering _cancelAddModule(e=${e})`, { "event.action": "_cancelAddModule", "logrite.e": e });
            var target = $(e.target);
            var moduleId = target.data('moduleid');
			logger.error(`_cancelAddModule(e=${e}): target → ${target}`, { "event.action": "_cancelAddModule", "logrite.e": e });
			logger.error(`_cancelAddModule(e=${e}): moduleId → ${moduleId}`, { "event.action": "_cancelAddModule", "logrite.e": e });

            var handler = this;
            var opts = {
                callbackTrue: function () {
					logger.error('>>> Entering callbackTrue()', { "event.action": "callbackTrue" });
                    //remove all action menus in current layout
                    handler._getEditorService().request('DeleteModule?moduleId=' + moduleId, 'POST', {
                    }, function (data) {
                        if (data.Status != 0) {
                            $.dnnAlert({ text: data.Message });
                        }

                        handler._resetPending();
                        handler.refreshPane('');
                    });
					logger.warn('<<< Exiting callbackTrue()', { "event.action": "callbackTrue" });
                },
                text: dnn.ContentEditorManagerResources.cancelConfirm,
                yesText: dnn.ContentEditorManagerResources.confirmYes,
                noText: dnn.ContentEditorManagerResources.confirmNo,
                title: dnn.ContentEditorManagerResources.confirmTitle
            };

            $.dnnConfirm(opts);
			logger.warn(`<<< Exiting _cancelAddModule(e=${e})`, { "event.action": "_cancelAddModule", "logrite.e": e });
            return false;
        },

        _processModuleForDrag: function (module) {
			logger.error(`>>> Entering _processModuleForDrag(module=${module})`, { "event.action": "_processModuleForDrag", "logrite.module": module });
            var handler = this;
            var moduleId = this.getModuleManager()._findModuleId(module);
			logger.error(`_processModuleForDrag(module=${module}): moduleId → ${moduleId}`, { "event.action": "_processModuleForDrag", "logrite.module": module });

            //hide related modules
            var relatedModules = [];
            if (!this._callFromExistingModule) {
                $('div.DnnModule').each(function() {
                    var id = handler.getModuleManager()._findModuleId($(this));
                    if (id > moduleId) {
                        $(this).hide();
                        $('#moduleActions-' + id).hide();
                        relatedModules.push(id);
                    }
                });
            }
            module.data('relatedModules', relatedModules);

            module.addClass('floating');
            //move module to current screen
            var left, top;
            var moduleWidth = module.outerWidth();
			logger.error(`_processModuleForDrag(module=${module}): moduleWidth → ${moduleWidth}`, { "event.action": "_processModuleForDrag", "logrite.module": module });
            var moduleHeight = module.outerHeight();
			logger.error(`_processModuleForDrag(module=${module}): moduleHeight → ${moduleHeight}`, { "event.action": "_processModuleForDrag", "logrite.module": module });
            left = $('#personaBar-iframe').outerWidth() / 2
                + ($(window).width() - moduleWidth) / 2;
            top = $(window).height() / 2 - moduleHeight;
            module.css({
                left: left,
                top: top
            });

            var initContentTimeout;
            var initFloatingContent = function () {
				logger.error('>>> Entering initFloatingContent()', { "event.action": "initFloatingContent" });
                var $dragHint = module.find('> div.dnnDragHint');

                if (initContentTimeout) {
                    clearTimeout(initContentTimeout);
                    initContentTimeout = null;
					logger.debug(`_processModuleForDrag(module=${module}): initContentTimeout → ${initContentTimeout}`, { "event.action": "_processModuleForDrag", "logrite.module": module });
                }
                if ($dragHint.length === 0) {
                    initContentTimeout = setTimeout(initFloatingContent, 50);
                    return;
                }

                $('div.dnnDragHint').off('mouseenter').addClass('dnnDragDisabled');
                $dragHint.removeClass('dnnDragDisabled');
                var $dragContent = $('<div />');
                $dragHint.append($dragContent);

                var moduleItem = handler._callFromExistingModule ?
                                    $('.dnnExistingModuleDialog li[data-moduleid="' + handler._addingDesktopModuleId + '"]')
                                    : handler.getElement().find('li[data-moduleid="' + handler._addingDesktopModuleId + '"]');

                if (moduleItem.length > 0) {
                    var $icon = moduleItem.find('span.icon');
                    var $cloneIcon = $icon.clone();
                    if ($icon.css('background-image') && $icon.css('background-image') != 'none') {
                        var backImg = $icon.css('background-image').match(/^(url\()?(['"]?)(.+?)\2\)?$/)[3];
						logger.error(`initFloatingContent(): backImg → ${backImg}`, { "event.action": "initFloatingContent" });
                        $cloneIcon.find('img').attr('src', backImg);
                    }
                    $dragContent.append($cloneIcon);
                    $dragContent.append(moduleItem.find('span.title').clone());
                } else {
                    $dragHint.append($dragContent);

                    var title = module.data('module-title');
					logger.error(`initFloatingContent(): title → ${title}`, { "event.action": "initFloatingContent" });
                    $('<span class="title" />').appendTo($dragContent).html(title);
                }

                $('<a name="' + moduleId + '" href="#" class="cancel-module" data-moduleid="' + moduleId + '" />').appendTo($dragContent).click($.proxy(handler._cancelAddModule, handler));
				logger.warn('<<< Exiting initFloatingContent()', { "event.action": "initFloatingContent" });
            };
            initFloatingContent();

            //show the drag tip if user add module first time.
            if (module.hasClass('dragtip')) {
                var dragTip = $('<div class="module-drag-tip"></div>');
				logger.error(`_processModuleForDrag(module=${module}): dragTip → ${dragTip}`, { "event.action": "_processModuleForDrag", "logrite.module": module });
                module.before(dragTip.css({opacity: 0}));
                dragTip.html(dnn.ContentEditorManagerResources.dragtip).css({
                    top: module.offset().top - $(document).scrollTop() - dragTip.outerHeight() - 28,
                    left: module.offset().left - $(document).scrollLeft() - 35 //the value calculate as: (dragtip.width - module.width) / 2
                });

                dragTip.animate({
                    top: '+=10',
                    opacity: 1
                }, 200);
            }

            module.addClass('drift').mouseover(function() {
                module.removeClass('drift');
            }).mouseout(function() {
                module.addClass('drift');
            });

            this._setPending('module-' + moduleId);
			logger.warn(`<<< Exiting _processModuleForDrag(module=${module})`, { "event.action": "_processModuleForDrag", "logrite.module": module });
        },

        _syncComplete: function (sender, args) {
			logger.error(`>>> Entering _syncComplete(sender=${sender},args=${args})`, { "event.action": "_syncComplete", "logrite.sender": sender, "logrite.args": args });
            if (args.get_error() != null) {
                args.set_errorHandled(true);
                location.reload();
                return;
            }

            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(this._syncCompleteHandler);
			logger.warn(`<<< Exiting _syncComplete(sender=${sender},args=${args})`, { "event.action": "_syncComplete", "logrite.sender": sender, "logrite.args": args });
            var handler = this;
            setTimeout(function() {
                handler._syncCompleteFunc();
            }, 25);
        },

        _syncCompleteFunc: function() {
			logger.error('>>> Entering _syncCompleteFunc()', { "event.action": "_syncCompleteFunc" });
            var moduleId = this.getModuleId();
            var handler = this;
			logger.error(`_syncCompleteFunc(): moduleId → ${moduleId}`, { "event.action": "_syncCompleteFunc" });
            var newModule = $('div.DnnModule-' + moduleId);

			logger.error(`_syncCompleteFunc(): newModule → ${newModule}`, { "event.action": "_syncCompleteFunc" });
            if (!this._noFloat) {
				logger.warn('<<< Exiting _syncCompleteFunc()', { "event.action": "_syncCompleteFunc" });
	            setTimeout(function() {
					handler._processModuleForDrag(newModule);
	            }, 150);
                
            } else {
                //animate the new module
                newModule.addClass('highlight');
                setTimeout(function() {
                    newModule.addClass('animate');
                }, 500);
                setTimeout(function() {
                    newModule.removeClass('highlight animate');
                }, 1000);

                // scroll to new added module
                var moduleTop = (newModule.offset().top - 80);
                if (moduleTop > 0) {
                    $('html, body').animate({scrollTop: moduleTop}, 500);
                }

                setTimeout(function () { //execute after inner script executed.
                    if (typeof window.dnnLoadScriptsInAjaxMode === "undefined" || window.dnnLoadScriptsInAjaxMode.length == 0) {
                        newModule.trigger('editmodule');
                    } else {
                        $(window).one('dnnScriptLoadComplete', function() {
                            newModule.trigger('editmodule');
                        });
                    }
                }, 300);

                this.setModuleId(-1);
            }

            this.getElement().trigger('addmodulecomplete', [newModule]);
            this._motionToNewModule(moduleId);

            setTimeout(function() {
                if (!handler._noFloat) {
                    $('#moduleActions-' + moduleId).addClass('floating');
                }

                handler._noFloat = false;
                handler._removeCookie('noFloat');

				if (dnn
	                && dnn.ContentEditorManager
	                && typeof dnn.ContentEditorManager.catchSortEvents === "function") {
	                dnn.ContentEditorManager.catchSortEvents();
                }
            }, 250);

            this._working = false;
        },

        _motionToNewModule: function(moduleId) {
			logger.error(`>>> Entering _motionToNewModule(moduleId=${moduleId})`, { "event.action": "_motionToNewModule", "logrite.moduleId": moduleId });
            var $listItem = this._callFromExistingModule ?
                                $('.dnnExistingModuleDialog li[data-moduleid="' + this._addingDesktopModuleId + '"]')
                                : this.getElement().find('li[data-moduleid="' + this._addingDesktopModuleId + '"]');
            if ($listItem.length === 0) {
                $listItem = this.getModuleManager().getHandler();
            }

            var $newModule = $('div.DnnModule-' + moduleId);
            var $motion = $('<div class="module-motion" />').hide().css({
                width: $listItem.width(),
                height: $listItem.height(),
                left: $listItem.offset().left,
                top: $listItem.offset().top
            }).appendTo($(document.body));

            this.close(function() {
                $motion.show().animate({
                    width: $newModule.width(),
                    height: $newModule.height(),
                    left: $newModule.offset().left,
                    top: $newModule.offset().top
                }, 'fast', function() {
                    $motion.remove();
                });
            });
        },

        _layoutResized: function (e) {
			logger.error(`>>> Entering _layoutResized(e=${e})`, { "event.action": "_layoutResized", "logrite.e": e });
            if (this.isOpen()) {
                this._calcPosition(this._moduleManager.getHandler());
				logger.warn(`<<< Exiting _layoutResized(e=${e})`, { "event.action": "_layoutResized", "logrite.e": e });
            }
        },

        _replaceAll: function (input, find, replace) {
			logger.error(`>>> Entering _replaceAll(input=${input},find=${find},replace=${replace})`, { "event.action": "_replaceAll", "logrite.input": input, "logrite.find": find, "logrite.replace": replace });
            var str = input;
            str += "";
			logger.error(`_replaceAll(input=${input},find=${find},replace=${replace}): str → ${str}`, { "event.action": "_replaceAll", "logrite.input": input, "logrite.find": find, "logrite.replace": replace });
            var indexOfMatch = str.indexOf(find);

            while (indexOfMatch != -1) {
                str = str.replace(find, replace);
                indexOfMatch = str.indexOf(find);
            }

			logger.error(`_replaceAll(input=${input},find=${find},replace=${replace}): indexOfMatch → ${indexOfMatch}`, { "event.action": "_replaceAll", "logrite.input": input, "logrite.find": find, "logrite.replace": replace });
			logger.warn(`<<< Exiting _replaceAll(input=${input},find=${find},replace=${replace})`, { "event.action": "_replaceAll", "logrite.input": input, "logrite.find": find, "logrite.replace": replace });
            return (str);
        },

        // SERVICES HELPERS
        _getService: function () {
            if (!this._serviceController) {
                this._serviceController = new dnn.dnnModuleService({
                    service: 'internalservices',
                    controller: 'controlbar'
                });
            }

            return this._serviceController;
        },

        _getItemListService: function () {
			logger.error('>>> Entering _getItemListService()', { "event.action": "_getItemListService" });
            if (!this._itemListServiceController) {
                this._itemListServiceController = new dnn.dnnModuleService({
                    service: 'internalservices',
                    controller: 'itemlistservice'
                });
            }

			logger.warn('<<< Exiting _getItemListService()', { "event.action": "_getItemListService" });
            return this._itemListServiceController;
        },

        _getEditorService: function () {
			logger.error('>>> Entering _getEditorService()', { "event.action": "_getEditorService" });
            if (!this._editorServiceController) {
                this._editorServiceController = new dnn.dnnModuleService({
                    service: 'editBar/common',
                    controller: 'ContentEditor'
                });
            }

			logger.warn('<<< Exiting _getEditorService()', { "event.action": "_getEditorService" });
            return this._editorServiceController;
        },
        // END - SERVICES HELPERS

        // MANAGE PENDING STATE
        _setPending: function(data) {
			logger.error(`>>> Entering _setPending(data=${data})`, { "event.action": "_setPending", "logrite.data": data });
            this._pending = true;
            $('.actionMenu').addClass('floating');
            this._setCookie('cem_pending', data);
			logger.debug(`_setPending(data=${data}): this._pending → ${this._pending}`, { "event.action": "_setPending", "logrite.data": data });
        },

        _resetPending: function() {
			logger.error('>>> Entering _resetPending()', { "event.action": "_resetPending" });
            this._pending = false;
            $('.actionMenu').removeClass('floating');
            this._removeCookie('cem_pending');
			logger.debug(`_resetPending(): this._pending → ${this._pending}`, { "event.action": "_resetPending" });
        },

        _windowBeforeUnload: function() {
			logger.error('>>> Entering _windowBeforeUnload()', { "event.action": "_windowBeforeUnload" });
            if (this._pending) {
                return dnn.ContentEditorManagerResources.pendingsave;
            }

			logger.warn('<<< Exiting _windowBeforeUnload()', { "event.action": "_windowBeforeUnload" });
            return;
        },
        // END - MANAGE PENDING STATE

        // COOKIE HELPERS
        _getCookie: function(name) {
			logger.error(`>>> Entering _getCookie(name=${name})`, { "event.action": "_getCookie", "logrite.name": name });
            if (dnn.dom) {
                return dnn.dom.getCookie(name);
            } else {
				logger.warn(`<<< Exiting _getCookie(name=${name})`, { "event.action": "_getCookie", "logrite.name": name });
                return '';
            }
        },

        _setCookie: function(name, value) {
			logger.error(`>>> Entering _setCookie(name=${name},value=${value})`, { "event.action": "_setCookie", "logrite.name": name, "logrite.value": value });
            if (dnn.dom) {
                dnn.dom.setCookie(name, value, 0, this.getSiteRoot());
				logger.warn(`<<< Exiting _setCookie(name=${name},value=${value})`, { "event.action": "_setCookie", "logrite.name": name, "logrite.value": value });
            }
        },

        _removeCookie: function(name) {
			logger.error(`>>> Entering _removeCookie(name=${name})`, { "event.action": "_removeCookie", "logrite.name": name });
            dnn.dom.setCookie(name, '', -1, this.getSiteRoot());
			logger.warn(`<<< Exiting _removeCookie(name=${name})`, { "event.action": "_removeCookie", "logrite.name": name });
        }
        // END - COOKIE HELPERS
    };

    dnnModuleDialog.defaultOptions = {};

    ///dnnModuleDialog Plugin END
}(jQuery));