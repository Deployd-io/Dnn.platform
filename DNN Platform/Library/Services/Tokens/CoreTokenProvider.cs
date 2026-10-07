// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    public class CoreTokenProvider : TokenProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreTokenProvider));
        /// <inheritdoc />
        public override bool ContainsTokens(string content, TokenContext context)
        {
			Logger.LogError(">>> Entering ContainsTokens(content={Content},context={Context})", content, context);
			Logger.LogWarning("<<< Exiting ContainsTokens(content={Content},context={Context})", content, context);
            return false; // already determined by BaseCustomTokenReplace
        }

        /// <inheritdoc />
        public override string Tokenize(string content, TokenContext context)
        {
			Logger.LogError(">>> Entering Tokenize(content={Content},context={Context})", content, context);
            var tokenizer = new TokenReplace { TokenContext = context };
			Logger.LogWarning("<<< Exiting Tokenize(content={Content},context={Context})", content, context);
            return tokenizer.ReplaceEnvironmentTokens(content);
        }
    }
}
