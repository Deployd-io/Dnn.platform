const logger = console;
﻿function ContactList($, ko, settings, resx, preloadedData){
	logger.debug(`>>> Entering ContactList(ko=${ko},settings=${settings},resx=${resx},preloadedData=${preloadedData})`);
    var $rootElement;

    var viewModel = {};

    var init = function(element) {
		logger.debug(`>>> Entering init(element=${element})`);
        $rootElement = $(element);

        var config = {
            settings: settings,
            preloadedData: preloadedData,
            resx: resx,
            util: contactList.utility(settings, resx),
            $rootElement: $rootElement
        };

		logger.debug(`init(element=${element}): config → ${config}`);
        viewModel = new contactList.contactsViewModel(config);
        viewModel.init();

        ko.applyBindings(viewModel, $rootElement[0]);
		logger.debug(`<<< Exiting init(element=${element})`);
    }

    return {
        init: init
    }
	logger.debug(`<<< Exiting ContactList(ko=${ko},settings=${settings},resx=${resx},preloadedData=${preloadedData})`);
}