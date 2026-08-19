const logger = console;
﻿(function ($) {
	logger.debug('>>> Entering function()');
    $.dnnSF = function (moduleId) {
        // To avoid scope issues, use 'base' instead of 'this'
        // to reference this class from internal events and functions.
        var base = this;

        base.getServiceRoot = function (moduleName) {
            var serviceRoot = dnn.getVar("sf_siteRoot", "/");
            serviceRoot += "API/" + moduleName + "/";
			logger.debug(`function(moduleName=${moduleName}): serviceRoot → ${serviceRoot}`);
            return serviceRoot;
        };

        base.getTabId = function () {
            return dnn.getVar("sf_tabId", -1);
        };

        base.getModuleId = function () {
            return moduleId;
        };

        base.setModuleHeaders = function (xhr) {
            var tabId = base.getTabId();
			logger.debug(`function(xhr=${xhr}): tabId → ${tabId}`);
            if (tabId > -1) {
                xhr.setRequestHeader("ModuleId", base.getModuleId());
                xhr.setRequestHeader("TabId", tabId);
            }

            var afValue = base.getAntiForgeryValue();
			logger.debug(`function(xhr=${xhr}): afValue → ${afValue}`);
            if (afValue) {
                xhr.setRequestHeader("RequestVerificationToken", afValue);
            }
        };

        base.getAntiForgeryKey = function () {
            return "__RequestVerificationToken";
        };

        base.getAntiForgeryValue = function () {
            return $('[name="__RequestVerificationToken"]').val();
        };

        return base;
    };

    $.ServicesFramework = function (moduleId) {
		logger.debug(`<<< Exiting function(moduleId=${moduleId})`);
        return new $.dnnSF(moduleId);
    };

})(jQuery);
//----------------------------
