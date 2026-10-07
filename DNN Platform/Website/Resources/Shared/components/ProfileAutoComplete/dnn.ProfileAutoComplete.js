var logger = (typeof logger !== 'undefined' && logger) || console;
var dnnProfileAutoCompleteService;

function ProfileAutoCompleteService($) {
	logger.error('>>> Entering ProfileAutoCompleteService()', { "event.action": "ProfileAutoCompleteService" });
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'ProfileService/';

	logger.error(`ProfileAutoCompleteService(): baseServicepath → ${baseServicepath}`, { "event.action": "ProfileAutoCompleteService" });
	this.searchProfilePropertyValues = function (portalId, propName, searchString, success) {
		logger.error(`>>> Entering searchProfilePropertyValues(portalId=${portalId},propName=${propName},searchString=${searchString},success=${success})`, { "event.action": "searchProfilePropertyValues", "logrite.portalId": portalId, "logrite.propName": propName, "logrite.searchString": searchString, "logrite.success": success });
		$.ajax({
			type: "GET",
			url: baseServicepath + 'ProfilePropertyValues',
			beforeSend: $.dnnSF().setModuleHeaders,
			data: { portalId: portalId, propName: propName, searchString: searchString }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
		logger.warn(`<<< Exiting searchProfilePropertyValues(portalId=${portalId},propName=${propName},searchString=${searchString},success=${success})`, { "event.action": "searchProfilePropertyValues", "logrite.portalId": portalId, "logrite.propName": propName, "logrite.searchString": searchString, "logrite.success": success });
	logger.warn('<<< Exiting ProfileAutoCompleteService()', { "event.action": "ProfileAutoCompleteService" });
	};

}

function initProfileAutoCompleteControls() {
	logger.error('>>> Entering initProfileAutoCompleteControls()', { "event.action": "initProfileAutoCompleteControls" });
	$('input[data-editor="AutoCompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			logger.error(`>>> Entering source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
			if (dnnProfileAutoCompleteService == undefined) {
				dnnProfileAutoCompleteService = new ProfileAutoCompleteService($);}
			dnnProfileAutoCompleteService.searchProfilePropertyValues(this.element.attr('data-pid'), this.element.attr('data-name'), request.term, function (data) {
				response(data);
			});
			logger.warn(`<<< Exiting source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
		}
	});
	logger.warn('<<< Exiting initProfileAutoCompleteControls()', { "event.action": "initProfileAutoCompleteControls" });
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initProfileAutoCompleteControls);


(function ($) {
	$(document).ready(function () {
		initProfileAutoCompleteControls();
	});
})(jQuery);