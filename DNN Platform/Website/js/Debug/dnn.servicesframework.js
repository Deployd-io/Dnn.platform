var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    $.dnnSF = function (moduleId) {
		logger.error(`>>> Entering dnnSF(moduleId=${moduleId})`, { "event.action": "dnnSF", "logrite.moduleId": moduleId });
        // To avoid scope issues, use 'base' instead of 'this'
        // to reference this class from internal events and functions.
        var base = this;

        base.getServiceRoot = function (moduleName) {
			logger.error(`>>> Entering getServiceRoot(moduleName=${moduleName})`, { "event.action": "getServiceRoot", "logrite.moduleName": moduleName });
            var serviceRoot = dnn.getVar("sf_siteRoot", "/");
            serviceRoot += "API/" + moduleName + "/";
			logger.error(`getServiceRoot(moduleName=${moduleName}): serviceRoot → ${serviceRoot}`, { "event.action": "getServiceRoot", "logrite.moduleName": moduleName });
			logger.warn(`<<< Exiting getServiceRoot(moduleName=${moduleName})`, { "event.action": "getServiceRoot", "logrite.moduleName": moduleName });
            return serviceRoot;
        };

        base.getTabId = function () {
			logger.error('>>> Entering getTabId()', { "event.action": "getTabId" });
			logger.warn('<<< Exiting getTabId()', { "event.action": "getTabId" });
            return dnn.getVar("sf_tabId", -1);
        };

        base.getModuleId = function () {
			logger.error('>>> Entering getModuleId()', { "event.action": "getModuleId" });
			logger.warn('<<< Exiting getModuleId()', { "event.action": "getModuleId" });
            return moduleId;
        };

        base.setModuleHeaders = function (xhr) {
			logger.error(`>>> Entering setModuleHeaders(xhr=${xhr})`, { "event.action": "setModuleHeaders", "logrite.xhr": xhr });
            var tabId = base.getTabId();
            if (tabId > -1) {
				logger.error(`setModuleHeaders(xhr=${xhr}): tabId → ${tabId}`, { "event.action": "setModuleHeaders", "logrite.xhr": xhr });
                xhr.setRequestHeader("ModuleId", base.getModuleId());
                xhr.setRequestHeader("TabId", tabId);
            }

            var afValue = base.getAntiForgeryValue();
			logger.error(`setModuleHeaders(xhr=${xhr}): afValue → ${afValue}`, { "event.action": "setModuleHeaders", "logrite.xhr": xhr });
            if (afValue) {
                xhr.setRequestHeader("RequestVerificationToken", afValue);
				logger.warn(`<<< Exiting setModuleHeaders(xhr=${xhr})`, { "event.action": "setModuleHeaders", "logrite.xhr": xhr });
            }
        };

        base.getAntiForgeryKey = function () {
			logger.error('>>> Entering getAntiForgeryKey()', { "event.action": "getAntiForgeryKey" });
			logger.warn('<<< Exiting getAntiForgeryKey()', { "event.action": "getAntiForgeryKey" });
            return "__RequestVerificationToken";
        };

        base.getAntiForgeryValue = function () {
			logger.error('>>> Entering getAntiForgeryValue()', { "event.action": "getAntiForgeryValue" });
			logger.warn('<<< Exiting getAntiForgeryValue()', { "event.action": "getAntiForgeryValue" });
            return $('[name="__RequestVerificationToken"]').val();
        };

        return base;
    };

    $.ServicesFramework = function (moduleId) {
		logger.error(`>>> Entering ServicesFramework(moduleId=${moduleId})`, { "event.action": "ServicesFramework", "logrite.moduleId": moduleId });
		logger.warn(`<<< Exiting ServicesFramework(moduleId=${moduleId})`, { "event.action": "ServicesFramework", "logrite.moduleId": moduleId });
        return new $.dnnSF(moduleId);
    };

})(jQuery);
//----------------------------
