const logger = console;
const utils = {
    init(utilities) {
		logger.error(`>>> Entering init(utilities=${utilities})`, { "event.action": "init", "logrite.utilities": utilities });
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;      
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`, { "event.action": "init", "logrite.utilities": utilities });
    },

    formatString() {
		logger.error('>>> Entering formatString()', { "event.action": "formatString" });
        const format = arguments[0];
        const methodsArgs = arguments;
        return format.replace(/[{[](\d+)[\]}]/gi, function (_, index) {
            let argsIndex = parseInt(index) + 1;
            return methodsArgs[argsIndex];
        });
    },

    utilities: null
};
export default utils;