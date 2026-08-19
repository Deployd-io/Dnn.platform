import util from "../utils";
const logger = console;

const resx = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        let moduleName = util.moduleName;
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.utilities.getResx(moduleName, key);
    }
};
export default resx;