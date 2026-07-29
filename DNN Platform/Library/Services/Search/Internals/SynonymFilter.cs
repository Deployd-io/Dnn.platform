// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Tokenattributes;

    /// <summary>SynonymFilter.</summary>
    /// <remarks>Implementation is inspired by sample code in Manning Lucene In Action 2nd Edition, pg. 133.</remarks>
    internal sealed class SynonymFilter : TokenFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Stack<string> synonymStack = new Stack<string>();
        private readonly TermAttribute termAtt;
        private readonly PositionIncrementAttribute posIncrAtt;
        private State current;

        /// <summary>Initializes a new instance of the <see cref="SynonymFilter"/> class.</summary>
        /// <param name="input">The token stream.</param>
        public SynonymFilter(TokenStream input)
            : base(input)
        {
			Logger.LogDebug(">>> Entering SynonymFilter(input={Input})", input);
            this.termAtt = (TermAttribute)this.AddAttribute<ITermAttribute>();
            this.posIncrAtt = (PositionIncrementAttribute)this.AddAttribute<IPositionIncrementAttribute>();
			Logger.LogDebug("<<< Exiting SynonymFilter(input={Input})", input);
			Logger.LogDebug("SynonymFilter(input={Input}): posIncrAtt → {PosIncrAtt}", input, posIncrAtt);
			Logger.LogDebug("SynonymFilter(input={Input}): termAtt → {TermAtt}", input, termAtt);
        }

        /// <inheritdoc />
        public override bool IncrementToken()
        {
			Logger.LogDebug(">>> Entering IncrementToken()");
            // Pop buffered synonyms
            if (this.synonymStack.Count > 0)
            {
                var syn = this.synonymStack.Pop();
                this.RestoreState(this.current);
				Logger.LogDebug("IncrementToken(): syn → {Syn}", syn);
                this.termAtt.SetTermBuffer(syn);

                // set position increment to 0
                this.posIncrAtt.PositionIncrement = 0;
                return true;
            }

            // read next token
            if (!this.input.IncrementToken())
            {
                return false;
            }

            // push synonyms onto stack
            if (this.AddAliasesToStack())
            {
                this.current = this.CaptureState(); // save current token
				Logger.LogDebug("IncrementToken(): current → {Current}", current);
            }

			Logger.LogDebug("<<< Exiting IncrementToken()");
            return true;
        }

        private bool AddAliasesToStack()
        {
			Logger.LogDebug(">>> Entering AddAliasesToStack()");
            var portalId = 0; // default
            string cultureCode;
			Logger.LogDebug("AddAliasesToStack(): portalId → {PortalId}", portalId);
            var searchDoc = Thread.GetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo)) as SearchDocument;
            if (searchDoc != null)
			Logger.LogDebug("AddAliasesToStack(): searchDoc → {SearchDoc}", searchDoc);
            {
                portalId = searchDoc.PortalId;
                cultureCode = searchDoc.CultureCode;
				Logger.LogDebug("AddAliasesToStack(): cultureCode → {CultureCode}", cultureCode);
                if (string.IsNullOrEmpty(cultureCode))
                {
                    var portalInfo = PortalController.Instance.GetPortal(portalId);
                    if (portalInfo != null)
					Logger.LogDebug("AddAliasesToStack(): portalInfo → {PortalInfo}", portalInfo);
                    {
                        cultureCode = portalInfo.DefaultLanguage;
                    }
                }
            }
            else
            {
                cultureCode = Thread.CurrentThread.CurrentCulture.Name;
            }

            var synonyms = SearchHelper.Instance.GetSynonyms(portalId, cultureCode, this.termAtt.Term).ToArray();
            if (synonyms.Length == 0)
			Logger.LogDebug("AddAliasesToStack(): synonyms → {Synonyms}", synonyms);
            {
                return false;
            }

            var cultureInfo = new CultureInfo(cultureCode);
            foreach (var synonym in synonyms)
            {
			Logger.LogDebug("AddAliasesToStack(): cultureInfo → {CultureInfo}", cultureInfo);
                this.synonymStack.Push(synonym.ToLower(cultureInfo));
            }

			Logger.LogDebug("<<< Exiting AddAliasesToStack()");
            return true;
        }
    }
}
