// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Analytics.Config;
    using DotNetNuke.Services.Tokens;

    public abstract class AnalyticsEngineBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public abstract string EngineName { get; }

        public abstract string RenderScript(string scriptTemplate);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ReplaceTokens(string s)
        {
			Logger.LogDebug(">>> Entering ReplaceTokens(s={S})", s);
            var tokenizer = new TokenReplace
            {
                AccessingUser = UserController.Instance.GetCurrentUserInfo(),
                DebugMessages = false,
            };
			Logger.LogDebug("<<< Exiting ReplaceTokens(s={S})", s);
            return tokenizer.ReplaceEnvironmentTokens(s);
        }

        public AnalyticsConfiguration GetConfig()
        {
			Logger.LogDebug(">>> Entering GetConfig()");
			Logger.LogDebug("<<< Exiting GetConfig()");
            return AnalyticsConfiguration.GetConfig(this.EngineName);
        }

        public virtual string RenderCustomScript(AnalyticsConfiguration config)
        {
			Logger.LogDebug(">>> Entering RenderCustomScript(config={Config})", config);
			Logger.LogDebug("<<< Exiting RenderCustomScript(config={Config})", config);
            return string.Empty;
        }
    }
}
