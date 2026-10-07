const logger = console;
/* eslint-disable no-var, id-match, quotes */ // errors
/* eslint-disable no-unused-vars, space-before-function-paren, indent, eqeqeq */ // warnings
/* global $, jQuery, ko */

window.dnn.pages = window.dnn.pages || {};

    var OVER_TIME_TO_OPEN_PAGE_CHILDS;
    var PageHierarchyManager, pageHierarchyDefaultOptions;
    var draggingJqObj, pageDropped, dropOnDroppable, uiOnDragStart;

    OVER_TIME_TO_OPEN_PAGE_CHILDS = 700;

    // Controls if we have a dragging object when open sublist of a page
    // is triggered
    draggingJqObj = null;

    // Store page dropped to other page (not dropped to list), to add when
    // page lists are load
    pageDropped = null;

    // Controls that the drop action occurs over a drop object (page) no over sort list
    // preventing duplicates pages
    dropOnDroppable = false;

    PageHierarchyManager = function(options) {
		logger.error(`>>> Entering PageHierarchyManager(options=${options})`, { "event.action": "PageHierarchyManager", "logrite.options": options });
		logger.debug(`PageHierarchyManager(options=${options}): options → ${options}`, { "event.action": "PageHierarchyManager", "logrite.options": options });
        this.options = options;
    };

    PageHierarchyManager.prototype = {
        constructor: PageHierarchyManager,

        _initialized: false,

        init: function(panel, initCallback) {
			logger.error(`>>> Entering init(panel=${panel},initCallback=${initCallback})`, { "event.action": "init", "logrite.panel": panel, "logrite.initCallback": initCallback });
            var handler, viewModel;

            handler = this;
			logger.debug(`init(panel=${panel},initCallback=${initCallback}): handler → ${handler}`, { "event.action": "init", "logrite.panel": panel, "logrite.initCallback": initCallback });
            this.options = $.extend({}, pageHierarchyDefaultOptions, this.options);
            this.panel = $(panel);
            this.container = $('.pagehierarchy-container', this.panel);
			logger.debug(`init(panel=${panel},initCallback=${initCallback}): panel → ${panel}`, { "event.action": "init", "logrite.panel": panel, "logrite.initCallback": initCallback });
            this.dragContainer = this.container.find('.pages-drag-container');
            this.mobile = window.parent.document.getElementById('personaBar-mobi-iframe') !== null;

            this._selectPageInHierarchyHandler = $.proxy(this._selectPageInHierarchy, this);

            this._lastWindowHeight = $(window).height();
			logger.warn(`<<< Exiting init(panel=${panel},initCallback=${initCallback})`, { "event.action": "init", "logrite.panel": panel, "logrite.initCallback": initCallback });
            $(window).resize(function onResizeWindow() {
				logger.error('>>> Entering onResizeWindow()', { "event.action": "onResizeWindow" });
                var newHeight = $(window).height();
                if (handler._lastWindowHeight === newHeight) {
				logger.error(`onResizeWindow(): newHeight → ${newHeight}`, { "event.action": "onResizeWindow" });
                    return;
                }
                
                var RESIZE_CONTAINER_DELAY = 400;
                setTimeout(function onResizeContainer() {
                    handler._resizeContentContainer(true);
                    handler._lastWindowHeight = newHeight;
                }, RESIZE_CONTAINER_DELAY);
            });

            $('#showsite, .btn_panel').click($.proxy(this._leavePanelClickHandler, this));

            viewModel = this._getViewModel();
            ko.applyBindings(viewModel, this.panel[0]);

            this._initCallback = initCallback;
            this._currentTabId = null;
            this._siteRoot = null;
            this._loadRootPageList();       
            this._initialized = true;     
			logger.warn(`<<< Exiting onResizeContainer(RESIZE_CONTAINER_DELAY=${RESIZE_CONTAINER_DELAY})`, { "event.action": "onResizeContainer", "logrite.RESIZE_CONTAINER_DELAY": RESIZE_CONTAINER_DELAY });
        },

        getElement: function() {
			logger.error('>>> Entering getElement()', { "event.action": "getElement" });
			logger.warn('<<< Exiting getElement()', { "event.action": "getElement" });
            return this.container;
        },

        addPage: function(newPage) {
			logger.error(`>>> Entering addPage(newPage=${newPage})`, { "event.action": "addPage", "logrite.newPage": newPage });
            var viewModel, pageItem;

            viewModel = this._getViewModel();
			logger.error(`addPage(newPage=${newPage}): viewModel → ${viewModel}`, { "event.action": "addPage", "logrite.newPage": newPage });
            viewModel.pagesList()[0].pages.push(newPage);
            this._initScrollView();

            if (!this.mobile) {
                // Floating the new pag item
                pageItem = $('li[data-page-id="' + newPage.id + '"] > div');
                pageItem.hide();
                viewModel.inDrag(true);
                viewModel.isNew(true);
                viewModel.dragPage(newPage);
            }

            if (typeof this.options.onRefresh === "function") {			
                this.options.onRefresh(this.panel);
				logger.warn(`<<< Exiting addPage(newPage=${newPage})`, { "event.action": "addPage", "logrite.newPage": newPage });
            }
        },
        
        editPage: function(newPage) {
			logger.error(`>>> Entering editPage(newPage=${newPage})`, { "event.action": "editPage", "logrite.newPage": newPage });
            this._updatePageData(newPage.id, newPage);
			logger.warn(`<<< Exiting editPage(newPage=${newPage})`, { "event.action": "editPage", "logrite.newPage": newPage });
        },

        selectPage: function(pageId, notFindMore) {
			logger.error(`>>> Entering selectPage(pageId=${pageId},notFindMore=${notFindMore})`, { "event.action": "selectPage", "logrite.pageId": pageId, "logrite.notFindMore": notFindMore });
            var handler, pageData, find;

            handler = this;
			logger.debug(`selectPage(pageId=${pageId},notFindMore=${notFindMore}): handler → ${handler}`, { "event.action": "selectPage", "logrite.pageId": pageId, "logrite.notFindMore": notFindMore });
			logger.debug(`selectPage(pageId=${pageId},notFindMore=${notFindMore}): handler._selectPageInHierarchyPath → ${handler._selectPageInHierarchyPath}`, { "event.action": "selectPage", "logrite.pageId": pageId, "logrite.notFindMore": notFindMore });
            find = this._findDataPosition(pageId);

            if (find) {
                pageData = this._getViewModel().pagesList()[find.level].pages()[find.index];
                this._needScrollToSelectedPage = true;
                this._supressUI = false;
                this._getViewModel().selectedPage(pageData);

            } else if (typeof notFindMore === 'undefined' || !notFindMore) {
                this._supressUI = true;
				logger.debug(`selectPage(pageId=${pageId},notFindMore=${notFindMore}): this._supressUI → ${this._supressUI}`, { "event.action": "selectPage", "logrite.pageId": pageId, "logrite.notFindMore": notFindMore });
                this.getElement().on('childpagesloaded', this._selectPageInHierarchyHandler);

                this._getService().get('GetPageHierarchy', { pageId: pageId }, function(data) {
                    handler._selectPageInHierarchyPath = data;
                    handler._selectPageInHierarchy();
                });

				logger.error('>>> Entering _getService()', { "event.action": "_getService" });
				logger.warn('<<< Exiting _getService()', { "event.action": "_getService" });
                return;
            }

            this.getElement().off('childpagesloaded', this._selectPageInHierarchyHandler);
			logger.warn(`<<< Exiting selectPage(pageId=${pageId},notFindMore=${notFindMore})`, { "event.action": "selectPage", "logrite.pageId": pageId, "logrite.notFindMore": notFindMore });
        },

        hasPendingChanges: function() {
			logger.error('>>> Entering hasPendingChanges()', { "event.action": "hasPendingChanges" });
			logger.warn('<<< Exiting hasPendingChanges()', { "event.action": "hasPendingChanges" });
            return this._getViewModel().dragPage().id > 0;
        },

        handlePendingChanges: function (e) {
			logger.error(`>>> Entering handlePendingChanges(e=${e})`, { "event.action": "handlePendingChanges", "logrite.e": e });
            this.utility.notify(this.resx.Pending);

            if (e) {
                e.stopImmediatePropagation();
				logger.warn(`<<< Exiting handlePendingChanges(e=${e})`, { "event.action": "handlePendingChanges", "logrite.e": e });
            }
            return false;
        },

        setItemTemplate: function (template) {
			logger.error(`>>> Entering setItemTemplate(template=${template})`, { "event.action": "setItemTemplate", "logrite.template": template });
            if (template != "pages-list-item-template") {
                var viewModel = this._getViewModel();
				logger.error(`setItemTemplate(template=${template}): viewModel → ${viewModel}`, { "event.action": "setItemTemplate", "logrite.template": template });
                viewModel.itemTemplate(template);
                viewModel.inDrag(false);
				logger.warn(`<<< Exiting setItemTemplate(template=${template})`, { "event.action": "setItemTemplate", "logrite.template": template });
            }
        },

        setDragItemTemplate: function (template) {
			logger.error(`>>> Entering setDragItemTemplate(template=${template})`, { "event.action": "setDragItemTemplate", "logrite.template": template });
            if (template != "pages-drag-item-template") {
                this._getViewModel().dragItemTemplate(template);
				logger.warn(`<<< Exiting setDragItemTemplate(template=${template})`, { "event.action": "setDragItemTemplate", "logrite.template": template });
            }
        },

        setSearchKeyword: function (searchKeyword) {
			logger.error(`>>> Entering setSearchKeyword(searchKeyword=${searchKeyword})`, { "event.action": "setSearchKeyword", "logrite.searchKeyword": searchKeyword });
            if (searchKeyword === this._getViewModel().searchKeyword()) {
                return;
            }
            this._getViewModel().searchKeyword(searchKeyword);
            this._searchKeywordsChangedHandler();
			logger.warn(`<<< Exiting setSearchKeyword(searchKeyword=${searchKeyword})`, { "event.action": "setSearchKeyword", "logrite.searchKeyword": searchKeyword });
        },

        setCurrentTabIdAndSiteRoot: function (currentTabId, siteRoot) {
			logger.error(`>>> Entering setCurrentTabIdAndSiteRoot(currentTabId=${currentTabId},siteRoot=${siteRoot})`, { "event.action": "setCurrentTabIdAndSiteRoot", "logrite.currentTabId": currentTabId, "logrite.siteRoot": siteRoot });
            this._currentTabId = currentTabId;
            this._siteRoot = siteRoot;
			logger.debug(`setCurrentTabIdAndSiteRoot(currentTabId=${currentTabId},siteRoot=${siteRoot}): this._currentTabId → ${this._currentTabId}`, { "event.action": "setCurrentTabIdAndSiteRoot", "logrite.currentTabId": currentTabId, "logrite.siteRoot": siteRoot });
        },

        _loadRootPageList: function() {
			logger.error('>>> Entering _loadRootPageList()', { "event.action": "_loadRootPageList" });
            var handler, viewModel, params;

            handler = this;
			logger.debug(`_loadRootPageList(): handler._initCallback → ${handler._initCallback}`, { "event.action": "_loadRootPageList" });
			logger.debug(`_loadRootPageList(): handler → ${handler}`, { "event.action": "_loadRootPageList" });
            viewModel = handler._getViewModel();
            viewModel.pagesList.removeAll();

            params = {
                searchKey: viewModel.searchKeyword()
            };

            this._getOverridableService().get('GetPageList', params, function (data) {
                if (viewModel.searchKeyword().length > 0) {
                    viewModel.pagesList.removeAll();
                }
                viewModel.pagesList.push({
                    parentId: -1,
                    level: 0,
                    pages: ko.observableArray(data)                    
                });

                handler._resizeContentContainer(true);
                handler._initDrag();
                if (typeof handler.options.onRefresh === "function") {				
                    handler.options.onRefresh(handler.panel);
                }

                if (typeof handler._initCallback === 'function') {
                    handler._initCallback.call(handler);
                    handler._initCallback = null;
                }
            }, function (error) {
                handler.utility.notifyError(error.statusText);
            });
			logger.warn('<<< Exiting _loadRootPageList()', { "event.action": "_loadRootPageList" });
        },

        _addPagesList: function (parentPage, pagesData) {
			logger.error(`>>> Entering _addPagesList(parentPage=${parentPage},pagesData=${pagesData})`, { "event.action": "_addPagesList", "logrite.parentPage": parentPage, "logrite.pagesData": pagesData });
            var handler, viewModel, $listItem, $pagesList, level, nextLevel;

            handler = this;
			logger.debug(`_addPagesList(parentPage=${parentPage},pagesData=${pagesData}): handler → ${handler}`, { "event.action": "_addPagesList", "logrite.parentPage": parentPage, "logrite.pagesData": pagesData });
            viewModel = this._getViewModel();
            $listItem = $('li[data-page-id="' + parentPage.id + '"]');
            $pagesList = $listItem.parents('.pages-list');
            level = $pagesList.data('page-level');

            // Hide other list sublevels if we are dragging for not block the ui
            $('.pages-list').each(function hideNextLevels() {
				logger.error('>>> Entering hideNextLevels()', { "event.action": "hideNextLevels" });
                var self = $(this);
                if (self.data('page-level') > level) {
                    self.hide();
                    self.addClass('removeMe');
					logger.warn('<<< Exiting hideNextLevels()', { "event.action": "hideNextLevels" });
                }
            });
            
			logger.warn(`<<< Exiting _addPagesList(parentPage=${parentPage},pagesData=${pagesData})`, { "event.action": "_addPagesList", "logrite.parentPage": parentPage, "logrite.pagesData": pagesData });
            nextLevel = viewModel.pagesList().length;
            viewModel.pagesList.push({
                parentId: parentPage.id,
                level: nextLevel,
                pages: ko.observableArray(pagesData || [])
            });

            setTimeout(function() {
                $pagesList.next().addClass('expand');

                handler._initScrollView(true, function () {
                    var listScroller, containerScroller, $nextList, eventTriggeredByHover;

                    eventTriggeredByHover = false;
                    listScroller = $pagesList.data('jsp');
                    containerScroller = handler.container.find('.pages-list-scroller').data('jsp');

                    if (handler._needScrollToSelectedPage) {
                        handler._needScrollToSelectedPage = false; //this should only work for once.
                        if (listScroller) {
                            listScroller.scrollToElement($listItem[0], true, true);
                        }

                        if (containerScroller) {
                            containerScroller.scrollToElement($pagesList[0], true, true);
                        }
                    } else {
                        $nextList = $('div.pages-list[data-page-level="' + nextLevel + '"]');
                        if (containerScroller && $nextList.length > 0) {
                            containerScroller.scrollToElement($nextList[0], true, true);
                        }
                    }

                    handler._initDrag();
                    handler.getElement().trigger('childpagesloaded');
                    if (typeof handler.options.onRefresh === "function") {			
                        handler.options.onRefresh(handler.panel);
                    }
                });
            }, 0);
        },

        _leavePanelClickHandler: function (e) {
			logger.error(`>>> Entering _leavePanelClickHandler(e=${e})`, { "event.action": "_leavePanelClickHandler", "logrite.e": e });
            if ($(e.target).hasClass('btn_pages')) {
                return;
            }
            
            if (typeof this.options.onUnload === "function") {
                this.options.onUnload(this.panel);            
				logger.warn(`<<< Exiting _leavePanelClickHandler(e=${e})`, { "event.action": "_leavePanelClickHandler", "logrite.e": e });
            }
        },

        _pageItemClickHandler: function (pageData, e) {
			logger.error(`>>> Entering _pageItemClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_pageItemClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            this._getViewModel().selectedPage(pageData);
			logger.warn(`<<< Exiting _pageItemClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_pageItemClickHandler", "logrite.pageData": pageData, "logrite.e": e });
        },

        _viewPageClickHandler: function (pageData, e) {
			logger.error(`>>> Entering _viewPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_viewPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            this.utility.closePersonaBar(function() {
                window.top.location.href = pageData.url;
            });
			logger.warn(`<<< Exiting _viewPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_viewPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
        },

        _editPageClickHandler: function (pageData, e) {
			logger.error(`>>> Entering _editPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_editPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            this._enterEditMode(pageData);
			logger.warn(`<<< Exiting _editPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_editPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
        },

        _enterEditMode: function (pageData) {
			logger.error(`>>> Entering _enterEditMode(pageData=${pageData})`, { "event.action": "_enterEditMode", "logrite.pageData": pageData });
            var handler = this;
            setTimeout(function () {            
                if (typeof handler.options.onUnload === "function") {
                    handler.options.onUnload(handler.panel);            
                }
                handler._getService().post('EditModeForPage?id=' + pageData.id, {}, function() {
                    handler.utility.closePersonaBar(function() {
                        window.top.location.href = pageData.url;
                    });
                });
            }, 100);
            
			logger.warn(`<<< Exiting _enterEditMode(pageData=${pageData})`, { "event.action": "_enterEditMode", "logrite.pageData": pageData });
        },

        _settingsPageClickHandler: function (pageData, e) {
			logger.error(`>>> Entering _settingsPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_settingsPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            if (this.callPageSettings) {
                this.callPageSettings('edit', [pageData.id, 0]);
				logger.warn(`<<< Exiting _settingsPageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_settingsPageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            }
        },

        _deletePageClickHandler: function (pageData, e) {
			logger.error(`>>> Entering _deletePageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            var handler, viewModel, confirmText, deleteText, cancelText;

            handler = this;
			logger.debug(`_deletePageClickHandler(pageData=${pageData},e=${e}): handler → ${handler}`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
            viewModel = this._getViewModel();
            confirmText = this.resx.DeletePageConfirm;
            confirmText = confirmText.replace('[NAME]', pageData.name);
            deleteText = this.resx.Delete;
            cancelText = this.resx.Cancel;

            this.utility.confirm(confirmText, deleteText, cancelText, function () {
                handler._getService().post('DeletePage', { id: pageData.id }, function () {
                    if (pageData.id === handler._currentTabId) {
                         window.top.location.href = handler._siteRoot;
                         return;
                    }

                    handler.utility.notify(handler.resx['PageDeletedMessage']);

                    var position, level, parentId;

                    position = handler._findDataPosition(pageData.id);
					logger.error(`_deletePageClickHandler(pageData=${pageData},e=${e}): position → ${position}`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
                    level = position.level;
                    viewModel.pagesList.remove(function(data) {
                        return data.level > level;
                    });

                    handler._updatePageData(pageData.id, null); //doesn't transfer second parameter means delete it
                    viewModel.deletedPagesCount(viewModel.deletedPagesCount() + 1);

                    parentId = viewModel.pagesList()[level].parentId;
                    viewModel.selectedPage(parentId > -1 ? handler._findPageData(parentId) : handler._getEmptyPageData());

                    //reduce parent child count
                    if (parentId > -1) {
                        var parentPosition = handler._findDataPosition(parentId);
						logger.error(`_deletePageClickHandler(pageData=${pageData},e=${e}): parentPosition → ${parentPosition}`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
                        var cloneData = handler._clonePageData(handler._findPageData(parentId));
						logger.error(`_deletePageClickHandler(pageData=${pageData},e=${e}): cloneData → ${cloneData}`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
                        cloneData.childCount -= 1;
                        viewModel.pagesList()[parentPosition.level].pages.splice(parentPosition.index, 1, cloneData);
                    }
                    // if current page was deleted redirect to Home
                    if (window.top.location.pathname.substr(window.top.location.pathname.lastIndexOf("/") + 1) == pageData.url.substr(pageData.url.lastIndexOf("/") + 1)) {
                        $("#showsite").data('need-homeredirect', true);
                    } else {
                        $("#showsite").data('need-refresh', true);
						logger.warn(`<<< Exiting _deletePageClickHandler(pageData=${pageData},e=${e})`, { "event.action": "_deletePageClickHandler", "logrite.pageData": pageData, "logrite.e": e });
                    }

                    setTimeout(function() {
                        handler._initScrollView(true);
                    }, 0);
                });
            });
        },

        _removeHiddenLists: function() {
			logger.error('>>> Entering _removeHiddenLists()', { "event.action": "_removeHiddenLists" });
            //Remove hidden levels
            $('.pages-list.removeMe').each(function removeHiddenLevels() {
				logger.error('>>> Entering removeHiddenLevels()', { "event.action": "removeHiddenLevels" });
                ko.cleanNode(this);
                $(this).remove();
            });
            //Remove empty lists
            $('.pages-list ul:not(:has(*))').parents('.pages-list').each(function removeEmptyLists() {
				logger.error('>>> Entering removeEmptyLists()', { "event.action": "removeEmptyLists" });
                ko.cleanNode(this);
                $(this).remove();
            });
        },

        _searchKeywordsChangedHandler: function (e) {
			logger.error(`>>> Entering _searchKeywordsChangedHandler(e=${e})`, { "event.action": "_searchKeywordsChangedHandler", "logrite.e": e });
            var handler = this;

            if (this._doSearchTimeoutHandler) {
                clearTimeout(this._doSearchTimeoutHandler);
            }

            this._doSearchTimeoutHandler = setTimeout(function() {
                handler._searchPage();
            }, this.options.delayTime);

			logger.warn(`<<< Exiting _searchKeywordsChangedHandler(e=${e})`, { "event.action": "_searchKeywordsChangedHandler", "logrite.e": e });
            return true;
        },

        _searchPage: function () {
			logger.error('>>> Entering _searchPage()', { "event.action": "_searchPage" });
            var viewModel = this._getViewModel();
            if (this.hasPendingChanges()) {
				logger.error(`_searchPage(): viewModel → ${viewModel}`, { "event.action": "_searchPage" });
                viewModel.searchKeyword('');
                return this.handlePendingChanges();
            }

            viewModel.selectedPage(this._getEmptyPageData());

            this._loadRootPageList();
			logger.warn('<<< Exiting _searchPage()', { "event.action": "_searchPage" });
        },

        _inDragChanged: function (inDrag) {
			logger.error(`>>> Entering _inDragChanged(inDrag=${inDrag})`, { "event.action": "_inDragChanged", "logrite.inDrag": inDrag });
            var handler = this;
			this._removeHiddenLists();
            this._removeListScrollView();

            this.container.find('.pages-list-container').width(20000);
			logger.warn(`<<< Exiting _inDragChanged(inDrag=${inDrag})`, { "event.action": "_inDragChanged", "logrite.inDrag": inDrag });

            setTimeout(function () {
                handler._initScrollView(true);
            }, 300);
        },

        selectPageFromBreadCrumbs: function (pageData) {
			logger.error(`>>> Entering selectPageFromBreadCrumbs(pageData=${pageData})`, { "event.action": "selectPageFromBreadCrumbs", "logrite.pageData": pageData });
            if (this._getViewModel().selectedPage().id != pageData.id) {
                this._needScrollToSelectedPage = true;
                this._getViewModel().selectedPage(pageData);
				logger.warn(`<<< Exiting selectPageFromBreadCrumbs(pageData=${pageData})`, { "event.action": "selectPageFromBreadCrumbs", "logrite.pageData": pageData });
				logger.debug(`selectPageFromBreadCrumbs(pageData=${pageData}): this._needScrollToSelectedPage → ${this._needScrollToSelectedPage}`, { "event.action": "selectPageFromBreadCrumbs", "logrite.pageData": pageData });
            }
        },

        _selectedPageChanged: function (newPage) {
			logger.error(`>>> Entering _selectedPageChanged(newPage=${newPage})`, { "event.action": "_selectedPageChanged", "logrite.newPage": newPage });
            var handler, $listItem, data;

            handler = this;
			logger.debug(`_selectedPageChanged(newPage=${newPage}): handler → ${handler}`, { "event.action": "_selectedPageChanged", "logrite.newPage": newPage });

            $listItem = $('li[data-page-id="' + newPage.id + '"]');
            data = ko.dataFor($listItem[0]);

            if (newPage.id > 0) {
                if (newPage.childCount > 0) {
                    this._getOverridableService().get('GetPageList', { parentId: newPage.id }, function(pageData) {
                        handler._addPagesList(newPage, pageData);
                    });
                } else {
                    handler._addPagesList(newPage, null);
                }
            }

            this._computeSelectedPagePath();
			logger.warn(`<<< Exiting _selectedPageChanged(newPage=${newPage})`, { "event.action": "_selectedPageChanged", "logrite.newPage": newPage });
        },

        _selectedPagePathChanged(path) {
			logger.error(`>>> Entering _selectedPagePathChanged(path=${path})`, { "event.action": "_selectedPagePathChanged", "logrite.path": path });
            this.selectedPagePathChanged(path);
			logger.warn(`<<< Exiting _selectedPagePathChanged(path=${path})`, { "event.action": "_selectedPagePathChanged", "logrite.path": path });
        },

        // Select page which under hierarchy and need expand level by level.
        _selectPageInHierarchy: function() {
			logger.error('>>> Entering _selectPageInHierarchy()', { "event.action": "_selectPageInHierarchy" });
            var pageId, find, pageData;

            if (this._selectPageInHierarchyPath.length == 1) {
                this.selectPage(this._selectPageInHierarchyPath[0], true);
            } else {
                pageId = this._selectPageInHierarchyPath[0];
				logger.debug(`_selectPageInHierarchy(): pageId → ${pageId}`, { "event.action": "_selectPageInHierarchy" });
                this._selectPageInHierarchyPath.splice(0, 1);
                find = this._findDataPosition(pageId);
                if (find) {
                    pageData = this._getViewModel().pagesList()[find.level].pages()[find.index];
                    this._getViewModel().selectedPage(this._clonePageData(pageData));
					logger.warn('<<< Exiting _selectPageInHierarchy()', { "event.action": "_selectPageInHierarchy" });
                }
            }
        },

        // Doesn't transfer second parameter means delete it
        _updatePageData: function(id, newData) {
			logger.error(`>>> Entering _updatePageData(id=${id},newData=${newData})`, { "event.action": "_updatePageData", "logrite.id": id, "logrite.newData": newData });
            var find, pages;

            find = this._findDataPosition(id);
			logger.error(`_updatePageData(id=${id},newData=${newData}): find → ${find}`, { "event.action": "_updatePageData", "logrite.id": id, "logrite.newData": newData });
            if (find !== null) {
                pages = this._getViewModel().pagesList()[find.level].pages;
                if (newData) {
                    pages.splice(find.index, 1, newData);
                } else {
                    pages.splice(find.index, 1);
					logger.warn(`<<< Exiting _updatePageData(id=${id},newData=${newData})`, { "event.action": "_updatePageData", "logrite.id": id, "logrite.newData": newData });
                }
            }
        },

        _findDataPosition: function(pageId) {
			logger.error(`>>> Entering _findDataPosition(pageId=${pageId})`, { "event.action": "_findDataPosition", "logrite.pageId": pageId });
            var viewModel, i, j, pagesList, listData;

            viewModel = this._getViewModel();
			logger.error(`_findDataPosition(pageId=${pageId}): viewModel → ${viewModel}`, { "event.action": "_findDataPosition", "logrite.pageId": pageId });

            for (i = 0; i < viewModel.pagesList().length; i++) {
                pagesList = viewModel.pagesList()[i].pages;
                listData = pagesList();

                for (j = 0; j < listData.length; j++) {
                    if (listData[j].id == pageId) {
						logger.warn(`<<< Exiting _findDataPosition(pageId=${pageId})`, { "event.action": "_findDataPosition", "logrite.pageId": pageId });
                        return { level: i, index: j, parentId: listData[j].parentId, childCount: listData[j].childCount };
                    }
                }
            }
            return null;
        },

        _findPageData: function(pageId) {
			logger.error(`>>> Entering _findPageData(pageId=${pageId})`, { "event.action": "_findPageData", "logrite.pageId": pageId });
            var viewModel, position;

            viewModel = this._getViewModel();
			logger.error(`_findPageData(pageId=${pageId}): viewModel → ${viewModel}`, { "event.action": "_findPageData", "logrite.pageId": pageId });
            position = this._findDataPosition(pageId);

            if (position !== null) {
                return viewModel.pagesList()[position.level].pages()[position.index];
            }

			logger.warn(`<<< Exiting _findPageData(pageId=${pageId})`, { "event.action": "_findPageData", "logrite.pageId": pageId });
            return null;
        },

        _getEmptyPageData: function() {
			logger.error('>>> Entering _getEmptyPageData()', { "event.action": "_getEmptyPageData" });
            return {
                id:   0,
                parentId: 0,
                name: '',
                childCount: 0,
                isspecial: false
            };
			logger.warn('<<< Exiting _getEmptyPageData()', { "event.action": "_getEmptyPageData" });
        },

        _clonePageData: function(data) {
			logger.error(`>>> Entering _clonePageData(data=${data})`, { "event.action": "_clonePageData", "logrite.data": data });
            var clone = {};

            for (var prop in data) {
                clone[prop] = data[prop];
            }  
            clone.timestamp = (new Date()).getTime();

			logger.warn(`<<< Exiting _clonePageData(data=${data})`, { "event.action": "_clonePageData", "logrite.data": data });
            return clone;
        },

        _scrollToSelectedPage: function() {
			logger.error('>>> Entering _scrollToSelectedPage()', { "event.action": "_scrollToSelectedPage" });
            var selectedPage, $listItem, $pagesList, containerScroller;

            selectedPage = this._getViewModel().selectedPage();
			logger.error(`_scrollToSelectedPage(): selectedPage → ${selectedPage}`, { "event.action": "_scrollToSelectedPage" });

            if (selectedPage.id > 0) {
                // Prevent scroll if click has been triggered by item-can-be-parent-list
                if ($(this.container.selector).find('.item-can-be-parent-list').length > 0) {return;}

                $listItem = this.container.find('li[data-page-id="' + selectedPage.id + '"]');
                $pagesList = $listItem.parents('.pages-list');
                containerScroller = this.container.find('.pages-list-scroller');

                if ($pagesList.hasClass('jspScrollable') && $pagesList.data('jsp')) {
                    $pagesList.data('jsp').scrollToElement($listItem[0], true, true);
                }

                if (containerScroller.hasClass('jspScrollable') && containerScroller.data('jsp')) {
                    containerScroller.data('jsp').scrollToElement($pagesList[0], true, true);
					logger.warn('<<< Exiting _scrollToSelectedPage()', { "event.action": "_scrollToSelectedPage" });
                }
            }
        },

        _pageExist: function(parentId) {
			logger.error(`>>> Entering _pageExist(parentId=${parentId})`, { "event.action": "_pageExist", "logrite.parentId": parentId });
            var pagesList, i;

            pagesList = this._getViewModel().pagesList();
			logger.error(`_pageExist(parentId=${parentId}): pagesList → ${pagesList}`, { "event.action": "_pageExist", "logrite.parentId": parentId });

            for (i = 0; i < pagesList.length; i++) {
                if (pagesList[i].parentId == parentId) {
                    return true;
                }
            }

			logger.warn(`<<< Exiting _pageExist(parentId=${parentId})`, { "event.action": "_pageExist", "logrite.parentId": parentId });
            return false;
        },

        _resizeContentContainer: function(resetContainer) {
			logger.error(`>>> Entering _resizeContentContainer(resetContainer=${resetContainer})`, { "event.action": "_resizeContentContainer", "logrite.resetContainer": resetContainer });
            var $body = this.panel.closest(".dnn-persona-bar-page-body");             
            var pageHierarchyContainer = $body.find(".pagehierarchy-container");
			logger.error(`_resizeContentContainer(resetContainer=${resetContainer}): pageHierarchyContainer → ${pageHierarchyContainer}`, { "event.action": "_resizeContentContainer", "logrite.resetContainer": resetContainer });
            var restHeight = pageHierarchyContainer.position().top;
			logger.error(`_resizeContentContainer(resetContainer=${resetContainer}): restHeight → ${restHeight}`, { "event.action": "_resizeContentContainer", "logrite.resetContainer": resetContainer });

            this.container.css('height', $(window).height() - restHeight - 20);

            this._initScrollView(resetContainer);
			logger.warn(`<<< Exiting _resizeContentContainer(resetContainer=${resetContainer})`, { "event.action": "_resizeContentContainer", "logrite.resetContainer": resetContainer });
        },

        _initScrollView: function (resetContainer, callback) {
			logger.error(`>>> Entering _initScrollView(resetContainer=${resetContainer},callback=${callback})`, { "event.action": "_initScrollView", "logrite.resetContainer": resetContainer, "logrite.callback": callback });
            var handler, inDrag, $pagesList, bottomSpace, scrollContainer, totalWidth;

            handler = this;
			logger.debug(`_initScrollView(resetContainer=${resetContainer},callback=${callback}): handler → ${handler}`, { "event.action": "_initScrollView", "logrite.resetContainer": resetContainer, "logrite.callback": callback });
            inDrag = this._getViewModel().inDrag();

            if (this._supressUI) {
                if (typeof callback == "function") {
                    callback.call(this);
                }

                return;
            }

            $pagesList = this.container.find('.pages-list-container div.pages-list');
            bottomSpace = 30;
            $pagesList.css('height', this.container.height() - bottomSpace).each(function() {
                var scrollContent = $(this);

                scrollContent.removeClass('animate');

                // Update ul opened height (space to drop)
                var padding = parseInt(scrollContent.css('padding-top')) + parseInt(scrollContent.css('padding-bottom'));
                if (padding == 0) { //jspPane may take the padding value
                    padding = parseInt(scrollContent.find('.jspPane').css('padding-top')) + parseInt(scrollContent.find('.jspPane').css('padding-bottom'));
                }

                scrollContent.find('ul:first').css({
                    'min-height': (scrollContent.innerHeight() - padding * 2) + 'px'
                });

                if (scrollContent.data('jsp')) {
                    scrollContent.data('jsp').reinitialise();
                } else {
                    scrollContent.jScrollPane({contentWidth: scrollContent.width()});
                }
            });

            scrollContainer = this.container.find('.pages-list-scroller');

            if (resetContainer) {
                // Reset the list container width
                totalWidth = 0;
                $pagesList.each(function() {
                    var self = $(this);
                    if (self.css('display') === 'none') {return true;} // Continue next elment if this is hidden
                    totalWidth += self.outerWidth() + parseInt(self.css('margin-left'));
                });
                this.container.find('.pages-list-container').width(totalWidth);

                if (scrollContainer.data('jsp')) {
                    scrollContainer.data('jsp').destroy();
                    scrollContainer = this.container.find('.pages-list-scroller');
                }

                scrollContainer.css('height', handler.container.height());
                scrollContainer.find('> .jspContainer .shadow').height($pagesList.outerHeight(true));
                scrollContainer.jScrollPane().on('jsp-scroll-x', function (e, offset, isLeft) {
                    var $this = $(this);
                    if (!isLeft) {
                        if ($this.find('> .jspContainer .shadow').length == 0) {
                            var $shadow = $('<div class="shadow" />');
                            $this.find('> .jspContainer').append($shadow);
                            $shadow.height($pagesList.outerHeight(true));
                        }
                    } else {
                        $this.find('> .jspContainer .shadow').remove();
                    }
                });
                handler.container.find('.pages-list-scroller > .jspContainer').css('height', handler.container.height());

                handler._scrollToSelectedPage();

                if (typeof callback === 'function') {
                    callback.call(this);
					logger.warn(`<<< Exiting _initScrollView(resetContainer=${resetContainer},callback=${callback})`, { "event.action": "_initScrollView", "logrite.resetContainer": resetContainer, "logrite.callback": callback });
                }
            }

            setTimeout(function() {
                $pagesList.addClass('animate');
            }, 100);
        },

        _removeListScrollView: function() {
			logger.error('>>> Entering _removeListScrollView()', { "event.action": "_removeListScrollView" });
            this.container.find('.pages-list-container div.pages-list').each(function () {
                var scrollContent = $(this);
                if (scrollContent.data('jsp')) {
                    scrollContent.data('jsp').destroy();
                }
            });

            this.container.find('.pages-list-container div.pages-list').addClass('animate');
			logger.warn('<<< Exiting _removeListScrollView()', { "event.action": "_removeListScrollView" });
        },

        _initDrag: function () {
			logger.error('>>> Entering _initDrag()', { "event.action": "_initDrag" });
            var handler, viewModel, lists, droppables;

            handler = this;
			logger.debug(`_initDrag(): handler → ${handler}`, { "event.action": "_initDrag" });
            viewModel = this._getViewModel();

            lists = this.container.find('.pages-list ul');

			logger.warn('<<< Exiting _initDrag()', { "event.action": "_initDrag" });
            droppables = $('.pagehierarchy-container .pages-list ul li');

            droppables.droppable({
                hoverClass: 'item-can-be-parent-list',

                // Open childs on over (wating some time)
                over: function () {
					logger.error('>>> Entering over()', { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                    var self = $(this);
					logger.error(`_initDrag(): self → ${self}`, { "event.action": "_initDrag" });
                    setTimeout(function() {
						logger.warn(`<<< Exiting over(event=${event},ui=${ui})`, { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                        if (self.hasClass('item-can-be-parent-list')) {
                            self.trigger('click'); // Triggers _addPagesList
                        }
                    }, OVER_TIME_TO_OPEN_PAGE_CHILDS);
					logger.error(`over(): self → ${self}`, { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                },

                // Drop directly on item
                drop: function (event, ui) {
					logger.error(`>>> Entering drop(event=${event},ui=${ui})`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
                    var self, item;

                    // Prevent add to list (sortable) on drop on droppable element
                    if ($(event.target).hasClass('page-drag-target')) {
                        dropOnDroppable = false;
                        return;
                    }

                    dropOnDroppable = true;

                    if (!uiOnDragStart) {return;}

                    self = $(this);
                    if (self.hasClass('page-drag-target')) {return;}
					logger.error(`drop(event=${event},ui=${ui}): self → ${self}`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
					logger.debug(`drop(event=${event},ui=${ui}): self → ${self}`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });

                    item = jQuery.extend({}, uiOnDragStart.item);
                    uiOnDragStart = null;
					logger.warn(`<<< Exiting drop(event=${event},ui=${ui})`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });

                    function updateHirerarchy() {
						logger.error('>>> Entering updateHirerarchy()', { "event.action": "updateHirerarchy" });
                        var sourcePageId, sourceUl, sourceFind, source, sourceData, targetId, targetIndex, targetFind, target;
                        var params, movePage200Callback, movePageErrorCallback;

                        pageDropped = {
                            item: $(ui.draggable[0]),
                            helper: $(ui.helper[0])
                        };

                        sourcePageId = item.data('page-id');
						logger.error(`updateHirerarchy(): sourcePageId → ${sourcePageId}`, { "event.action": "updateHirerarchy" });
                        sourceUl     = item.parent();
                        sourceFind   = handler._findDataPosition(sourcePageId);
                        source       = viewModel.pagesList()[sourceFind.level].pages()[sourceFind.index];
                        sourceData   = handler._clonePageData(source);

                        targetId     = self.data('page-id');

                        if (targetId === sourceFind.parentId) {return;}

                        targetFind  = handler._findDataPosition(targetId);
                        target      = viewModel.pagesList()[targetFind.level].pages()[targetFind.index];

                        setTimeout(function removeItemAndCallToUpdate() {
							logger.error('>>> Entering removeItemAndCallToUpdate()', { "event.action": "removeItemAndCallToUpdate" });
                            var newPagesArray, allowDrop;

                            allowDrop = handler._allowDrop(sourcePageId, targetFind.level);
                            if (!allowDrop) {
                                handler.utility.notifyError(handler.resx.DragInvalid);
                                sourceUl.sortable('cancel');

                                // After call cancel method, refresh page data to let ko populate DOM tree.
                                viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 1, sourceData);
                                ko.cleanNode(item[0]);
                                item.remove();

                                return;
                            }

                            // Remove page from UI
                            sourceUl.sortable('cancel');
                            viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 1);
                            ko.cleanNode(item[0]);
                            item.remove();

                            // Add source page as first child of target page
                            params = {
                                PageId: sourcePageId,
                                RelatedPageId: -1, // Not necessary for parent
                                ParentId: targetId,
                                Action:   'parent'
                            };

                            movePage200Callback = function(data) {
								logger.error(`>>> Entering movePage200Callback(data=${data})`, { "event.action": "movePage200Callback", "logrite.data": data });
                                var targetDataPosition, targetData, index, sourceParentId, sourceParentFind, sourceParentData;

                                if (data.Status > 0) {
                                    handler.utility.notifyError(handler.resx['Error_' + data.Message]);

                                    viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 0, sourceData);

                                    draggingJqObj = null;
                                    uiOnDragStart = null;

                                    dropOnDroppable = false;

                                    return;
                                }

                                // No errors, add elemnt to UI
                                sourceData.url = data.Page.url;
                                sourceData.parentId = data.Page.parentId;

                                // Update target child count
                                // If element moved to element below in same list
                                // index will be target - 1
                                if (sourceFind.level === targetFind.level && sourceFind.index < targetFind.index) {
                                    index = targetFind.index - 1;
                                } else {
                                    index = targetFind.index;
                                }

                                targetData = handler._clonePageData(viewModel.pagesList()[targetFind.level].pages()[index]);
                                targetData.childCount += 1;
                                viewModel.pagesList()[targetFind.level].pages.splice(index, 1, targetData);


                                if (sourceFind.parentId != -1) {
                                    sourceParentFind = handler._findDataPosition(sourceFind.parentId);
                                    sourceParentData = handler._clonePageData(viewModel.pagesList()[sourceParentFind.level].pages()[sourceParentFind.index]);
                                    sourceParentData.childCount -= 1;
                                    viewModel.pagesList()[sourceParentFind.level].pages.splice(sourceParentFind.index, 1, sourceParentData);
                                }

                                // If target page is open add page because otherwise it will be painted on wrong level
                                if (self.hasClass('selected') && viewModel.pagesList()[data.Page.level] !== undefined) {
                                    viewModel.pagesList()[data.Page.level].pages.push(sourceData);
                                }

                                // ??
                                $("#showsite").data('need-refresh', true);

                                draggingJqObj = null;
                                uiOnDragStart = null;

                                // Reactivate droppables/sortables
                                setTimeout(function reinitDragAndDrop() {
									logger.error('>>> Entering reinitDragAndDrop()', { "event.action": "reinitDragAndDrop" });
                                    handler._initDrag();
									logger.warn('<<< Exiting reinitDragAndDrop()', { "event.action": "reinitDragAndDrop" });
                                }, 0);
                                viewModel.dragPage(handler._getEmptyPageData());
                                if (viewModel.isNew()) {
                                    viewModel.isNew(false);
                                    handler._enterEditMode(data.Page);
                                }

                                dropOnDroppable = false;
                            };

                            movePageErrorCallback = function (data) {
								logger.error(`>>> Entering movePageErrorCallback(data=${data})`, { "event.action": "movePageErrorCallback", "logrite.data": data });
                                handler.utility.notifyError('Unknown error');

                                draggingJqObj = null;
                                uiOnDragStart = null;


                                viewModel.pagesList()[sourceFind.level].pages.splice(undefined, 0, sourceData);

                                dropOnDroppable = false;

								logger.warn(`<<< Exiting movePageErrorCallback(data=${data})`, { "event.action": "movePageErrorCallback", "logrite.data": data });
                                return;
                            };

                            // Tray to move by api call
							logger.warn('<<< Exiting removeItemAndCallToUpdate()', { "event.action": "removeItemAndCallToUpdate" });
                            handler._getService().post('MovePage', params, movePage200Callback, movePageErrorCallback);
                        }, 0);
                    }

                    updateHirerarchy();
                }
            });

            lists.each(function () {
                var $self = $(this), triggerClickPage;

                $self.sortable({
                    connectWith: '.pagehierarchy-container .pages-list ul',
                    dropOnEmpty: true,
                    cursor: '',
                    cursorAt: { left: 110, top: 10 },
                    handle: 'span.drag-area',
                    placeholder: 'page-drag-target',
                    tolerance: 'intersect',
                    revert: false,
                    helper: function (event, ui) {
						logger.error(`>>> Entering helper(event=${event},ui=${ui})`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });
                        var $dragItem, data;

                        $dragItem = $('<div class="page-drag-helper"></div>')
                                            .append('<span class="icon" />')
                                            .append('<span class="drag-page-name">'+ ui.find('span.field-name').html() + '</span>')
                                            .appendTo(handler.container);

                        data = ko.dataFor(ui[0]);

                        if (data && data.childCount > 0) {
                            $dragItem.addClass('page-drag-multiple');
                        }

						logger.warn(`<<< Exiting helper(event=${event},ui=${ui})`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });
                        return $dragItem;
                    },

                    start: function (event, ui) {
						logger.error(`>>> Entering start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
                        uiOnDragStart = {
                            item: $(ui.item)
                        };
                        draggingJqObj = $(ui.item[0]);
                        $self.data('ui-sa-ph', $self.data('ui-sortable').options.placeholder);
                        handler.container.find('div.pages-drag-container').hide();
						logger.warn(`<<< Exiting start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
                    },

                    beforeStop: function(event, ui) {
						logger.error(`>>> Entering beforeStop(event=${event},ui=${ui})`, { "event.action": "beforeStop", "logrite.event": event, "logrite.ui": ui });
                        if (dropOnDroppable) {
                            uiOnDragStart = null;
                            dropOnDroppable = false;
							logger.warn(`<<< Exiting beforeStop(event=${event},ui=${ui})`, { "event.action": "beforeStop", "logrite.event": event, "logrite.ui": ui });
                        }
                    },

                    receive: function (event, ui) {
					logger.error(`>>> Entering receive(event=${event},ui=${ui})`, { "event.action": "receive", "logrite.event": event, "logrite.ui": ui });
                    },

                    over: function (event, ui) {
					logger.error(`>>> Entering over(event=${event},ui=${ui})`, { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                    },

                    sort: function(event, ui) {
						logger.error(`>>> Entering sort(event=${event},ui=${ui})`, { "event.action": "sort", "logrite.event": event, "logrite.ui": ui });
                        ui.placeholder.html(ui.helper.html());
						logger.warn(`<<< Exiting sort(event=${event},ui=${ui})`, { "event.action": "sort", "logrite.event": event, "logrite.ui": ui });
                    },

                    stop: function (event, ui) {
						logger.error(`>>> Entering stop(event=${event},ui=${ui})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
                        var item, sourceParentFind,
                        isDragItem, $pageList, level, parentId, pageId, index, find,
                        movePageData, allowDrop, pageItem, moveAction, relatedPageId, params;

                        if (!uiOnDragStart) {return;}

                        item = jQuery.extend({}, uiOnDragStart.item);
						logger.error(`stop(event=${event},ui=${ui}): item → ${item}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
                        uiOnDragStart = null;
                        draggingJqObj = null;

                        // Remove hidden lists created when dragging for prevent
                        // view change (list/deail list) revelating this lists
                        handler._removeHiddenLists();

                        // Means drag from drag container, which always need post data.
                        isDragItem = ui.item.find('> div').hasClass('drag-item');

                        $pageList = ui.item.parents('.pages-list:eq(0)');
                        level = $pageList.data('page-level');
                        parentId = $pageList.data('parent-id');
                        pageId = ui.item.data('page-id');
                        index = ui.item.parent().find('> li').index(ui.item);
                        find = handler._findDataPosition(pageId);

                        if (!isDragItem && find && find.level === level && find.index === index) {
                            return;
                        }

                        movePageData = viewModel.pagesList()[find.level].pages()[find.index];

                        allowDrop = handler._allowDrop(pageId, level);
                        if (!allowDrop) {
                            handler.utility.notifyError(handler.resx.DragInvalid);
                            $self.sortable('cancel');

                            // After call cancel method, refresh page data to let ko populate DOM tree.
                            viewModel.pagesList()[find.level].pages.splice(find.index, 1, handler._clonePageData(movePageData));
                            ko.cleanNode(ui.item[0]);
                            ui.item.remove();

                            return;
                        }

                        moveAction = "after";
                        relatedPageId = -1;

                        if (ui.item.prev().length === 0 && ui.item.next().length === 0) {
                            if (isDragItem && parentId === undefined) {
                                // If release drag item not in pane, then append it as last item in root.
                                ui.item.remove();
                                viewModel.inDrag(false);
                                pageItem = $('li[data-page-id="' + pageId + '"] > div');
                                pageItem.show();

                                handler._needScrollToSelectedPage = true;
                                viewModel.selectedPage(movePageData);
                                viewModel.dragPage(handler._getEmptyPageData());

                                return;
                            } else {
                                moveAction = "parent";
                            }

                        } else if (ui.item.prev().length === 0) {
                            moveAction = "before";
                            relatedPageId = ui.item.next().length > 0 ? ui.item.next().data('page-id') : relatedPageId;
                        } else  {
                            relatedPageId = ui.item.prev().data('page-id');
                        }

                        params = {
                            PageId: pageId,
                            RelatedPageId: relatedPageId,
                            ParentId: parentId,
                            Action: moveAction
                        };

                        handler._getService().post('MovePage', params, function(data) {
                            var pageData, sourceParentFindB, sourceParentData, targetId, targetFind, targetFindB;
                        
                            if (data.Status > 0) {
								logger.debug(`stop(event=${event},ui=${ui}): pageData.url → ${pageData.url}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
								logger.debug(`stop(event=${event},ui=${ui}): pageData.parentId → ${pageData.parentId}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
                                handler.utility.notifyError(handler.resx['Error_' + data.Message]);
                                $self.sortable('cancel');

                                // After call cancel method, refresh page data to let ko populate DOM tree.
                                viewModel.pagesList()[find.level].pages.splice(find.index, 1, handler._clonePageData(movePageData));
                                ko.cleanNode(ui.item[0]);
                                ui.item.remove();

                                return;
                            }

                            targetId = data.Page.parentId;
                            pageData = ko.dataFor(ui.item[0]);
							logger.error(`stop(event=${event},ui=${ui}): pageData → ${pageData}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
                            pageData.url = data.Page.url;
                            pageData.parentId = data.Page.parentId;

                            sourceParentFind = handler._findDataPosition(find.parentId);
                            targetFind = handler._findDataPosition(targetId);
                            handler._updatePagePosition(pageData, level, index, find.parentId);
                            targetFindB = handler._findDataPosition(targetId);
                            sourceParentFindB = handler._findDataPosition(find.parentId);

                            // Update source parent child counter if not yet
                            if (find.parentId != -1 && sourceParentFind.childCount === sourceParentFindB.childCount) {
                                sourceParentData = handler._clonePageData(viewModel.pagesList()[sourceParentFind.level].pages()[sourceParentFind.index]);
                                sourceParentData.childCount -= 1;
                                viewModel.pagesList()[sourceParentFind.level].pages.splice(sourceParentFind.index, 1, sourceParentData);
                            }

                            // Update target parent child counter if not yet
                            if (targetFind && targetFind.childCount === targetFindB.childCount) {
                                sourceParentData = handler._clonePageData(viewModel.pagesList()[targetFind.level].pages()[targetFind.index]);
                                sourceParentData.childCount += 1;
                                viewModel.pagesList()[targetFind.level].pages.splice(targetFind.index, 1, sourceParentData);
                            }

                            if (isDragItem) {
                                handler._needScrollToSelectedPage = true;
                                viewModel.selectedPage(pageData);
                                viewModel.dragPage(handler._getEmptyPageData());

                                if (pageData.isCopy) {
                                    handler._enterEditMode(pageData);
                                }
                            }

                            $("#showsite").data('need-refresh', true);

                            ko.cleanNode(ui.item[0]);
                            ui.item.remove();

							logger.warn(`<<< Exiting stop(event=${event},ui=${ui})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui });
                            // Reactivate droppables/sortables
                            setTimeout(function reinitDragAndDrop() {
                                // $('.pages-list-container .pages-list li[data-page-id="' + pageId + '"]').removeClass('selected');
                                handler._initDrag();
                            }, 0);

                            // enter into Edit mode
                            if (viewModel.isNew() && data.Page.pageType === "normal") {
                                viewModel.isNew(false);
                                handler._enterEditMode(pageData);
                            }
                        });
                    }
                });

                // If we are in middle of dragging action, need to refresh
                // sortable elements to update lists conections
                if (draggingJqObj) {
                    setTimeout(function () {
                        $('.pages-list ul').each(function () {
                            $(this).sortable('refresh');
                        });

                        $self.data('ui-sortable').options.placeholder = $self.data('ui-sa-ph');
                    }, 0);
                }

                return true;
            });
        },

        _allowDrop: function(pageId, level) {
			logger.error(`>>> Entering _allowDrop(pageId=${pageId},level=${level})`, { "event.action": "_allowDrop", "logrite.pageId": pageId, "logrite.level": level });
            if (level === 0) {
                return true;
            }

            var $pagesList = this.container.find('.pages-list-container .pages-list');
            var $currentList = this.container.find('.pages-list-container .pages-list[data-parent-id="' + pageId + '"]');

			logger.warn(`<<< Exiting _allowDrop(pageId=${pageId},level=${level})`, { "event.action": "_allowDrop", "logrite.pageId": pageId, "logrite.level": level });
            return $currentList.length === 0 || $pagesList.index($currentList) > level;
        },

        _updatePagePosition: function(page, level, index, pId) {
			logger.error(`>>> Entering _updatePagePosition(page=${page},level=${level},index=${index},pId=${pId})`, { "event.action": "_updatePagePosition", "logrite.page": page, "logrite.level": level, "logrite.index": index, "logrite.pId": pId });
            var viewModel, pagesList, originalPosition, originalLevel, parentId,
                addOffset, find, pageData;

            viewModel = this._getViewModel();
			logger.error(`_updatePagePosition(page=${page},level=${level},index=${index},pId=${pId}): viewModel → ${viewModel}`, { "event.action": "_updatePagePosition", "logrite.page": page, "logrite.level": level, "logrite.index": index, "logrite.pId": pId });
            pagesList = viewModel.pagesList();
            originalPosition = this._findDataPosition(page.id);

            // Page was moved in other function
            if (!originalPosition) {return;}

            originalLevel = originalPosition.level;

            pagesList[originalLevel].pages.splice(originalPosition.index, 1);
            pagesList[level].pages.splice(index, 0, page);

            // Update child count of parent page
            parentId = -1;
            addOffset = 0;

            if (level > originalLevel) { //move as child so need add parent's child count.
                parentId = pagesList[level].parentId;
                if (originalPosition.parentId !== pId) {
                    addOffset = 1;
                } else {
                    addOffset = 0;
                }
            } else if (level < originalLevel) { //move from child so need reduce parent's child count.
                parentId = pagesList[originalLevel].parentId;
                addOffset = -1;
            }

            if (parentId > 0) {
                find = this._findDataPosition(parentId);

                if (find) {
                    pageData = this._clonePageData(pagesList[find.level].pages()[find.index]);
                    pageData.childCount += addOffset;
                    pagesList[find.level].pages.splice(find.index, 1, pageData);

                    if (viewModel.selectedPage().id == pageData.id) {
                        viewModel.selectedPage(pageData);
                    }
                }
            }

            if (originalLevel != level && viewModel.selectedPage().id == page.id) {

                viewModel.pagesList.remove(function(data) {
                    return data.level >= originalLevel;
                });
                //viewModel.selectedPage(this._getEmptyPageData()); // cannot read property 'nodeType' of undefined
                viewModel.selectedPage(page);
            }

            this._initScrollView();
			logger.warn(`<<< Exiting _updatePagePosition(page=${page},level=${level},index=${index},pId=${pId})`, { "event.action": "_updatePagePosition", "logrite.page": page, "logrite.level": level, "logrite.index": index, "logrite.pId": pId });
        },

        _computeSelectedPagePath: function() {
			logger.error('>>> Entering _computeSelectedPagePath()', { "event.action": "_computeSelectedPagePath" });
            var selectedPage, selectedPagePath, find, i, parentId, $pageItem, pageData;

            selectedPage = this._getViewModel().selectedPage();
			logger.error(`_computeSelectedPagePath(): selectedPage → ${selectedPage}`, { "event.action": "_computeSelectedPagePath" });
            selectedPagePath = this._getViewModel().selectedPagePath;

            var selectedPagePathLenght = selectedPagePath().length;
			logger.error(`_computeSelectedPagePath(): selectedPagePathLenght → ${selectedPagePathLenght}`, { "event.action": "_computeSelectedPagePath" });
            if (!selectedPagePathLenght) {
                selectedPagePath.push(selectedPage);
                return;    
            }

            while (selectedPagePathLenght > 0 && 
                   selectedPagePath()[selectedPagePathLenght - 1].id !== selectedPage.parentId) {
                selectedPagePath.pop();
                -- selectedPagePathLenght;           
            }
            
            selectedPagePath.push(selectedPage);
			logger.warn('<<< Exiting _computeSelectedPagePath()', { "event.action": "_computeSelectedPagePath" });
        },

        _getViewModel: function() {
			logger.error('>>> Entering _getViewModel()', { "event.action": "_getViewModel" });
            var handler = this;

            if (typeof this._viewModel.pagesList == "undefined") {
                this._viewModel.itemTemplate = ko.observable("pages-list-item-template");
                this._viewModel.dragItemTemplate = ko.observable("pages-drag-item-template");
                this._viewModel.pagesList = ko.observableArray([]);
                this._viewModel.resx = handler.resx;
                this._viewModel.selectedPage = ko.observable(handler._getEmptyPageData());
                this._viewModel.dragPage = ko.observable({id: 0, name: '', status: '', childCount: 0});
                this._viewModel.inDrag = ko.observable(true);
                this._viewModel.isNew = ko.observable(false);
                this._viewModel.selectedPagePath = ko.observableArray([]);
                this._viewModel.selectedPagePath.subscribe($.proxy(this._selectedPagePathChanged, this));
                this._viewModel.searchKeyword = ko.observable('');
                this._viewModel.searchFocus = ko.observable(true);
                this._viewModel.deletedPagesCount = ko.observable(0);

                this._viewModel.selectedPage.subscribe($.proxy(this._selectedPageChanged, this));
                this._viewModel.inDrag.subscribe($.proxy(this._inDragChanged, this));
                
                this._viewModel.pageItemClick = $.proxy(this._pageItemClickHandler, this);
                this._viewModel.viewPageClick = $.proxy(this._viewPageClickHandler, this);
                this._viewModel.editPageClick = $.proxy(this._editPageClickHandler, this);
                this._viewModel.settingsPageClick = $.proxy(this._settingsPageClickHandler, this);
                this._viewModel.deletePageClick = $.proxy(this._deletePageClickHandler, this);

                this._viewModel.doSearch = $.proxy(this._searchPage, this);

                if (window.dnn.pages.viewModelExtension) {
                    for (var prop in window.dnn.pages.viewModelExtension) {
                        if (typeof window.dnn.pages.viewModelExtension[prop] === 'function') {
                            this._viewModel[prop] = $.proxy(window.dnn.pages.viewModelExtension[prop], this);
                        } else {
                            this._viewModel[prop] = window.dnn.pages.viewModelExtension[prop];
							logger.warn('<<< Exiting _getViewModel()', { "event.action": "_getViewModel" });
                        }
                    }                
                }
            }
            return this._viewModel;
        },       
        _getOverridableService() {
			logger.error('>>> Entering _getOverridableService()', { "event.action": "_getOverridableService" });
            this.utility.sf.moduleRoot = "PersonaBar";
            this.utility.sf.controller =  window.dnn.pages.apiController;

			logger.warn('<<< Exiting _getOverridableService()', { "event.action": "_getOverridableService" });
            return this.utility.sf;
        },
        _getService() {
            this.utility.sf.moduleRoot = "PersonaBar";
            this.utility.sf.controller = "Pages";

            return this.utility.sf;
        }
    };    

    pageHierarchyDefaultOptions = {
        delayTime: 500,
        requestDelayTime: 2000,
        requestTimeout: 4000
    };

window.dnn.pages.pageHierarchyManager = new PageHierarchyManager(window.dnn.pages.pageHierarchyManagerOptions);
export const pageHierarchyManager = window.dnn.pages.pageHierarchyManager;
