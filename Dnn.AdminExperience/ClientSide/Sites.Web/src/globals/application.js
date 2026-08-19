import utilities from "utils/applicationSettings";
import "../less/style.less";
const logger = console;

const Sites = {
    init(initCallback) {
		logger.debug(`>>> Entering init(initCallback=${initCallback})`);
        if (typeof window.dnn[initCallback] === "function") {
            let options = window.dnn[initCallback]();
            utilities.init(options);
			logger.debug(`init(initCallback=${initCallback}): options → ${options}`);
        }
		logger.debug(`<<< Exiting init(initCallback=${initCallback})`);
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default Sites;