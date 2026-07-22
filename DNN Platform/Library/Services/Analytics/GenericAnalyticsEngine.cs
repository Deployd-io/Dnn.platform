// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Analytics
{
    public class GenericAnalyticsEngine : AnalyticsEngineBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override string EngineName
        {
            get
            {
                return "GenericAnalytics";
            }
        }

        /// <inheritdoc />
        public override string RenderScript(string scriptTemplate)
        {
			Logger.LogDebug(">>> Entering RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
			Logger.LogDebug("<<< Exiting RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
            return this.ReplaceTokens(scriptTemplate);
        }
    }
}
