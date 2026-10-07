var logger = (typeof logger !== 'undefined' && logger) || console;
(function($, win) {
    win.ShowNotificationBar = function(text, notificationType, imagePath) {
		logger.error(`>>> Entering ShowNotificationBar(text=${text},notificationType=${notificationType},imagePath=${imagePath})`, { "event.action": "ShowNotificationBar", "logrite.text": text, "logrite.notificationType": notificationType, "logrite.imagePath": imagePath });
        showNotification({
            type: notificationType,
            message: text,
            autoClose: true,
            duration: 20,
            imagepath: imagePath
        });
		logger.warn(`<<< Exiting ShowNotificationBar(text=${text},notificationType=${notificationType},imagePath=${imagePath})`, { "event.action": "ShowNotificationBar", "logrite.text": text, "logrite.notificationType": notificationType, "logrite.imagePath": imagePath });
    }

    win.pageLoad = function() {
		logger.error('>>> Entering pageLoad()', { "event.action": "pageLoad" });
        $('.panelLoading').show();
        $(".copyButton").button({ icons: { primary: "ui-icon-copy" } });
        $(".removeButton").button({ icons: { primary: "ui-icon-trash" } });
        $(".importButton").button({ icons: { primary: "ui-icon-arrowreturnthick-1-s" } });
        $(".exportButton").button({ icons: { primary: "ui-icon-disk" } });
        $(".DefaultButton").button();
        $(".Toolbar").controlgroup();

        EnableSorting();

        $("#createGroup").click(function () {
            $(".groups").append('<li class="groupItem">' +
                         '<span class="ui-icon ui-icon-cancel" title="' + win.deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
                         '<a href="#" class="groupName" title="' + win.editGroupName + '">' + win.newGroupName + '</a>' +
                                     '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
                                     '<span class="ui-icon ui-icon-check" title="' + win.saveGroupName + '"></div>' +
                                     '<ul class="groupButtons"></ul><div style="clear:both"></div></li>');

            EnableSorting();
        });

        $("#addRowBreak").click(function () {
            $(".groups").append('<li class="groupItem rowBreakItem">' +
                         '<span class="ui-icon ui-icon-cancel" title="' + win.deleteGroup + '"></span><span class="ui-icon ui-icon-arrowthick-2-n-s"></span>' +
                         '<p class="rowBreakLabel">' + win.newRowName + '</p>' +
                         '<a href="#" class="groupName" title="' + win.editGroupName + '">rowBreak</a>' +
                                     '<input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName">' +
                                     '<span class="ui-icon ui-icon-check" title="' + win.saveGroupName + '"></div>' +
                                     '<ul class="groupButtons"><li class="groupButton ui-state-default ui-corner-all rowBreak">' +
                                          '<span class="ui-icon ui-icon-cancel"></span>' +
                                          '<span class="item">/</span>' +
                                         '</li></ul><div style="clear:both"></div></li>');

            EnableSorting();
            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $('#SettingsTabs').tabs({
            activate: function () {
				logger.error('>>> Entering activate()', { "event.action": "activate" });
                var sel = $('#SettingsTabs').tabs('option', 'active');
                $('[id*="CKEditor_Options_LastTabId"]').val(sel);
				logger.error(`activate(): sel → ${sel}`, { "event.action": "activate" });
            },
            active: $('[id*="CKEditor_Options_LastTabId"]').val()
        });

        $('#SettingsBox').height($(window).height() - 100);
        $('.ui-tabs .ui-tabs-panel').height($(window).height() - 285);

        $('#ExportDialog').dialog({
            autoOpen: false,
            width: 350,
            classes: {
                'ui-dialog': 'dnnFormPopup',
            },
            resizable: false,
            buttons: {
                "Cancel": function () {
					logger.error('>>> Entering Cancel()', { "event.action": "Cancel" });
                    $(this).dialog("close");
					logger.warn('<<< Exiting Cancel()', { "event.action": "Cancel" });
                }, 
                "Export Now": function() {
                    window.location = $(".ExportHidden").attr("href"); $(this).dialog("close");
                }
            },
            open: function () {
				logger.error('>>> Entering open()', { "event.action": "open" });
                $(this).parent().appendTo("form");
				logger.warn('<<< Exiting open()', { "event.action": "open" });
            }
        });

        $('#ImportDialog').dialog({
            autoOpen: false,
            width: 350,
            classes: {
                'ui-dialog': 'dnnFormPopup',
            },
            resizable: false,
            buttons: {
                 "Cancel": function() {
                     $(this).dialog("close");
                 },
                 "Import Now": function() {
                     window.location = $(".ImportHidden").attr("href"); $(this).dialog("close");
                 }
            },
            open: function () {
                $(this).parent().appendTo("form");
            }
        });

        $('#ToolbarGuide').dialog({
            autoOpen: false,
            buttons: {
                "OK": function() {
					logger.error('>>> Entering OK()', { "event.action": "OK" });
                    $(this).dialog("close");
					logger.warn('<<< Exiting OK()', { "event.action": "OK" });
                }
            },
            open: function () {
                $(this).parent().appendTo("form");
            }
        });

        if ($(".settingValueInputNumeric").spinner) {
            $(".settingValueInputNumeric").spinner();
        }

        if ($(".settingValueContainer").tooltip) {
            $(".settingValueInputNumeric").tooltip();
        }

        $("#CKEditor_Options_rBlSetMode input").button();
        $("#CKEditor_Options_rBlSetMode").controlgroup();
		logger.warn('<<< Exiting pageLoad()', { "event.action": "pageLoad" });
    }

    win.showDialog = function (id) {
		logger.error(`>>> Entering showDialog(id=${id})`, { "event.action": "showDialog", "logrite.id": id });
        $('#' + id).dialog("open");
		logger.warn(`<<< Exiting showDialog(id=${id})`, { "event.action": "showDialog", "logrite.id": id });
    }

    win.EnableSorting = function() {
		logger.error('>>> Entering EnableSorting()', { "event.action": "EnableSorting" });
        if ($(".rowBreakItem .rowBreak").parent("ul").length) {
            $(".rowBreakItem .rowBreak").parent("ul").removeClass("groupButtons");
        }

        $(".groups").sortable({
            connectWith: ".groups, .availableButtons",
            placeholder: "ui-state-highlight",
            update: function () {
				logger.error('>>> Entering update()', { "event.action": "update", "logrite.e": e, "logrite.ui": ui });
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
				logger.warn('<<< Exiting update()', { "event.action": "update", "logrite.e": e, "logrite.ui": ui });
            }
        });

        $(".groupButtons").sortable({
            connectWith: ".groupButtons, .availableButtons",
            placeholder: "ui-state-highlight",
            start: function (e, ui) {
				logger.error(`>>> Entering start(e=${e},ui=${ui})`, { "event.action": "start", "logrite.e": e, "logrite.ui": ui });
                $('.groupButtons').css('min-height', '50px');
                $('.groupButtons').sortable('refreshPositions');
				logger.warn(`<<< Exiting start(e=${e},ui=${ui})`, { "event.action": "start", "logrite.e": e, "logrite.ui": ui });
            },
            update: function () {
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
            }
        });

        $(".availableButtons").sortable({
            connectWith: ".groupButtons",
            placeholder: "ui-state-highlight",
            start: function (e, ui) {
                $('.groupButtons').css('min-height', '50px');
                $('.groupButtons').sortable('refreshPositions');
            },
            remove: function (e, ui) {
				logger.error(`>>> Entering remove(e=${e},ui=${ui})`, { "event.action": "remove", "logrite.e": e, "logrite.ui": ui });
                if (ui.item.attr('class').indexOf('separator') != -1) {
                    var $separator = ui.item.clone(true);
                    $separator.children(".ui-icon").remove();
                    $(this).append($separator);
                }
                ui.item.prepend('<span class="ui-icon ui-icon-cancel" title="Delete this Toolbar"></span>');
                EnableSorting();
                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
				logger.warn(`<<< Exiting remove(e=${e},ui=${ui})`, { "event.action": "remove", "logrite.e": e, "logrite.ui": ui });
            },
            update: function (e, ui) {
			logger.error(`>>> Entering update(e=${e},ui=${ui})`, { "event.action": "update", "logrite.e": e, "logrite.ui": ui });

			logger.warn(`<<< Exiting update(e=${e},ui=${ui})`, { "event.action": "update", "logrite.e": e, "logrite.ui": ui });
            }
        });

        $(".groups .ui-icon-cancel,.groupButtons .ui-icon-cancel").hide();

        $(".groups li,.groupButtons li").not(".rowBreak").hover(function () {
            $(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeIn('fast');
        }, function () {
            $(this).find(".ui-icon-cancel").eq(0).stop(true, true).fadeOut('slow');
        });


        $(".groupItem .saveGroupName").not(".rowBreakItem .saveGroupName").click(function () {
            var input = $(this).prev();
            var a = input.prev();

            a.show();
            input.hide();

            a.text(input.val());

            $(this).hide();

            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $(".groupItem .groupName").not(".rowBreakItem .groupName").click(function () {
            var a = $(this);
            var input = a.next();

            a.hide();
            input.show();

            input.val(a.text()).focus();

            input.next().css('display', 'inline-block');
        });

        $(".groupItem .groupEdit").not(".rowBreakItem .groupEdit").keydown(function (e) {
            if (e.keyCode === $.ui.keyCode.ENTER) {
                $(this).parent().find('.saveGroupName').click();

                return false;
            }
        });

        $(".groups .ui-icon-cancel").click(function () {
            $(this).parent(".groupItem").children(".groupButtons").children("li").each(function (e) {
                var $item = $(this);

                if ($item.attr('class').indexOf('separator') === -1 &&
                    $item.attr('class').indexOf('rowBreak') === -1) {

                    $item.children(".ui-icon").remove();
                    $(".availableButtons").children(".separator").before($item);
                }

                $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());

            });

            $(this).parent(".groupItem").remove();
        });

        $(".groupButtons .ui-icon-cancel").click(function () {
            var $item = $(this).parent("li");

            if ($item.attr('class').indexOf('separator') === -1 &&
               $item.attr('class').indexOf('rowBreak') === -1) {
                $item.children(".ui-icon").remove();
                $(".availableButtons").children(".separator").before($item);
            } else {
                $item.remove();
            }

            $('[id*="Options_ToolbarSet"]').val($('.groups').SerializeToolbars());
        });

        $('.panelLoading').hide();
		logger.warn('<<< Exiting EnableSorting()', { "event.action": "EnableSorting" });
    }

    $.fn.SerializeToolbars = function () {
		logger.error('>>> Entering SerializeToolbars()', { "event.action": "SerializeToolbars" });
        var items = [];
        this.children().each(function () {
            var $this = $(this);

            if (!$this.has("li").length) {
                return;
            }

            var buttons = [];
            $this.children("ul").children("li").each(function (e) {
                var html = $(this).children(".item").html();
                buttons.push(html);
            });

            var item = { name: $(this).children("a").html(), items: buttons };
            items.push(item);
        });

        var json = JSON.stringify(items);
		logger.error(`SerializeToolbars(): json → ${json}`, { "event.action": "SerializeToolbars" });
        return json.substring(1, json.length - 1).replaceAll("\"name\"", "name").replaceAll("\"items\"", "items");
    };

    String.prototype.replaceAll = function (token, newToken, ignoreCase) {
        var str, i = -1, _token;
        if ((str = this.toString()) && typeof token === "string") {
            _token = ignoreCase === true ? token.toLowerCase() : undefined;
            while ((i = (
                _token !== undefined ?
        str.toLowerCase().indexOf(
        _token,
        i >= 0 ? i + newToken.length : 0
        ) : str.indexOf(
        token,
        i >= 0 ? i + newToken.length : 0
        )
            )) !== -1) {
                str = str.substring(0, i)
            .concat(newToken)
            .concat(str.substring(i + token.length));
            }
        }
        return str;
    };

    $(win).bind('resize', function () {
        $('#SettingsBox').height($(window).height() - 100);
        $('.ui-tabs .ui-tabs-panel').height($(window).height() - 285);
    });

    $(win).bind('fileUploadRequest', function (evt) {
        console.log("setting module headers");
        $.ServicesFramework(-1).setModuleHeaders(evt.data.fileLoader.xhr);
    });

})(jQuery, window);


        