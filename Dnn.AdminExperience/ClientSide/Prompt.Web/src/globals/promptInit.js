const logger = console;
import { util } from "utils/helpers";

const promptInit = {
    init() {
		logger.error('>>> Entering init()', { "event.action": "init" });
        let options = window.dnn.initPrompt();

		logger.error(`init(): options → ${options}`, { "event.action": "init" });
        util.init(options.utility);
        util.moduleName = options.moduleName;
        util.settings = options.settings;
		logger.warn('<<< Exiting init()', { "event.action": "init" });
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};

export default promptInit;

 
export const IS_DEV = process.env.NODE_ENV !== "production";