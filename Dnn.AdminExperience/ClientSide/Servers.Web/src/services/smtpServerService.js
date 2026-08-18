import serviceFramework from "./serviceFramework";
import utils from "../utils";
const logger = console;

function getControllerName() {
	logger.debug('>>> Entering getControllerName()');
	logger.debug('<<< Exiting getControllerName()');
    return utils.isHostUser() ? "ServerSettingsSmtpHost" : "ServerSettingsSmtpAdmin";
}

const getSmtpSettings = function () {    
	logger.debug('>>> Entering getSmtpSettings()');
	logger.debug('<<< Exiting getSmtpSettings()');
    return serviceFramework.get(getControllerName(), "GetSmtpSettings");
};

const updateSmtpSettings = function (parameters) {    
	logger.debug(`>>> Entering updateSmtpSettings(parameters=${parameters})`);
	logger.debug(`<<< Exiting updateSmtpSettings(parameters=${parameters})`);
    return serviceFramework.post(getControllerName(), "UpdateSmtpSettings", parameters);
};

const sendTestEmail = function (parameters) {    
	logger.debug(`>>> Entering sendTestEmail(parameters=${parameters})`);
	logger.debug(`<<< Exiting sendTestEmail(parameters=${parameters})`);
    return serviceFramework.post(getControllerName(), "SendTestEmail", parameters);
};

const getOAuthProviders = function () {
	logger.debug('>>> Entering getOAuthProviders()');
	logger.debug('<<< Exiting getOAuthProviders()');
    return serviceFramework.get(getControllerName(), "GetSmtpOAuthProviders");
};

const smtpServerService = {
    getSmtpSettings,
    updateSmtpSettings,
    sendTestEmail,
    getOAuthProviders
};

export default smtpServerService; 