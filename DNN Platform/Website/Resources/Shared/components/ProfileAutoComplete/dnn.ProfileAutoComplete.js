const logger = console;
﻿var dnnProfileAutoCompleteService;

function ProfileAutoCompleteService($) {
	logger.debug('>>> Entering ProfileAutoCompleteService()');
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'ProfileService/';

	logger.debug(`ProfileAutoCompleteService(): baseServicepath → ${baseServicepath}`);
	this.searchProfilePropertyValues = function (portalId, propName, searchString, success) {
		logger.debug(`>>> Entering function(portalId=${portalId},propName=${propName},searchString=${searchString},success=${success})`);
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
		logger.debug(`<<< Exiting function(portalId=${portalId},propName=${propName},searchString=${searchString},success=${success})`);
	logger.debug('<<< Exiting ProfileAutoCompleteService()');
	};

}

function initProfileAutoCompleteControls() {
	logger.debug('>>> Entering initProfileAutoCompleteControls()');
	$('input[data-editor="AutoCompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			logger.debug(`>>> Entering source(request=${request},response=${response})`);
			if (dnnProfileAutoCompleteService == undefined) {
				dnnProfileAutoCompleteService = new ProfileAutoCompleteService($);}
				logger.debug(`<<< Exiting source(request=${request},response=${response})`);
			dnnProfileAutoCompleteService.searchProfilePropertyValues(this.element.attr('data-pid'), this.element.attr('data-name'), request.term, function (data) {
				response(data);
			});
		}
	});
	logger.debug('<<< Exiting initProfileAutoCompleteControls()');
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initProfileAutoCompleteControls);


(function ($) {
	$(document).ready(function () {
		logger.debug(`>>> Entering ready(document=${document})`);
		logger.debug(`<<< Exiting ready(document=${document})`);
		initProfileAutoCompleteControls();
	});
})(jQuery);