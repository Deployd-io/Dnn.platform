import util from "../utils";
const logger = console;
//import "fetch-ie8";
function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}
class VocabularyService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);

		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }
    getVocabularyList(searchParameters, callback, failureCallback) {
		logger.debug(`>>> Entering getVocabularyList(searchParameters=${searchParameters},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");
        searchParameters = Object.assign({}, searchParameters, {
            scopeTypeId: searchParameters.scopeTypeId > 0 ? searchParameters.scopeTypeId : "*"
        });
        sf.get("GetVocabularies?" + serializeQueryStringParameters(searchParameters), {}, callback, failureCallback);
		logger.debug(`getVocabularyList(searchParameters=${searchParameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getVocabularyList(searchParameters=${searchParameters},callback=${callback},failureCallback=${failureCallback})`);
    }
    addVocabulary(vocabulary, callback, failureCallback) {
		logger.debug(`>>> Entering addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");

		logger.debug(`addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
        sf.post("CreateVocabulary", vocabulary, callback, failureCallback);
		logger.debug(`<<< Exiting addVocabulary(vocabulary=${vocabulary},callback=${callback},failureCallback=${failureCallback})`);
    }
    updateVocabulary(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");

		logger.debug(`updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
        sf.post("UpdateVocabulary", payload, callback, failureCallback);
		logger.debug(`<<< Exiting updateVocabulary(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
    deleteVocabulary(vocabularyId, callback, failureCallback) {
		logger.debug(`>>> Entering deleteVocabulary(vocabularyId=${vocabularyId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");
        sf.post("DeleteVocabulary?vocabularyId=" + vocabularyId, {}, callback, failureCallback);
		logger.debug(`deleteVocabulary(vocabularyId=${vocabularyId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteVocabulary(vocabularyId=${vocabularyId},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const vocabularyService = new VocabularyService();
export default vocabularyService;