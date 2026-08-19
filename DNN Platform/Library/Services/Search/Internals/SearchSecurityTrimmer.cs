// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    internal delegate bool SecurityCheckerDelegate(Document luceneResult, SearchQuery searchQuery);

    internal class SearchSecurityTrimmer : Collector
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchSecurityTrimmer));
        private readonly SecurityCheckerDelegate securityChecker;
        private readonly IndexSearcher searcher;
        private readonly LuceneQuery luceneQuery;
        private readonly SearchQuery searchQuery;
        private readonly List<ScoreDoc> hitDocs;

        private Scorer scorer;
        private int docBase;
        private int totalHits;
        private List<ScoreDoc> scoreDocs;

        /// <summary>Initializes a new instance of the <see cref="SearchSecurityTrimmer"/> class.</summary>
        /// <param name="searchContext">The search context.</param>
        public SearchSecurityTrimmer(SearchSecurityTrimmerContext searchContext)
        {
			Logger.LogDebug(">>> Entering SearchSecurityTrimmer(searchContext={SearchContext})", searchContext);
            this.securityChecker = searchContext.SecurityChecker;
            this.searcher = searchContext.Searcher;
			Logger.LogDebug("SearchSecurityTrimmer(searchContext={SearchContext}): this.securityChecker → {Arg1}", searchContext, this.securityChecker);
			Logger.LogDebug("SearchSecurityTrimmer(searchContext={SearchContext}): searcher → {Searcher}", searchContext, searcher);
			Logger.LogDebug("SearchSecurityTrimmer(searchContext={SearchContext}): this.searcher → {Arg1}", searchContext, this.searcher);
            this.luceneQuery = searchContext.LuceneQuery;
            this.searchQuery = searchContext.SearchQuery;
            this.hitDocs = new List<ScoreDoc>(16);
			Logger.LogDebug("SearchSecurityTrimmer(searchContext={SearchContext}): this.luceneQuery → {Arg1}", searchContext, this.luceneQuery);
			Logger.LogDebug("SearchSecurityTrimmer(searchContext={SearchContext}): luceneQuery → {LuceneQuery}", searchContext, luceneQuery);
			Logger.LogDebug("<<< Exiting SearchSecurityTrimmer(searchContext={SearchContext})", searchContext);
        }

        /// <inheritdoc />
        public override bool AcceptsDocsOutOfOrder
        {
            get { return false; }
        }

        public int TotalHits
        {
            get
            {
                if (this.scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this.totalHits;
            }
        }

        public List<ScoreDoc> ScoreDocs
        {
            get
            {
                if (this.scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this.scoreDocs;
            }
        }

        /// <inheritdoc />
        public override void SetNextReader(IndexReader reader, int docBase)
        {
			Logger.LogDebug(">>> Entering SetNextReader(reader={Reader},docBase={DocBase})", reader, docBase);
            this.docBase = docBase;
			Logger.LogDebug("SetNextReader(reader={Reader},docBase={DocBase}): docBase → {DocBase1}", reader, docBase, docBase);
			Logger.LogDebug("SetNextReader(reader={Reader},docBase={DocBase}): this.docBase → {Arg2}", reader, docBase, this.docBase);
			Logger.LogDebug("<<< Exiting SetNextReader(reader={Reader},docBase={DocBase})", reader, docBase);
        }

        /// <inheritdoc />
        public override void SetScorer(Scorer scorer)
        {
			Logger.LogDebug(">>> Entering SetScorer(scorer={Scorer})", scorer);
            this.scorer = scorer;
			Logger.LogDebug("SetScorer(scorer={Scorer}): scorer → {Scorer1}", scorer, scorer);
			Logger.LogDebug("SetScorer(scorer={Scorer}): this.scorer → {Arg1}", scorer, this.scorer);
			Logger.LogDebug("<<< Exiting SetScorer(scorer={Scorer})", scorer);
        }

        /// <inheritdoc />
        public override void Collect(int doc)
        {
			Logger.LogDebug(">>> Entering Collect(doc={Doc})", doc);
            this.hitDocs.Add(new ScoreDoc(doc + this.docBase, this.scorer.Score()));
			Logger.LogDebug("<<< Exiting Collect(doc={Doc})", doc);
        }

        private static string GetStringFromField(Document doc, SortField sortField)
        {
			Logger.LogDebug(">>> Entering GetStringFromField(doc={Doc},sortField={SortField})", doc, sortField);
            var field = doc.GetField(sortField.Field);
			Logger.LogDebug("<<< Exiting GetStringFromField(doc={Doc},sortField={SortField})", doc, sortField);
            return field == null ? string.Empty : field.StringValue;
        }

        private static long GetLongFromField(Document doc, SortField sortField)
        {
			Logger.LogDebug(">>> Entering GetLongFromField(doc={Doc},sortField={SortField})", doc, sortField);
            var field = doc.GetField(sortField.Field);
            if (field == null)
			Logger.LogDebug("GetLongFromField(doc={Doc},sortField={SortField}): field → {Field}", doc, sortField, field);
            {
                return 0;
            }

            long data;
            if (long.TryParse(field.StringValue, out data) && data >= 0)
            {
			Logger.LogDebug("<<< Exiting GetLongFromField(doc={Doc},sortField={SortField})", doc, sortField);
                return data;
            }

            return 0;
        }

        private void PrepareScoreDocs()
        {
			Logger.LogDebug(">>> Entering PrepareScoreDocs()");
            int skippedSoFar;
            var collectedSoFar = skippedSoFar = 0;
            var pageSize = this.luceneQuery.PageSize;
            var toSkip = this.luceneQuery.PageIndex <= 1 ? 0 : ((this.luceneQuery.PageIndex - 1) * pageSize);
			Logger.LogDebug("PrepareScoreDocs(): toSkip → {ToSkip}", toSkip);
            IEnumerable<ScoreDoc> tempDocs = new List<ScoreDoc>();
			Logger.LogDebug("PrepareScoreDocs(): tempDocs → {TempDocs}", tempDocs);

            this.totalHits = this.hitDocs.Count;
			Logger.LogDebug("PrepareScoreDocs(): totalHits → {TotalHits}", totalHits);
			Logger.LogDebug("PrepareScoreDocs(): this.totalHits → {Arg0}", this.totalHits);

            var useRelevance = false;
			Logger.LogDebug("PrepareScoreDocs(): useRelevance → {UseRelevance}", useRelevance);
            if (ReferenceEquals(Sort.RELEVANCE, this.luceneQuery.Sort))
            {
                useRelevance = true;
            }
            else
            {
                var fields = this.luceneQuery.Sort.GetSort();
				Logger.LogDebug("PrepareScoreDocs(): fields → {Fields}", fields);
                if (fields is not { Length: 1 })
                {
                    useRelevance = true;
                }
                else
                {
                    var field = fields[0];
                    if (field.Type is SortField.INT or SortField.LONG)
                    {
                        if (field.Reverse)
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderByDescending(rec => GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderBy(rec => GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                    else
                    {
                        if (field.Reverse)
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                           .OrderByDescending(rec => GetStringFromField(rec.Document, field))
                                           .ThenByDescending(rec => rec.Document.Boost)
                                           .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderBy(rec => GetStringFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                }
            }

            if (useRelevance)
            {
                tempDocs = this.hitDocs.OrderByDescending(d => d.Score).ThenBy(d => d.Doc);
            }

            var scoreDocSize = Math.Min(tempDocs.Count(), pageSize);
			Logger.LogDebug("PrepareScoreDocs(): scoreDocSize → {ScoreDocSize}", scoreDocSize);
            this.scoreDocs = new List<ScoreDoc>(scoreDocSize);
			Logger.LogDebug("PrepareScoreDocs(): scoreDocs → {ScoreDocs}", scoreDocs);

            foreach (var scoreDoc in tempDocs)
            {
                if (this.securityChecker == null || this.securityChecker(this.searcher.Doc(scoreDoc.Doc), this.searchQuery))
                {
                    if (skippedSoFar < toSkip)
                    {
                        skippedSoFar++;
                        continue;
                    }

                    if (collectedSoFar >= pageSize)
                    {
                        continue;
                    }

                    this.scoreDocs.Add(scoreDoc);
                    ++collectedSoFar;
                }
                else
                {
                    this.totalHits--;
                }
            }
			Logger.LogDebug("<<< Exiting PrepareScoreDocs()");
        }
    }
}
