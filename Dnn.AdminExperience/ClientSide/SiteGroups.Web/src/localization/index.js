import util from "utils";
const logger = console;

const Localization = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        let moduleName = "SiteGroups";
		logger.debug(`get(key=${key}): moduleName → ${moduleName}`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.getResx(moduleName, key);
    }
};
export default Localization;