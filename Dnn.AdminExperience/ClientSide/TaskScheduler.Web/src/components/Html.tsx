import parse from "html-react-parser";
import DOMPurify from "dompurify";
const logger = console;

export default function Html({html}) {
	logger.error('>>> Entering Html()', { "event.action": "Html" });
	logger.warn('<<< Exiting Html()', { "event.action": "Html" });
    return parse(DOMPurify.sanitize(html));
}