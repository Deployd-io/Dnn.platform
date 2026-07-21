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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static string NavigateUrl(int tabId, string[] @params)
        {
			Logger.LogDebug(">>> Entering NavigateUrl(tabId={TabId})", tabId);
            var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("NavigateUrl(tabId={TabId}): navigationManager → {NavigationManager}", tabId, navigationManager);
			Logger.LogDebug("<<< Exiting NavigateUrl(tabId={TabId})", tabId);
            return navigationManager.NavigateURL(tabId, string.Empty, @params);
        }

        public static string[] AddParams(string param, string[] currParams)
        {
			Logger.LogDebug(">>> Entering AddParams(param={Param},currParams={CurrParams})", param, currParams);
            var tmpParams = new string[] { param };
            var intLength = tmpParams.Length;
            var currLength = currParams.Length;
            Array.Resize(ref tmpParams, intLength + currLength);
            currParams.CopyTo(tmpParams, intLength);
			Logger.LogDebug("<<< Exiting AddParams(param={Param},currParams={CurrParams})", param, currParams);
            return tmpParams;
        }

        internal static string ParseTokenWrapper(string template, string token, bool condition)
        {
			Logger.LogDebug(">>> Entering ParseTokenWrapper(template={Template},token={Token},condition={Condition})", template, token, condition);
            var pattern = "(\\[" + token + "\\](.*?)\\[\\/" + token + "\\])";
            var regExp = RegexUtils.GetCachedRegex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			Logger.LogDebug("ParseTokenWrapper(template={Template},token={Token},condition={Condition}): pattern → {Pattern}", template, token, condition, pattern);
			Logger.LogDebug("ParseTokenWrapper(template={Template},token={Token},condition={Condition}): regExp → {RegExp}", template, token, condition, regExp);
            var matches = regExp.Matches(template);
            foreach (Match match in matches)
            {
			Logger.LogDebug("ParseTokenWrapper(template={Template},token={Token},condition={Condition}): matches → {Matches}", template, token, condition, matches);
                template = template.Replace(match.Value, condition ? match.Groups[2].Value : string.Empty);
            }
			Logger.LogDebug("ParseTokenWrapper(template={Template},token={Token},condition={Condition}): template → {Template1}", template, token, condition, template);

			Logger.LogDebug("<<< Exiting ParseTokenWrapper(template={Template},token={Token},condition={Condition})", template, token, condition);
            return template;
        }
    }
}
