var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/***
 * @class DnnPageRecycleBin
 *
 * This class bla bla bla
 *
 * @depends jQuery
 * @depends knockout
 *
 * @param {type} contructorParam (default value), explanation
 *
 */

define(['jquery', 'knockout',
    'main/koBindingHandlers/toggle',
    'main/koBindingHandlers/jScrollPane',
    'main/koBindingHandlers/tabs',
    'main/koBindingHandlers/setWhenHover',
    'main/koBindingHandlers/setWidthFromParentScrollPaneWhen'], function ($, ko) {
        var DnnPageRecycleBin;

        var ModuleInfo = function (requestData) {
			logger.error(`>>> Entering ModuleInfo(requestData=${requestData})`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
            var self = this;

            // request data
            self.Id = requestData.Id;
			logger.debug(`ModuleInfo(requestData=${requestData}): self.Id → ${self.Id}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.Title → ${self.Title}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.PortalId → ${self.PortalId}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.TabName → ${self.TabName}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.TabID → ${self.TabID}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.TabDeleted → ${self.TabDeleted}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.LastModifiedOnDate → ${self.LastModifiedOnDate}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
			logger.debug(`ModuleInfo(requestData=${requestData}): self.friendlyLastModifiedOnDate → ${self.friendlyLastModifiedOnDate}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
            self.TabModuleId = requestData.TabModuleId;
			logger.debug(`ModuleInfo(requestData=${requestData}): self.TabModuleId → ${self.TabModuleId}`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
            self.Title = requestData.Title;
            self.PortalId = requestData.PortalId;
            self.TabName = requestData.TabName;
            self.TabID = requestData.TabID;
            self.TabDeleted = requestData.TabDeleted;
            self.LastModifiedOnDate = requestData.LastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.selected = ko.observable(false);
            self.mouseOver = ko.observable(false);
			logger.warn(`<<< Exiting ModuleInfo(requestData=${requestData})`, { "event.action": "ModuleInfo", "logrite.requestData": requestData });
        };

        var UserInfo = function (requestData) {
			logger.error(`>>> Entering UserInfo(requestData=${requestData})`, { "event.action": "UserInfo", "logrite.requestData": requestData });
            var self = this;

            // request data
            self.Id = requestData.Id;
			logger.debug(`UserInfo(requestData=${requestData}): self.Id → ${self.Id}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
			logger.debug(`UserInfo(requestData=${requestData}): self.PortalId → ${self.PortalId}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
			logger.debug(`UserInfo(requestData=${requestData}): self.DisplayName → ${self.DisplayName}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
			logger.debug(`UserInfo(requestData=${requestData}): self.Email → ${self.Email}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
			logger.debug(`UserInfo(requestData=${requestData}): self.LastModifiedOnDate → ${self.LastModifiedOnDate}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
			logger.debug(`UserInfo(requestData=${requestData}): self.friendlyLastModifiedOnDate → ${self.friendlyLastModifiedOnDate}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
            self.Username = requestData.Username;
			logger.debug(`UserInfo(requestData=${requestData}): self.Username → ${self.Username}`, { "event.action": "UserInfo", "logrite.requestData": requestData });
            self.PortalId = requestData.PortalId;
            self.DisplayName = requestData.DisplayName;
            self.Email = requestData.Email;
            self.LastModifiedOnDate = requestData.LastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.selected = ko.observable(false);
            self.mouseOver = ko.observable(false);
			logger.warn(`<<< Exiting UserInfo(requestData=${requestData})`, { "event.action": "UserInfo", "logrite.requestData": requestData });
        };

        var PageInfo = function (requestData) {
			logger.error(`>>> Entering PageInfo(requestData=${requestData})`, { "event.action": "PageInfo", "logrite.requestData": requestData });
            var PAGES_TREE_LEFT_PADDING = 50;

            var self = this;
            // private methods declaration 
            var calculateDepthOfNode, calculateNumberOfAllChildrenTree,
                selectAllChildren, isItsRootElementEven, setOddEvenInChildren,
                hasUnselectedChildren;

            // private methods implementation
            calculateDepthOfNode = function (node) {
				logger.error(`>>> Entering calculateDepthOfNode(node=${node})`, { "event.action": "calculateDepthOfNode", "logrite.node": node });
                if (node.parent) {
                    self.depth(1 + calculateDepthOfNode(node.parent));
                } else {
                    self.depth(0);
                }
				logger.warn(`<<< Exiting calculateDepthOfNode(node=${node})`, { "event.action": "calculateDepthOfNode", "logrite.node": node });
                return self.depth();
            };

            calculateNumberOfAllChildrenTree = function (children) {
				logger.error(`>>> Entering calculateNumberOfAllChildrenTree(children=${children})`, { "event.action": "calculateNumberOfAllChildrenTree", "logrite.children": children });
                var numberOfNestedChildren = 0;

                if (!children || children.length === 0) {
                    return 0;
                }

                children.forEach(function (child) {
                    numberOfNestedChildren += calculateNumberOfAllChildrenTree(child.children());
                });

                return children.length + numberOfNestedChildren;
            };

            hasUnselectedChildren = function (children) {
				logger.error(`>>> Entering hasUnselectedChildren(children=${children})`, { "event.action": "hasUnselectedChildren" });
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    if (!child.selected()) {
                        return true;
                    }

                    if (hasUnselectedChildren(child.children())) {
                        return true;
                    }
                }
				logger.warn(`<<< Exiting hasUnselectedChildren(children=${children})`, { "event.action": "hasUnselectedChildren" });
				logger.warn('<<< Exiting hasUnselectedChildren()', { "event.action": "hasUnselectedChildren" });
                return false;
            };

            selectAllChildren = function (children, newValue) {
				logger.error(`>>> Entering selectAllChildren(children=${children},newValue=${newValue})`, { "event.action": "selectAllChildren", "logrite.children": children, "logrite.newValue": newValue });
                if (!self.children) {
                    return;
                }

                children().forEach(function (child) {
                    child.selected(newValue);
                });
				logger.warn(`<<< Exiting selectAllChildren(children=${children},newValue=${newValue})`, { "event.action": "selectAllChildren", "logrite.children": children, "logrite.newValue": newValue });
            };

            setOddEvenInChildren = function (children, newValue) {
				logger.error(`>>> Entering setOddEvenInChildren(children=${children},newValue=${newValue})`, { "event.action": "setOddEvenInChildren", "logrite.children": children, "logrite.newValue": newValue });
                if (!self.children) {
                    return;
                }

                children().forEach(function (child) {
                    child.even(newValue);
                });
				logger.warn(`<<< Exiting setOddEvenInChildren(children=${children},newValue=${newValue})`, { "event.action": "setOddEvenInChildren", "logrite.children": children, "logrite.newValue": newValue });
            };

            isItsRootElementEven = function (node) {
				logger.error(`>>> Entering isItsRootElementEven(node=${node})`, { "event.action": "isItsRootElementEven", "logrite.node": node });
                if (node.parent) {
                    return isItsRootElementEven(node.parent);
                }
				logger.warn(`<<< Exiting isItsRootElementEven(node=${node})`, { "event.action": "isItsRootElementEven", "logrite.node": node });
                return node.even();
            };

            // request data
            self.id = requestData.id;
			logger.debug(`PageInfo(requestData=${requestData}): self.id → ${self.id}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.childCount → ${self.childCount}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.url → ${self.url}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.publishDate → ${self.publishDate}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.status → ${self.status}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.parentId → ${self.parentId}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.level → ${self.level}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.tabpath → ${self.tabpath}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.isspecial → ${self.isspecial}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.useDefaultSkin → ${self.useDefaultSkin}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.lastModifiedOnDate → ${self.lastModifiedOnDate}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): self.friendlyLastModifiedOnDate → ${self.friendlyLastModifiedOnDate}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
            self.name = requestData.name;
			logger.debug(`PageInfo(requestData=${requestData}): self.name → ${self.name}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
            self.childCount = requestData.childCount;
            self.url = requestData.url;
            self.publishDate = requestData.publishDate;
            self.status = requestData.status;
            self.parentId = requestData.parentId;
            self.level = requestData.level;
            self.tabpath = requestData.tabpath;
            self.isspecial = requestData.isspecial;
            self.useDefaultSkin = requestData.useDefaultSkin;
            self.lastModifiedOnDate = requestData.lastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.even = ko.observable(false);
            self.even.extend({ notify: 'always' });

            self.selected = ko.observable(false);
            self.selected.extend({ notify: 'always' });

            self.mouseOver = ko.observable(false);
            self.depth = ko.observable(0);

            self.parent = null;
			logger.debug(`PageInfo(requestData=${requestData}): self.parent → ${self.parent}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
            self.children = ko.observableArray([]);

            // computed attributes
            self.numberOfNestedChildren = ko.computed(function () {
                return 1 + calculateNumberOfAllChildrenTree(self.children());
            });

            self.cellWidth = ko.computed(function () {
                var separatorWidth = 19;
                var labelWidth = 160;
			logger.debug(`PageInfo(requestData=${requestData}): childrenWithEdge → ${childrenWithEdge}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
			logger.debug(`PageInfo(requestData=${requestData}): childrenHeight → ${childrenHeight}`, { "event.action": "PageInfo", "logrite.requestData": requestData });
                return (self.numberOfNestedChildren() * separatorWidth) + labelWidth;
            });

            self.totalHeight = ko.computed(function () {

                var childrenWithEdge, childrenHeight;

                childrenWithEdge = [];
                for (var i = 0; i < self.children().length - 1; i++) {
                    childrenWithEdge.push(self.children()[i]);
                }

                childrenHeight = calculateNumberOfAllChildrenTree(childrenWithEdge);

                if (childrenHeight === 0) {
                    return 0;
                }

                return (childrenHeight * 64) + "px";
            });

            self.isItsRootElementEven = ko.computed(function () {
                return isItsRootElementEven(self);
            });

            self.depthInPixels = ko.computed(function () {
                return (self.depth() * PAGES_TREE_LEFT_PADDING) + "px";
            });

            // subscribe methods
            self.selected.subscribe(function (newValue) {
                selectAllChildren(self.children, newValue);
            });

            self.even.subscribe(function (newValue) {
                setOddEvenInChildren(self.children, newValue);
            });

			logger.warn(`<<< Exiting PageInfo(requestData=${requestData})`, { "event.action": "PageInfo", "logrite.requestData": requestData });
            // API methods
            self.calculateDepth = function () {
				logger.error('>>> Entering calculateDepth()', { "event.action": "calculateDepth" });
                calculateDepthOfNode(self);
				logger.warn('<<< Exiting calculateDepth()', { "event.action": "calculateDepth" });
            };
            self.hasUnselectedChildren = function () {
                return hasUnselectedChildren(self.children());
            };
        };

        DnnPageRecycleBin = (function () {
            'use strict';

            // var CONSTANT;
            var RECYCLE_BIN_DEFAULT_OPTIONS = {

            };

            // var _privateVar;
            var _options, _resx, _serviceController, _utility, _viewModel, _settings, _listOfPages;

            // var privateMethod
            var getSelectedElements, markAllElementsAs, markAllModulesAsIfPossible, getTreesOfPages, setOddEvenOrderInRoots, calculateDepthOfPages,
                changeElementSelectedStatus, restoreSelectedPagesHandler, removeSelectedPagesHandler,
                pageRestoreRevomeOperationsCallback, canBeDeleted,
                restoreSelectedModulesHandler, removeSelectedModulesHandler,
                restorePageHandler, removePageHandler, restoreModuleHandler, removeModuleHandler, emptyRecycleBinHandler,
                restoreUserHandler, removeUserHandler, restoreSelectedUsersHandler, removeSelectedUsersHandler, userRestoreRevomeOperationsCallback,
                Paginate, getDeletedPageList, getDeletedModuleList, getDeletedUserList,
                getService, getViewModel, tabActivated, sortBy;

            /* Class properties */
            DnnPageRecycleBin.class = 'DnnPageRecycleBin';
            DnnPageRecycleBin.type = 'Class'; // If class only has Class Methods put Static Class here

            /* Private Constants and Properties */

            /* Constructor */
            function DnnPageRecycleBin(resx, serviceController, utility, options, settings) {
				logger.error(`>>> Entering DnnPageRecycleBin(resx=${resx},serviceController=${serviceController},utility=${utility},options=${options},settings=${settings})`, { "event.action": "DnnPageRecycleBin", "logrite.resx": resx, "logrite.serviceController": serviceController, "logrite.utility": utility, "logrite.options": options, "logrite.settings": settings });
                _resx = resx;
                _serviceController = serviceController;
                _utility = utility;
                _options = options;
				logger.warn(`<<< Exiting DnnPageRecycleBin(resx=${resx},serviceController=${serviceController},utility=${utility},options=${options},settings=${settings})`, { "event.action": "DnnPageRecycleBin", "logrite.resx": resx, "logrite.serviceController": serviceController, "logrite.utility": utility, "logrite.options": options, "logrite.settings": settings });
                _settings = {
                    canViewPages: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_PAGES_VIEW,
                    canViewModules: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_MODULES_VIEW,
                    canViewUsers: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_USERS_VIEW,
                    canManagePages: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_PAGES_EDIT,
                    canManageModules: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_MODULES_EDIT,
                    canManageUsers: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_USERS_EDIT,
                    canEmptyRecycleBin: settings.isHost || settings.isAdmin ||
                        (settings.permissions.RECYCLEBIN_PAGES_VIEW && settings.permissions.RECYCLEBIN_PAGES_EDIT && settings.permissions.RECYCLEBIN_MODULES_VIEW && settings.permissions.RECYCLEBIN_MODULES_EDIT && settings.permissions.RECYCLEBIN_USERS_VIEW && settings.permissions.RECYCLEBIN_USERS_EDIT)
                };
            }


            /* Private Methods */
            // ex: method = function () {};
            changeElementSelectedStatus = function (page) {
                page.selected(!page.selected());
				logger.warn(`<<< Exiting changeElementSelectedStatus(page=${page})`, { "event.action": "changeElementSelectedStatus", "logrite.page": page });
            };

            getSelectedElements = function (collection) {
				logger.error(`>>> Entering getSelectedElements(collection=${collection})`, { "event.action": "getSelectedElements", "logrite.collection": collection });
                var selectedElements = [];

                if (!collection) {
                    return selectedElements;
                }

                collection.forEach(function (element) {
                    if (element.selected()) {
                        selectedElements.push(element);
                    }
                    if (element.children) {
                        selectedElements = selectedElements.concat(getSelectedElements(element.children()));
                    }
                });

                return selectedElements;
            };

            markAllElementsAs = function (collection, value) {
				logger.error(`>>> Entering markAllElementsAs(collection=${collection},value=${value})`, { "event.action": "markAllElementsAs", "logrite.collection": collection, "logrite.value": value });
                if (!collection) {
                    return;
                }

                collection.forEach(function (element) {
                    element.selected(value);
                });
				logger.warn(`<<< Exiting markAllElementsAs(collection=${collection},value=${value})`, { "event.action": "markAllElementsAs", "logrite.collection": collection, "logrite.value": value });
            };

            markAllModulesAsIfPossible = function (modules, newValue) {
				logger.error(`>>> Entering markAllModulesAsIfPossible(modules=${modules},newValue=${newValue})`, { "event.action": "markAllModulesAsIfPossible", "logrite.modules": modules, "logrite.newValue": newValue });
                if (!modules) {
                    return;
                }
                var isAnyModuleInDeletedPage = false;
                var viewModel = getViewModel();
                modules.forEach(function (module) {
                    if (!module.TabDeleted) {
                        module.selected(newValue);
                    } else {
                        isAnyModuleInDeletedPage = true;
                    }
                });
                if (isAnyModuleInDeletedPage && viewModel.selectAllModules())
                    _utility.notifyError(_resx.recyclebin_UnableToSelectAllModules);
					logger.warn(`<<< Exiting markAllModulesAsIfPossible(modules=${modules},newValue=${newValue})`, { "event.action": "markAllModulesAsIfPossible", "logrite.modules": modules, "logrite.newValue": newValue });
            }

            restoreSelectedPagesHandler = function () {
				logger.error('>>> Entering restoreSelectedPagesHandler()', { "event.action": "restoreSelectedPagesHandler" });
                if (_settings.canViewPages && _settings.canManagePages) {
                    var pagesList, viewModel;

                    viewModel = getViewModel();
                    pagesList = getSelectedElements(viewModel.deletedpagesList());
					logger.error(`restoreSelectedPagesHandler(): pagesList → ${pagesList}`, { "event.action": "restoreSelectedPagesHandler" });

                    if (pagesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = pagesList.length > 1 ? _resx.recyclebin_RestorePagesConfirm : _resx.recyclebin_RestorePageConfirm;
						logger.debug(`restoreSelectedPagesHandler(): confirmText → ${confirmText}`, { "event.action": "restoreSelectedPagesHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestorePage', pagesList, pageRestoreRevomeOperationsCallback);
                        });
						logger.warn('<<< Exiting restoreSelectedPagesHandler()', { "event.action": "restoreSelectedPagesHandler" });
                    }
                }
            };

            canBeDeleted = function (selectedPages) {
				logger.error(`>>> Entering canBeDeleted(selectedPages=${selectedPages})`, { "event.action": "canBeDeleted", "logrite.selectedPages": selectedPages });
                var result = {};
                result.value = true;
				logger.error(`canBeDeleted(selectedPages=${selectedPages}): result → ${result}`, { "event.action": "canBeDeleted", "logrite.selectedPages": selectedPages });
				logger.debug(`canBeDeleted(selectedPages=${selectedPages}): result.value → ${result.value}`, { "event.action": "canBeDeleted", "logrite.selectedPages": selectedPages });
                result.errors = "";
				logger.debug(`canBeDeleted(selectedPages=${selectedPages}): result.errors → ${result.errors}`, { "event.action": "canBeDeleted", "logrite.selectedPages": selectedPages });

                selectedPages.forEach(function (page) {
                    if (page.hasUnselectedChildren()) {

                        result.value = false;
                        result.errors += _resx.Service_RemoveTabError.replace(/\{0\}/g, page.name);
                    }
                });

                return result;
            };

            removeSelectedPagesHandler = function () {
				logger.error('>>> Entering removeSelectedPagesHandler()', { "event.action": "removeSelectedPagesHandler" });
                if (_settings.canViewPages && _settings.canManagePages) {
                    var pagesList, viewModel;

                    viewModel = getViewModel();
                    pagesList = getSelectedElements(viewModel.deletedpagesList());
					logger.error(`removeSelectedPagesHandler(): pagesList → ${pagesList}`, { "event.action": "removeSelectedPagesHandler" });

                    if (pagesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = pagesList.length > 1 ? _resx.recyclebin_RemovePagesConfirm : _resx.recyclebin_RemovePageConfirm;
						logger.debug(`removeSelectedPagesHandler(): confirmText → ${confirmText}`, { "event.action": "removeSelectedPagesHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            var result = canBeDeleted(pagesList);
                            if (!result.value) {
                                _utility.notifyError(_resx.Service_RemoveTabErrorHeader + result.errors);
                                return;
                            }
                            getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                        });
                    }
                }
				logger.warn('<<< Exiting removeSelectedPagesHandler()', { "event.action": "removeSelectedPagesHandler" });
            };

            restoreSelectedModulesHandler = function () {
				logger.error('>>> Entering restoreSelectedModulesHandler()', { "event.action": "restoreSelectedModulesHandler" });
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, modulesList;

                    viewModel = getViewModel();
					logger.error(`restoreSelectedModulesHandler(): viewModel → ${viewModel}`, { "event.action": "restoreSelectedModulesHandler" });
                    modulesList = getSelectedElements(viewModel.deletedmodulesList());

                    if (modulesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = modulesList.length > 1 ? _resx.recyclebin_RestoreModulesConfirm : _resx.recyclebin_RestoreModuleConfirm;
						logger.debug(`restoreSelectedModulesHandler(): confirmText → ${confirmText}`, { "event.action": "restoreSelectedModulesHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestoreModule', modulesList, function () {
                                for (var i = 0; i < modulesList.length; i++) {
                                    viewModel.deletedmodulesList.remove(modulesList[i]);
                                }
                                viewModel.selectAllModules(false);
								logger.warn('<<< Exiting getService()', { "event.action": "getService" });
                            });
                        });
                    }
					logger.warn('<<< Exiting restoreSelectedModulesHandler()', { "event.action": "restoreSelectedModulesHandler" });
                }
            };

            removeSelectedModulesHandler = function () {
				logger.error('>>> Entering removeSelectedModulesHandler()', { "event.action": "removeSelectedModulesHandler" });
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, modulesList;

                    viewModel = getViewModel();
					logger.error(`removeSelectedModulesHandler(): viewModel → ${viewModel}`, { "event.action": "removeSelectedModulesHandler" });
                    modulesList = getSelectedElements(viewModel.deletedmodulesList());

                    if (modulesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = modulesList.length > 1 ? _resx.recyclebin_RemoveModulesConfirm : _resx.recyclebin_RemoveModuleConfirm;
						logger.debug(`removeSelectedModulesHandler(): confirmText → ${confirmText}`, { "event.action": "removeSelectedModulesHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RemoveModule', modulesList, function () {
                                for (var i = 0; i < modulesList.length; i++) {
                                    viewModel.deletedmodulesList.remove(modulesList[i]);
                                }
                                viewModel.selectAllModules(false);
                            });
                        });
                    }
					logger.warn('<<< Exiting removeSelectedModulesHandler()', { "event.action": "removeSelectedModulesHandler" });
                }
            };

            restoreSelectedUsersHandler = function () {
				logger.error('>>> Entering restoreSelectedUsersHandler()', { "event.action": "restoreSelectedUsersHandler" });
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var usersList, viewModel;

                    viewModel = getViewModel();
                    usersList = getSelectedElements(viewModel.deletedusersList());
					logger.error(`restoreSelectedUsersHandler(): usersList → ${usersList}`, { "event.action": "restoreSelectedUsersHandler" });

                    if (usersList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = usersList.length > 1 ? _resx.recyclebin_RestoreUsersConfirm : _resx.recyclebin_RestoreUserConfirm;
						logger.debug(`restoreSelectedUsersHandler(): confirmText → ${confirmText}`, { "event.action": "restoreSelectedUsersHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestoreUser', usersList, userRestoreRevomeOperationsCallback);
                        });
						logger.warn('<<< Exiting restoreSelectedUsersHandler()', { "event.action": "restoreSelectedUsersHandler" });
                    }
                }
            };

            removeSelectedUsersHandler = function () {
				logger.error('>>> Entering removeSelectedUsersHandler()', { "event.action": "removeSelectedUsersHandler" });
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var viewModel, usersList;

                    viewModel = getViewModel();
					logger.error(`removeSelectedUsersHandler(): viewModel → ${viewModel}`, { "event.action": "removeSelectedUsersHandler" });
                    usersList = getSelectedElements(viewModel.deletedusersList());

                    if (usersList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = usersList.length > 1 ? _resx.recyclebin_RemoveUsersConfirm : _resx.recyclebin_RemoveUserConfirm;
						logger.debug(`removeSelectedUsersHandler(): confirmText → ${confirmText}`, { "event.action": "removeSelectedUsersHandler" });
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RemoveUser', usersList, function () {
                                for (var i = 0; i < usersList.length; i++) {
                                    viewModel.deletedusersList.remove(usersList[i]);
                                }
                                viewModel.selectAllUsers(false);
                            });
                        });
                    }
					logger.warn('<<< Exiting removeSelectedUsersHandler()', { "event.action": "removeSelectedUsersHandler" });
                }
            };

            userRestoreRevomeOperationsCallback = function (data) {
				logger.error(`>>> Entering userRestoreRevomeOperationsCallback(data=${data})`, { "event.action": "userRestoreRevomeOperationsCallback", "logrite.data": data });
                if (data.Status > 0) {
                    // Error: inform
                    _utility.notifyError(data.Message);
                }
                getDeletedUserList();
                //CONTENT-4010 - call refresh in person bar
                $("#showsite").data('need-refresh', true);
				logger.warn(`<<< Exiting userRestoreRevomeOperationsCallback(data=${data})`, { "event.action": "userRestoreRevomeOperationsCallback", "logrite.data": data });
            };

            pageRestoreRevomeOperationsCallback = function (data) {
				logger.error(`>>> Entering pageRestoreRevomeOperationsCallback(data=${data})`, { "event.action": "pageRestoreRevomeOperationsCallback", "logrite.data": data });
                if (data.Status > 0) {
                    // Error: inform
                    _utility.notifyError(data.Message);
                }
                getDeletedPageList();
                //CONTENT-4010 - call refresh in person bar
                $("#showsite").data('need-refresh', true);
				logger.warn(`<<< Exiting pageRestoreRevomeOperationsCallback(data=${data})`, { "event.action": "pageRestoreRevomeOperationsCallback", "logrite.data": data });
            };

            restorePageHandler = function (pageData) {
				logger.error(`>>> Entering restorePageHandler(pageData=${pageData})`, { "event.action": "restorePageHandler", "logrite.pageData": pageData });
                if (_settings.canViewPages && _settings.canManagePages) {
                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RestorePageConfirm;
					logger.debug(`restorePageHandler(pageData=${pageData}): confirmText → ${confirmText}`, { "event.action": "restorePageHandler", "logrite.pageData": pageData });
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var pagesList = [];
                        pagesList.push({ id: pageData.id });
                        getService().post('RestorePage', pagesList, pageRestoreRevomeOperationsCallback);
                    });
					logger.warn(`<<< Exiting restorePageHandler(pageData=${pageData})`, { "event.action": "restorePageHandler", "logrite.pageData": pageData });
                }
            };

            restoreUserHandler = function (userData) {
				logger.error(`>>> Entering restoreUserHandler(userData=${userData})`, { "event.action": "restoreUserHandler", "logrite.userData": userData });
                if (_settings.canViewUsers && _settings.canManageUsers) {

                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RestoreUserConfirm;
					logger.debug(`restoreUserHandler(userData=${userData}): confirmText → ${confirmText}`, { "event.action": "restoreUserHandler", "logrite.userData": userData });
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var usersList = [];
                        usersList.push({ id: userData.Id });
                        getService().post('RestoreUser', usersList, userRestoreRevomeOperationsCallback);
                    });
					logger.warn(`<<< Exiting restoreUserHandler(userData=${userData})`, { "event.action": "restoreUserHandler", "logrite.userData": userData });
                }
            };

            removePageHandler = function (pageData) {
				logger.error(`>>> Entering removePageHandler(pageData=${pageData})`, { "event.action": "removePageHandler", "logrite.pageData": pageData });
                if (_settings.canViewPages && _settings.canManagePages) {
                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RemovePageConfirm;
					logger.debug(`removePageHandler(pageData=${pageData}): confirmText → ${confirmText}`, { "event.action": "removePageHandler", "logrite.pageData": pageData });
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        if (pageData.children().length > 0) {
                            _utility.notifyError(_resx.Service_RemoveTabParentTabError.replace(/\{0\}/g, pageData.name));
                            return;
                        }
                        var pagesList = [];
                        pagesList.push({ id: pageData.id });
                        getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                    });
                }
				logger.warn(`<<< Exiting removePageHandler(pageData=${pageData})`, { "event.action": "removePageHandler", "logrite.pageData": pageData });
            };

            removeUserHandler = function (userData) {
				logger.error(`>>> Entering removeUserHandler(userData=${userData})`, { "event.action": "removeUserHandler", "logrite.userData": userData });
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
					logger.error(`removeUserHandler(userData=${userData}): viewModel → ${viewModel}`, { "event.action": "removeUserHandler", "logrite.userData": userData });
                    confirmText = _resx.recyclebin_RemoveUserConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var userList = [];
                        userList.push({
                            Id: userData.Id
                        });
                        getService().post('RemoveUser', userList, function () {
                            viewModel.deletedusersList.remove(userData);
                        });
                    });
					logger.warn(`<<< Exiting removeUserHandler(userData=${userData})`, { "event.action": "removeUserHandler", "logrite.userData": userData });
                }
            };

            restoreModuleHandler = function (moduleData, e) {
				logger.error(`>>> Entering restoreModuleHandler(moduleData=${moduleData},e=${e})`, { "event.action": "restoreModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
					logger.error(`restoreModuleHandler(moduleData=${moduleData},e=${e}): viewModel → ${viewModel}`, { "event.action": "restoreModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                    confirmText = _resx.recyclebin_RestoreModuleConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var moduleList = [];
                        moduleList.push({
                            Id: moduleData.Id,
                            TabModuleId: moduleData.TabModuleId,
                            TabID: moduleData.TabID
                        });
                        getService().post('RestoreModule', moduleList, function () {
                            viewModel.deletedmodulesList.remove(moduleData);
                        });
                    });
					logger.warn(`<<< Exiting restoreModuleHandler(moduleData=${moduleData},e=${e})`, { "event.action": "restoreModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                }
            };

            removeModuleHandler = function (moduleData, e) {
				logger.error(`>>> Entering removeModuleHandler(moduleData=${moduleData},e=${e})`, { "event.action": "removeModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
					logger.error(`removeModuleHandler(moduleData=${moduleData},e=${e}): viewModel → ${viewModel}`, { "event.action": "removeModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                    confirmText = _resx.recyclebin_RemoveModuleConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var moduleList = [];
                        moduleList.push({
                            Id: moduleData.Id,
                            TabModuleId: moduleData.TabModuleId,
                            TabID: moduleData.TabID
                        });
                        getService().post('RemoveModule', moduleList, function () {
                            viewModel.deletedmodulesList.remove(moduleData);
                        });
                    });
					logger.warn(`<<< Exiting removeModuleHandler(moduleData=${moduleData},e=${e})`, { "event.action": "removeModuleHandler", "logrite.moduleData": moduleData, "logrite.e": e });
                }
            };

            emptyRecycleBinHandler = function () {
				logger.error('>>> Entering emptyRecycleBinHandler()', { "event.action": "emptyRecycleBinHandler" });
                if (_settings.canEmptyRecycleBin) {
                    var viewModel = getViewModel();
					logger.error(`emptyRecycleBinHandler(): viewModel → ${viewModel}`, { "event.action": "emptyRecycleBinHandler" });
                    var confirmText = _resx.recyclebin_EmptyRecycleBinConfirm;
                    var deleteText = _resx.recyclebin_DeleteConfirm;
                    var cancelText = _resx.recyclebin_CancelConfirm;

                    _utility.confirm(confirmText, deleteText, cancelText, function () {
                        getService().get('EmptyRecycleBin', { t: (new Date).getTime() }, function () {
                            viewModel.deletedpagesList.removeAll();
                            viewModel.deletedmodulesList.removeAll();
                            viewModel.deletedusersList.removeAll();
                        });
                    });
					logger.warn('<<< Exiting emptyRecycleBinHandler()', { "event.action": "emptyRecycleBinHandler" });
                }
            };

            setOddEvenOrderInRoots = function (roots) {
				logger.error(`>>> Entering setOddEvenOrderInRoots(roots=${roots})`, { "event.action": "setOddEvenOrderInRoots", "logrite.roots": roots });
                var isPreviousElementEven = false;
                roots.forEach(function (root) {
                    root.even(isPreviousElementEven);
                    isPreviousElementEven = !isPreviousElementEven;
                });
				logger.warn(`<<< Exiting setOddEvenOrderInRoots(roots=${roots})`, { "event.action": "setOddEvenOrderInRoots", "logrite.roots": roots });
            };

            calculateDepthOfPages = function (pages) {
				logger.error(`>>> Entering calculateDepthOfPages(pages=${pages})`, { "event.action": "calculateDepthOfPages", "logrite.pages": pages });
                if (typeof pages === "undefined") {
                    return;
                }

                pages.forEach(function (page) {
                    page.calculateDepth();
                });
				logger.warn(`<<< Exiting calculateDepthOfPages(pages=${pages})`, { "event.action": "calculateDepthOfPages", "logrite.pages": pages });
            };

            getTreesOfPages = function (pages) {
				logger.error(`>>> Entering getTreesOfPages(pages=${pages})`, { "event.action": "getTreesOfPages", "logrite.pages": pages });
                var roots, idToNodeMap, parentNode;
                roots = [];
				logger.debug(`getTreesOfPages(pages=${pages}): roots → ${roots}`, { "event.action": "getTreesOfPages", "logrite.pages": pages });
                idToNodeMap = {};

                pages.forEach(function (page) {
                    idToNodeMap[page.id] = page;

                    if (typeof page.parentId === "undefined") {
                        roots.push(page);
                        return;
                    }

                    parentNode = idToNodeMap[page.parentId];
                    if (!parentNode) {
                        roots.push(page);
                    } else {
                        parentNode.children.push(page);
                        page.parent = parentNode;
                    }
                });

                calculateDepthOfPages(pages);

                setOddEvenOrderInRoots(roots);

                return roots;
            };

            var TotalResults = {}
            var timeout = null;
            var pageSize=20;

            Paginate = function(API_METHOD, viewModelProp, elementId){
				logger.error(`>>> Entering Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId})`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });
                var element = $(elementId).jScrollPane();
                var api = element.data("jsp");
				logger.error(`Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId}): element → ${element}`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });
				logger.error(`Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId}): api → ${api}`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });
                var viewModel = getViewModel();

				logger.error(`Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId}): viewModel → ${viewModel}`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });
                var compare = TotalResults.hasOwnProperty(viewModelProp)==true
				logger.error(`Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId}): compare → ${compare}`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });

                TotalResults[viewModelProp] = (compare) ? TotalResults[viewModelProp] : {modelName:viewModelProp, paginationRequestCount:1};

                var currentResultsLessThanTotal = function(){
					logger.error('>>> Entering currentResultsLessThanTotal()', { "event.action": "currentResultsLessThanTotal" });
                    var comparator = TotalResults[viewModelProp].remainingPages > 0;
					logger.warn('<<< Exiting currentResultsLessThanTotal()', { "event.action": "currentResultsLessThanTotal" });
                    return comparator;
                };


                element.bind('jsp-scroll-y', function(event, scrollPositionY, isAtTop, isAtBottom) {
                    if(!timeout){
                        if(scrollPositionY > 0 && isAtBottom && !timeout && currentResultsLessThanTotal() ) {
                            timeout = setTimeout(function(){
                                timeout=null;
                            },2000);

                            TotalResults[viewModelProp].paginationRequestCount = TotalResults[viewModelProp].paginationRequestCount || 1;

                            getService().get(API_METHOD, {pageIndex: TotalResults[viewModelProp].paginationRequestCount++, pageSize: pageSize, sortType: viewModel.sortType(), sortDirection: viewModel.sortDirection() }, function (data) {

                                var results = data.Results;
                                var conditional = TotalResults[viewModelProp].remainingPages-results.length > 0;
                                var temp = [];
                                TotalResults[viewModelProp].remainingPages = (conditional) ?  TotalResults[viewModelProp].remainingPages-results.length : 0;

                                switch(viewModelProp) {
                                    case 'deletedpagesList':
                                        results.forEach(function(temppage){
                                            var page = new PageInfo(temppage);
                                            temp.push(page);
                                        });
                                        var treeOfPages = getTreesOfPages(temp);
                                        treeOfPages.forEach(function(pageTemp) {
                                            viewModel[viewModelProp].push(pageTemp);
                                        });
                                        break;

                                    case 'deletedusersList':
                                        results.forEach(function(tempUser){
                                            var user = new UserInfo(tempUser);
                                            viewModel[viewModelProp].push(user)
                                        });
                                        break;

                                    case 'deletedtemplatesList':
                                        results.forEach(function(tempTemplate){
                                            var template = new TemplateInfo(tempTemplate);
                                            viewModel[viewModelProp].push(template);
                                        });
                                        break;

                                    case 'deletedmodulesList':
                                        results.forEach(function(tempModule){
                                            var module = new ModuleInfo(tempModule);
                                            viewModel[viewModelProp].push(module);
                                        });
                                        break;

                                }
                                api.reinitialise();
                                api.scrollToPercentY(.95);
                            });
                        }
                    }
                });
				logger.warn(`<<< Exiting Paginate(API_METHOD=${API_METHOD},viewModelProp=${viewModelProp},elementId=${elementId})`, { "event.action": "Paginate", "logrite.API_METHOD": API_METHOD, "logrite.viewModelProp": viewModelProp, "logrite.elementId": elementId });
            };

            var initPagination = function() {
				logger.error('>>> Entering initPagination()', { "event.action": "initPagination" });
                var tabs = [
                    {apiMethod: "GetDeletedPageList", viewModelProp: "deletedpagesList", elementId:"#pageList" },
                    {apiMethod: "GetDeletedUserList", viewModelProp: "deletedusersList", elementId: "#userList"},
                    {apiMethod: "GetDeletedModuleList", viewModelProp: "deletedmodulesList", elementId:"#moduleList"},
                    {apiMethod: "GetDeletedTemplates", viewModelProp: "deletedtemplatesList", elementId:"#templateList"}
                ];

                tabs.forEach(function(tab){
                    Paginate(tab.apiMethod, tab.viewModelProp, tab.elementId );
                });
				logger.warn('<<< Exiting initPagination()', { "event.action": "initPagination" });
            };

            getDeletedPageList = function () {
				logger.error('>>> Entering getDeletedPageList()', { "event.action": "getDeletedPageList" });
                if (_settings.canViewPages) {
                    var listOfPages, viewModel;
                    var element = $('#pageList').jScrollPane();
					logger.error(`getDeletedPageList(): element → ${element}`, { "event.action": "getDeletedPageList" });
                    var api = element.data("jsp");
					logger.error(`getDeletedPageList(): api → ${api}`, { "event.action": "getDeletedPageList" });

                    api.scrollToPercentY(0);
                    listOfPages = [];
					logger.debug(`getDeletedPageList(): listOfPages → ${listOfPages}`, { "event.action": "getDeletedPageList" });
                    viewModel = getViewModel();
                    viewModel.deletedPagesReady(false);
                    viewModel.deletedpagesList.removeAll();


                    getService().get('GetDeletedPageList', {pageIndex:0, pageSize:pageSize, sortType:viewModel.sortType(), sortDirection:viewModel.sortDirection() }, function (data) {
                        var results = data.Results;
                        var viewModelProp = "deletedpagesList";
                        TotalResults[viewModelProp]={};
                        TotalResults[viewModelProp].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelProp].moduleName = viewModelProp;

                        for (var i = 0; i < results.length; i++) {
                            var page = new PageInfo(results[i]);
                            listOfPages.push(page);
                        }

                        _listOfPages = listOfPages.concat();
                        viewModel.selectAllPages(false);
                        viewModel.deletedpagesList(getTreesOfPages(_listOfPages));
                        viewModel.deletedPagesReady(true);
                    });
				logger.warn('<<< Exiting getDeletedPageList()', { "event.action": "getDeletedPageList" });
                }

            };

            getDeletedModuleList = function (isInitCall) {
				logger.error(`>>> Entering getDeletedModuleList(isInitCall=${isInitCall})`, { "event.action": "getDeletedModuleList", "logrite.isInitCall": isInitCall });
                if (_settings.canViewModules) {
                    var viewModel = getViewModel();
					logger.error(`getDeletedModuleList(isInitCall=${isInitCall}): viewModel → ${viewModel}`, { "event.action": "getDeletedModuleList", "logrite.isInitCall": isInitCall });
                    if ((typeof (isInitCall) !== "boolean" || typeof (isInitCall) === "boolean" && !isInitCall) && !viewModel.deletedModulesReady()) {
                        return;
                    }
                    var element = $('#moduleList').jScrollPane();
					logger.error(`getDeletedModuleList(isInitCall=${isInitCall}): element → ${element}`, { "event.action": "getDeletedModuleList", "logrite.isInitCall": isInitCall });
                    var api = element.data("jsp");
					logger.error(`getDeletedModuleList(isInitCall=${isInitCall}): api → ${api}`, { "event.action": "getDeletedModuleList", "logrite.isInitCall": isInitCall });

                    api.scrollToPercentY(0);
                    viewModel.deletedModulesReady(false);
                    viewModel.deletedmodulesList.removeAll();


                    getService().get('GetDeletedModuleList', {pageIndex:0, pageSize:pageSize, sortType:viewModel.sortType(), sortDirection:viewModel.sortDirection()}, function (data) {
                        var results = data.Results;
                        var viewModelPropName = 'deletedmodulesList';

                        TotalResults[viewModelPropName]={};
                        TotalResults[viewModelPropName].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelPropName].paginationRequestCount=1;
                        TotalResults[viewModelPropName].moduleName = viewModelPropName;

                        for (var i = 0; i < results.length; i++) {
                            var module = new ModuleInfo(results[i]);
                            viewModel.deletedmodulesList.push(module);
                        }
                        viewModel.deletedModulesReady(true);

                    });
                }
				logger.warn(`<<< Exiting getDeletedModuleList(isInitCall=${isInitCall})`, { "event.action": "getDeletedModuleList", "logrite.isInitCall": isInitCall });
            };


            getDeletedUserList = function (isInitCall) {
				logger.error(`>>> Entering getDeletedUserList(isInitCall=${isInitCall})`, { "event.action": "getDeletedUserList", "logrite.isInitCall": isInitCall });
                if (_settings.canViewUsers) {
                    var viewModel = getViewModel();
					logger.error(`getDeletedUserList(isInitCall=${isInitCall}): viewModel → ${viewModel}`, { "event.action": "getDeletedUserList", "logrite.isInitCall": isInitCall });
                    if ((typeof (isInitCall) !== "boolean" || typeof (isInitCall) === "boolean" && !isInitCall) && !viewModel.deletedUsersReady()) {
                        return;
                    }
                    var element = $('#userList').jScrollPane();
					logger.error(`getDeletedUserList(isInitCall=${isInitCall}): element → ${element}`, { "event.action": "getDeletedUserList", "logrite.isInitCall": isInitCall });
                    var api = element.data("jsp");
					logger.error(`getDeletedUserList(isInitCall=${isInitCall}): api → ${api}`, { "event.action": "getDeletedUserList", "logrite.isInitCall": isInitCall });

                    api.scrollToPercentY(0);

                    viewModel.deletedUsersReady(false);
                    viewModel.deletedusersList.removeAll();

                    getService().get('GetDeletedUserList', {pageIndex:0, pageSize:pageSize, sortType:viewModel.sortType(), sortDirection:viewModel.sortDirection()}, function (data) {
                        var results = data.Results;
                        var viewModelPropName = "deletedusersList";
                        TotalResults[viewModelPropName]={};
                        TotalResults[viewModelPropName].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelPropName].moduleName = viewModelPropName;

                        for (var i = 0; i < results.length; i++) {
                            var user = new UserInfo(results[i]);
                            viewModel.deletedusersList.push(user);
                        }
                        viewModel.selectAllUsers(false);
                        viewModel.deletedUsersReady(true);

					logger.error('>>> Entering getService()', { "event.action": "getService" });
					logger.warn('<<< Exiting getService()', { "event.action": "getService" });
                    });
                }
				logger.warn(`<<< Exiting getDeletedUserList(isInitCall=${isInitCall})`, { "event.action": "getDeletedUserList", "logrite.isInitCall": isInitCall });
            };

            tabActivated = function (event, ui) {
				logger.error(`>>> Entering tabActivated(event=${event},ui=${ui})`, { "event.action": "tabActivated", "logrite.event": event, "logrite.ui": ui });
                var panelId = ui.newPanel.attr('id');
                var activeTab = 0;
                switch (panelId) {
                    case 'pages':
                        activeTab = 0;
						logger.error(`tabActivated(event=${event},ui=${ui}): panelId → ${panelId}`, { "event.action": "tabActivated", "logrite.event": event, "logrite.ui": ui });
                        break;
                    case 'modules':
                        activeTab = 1;
                        break;
                    case 'users':
                        activeTab = 2;
                        break;
                    default:
                        activeTab = 0;
                        break;
                }
                getViewModel().activeTab(activeTab);
				logger.warn(`<<< Exiting tabActivated(event=${event},ui=${ui})`, { "event.action": "tabActivated", "logrite.event": event, "logrite.ui": ui });
            };

            sortBy = function (section, type, direction) {
				logger.error(`>>> Entering sortBy(section=${section},type=${type},direction=${direction})`, { "event.action": "sortBy", "logrite.section": section, "logrite.type": type, "logrite.direction": direction });
                getViewModel().sortDirection(direction);
                getViewModel().sortType(type);
                switch (section) {
                    case "pages":
                        getDeletedPageList(false);
                        break;
                    case "modules":
                        getDeletedModuleList(false);
                        break;
                    case "users":
                        getDeletedUserList(false);
						logger.warn(`<<< Exiting sortBy(section=${section},type=${type},direction=${direction})`, { "event.action": "sortBy", "logrite.section": section, "logrite.type": type, "logrite.direction": direction });
                        break;
                }
            }

            getViewModel = function () {
				logger.error('>>> Entering getViewModel()', { "event.action": "getViewModel" });
                if (!_viewModel) {
                    _viewModel = {
                        resx: _resx,
                        settings: _settings,

                        sortType: ko.observable(""),
                        sortDirection: ko.observable(""),

                        activeTab: ko.observable(0),
                        deletedPagesReady: ko.observable(false),
                        deletedModulesReady: ko.observable(false),
                        deletedUsersReady: ko.observable(false),

                        deletedpagesList: ko.observableArray([]),
                        deletedmodulesList: ko.observableArray([]),
                        deletedusersList: ko.observableArray([]),

                        selectAllPages: ko.observable(false),
                        selectAllModules: ko.observable(false),
                        selectAllUsers: ko.observable(false),

                        changeElementSelectedStatus: changeElementSelectedStatus,

                        removePage: removePageHandler,
                        removeModule: removeModuleHandler,
                        removeUser: removeUserHandler,

                        restorePage: restorePageHandler,
                        restoreModule: restoreModuleHandler,
                        restoreUser: restoreUserHandler,

                        restoreSelectedPages: restoreSelectedPagesHandler,
                        restoreSelectedModules: restoreSelectedModulesHandler,
                        restoreSelectedUsers: restoreSelectedUsersHandler,

                        removeSelectedPages: removeSelectedPagesHandler,
                        removeSelectedModules: removeSelectedModulesHandler,
                        removeSelectedUsers: removeSelectedUsersHandler,

                        refreshPages: getDeletedPageList,
                        refreshModules: getDeletedModuleList,
                        refreshUsers: getDeletedUserList,

                        emptyRecycleBin: emptyRecycleBinHandler,

                        tabActivated: tabActivated,

                        sortBy: sortBy

                    };
                    if (_settings.canViewPages && _settings.canManagePages) {
                        _viewModel.selectAllPages.subscribe(function (newValue) {
                            markAllElementsAs(_viewModel.deletedpagesList(), newValue);
                        });
                    }
                    if (_settings.canViewModules && _settings.canManageModules) {
                        _viewModel.selectAllModules.subscribe(function (newValue) {
                            markAllModulesAsIfPossible(_viewModel.deletedmodulesList(), newValue);
                        });
                    }
                    if (_settings.canViewUsers && _settings.canManageUsers) {
                        _viewModel.selectAllUsers.subscribe(function (newValue) {
                            markAllElementsAs(_viewModel.deletedusersList(), newValue);
                        });
                    }
                }

				logger.warn('<<< Exiting getViewModel()', { "event.action": "getViewModel" });
                return _viewModel;
            };

            getService = function () {
				logger.error('>>> Entering getService()', { "event.action": "getService" });
                _utility.sf.moduleRoot = "personaBar";
                _utility.sf.controller = "Recyclebin";

                return _utility.sf;
            };

            DnnPageRecycleBin.prototype.init = function (wrapper) {
				logger.error(`>>> Entering init(wrapper=${wrapper})`, { "event.action": "init", "logrite.wrapper": wrapper });
                _options = $.extend({}, RECYCLE_BIN_DEFAULT_OPTIONS, _options);
                var viewModel = getViewModel();
                ko.applyBindings(viewModel, wrapper[0]);
				logger.error(`init(wrapper=${wrapper}): viewModel → ${viewModel}`, { "event.action": "init", "logrite.wrapper": wrapper });
                initPagination();
				logger.warn(`<<< Exiting init(wrapper=${wrapper})`, { "event.action": "init", "logrite.wrapper": wrapper });
            };

            DnnPageRecycleBin.prototype.show = function () {
				logger.error('>>> Entering show()', { "event.action": "show" });
                getDeletedPageList();
                getDeletedModuleList(true);
				logger.warn('<<< Exiting show()', { "event.action": "show" });
                getDeletedUserList(true);

            };

            return DnnPageRecycleBin;
        })();

        return DnnPageRecycleBin;
    });