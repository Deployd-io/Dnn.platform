// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System.IO;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;

    /// <summary>This is responsible for the filters chain that analyzes search documents/queries.</summary>
    internal class SearchQueryAnalyzer : Analyzer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchQueryAnalyzer));
        private readonly bool useStemmingFilter;

        /// <summary>Initializes a new instance of the <see cref="SearchQueryAnalyzer"/> class.</summary>
        /// <param name="useStemmingFilter">Whether to use the stemming filter.</param>
        public SearchQueryAnalyzer(bool useStemmingFilter)
        {
			Logger.LogDebug(">>> Entering SearchQueryAnalyzer(useStemmingFilter={UseStemmingFilter})", useStemmingFilter);
            this.useStemmingFilter = useStemmingFilter;
			Logger.LogDebug("SearchQueryAnalyzer(useStemmingFilter={UseStemmingFilter}): this.useStemmingFilter → {Arg1}", useStemmingFilter, this.useStemmingFilter);
			Logger.LogDebug("<<< Exiting SearchQueryAnalyzer(useStemmingFilter={UseStemmingFilter})", useStemmingFilter);
        }

        /// <inheritdoc />
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
			Logger.LogDebug(">>> Entering TokenStream(fieldName={FieldName},reader={Reader})", fieldName, reader);
            var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

			Logger.LogDebug("TokenStream(fieldName={FieldName},reader={Reader}): wordLengthMinMax → {WordLengthMinMax}", fieldName, reader, wordLengthMinMax);
            // Note: the order of filtering is important for both operation and performance, so we try to make it work faster
            // Also, note that filters are applied from the innermost outwards.
            var filter =
                new ASCIIFoldingFilter(// accents filter
                    new LowerCaseFilter(
                        new LengthFilter(
                            new StandardFilter(
                                new StandardTokenizer(Constants.LuceneVersion, reader)),
                            wordLengthMinMax.Item1,
                            wordLengthMinMax.Item2)));

            if (!this.useStemmingFilter)
            {
			Logger.LogDebug("<<< Exiting TokenStream(fieldName={FieldName},reader={Reader})", fieldName, reader);
                return filter;
            }

            return new PorterStemFilter(filter);
        }
    }
}
