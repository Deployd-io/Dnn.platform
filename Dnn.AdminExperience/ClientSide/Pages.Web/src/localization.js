import utils from "./utils";
const logger = console;

const localization = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        const moduleName = utils.getModuleName();
		logger.error(`get(key=${key}): moduleName → ${moduleName}`, { "event.action": "get", "logrite.key": key });
        return utils.getResx(moduleName, key);
    }	
};
export default localization;
