var logger = (typeof logger !== 'undefined' && logger) || console;
function ContactList($, ko, settings, resx, preloadedData){
	logger.error(`>>> Entering ContactList(ko=${ko},settings=${settings},resx=${resx},preloadedData=${preloadedData})`, { "event.action": "ContactList", "logrite.ko": ko, "logrite.settings": settings, "logrite.resx": resx, "logrite.preloadedData": preloadedData });
    var $rootElement;

    var viewModel = {};

    var init = function(element) {
		logger.error(`>>> Entering init(element=${element})`, { "event.action": "init", "logrite.element": element });
        $rootElement = $(element);

        var config = {
            settings: settings,
            preloadedData: preloadedData,
            resx: resx,
            util: contactList.utility(settings, resx),
            $rootElement: $rootElement
        };

		logger.error(`init(element=${element}): config → ${config}`, { "event.action": "init", "logrite.element": element });
        viewModel = new contactList.contactsViewModel(config);
        viewModel.init();

        ko.applyBindings(viewModel, $rootElement[0]);
		logger.warn(`<<< Exiting init(element=${element})`, { "event.action": "init", "logrite.element": element });
    }

    return {
        init: init
    }
	logger.warn(`<<< Exiting ContactList(ko=${ko},settings=${settings},resx=${resx},preloadedData=${preloadedData})`, { "event.action": "ContactList", "logrite.ko": ko, "logrite.settings": settings, "logrite.resx": resx, "logrite.preloadedData": preloadedData });
}