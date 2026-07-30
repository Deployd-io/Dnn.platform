// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Prompt;

using DotNetNuke.Abstractions.Prompt;

using Newtonsoft.Json;

/// <summary>A field of console output with plain text content.</summary>
/// <param name="text">The text content.</param>
public class TextOutput(string text) : IConsoleOutput
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
	Logger.LogDebug(">>> Entering TextOutput(text={Text})", text);
    [JsonProperty(PropertyName = "isHtml")]
    public bool IsHtml => false;

    /// <inheritdoc />
    [JsonProperty(PropertyName = "output")]
    public string Output => text;
	Logger.LogDebug("<<< Exiting TextOutput(text={Text})", text);
}
