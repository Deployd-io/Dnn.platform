import util from "../utils";
const logger = console;
class VocabularyTermService {
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
    getVocabularyTerms(vocabularyTermId, callback, failureCallback) {
		logger.debug(`>>> Entering getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");

		logger.debug(`getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
        sf.get("GetTermsByVocabularyId?vocabularyId=" + vocabularyTermId + "&pageIndex=0", {}, callback, failureCallback);
		logger.debug(`<<< Exiting getVocabularyTerms(vocabularyTermId=${vocabularyTermId},callback=${callback},failureCallback=${failureCallback})`);
    }
    addVocabularyTerm(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");

		logger.debug(`addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
        sf.post("CreateTerm", payload, callback, failureCallback);
		logger.debug(`<<< Exiting addVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
    updateVocabularyTerm(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Vocabularies");

		logger.debug(`updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
        sf.post("UpdateTerm", payload, callback, failureCallback);
		logger.debug(`<<< Exiting updateVocabularyTerm(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
    deleteVocabularyTerm(termId, callback, failureCallback) {
	logger.debug(`>>> Entering deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback})`);

        const sf = this.getServiceFramework("Vocabularies");
		logger.debug(`deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);

        sf.post("DeleteTerm?termId=" + termId, {}, callback, failureCallback);
		logger.debug(`<<< Exiting deleteVocabularyTerm(termId=${termId},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const vocabularyTermService = new VocabularyTermService();
export default vocabularyTermService;