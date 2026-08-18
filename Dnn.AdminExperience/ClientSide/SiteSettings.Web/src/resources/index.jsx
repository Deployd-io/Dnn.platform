import util from "../utils";
const logger = console;

const resx = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.utilities.getResx("SiteSettings", key);
    }
};
export default resx;