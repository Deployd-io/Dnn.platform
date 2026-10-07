(function () {
    'use strict';

    var debugMode = window.parent['personaBarSettings']['debugMode'] === true;
    var cdv = window.parent['personaBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css',
            'main': '../../scripts',
            'modules': '../../modules'
        },
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : ''),
        shim: {
            'jquery.hoverintent.min': ['jquery'],
            'jquery.qatooltip': ['jquery.hoverintent.min']

        },
        map: {
            '*': {
                'dnn.jquery': ['../../../../../Resources/Shared/Scripts/dnn.jquery'],
                'dnn.jquery.extensions': ['../../../../../Resources/Shared/Scripts/dnn.jquery.extensions'],
                'dnn.extensions': ['../../../../../Resources/Shared/scripts/dnn.extensions'],
                'jquery.tokeninput': ['../../../../../Resources/Shared/components/Tokeninput/jquery.tokeninput'],
                'dnn.jScrollBar': ['../../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar'],
                'dnn.servicesframework': ['../../../../../js/dnn.servicesframework'],
                'dnn.DataStructures': ['../../../../../Resources/Shared/scripts/dnn.DataStructures'],
                'jquery.mousewheel': ['../../../../../Resources/Shared/scripts/jquery/jquery.mousewheel'],
                'dnn.TreeView': ['../../../../../Resources/Shared/scripts/TreeView/dnn.TreeView'],
                'dnn.DynamicTreeView': ['../../../../../Resources/Shared/scripts/TreeView/dnn.DynamicTreeView'],
                'dnn.DropDownList': ['../../../../../Resources/Shared/Components/DropDownList/dnn.DropDownList'],
                'css.DropDownList': ['css!../../../../../Resources/Shared/components/DropDownList/dnn.DropDownList.css'],
                'css.jScrollBar': ['css!../../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar.css']
            }
        },
        packages: [{
            name: "codemirror",
            location: "./../../../../../Resources/Shared/components/CodeEditor",
            main: "lib/codemirror"
        }],
        waitSeconds: 30
    });
    requirejs.onError = function (err) {
		logger.error(`>>> Entering onError(err=${err})`, { "event.action": "onError", "logrite.err": err });
        // If requireJs throws a timeout reload the page
        if (err.requireType === 'timeout') {
            console.error(err);
            location.reload();
        }
        else {
            console.error(err);
			logger.warn(`<<< Exiting onError(err=${err})`, { "event.action": "onError", "logrite.err": err });
            throw err;
        }
    };
})();

// Enable React Dev Tools inside the iframe, this should be loaded before React has been loaded
if (window.parent['personaBarSettings'].debugMode === true) {
    window.__REACT_DEVTOOLS_GLOBAL_HOOK__ = window.parent.__REACT_DEVTOOLS_GLOBAL_HOOK__;
}

