import utilities from "../utils";
const logger = console;
const extensions = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initExtensions();
        utilities.init(options);

        // eslint-disable-next-line no-undef
        require("../less/style.less");
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default extensions;
