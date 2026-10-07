const logger = console;
import {
    ArrowIcon
} from "./arrow-icon/_arrow_icon";


const IconSelector = (type) => {
	logger.error(`>>> Entering IconSelector(type=${type})`, { "event.action": "IconSelector", "logrite.type": type });
    switch (type) {
        case "arrow_bullet":
            return ArrowIcon;

        default:
			logger.warn(`<<< Exiting IconSelector(type=${type})`, { "event.action": "IconSelector", "logrite.type": type });
            return ArrowIcon;
    }
};

export default IconSelector;