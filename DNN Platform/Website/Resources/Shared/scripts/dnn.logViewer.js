const logger = console;
﻿(function ($) {
    if (typeof dnn === 'undefined') dnn = {};
    dnn.logViewer = dnn.logViewer || {};

    //handle window unload event, when window in unload process, then don't remove the cookie.
    var windowUnloaded = false;
    $(window).on('beforeunload', function () {
        windowUnloaded = true;
    });
    
    dnn.logViewer.init = function () {
        if (windowUnloaded) {
            return;
        }
        var handler = this;
        var serviceFramework = $.ServicesFramework();
        var guid = dnn.dom.getCookie('LogGUID');
        if (!guid || guid.length <= 0) {
            return;
        }

        dnn.dom.deleteCookie('LogGUID');
        var servicePath = serviceFramework.getServiceRoot('InternalServices') + 'EventLogService/GetLogDetails';
        $('div[class*=dnnFormMessage][class*=dnnFormValidationSummary]').click(function() {
			logger.debug('>>> Entering click()');
            $.ajax({
                type: "GET",
                url: servicePath,
                data: { guid: guid },
                beforeSend: serviceFramework.setModuleHeaders,
                success: function (data) {
					logger.debug(`>>> Entering success(data=${data})`);
                    var container = handler.getContainer();
                    container.html(data.Content).dialog({
                        modal: true,
                        autoOpen: true,
                        title: data.Title,
                        classes: { "ui-dialog": "dnnFormPopup" },
                        position: "center",
                        width: 800,
                        height: 480,
                        resizable: false,
                        closeOnEscape: true
                    });
					logger.debug(`<<< Exiting success(data=${data})`);
                },
                error: function (xhr, status, error) {
					logger.debug(`>>> Entering error(xhr=${xhr},status=${status},error=${error})`);
                    alert(error);
					logger.debug(`<<< Exiting error(xhr=${xhr},status=${status},error=${error})`);
                }
            });
			logger.debug('<<< Exiting click()');
        }).css('cursor', 'pointer');
    };

    dnn.logViewer.getContainer = function() {
        var container = $('div[class=errorWin]');
        if (container.length == 0) {
            $(document.body).append('<div class="errorWin"></div>');
        }

        return $('div[class=errorWin]');
    };

    $(document).ready(function() {
        dnn.logViewer.init.call(dnn.logViewer);
    });
}(jQuery));