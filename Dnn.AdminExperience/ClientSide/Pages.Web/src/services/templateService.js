import Api from "./api";
const logger = console;

const templateService = {
    savePageAsTemplate(template) {
		logger.debug(`>>> Entering savePageAsTemplate(template=${template})`); const _start = Date.now();
        const api = new Api("Pages");
		logger.debug(`savePageAsTemplate(template=${template}): api → ${api}`);
		logger.info(`savePageAsTemplate(template=${template}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting savePageAsTemplate(template=${template})`);
        return api.post("SavePageAsTemplate", template);
    }
};
export default templateService;