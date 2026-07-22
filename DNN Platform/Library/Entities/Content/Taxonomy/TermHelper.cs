// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;

    public class TermHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public const string PageTagsVocabulary = "PageTags";

        public static List<Term> ToTabTerms(string pageSettingsTags, int tabPortalId)
        {
			Logger.LogDebug(">>> Entering ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId})", pageSettingsTags, tabPortalId);
            var terms = new List<Term>();

			Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): terms → {Terms}", pageSettingsTags, tabPortalId, terms);
            if (string.IsNullOrEmpty(pageSettingsTags))
            {
                return terms;
            }

            var termController = new TermController();
            var vocabularyController = Util.GetVocabularyController();
			Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): termController → {TermController}", pageSettingsTags, tabPortalId, termController);
			Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): vocabularyController → {VocabularyController}", pageSettingsTags, tabPortalId, vocabularyController);
            var vocabulary = vocabularyController.GetVocabularies()
                                .Cast<Vocabulary>()
                                .Where(v => v.Name == PageTagsVocabulary && v.ScopeId == tabPortalId)
                                .SingleOrDefault();

            var vocabularyId = Null.NullInteger;
            if (vocabulary == null)
			Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): vocabularyId → {VocabularyId}", pageSettingsTags, tabPortalId, vocabularyId);
            {
                var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
                if (scopeType == null)
				Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): scopeType → {ScopeType}", pageSettingsTags, tabPortalId, scopeType);
                {
                    throw new ScopeNotFoundException("Can't create default vocabulary as scope type 'Portal' can't be found.");
                }

                vocabularyId = vocabularyController.AddVocabulary(
                    new Vocabulary(PageTagsVocabulary, string.Empty, VocabularyType.Simple)
                    {
                        ScopeTypeId = scopeType.ScopeTypeId,
                        ScopeId = tabPortalId,
                    });
            }
            else
            {
                vocabularyId = vocabulary.VocabularyId;
            }

            // get all terms info
            var allTerms = new List<Term>();
            var vocabularies = from v in vocabularyController.GetVocabularies()
                               where v.ScopeType.ScopeType == "Portal" && v.ScopeId == tabPortalId && !v.Name.Equals("Tags", StringComparison.OrdinalIgnoreCase)
                               select v;
							Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): allTerms → {AllTerms}", pageSettingsTags, tabPortalId, allTerms);
            foreach (var v in vocabularies)
            {
                allTerms.AddRange(termController.GetTermsByVocabulary(v.VocabularyId));
            }

            foreach (var tag in pageSettingsTags.Trim().Split(','))
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    var term = allTerms.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                    if (term == null)
					Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): term → {Term}", pageSettingsTags, tabPortalId, term);
                    {
                        var termId = termController.AddTerm(new Term(tag, string.Empty, vocabularyId));
                        term = termController.GetTerm(termId);
						Logger.LogDebug("ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId}): termId → {TermId}", pageSettingsTags, tabPortalId, termId);
                    }

                    terms.Add(term);
                }
            }

			Logger.LogDebug("<<< Exiting ToTabTerms(pageSettingsTags={PageSettingsTags},tabPortalId={TabPortalId})", pageSettingsTags, tabPortalId);
            return terms;
        }
    }
}
