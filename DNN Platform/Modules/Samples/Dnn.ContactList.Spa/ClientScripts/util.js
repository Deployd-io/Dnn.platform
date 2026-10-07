var logger = (typeof logger !== 'undefined' && logger) || console;
if (typeof contactList === 'undefined' || contactList === null) {
    contactList = {};
};

contactList.utility = function(settings, resx){

    var resx = resx;

    var alertConfirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
		logger.error(`>>> Entering alertConfirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "alertConfirm", "logrite.text": text, "logrite.confirmBtnText": confirmBtnText, "logrite.cancelBtnText": cancelBtnText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
        $('#confirmation-dialog > div.dnnDialog').html(text);
        $('#confirmation-dialog a#confirmbtn').html(confirmBtnText).unbind('click').bind('click', function() {
            if (typeof confirmHandler === 'function') confirmHandler.apply();
            $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
        });

        var $cancelBtn = $('#confirmation-dialog a#cancelbtn');
        if(cancelBtnText !== ''){
            $cancelBtn.html(cancelBtnText).unbind('click').bind('click', function() {
                if (typeof cancelHandler === 'function') cancelHandler.apply();
                $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
            });
            $cancelBtn.show();
        }
        else {
            $cancelBtn.hide();
        }

        $('#mask').show();
        $('#confirmation-dialog').fadeIn(200, 'linear');

        $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {

            if (evt.keyCode === 27) {
                $(window).off('keydown.confirmDialog');
                $('#confirmation-dialog a#cancelbtn').trigger('click');
				logger.warn(`<<< Exiting alertConfirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "alertConfirm", "logrite.text": text, "logrite.confirmBtnText": confirmBtnText, "logrite.cancelBtnText": cancelBtnText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
            }
        });
    };

    var alert = function(text, closeBtnText, closeBtnHandler) {
		logger.error(`>>> Entering alert(text=${text},closeBtnText=${closeBtnText},closeBtnHandler=${closeBtnHandler})`, { "event.action": "alert", "logrite.text": text, "logrite.closeBtnText": closeBtnText, "logrite.closeBtnHandler": closeBtnHandler });
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.alert);
        alertConfirm(text, closeBtnText, "", closeBtnHandler, null)
		logger.warn(`<<< Exiting alert(text=${text},closeBtnText=${closeBtnText},closeBtnHandler=${closeBtnHandler})`, { "event.action": "alert", "logrite.text": text, "logrite.closeBtnText": closeBtnText, "logrite.closeBtnHandler": closeBtnHandler });
    };

    var confirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
		logger.error(`>>> Entering confirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtnText": confirmBtnText, "logrite.cancelBtnText": cancelBtnText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.confirm);
        alertConfirm(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler)
		logger.warn(`<<< Exiting confirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.text": text, "logrite.confirmBtnText": confirmBtnText, "logrite.cancelBtnText": cancelBtnText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
    };

    var sf = contactList.sf();
    sf.init(settings);

    return {
        alert: alert,
        confirm: confirm,
        sf: sf
    }
};

contactList.sf = function(){
	logger.error('>>> Entering sf()', { "event.action": "sf" });
    var serviceController = "";
    var serviceFramework;
	logger.error(`sf(): serviceController → ${serviceController}`, { "event.action": "sf" });
    var baseServicepath;

    var call = function(httpMethod, url, params, success, failure){
        var options = {
            url: url,
            beforeSend: serviceFramework.setModuleHeaders,
            type: httpMethod,
            async: true,
            success: function(data){
				logger.error(`>>> Entering success(data=${data})`, { "event.action": "success", "logrite.data": data });
                if(typeof success === 'function'){
                    success(data || {});
					logger.warn(`<<< Exiting success(data=${data})`, { "event.action": "success", "logrite.data": data });
                }
            },
            error: function(xhr, status, err){
				logger.error(`>>> Entering error(xhr=${xhr},status=${status},err=${err})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.err": err });
                if(typeof failure === 'function'){
                    if(xhr){
                        failure(xhr, err);
                    }
                    else{
                        failure(null, 'Unknown error');
						logger.warn(`<<< Exiting error(xhr=${xhr},status=${status},err=${err})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.err": err });
                    }
                }
            }
        };

        if (httpMethod == 'GET') {
            options.data = params;
        }
        else {
            options.contentType = 'application/json; charset=UTF-8';
            options.data = JSON.stringify(params);
            options.dataType = 'json';
        }

        return $.ajax(options);
    };

    var get = function (method, params, success, failure) {
		logger.error(`>>> Entering get(method=${method},params=${params},success=${success},failure=${failure})`, { "event.action": "get", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure });
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
		logger.error(`get(method=${method},params=${params},success=${success},failure=${failure}): url → ${url}`, { "event.action": "get", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure });
        return call('GET', url, params, success, failure);
    };

    var init = function(settings){
		logger.error(`>>> Entering init(settings=${settings})`, { "event.action": "init", "logrite.settings": settings });
        serviceFramework = settings.servicesFramework;
        baseServicepath = serviceFramework.getServiceRoot('Dnn/ContactList');
		logger.warn(`<<< Exiting init(settings=${settings})`, { "event.action": "init", "logrite.settings": settings });
    };

    var post = function (method, params, success, failure, loading) {
		logger.error(`>>> Entering post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading})`, { "event.action": "post", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading });
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
		logger.error(`post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading}): url → ${url}`, { "event.action": "post", "logrite.method": method, "logrite.params": params, "logrite.success": success, "logrite.failure": failure, "logrite.loading": loading });
        return call('POST', url, params, success, failure);
    };

    var setHeaders = function(xhr){
		logger.error(`>>> Entering setHeaders(xhr=${xhr})`, { "event.action": "setHeaders", "logrite.xhr": xhr });
        if(tabId){
            xhr.setRequestHeader('TabId', tabId);
        }

        if(antiForgeryToken){
            xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
			logger.warn(`<<< Exiting setHeaders(xhr=${xhr})`, { "event.action": "setHeaders", "logrite.xhr": xhr });
        }
    };

    return {
        get: get,
        init: init,
        post: post,
        serviceController: serviceController
    }
};
