const logger = console;
import { util } from "utils/helpers";

const promptInit = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initPrompt();

		logger.debug(`init(): options → ${options}`);
        util.init(options.utility);
        util.moduleName = options.moduleName;
		logger.debug(`init(): util.moduleName → ${util.moduleName}`);
        util.settings = options.settings;
		logger.debug(`init(): util.settings → ${util.settings}`);
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};

export default promptInit;

 
export const IS_DEV = process.env.NODE_ENV !== "production";