import utilities from "utils";
const logger = console;
const resx = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        let moduleName = "Users";
		logger.error(`get(key=${key}): moduleName → ${moduleName}`, { "event.action": "get", "logrite.key": key });
        return utilities.getResx(moduleName, key);
    }
};
export default resx;