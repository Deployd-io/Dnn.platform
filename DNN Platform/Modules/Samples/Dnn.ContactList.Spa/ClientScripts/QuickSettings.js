const logger = console;
﻿var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettings = function (config, root, moduleId, ko) {
	logger.debug(`>>> Entering function(config=${config},root=${root},moduleId=${moduleId},ko=${ko})`);
	logger.debug('>>> Entering function()');
    var savedIsFormEnabled = config.isFormEnabled;
    var quickSettingsDispatcher = config.quickSettingsDispatcher;

    var viewModel = {
        isFormEnabled: ko.observable(savedIsFormEnabled)
    };

    var settings = {
        servicesFramework: $.ServicesFramework(moduleId)
    }
    var util = contactList.utility(settings, {});
	logger.debug(`function(config=${config},root=${root},moduleId=${moduleId},ko=${ko}): util → ${util}`);
    util.settingsService = function(){
        util.sf.serviceController = "Settings";
        return util.sf;
    };

    // The function dnnQuickSettings definded in 'ModuleActions.js' requires working with promises.
    var SaveSettings = function () {
		logger.debug('>>> Entering SaveSettings()');
        var deferred = $.Deferred();

        if (viewModel.isFormEnabled() == savedIsFormEnabled){
			logger.debug(`SaveSettings(): deferred → ${deferred}`);
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
					logger.debug(`<<< Exiting function(config=${config},root=${root},moduleId=${moduleId},ko=${ko})`);
                }
            );
        }
        
		logger.debug('<<< Exiting SaveSettings()');
        return deferred.promise();
    };

    var CancelSettings = function () {
		logger.debug('>>> Entering CancelSettings()');
        var deferred = $.Deferred();
        viewModel.isFormEnabled(savedIsFormEnabled);
		logger.debug(`CancelSettings(): deferred → ${deferred}`);
        deferred.resolve();

        quickSettingsDispatcher.notify(moduleId, quickSettingsDispatcher.eventTypes.CANCEL);
		logger.debug('<<< Exiting CancelSettings()');
        return deferred.promise();
    };

    var init = function () {
		logger.debug('>>> Entering init()');
        var $root = $("#"+ root);
        ko.applyBindings(viewModel, $root[0]);

		logger.debug('<<< Exiting init()');
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
