import util from "../utils";
const logger = console;

const resx = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
		logger.warn(`<<< Exiting get(key=${key})`, { "event.action": "get", "logrite.key": key });
        return util.utilities.getResx("SiteSettings", key);
    }
};
export default resx;