const logger = console;
import { util } from "utils/helpers";

const Localization = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        let moduleName = util.moduleName;
		logger.warn(`<<< Exiting get(key=${key})`, { "event.action": "get", "logrite.key": key });
        return util.utilities.getResx(moduleName, key);
    }
};
export default Localization;