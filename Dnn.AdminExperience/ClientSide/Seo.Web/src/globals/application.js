import utilities from "../utils";
import "../less/style.less";
const logger = console;

const boilerPlate = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initSeo();
        utilities.init(options.utility);
		logger.debug(`init(): options → ${options}`);
		logger.debug(`init(): utilities.settings → ${utilities.settings}`);
        utilities.moduleName = options.moduleName;
		logger.debug(`init(): utilities.moduleName → ${utilities.moduleName}`);
        utilities.settings = options.settings;
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;