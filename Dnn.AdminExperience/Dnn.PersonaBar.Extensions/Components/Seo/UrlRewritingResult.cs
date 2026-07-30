// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Seo.Components
{
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [JsonObject]
    public class UrlRewritingResult
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public UrlRewritingResult()
        {
			Logger.LogDebug(">>> Entering UrlRewritingResult()");
            var noneText = Localization.GetString("None", Localization.GlobalResourceFile);
            this.RewritingResult = noneText;
			Logger.LogDebug("UrlRewritingResult(): noneText → {NoneText}", noneText);
			Logger.LogDebug("UrlRewritingResult(): this.RewritingResult → {Arg0}", this.RewritingResult);
            this.Culture = noneText;
            this.IdentifiedPage = noneText;
            this.RedirectionResult = noneText;
			Logger.LogDebug("UrlRewritingResult(): this.Culture → {Arg0}", this.Culture);
			Logger.LogDebug("UrlRewritingResult(): this.IdentifiedPage → {Arg0}", this.IdentifiedPage);
			Logger.LogDebug("UrlRewritingResult(): this.RedirectionResult → {Arg0}", this.RedirectionResult);
            this.OperationMessages = noneText;
			Logger.LogDebug("<<< Exiting UrlRewritingResult()");
			Logger.LogDebug("UrlRewritingResult(): this.OperationMessages → {Arg0}", this.OperationMessages);
        }

        [JsonProperty("rewritingResult")]
        public string RewritingResult { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("identifiedPage")]
        public string IdentifiedPage { get; set; }

        [JsonProperty("redirectionReason")]
        public string RedirectionReason { get; set; }

        [JsonProperty("redirectionResult")]
        public string RedirectionResult { get; set; }

        [JsonProperty("operationMessages")]
        public string OperationMessages { get; set; }

        [JsonIgnore]
        public int Status { get; set; }
    }
}
