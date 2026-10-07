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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SynonymFilter));
        private readonly Stack<string> synonymStack = new Stack<string>();
        private readonly TermAttribute termAtt;
        private readonly PositionIncrementAttribute posIncrAtt;
        private State current;

        /// <summary>Initializes a new instance of the <see cref="SynonymFilter"/> class.</summary>
        /// <param name="input">The token stream.</param>
        public SynonymFilter(TokenStream input)
            : base(input)
        {
			Logger.LogError(">>> Entering SynonymFilter(input={Input})", input);
            this.termAtt = (TermAttribute)this.AddAttribute<ITermAttribute>();
			Logger.LogDebug("SynonymFilter(input={Input}): termAtt → {TermAtt}", input, termAtt);
        Logger.LogWarning("<<< Exiting SynonymFilter(input={Input})", input);
            this.posIncrAtt = (PositionIncrementAttribute)this.AddAttribute<IPositionIncrementAttribute>();
        }

        /// <inheritdoc />
        public override bool IncrementToken()
        {
			Logger.LogError(">>> Entering IncrementToken()");
            // Pop buffered synonyms
            if (this.synonymStack.Count > 0)
            {
                var syn = this.synonymStack.Pop();
				Logger.LogError("IncrementToken(): syn → {Syn}", syn);
                this.RestoreState(this.current);
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

			Logger.LogWarning("<<< Exiting IncrementToken()");
            return true;
        }

        private bool AddAliasesToStack()
        {
			Logger.LogError(">>> Entering AddAliasesToStack()");
            var portalId = 0; // default
            string cultureCode;
			Logger.LogError("AddAliasesToStack(): portalId → {PortalId}", portalId);
            var searchDoc = Thread.GetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo)) as SearchDocument;
			Logger.LogError("AddAliasesToStack(): searchDoc → {SearchDoc}", searchDoc);
            if (searchDoc != null)
            {
                portalId = searchDoc.PortalId;
                cultureCode = searchDoc.CultureCode;
				Logger.LogDebug("AddAliasesToStack(): cultureCode → {CultureCode}", cultureCode);
                if (string.IsNullOrEmpty(cultureCode))
                {
					Logger.LogError("AddAliasesToStack(): cultureCode → {CultureCode}", cultureCode);
                    var portalInfo = PortalController.Instance.GetPortal(portalId);
					Logger.LogError("AddAliasesToStack(): portalInfo → {PortalInfo}", portalInfo);
                    if (portalInfo != null)
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
			Logger.LogError("AddAliasesToStack(): synonyms → {Synonyms}", synonyms);
            if (synonyms.Length == 0)
            {
                return false;
            }

            var cultureInfo = new CultureInfo(cultureCode);
			Logger.LogError("AddAliasesToStack(): cultureInfo → {CultureInfo}", cultureInfo);
            foreach (var synonym in synonyms)
            {
                this.synonymStack.Push(synonym.ToLower(cultureInfo));
            }

			Logger.LogWarning("<<< Exiting AddAliasesToStack()");
            return true;
        }
    }
}
