const logger = console;
import { util } from "../../utils/helpers";

const Localization = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        let moduleName = "Prompt";
		logger.error(`get(key=${key}): moduleName → ${moduleName}`, { "event.action": "get", "logrite.key": key });
        return util.utilities.resx[moduleName][key];
    }
};
export default Localization;