require(['jquery', 'knockout', 'dayjs', '../util', '../sf', '../config', './../extension',
        '../persistent', '../eventEmitter', '../menuIconLoader', '../gateway', 'domReady!', '../exports/export-bundle'],
    function ($, ko, dayjs, ut, sf, cf, extension, persistent, eventEmitter, iconLoader, Gateway) {
        var iframe = window.parent.document.getElementById("personaBar-iframe");
        if (!iframe) return;

        var onTouch = "ontouchstart" in document.documentElement;
        // Checking touch screen for second level menu - the above onTouch won't work on windows tablet - IE I didnt test but read about it.
        var isTouch = ('ontouchstart' in window) || (navigator.msMaxTouchPoints > 0);

        var activePath = null;
        var activemodule = '';
        var parentBody = window.parent.document.body;
        var body = window.document.body;
        var config = cf.init();
        var utility = ut.init(config);
        var inAnimation = false;
        var personaBarMenuWidth = parseInt($("#personabar").width());
        var $iframe = $(iframe);
        var $body = $(body);
        var $personaBarPanels = $("#personabar-panels");
        var $personaBarPlaceholder = $('.socialpanel-placeholder');
        var $mask = $(".socialmask");
        var $avatarImage;
        var $personaBar = $('#personabar');
        var $showSiteButton = $('#showsite');
        var customModules = [];
        var disableEditBar = window.parent['personaBarSettings']['disableEditBar'] === true;

        window.requirejs.config({
            paths: {
                'rootPath': utility.getApplicationRootPath()
            }
        });

        if (config.skin) {
            $personaBar.addClass(config.skin);
        }

        var menuViewModel = utility.buildMenuViewModel(config.menuStructure);
        var cachedPersonaBarPageWidth = 860;

        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
            onTouch: onTouch,
            dayjs: dayjs,
            persistent: persistent.init(config, sf),
            inAnimation: inAnimation,

            setConfirmationDialogPosition: function () {
				logger.error('>>> Entering setConfirmationDialogPosition()', { "event.action": "setConfirmationDialogPosition" });
                var confirmation = document.getElementById('confirmation-dialog');
                var personaBarPage = document.getElementsByClassName('dnn-persona-bar-page')[0];
				logger.error(`setConfirmationDialogPosition(): confirmation → ${confirmation}`, { "event.action": "setConfirmationDialogPosition" });
				logger.error(`setConfirmationDialogPosition(): personaBarPage → ${personaBarPage}`, { "event.action": "setConfirmationDialogPosition" });
                var condition = personaBarPage.classList.contains('full-width')
                confirmation.classList.toggle("confirmation-dialog-full-width-center", condition);
				logger.error(`setConfirmationDialogPosition(): condition → ${condition}`, { "event.action": "setConfirmationDialogPosition" });
            },


            openSocialTasks: function openTaskWindow(){
				logger.error('>>> Entering openTaskWindow()', { "event.action": "openTaskWindow" });
                 var taskWindow = $('.socialtasks');
                 taskWindow.css({visibility:'visible'});
				logger.error(`openTaskWindow(): taskWindow → ${taskWindow}`, { "event.action": "openTaskWindow" });
            },

            closeSocialTasks: function closeTaskWindow() {
				logger.error('>>> Entering closeTaskWindow()', { "event.action": "closeTaskWindow" });
                 var taskWindow = $('.socialtasks')
                 taskWindow.css({visibility:'hidden'});
				logger.error(`closeTaskWindow(): taskWindow → ${taskWindow}`, { "event.action": "closeTaskWindow" });
            },

            expandPersonaBarPage: function expandPersonaBar(){
				logger.error('>>> Entering expandPersonaBar()', { "event.action": "expandPersonaBar" });
                var personaBarPage = $(".dnn-persona-bar-page")
                personaBarPage.css({width:"1159px"});
				logger.error(`expandPersonaBar(): personaBarPage → ${personaBarPage}`, { "event.action": "expandPersonaBar" });
            },

            contractPersonaBarPage: function contractPersonaBar(){
				logger.error('>>> Entering contractPersonaBar()', { "event.action": "contractPersonaBar" });
                var personaBarPage = $('.dnn-persona-bar-page');
                personaBarPage.css({width: cachedPersonaBarPageWidth+'px'});
				logger.error(`contractPersonaBar(): personaBarPage → ${personaBarPage}`, { "event.action": "contractPersonaBar" });
            },

            closePersonaBar: function handleClosePersonarBar(callback, keepSelection) {
				logger.error(`>>> Entering handleClosePersonarBar(callback=${callback},keepSelection=${keepSelection})`, { "event.action": "handleClosePersonarBar", "logrite.callback": callback, "logrite.keepSelection": keepSelection });
                var self = this;

                if ($personaBarPlaceholder.is(":hidden")) {
                    if (typeof callback === 'function') {
                        callback();
                    }

                    return;
                }

                if (keepSelection) {
                    $('.btn_panel.selected, .hovermenu > ul > li.selected').removeClass('selected').addClass('pending');
                    $('.btn_panel, .hovermenu > ul > li').removeClass('selected');
                } else {
                    $('.btn_panel, .hovermenu > ul > li').removeClass('selected pending');
                    self.panelViewData(null, null);
                }

                parentBody.style.removeProperty('overflow');
                body.style.overflow = "hidden";

                inAnimation = true;
                $personaBarPlaceholder.hide();
                self.leaveCustomModules();
                var $activePanel = $('#' + utility.getPanelIdFromPath(activePath));
                $activePanel.animate({ left: -860 }, 189, 'linear', function () {
                    $('.socialpanel').css({ left: -860 }).hide();
                    $mask.animate({
                        opacity: 0.0
                    }, 200, function () {
                        $iframe.width(personaBarMenuWidth);

                        // for mobile pad device...
                        if (onTouch) {
                            iframe.style["min-width"] = "0";
                            iframe.style.position = "fixed";
                        }

                        $mask.css("display", "none");
                        $showSiteButton.hide();
                        activePath = null;
                        inAnimation = false;
                        $(document).unbind('keyup');

                        eventEmitter.emitClosePanelEvent();

                        if (typeof callback === 'function') {
                            callback();
                        }
                    });
                });

                if (!keepSelection) {
                    saveUserSetting({
                        expandPersonaBar: false
                    });
                }
				logger.warn(`<<< Exiting handleClosePersonarBar(callback=${callback},keepSelection=${keepSelection})`, { "event.action": "handleClosePersonarBar", "logrite.callback": callback, "logrite.keepSelection": keepSelection });
            },
            loadPanel: function handleLoadPanel(identifier, params) {
				logger.error(`>>> Entering handleLoadPanel(identifier=${identifier},params=${params})`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });
                if (inAnimation) return;

                var $menuItem = $('ul.personabarnav').find('[id="' + identifier + '"]');
                if ($menuItem.length === 0) {
                    return;
                }

                if (!params.identifier) {
                    params.identifier = identifier;
					logger.debug(`handleLoadPanel(identifier=${identifier},params=${params}): params.identifier → ${params.identifier}`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });
                }

                if (!params.settings) {
                    params.settings = util.findMenuSettings(params.identifier);
                } else {
                    params.settings = $.extend({}, util.findMenuSettings(params.identifier), params.settings);
                }

                if (!params.moduleName) {
                    params.moduleName = $menuItem.data('module-name');
                }

                if (!params.folderName) {
                    params.folderName = $menuItem.data('folder-name');
                }

                if (!params.query) {
                    params.query = $menuItem.data('query');
                }

                if (!params.path) {
                    params.path = $menuItem.data('path');
                }

                var self = this;
                var path = params.path;
                var moduleName = params.moduleName;
                var folderName = params.folderName || identifier;

                if (activePath === path && activemodule === moduleName) {
                    return;
                }
                $showSiteButton.hide();
                var $menuItems = $(".btn_panel");
                var $hoverMenuItems = $(".hovermenu > ul > li");

                $menuItems.removeClass('selected pending');
                $hoverMenuItems.removeClass('selected pending');

                var $btn = $(".btn_panel[id='" + identifier + "']");
                $btn.addClass('selected');

                var hoverMenuItemSelector = ".hovermenu > ul > li[id='" + identifier + "']";
				logger.error(`handleLoadPanel(identifier=${identifier},params=${params}): hoverMenuItemSelector → ${hoverMenuItemSelector}`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });
                if (moduleName) {
                    hoverMenuItemSelector += "[data-module-name='" + moduleName + "']";
                } else {
                    hoverMenuItemSelector += ":not([data-module-name])";
                }
                $(hoverMenuItemSelector)
                    .addClass('selected')
                    .closest('.btn_panel').addClass('selected');

                var panelId = utility.getPanelIdFromPath(path);
				logger.error(`handleLoadPanel(identifier=${identifier},params=${params}): panelId → ${panelId}`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });
                var $panel = $('#' + panelId);
                if ($panel.length === 0) {
                    $panel = $("<div class='socialpanel' id='" + panelId + "'></div>");
                    $personaBarPanels.append($panel);
                }
                var template = path;
                var loaded = self.loaded(template);
				logger.error(`handleLoadPanel(identifier=${identifier},params=${params}): loaded → ${loaded}`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });

                if ($mask.css("display") === 'none') {
                    activePath = path;
                    activemodule = moduleName;
                    eventEmitter.emitOpenPanelEvent();

                    iframe.style.width = "100%";
                    parentBody.style.overflow = "hidden";
                    body.style.overflow = 'auto';

                    // for mobile pad device...
                    if (onTouch) {
                        iframe.style["min-width"] = "1245px";
                        iframe.style.position = "fixed";
                    }

                    $mask.css("display", "block");
                    inAnimation = true;
                    $mask.animate({
                        opacity: 0.85
                    }, 200, function () {
                        $panel.show().delay(100).animate({ left: personaBarMenuWidth }, 189, 'linear', function () {
                            inAnimation = false;
                            $personaBarPlaceholder.show();
                            self.loadTemplate(folderName, template, $panel, params, function () {
                                self.panelLoaded(params, loaded);
                            });
                            $(document).keyup(function (e) {
                                if (e.keyCode === 27) {
                                    e.preventDefault();
                                    if (!window.dnn.stopEscapeFromClosingPB) {
                                        util.closePersonaBar(null, true);
                                    }
                                }
                            });
                        });
                    });

                    saveUserSetting({
                        expandPersonaBar: true,
                        activeIdentifier: identifier
                    });

                } else {
                    if (activePath !== path) {
                        inAnimation = true;
                        var $activePanel = $('#' + util.getPanelIdFromPath(activePath));
                        $activePanel.fadeOut("fast", function handleHideCurrentPanel() {
							logger.error(`>>> Entering handleHideCurrentPanel(activePath=${activePath})`, { "event.action": "handleHideCurrentPanel", "logrite.activePath": activePath });
                            $panel.css({ left: personaBarMenuWidth }).fadeIn("fast", function handleShowSelectedPanel() {
							logger.error(`>>> Entering handleShowSelectedPanel(personaBarMenuWidth=${personaBarMenuWidth})`, { "event.action": "handleShowSelectedPanel", "logrite.personaBarMenuWidth": personaBarMenuWidth });

                                activePath = path;
                                activemodule = moduleName;
                                inAnimation = false;
                                self.loadTemplate(folderName, template, $panel, params, function () {
                                    self.panelLoaded(params, loaded);
                                });
								logger.warn(`<<< Exiting handleShowSelectedPanel(personaBarMenuWidth=${personaBarMenuWidth})`, { "event.action": "handleShowSelectedPanel", "logrite.personaBarMenuWidth": personaBarMenuWidth });

                                saveUserSetting({
                                    expandPersonaBar: true,
                                    activeIdentifier: identifier
                                });
                            });
                        });
                    } else if (activemodule !== moduleName) {
                        activemodule = moduleName;
                        self.loadTemplate(folderName, template, $panel, params, function () {
                            self.panelLoaded(params, loaded);
                        });
                    }
                }
                setCloseButtonClass(panelId);
				logger.warn(`<<< Exiting handleLoadPanel(identifier=${identifier},params=${params})`, { "event.action": "handleLoadPanel", "logrite.identifier": identifier, "logrite.params": params });
            },
            panelLoaded: function (params, loaded) {
				logger.error(`>>> Entering panelLoaded(params=${params},loaded=${loaded})`, { "event.action": "panelLoaded", "logrite.params": params, "logrite.loaded": loaded });
                extension.load(util, params);
                this.loadCustomModules();

                if (params.handleTabViewInModule !== true && loaded === false) {
                    var panelId = util.getPanelIdFromPath(params.path);
                    util.updatePanelTabView(panelId);
					logger.error(`panelLoaded(params=${params},loaded=${loaded}): panelId → ${panelId}`, { "event.action": "panelLoaded", "logrite.params": params, "logrite.loaded": loaded });
                }
            },
            initCustomModules: function (callback) {
				logger.error(`>>> Entering initCustomModules(callback=${callback})`, { "event.action": "initCustomModules", "logrite.callback": callback });
                if (config.customModules && config.customModules.length > 0) {
                    var self = this;
                    for (var i = 0; i < config.customModules.length; i++) {
                        (function (index) {
							logger.warn(`<<< Exiting initCustomModules(callback=${callback})`, { "event.action": "initCustomModules", "logrite.callback": callback });
                            var path = '../' + config.customModules[index];
							logger.error(`initCustomModules(callback=${callback}): path → ${path}`, { "event.action": "initCustomModules", "logrite.callback": callback });
                            require([path], function (module) {
                                customModules.push(module);
                                if (typeof module.init === "function") {
                                    module.init.call(self, util);
                                }

                                if (index === config.customModules.length - 1 && typeof callback === "function") {
                                    callback();
                                }
                            });
                        })(i);

                    }
                } else {
                    callback();
                }
            },
            loadCustomModules: function () {
				logger.error('>>> Entering loadCustomModules()', { "event.action": "loadCustomModules" });
                for (var i = 0; i < customModules.length; i++) {
                    if (typeof customModules[i].load === "function") {
                        customModules[i].load.call(this);
						logger.warn('<<< Exiting loadCustomModules()', { "event.action": "loadCustomModules" });
                    }
                }
            },
            leaveCustomModules: function () {
				logger.error('>>> Entering leaveCustomModules()', { "event.action": "leaveCustomModules" });
                for (var i = 0; i < customModules.length; i++) {
                    if (typeof customModules[i].leave === "function") {
                        customModules[i].leave.call(this);
						logger.warn('<<< Exiting leaveCustomModules()', { "event.action": "leaveCustomModules" });
                    }
                }
            },
            findMenuSettings: function(identifier, menuItems) {
				logger.error(`>>> Entering findMenuSettings(identifier=${identifier},menuItems=${menuItems})`, { "event.action": "findMenuSettings", "logrite.identifier": identifier, "logrite.menuItems": menuItems });
                menuItems = menuItems || menuViewModel.menu.menuItems;
                var settings = null;
				logger.error(`findMenuSettings(identifier=${identifier},menuItems=${menuItems}): settings → ${settings}`, { "event.action": "findMenuSettings", "logrite.identifier": identifier, "logrite.menuItems": menuItems });
                for (var i = 0; i < menuItems.length; i++) {
                    var menuItem = menuItems[i];
                    if (typeof menuItem.length === "number" && menuItem.length > 0) {
                        settings = this.findMenuSettings(identifier, menuItem);
                        if (settings) {
                            break;
                        }
                    } else {
                        if (menuItem.id === identifier) {
                            if (menuItem.settings) {
                                var defaultSettings = { isAdmin: config.isAdmin, isHost: config.isHost };
                                settings = $.extend({}, defaultSettings, JSON.parse(menuItem.settings));
                            } else {
                                settings = {};
                            }
                        } else if (typeof menuItem.menuItems !== "undefined" && menuItem.menuItems.length > 0) {
                            settings = this.findMenuSettings(identifier, menuItem.menuItems);
                        }

                        if (settings) {
                            break;
                        }
                    }
                }

				logger.warn(`<<< Exiting findMenuSettings(identifier=${identifier},menuItems=${menuItems})`, { "event.action": "findMenuSettings", "logrite.identifier": identifier, "logrite.menuItems": menuItems });
                return settings;
            },
            updateMenuSettings: function(identifier, settings, menuItems) {
				logger.error(`>>> Entering updateMenuSettings(identifier=${identifier},settings=${settings},menuItems=${menuItems})`, { "event.action": "updateMenuSettings", "logrite.identifier": identifier, "logrite.settings": settings, "logrite.menuItems": menuItems });
                menuItems = menuItems || menuViewModel.menu.menuItems;
                for (var i = 0; i < menuItems.length; i++) {
                    var menuItem = menuItems[i];
                    if (typeof menuItem.length === "number" && menuItem.length > 0) {
                        this.updateMenuSettings(identifier, settings, menuItem);
                    } else {
                        if (menuItem.id === identifier) {
                            menuItem.settings = JSON.stringify(settings);
                        } else if (typeof menuItem.menuItems !== "undefined" && menuItem.menuItems.length > 0) {
                            this.updateMenuSettings(identifier, settings, menuItem.menuItems);
							logger.warn(`<<< Exiting updateMenuSettings(identifier=${identifier},settings=${settings},menuItems=${menuItems})`, { "event.action": "updateMenuSettings", "logrite.identifier": identifier, "logrite.settings": settings, "logrite.menuItems": menuItems });
                        }
                    }
                }
            },
            /**
             * in case path is an array, it is expected to be a sorted list of dependent sources.
             *
             * @param path string|array
             */
            loadBundleScript: function (path) {
			logger.error(`>>> Entering loadBundleScript(path=${path})`, { "event.action": "loadBundleScript", "logrite.path": path });

                var urls = path;
                if(Array.isArray(urls) === false) {
                    urls = [path];
					logger.warn(`<<< Exiting loadBundleScript(path=${path})`, { "event.action": "loadBundleScript", "logrite.path": path });
                }
                function ajax(urls, build) {
					logger.error(`>>> Entering ajax(urls=${urls},build=${build})`, { "event.action": "ajax", "logrite.urls": urls, "logrite.build": build });
                    if(urls.length == 0) {
                        return;
                    }
                    $.ajax({
                        dataType: "script",
                        cache: true,
                        data: {
                            cdv: build
                        },
                        url: urls.pop(),
                        complete: function() {
							logger.error('>>> Entering complete()', { "event.action": "complete" });
                            ajax(urls, build);
							logger.warn('<<< Exiting complete()', { "event.action": "complete" });
                        }
                    });
					logger.warn(`<<< Exiting ajax(urls=${urls},build=${build})`, { "event.action": "ajax", "logrite.urls": urls, "logrite.build": build });
                }

                ajax(urls.reverse(), config.buildNumber);
            },
            panelViewData: function (panelId, viewData) {
				logger.error(`>>> Entering panelViewData(panelId=${panelId},viewData=${viewData})`, { "event.action": "panelViewData", "logrite.panelId": panelId, "logrite.viewData": viewData });
                var localStorageAllowed = function () {
					logger.error('>>> Entering localStorageAllowed()', { "event.action": "localStorageAllowed" });
                    var mod = 'DNN_localStorageTEST';
                    try {
						logger.error(`localStorageAllowed(): mod → ${mod}`, { "event.action": "localStorageAllowed" });
                        window.localStorage.setItem(mod, mod);
                        window.localStorage.removeItem(mod);
                        return true;
                    } catch (e) {
						logger.error(`Exception in panelViewData(panelId=${panelId},viewData=${viewData}): ${e.message}`, e, { "event.action": "panelViewData", "event.category": "process", "logrite.panelId": panelId, "logrite.viewData": viewData });
						logger.warn('<<< Exiting localStorageAllowed()', { "event.action": "localStorageAllowed" });
                        return false;
                    }
                };

                if (!localStorageAllowed()) {
                    return {};
                }

                var cacheKey = "DNN_PB_PANEL_VIEW";
				logger.error(`panelViewData(panelId=${panelId},viewData=${viewData}): cacheKey → ${cacheKey}`, { "event.action": "panelViewData", "logrite.panelId": panelId, "logrite.viewData": viewData });
                var savedData = window.localStorage[cacheKey];
                if (!savedData) {
                    savedData = {};
                } else {
                    savedData = JSON.parse(savedData);
                }

                if (typeof viewData !== "undefined") {
                    if (panelId === null && viewData === null) {
                        window.localStorage.removeItem(cacheKey);
                        savedData = {};
                    } else {
                        if (panelId === null) {
                            savedData = viewData;
                        } else {
                            savedData[panelId] = viewData;
                        }
                        window.localStorage.setItem(cacheKey, JSON.stringify(savedData));
                    }
                }

				logger.warn(`<<< Exiting panelViewData(panelId=${panelId},viewData=${viewData})`, { "event.action": "panelViewData", "logrite.panelId": panelId, "logrite.viewData": viewData });
                return panelId ? savedData[panelId] : savedData;
            },
            savePanelTabView: function(panelId) {
				logger.error(`>>> Entering savePanelTabView(panelId=${panelId})`, { "event.action": "savePanelTabView", "logrite.panelId": panelId });
                var $panel = $('#' + panelId);
                var $primaryTabs = $panel.find('.dnn-tabs.primary, .ui-tabs').eq(0).find('> ul > li');
                if (!$primaryTabs.length) {
                    $primaryTabs = $panel.find('.dnn-tabs.secondary').eq(0).find('> ul > li');
                }
                var $primarySelected = $primaryTabs.parent().find('>li[aria-selected="true"],>li[class*="selected"]');
                if ($primarySelected.length) {
                    var primaryIndex = $primaryTabs.index($primarySelected);
					logger.error(`savePanelTabView(panelId=${panelId}): primaryIndex → ${primaryIndex}`, { "event.action": "savePanelTabView", "logrite.panelId": panelId });

                    var viewData = [primaryIndex];
                    var $primaryPanel = $('#' + $primarySelected.attr('aria-controls'));
                    if ($primaryPanel.length) {

                        var $secondaryTabs = $primaryPanel.find('.dnn-tabs.secondary').eq(0).find('> ul > li');
                        var $secondarySelected = $secondaryTabs.parent().find('>li[aria-selected="true"],>li[class*="selected"]');
                        if ($secondarySelected.length) {
                            var secondaryIndex = $secondaryTabs.index($secondarySelected);
							logger.error(`savePanelTabView(panelId=${panelId}): secondaryIndex → ${secondaryIndex}`, { "event.action": "savePanelTabView", "logrite.panelId": panelId });

                            viewData.push(secondaryIndex);
                        }
                    }

                    util.panelViewData(panelId, { tab: viewData });
					logger.warn(`<<< Exiting savePanelTabView(panelId=${panelId})`, { "event.action": "savePanelTabView", "logrite.panelId": panelId });
                }
            },
            updatePanelTabView: function (panelId) {
				logger.error(`>>> Entering updatePanelTabView(panelId=${panelId})`, { "event.action": "updatePanelTabView", "logrite.panelId": panelId });
                var viewData = (util.panelViewData(panelId) || {}).tab;
                if (!viewData || !viewData.length) {
				logger.error(`updatePanelTabView(panelId=${panelId}): viewData → ${viewData}`, { "event.action": "updatePanelTabView", "logrite.panelId": panelId });
                    return;
                }

                var sleep = function(timeout) {
					logger.warn(`<<< Exiting updatePanelTabView(panelId=${panelId})`, { "event.action": "updatePanelTabView", "logrite.panelId": panelId });
                    setTimeout(function() {
						logger.error(`>>> Entering sleep(timeout=${timeout})`, { "event.action": "sleep", "logrite.timeout": timeout });
						logger.warn(`<<< Exiting sleep(timeout=${timeout})`, { "event.action": "sleep", "logrite.timeout": timeout });
                        util.updatePanelTabView(panelId);
                    }, timeout);
                }

                var $panel = $('#' + panelId);
                var $primaryTab = $panel.find('.dnn-tabs.primary,.ui-tabs').eq(0).find('> ul > li').eq(viewData[0]);
                if (!$primaryTab.length) {
                    $primaryTab = $panel.find('.dnn-tabs.secondary').eq(0).find('> ul > li').eq(viewData[0]);
                }

                if (!$primaryTab.length) {
                    sleep(50);
                    return;
                }

                if ($primaryTab.attr('aria-selected') !== "true" && $primaryTab.attr('class').indexOf('selected') === -1) {
                    if ($primaryTab.find('a').length) {
                        $primaryTab.find('a').trigger('click', [true]);
                    } else {
                        $primaryTab.trigger('click', [true]);
                    }
                }

                if (viewData.length > 1) {
                    var $primaryPanel = $('#' + $primaryTab.attr('aria-controls'));
                    var $secondaryTab = $primaryPanel.find('.dnn-tabs.secondary').eq(0).find('> ul > li').eq(viewData[1]);
                    if (!$secondaryTab.length) {
                        sleep(50);
                    }

                    $secondaryTab.trigger('click', [true]);
                }
            }
        };
        util = $.extend(util, utility);
        // end define util

        function setCloseButtonClass(id) {
			logger.error(`>>> Entering setCloseButtonClass(id=${id})`, { "event.action": "setCloseButtonClass", "logrite.id": id });
            var panel = document.querySelector('#' + id + '>div');
            if (panel != null && panel.innerHTML !== "") {
				logger.error(`setCloseButtonClass(id=${id}): panel → ${panel}`, { "event.action": "setCloseButtonClass", "logrite.id": id });
                var page = panel.querySelector('.dnn-persona-bar-page');
                if (page != null && page.classList.contains('full-width')) {
                    $showSiteButton.addClass('full-width-mode');
                }
                else {
                    $showSiteButton.removeClass();
                }
                $showSiteButton.show();
				logger.error(`setCloseButtonClass(id=${id}): page → ${page}`, { "event.action": "setCloseButtonClass", "logrite.id": id });
				logger.warn(`<<< Exiting setCloseButtonClass(id=${id})`, { "event.action": "setCloseButtonClass", "logrite.id": id });
                return;
            }
            else {
                setTimeout(function () {
                    setCloseButtonClass(id);
                }, 100);
            }
        }

        function onShownPersonaBar() {
			logger.error('>>> Entering onShownPersonaBar()', { "event.action": "onShownPersonaBar" });
            (function handleResizeWindow() {
				logger.error('>>> Entering handleResizeWindow()', { "event.action": "handleResizeWindow" });
                var evt = document.createEvent('HTMLEvents');
                evt.initEvent('resize', true, false);
				logger.error(`handleResizeWindow(): evt → ${evt}`, { "event.action": "handleResizeWindow" });
                window.top.dispatchEvent(evt);
				logger.warn('<<< Exiting handleResizeWindow()', { "event.action": "handleResizeWindow" });
            })();

            (function ($) {
                $(parent.document).trigger('personabar:show');
            }(parent.$));
			logger.warn('<<< Exiting onShownPersonaBar()', { "event.action": "onShownPersonaBar" });
        }

        function checkMenuLink($menu) {
			logger.error(`>>> Entering checkMenuLink($menu=${$menu})`, { "event.action": "checkMenuLink", "logrite.$menu": $menu });
            var href = $menu.attr('href');
            if (href) {
                if (href.indexOf('://') > -1) {
                    window.open(href);
                } else {
                    href = config.siteRoot + href;
                    util.closePersonaBar(function () {
                        window.top.location.href = href;
                    });
                }
                return true;
            }

            return false;
        }

        function calculateHoverMenuPosition($menu) {
			logger.error(`>>> Entering calculateHoverMenuPosition($menu=${$menu})`, { "event.action": "calculateHoverMenuPosition", "logrite.$menu": $menu });
            if ($menu.length === 0) {
                return;
            }

            var bottom = $menu.parent().offset().top + $menu.outerHeight();
			logger.error(`calculateHoverMenuPosition($menu=${$menu}): bottom → ${bottom}`, { "event.action": "calculateHoverMenuPosition", "logrite.$menu": $menu });
            var availableArea = $(window).height() + $(window).scrollTop() - bottom;
            $menu.css('top', availableArea < 0 ? availableArea + 'px' : '');
			logger.error(`calculateHoverMenuPosition($menu=${$menu}): availableArea → ${availableArea}`, { "event.action": "calculateHoverMenuPosition", "logrite.$menu": $menu });
        }

        function getSiteRoot() {
			logger.error('>>> Entering getSiteRoot()', { "event.action": "getSiteRoot" });
			logger.warn('<<< Exiting getSiteRoot()', { "event.action": "getSiteRoot" });
            return config.siteRoot || "/";
        }

        function saveBtnEditSettings(success, error) {
			logger.error(`>>> Entering saveBtnEditSettings(success=${success},error=${error})`, { "event.action": "saveBtnEditSettings", "logrite.success": success, "logrite.error": error });
            saveUserSetting({
                expandPersonaBar: false,
                activePath: null,
                activeIdentifier: null
            }, success, error);
			logger.warn(`<<< Exiting saveBtnEditSettings(success=${success},error=${error})`, { "event.action": "saveBtnEditSettings", "logrite.success": success, "logrite.error": error });
        }

        function saveUserSetting(settings, success, error) {
			logger.error(`>>> Entering saveUserSetting(settings=${settings},success=${success},error=${error})`, { "event.action": "saveUserSetting", "logrite.settings": settings, "logrite.success": success, "logrite.error": error });
            util.persistent.save(settings, success, error);
			logger.warn(`<<< Exiting saveUserSetting(settings=${settings},success=${success},error=${error})`, { "event.action": "saveUserSetting", "logrite.settings": settings, "logrite.success": success, "logrite.error": error });
        }

        function inLockEditMode() {
			logger.error('>>> Entering inLockEditMode()', { "event.action": "inLockEditMode" });
            if (typeof window.top.dnn != "undefined") {
                return window.top.dnn.dom.getCookie('StayInEditMode') === "YES";
            }

			logger.warn('<<< Exiting inLockEditMode()', { "event.action": "inLockEditMode" });
            return false;
        }

        function setLockEditMode(locked) {
			logger.error(`>>> Entering setLockEditMode(locked=${locked})`, { "event.action": "setLockEditMode", "logrite.locked": locked });
            if (typeof window.top.dnn != "undefined") {
                window.top.dnn.dom.setCookie('StayInEditMode', locked ? "YES" : "NO", '', getSiteRoot());

                updateLockModeTooltip(locked);
				logger.warn(`<<< Exiting setLockEditMode(locked=${locked})`, { "event.action": "setLockEditMode", "logrite.locked": locked });
            }
        }

        function updateLockModeTooltip(locked) {
			logger.error(`>>> Entering updateLockModeTooltip(locked=${locked})`, { "event.action": "updateLockModeTooltip", "logrite.locked": locked });
            var $btnEdit = $("#Edit.btn_panel");
            var title, message;
            if (locked) {
                title = util.resx.PersonaBar["UnlockEditMode"];
				logger.error(`updateLockModeTooltip(locked=${locked}): title → ${title}`, { "event.action": "updateLockModeTooltip", "logrite.locked": locked });
                message = util.resx.PersonaBar["UnlockEditMode.Help"];
            } else {
                title = util.resx.PersonaBar["LockEditMode"];
                message = util.resx.PersonaBar["LockEditMode.Help"];
            }

            $btnEdit.find(".editmode-tooltip > span").fadeOut('fast', '', function() {
                $btnEdit.find('.tooltip-title').html(title);
                $btnEdit.find('.tooltip-message').html(message);

                $btnEdit.find(".editmode-tooltip > span").fadeIn('fast');
            });

            if (locked) {
                $btnEdit.removeClass('unlocked').addClass('locked');
            } else {
                $btnEdit.removeClass('locked').addClass('unlocked');
				logger.warn(`<<< Exiting updateLockModeTooltip(locked=${locked})`, { "event.action": "updateLockModeTooltip", "logrite.locked": locked });
            }
        }

        function handleDisabledEditBar($btnEdit) {
		logger.error(`>>> Entering handleDisabledEditBar($btnEdit=${$btnEdit})`, { "event.action": "handleDisabledEditBar", "logrite.$btnEdit": $btnEdit });

            $btnEdit.addClass('disabled');

            var $tooltip = $('<div class="editmode-tooltip"><span class="tooltip-title"></span><span class="tooltip-message"></span></div>');
            $tooltip.click(function (e) {
                e.preventDefault();
                e.stopImmediatePropagation();
            });
            $btnEdit.append($tooltip);

            var title = util.resx.PersonaBar["DisableEditBar"];
			logger.error(`handleDisabledEditBar($btnEdit=${$btnEdit}): title → ${title}`, { "event.action": "handleDisabledEditBar", "logrite.$btnEdit": $btnEdit });
            var message = util.resx.PersonaBar["DisableEditBar.Help"];
			logger.error(`handleDisabledEditBar($btnEdit=${$btnEdit}): message → ${message}`, { "event.action": "handleDisabledEditBar", "logrite.$btnEdit": $btnEdit });

            $btnEdit.find(".editmode-tooltip > span").fadeOut('fast', '', function () {
                $btnEdit.find('.tooltip-title').html(title);
                $btnEdit.find('.tooltip-message').html(message);

                $btnEdit.find(".editmode-tooltip > span").fadeIn('fast');
            });
			logger.warn(`<<< Exiting handleDisabledEditBar($btnEdit=${$btnEdit})`, { "event.action": "handleDisabledEditBar", "logrite.$btnEdit": $btnEdit });
        }

        function handleLockEditState($btnEdit) {
			logger.error(`>>> Entering handleLockEditState($btnEdit=${$btnEdit})`, { "event.action": "handleLockEditState", "logrite.$btnEdit": $btnEdit });
            var $tooltip = $('<div class="editmode-tooltip"><span class="tooltip-title"></span><span class="tooltip-message"></span></div>');
            $tooltip.click(function(e) {
                e.preventDefault();
                e.stopImmediatePropagation();
            });
            $btnEdit.append($tooltip);
            var lockEdit = inLockEditMode();
			logger.error(`handleLockEditState($btnEdit=${$btnEdit}): lockEdit → ${lockEdit}`, { "event.action": "handleLockEditState", "logrite.$btnEdit": $btnEdit });
            updateLockModeTooltip(lockEdit);


            $btnEdit.on('click', function handleEdit() {
				logger.error('>>> Entering handleEdit()', { "event.action": "handleEdit" });
                setLockEditMode(!inLockEditMode());
                util.closePersonaBar(saveBtnEditSettings);
            });

            eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
				logger.error('>>> Entering handleClosingPersonaBar()', { "event.action": "handleClosingPersonaBar" });
                saveBtnEditSettings();
            });
        }

        function handleTabSelection (callback) {
			logger.error(`>>> Entering handleTabSelection(callback=${callback})`, { "event.action": "handleTabSelection", "logrite.callback": callback });
            $('#personabar-panels').on('click',
                '> .socialpanel .dnn-tabs.primary > ul > li,' +
                '> .socialpanel .dnn-tabs.secondary > ul > li,' +
                '> .socialpanel .ui-tabs > ul > li > a', function (e, byScript) {
                if (byScript) {
                    return;
                }

				logger.warn(`<<< Exiting handleTabSelection(callback=${callback})`, { "event.action": "handleTabSelection", "logrite.callback": callback });
                var panelId = $(this).parents('.socialpanel').attr('id');
				logger.error(`handleTabSelection(callback=${callback}): panelId → ${panelId}`, { "event.action": "handleTabSelection", "logrite.callback": callback });
                setTimeout(function() {
                    util.savePanelTabView(panelId);
                }, 0);
            });

            callback();
        }

        util.asyncParallel([
                function (callback) {
                    util.loadResx(function onResxLoaded() {
						logger.error('>>> Entering onResxLoaded()', { "event.action": "onResxLoaded" });
                        var viewModel = {
                            resx: util.resx.PersonaBar,
                            menu: menuViewModel.menu,
                            upToDate: ko.observable(true),
                            updateLink: ko.observable(''),
                            updateCritical: ko.observable(false),
                            logOff: function() {
                                function onLogOffSuccess() {
									logger.error('>>> Entering onLogOffSuccess()', { "event.action": "onLogOffSuccess" });
                                    if (typeof window.top.dnn != "undefined" && typeof window.top.dnn.PersonaBar != "undefined") {
                                        window.top.dnn.PersonaBar.userLoggedOut = true;
                                    }
                                    window.top.document.location.href = window.top.document.location.href;
									logger.warn('<<< Exiting onLogOffSuccess()', { "event.action": "onLogOffSuccess" });
                                };

                                util.sf.rawCall("GET", config.logOff, null, onLogOffSuccess);
                                return;
                            },
                            startLocalUpgrade: function() {
								logger.error('>>> Entering startLocalUpgrade()', { "event.action": "startLocalUpgrade" });
                                util.sf.moduleRoot = 'personabar';
                                util.sf.controller = "serversummary";
                                util.sf.postsilence('StartLocalUpgrade', {}, function onSuccess() {
									logger.error('>>> Entering onSuccess()', { "event.action": "onSuccess" });
                                    window.top.document.location.href = window.top.document.location.href;
                                });
                            }
                        };

                        viewModel.updateText = ko.computed(function() {
                            return util.resx.PersonaBar.Update;
                        });

                        ko.applyBindings(viewModel, document.getElementById('personabar'));

                        iconLoader.load();

                        util.sf.moduleRoot = 'personabar';
                        util.sf.controller = "serversummary";
                        util.sf.getsilence('GetUpdateInfo', {}, function (data) {
                            viewModel.upToDate(data.UpToDate);
                            viewModel.updateLink(data.Url);
                            viewModel.updateCritical(data.Critical);
                        });

                        document.addEventListener("click", function(e) {
                            $('#topLevelMenu .hovermenu').hide();
                        });

                        setTimeout(function () {

                            // Setting for 1024 resolution
                            var width = parent.document.body.clientWidth;
                            if (width <= 1024 && width > 768) {
                                $personaBarPlaceholder.css({ 'width': '700px' });
                                $personaBarPanels.addClass("view-ipad landscape");
                                $personaBar.addClass("view-ipad landscape");
                            } else if (width <= 768) {
                                $personaBarPlaceholder.css({ 'width': '500px' });
                                $personaBarPanels.addClass("view-ipad portrait");
                                $personaBar.addClass("view-ipad portrait");
                            }
                            else {
                                $personaBarPanels.removeClass("view-ipad landscape portrait");
                                $personaBar.removeClass("view-ipad landscape portrait");
                            }

                            if (isTouch) {
                                $('#topLevelMenu .personabarnav > li').click(function () {
                                    var hoverMenuId = $(this).attr('data-hovermenu-id');
                                    $('#topLevelMenu .hovermenu').hide();
                                    if (hoverMenuId) {
                                        var $hoverMenuId = $("#" + hoverMenuId);
                                        $hoverMenuId.toggle();
                                        $iframe.width("100%");
                                    }
                                });
                                $(document).on('touchstart', function (event) {
                                    if (!$(event.target).closest('#topLevelMenu .hovermenu').length) {
                                        $('#topLevelMenu .hovermenu').hide();
                                    }
                                });
                                $body.addClass('touch');
                            } else {
                                $body.addClass('non-touch');
                            }
                            var isMac = navigator.appVersion.indexOf('Mac') > -1;
                            if (isMac) {
                                $body.addClass('mac');
                            }

                            var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
                            if (isSafari) {
                                $body.addClass('safari');
                            }

                            var isIe = (function isInternetExplorer() {
                                if (navigator.appName === 'Microsoft Internet Explorer') return true;
                                if (navigator.appName === 'Netscape') {
									logger.error('>>> Entering isInternetExplorer()', { "event.action": "isInternetExplorer" });
                                    var ua = navigator.userAgent;
                                    if (ua.indexOf('Trident') > -1) return true;
                                }
                                return false;
                            })();
                            if (isIe) {
                                $body.addClass('ie');
                                iframe.style.backgroundColor = "rgba(0,0,0,0.01)"; // IE10 flashing bug
                            }

                            if (config.visible) {


                                (function setupMenu() {
									logger.error('>>> Entering setupMenu()', { "event.action": "setupMenu" });
                                    $(".btn_panel .hovermenu").click(function(e) {
                                        e.stopPropagation();
                                    });

                                    $(".btn_panel, .hovermenu > ul > li").click(function handleClickOnHoverMenuItem(evt) {
										logger.error('>>> Entering handleClickOnHoverMenuItem()', { "event.action": "handleClickOnHoverMenuItem" });
                                        evt.preventDefault();
                                        evt.stopPropagation();

                                        var $this = $(this);

                                        if ($this.hasClass('selected')) {
                                            var panelId = utility.getPanelIdFromPath($this.data('path'));
											logger.error(`handleClickOnHoverMenuItem(): panelId → ${panelId}`, { "event.action": "handleClickOnHoverMenuItem" });
                                            var panelAlreadyOpened = $('#' + panelId + ':not(visible)');
											logger.error(`handleClickOnHoverMenuItem(): panelAlreadyOpened → ${panelAlreadyOpened}`, { "event.action": "handleClickOnHoverMenuItem" });
                                            if (panelAlreadyOpened) {
                                                panelAlreadyOpened.fadeIn('fast');
                                            }
                                            return;
                                        }

                                        var path = $this.data('path');
                                        var params = null;

                                        var identifier = $this.attr('id');
                                        var moduleName = $this.data('module-name');
                                        var folderName = $this.data('folder-name');
                                        var query = $this.data('query');
                                        var settings = util.findMenuSettings(identifier);
										logger.error(`handleClickOnHoverMenuItem(): settings → ${settings}`, { "event.action": "handleClickOnHoverMenuItem" });
                                        if (path === '') {
                                            var menuItems = menuViewModel.menu.menuItems;
                                            for (var i = 0; i < menuItems.length; i++) {
                                                if (menuItems[i].id === identifier) {
                                                    if (menuItems[i].menuItems.length > 0) {
                                                        var subMenu = menuItems[i].menuItems[0][0];
                                                        identifier = subMenu.id;
                                                        moduleName = subMenu.moduleName;
                                                        folderName = subMenu.folderName;
                                                        path = subMenu.path;
                                                        query = subMenu.query;
                                                        settings = util.findMenuSettings(identifier);
                                                    }
                                                }
                                            }
                                        }

                                        if (checkMenuLink($('li[id="' + identifier + '"]'))) {
                                            return;
                                        }

                                        if (!path) return;

                                        if (moduleName !== undefined) {
                                            params = {
                                                moduleName: moduleName,
                                                folderName: folderName,
                                                identifier: identifier,
                                                path: path,
                                                query: query,
                                                settings: settings
                                            };
                                        };

                                        util.loadPanel(identifier, params);

                                        $('.btn_panel > .hovermenu').fadeOut('fast');
                                    });

                                    var $avatarMenu = $('li.useravatar');
                                    if ($avatarMenu.length) {
                                        $avatarMenu.before($showSiteButton);
                                    }

                                    $showSiteButton.click(function handleShowSite(e, keepSelection) {
                                        e.preventDefault();
                                        var needRefresh = $(this).data('need-refresh');
										logger.error(`handleShowSite(needRefresh=${needRefresh}): needRefresh → ${needRefresh}`, { "event.action": "handleShowSite", "logrite.needRefresh": needRefresh });
                                        var needHomeRedirect = $(this).data('need-homeredirect');
										logger.error(`handleShowSite(needRefresh=${needRefresh}): needHomeRedirect → ${needHomeRedirect}`, { "event.action": "handleShowSite", "logrite.needRefresh": needRefresh });
                                        $showSiteButton.hide();
                                        util.closePersonaBar(function() {
                                            if (needHomeRedirect) {
                                                window.top.location.href = config.siteRoot;
                                            } else {
                                                if (needRefresh) {
                                                    window.top.location.reload();
                                                }
                                            }
                                        }, keepSelection);
                                    });
                                }());

                                (function setupHoverMenu() {
								logger.error('>>> Entering setupHoverMenu()', { "event.action": "setupHoverMenu" });


                                    var showMenuHandlers = [];
                                    var leaveSubMenuHandlers = [];
                                    var mouseOnHovermenu = false;
                                    var mouseOnButton = false;

                                    var resetHandlers = function() {
										logger.error('>>> Entering resetHandlers()', { "event.action": "resetHandlers" });
                                        if (showMenuHandlers.length > 0) {
                                            $.each(showMenuHandlers, function(index, item) {
                                                clearTimeout(item);
                                            });
                                            showMenuHandlers = [];
                                        }

                                        if (leaveSubMenuHandlers.length > 0) {
                                            $.each(leaveSubMenuHandlers, function(index, item) {
                                                clearTimeout(item);
                                            });
                                            leaveSubMenuHandlers = [];
											logger.warn('<<< Exiting resetHandlers()', { "event.action": "resetHandlers" });
                                        }
                                    };

                                    $('.btn_panel').each(function () {
                                        var $this = $(this);
                                        var hoverMenuId = $this.data('hovermenu-id');
                                        if (hoverMenuId === undefined) return;

                                        var $hoverMenu = $('#' + hoverMenuId);
                                        $this.hover(function () {
                                            mouseOnButton = true;
                                            if ($hoverMenu.css('display') === 'none' || $this.find('> div').length > 0) {
                                                resetHandlers();

                                                showMenuHandlers.push(setTimeout(function () {
                                                    if (($hoverMenu.css('display') === 'none' || $this.find('> div').length > 0) && mouseOnButton) {
                                                        if (!activePath) iframe.style.width = "100%";

                                                        $hoverMenu.css({
                                                            position: 'absolute',
                                                            left: '-1000px'
                                                        });

                                                        $('.btn_panel').each(function () {
                                                            var hoverMenuId = $(this).data('hovermenu-id');
                                                            if (hoverMenuId === undefined) return;

                                                            $('#' + hoverMenuId).hide();
                                                        });

                                                        // Set aria-expanded to true when menu is shown
                                                        $hoverMenu.show();
                                                        $this.attr('aria-expanded', 'true');

                                                        // Fix ie personabar hover menús
                                                        showMenuHandlers.push(setTimeout(function () {
                                                            $('.hovermenu > ul').css('list-style-type', 'square');
                                                            showMenuHandlers.push(setTimeout(function () {
                                                                $('.hovermenu > ul').css('list-style-type', 'none');
                                                                showMenuHandlers.push(setTimeout(function () {
                                                                    $hoverMenu.hide();
                                                                    $hoverMenu.removeAttr('style');
                                                                    calculateHoverMenuPosition($hoverMenu);
                                                                    showMenuHandlers.push(setTimeout(function () {
                                                                        $hoverMenu.fadeIn('fast');
                                                                    }));

                                                                }, 100));
                                                            }));
                                                        }));

                                                    }
                                                }, 50));
                                            }
                                        }, function () {
                                            mouseOnButton = false;
											logger.warn('<<< Exiting setupHoverMenu()', { "event.action": "setupHoverMenu" });
                                            if (($hoverMenu.css('display') == 'block' || $this.find('> div').length > 0) && !mouseOnHovermenu) {
                                                setTimeout(function () {
                                                    if (($hoverMenu.css('display') == 'block' || $this.find('> div').length > 0) && !mouseOnButton && !mouseOnHovermenu) {
                                                        if (!activePath) {
                                                            $iframe.width(personaBarMenuWidth);
                                                        }
                                                        // Set aria-expanded to false when menu is hidden
                                                        $hoverMenu.hide();
                                                        $this.attr('aria-expanded', 'false');

                                                        resetHandlers();
                                                    }
                                                }, 50);
                                            }
                                        });
                                    });

                                    $(".hovermenu").each(function () {
                                        var $this = $(this);

                                        $this.hover(function () {
                                            mouseOnHovermenu = true;
                                        }, function () {
                                            mouseOnHovermenu = false;
                                            if ($this.css('display') === 'block' && !mouseOnButton) {
                                                leaveSubMenuHandlers.push(setTimeout(function () {
                                                    if ($this.css('display') === 'block' && !mouseOnButton && !mouseOnHovermenu) {
                                                        if (!activePath) {
                                                            $iframe.width(personaBarMenuWidth);
                                                        }
                                                        $this.hide();

                                                        resetHandlers();
                                                    }
                                                }, 800));
                                            }
                                        });
                                    });
                                })();
                            } else {
                                $(".personabarnav > li.btn_panel, .hovermenu > ul > li").addClass("disabled");
                            }

                            (function setupEditButton() {
								logger.error('>>> Entering setupEditButton()', { "event.action": "setupEditButton" });
                                var $btnEdit = $("#Edit.btn_panel");
                                if (!config.visible) {
                                    return;
                                }

                                if (disableEditBar) {
                                    handleDisabledEditBar($btnEdit);
                                    return;
                                }

                                eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
                                    $btnEdit.show();
                                });
                                eventEmitter.addPanelOpenEventListener(function handleOpeningPersonaBar() {
									logger.error('>>> Entering handleOpeningPersonaBar()', { "event.action": "handleOpeningPersonaBar" });
                                    $btnEdit.hide();
                                });

                                if (config.userMode !== 'Edit') {
                                    $btnEdit.on('click', function handleEdit() {
                                        function toogleUserMode(mode, successCallback) {
											logger.error(`>>> Entering toogleUserMode(mode=${mode},successCallback=${successCallback})`, { "event.action": "toogleUserMode", "logrite.mode": mode, "logrite.successCallback": successCallback });
                                            util.sf.moduleRoot = 'internalservices';
                                            util.sf.controller = "controlBar";
                                            util.sf.post('ToggleUserMode', { UserMode: mode }, successCallback);
											logger.warn(`<<< Exiting toogleUserMode(mode=${mode},successCallback=${successCallback})`, { "event.action": "toogleUserMode", "logrite.mode": mode, "logrite.successCallback": successCallback });
                                        };
                                        util.closePersonaBar(function () {
                                            toogleUserMode('EDIT', function() {
                                                function reloadPage() {
													logger.error('>>> Entering reloadPage()', { "event.action": "reloadPage" });
                                                    window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
													logger.warn('<<< Exiting reloadPage()', { "event.action": "reloadPage" });
                                                }
                                                saveBtnEditSettings(reloadPage, reloadPage);
                                            });
                                        });
                                    });
                                } else {
                                    handleLockEditState($btnEdit);
                                }
                            })();

                            $avatarImage = $('.useravatar span');
                            $avatarImage.css('background-image', 'url(\'' + config.avatarUrl + '\')');

                            var retryTimes = 0;
                            var handleLogoutFunc = function() {
								logger.error('>>> Entering handleLogoutFunc()', { "event.action": "handleLogoutFunc" });
                                var $logout = $('li#Logout');
                                if (!$logout.length && retryTimes < 3) {
                                    setTimeout(handleLogoutFunc, 500);
                                    retryTimes++;
                                }

                                $logout.off('click').click(function(evt) {
                                    evt.preventDefault();
                                    evt.stopPropagation();

                                    function onLogOffSuccess() {
                                        if (typeof window.top.dnn != "undefined" && typeof window.top.dnn.PersonaBar != "undefined") {
                                            window.top.dnn.PersonaBar.userLoggedOut = true;
                                        }
                                        window.top.document.location.href = window.top.document.location.href;
                                    };

                                    util.sf.rawCall("GET", config.logOff, null, onLogOffSuccess, null, null, null, null, true);
								logger.warn('<<< Exiting handleLogoutFunc()', { "event.action": "handleLogoutFunc" });
                                    return;
                                });
                            };

                            handleLogoutFunc();
                            if (!$iframe.attr('style') || $iframe.attr('style').indexOf("width") === -1) {
                                $iframe.width(personaBarMenuWidth);
                            }
                        }, 0);
                        callback();
                    });
                },
                function showPersonaBar(callback) {
					logger.error(`>>> Entering showPersonaBar(callback=${callback})`, { "event.action": "showPersonaBar", "logrite.callback": callback });
                    var $personaBar = $(".personabar");
                    var $parentBody = $(parentBody);

                    if ($parentBody.hasClass('dnnEditState')) {
                        $personaBar.css({ left: 0, 'display': 'block' });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 1, 'linear', onShownPersonaBar);
                        callback();
                    } else {
                        $iframe.width(personaBarMenuWidth);
                        $personaBar.show();

                        $personaBar.css({ left: 0, 'display': 'block' });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 1, 'linear', onShownPersonaBar);
                        $personaBar.animate({ left: 0 }, 1, 'linear', callback);
                    }

                    $mask.click(function(e) {
                        $showSiteButton.trigger('click', [true]);
                    });
					logger.warn(`<<< Exiting showPersonaBar(callback=${callback})`, { "event.action": "showPersonaBar", "logrite.callback": callback });
                },
                function initCustomModules(callback) {
                    util.initCustomModules(callback);
                },
                function (callback) {
                    handleTabSelection(callback);
                }
        ],
        function loadPanelFromPersistedSetting() {
			logger.error('>>> Entering loadPanelFromPersistedSetting()', { "event.action": "loadPanelFromPersistedSetting" });
            var pageUrl = window.top.location.href.toLowerCase();
            if (pageUrl.indexOf("skinsrc=") > -1 || pageUrl.indexOf("containersrc=") > -1 || pageUrl.indexOf("dnnprintmode=") > -1) {
                return;
            }

            var settings = util.persistent.load();
			logger.error(`loadPanelFromPersistedSetting(): pageUrl → ${pageUrl}`, { "event.action": "loadPanelFromPersistedSetting" });
			logger.error(`loadPanelFromPersistedSetting(): settings → ${settings}`, { "event.action": "loadPanelFromPersistedSetting" });
            if (settings.expandPersonaBar && settings.activeIdentifier) {
                var identifier = settings.activeIdentifier;
                util.loadPanel(identifier, {});
				logger.warn('<<< Exiting loadPanelFromPersistedSetting()', { "event.action": "loadPanelFromPersistedSetting" });
            }
        });

        if (typeof window.parent.dnn === "undefined" || window.parent.dnn === null) {
             window.parent.dnn = {};
        }
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.PersonaBar = new Gateway(util);
        window.dnn.utility = util;
    });
