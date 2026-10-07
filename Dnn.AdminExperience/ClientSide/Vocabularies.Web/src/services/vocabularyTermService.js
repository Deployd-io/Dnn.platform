import util from "../utils";
const logger = console;
class VocabularyTermService {
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
    getVocabularyTerms(vocabularyTermId, callback, failureCallback) {
		logger.error(`>>> Entering getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyTermId": vocabularyTermId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");

		logger.error(`getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "getVocabularyTerms", "logrite.vocabularyTermId": vocabularyTermId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        sf.get("GetTermsByVocabularyId?vocabularyId=" + vocabularyTermId + "&pageIndex=0", {}, callback, failureCallback);
		logger.warn(`<<< Exiting getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyTermId": vocabularyTermId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    addVocabularyTerm(payload, callback, failureCallback) {
		logger.error(`>>> Entering addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");

		logger.error(`addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        sf.post("CreateTerm", payload, callback, failureCallback);
		logger.warn(`<<< Exiting addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    updateVocabularyTerm(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Vocabularies");

		logger.error(`updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        sf.post("UpdateTerm", payload, callback, failureCallback);
		logger.warn(`<<< Exiting updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateVocabularyTerm", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    deleteVocabularyTerm(termId, callback, failureCallback) {
	logger.error(`>>> Entering deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteVocabularyTerm", "logrite.termId": termId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });

        const sf = this.getServiceFramework("Vocabularies");
		logger.error(`deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteVocabularyTerm", "logrite.termId": termId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });

        sf.post("DeleteTerm?termId=" + termId, {}, callback, failureCallback);
		logger.warn(`<<< Exiting deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteVocabularyTerm", "logrite.termId": termId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const vocabularyTermService = new VocabularyTermService();
export default vocabularyTermService;