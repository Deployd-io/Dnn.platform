import util from "../utils";
const logger = console;
//import "fetch-ie8";
function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}
class VocabularyService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = util.utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }
    getVocabularyList(searchParameters, callback, failureCallback) {
		logger.error(`>>> Entering getVocabularyList(searchParameters=${searchParameters},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getVocabularyList", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");
        searchParameters = Object.assign({}, searchParameters, {
            scopeTypeId: searchParameters.scopeTypeId > 0 ? searchParameters.scopeTypeId : "*"
        });
        sf.get("GetVocabularies?" + serializeQueryStringParameters(searchParameters), {}, callback, failureCallback);
		logger.error(`getVocabularyList(searchParameters=${searchParameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "getVocabularyList", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    addVocabulary(vocabulary, callback, failureCallback) {
		logger.error(`>>> Entering addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");

		logger.error(`addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        sf.post("CreateVocabulary", vocabulary, callback, failureCallback);
		logger.warn(`<<< Exiting addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    updateVocabulary(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateVocabulary", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");

		logger.error(`updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateVocabulary", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        sf.post("UpdateVocabulary", payload, callback, failureCallback);
		logger.warn(`<<< Exiting updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateVocabulary", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    deleteVocabulary(vocabularyId, callback, failureCallback) {
		logger.error(`>>> Entering deleteVocabulary(vocabularyId=${vocabularyId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteVocabulary", "logrite.vocabularyId": vocabularyId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");
        sf.post("DeleteVocabulary?vocabularyId=" + vocabularyId, {}, callback, failureCallback);
		logger.error(`deleteVocabulary(vocabularyId=${vocabularyId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteVocabulary", "logrite.vocabularyId": vocabularyId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const vocabularyService = new VocabularyService();
export default vocabularyService;