var logger = (typeof logger !== 'undefined' && logger) || console;
var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettings = function (config, root, moduleId, ko) {
	logger.error(`>>> Entering quickSettings(config=${config},root=${root},moduleId=${moduleId},ko=${ko})`, { "event.action": "quickSettings", "logrite.config": config, "logrite.root": root, "logrite.moduleId": moduleId, "logrite.ko": ko });
    var savedIsFormEnabled = config.isFormEnabled;
    var quickSettingsDispatcher = config.quickSettingsDispatcher;

    var viewModel = {
        isFormEnabled: ko.observable(savedIsFormEnabled)
    };

    var settings = {
        servicesFramework: $.ServicesFramework(moduleId)
    }
    var util = contactList.utility(settings, {});
	logger.error(`quickSettings(config=${config},root=${root},moduleId=${moduleId},ko=${ko}): util → ${util}`, { "event.action": "quickSettings", "logrite.config": config, "logrite.root": root, "logrite.moduleId": moduleId, "logrite.ko": ko });
    util.settingsService = function(){
		logger.error('>>> Entering settingsService()', { "event.action": "settingsService" });
        util.sf.serviceController = "Settings";
		logger.warn('<<< Exiting settingsService()', { "event.action": "settingsService" });
        return util.sf;
    };

    // The function dnnQuickSettings definded in 'ModuleActions.js' requires working with promises.
    var SaveSettings = function () {
		logger.error('>>> Entering SaveSettings()', { "event.action": "SaveSettings" });
        var deferred = $.Deferred();

		logger.error(`SaveSettings(): deferred → ${deferred}`, { "event.action": "SaveSettings" });
        if (viewModel.isFormEnabled() == savedIsFormEnabled){
            deferred.resolve();    
        } else {
            var params = {
                isFormEnabled: viewModel.isFormEnabled()
            };

            util.settingsService().post("SaveSettings", params,
                function(data){
                    //Success
                    deferred.resolve();
                    savedIsFormEnabled = params.isFormEnabled;
                    quickSettingsDispatcher.notify(moduleId, quickSettingsDispatcher.eventTypes.SAVE, params);
                },

                function(data){
                    //Failure
                    deferred.reject();
                }
            );
        }
        
		logger.warn('<<< Exiting SaveSettings()', { "event.action": "SaveSettings" });
        return deferred.promise();
    };

    var CancelSettings = function () {
		logger.error('>>> Entering CancelSettings()', { "event.action": "CancelSettings" });
        var deferred = $.Deferred();
        viewModel.isFormEnabled(savedIsFormEnabled);
		logger.error(`CancelSettings(): deferred → ${deferred}`, { "event.action": "CancelSettings" });
        deferred.resolve();

        quickSettingsDispatcher.notify(moduleId, quickSettingsDispatcher.eventTypes.CANCEL);
		logger.warn('<<< Exiting CancelSettings()', { "event.action": "CancelSettings" });
        return deferred.promise();
    };

    var init = function () {
		logger.error('>>> Entering init()', { "event.action": "init" });
        var $root = $("#"+ root);
        ko.applyBindings(viewModel, $root[0]);

		logger.warn('<<< Exiting init()', { "event.action": "init" });
        // dnnQuickSettings needs three parameters: moduleId, onSave and onCancel.
        // These two functions are associated to the save and cancel buttons and the
        // callbacks mechanism is based on promises.
        $(root).dnnQuickSettings({
            moduleId: moduleId,
            onSave: SaveSettings,
            onCancel: CancelSettings
        });
    }

    return {
        init: init
    }
};
