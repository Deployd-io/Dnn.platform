const logger = console;
import { util } from "../../utils/helpers";

const Localization = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        let moduleName = "Prompt";
		logger.debug(`get(key=${key}): moduleName → ${moduleName}`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.utilities.resx[moduleName][key];
    }
};
export default Localization;