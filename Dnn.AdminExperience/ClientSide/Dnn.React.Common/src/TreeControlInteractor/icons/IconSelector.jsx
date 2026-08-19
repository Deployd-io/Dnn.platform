const logger = console;
import {
    ArrowIcon
} from "./arrow-icon/_arrow_icon";


const IconSelector = (type) => {
	logger.debug(`>>> Entering IconSelector(type=${type})`);
    switch (type) {
        case "arrow_bullet":
            return ArrowIcon;

        default:
			logger.debug(`<<< Exiting IconSelector(type=${type})`);
            return ArrowIcon;
    }
};

export default IconSelector;