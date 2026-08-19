import util from "../utils";
const logger = console;

const Localization = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        let moduleName = util.moduleName;
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.utilities.getResx(moduleName, key);
    }
};
export default Localization;