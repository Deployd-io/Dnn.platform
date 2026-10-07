const logger = console;
let instance = null;

export default class DomKey {

    constructor() {
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        if (!instance) {
            instance = this;
            this.key = 0;
        }

		logger.debug(`constructor(): this.key → ${this.key}`, { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
        return instance;
    }

    static get(prefix = "") {
		logger.error(`>>> Entering get(prefix=${prefix})`, { "event.action": "get", "logrite.prefix": prefix });
        const self = instance ? instance : new DomKey();
		logger.error(`get(prefix=${prefix}): self → ${self}`, { "event.action": "get", "logrite.prefix": prefix });
        return `${prefix}-${self.key++}`;
    }
}
