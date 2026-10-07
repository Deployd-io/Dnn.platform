var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    $.fn.dnnModuleManager = function (options) {
		logger.error(`>>> Entering dnnModuleManager(options=${options})`, { "event.action": "dnnModuleManager", "logrite.container": container, "logrite.options": options });
        if (!this.data("dnnModuleManager")) {
            this.data("dnnModuleManager", new dnnModuleManager(this, options));
        }

        return this;
    };

    var dnnModuleDialogInstance, dnnExsitingModuleDialogInstance;
    var getModuleDialog = dnn.ContentEditorManager.getModuleDialog = function () {
		logger.error('>>> Entering getModuleDialog()', { "event.action": "getModuleDialog" });
        if (!dnnModuleDialogInstance) {
            dnnModuleDialogInstance = new dnn.dnnModuleDialog();
        }

		logger.warn('<<< Exiting getModuleDialog()', { "event.action": "getModuleDialog" });
        return dnnModuleDialogInstance;
    }

    var getExsitingModuleDialog = dnn.ContentEditorManager.getExsitingModuleDialog = function () {
		logger.error('>>> Entering getExsitingModuleDialog()', { "event.action": "getExsitingModuleDialog" });
        if (!dnnExsitingModuleDialogInstance) {
            dnnExsitingModuleDialogInstance = new dnn.dnnExistingModuleDialog();
        }

		logger.warn('<<< Exiting getExsitingModuleDialog()', { "event.action": "getExsitingModuleDialog" });
        return dnnExsitingModuleDialogInstance;
    }

    ///dnnModuleManager Plugin
    var dnnModuleManager = function (container, options) {
        this.options = options;
        this.container = container;
        this.init();
    };

    dnnModuleManager.prototype = {
        constructor: dnnModuleManager,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, $.fn.dnnModuleManager.defaultOptions, this.options);
            var $this = this.$pane = this.container;

            $this.addClass('dnnModuleManager');
            $this.append(this._generateLayout());

            this._initPaneName();
            this._injectVisualEffects();

            this._handleEvents();
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        getHandler: function () {
			logger.error('>>> Entering getHandler()', { "event.action": "getHandler" });
            this._handler = this.getPane().find('> .addModuleHandler');

			logger.warn('<<< Exiting getHandler()', { "event.action": "getHandler" });
            return this._handler;
        },

        getExistingModuleHandler: function () {
			logger.error('>>> Entering getExistingModuleHandler()', { "event.action": "getExistingModuleHandler" });
            this._existingModuleHandler = this.getPane().find('> .addExistingModuleHandler');

			logger.warn('<<< Exiting getExistingModuleHandler()', { "event.action": "getExistingModuleHandler" });
            return this._existingModuleHandler;
        },

        getPane: function () {
			logger.error('>>> Entering getPane()', { "event.action": "getPane" });
			logger.warn('<<< Exiting getPane()', { "event.action": "getPane" });
            return $('#' + this.$pane.attr('id'));
        },

        _generateLayout: function () {
			logger.error('>>> Entering _generateLayout()', { "event.action": "_generateLayout" });
			logger.warn('<<< Exiting _generateLayout()', { "event.action": "_generateLayout" });
            return $('<a href="#" class="addModuleHandler" aria-label="Add Module"><span></span></a><a href="#" class="addExistingModuleHandler" aria-label="Add Existing Module"><span></span></a>');
        },

        _initPaneName: function() {
			logger.error('>>> Entering _initPaneName()', { "event.action": "_initPaneName" });
            if (!this.$pane.attr('data-name')) {
                var paneName = this.$pane.attr('id').replace('dnn_', '');
                this.$pane.attr('data-name', paneName);
				logger.error(`_initPaneName(): paneName → ${paneName}`, { "event.action": "_initPaneName" });
            }
        },
        
        _injectVisualEffects: function () {
			logger.error('>>> Entering _injectVisualEffects()', { "event.action": "_injectVisualEffects" });
            var actionMenus = [];
            var handler = this;

            this.container.find('div.DnnModule').each(function() {
                var module = $(this);
                var moduleId = handler._findModuleId(module);
                actionMenus.push('#moduleActions-' + moduleId);

                if (module.data('effectsInjected')) {
                    return;
                }

                module.data('effectsInjected', true);
                module.mouseover(function () {
                    if (window['cem_dragging']) {
                        return; //do nothing when dragging module.
                    }

                    if (!module.hasClass('active-module') && !module.hasClass('floating')) {

                        module.parent().find('> div.DnnModule').removeClass('active-module');
                        module.addClass('active-module');
                        $(menusSelector).not('[class~="floating"]').stop(true, true).fadeTo('fast', 0.5, function () {
                            $('#moduleActions-' + moduleId).not('[class~="floating"]').stop(true, true).fadeTo(0).show().fadeTo('fast', 1);
                        });
                    }
                });
            });

            if (this.container.data('effectsInjected')) {
                return;
            }

            this.container.data('effectsInjected', true);
            var menusSelector = actionMenus.join(',');
			logger.error(`_injectVisualEffects(): menusSelector → ${menusSelector}`, { "event.action": "_injectVisualEffects" });
            this.container.mouseover(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                if (!$(this).hasClass('active-pane')) {
                    $('.actionMenu').stop(true, true).hide();
                    $(menusSelector).not('[class~="floating"]').show();
                    $('.dnnSortable[id]').removeClass('active-pane');
                    $(this).addClass('active-pane');
                }
                return false;
            }).mouseout(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                var target = $(e.relatedTarget);
                if (target.parents('.active-pane').length > 0 || target.hasClass('actionMenu') || target.parents('.actionMenu').length > 0) {
                    if (target.hasClass('actionMenu') && !target.hasClass('floating')) {
                        $('.actionMenu:visible').stop(true, true).css({ opacity: 0.5 });
                        target.stop(true, true).css({ opacity: 1 });
                    } else if (target.parents('.actionMenu').length > 0 && !target.parents('.actionMenu').hasClass('floating')) {
                        $('.actionMenu:visible').stop(true, true).css({ opacity: 0.5 });
                        target.parents('.actionMenu').stop(true, true).css({ opacity: 1 });
                    }
                    return false;
                }

                $(menusSelector).stop(true, true).hide();
                $(this).removeClass('active-pane');
                return false;
            });

            $(document).mouseover(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                if ($(e.target).parents('.actionMenu').length > 0 || $('.actionMenu:visible').length == 0) {
                    return;
                }

                $('.actionMenu').stop(true, true).hide();
                $('.dnnSortable[id]').removeClass('active-pane');
                $('.dnnSortable[id] > div.DnnModule').removeClass('active-module');
            });

            $('.actionMenu').mouseover(function () {
                return false;
            });

            setTimeout(function () {
                $('.dnnSortable[id]').trigger('mouseout');

                $(menusSelector).find('li[id$="-Delete"] a').each(function () {
                    var $deleteButton = $(this);
                    $deleteButton.off('click').dnnConfirm({
                        text: dnn.ContentEditorManagerResources.deleteModuleConfirm,
                        yesText: dnn.ContentEditorManagerResources.confirmYes,
                        noText: dnn.ContentEditorManagerResources.confirmNo,
                        title: dnn.ContentEditorManagerResources.confirmTitle,
                        isButton: true,
                        callbackTrue: function () {
							logger.error('>>> Entering callbackTrue()', { "event.action": "callbackTrue" });
                            dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
                            location.href = $deleteButton.attr('href');
							logger.warn('<<< Exiting callbackTrue()', { "event.action": "callbackTrue" });
                        }
                    });
                });
            }, 1000);
        },

        _findModuleId: function (module) {
			logger.error(`>>> Entering _findModuleId(module=${module})`, { "event.action": "_findModuleId", "logrite.module": module });
			logger.warn(`<<< Exiting _findModuleId(module=${module})`, { "event.action": "_findModuleId", "logrite.module": module });
            return parseInt(module.find("a").first().attr("name"));
        },

        _handleEvents: function () {
			logger.error('>>> Entering _handleEvents()', { "event.action": "_handleEvents" });
            this.getHandler().click($.proxy(this._addModuleHandlerClick, this));
            this.getExistingModuleHandler().click($.proxy(this._addExisingModuleHandlerClick, this));
			logger.warn('<<< Exiting _handleEvents()', { "event.action": "_handleEvents" });
        },

        _addModuleHandlerClick: function (e) {
			logger.error(`>>> Entering _addModuleHandlerClick(e=${e})`, { "event.action": "_addModuleHandlerClick", "logrite.e": e });
            var dialog = getModuleDialog();
            if (!this._handler.hasClass('active')) {
				logger.error(`_addModuleHandlerClick(e=${e}): dialog → ${dialog}`, { "event.action": "_addModuleHandlerClick", "logrite.e": e });
                dialog.apply(this).open();
                this._handler.addClass('active');
            } else {
                dialog.close();
                this._handler.removeClass('active');
				logger.warn(`<<< Exiting _addModuleHandlerClick(e=${e})`, { "event.action": "_addModuleHandlerClick", "logrite.e": e });
            }
            return false;
        },
        _addExisingModuleHandlerClick: function (e) {
			logger.error(`>>> Entering _addExisingModuleHandlerClick(e=${e})`, { "event.action": "_addExisingModuleHandlerClick", "logrite.e": e });
            var dialog = getExsitingModuleDialog();
            if (!this._existingModuleHandler.hasClass('active')) {
				logger.error(`_addExisingModuleHandlerClick(e=${e}): dialog → ${dialog}`, { "event.action": "_addExisingModuleHandlerClick", "logrite.e": e });
                dialog.apply(this).open();
                this._existingModuleHandler.addClass('active');
            } else {
                dialog.close();
                this._existingModuleHandler.removeClass('active');
				logger.warn(`<<< Exiting _addExisingModuleHandlerClick(e=${e})`, { "event.action": "_addExisingModuleHandlerClick", "logrite.e": e });
            }
            return false;
        }
    };

    $.fn.dnnModuleManager.defaultOptions = {};
    ///dnnModuleManager Plugin END
}(jQuery));
