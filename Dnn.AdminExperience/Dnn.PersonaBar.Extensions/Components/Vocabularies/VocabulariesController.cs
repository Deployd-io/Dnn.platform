// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Vocabularies.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Vocabularies.Exceptions;
    using Dnn.PersonaBar.Vocabularies.Services.Dto;
    using Dnn.PersonaBar.Vocabularies.Validators;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Validators;

    public class VocabulariesController
    {
        private TermController termController;
        private VocabularyController vocabularyController;
        private Validator validator;

        public VocabulariesController()
        {
			eventLogger.LogError(">>> Entering VocabulariesController()");
            this.termController = new TermController();
			eventLogger.LogWarning("<<< Exiting VocabulariesController()");
			eventLogger.LogDebug("VocabulariesController(): termController → {TermController}", termController);
            this.vocabularyController = new VocabularyController();
			eventLogger.LogDebug("VocabulariesController(): vocabularyController → {VocabularyController}", vocabularyController);
            this.validator = new Validator(new DataAnnotationsObjectValidator());
			eventLogger.LogDebug("VocabulariesController(): validator → {Validator}", validator);
            this.validator.Validators.Add(new VocabularyNameValidator(this.vocabularyController, this.termController));
        }

        public List<Vocabulary> GetVocabularies(int portalId, int pageIndex, int pageSize, int scopeTypeId, out int total)
        {
			eventLogger.LogError(">>> Entering GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId})", portalId, pageIndex, pageSize, scopeTypeId);
            var vocabularies = this.vocabularyController.GetVocabularies().Where(v => v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId));
			eventLogger.LogError("GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId}): vocabularies → {Vocabularies}", portalId, pageIndex, pageSize, scopeTypeId, vocabularies);
            if (scopeTypeId != Null.NullInteger)
            {
                vocabularies = vocabularies.Where(v => v.ScopeTypeId == scopeTypeId);
            }

            total = vocabularies.Count();
			eventLogger.LogError("GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId}): total → {Total}", portalId, pageIndex, pageSize, scopeTypeId, total);
			eventLogger.LogDebug("GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId}): total → {Total}", portalId, pageIndex, pageSize, scopeTypeId, total);
			eventLogger.LogWarning("<<< Exiting GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId})", portalId, pageIndex, pageSize, scopeTypeId);
            return vocabularies.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public bool IsSystemVocabulary(int vocabularyId)
        {
			eventLogger.LogError(">>> Entering IsSystemVocabulary(vocabularyId={VocabularyId})", vocabularyId);
			eventLogger.LogWarning("<<< Exiting IsSystemVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.vocabularyController.GetVocabularies().Any(v => v.VocabularyId == vocabularyId && v.IsSystem);
        }

        public int AddVocabulary(Vocabulary vocabulary)
        {
			eventLogger.LogError(">>> Entering AddVocabulary(vocabulary={Vocabulary})", vocabulary);
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
				eventLogger.LogDebug("AddVocabulary(vocabulary={Vocabulary}): vocabulary.ScopeId → {Arg1}", vocabulary, vocabulary.ScopeId);
            }

            var result = this.validator.ValidateObject(vocabulary);
			eventLogger.LogError("AddVocabulary(vocabulary={Vocabulary}): result → {Result}", vocabulary, result);
            if (result.IsValid)
            {
                return this.vocabularyController.AddVocabulary(vocabulary);
            }
            else
            {
				eventLogger.LogError("Throwing VocabularyNameAlreadyExistsException from AddVocabulary(vocabulary={Vocabulary})", vocabulary);
			eventLogger.LogWarning("<<< Exiting AddVocabulary(vocabulary={Vocabulary})", vocabulary);
                throw new VocabularyNameAlreadyExistsException();
            }
        }

        public void UpdateVocabulary(Vocabulary vocabulary)
        {
			eventLogger.LogError(">>> Entering UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
				eventLogger.LogDebug("UpdateVocabulary(vocabulary={Vocabulary}): vocabulary.ScopeId → {Arg1}", vocabulary, vocabulary.ScopeId);
            }

            var result = this.validator.ValidateObject(vocabulary);
			eventLogger.LogError("UpdateVocabulary(vocabulary={Vocabulary}): result → {Result}", vocabulary, result);
            if (result.IsValid)
            {
                this.vocabularyController.UpdateVocabulary(vocabulary);
            }
            else
            {
				eventLogger.LogError("Throwing VocabularyValidationException from UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
			eventLogger.LogWarning("<<< Exiting UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
                throw new VocabularyValidationException();
            }
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
			eventLogger.LogError(">>> Entering DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
            this.vocabularyController.DeleteVocabulary(vocabulary);
			eventLogger.LogWarning("<<< Exiting DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
        }

        public List<Term> GetTermsByVocabulary(int vocabularyId)
        {
			eventLogger.LogError(">>> Entering GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            var vocabulary = this.vocabularyController.GetVocabularies().SingleOrDefault(v => v.VocabularyId == vocabularyId);
			eventLogger.LogError("GetTermsByVocabulary(vocabularyId={VocabularyId}): vocabulary → {Vocabulary}", vocabularyId, vocabulary);
			eventLogger.LogWarning("<<< Exiting GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.termController.GetTermsByVocabulary(vocabularyId).ToList();
        }

        public int AddTerm(Term term)
        {
			eventLogger.LogError(">>> Entering AddTerm(term={Term})", term);
            var result = this.validator.ValidateObject(term);
			eventLogger.LogError("AddTerm(term={Term}): result → {Result}", term, result);
            if (result.IsValid)
            {
                return this.termController.AddTerm(term);
            }
            else
            {
				eventLogger.LogError("Throwing TermValidationException from AddTerm(term={Term})", term);
			eventLogger.LogWarning("<<< Exiting AddTerm(term={Term})", term);
                throw new TermValidationException();
            }
        }

        public void UpdateTerm(Term term)
        {
			eventLogger.LogError(">>> Entering UpdateTerm(term={Term})", term);
            var result = this.validator.ValidateObject(term);
			eventLogger.LogError("UpdateTerm(term={Term}): result → {Result}", term, result);
            if (result.IsValid)
            {
                this.termController.UpdateTerm(term);
            }
            else
            {
				eventLogger.LogError("Throwing TermValidationException from UpdateTerm(term={Term})", term);
			eventLogger.LogWarning("<<< Exiting UpdateTerm(term={Term})", term);
                throw new TermValidationException();
            }
        }

        public void DeleteTerm(Term term)
        {
			eventLogger.LogError(">>> Entering DeleteTerm(term={Term})", term);
            this.termController.DeleteTerm(term);
			eventLogger.LogWarning("<<< Exiting DeleteTerm(term={Term})", term);
        }

        public Term GetTerm(int termId)
        {
			eventLogger.LogError(">>> Entering GetTerm(termId={TermId})", termId);
            eventLogger.LogWarning("<<< Exiting GetTerm(termId={TermId})", termId);
            return this.termController.GetTerm(termId);
        }

        private static void AddChildNodes(List<Term> termList, TermDto parentNode)
        {
			eventLogger.LogError(">>> Entering AddChildNodes(termList={TermList},parentNode={ParentNode})", termList, parentNode);
            if (parentNode.ChildTerms != null)
            {
                parentNode.ChildTerms.Clear();
                var parentId = parentNode.TermId;
                var terms = termList.Where(v => v.ParentTermId == parentId);
				eventLogger.LogError("AddChildNodes(termList={TermList},parentNode={ParentNode}): terms → {Terms}", termList, parentNode, terms);

                foreach (var term in terms)
                {
                    if (term.ParentTermId == parentId)
                    {
                        var node = new TermDto
                        {
                            TermId = term.TermId,
                            Description = term.Description,
                            Name = term.Name,
                            ParentTermId = term.ParentTermId ?? Null.NullInteger,
                            VocabularyId = term.VocabularyId,
                            ChildTerms = new List<TermDto>(),
                        };
                        AddChildNodes(termList, node);
                        parentNode.ChildTerms.Add(node);
                    }
                }
            }
			eventLogger.LogWarning("<<< Exiting AddChildNodes(termList={TermList},parentNode={ParentNode})", termList, parentNode);
        }
    }
}
