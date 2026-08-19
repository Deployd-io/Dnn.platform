import utils from "./utils";
const logger = console;

const localization = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        const moduleName = utils.getModuleName();
		logger.debug(`get(key=${key}): moduleName → ${moduleName}`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return utils.getResx(moduleName, key);
    }	
};
export default localization;
