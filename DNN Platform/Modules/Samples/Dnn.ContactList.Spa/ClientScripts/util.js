const logger = console;
if (typeof contactList === 'undefined' || contactList === null) {
    contactList = {};
};

contactList.utility = function(settings, resx){
logger.debug('>>> Entering function()');

    var resx = resx;

    var alertConfirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
		logger.debug(`>>> Entering alertConfirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
        $('#confirmation-dialog > div.dnnDialog').html(text);
        $('#confirmation-dialog a#confirmbtn').html(confirmBtnText).unbind('click').bind('click', function() {
			logger.debug('>>> Entering bind()');
            if (typeof confirmHandler === 'function') confirmHandler.apply();
			logger.debug('<<< Exiting bind()');
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

		logger.debug(`<<< Exiting alertConfirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
        $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {
		logger.debug(`>>> Entering on(window=${window})`);

            if (evt.keyCode === 27) {
                $(window).off('keydown.confirmDialog');
                $('#confirmation-dialog a#cancelbtn').trigger('click');
            }
		logger.debug(`<<< Exiting on(window=${window})`);
        });
    };

    var alert = function(text, closeBtnText, closeBtnHandler) {
		logger.debug(`>>> Entering alert(text=${text},closeBtnText=${closeBtnText},closeBtnHandler=${closeBtnHandler})`);
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.alert);
        alertConfirm(text, closeBtnText, "", closeBtnHandler, null)
		logger.debug(`<<< Exiting alert(text=${text},closeBtnText=${closeBtnText},closeBtnHandler=${closeBtnHandler})`);
    };

    var confirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
		logger.debug(`>>> Entering confirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.confirm);
        alertConfirm(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler)
		logger.debug(`<<< Exiting confirm(text=${text},confirmBtnText=${confirmBtnText},cancelBtnText=${cancelBtnText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
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
    var serviceController = "";
	logger.debug(`function(): serviceController → ${serviceController}`);
    var serviceFramework;
    var baseServicepath;

    var call = function(httpMethod, url, params, success, failure){
        var options = {
            url: url,
            beforeSend: serviceFramework.setModuleHeaders,
            type: httpMethod,
            async: true,
            success: function(data){
				logger.debug(`>>> Entering success(data=${data})`);
                if(typeof success === 'function'){
                    success(data || {});
					logger.debug(`<<< Exiting success(data=${data})`);
                }
            },
            error: function(xhr, status, err){
				logger.debug(`>>> Entering error(xhr=${xhr},status=${status},err=${err})`);
                if(typeof failure === 'function'){
                    if(xhr){
                        failure(xhr, err);
                    }
                    else{
                        failure(null, 'Unknown error');
						logger.debug(`<<< Exiting error(xhr=${xhr},status=${status},err=${err})`);
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
		logger.debug(`>>> Entering get(method=${method},params=${params},success=${success},failure=${failure})`);
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
		logger.debug(`get(method=${method},params=${params},success=${success},failure=${failure}): url → ${url}`);
		logger.debug(`<<< Exiting get(method=${method},params=${params},success=${success},failure=${failure})`);
        return call('GET', url, params, success, failure);
    };

    var init = function(settings){
		logger.debug(`>>> Entering init(settings=${settings})`);
        serviceFramework = settings.servicesFramework;
        baseServicepath = serviceFramework.getServiceRoot('Dnn/ContactList');
		logger.debug(`init(settings=${settings}): serviceFramework → ${serviceFramework}`);
		logger.debug(`<<< Exiting init(settings=${settings})`);
    };

	logger.debug(`function(): serviceFramework → ${serviceFramework}`);
    var post = function (method, params, success, failure, loading) {
		logger.debug(`>>> Entering post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading})`);
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
		logger.debug(`post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading}): url → ${url}`);
		logger.debug(`<<< Exiting post(method=${method},params=${params},success=${success},failure=${failure},loading=${loading})`);
        return call('POST', url, params, success, failure);
    };

    var setHeaders = function(xhr){
		logger.debug(`>>> Entering setHeaders(xhr=${xhr})`);
        if(tabId){
            xhr.setRequestHeader('TabId', tabId);
        }

        if(antiForgeryToken){
            xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
			logger.debug(`<<< Exiting setHeaders(xhr=${xhr})`);
        }
    };

    return {
        get: get,
        init: init,
        post: post,
        serviceController: serviceController
    }
	logger.debug('<<< Exiting function()');
};
