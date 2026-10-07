import Api from "./api";
const logger = console;

const templateService = {
    savePageAsTemplate(template) {
		logger.error(`>>> Entering savePageAsTemplate(template=${template})`, { "event.action": "savePageAsTemplate", "logrite.template": template });
        const api = new Api("Pages");
		logger.error(`savePageAsTemplate(template=${template}): api → ${api}`, { "event.action": "savePageAsTemplate", "logrite.template": template });
        return api.post("SavePageAsTemplate", template);
    }
};
export default templateService;