const logger = console;
const string = {
    format() {
        let format = arguments[0];
        let methodsArgs = arguments;
        return format.replace(/{(\d+)}/gi, function (value, index) {
            let argsIndex = parseInt(index) + 1;
		logger.debug('<<< Exiting format()');
            return methodsArgs[argsIndex];
        });
    }
};
export default string;