var logger = (typeof logger !== 'undefined' && logger) || console;
var dnnCountryRegionService;

function CountryRegionService($) {
	logger.error('>>> Entering CountryRegionService()', { "event.action": "CountryRegionService" });
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'CountryRegion/';

	logger.error(`CountryRegionService(): baseServicepath → ${baseServicepath}`, { "event.action": "CountryRegionService" });
	this.listCountries = function (searchString, success) {
		logger.error(`>>> Entering listCountries(searchString=${searchString},success=${success})`, { "event.action": "listCountries", "logrite.searchString": searchString, "logrite.success": success });
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
		logger.warn(`<<< Exiting listCountries(searchString=${searchString},success=${success})`, { "event.action": "listCountries", "logrite.searchString": searchString, "logrite.success": success });
	};

	this.listRegions = function (country, success) {
		logger.error(`>>> Entering listRegions(country=${country},success=${success})`, { "event.action": "listRegions", "logrite.country": country, "logrite.success": success });
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
		logger.warn(`<<< Exiting listRegions(country=${country},success=${success})`, { "event.action": "listRegions", "logrite.country": country, "logrite.success": success });
	logger.warn('<<< Exiting CountryRegionService()', { "event.action": "CountryRegionService" });
	};

}

function setupRegionLists() {
	logger.error('>>> Entering setupRegionLists()', { "event.action": "setupRegionLists" });
	$('div[data-list="Region"]').each(function (index, value) {
		var stringValue = $(value).children('input[data-editor="DNNRegionEditControl_Hidden"]').val();
		var $row = $(value);
		var $label = $row.prev().find('label');
		var $textControl = $row.children('input[data-editor="DNNRegionEditControl_Text"]');
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
		};
	});
	$('select[data-editor="DNNRegionEditControl_DropDown"]').change(function () {
		$(this).parent().children('input[data-editor="DNNRegionEditControl_Hidden"]').val($(this).val());
	});
	logger.warn('<<< Exiting setupRegionLists()', { "event.action": "setupRegionLists" });
}

function loadRegionList(category, country) {
	logger.error(`>>> Entering loadRegionList(category=${category},country=${country})`, { "event.action": "loadRegionList", "logrite.category": category, "logrite.country": country });
	var selector = category ? 'div[data-list="Region"][data-category="' + category + '"]' : 'div[data-list="Region"]';
	$(selector).each(function (index, value) {
		logger.error(`loadRegionList(category=${category},country=${country}): selector → ${selector}`, { "event.action": "loadRegionList", "logrite.category": category, "logrite.country": country });
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
			logger.warn(`<<< Exiting loadRegionList(category=${category},country=${country})`, { "event.action": "loadRegionList", "logrite.category": category, "logrite.country": country });
		}
	});
}

function clearCountryValue(countryControl) {
	logger.error(`>>> Entering clearCountryValue(countryControl=${countryControl})`, { "event.action": "clearCountryValue", "logrite.countryControl": countryControl });
	$('#' + $(countryControl).attr('id').replace('_name', '') + '_id').val('');
	$(countryControl).attr('data-value', '');
	$(countryControl).attr('value', '');
	loadRegionList($(countryControl).attr('data-category'), '');
	setupRegionLists();
	logger.warn(`<<< Exiting clearCountryValue(countryControl=${countryControl})`, { "event.action": "clearCountryValue", "logrite.countryControl": countryControl });
}

function setupCountryAutoComplete() {
	logger.error('>>> Entering setupCountryAutoComplete()', { "event.action": "setupCountryAutoComplete" });
	$('input[data-editor="DnnCountryAutocompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			logger.error(`>>> Entering source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
			var control = this.element;
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			dnnCountryRegionService.listCountries(request.term, function (data) {
				if (data.length == 0) {
					clearCountryValue(control);
					logger.warn(`<<< Exiting source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
				};
				response($.map(data, function (item) {
					return {
						label: item.FullName,
						id: item.Id,
						value: item.Id,
						name: item.Name
					};
				}))
			})
		},
		select: function (event, ui) {
			logger.error(`>>> Entering select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
			$('#' + $(this).attr('id').replace('_name', '') + '_id').val(ui.item.id);
			$(this).attr('data-value', ui.item.id);
			$(this).attr('data-text', ui.item.name);
			logger.warn(`<<< Exiting select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
		},
		close: function () {
			logger.error('>>> Entering close()', { "event.action": "close" });
			if ($(this).attr('data-text') != undefined) {
				$(this).val($(this).attr('data-text'));
				loadRegionList($(this).attr('data-category'), $(this).attr('data-value'));
			} else {
				clearCountryValue(this);
				logger.warn('<<< Exiting close()', { "event.action": "close" });
			}
		},
		focus: function (event, ui) {
			logger.error(`>>> Entering focus(event=${event},ui=${ui})`, { "event.action": "focus", "logrite.event": event, "logrite.ui": ui });
			event.preventDefault();
			this.value = ui.item.name;
			$(this).attr('data-value', ui.item.id);
			logger.debug(`focus(event=${event},ui=${ui}): this.value → ${this.value}`, { "event.action": "focus", "logrite.event": event, "logrite.ui": ui });
			$(this).attr('data-text', ui.item.name);
			logger.warn(`<<< Exiting focus(event=${event},ui=${ui})`, { "event.action": "focus", "logrite.event": event, "logrite.ui": ui });
		}
	});
	$('input[data-editor="DnnCountryAutocompleteControl"]').change(function () {
		if ($(this).val().length < 2) {
			clearCountryValue(this);
		}
	});
}

function initCountryRegionControls() {
	logger.error('>>> Entering initCountryRegionControls()', { "event.action": "initCountryRegionControls" });
	setupCountryAutoComplete();
	setupRegionLists();
	logger.warn('<<< Exiting initCountryRegionControls()', { "event.action": "initCountryRegionControls" });
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initCountryRegionControls);


(function ($) {
	$(document).ready(function () {
		initCountryRegionControls();
	});
})(jQuery);
