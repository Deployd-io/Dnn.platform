import utilities from "utils/applicationSettings";
import "../less/style.less";
const logger = console;

const Sites = {
    init(initCallback) {
		logger.error(`>>> Entering init(initCallback=${initCallback})`, { "event.action": "init", "logrite.initCallback": initCallback });
        if (typeof window.dnn[initCallback] === "function") {
            let options = window.dnn[initCallback]();
            utilities.init(options);
			logger.error(`init(initCallback=${initCallback}): options → ${options}`, { "event.action": "init", "logrite.initCallback": initCallback });
        }
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default Sites;