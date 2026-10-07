// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>Extension methods for Term, Vocabulary, ContentItem.</summary>
    /// <seealso cref="Term"/>
    public static class ContentExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentExtensions));
        /// <summary>Toes the delimitted string.</summary>
        /// <param name="terms">The terms.</param>
        /// <param name="delimitter">The delimitter.</param>
        /// <returns>terms' name as a string and split with the given delimitter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string delimitter)
        {
			Logger.LogError(">>> Entering ToDelimittedString(terms={Terms},delimitter={Delimitter})", terms, delimitter);
            var sb = new StringBuilder();
			Logger.LogError("ToDelimittedString(terms={Terms},delimitter={Delimitter}): sb → {Sb}", terms, delimitter, sb);
            if (terms != null)
            {
                foreach (Term term in terms.OrderBy(term => term.Name))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(term.Name);
                }
            }

			Logger.LogWarning("<<< Exiting ToDelimittedString(terms={Terms},delimitter={Delimitter})", terms, delimitter);
			Logger.LogWarning("<<< Exiting ToDelimittedString(terms={Terms},format={Format},delimitter={Delimitter})", terms, format, delimitter);
            return sb.ToString();
        }

        /// <summary>Toes the delimited string.</summary>
        /// <param name="terms">The terms.</param>
        /// <param name="format">The format.</param>
        /// <param name="delimitter">The delimiter.</param>
        /// <returns> formatted terms' name as a string and split with the given delimiter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string format, string delimitter)
        {
            var sb = new StringBuilder();
            if (terms != null)
            {
                foreach (var term in terms.OrderBy(term => term.Name))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(string.Format(CultureInfo.InvariantCulture, format, term.Name));
                }
            }

            return sb.ToString();
        }

        /// <summary>Gets the child terms.</summary>
        /// <param name="term1">The term.</param>
        /// <param name="termId">The term id.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection which's parent is the specific term.</returns>
        internal static List<Term> GetChildTerms(this Term term1, int termId, int vocabularyId)
        {
			Logger.LogError(">>> Entering GetChildTerms(term1={Term1},termId={TermId},vocabularyId={VocabularyId})", term1, termId, vocabularyId);
            ITermController ctl = Util.GetTermController();

			Logger.LogError("GetChildTerms(term1={Term1},termId={TermId},vocabularyId={VocabularyId}): ctl → {Ctl}", term1, termId, vocabularyId, ctl);
            IQueryable<Term> terms = from term in ctl.GetTermsByVocabulary(vocabularyId) where term.ParentTermId == termId select term;
			Logger.LogError("GetChildTerms(term1={Term1},termId={TermId},vocabularyId={VocabularyId}): terms → {Terms}", term1, termId, vocabularyId, terms);

			Logger.LogWarning("<<< Exiting GetChildTerms(term1={Term1},termId={TermId},vocabularyId={VocabularyId})", term1, termId, vocabularyId);
            return terms.ToList();
        }

        /// <summary>Gets the vocabulary.</summary>
        /// <param name="term">The term.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>Vocabulary.</returns>
        internal static Vocabulary GetVocabulary(this Term term, int vocabularyId)
        {
			Logger.LogError(">>> Entering GetVocabulary(term={Term},vocabularyId={VocabularyId})", term, vocabularyId);
            IVocabularyController ctl = Util.GetVocabularyController();

			Logger.LogError("GetVocabulary(term={Term},vocabularyId={VocabularyId}): ctl → {Ctl}", term, vocabularyId, ctl);
			Logger.LogWarning("<<< Exiting GetVocabulary(term={Term},vocabularyId={VocabularyId})", term, vocabularyId);
            return (from v in ctl.GetVocabularies() where v.VocabularyId == vocabularyId select v).SingleOrDefault();
        }

        /// <summary>Gets the type of the scope.</summary>
        /// <param name="voc">The voc.</param>
        /// <param name="scopeTypeId">The scope type id.</param>
        /// <returns>scope type.</returns>
        internal static ScopeType GetScopeType(this Vocabulary voc, int scopeTypeId)
        {
			Logger.LogError(">>> Entering GetScopeType(voc={Voc},scopeTypeId={ScopeTypeId})", voc, scopeTypeId);
            IScopeTypeController ctl = Util.GetScopeTypeController();

			Logger.LogError("GetScopeType(voc={Voc},scopeTypeId={ScopeTypeId}): ctl → {Ctl}", voc, scopeTypeId, ctl);
			Logger.LogWarning("<<< Exiting GetScopeType(voc={Voc},scopeTypeId={ScopeTypeId})", voc, scopeTypeId);
            return ctl.GetScopeTypes().Where(s => s.ScopeTypeId == scopeTypeId).SingleOrDefault();
        }

        /// <summary>Gets the terms by vocabularyId.</summary>
        /// <param name="voc">The voc.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this Vocabulary voc, int vocabularyId)
        {
			Logger.LogError(">>> Entering GetTerms(voc={Voc},vocabularyId={VocabularyId})", voc, vocabularyId);
            ITermController ctl = Util.GetTermController();

			Logger.LogError("GetTerms(voc={Voc},vocabularyId={VocabularyId}): ctl → {Ctl}", voc, vocabularyId, ctl);
			Logger.LogWarning("<<< Exiting GetTerms(voc={Voc},vocabularyId={VocabularyId})", voc, vocabularyId);
			Logger.LogWarning("<<< Exiting GetTerms(item={Item},contentItemId={ContentItemId})", item, contentItemId);
            return ctl.GetTermsByVocabulary(vocabularyId).ToList();
        }

        /// <summary>Gets the meta data.</summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>meta data collection.</returns>
        internal static NameValueCollection GetMetaData(this ContentItem item, int contentItemId)
        {
			Logger.LogError(">>> Entering GetMetaData(item={Item},contentItemId={ContentItemId})", item, contentItemId);
            IContentController ctl = Util.GetContentController();

			Logger.LogError("GetMetaData(item={Item},contentItemId={ContentItemId}): ctl → {Ctl}", item, contentItemId, ctl);
            NameValueCollection metaData;
            if (contentItemId == Null.NullInteger)
            {
                metaData = new NameValueCollection();
				Logger.LogError("GetMetaData(item={Item},contentItemId={ContentItemId}): metaData → {MetaData}", item, contentItemId, metaData);
            }
            else
            {
                metaData = ctl.GetMetaData(contentItemId);
				Logger.LogDebug("GetMetaData(item={Item},contentItemId={ContentItemId}): metaData → {MetaData}", item, contentItemId, metaData);
            }

			Logger.LogWarning("<<< Exiting GetMetaData(item={Item},contentItemId={ContentItemId})", item, contentItemId);
            return metaData;
        }

        /// <summary>Gets the terms by content item id.</summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this ContentItem item, int contentItemId)
        {
            ITermController ctl = Util.GetTermController();

            List<Term> terms = null;
            if (contentItemId == Null.NullInteger)
            {
                terms = new List<Term>();
            }
            else
            {
                terms = ctl.GetTermsByContent(contentItemId).ToList();
            }

            return terms;
        }
    }
}
