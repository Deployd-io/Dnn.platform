// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Search.Vectorhighlight;
    using Lucene.Net.Store;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>  The Impl Controller class for Lucene.</summary>
    internal class LuceneControllerImpl : ILuceneController, IDisposable
    {
        private const string DefaultSearchFolder = @"App_Data\Search";
        private const string WriteLockFile = "write.lock";
        private const int DefaultSearchRetryTimes = 5;
        private const int DISPOSED = 1;
        private const int UNDISPOSED = 0;
        private const string HighlightPreTag = "[b]";
        private const string HighlightPostTag = "[/b]";

        private const string HtmlPreTag = "<b>";
        private const string HtmlPostTag = "</b>";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LuceneControllerImpl));
        private static readonly TimeSpan DefaultRereadTimeSpan = TimeSpan.FromSeconds(30);
        private readonly object writerLock = new object();
        private readonly TimeSpan readerTimeSpan;
        private readonly int searchRetryTimes; // search retry times if exception thrown during search process
        private readonly List<CachedReader> oldReaders = new List<CachedReader>();

        private IndexWriter writer;
        private IndexReader idxReader;
        private CachedReader reader;
        private FastVectorHighlighter fastHighlighter;
        private int isDisposed = UNDISPOSED;

        private DateTime lastReadTimeUtc;
        private DateTime lastDirModifyTimeUtc;

        /// <summary>Initializes a new instance of the <see cref="LuceneControllerImpl"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="appStatus">The application status.</param>
        public LuceneControllerImpl(IHostSettingsService hostSettingsService, IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering LuceneControllerImpl(hostSettingsService={HostSettingsService},appStatus={AppStatus})", hostSettingsService, appStatus);
            var folder = hostSettingsService.GetString(Constants.SearchIndexFolderKey, DefaultSearchFolder);
            if (string.IsNullOrEmpty(folder))
			Logger.LogDebug("LuceneControllerImpl(hostSettingsService={HostSettingsService},appStatus={AppStatus}): folder → {Folder}", hostSettingsService, appStatus, folder);
            {
                folder = DefaultSearchFolder;
            }

            this.IndexFolder = Path.Combine(appStatus.ApplicationMapPath, folder);
            this.readerTimeSpan = TimeSpan.FromSeconds(hostSettingsService.GetDouble(Constants.SearchReaderRefreshTimeKey, DefaultRereadTimeSpan.TotalSeconds));
			Logger.LogDebug("LuceneControllerImpl(hostSettingsService={HostSettingsService},appStatus={AppStatus}): readerTimeSpan → {ReaderTimeSpan}", hostSettingsService, appStatus, readerTimeSpan);
            this.searchRetryTimes = hostSettingsService.GetInteger(Constants.SearchRetryTimesKey, DefaultSearchRetryTimes);
			Logger.LogDebug("<<< Exiting LuceneControllerImpl(hostSettingsService={HostSettingsService},appStatus={AppStatus})", hostSettingsService, appStatus);
			Logger.LogDebug("LuceneControllerImpl(hostSettingsService={HostSettingsService},appStatus={AppStatus}): searchRetryTimes → {SearchRetryTimes}", hostSettingsService, appStatus, searchRetryTimes);
        }

        internal string IndexFolder { get; private set; }

        private IndexWriter Writer
        {
            get
            {
                if (this.writer == null)
                {
                    lock (this.writerLock)
                    {
                        if (this.writer == null)
                        {
                            var lockFile = Path.Combine(this.IndexFolder, WriteLockFile);
                            if (File.Exists(lockFile))
                            {
                                try
                                {
                                    // if we succeed in deleting the file, move on and create a new writer; otherwise,
                                    // the writer is locked by another instance (e.g., another server in a web farm).
                                    File.Delete(lockFile);
                                }
                                catch (IOException e)
                                {
                                    throw new SearchException(
                                        Localization.GetExceptionMessage(
                                            "UnableToCreateLuceneWriter",
                                            "Unable to create Lucene writer (lock file is in use). Please recycle AppPool in IIS to release lock."),
                                        e);
                                }
                            }

                            this.CheckDisposed();
                            var writer = new IndexWriter(
                                FSDirectory.Open(this.IndexFolder),
                                this.GetCustomAnalyzer() ?? new SynonymAnalyzer(),
                                IndexWriter.MaxFieldLength.UNLIMITED);
                            this.idxReader = writer.GetReader();
                            Thread.MemoryBarrier();
                            this.writer = writer;
                        }
                    }
                }

                return this.writer;
            }
        }

        private bool MustRereadIndex
        {
            get
            {
                return DateTime.UtcNow - this.lastReadTimeUtc >= this.readerTimeSpan &&
                    System.IO.Directory.Exists(this.IndexFolder) &&
                    System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder) != this.lastDirModifyTimeUtc;
            }
        }

        private FastVectorHighlighter FastHighlighter
        {
            get
            {
                if (this.fastHighlighter == null)
                {
                    FragListBuilder fragListBuilder = new SimpleFragListBuilder();
                    FragmentsBuilder fragmentBuilder = new ScoreOrderFragmentsBuilder(
                        new[] { HighlightPreTag }, new[] { HighlightPostTag });
                    this.fastHighlighter = new FastVectorHighlighter(true, true, fragListBuilder, fragmentBuilder);
                }

                return this.fastHighlighter;
            }
        }

        /// <inheritdoc />
        public LuceneResults Search(LuceneSearchContext searchContext)
        {
			Logger.LogDebug(">>> Entering Search(searchContext={SearchContext})", searchContext);
            Requires.NotNull("LuceneQuery", searchContext.LuceneQuery);
            Requires.NotNull("LuceneQuery.Query", searchContext.LuceneQuery.Query);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageSize", searchContext.LuceneQuery.PageSize, 0);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageIndex", searchContext.LuceneQuery.PageIndex, 0);

            var luceneResults = new LuceneResults();

			Logger.LogDebug("Search(searchContext={SearchContext}): luceneResults → {LuceneResults}", searchContext, luceneResults);
            // validate whether index folder exists and contains index files, otherwise return null.
            if (!this.ValidateIndexFolder())
            {
                return luceneResults;
            }

            var highlighter = this.FastHighlighter;
            var fieldQuery = highlighter.GetFieldQuery(searchContext.LuceneQuery.Query);

			Logger.LogDebug("Search(searchContext={SearchContext}): fieldQuery → {FieldQuery}", searchContext, fieldQuery);
            var maxResults = searchContext.LuceneQuery.PageIndex * searchContext.LuceneQuery.PageSize;
            var minResults = maxResults - searchContext.LuceneQuery.PageSize + 1;

            for (var i = 0; i < this.searchRetryTimes; i++)
            {
                try
                {
                    var searcher = this.GetSearcher();
                    var searchSecurityTrimmer = new SearchSecurityTrimmer(new SearchSecurityTrimmerContext
                    {
					Logger.LogDebug("Search(searchContext={SearchContext}): searcher → {Searcher}", searchContext, searcher);
                        Searcher = searcher,
                        SecurityChecker = searchContext.SecurityCheckerDelegate,
                        LuceneQuery = searchContext.LuceneQuery,
                        SearchQuery = searchContext.SearchQuery,
                    });
                    searcher.Search(searchContext.LuceneQuery.Query, null, searchSecurityTrimmer);
                    luceneResults.TotalHits = searchSecurityTrimmer.TotalHits;

                    if (Logger.IsDebugEnabled)
                    {
                        var sb = GetSearcResultExplanation(searchContext.LuceneQuery, searchSecurityTrimmer.ScoreDocs, searcher);
                        Logger.Trace(sb);
						Logger.LogDebug("Search(searchContext={SearchContext}): sb → {Sb}", searchContext, sb);
                    }

                    // Page doesn't exist
                    if (luceneResults.TotalHits < minResults)
                    {
                        break;
                    }

                    luceneResults.Results = searchSecurityTrimmer.ScoreDocs.Select(match =>
                        new LuceneResult
                        {
                            Document = searcher.Doc(match.Doc),
                            Score = match.Score,
                            DisplayScore = GetDisplayScoreFromMatch(match.ToString()),
                            TitleSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.TitleTag, searchContext.LuceneQuery.TitleSnippetLength),
                            BodySnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.BodyTag, searchContext.LuceneQuery.BodySnippetLength),
                            DescriptionSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.DescriptionTag, searchContext.LuceneQuery.TitleSnippetLength),
                            TagSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.Tag, searchContext.LuceneQuery.TitleSnippetLength),
                            AuthorSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.AuthorNameTag, searchContext.LuceneQuery.TitleSnippetLength),
                            ContentSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.ContentTag, searchContext.LuceneQuery.TitleSnippetLength),
                        }).ToList();
                    break;
                }
                catch (Exception ex) when (ex is IOException or AlreadyClosedException)
                {
					Logger.LogError("Exception in Search(searchContext={SearchContext}): {Arg1}", searchContext, ex.Message);
                    this.DisposeReaders();
                    this.DisposeWriter(false);

                    if (i == this.searchRetryTimes - 1)
                    {
                        throw;
                    }

                    Logger.Error(ex);
                    Logger.Error($"Search Index Folder Is Not Available: {ex.Message}, Retry {i + 1} time(s).");
                    Thread.Sleep(100);
                }
            }

			Logger.LogDebug("<<< Exiting Search(searchContext={SearchContext})", searchContext);
            return luceneResults;
        }

        /// <inheritdoc />
        public void Add(Document doc)
        {
            Requires.NotNull("searchDocument", doc);
            if (doc.GetFields().Count > 0)
            {
                try
                {
                    this.Writer.AddDocument(doc);
                }
                catch (OutOfMemoryException)
                {
                    lock (this.writerLock)
                    {
                        // as suggested by Lucene's doc
                        this.DisposeWriter();
                        this.Writer.AddDocument(doc);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Delete(Query query)
        {
			Logger.LogDebug(">>> Entering Delete(query={Query})");
            Requires.NotNull("luceneQuery", query);
            this.Writer.DeleteDocuments(query);
			Logger.LogDebug("<<< Exiting Delete(query={Query})");
        }

        /// <inheritdoc />
        public void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            if (this.writer != null)
            {
                lock (this.writerLock)
                {
                    if (this.writer != null)
                    {
                        this.CheckDisposed();
                        this.writer.Commit();
                        this.RescheduleAccessTimes();
                    }
                }
            }
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public bool OptimizeSearchIndex(bool doWait)
        {
            var writer = this.writer;
            if (writer != null && writer.HasDeletions())
            {
                if (doWait)
                {
                    Logger.Debug("Compacting Search Index - started");
                }

                this.CheckDisposed();

                // optimize down to "> 1 segments" for better performance than down to 1
                this.writer.Optimize(4, doWait);

                if (doWait)
                {
                    this.Commit();
                    Logger.Debug("Compacting Search Index - finished");
                }

                return true;
            }

			Logger.LogDebug("<<< Exiting OptimizeSearchIndex(doWait={DoWait})", doWait);
            return false;
        }

        /// <inheritdoc />
        public bool HasDeletions()
        {
			Logger.LogDebug(">>> Entering HasDeletions()");
            this.CheckDisposed();
            var searcher = this.GetSearcher();
			Logger.LogDebug("HasDeletions(): searcher → {Searcher}", searcher);
			Logger.LogDebug("<<< Exiting HasDeletions()");
            return searcher.IndexReader.HasDeletions;
        }

        /// <inheritdoc />
        public int MaxDocsCount()
        {
			Logger.LogDebug(">>> Entering MaxDocsCount()");
            this.CheckDisposed();
            var searcher = this.GetSearcher();
			Logger.LogDebug("MaxDocsCount(): searcher → {Searcher}", searcher);
			Logger.LogDebug("<<< Exiting MaxDocsCount()");
            return searcher.IndexReader.MaxDoc;
        }

        /// <inheritdoc />
        public int SearchbleDocsCount()
        {
			Logger.LogDebug(">>> Entering SearchbleDocsCount()");
            this.CheckDisposed();
            var searcher = this.GetSearcher();
			Logger.LogDebug("SearchbleDocsCount(): searcher → {Searcher}", searcher);
			Logger.LogDebug("<<< Exiting SearchbleDocsCount()");
            return searcher.IndexReader.NumDocs();
        }

        /// <inheritdoc />
        public SearchStatistics GetSearchStatistics()
        {
			Logger.LogDebug(">>> Entering GetSearchStatistics()");
            this.CheckDisposed();
            var searcher = this.GetSearcher();

			Logger.LogDebug("GetSearchStatistics(): searcher → {Searcher}", searcher);
            var files = System.IO.Directory.GetFiles(this.IndexFolder, "*.*");
            var size = files.Select(name => new FileInfo(name)).Select(fInfo => fInfo.Length).Sum();
			Logger.LogDebug("GetSearchStatistics(): files → {Files}", files);
			Logger.LogDebug("GetSearchStatistics(): size → {Size}", size);

			Logger.LogDebug("<<< Exiting GetSearchStatistics()");
            return new SearchStatistics
            {
                // Hack: seems that NumDocs/MaxDoc are buggy and return incorrect/swapped values
                TotalActiveDocuments = searcher.IndexReader.NumDocs(),
                TotalDeletedDocuments = searcher.IndexReader.NumDeletedDocs,
                IndexLocation = this.IndexFolder,
                LastModifiedOn = System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder),
                IndexDbSize = size,
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            var status = Interlocked.CompareExchange(ref this.isDisposed, DISPOSED, UNDISPOSED);
            if (status == UNDISPOSED)
			Logger.LogDebug("Dispose(): status → {Status}", status);
            {
                this.DisposeWriter();
                this.DisposeReaders();
            }
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <inheritdoc />
        public Analyzer GetCustomAnalyzer()
        {
			Logger.LogDebug(">>> Entering GetCustomAnalyzer()");
            var analyzer = DataCache.GetCache<Analyzer>("Search_CustomAnalyzer");
            if (analyzer == null)
			Logger.LogDebug("GetCustomAnalyzer(): analyzer → {Analyzer}", analyzer);
            {
                var customAnalyzerType = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty);

				Logger.LogDebug("GetCustomAnalyzer(): customAnalyzerType → {CustomAnalyzerType}", customAnalyzerType);
                if (!string.IsNullOrEmpty(customAnalyzerType))
                {
                    try
                    {
                        var analyzerType = Reflection.CreateType(customAnalyzerType);

						Logger.LogDebug("GetCustomAnalyzer(): analyzerType → {AnalyzerType}", analyzerType);
                        // If parameterless ctor exists, use that; if not, pass the Lucene Version.
                        if (analyzerType?.GetConstructor(Type.EmptyTypes) != null)
                        {
                            analyzer = Reflection.CreateInstance(analyzerType) as Analyzer;
                        }
                        else if (analyzerType?.GetConstructor([typeof(Lucene.Net.Util.Version),]) != null)
                        {
                            analyzer = Reflection.CreateInstance(analyzerType, [Constants.LuceneVersion,]) as Analyzer;
                        }

                        if (analyzer == null)
                        {
                            throw new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture,
                                Localization.GetExceptionMessage("InvalidAnalyzerClass", "The class '{0}' cannot be created because it's invalid or is not an analyzer, will use default analyzer."),
                                customAnalyzerType));
                        }

                        DataCache.SetCache("Search_CustomAnalyzer", analyzer);
                        return analyzer;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetCustomAnalyzer()");
            return analyzer;
        }

        // made internal to be used in unit tests only; otherwise could be made private
        internal IndexSearcher GetSearcher()
        {
			Logger.LogDebug(">>> Entering GetSearcher()");
            if (this.reader == null || this.MustRereadIndex)
            {
                this.CheckValidIndexFolder();
                this.UpdateLastAccessTimes();
                this.InstantiateReader();
            }

			Logger.LogDebug("<<< Exiting GetSearcher()");
            return this.reader.GetSearcher();
        }

        private static StringBuilder GetSearcResultExplanation(LuceneQuery luceneQuery, IEnumerable<ScoreDoc> scoreDocs, IndexSearcher searcher)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Query: " + luceneQuery.Query.ToString());
            foreach (var match in scoreDocs)
            {
                var explanation = searcher.Explain(luceneQuery.Query, match.Doc);
                sb.AppendLine("-------------------");
                var doc = searcher.Doc(match.Doc);
                sb.AppendLine(doc.Get(Constants.TitleTag));
                sb.AppendLine(explanation.ToString());
            }

            return sb;
        }

        private static string GetHighlightedText(FastVectorHighlighter highlighter, FieldQuery fieldQuery, IndexSearcher searcher, ScoreDoc match, string tag, int length)
        {
            var s = highlighter.GetBestFragment(fieldQuery, searcher.IndexReader, match.Doc, tag, length);
            if (!string.IsNullOrEmpty(s))
            {
                s = HttpUtility.HtmlEncode(s).Replace(HighlightPreTag, HtmlPreTag).Replace(HighlightPostTag, HtmlPostTag);
            }

            return s;
        }

        /// <summary>Extract the Score portion of the match.ToString().</summary>
        private static string GetDisplayScoreFromMatch(string match)
        {
			Logger.LogDebug(">>> Entering GetDisplayScoreFromMatch(match={Match})", match);
            var displayScore = string.Empty;
            if (!string.IsNullOrEmpty(match))
			Logger.LogDebug("GetDisplayScoreFromMatch(match={Match}): displayScore → {DisplayScore}", match, displayScore);
            {
                var beginPos = match.IndexOf('[');
                var endPos = match.LastIndexOf(']');
				Logger.LogDebug("GetDisplayScoreFromMatch(match={Match}): beginPos → {BeginPos}", match, beginPos);
				Logger.LogDebug("GetDisplayScoreFromMatch(match={Match}): endPos → {EndPos}", match, endPos);
                if (beginPos > 0 && endPos > 0 && endPos > beginPos)
                {
                    displayScore = match.Substring(beginPos + 1, endPos - beginPos - 1);
                }
            }

			Logger.LogDebug("<<< Exiting GetDisplayScoreFromMatch(match={Match})", match);
            return displayScore;
        }

        private void CheckDisposed()
        {
			Logger.LogDebug(">>> Entering CheckDisposed()");
            if (Thread.VolatileRead(ref this.isDisposed) == DISPOSED)
            {
                throw new ObjectDisposedException(Localization.GetExceptionMessage("LuceneControlerIsDisposed", "LuceneController is disposed and cannot be used anymore"));
            }
			Logger.LogDebug("<<< Exiting CheckDisposed()");
        }

        private void InstantiateReader()
        {
			Logger.LogDebug(">>> Entering InstantiateReader()");
            IndexSearcher searcher;
            if (this.idxReader != null)
            {
                // use the Reopen() method for better near-realtime when the _writer isn't null
                var newReader = this.idxReader.Reopen();
                if (this.idxReader != newReader)
				Logger.LogDebug("InstantiateReader(): newReader → {NewReader}", newReader);
                {
                    ////_idxReader.Dispose(); -- will get disposed upon disposing the searcher
                    Interlocked.Exchange(ref this.idxReader, newReader);
                }

                searcher = new IndexSearcher(this.idxReader);
				Logger.LogDebug("InstantiateReader(): searcher → {Searcher}", searcher);
            }
            else
            {
                // Note: disposing the IndexSearcher instance obtained from the next
                // statement will not close the underlying reader on dispose.
                searcher = new IndexSearcher(FSDirectory.Open(this.IndexFolder));
            }

            var reader = new CachedReader(searcher);
            var cutoffTime = DateTime.Now - TimeSpan.FromSeconds(this.readerTimeSpan.TotalSeconds * 10);
			Logger.LogDebug("InstantiateReader(): reader → {Reader}", reader);
			Logger.LogDebug("InstantiateReader(): cutoffTime → {CutoffTime}", cutoffTime);
            lock (((ICollection)this.oldReaders).SyncRoot)
            {
                this.CheckDisposed();
                this.oldReaders.RemoveAll(r => r.LastUsed <= cutoffTime);
                this.oldReaders.Add(reader);
                Interlocked.Exchange(ref this.reader, reader);
            }
			Logger.LogDebug("<<< Exiting InstantiateReader()");
        }

        private void UpdateLastAccessTimes()
        {
			Logger.LogDebug(">>> Entering UpdateLastAccessTimes()");
            this.lastReadTimeUtc = DateTime.UtcNow;
            if (System.IO.Directory.Exists(this.IndexFolder))
			Logger.LogDebug("UpdateLastAccessTimes(): this.lastReadTimeUtc → {Arg0}", this.lastReadTimeUtc);
			Logger.LogDebug("UpdateLastAccessTimes(): lastReadTimeUtc → {LastReadTimeUtc}", lastReadTimeUtc);
            {
                this.lastDirModifyTimeUtc = System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder);
				Logger.LogDebug("UpdateLastAccessTimes(): lastDirModifyTimeUtc → {LastDirModifyTimeUtc}", lastDirModifyTimeUtc);
            }
			Logger.LogDebug("<<< Exiting UpdateLastAccessTimes()");
        }

        private void RescheduleAccessTimes()
        {
			Logger.LogDebug(">>> Entering RescheduleAccessTimes()");
            // forces re-opening the reader within 30 seconds from now (used mainly by commit)
            var now = DateTime.UtcNow;
            if (this.readerTimeSpan > DefaultRereadTimeSpan && now - this.lastReadTimeUtc > DefaultRereadTimeSpan)
            {
                this.lastReadTimeUtc = now - this.readerTimeSpan - DefaultRereadTimeSpan;
				Logger.LogDebug("RescheduleAccessTimes(): lastReadTimeUtc → {LastReadTimeUtc}", lastReadTimeUtc);
				Logger.LogDebug("RescheduleAccessTimes(): this.lastReadTimeUtc → {Arg0}", this.lastReadTimeUtc);
            }
			Logger.LogDebug("<<< Exiting RescheduleAccessTimes()");
        }

        private void CheckValidIndexFolder()
        {
			Logger.LogDebug(">>> Entering CheckValidIndexFolder()");
            if (!this.ValidateIndexFolder())
            {
                throw new SearchIndexEmptyException(Localization.GetExceptionMessage("SearchIndexingDirectoryNoValid", "Search indexing directory is either empty or does not exist"));
            }
			Logger.LogDebug("<<< Exiting CheckValidIndexFolder()");
        }

        private bool ValidateIndexFolder()
        {
			Logger.LogDebug(">>> Entering ValidateIndexFolder()");
			Logger.LogDebug("<<< Exiting ValidateIndexFolder()");
            return System.IO.Directory.Exists(this.IndexFolder) &&
                   System.IO.Directory.GetFiles(this.IndexFolder, "*.*").Length > 0;
        }

        private void DisposeWriter(bool commit = true)
        {
			Logger.LogDebug(">>> Entering DisposeWriter(commit={Commit})", commit);
            if (this.writer != null)
            {
                lock (this.writerLock)
                {
                    if (this.writer != null)
                    {
                        this.idxReader.Dispose();
                        this.idxReader = null;
						Logger.LogDebug("DisposeWriter(commit={Commit}): idxReader → {IdxReader}", commit, idxReader);
						Logger.LogDebug("DisposeWriter(commit={Commit}): this.idxReader → {Arg1}", commit, this.idxReader);

                        if (commit)
                        {
                            this.writer.Commit();
                        }

                        this.writer.Dispose();
                        this.writer = null;
						Logger.LogDebug("DisposeWriter(commit={Commit}): writer → {Writer}", commit, writer);
						Logger.LogDebug("DisposeWriter(commit={Commit}): this.writer → {Arg1}", commit, this.writer);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting DisposeWriter(commit={Commit})", commit);
        }

        private void DisposeReaders()
        {
			Logger.LogDebug(">>> Entering DisposeReaders()");
            lock (((ICollection)this.oldReaders).SyncRoot)
            {
                foreach (var rdr in this.oldReaders)
                {
                    rdr.Dispose();
                }

                this.oldReaders.Clear();
                this.reader = null;
				Logger.LogDebug("DisposeReaders(): reader → {Reader}", reader);
				Logger.LogDebug("DisposeReaders(): this.reader → {Arg0}", this.reader);
            }
			Logger.LogDebug("<<< Exiting DisposeReaders()");
        }

        private class CachedReader : IDisposable
        {
            private readonly IndexSearcher searcher;

            public CachedReader(IndexSearcher searcher)
            {
                this.searcher = searcher;
                this.UpdateLastUsed();
				Logger.LogDebug("<<< Exiting CachedReader(searcher={Searcher})", searcher);
				Logger.LogDebug("CachedReader(searcher={Searcher}): this.searcher → {Arg1}", searcher, this.searcher);
				Logger.LogDebug("CachedReader(searcher={Searcher}): searcher → {Searcher1}", searcher, searcher);
            }

            public DateTime LastUsed { get; private set; }

            public IndexSearcher GetSearcher()
            {
                this.UpdateLastUsed();
                return this.searcher;
            }

            public void Dispose()
            {
                this.searcher.Dispose();
                this.searcher.IndexReader.Dispose();
            }

            private void UpdateLastUsed()
            {
				Logger.LogDebug(">>> Entering UpdateLastUsed()");
                this.LastUsed = DateTime.Now;
				Logger.LogDebug("<<< Exiting UpdateLastUsed()");
				Logger.LogDebug("UpdateLastUsed(): this.LastUsed → {Arg0}", this.LastUsed);
            }
        }
    }
}
