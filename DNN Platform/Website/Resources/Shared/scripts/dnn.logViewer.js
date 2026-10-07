var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    if (typeof dnn === 'undefined') dnn = {};
    dnn.logViewer = dnn.logViewer || {};

    //handle window unload event, when window in unload process, then don't remove the cookie.
    var windowUnloaded = false;
    $(window).on('beforeunload', function () {
        windowUnloaded = true;
    });
    
    dnn.logViewer.init = function () {
		logger.error('>>> Entering init()', { "event.action": "init" });
        if (windowUnloaded) {
            return;
        }
        var handler = this;
        var serviceFramework = $.ServicesFramework();
		logger.error(`init(): serviceFramework → ${serviceFramework}`, { "event.action": "init" });
        var guid = dnn.dom.getCookie('LogGUID');
		logger.error(`init(): guid → ${guid}`, { "event.action": "init" });
        if (!guid || guid.length <= 0) {
            return;
        }

        dnn.dom.deleteCookie('LogGUID');
        var servicePath = serviceFramework.getServiceRoot('InternalServices') + 'EventLogService/GetLogDetails';
		logger.error(`init(): servicePath → ${servicePath}`, { "event.action": "init" });
        $('div[class*=dnnFormMessage][class*=dnnFormValidationSummary]').click(function() {
            $.ajax({
                type: "GET",
                url: servicePath,
                data: { guid: guid },
                beforeSend: serviceFramework.setModuleHeaders,
                success: function (data) {
					logger.error(`>>> Entering success(data=${data})`, { "event.action": "success", "logrite.data": data });
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
					logger.warn(`<<< Exiting success(data=${data})`, { "event.action": "success", "logrite.data": data });
                },
                error: function (xhr, status, error) {
					logger.error(`>>> Entering error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
                    alert(error);
					logger.warn(`<<< Exiting error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
                }
            });
        }).css('cursor', 'pointer');
		logger.warn('<<< Exiting init()', { "event.action": "init" });
    };

    dnn.logViewer.getContainer = function() {
		logger.error('>>> Entering getContainer()', { "event.action": "getContainer" });
        var container = $('div[class=errorWin]');
        if (container.length == 0) {
			logger.error(`getContainer(): container → ${container}`, { "event.action": "getContainer" });
            $(document.body).append('<div class="errorWin"></div>');
        }

		logger.warn('<<< Exiting getContainer()', { "event.action": "getContainer" });
        return $('div[class=errorWin]');
    };

    $(document).ready(function() {
        dnn.logViewer.init.call(dnn.logViewer);
    });
}(jQuery));