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
			eventLogger.LogDebug(">>> Entering VocabulariesController()");
            this.termController = new TermController();
            this.vocabularyController = new VocabularyController();
			eventLogger.LogDebug("VocabulariesController(): vocabularyController → {VocabularyController}", vocabularyController);
			eventLogger.LogDebug("VocabulariesController(): termController → {TermController}", termController);
            this.validator = new Validator(new DataAnnotationsObjectValidator());
			eventLogger.LogDebug("VocabulariesController(): validator → {Validator}", validator);
            this.validator.Validators.Add(new VocabularyNameValidator(this.vocabularyController, this.termController));
			eventLogger.LogDebug("<<< Exiting VocabulariesController()");
        }

        public List<Vocabulary> GetVocabularies(int portalId, int pageIndex, int pageSize, int scopeTypeId, out int total)
        {
			eventLogger.LogDebug(">>> Entering GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId})", portalId, pageIndex, pageSize, scopeTypeId);
            var vocabularies = this.vocabularyController.GetVocabularies().Where(v => v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId));
            if (scopeTypeId != Null.NullInteger)
			eventLogger.LogDebug("GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId}): vocabularies → {Vocabularies}", portalId, pageIndex, pageSize, scopeTypeId, vocabularies);
            {
                vocabularies = vocabularies.Where(v => v.ScopeTypeId == scopeTypeId);
            }

            total = vocabularies.Count();
			eventLogger.LogDebug("GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId}): total → {Total}", portalId, pageIndex, pageSize, scopeTypeId, total);
			eventLogger.LogDebug("<<< Exiting GetVocabularies(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},scopeTypeId={ScopeTypeId})", portalId, pageIndex, pageSize, scopeTypeId);
            return vocabularies.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public bool IsSystemVocabulary(int vocabularyId)
        {
			eventLogger.LogDebug(">>> Entering IsSystemVocabulary(vocabularyId={VocabularyId})", vocabularyId);
			eventLogger.LogDebug("<<< Exiting IsSystemVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.vocabularyController.GetVocabularies().Any(v => v.VocabularyId == vocabularyId && v.IsSystem);
        }

        public int AddVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }

            var result = this.validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                return this.vocabularyController.AddVocabulary(vocabulary);
            }
            else
            {
			eventLogger.LogDebug("<<< Exiting AddVocabulary(vocabulary={Vocabulary})", vocabulary);
                throw new VocabularyNameAlreadyExistsException();
            }
        }

        public void UpdateVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }

            var result = this.validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                this.vocabularyController.UpdateVocabulary(vocabulary);
            }
            else
            {
			eventLogger.LogDebug("<<< Exiting UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
                throw new VocabularyValidationException();
            }
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            this.vocabularyController.DeleteVocabulary(vocabulary);
			eventLogger.LogDebug("<<< Exiting DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
        }

        public List<Term> GetTermsByVocabulary(int vocabularyId)
        {
			eventLogger.LogDebug(">>> Entering GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            var vocabulary = this.vocabularyController.GetVocabularies().SingleOrDefault(v => v.VocabularyId == vocabularyId);
			eventLogger.LogDebug("GetTermsByVocabulary(vocabularyId={VocabularyId}): vocabulary → {Vocabulary}", vocabularyId, vocabulary);
			eventLogger.LogDebug("<<< Exiting GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.termController.GetTermsByVocabulary(vocabularyId).ToList();
        }

        public int AddTerm(Term term)
        {
			eventLogger.LogDebug(">>> Entering AddTerm(term={Term})", term);
            var result = this.validator.ValidateObject(term);
            if (result.IsValid)
			eventLogger.LogDebug("AddTerm(term={Term}): result → {Result}", term, result);
            {
                return this.termController.AddTerm(term);
            }
            else
            {
				eventLogger.LogWarning("Throwing TermValidationException from AddTerm(term={Term})", term);
			eventLogger.LogDebug("<<< Exiting AddTerm(term={Term})", term);
                throw new TermValidationException();
            }
        }

        public void UpdateTerm(Term term)
        {
			eventLogger.LogDebug(">>> Entering UpdateTerm(term={Term})", term);
            var result = this.validator.ValidateObject(term);
            if (result.IsValid)
			eventLogger.LogDebug("UpdateTerm(term={Term}): result → {Result}", term, result);
            {
                this.termController.UpdateTerm(term);
            }
            else
            {
				eventLogger.LogWarning("Throwing TermValidationException from UpdateTerm(term={Term})", term);
			eventLogger.LogDebug("<<< Exiting UpdateTerm(term={Term})", term);
                throw new TermValidationException();
            }
        }

        public void DeleteTerm(Term term)
        {
			eventLogger.LogDebug(">>> Entering DeleteTerm(term={Term})", term);
            this.termController.DeleteTerm(term);
			eventLogger.LogDebug("<<< Exiting DeleteTerm(term={Term})", term);
        }

        public Term GetTerm(int termId)
        {
			eventLogger.LogDebug(">>> Entering GetTerm(termId={TermId})", termId);
			eventLogger.LogDebug("<<< Exiting GetTerm(termId={TermId})", termId);
            return this.termController.GetTerm(termId);
        }

        private static void AddChildNodes(List<Term> termList, TermDto parentNode)
        {
			eventLogger.LogDebug(">>> Entering AddChildNodes(termList={TermList},parentNode={ParentNode})", termList, parentNode);
            if (parentNode.ChildTerms != null)
            {
                parentNode.ChildTerms.Clear();
                var parentId = parentNode.TermId;
                var terms = termList.Where(v => v.ParentTermId == parentId);
				eventLogger.LogDebug("AddChildNodes(termList={TermList},parentNode={ParentNode}): terms → {Terms}", termList, parentNode, terms);

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
			eventLogger.LogDebug("<<< Exiting AddChildNodes(termList={TermList},parentNode={ParentNode})", termList, parentNode);
        }
    }
}
