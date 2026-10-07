var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    $.fn.dnnModuleDragDrop = function (options) {
	logger.error(`>>> Entering dnnModuleDragDrop(options=${options})`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });

        var isEditMode = $('body').hasClass('dnnEditState');
		logger.error(`dnnModuleDragDrop(options=${options}): isEditMode → ${isEditMode}`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });
        if (!isEditMode) {
            $(function () {
                $('.DNNEmptyPane').each(function () {
                    $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
            });

			logger.debug(`dnnModuleDragDrop(options=${options}): this.className → ${this.className}`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });
            return this;
        }

        //Default settings
        var settings = {
            actionMenu: "div.actionMenu",
            cursor: "move",
            draggingHintText: "DraggingHintText",
            dragHintText: "DragHintText",
            dropOnEmpty: true,
            dropHintText: "DropHintText",
            dropTargetText: "DropModuleText"
        };

        settings = $.extend(settings, options || {});
		logger.error(`dnnModuleDragDrop(options=${options}): settings → ${settings}`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });
        var $self = this;
        var paneModuleIndex;
        var modulePaneName;
        var tabId = settings.tabId;
        var mid;

        var $modules = $('.DnnModule');
        var $module;

        var getModuleId = function ($mod) {
			logger.error(`>>> Entering getModuleId($mod=${$mod})`, { "event.action": "getModuleId", "logrite.$mod": $mod });
            var result = $mod.attr("class").match(/\bDnnModule-([0-9]+)\b/);
			logger.error(`getModuleId($mod=${$mod}): result → ${result}`, { "event.action": "getModuleId", "logrite.$mod": $mod });
            return (result && result.length === 2) ? result[1] : null;
        };

        var getModuleIndex = function (moduleId, $pane) {
			logger.error(`>>> Entering getModuleIndex(moduleId=${moduleId},$pane=${$pane})`, { "event.action": "getModuleIndex", "logrite.moduleId": moduleId, "logrite.$pane": $pane });
            var index = -1;
            var modules = $pane.children(".DnnModule");
            for (var i = 0; i < modules.length; i++) {
                var module = modules[i];
                mid = getModuleId($(module));

                if (moduleId == parseInt(mid)) {
                    index = i;
                    break;
                }
            }
			logger.error(`getModuleIndex(moduleId=${moduleId},$pane=${$pane}): modules → ${modules}`, { "event.action": "getModuleIndex", "logrite.moduleId": moduleId, "logrite.$pane": $pane });
            return index;
        };

        var updateServer = function (moduleId, $pane, callback) {
			logger.error(`>>> Entering updateServer(moduleId=${moduleId},$pane=${$pane},callback=${callback})`, { "event.action": "updateServer", "logrite.moduleId": moduleId, "logrite.$pane": $pane, "logrite.callback": callback });
            var order;
            var paneName = $pane.attr("id").substring(4);
			logger.error(`updateServer(moduleId=${moduleId},$pane=${$pane},callback=${callback}): paneName → ${paneName}`, { "event.action": "updateServer", "logrite.moduleId": moduleId, "logrite.$pane": $pane, "logrite.callback": callback });
            var index = getModuleIndex(moduleId, $pane);

            if (paneName !== modulePaneName) {
                //Moved to new Pane
                order = index * 2;
            } else {
                //Module moved within Pane
                if (index > paneModuleIndex) {
                    //Module moved down
                    order = (index + 1) * 2;
                } else {
                    //Module moved up
                    order = index * 2;
                }
            }

            var dataVar = {
                TabId: tabId,
                ModuleId: moduleId,
                Pane: paneName,
                ModuleOrder: order
            };

			logger.error(`updateServer(moduleId=${moduleId},$pane=${$pane},callback=${callback}): dataVar → ${dataVar}`, { "event.action": "updateServer", "logrite.moduleId": moduleId, "logrite.$pane": $pane, "logrite.callback": callback });
            var service = $.dnnSF();
			logger.warn(`<<< Exiting updateServer(moduleId=${moduleId},$pane=${$pane},callback=${callback})`, { "event.action": "updateServer", "logrite.moduleId": moduleId, "logrite.$pane": $pane, "logrite.callback": callback });
            var serviceUrl = $.dnnSF().getServiceRoot("InternalServices") + "ModuleService/";
            $.ajax({
                url: serviceUrl + 'MoveModule',
                type: 'POST',
                data: dataVar,
                beforeSend: service.setModuleHeaders,
                success: function () {
					logger.error('>>> Entering success()', { "event.action": "success" });
                    if (typeof callback === "function") {
                        callback.call($pane);
                    } else {
                        window.location.reload();
						logger.warn('<<< Exiting success()', { "event.action": "success" });
                    }
                },
                error: function () {
				logger.error('>>> Entering error()', { "event.action": "error" });
                }
            });
        };

        for (var moduleNo = 0; moduleNo < $modules.length; moduleNo++) {
            $module = $($modules[moduleNo]);
            mid = getModuleId($module);
            if (!$module.hasClass("DnnModule-Admin")) {
                //Add a drag handle
                if ($module.find(".dnnDragHint").length === 0) {
                    $module.prepend("<div class=\"dnnDragHint\"></div>");
                }

                //Add a drag hint
                $module.find(".dnnDragHint").dnnHelperTip({
                    helpContent: settings.dragHintText,
                    holderId: "ModuleDragToolTip-" + mid
                });
            }
        }

        //call jQuery UI Sortable plugin
        var originalPane = null;
        var allDnnSortableEmpty = true;
        $('.dnnSortable').each(function (n, v) {
            if (!$.trim($(v).html()).length) {
                return true;
            }
            else {
                allDnnSortableEmpty = false;
                return false;
            }
        });
        if (allDnnSortableEmpty) {

            $(function () {
                $('.DNNEmptyPane').each(function () {
                    $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
            });


            $self.droppable({
                tolerance: "pointer",
                /*hoverClass: 'dnnDropTarget',*/
                over: function (event, ui) {
					logger.error(`>>> Entering over(event=${event},ui=${ui})`, { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                    $(this).append("<div class='dnnDropTarget'><p>" + settings.dropTargetText + "(" + $(this).attr("id").substring(4) + ")" + "</p></div>");
					logger.warn(`<<< Exiting over(event=${event},ui=${ui})`, { "event.action": "over", "logrite.event": event, "logrite.ui": ui });
                },
                out: function (event, ui) {
					logger.error(`>>> Entering out(event=${event},ui=${ui})`, { "event.action": "out", "logrite.event": event, "logrite.ui": ui });
                    $(this).empty();
					logger.warn(`<<< Exiting out(event=${event},ui=${ui})`, { "event.action": "out", "logrite.event": event, "logrite.ui": ui });
                },
                drop: function (event, ui) {
					logger.error(`>>> Entering drop(event=${event},ui=${ui})`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
                    // add module
                    var dropItem = ui.draggable;
                    var pane = $(this).empty();
					logger.error(`drop(event=${event},ui=${ui}): pane → ${pane}`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
                    var order = 0;
                    var paneName = pane.attr("id").substring(4);
					logger.error(`drop(event=${event},ui=${ui}): paneName → ${paneName}`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
                    dropItem.remove();
                    if (dnn.controlBar) {
                        dnn.controlBar.addModule(dnn.controlBar.dragdropModule + '',
                            dnn.controlBar.dragdropPage,
                            paneName,
                            '-1',
                            order + '',
                            dnn.controlBar.dragdropVisibility + '',
                            dnn.controlBar.dragdropAddExistingModule + '',
                            dnn.controlBar.dragdropCopyModule + '');
							logger.warn(`<<< Exiting drop(event=${event},ui=${ui})`, { "event.action": "drop", "logrite.event": event, "logrite.ui": ui });
                    }
                }
            });
            return $self;
        }

        $('div.dnnDragHint').mousedown(function () {
            $('.DNNEmptyPane').each(function () {
                $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
            });
            $('.contentPane').each(function () {
                // this special code is for you -- IE8
                this.className = this.className;
            });

        }).mouseup(function () {
            $('.dnnDropEmptyPanes').each(function () {
                $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
            });
            $('.contentPane').each(function () {
                // this special code is for you -- IE8
                this.className = this.className;
            });
        });

        $self.sortable({
            connectWith: ".dnnSortable",
            dropOnEmpty: settings.dropOnEmpty,
            cursor: settings.cursor,
            cursorAt: { left: 10, top: 30 },
            handle: "div.dnnDragHint",
            placeholder: "dnnDropTarget",
            tolerance: "pointer",
            helper: function (event, ui) {
			logger.error(`>>> Entering helper(event=${event},ui=${ui})`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });

                var dragTip = $('<div class="dnnDragdropTip ControlBar_DragdropModule"></div>');
				logger.error(`helper(event=${event},ui=${ui}): dragTip → ${dragTip}`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });
                var title = $('span.Head', ui).html();
				logger.error(`helper(event=${event},ui=${ui}): title → ${title}`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });
                if (!title)
                    title = "The Dragging Module";

                dragTip.html(title);
                $('body').append(dragTip);
				logger.warn(`<<< Exiting helper(event=${event},ui=${ui})`, { "event.action": "helper", "logrite.event": event, "logrite.ui": ui });
                return dragTip;
            },

            start: function (event, ui) {
				logger.error(`>>> Entering start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
                var $pane = ui.item.parent();
                originalPane = $pane;

                modulePaneName = $pane.attr("id").substring(4);
				logger.debug(`dnnModuleDragDrop(options=${options}): modulePaneName → ${modulePaneName}`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });
                mid = getModuleId(ui.item);
                paneModuleIndex = getModuleIndex(mid, $pane);

                //Add drop target text
                var $dropTarget = $(".dnnDropTarget");
                $dropTarget.append("<p>" + settings.dropTargetText + "(" + modulePaneName + ")" + "</p>");

                $(settings.actionMenu).hide();
				logger.warn(`<<< Exiting start(event=${event},ui=${ui})`, { "event.action": "start", "logrite.event": event, "logrite.ui": ui });
            },

            over: function (event, ui) {
                //Add drop target text
                var $dropTarget = $(".dnnDropTarget");
                $dropTarget.empty().append("<p>" + settings.dropTargetText + "(" + $(this).attr("id").substring(4) + ")" + "</p>");
            },

            stop: function (event, ui, callback) {
				logger.error(`>>> Entering stop(event=${event},ui=${ui},callback=${callback})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
                var dropItem = ui.item;
				logger.debug(`dnnModuleDragDrop(options=${options}): mid → ${mid}`, { "event.action": "dnnModuleDragDrop", "logrite.options": options });
                if (dnn.controlBar && dropItem.hasClass('ControlBar_ModuleDiv')) {
                    // add module
                	var pane = ui.item.parent();
					logger.error(`stop(event=${event},ui=${ui},callback=${callback}): pane → ${pane}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
                    var order = -1;
                    var paneName = pane.attr("id").substring(4);
					logger.error(`stop(event=${event},ui=${ui},callback=${callback}): paneName → ${paneName}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
	                if ($('div.DnnModule', pane).length > 0) {
		                var modules = $('div.DnnModule, div.ControlBar_ModuleDiv', pane);
						logger.error(`stop(event=${event},ui=${ui},callback=${callback}): modules → ${modules}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
		                for (var i = 0; i < modules.length; i++) {
			                var module = modules.get(i);
							logger.error(`stop(event=${event},ui=${ui},callback=${callback}): module → ${module}`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
			                if ($(module).hasClass('ControlBar_ModuleDiv')) {
				                order = i;
			                }
		                }
	                }
	                dropItem.remove();
                    dnn.controlBar.addModule(dnn.controlBar.dragdropModule + '',
                        dnn.controlBar.dragdropPage,
                        paneName,
                        '-1',
                        order + '',
                        dnn.controlBar.dragdropVisibility + '',
                        dnn.controlBar.dragdropAddExistingModule + '',
                        dnn.controlBar.dragdropCopyModule + '');

                } else {
                    // move module
                    mid = getModuleId(dropItem);
                    updateServer(mid, dropItem.parent(), callback);
                    $(settings.actionMenu).show();

                    //remove the empty pane holder class for current pane
                    dropItem.parent().removeClass("dnnDropEmptyPanes");
                    // if original pane is empty, add dnnemptypane class
                    if (originalPane && $('div', originalPane).length === 0)
                        originalPane.addClass('dnnDropEmptyPanes');

                    if (!callback) {
						logger.warn(`<<< Exiting stop(event=${event},ui=${ui},callback=${callback})`, { "event.action": "stop", "logrite.event": event, "logrite.ui": ui, "logrite.callback": callback });
                        // show animation
                        dropItem.css('background-color', '#fffacd');
                        setTimeout(function() {
                            dropItem.css('background', '#fffff0');
                            setTimeout(function() {
                                dropItem.css('background', 'transparent');
                            }, 300);
                        }, 2500);
                    }

                    $("div[data-tipholder=\"" + "ModuleDragToolTip-" + mid + "\"] .dnnHelpText").text(settings.dragHintText);
                   
                    $('.dnnDropEmptyPanes').each(function () {
                        $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
                    });
                    $('.contentPane').each(function () {
                        // this special code is for you -- IE8
                        this.className = this.className;
                    });

                    //fire window resize to reposition action menus
                    $(window).resize();
                }
            }
        });

        return $self;
    };
})(jQuery);