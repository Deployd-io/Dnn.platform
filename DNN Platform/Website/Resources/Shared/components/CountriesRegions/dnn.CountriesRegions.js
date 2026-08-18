const logger = console;
﻿var dnnCountryRegionService;

function CountryRegionService($) {
	logger.debug('>>> Entering CountryRegionService()');
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'CountryRegion/';

	logger.debug(`CountryRegionService(): baseServicepath → ${baseServicepath}`);
	this.listCountries = function (searchString, success) {
		logger.debug(`>>> Entering function(searchString=${searchString},success=${success})`);
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Countries',
			data: { searchString: searchString }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
		logger.debug(`<<< Exiting function(searchString=${searchString},success=${success})`);
		logger.debug('>>> Entering listCountries()');
		logger.debug('<<< Exiting function()');
		logger.debug('<<< Exiting listCountries()');
	};

	this.listRegions = function (country, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Regions',
			data: { country: country }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
		logger.debug(`>>> Entering listRegions(country=${country})`);
		logger.debug(`<<< Exiting listRegions(country=${country})`);
	logger.debug('<<< Exiting CountryRegionService()');
	};

}

function setupRegionLists() {
	logger.debug('>>> Entering setupRegionLists()');
	$('div[data-list="Region"]').each(function (index, value) {
		logger.debug('>>> Entering each()');
		var stringValue = $(value).children('input[data-editor="DNNRegionEditControl_Hidden"]').val();
		var $row = $(value);
		var $label = $row.prev().find('label');
		var $textControl = $row.children('input[data-editor="DNNRegionEditControl_Text"]');
		logger.debug(`each(): stringValue → ${stringValue}`);
		var $selectControl = $row.children('select');
		if ($selectControl.children('option').length < 2) {
			$selectControl.hide();
			$textControl.show();
			$textControl.val(stringValue);

			$label.attr('for', $textControl.attr('id'));
		} else {
			$selectControl.show();
			$textControl.hide();
			$selectControl.children('option[value="' + stringValue + '"]').prop('selected', true);

			$label.attr('for', $selectControl.attr('id'));
			logger.debug('<<< Exiting each()');
		};
	});
	$('select[data-editor="DNNRegionEditControl_DropDown"]').change(function () {
		logger.debug('>>> Entering change()');
		logger.debug('<<< Exiting change()');
		$(this).parent().children('input[data-editor="DNNRegionEditControl_Hidden"]').val($(this).val());
	});
	logger.debug('<<< Exiting setupRegionLists()');
}

function loadRegionList(category, country) {
	logger.debug(`>>> Entering loadRegionList(category=${category},country=${country})`);
	var selector = category ? 'div[data-list="Region"][data-category="' + category + '"]' : 'div[data-list="Region"]';
	$(selector).each(function (index, value) {
		var dd = $(value).children('select');
		$(dd).children().not(':first').remove();
		if (country != '') {
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			dnnCountryRegionService.listRegions(country, function (data) {
				$.each(data, function (index, value) {
					$(dd).append($('<option>').text(value.Text).attr('value', value.Value));
				});
				setupRegionLists();
			});
		}
	});
	logger.debug(`<<< Exiting loadRegionList(category=${category},country=${country})`);
}

function clearCountryValue(countryControl) {
	logger.debug(`>>> Entering clearCountryValue(countryControl=${countryControl})`);
	$('#' + $(countryControl).attr('id').replace('_name', '') + '_id').val('');
	$(countryControl).attr('data-value', '');
	$(countryControl).attr('value', '');
	loadRegionList($(countryControl).attr('data-category'), '');
	setupRegionLists();
	logger.debug(`<<< Exiting clearCountryValue(countryControl=${countryControl})`);
}

function setupCountryAutoComplete() {
	logger.debug('>>> Entering setupCountryAutoComplete()');
	$('input[data-editor="DnnCountryAutocompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			logger.debug(`>>> Entering source(request=${request},response=${response})`);
			var control = this.element;
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			logger.debug(`<<< Exiting source(request=${request},response=${response})`);
			dnnCountryRegionService.listCountries(request.term, function (data) {
				if (data.length == 0) {
					clearCountryValue(control);
				};
				response($.map(data, function (item) {
					logger.debug('>>> Entering response()');
					return {
						label: item.FullName,
						id: item.Id,
						value: item.Id,
						name: item.Name
					};
					logger.debug('<<< Exiting response()');
				}))
			})
		},
		select: function (event, ui) {
			logger.debug(`>>> Entering select(event=${event},ui=${ui})`);
			$('#' + $(this).attr('id').replace('_name', '') + '_id').val(ui.item.id);
			$(this).attr('data-value', ui.item.id);
			$(this).attr('data-text', ui.item.name);
			logger.debug(`<<< Exiting select(event=${event},ui=${ui})`);
		},
		close: function () {
			logger.debug('>>> Entering close()');
			if ($(this).attr('data-text') != undefined) {
				$(this).val($(this).attr('data-text'));
				loadRegionList($(this).attr('data-category'), $(this).attr('data-value'));
			} else {
				clearCountryValue(this);
				logger.debug('<<< Exiting close()');
			}
		},
		focus: function (event, ui) {
			logger.debug(`>>> Entering focus(event=${event},ui=${ui})`);
			event.preventDefault();
			this.value = ui.item.name;
			$(this).attr('data-value', ui.item.id);
			logger.debug(`focus(event=${event},ui=${ui}): this.value → ${this.value}`);
			$(this).attr('data-text', ui.item.name);
			logger.debug(`<<< Exiting focus(event=${event},ui=${ui})`);
		}
	});
	$('input[data-editor="DnnCountryAutocompleteControl"]').change(function () {
		if ($(this).val().length < 2) {
			clearCountryValue(this);
		}
	});
	logger.debug('<<< Exiting setupCountryAutoComplete()');
}

function initCountryRegionControls() {
	logger.debug('>>> Entering initCountryRegionControls()');
	setupCountryAutoComplete();
	setupRegionLists();
	logger.debug('<<< Exiting initCountryRegionControls()');
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initCountryRegionControls);


(function ($) {
	$(document).ready(function () {
		logger.debug(`>>> Entering ready(document=${document})`);
		logger.debug(`<<< Exiting ready(document=${document})`);
		initCountryRegionControls();
	});
})(jQuery);
