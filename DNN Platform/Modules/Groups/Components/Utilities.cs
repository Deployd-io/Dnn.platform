// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    public class Utilities
    {
        public static string NavigateUrl(int tabId, string[] @params)
        {
			eventLogger.LogError(">>> Entering NavigateUrl(tabId={TabId})", tabId);
            var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			eventLogger.LogError("NavigateUrl(tabId={TabId}): navigationManager → {NavigationManager}", tabId, navigationManager);
			eventLogger.LogWarning("<<< Exiting NavigateUrl(tabId={TabId})", tabId);
            return navigationManager.NavigateURL(tabId, string.Empty, @params);
        }

        public static string[] AddParams(string param, string[] currParams)
        {
			eventLogger.LogError(">>> Entering AddParams(param={Param},currParams={CurrParams})", param, currParams);
            var tmpParams = new string[] { param };
            var intLength = tmpParams.Length;
            var currLength = currParams.Length;
            Array.Resize(ref tmpParams, intLength + currLength);
            currParams.CopyTo(tmpParams, intLength);
			eventLogger.LogWarning("<<< Exiting AddParams(param={Param},currParams={CurrParams})", param, currParams);
            return tmpParams;
        }

        internal static string ParseTokenWrapper(string template, string token, bool condition)
        {
			eventLogger.LogError(">>> Entering ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition})", template, condition);
            var pattern = "(\\[" + token + "\\](.*?)\\[\\/" + token + "\\])";
            var regExp = RegexUtils.GetCachedRegex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			eventLogger.LogError("ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition}): regExp → {RegExp}", template, condition, regExp);
			eventLogger.LogError("ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition}): pattern → {Pattern}", template, condition, pattern);
            var matches = regExp.Matches(template);
            foreach (Match match in matches)
            {
                template = template.Replace(match.Value, condition ? match.Groups[2].Value : string.Empty);
            }
			eventLogger.LogError("ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition}): matches → {Matches}", template, condition, matches);
			eventLogger.LogError("ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition}): template → {Template1}", template, condition, template);

			eventLogger.LogWarning("<<< Exiting ParseTokenWrapper(template={Template},token=<redacted>,condition={Condition})", template, condition);
            return template;
        }
    }
}
