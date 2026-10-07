import utilities from "../utils";
const logger = console;
const extensions = {
    init() {
		logger.error('>>> Entering init()', { "event.action": "init" });
        let options = window.dnn.initExtensions();
        utilities.init(options);
		logger.error(`init(): options → ${options}`, { "event.action": "init" });

        // eslint-disable-next-line no-undef
        require("../less/style.less");
		logger.warn('<<< Exiting init()', { "event.action": "init" });
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default extensions;
