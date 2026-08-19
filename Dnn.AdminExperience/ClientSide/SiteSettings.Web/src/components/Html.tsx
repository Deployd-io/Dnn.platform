import parse from "html-react-parser";
import DOMPurify from "dompurify";
const logger = console;

export default function Html({html}) {
	logger.debug(`>>> Entering Html(html=${html})`);
	logger.debug(`<<< Exiting Html(html=${html})`);
    return parse(DOMPurify.sanitize(html));
}