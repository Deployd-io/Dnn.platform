const logger = console;
let instance = null;

export default class DomKey {

    constructor() {
		logger.debug('>>> Entering constructor()');
        if (!instance) {
            instance = this;
            this.key = 0;
        }

		logger.debug(`constructor(): this.key → ${this.key}`);
		logger.debug('<<< Exiting constructor()');
        return instance;
    }

    static get(prefix = "") {
		logger.debug(`>>> Entering get(prefix=${prefix})`);
        const self = instance ? instance : new DomKey();
		logger.debug(`get(prefix=${prefix}): self → ${self}`);
		logger.debug(`<<< Exiting get(prefix=${prefix})`);
        return `${prefix}-${self.key++}`;
    }
}
