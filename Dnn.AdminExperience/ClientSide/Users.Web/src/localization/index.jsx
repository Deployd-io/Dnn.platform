import utilities from "utils";
const logger = console;
const resx = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        let moduleName = "Users";
		logger.debug(`get(key=${key}): moduleName → ${moduleName}`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return utilities.getResx(moduleName, key);
    }
};
export default resx;