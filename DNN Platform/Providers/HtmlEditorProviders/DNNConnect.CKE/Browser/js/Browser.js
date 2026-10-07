var logger = (typeof logger !== 'undefined' && logger) || console;
(function($) {
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
		logger.error(`>>> Entering SwitchView(n=${n})`, { "event.action": "SwitchView", "logrite.n": n });
        $("#ListViewState").val(n);
        var t = $("#FilesBox ul");
		logger.error(`SwitchView(n=${n}): t → ${t}`, { "event.action": "SwitchView", "logrite.n": n });
        t.attr("class", "Files" + n);
        $(".SwitchDetailView").css("font-weight", "normal");
        $(".SwitchListView").css("font-weight", "normal");
        $(".SwitchIconsView").css("font-weight", "normal");
        $(".Switch" + n).css("font-weight", "bold");
		logger.warn(`<<< Exiting SwitchView(n=${n})`, { "event.action": "SwitchView", "logrite.n": n });
    };

    $(document).ready(function () {
        $("#BrowserMode input:checked").parent("td").addClass("SelectedPager");
        $("#dnntreeTabs li .rtIn, #dnntreeTabs li .rtImg").click(function() {
            $("#panelLoading").show();
        });
        SwitchView($("#ListViewState").val());
        $('input[type="submit"],.LinkNormal,.LinkDisabled').button(), $(".Toolbar").controlgroup();
        $("#txtWidth,#txtHeight").spinner({
            min: 0,
            max: 640,
            step: 1,
            spin: function(event, ui) {
				logger.error(`>>> Entering spin(event=${event},ui=${ui})`, { "event.action": "spin", "logrite.event": event, "logrite.ui": ui });
                $(this).trigger('change');
				logger.warn(`<<< Exiting spin(event=${event},ui=${ui})`, { "event.action": "spin", "logrite.event": event, "logrite.ui": ui });
            }
        });
        $("#BrowserMode td").addClass("ui-state-default ui-corner-top");
        $("#BrowserMode label").addClass("ui-tabs-anchor");
        $(".SelectedPager").addClass("ui-tabs-active ui-state-active ui-state-focus");
        $("#BrowserMode td").hover(function () {
            $(this).addClass("ui-state-hover");
        }, function() {
            $(this).removeClass("ui-state-hover");
        });

        $("#panUploadDiv .MessageBox").draggable({ cursor: "move", handle: "div.modalHeader" });
    });
}(jQuery));