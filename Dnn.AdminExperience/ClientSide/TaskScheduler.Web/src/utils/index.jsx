const logger = console;
const utils = {
    init(utilities) {
		logger.debug(`>>> Entering init(utilities=${utilities})`);
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;      
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`);
		logger.debug(`<<< Exiting init(utilities=${utilities})`);
    },

    formatString() {
		logger.debug('>>> Entering formatString()');
        const format = arguments[0];
        const methodsArgs = arguments;
        return format.replace(/[{[](\d+)[\]}]/gi, function (_, index) {
            let argsIndex = parseInt(index) + 1;
			logger.debug(`<<< Exiting function(argsIndex=${argsIndex})`);
		logger.debug('<<< Exiting formatString()');
            return methodsArgs[argsIndex];
        });
    },

    utilities: null
};
export default utils;