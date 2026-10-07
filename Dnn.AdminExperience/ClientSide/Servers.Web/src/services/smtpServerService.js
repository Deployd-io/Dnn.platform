import serviceFramework from "./serviceFramework";
import utils from "../utils";
const logger = console;

function getControllerName() {
	logger.error('>>> Entering getControllerName()', { "event.action": "getControllerName" });
	logger.warn('<<< Exiting getControllerName()', { "event.action": "getControllerName" });
    return utils.isHostUser() ? "ServerSettingsSmtpHost" : "ServerSettingsSmtpAdmin";
}

const getSmtpSettings = function () {    
	logger.error('>>> Entering getSmtpSettings()', { "event.action": "getSmtpSettings" });
	logger.warn('<<< Exiting getSmtpSettings()', { "event.action": "getSmtpSettings" });
    return serviceFramework.get(getControllerName(), "GetSmtpSettings");
};

const updateSmtpSettings = function (parameters) {    
	logger.error(`>>> Entering updateSmtpSettings(parameters=${parameters})`, { "event.action": "updateSmtpSettings", "logrite.parameters": parameters });
	logger.warn(`<<< Exiting updateSmtpSettings(parameters=${parameters})`, { "event.action": "updateSmtpSettings", "logrite.parameters": parameters });
    return serviceFramework.post(getControllerName(), "UpdateSmtpSettings", parameters);
};

const sendTestEmail = function (parameters) {    
	logger.error(`>>> Entering sendTestEmail(parameters=${parameters})`, { "event.action": "sendTestEmail", "logrite.parameters": parameters });
	logger.warn(`<<< Exiting sendTestEmail(parameters=${parameters})`, { "event.action": "sendTestEmail", "logrite.parameters": parameters });
    return serviceFramework.post(getControllerName(), "SendTestEmail", parameters);
};

const getOAuthProviders = function () {
	logger.error('>>> Entering getOAuthProviders()', { "event.action": "getOAuthProviders" });
	logger.warn('<<< Exiting getOAuthProviders()', { "event.action": "getOAuthProviders" });
    return serviceFramework.get(getControllerName(), "GetSmtpOAuthProviders");
};

const smtpServerService = {
    getSmtpSettings,
    updateSmtpSettings,
    sendTestEmail,
    getOAuthProviders
};

export default smtpServerService; 