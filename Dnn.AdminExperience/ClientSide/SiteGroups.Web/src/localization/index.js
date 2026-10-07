import util from "utils";
const logger = console;

const Localization = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        let moduleName = "SiteGroups";
		logger.error(`get(key=${key}): moduleName → ${moduleName}`, { "event.action": "get", "logrite.key": key });
        return util.getResx(moduleName, key);
    }
};
export default Localization;