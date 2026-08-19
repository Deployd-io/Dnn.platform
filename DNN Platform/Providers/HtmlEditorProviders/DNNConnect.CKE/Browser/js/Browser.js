const logger = console;
(function($) {
	logger.debug('>>> Entering function()');
    $.browser = {};
    (function() {
        $.browser.msie = false;
        $.browser.version = 0;
        if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
            $.browser.msie = true;
            $.browser.version = RegExp.$1;
        }
    })();

    window.SwitchView = function(n) {
        $("#ListViewState").val(n);
        var t = $("#FilesBox ul");
		logger.debug(`function(n=${n}): t → ${t}`);
        t.attr("class", "Files" + n);
        $(".SwitchDetailView").css("font-weight", "normal");
        $(".SwitchListView").css("font-weight", "normal");
        $(".SwitchIconsView").css("font-weight", "normal");
        $(".Switch" + n).css("font-weight", "bold");
    };

	logger.debug('<<< Exiting function()');
    $(document).ready(function () {
		logger.debug(`>>> Entering ready(document=${document})`);
        $("#BrowserMode input:checked").parent("td").addClass("SelectedPager");
        $("#dnntreeTabs li .rtIn, #dnntreeTabs li .rtImg").click(function() {
			logger.debug('>>> Entering click()');
			logger.debug('<<< Exiting click()');
            $("#panelLoading").show();
        });
        SwitchView($("#ListViewState").val());
        $('input[type="submit"],.LinkNormal,.LinkDisabled').button(), $(".Toolbar").controlgroup();
        $("#txtWidth,#txtHeight").spinner({
            min: 0,
            max: 640,
            step: 1,
            spin: function(event, ui) {
				logger.debug(`>>> Entering spin(event=${event},ui=${ui})`);
                $(this).trigger('change');
				logger.debug(`<<< Exiting spin(event=${event},ui=${ui})`);
            }
        });
        $("#BrowserMode td").addClass("ui-state-default ui-corner-top");
        $("#BrowserMode label").addClass("ui-tabs-anchor");
        $(".SelectedPager").addClass("ui-tabs-active ui-state-active ui-state-focus");
        $("#BrowserMode td").hover(function () {
			logger.debug('>>> Entering hover()');
			logger.debug('<<< Exiting hover()');
            $(this).addClass("ui-state-hover");
        }, function() {
            $(this).removeClass("ui-state-hover");
        });

		logger.debug(`<<< Exiting ready(document=${document})`);
        $("#panUploadDiv .MessageBox").draggable({ cursor: "move", handle: "div.modalHeader" });
    });
}(jQuery));