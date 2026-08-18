import utilities from "../utils";
import "../less/style.less";
const logger = console;

const vocabularies = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initVocabularies();

		logger.debug(`init(): options → ${options}`);
        utilities.init(options);
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default vocabularies;