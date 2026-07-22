// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Prompt;

using DotNetNuke.Abstractions.Prompt;

using Newtonsoft.Json;

/// <summary>A field of console output with HTML content.</summary>
/// <param name="html">The HTML content.</param>
public class HtmlOutput(string html) : IConsoleOutput
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
	Logger.LogDebug(">>> Entering HtmlOutput(html={Html})", html);
    [JsonProperty(PropertyName = "isHtml")]
    public bool IsHtml => true;

    /// <inheritdoc />
    [JsonProperty(PropertyName = "output")]
    public string Output => html;
	Logger.LogDebug("<<< Exiting HtmlOutput(html={Html})", html);
}
