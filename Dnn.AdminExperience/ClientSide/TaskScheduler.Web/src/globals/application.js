import utilities from "../utils";
import "../less/style.less";
const logger = console;

const boilerPlate = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initScheduler();

		logger.debug(`init(): options → ${options}`);
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
		logger.debug(`init(): utilities.moduleName → ${utilities.moduleName}`);
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;