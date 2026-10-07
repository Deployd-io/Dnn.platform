// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System.Collections.Generic;
    using System.Linq;

    public static class TabExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabExtensions));
        public static bool ContainsAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
			Logger.LogError(">>> Entering ContainsAlias(aliases={Aliases},httpAlias={HttpAlias})", aliases, httpAlias);
			Logger.LogWarning("<<< Exiting ContainsAlias(aliases={Aliases},httpAlias={HttpAlias})", aliases, httpAlias);
            return aliases.Any(tas => string.Equals(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase));
        }

        public static TabAliasSkinInfo FindByHttpAlias(this List<TabAliasSkinInfo> aliases, string httpAlias)
        {
			Logger.LogError(">>> Entering FindByHttpAlias(aliases={Aliases},httpAlias={HttpAlias})", aliases, httpAlias);
			Logger.LogWarning("<<< Exiting FindByHttpAlias(aliases={Aliases},httpAlias={HttpAlias})", aliases, httpAlias);
            return aliases.FirstOrDefault(tas => string.Equals(httpAlias, tas.HttpAlias, System.StringComparison.OrdinalIgnoreCase));
        }

        public static int GetNextAvailableSeqNum(this List<TabUrlInfo> redirects, bool positive)
        {
			Logger.LogError(">>> Entering GetNextAvailableSeqNum(redirects={Redirects},positive={Positive})", redirects, positive);
            int seqNum = 1;
			Logger.LogError("GetNextAvailableSeqNum(redirects={Redirects},positive={Positive}): seqNum → {SeqNum}", redirects, positive, seqNum);
            foreach (TabUrlInfo redirect in redirects)
            {
                if (positive)
                {
                    if (redirect.SeqNum >= seqNum)
                    {
                        seqNum = redirect.SeqNum + 1;

                        // 602 : when seqnum == 0, then duplicate key problems arise
                        if (seqNum == 0)
                        {
                            seqNum++;
                        }
                    }
                }
                else
                {
                    seqNum = -1;
                    if (redirect.SeqNum <= seqNum)
                    {
                        seqNum = redirect.SeqNum - 1;

                        // 602 : don't allow seqnum to become zero
                        if (seqNum == 0)
                        {
                            seqNum--;
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetNextAvailableSeqNum(redirects={Redirects},positive={Positive})", redirects, positive);
            return seqNum;
        }

        public static TabUrlInfo FindByAliasId(this List<TabUrlInfo> redirects, int portalAliasId)
        {
			Logger.LogError(">>> Entering FindByAliasId(redirects={Redirects},portalAliasId={PortalAliasId})", redirects, portalAliasId);
			Logger.LogWarning("<<< Exiting FindByAliasId(redirects={Redirects},portalAliasId={PortalAliasId})", redirects, portalAliasId);
            return redirects.FirstOrDefault(redirect => redirect.PortalAliasId == portalAliasId && portalAliasId != 0);
        }

        public static TabUrlInfo CurrentUrl(this List<TabUrlInfo> redirects, string cultureCode)
        {
			Logger.LogError(">>> Entering CurrentUrl(redirects={Redirects},cultureCode={CultureCode})", redirects, cultureCode);
            TabUrlInfo result = null;
            TabUrlInfo lastSystemUrl = null;
			Logger.LogError("CurrentUrl(redirects={Redirects},cultureCode={CultureCode}): lastSystemUrl → {LastSystemUrl}", redirects, cultureCode, lastSystemUrl);
			Logger.LogError("CurrentUrl(redirects={Redirects},cultureCode={CultureCode}): result → {Result}", redirects, cultureCode, result);
            TabUrlInfo lastCustomUrl = null;
            foreach (var redirect in redirects)
            {
				Logger.LogError("CurrentUrl(redirects={Redirects},cultureCode={CultureCode}): lastCustomUrl → {LastCustomUrl}", redirects, cultureCode, lastCustomUrl);
                if (redirect.HttpStatus == "200" && redirect.CultureCode == cultureCode)
                {
                    if (redirect.IsSystem)
                    {
                        lastSystemUrl = redirect;
                    }
                    else
                    {
                        lastCustomUrl = redirect;
                    }
                }
            }

            // look at the results
            if (lastCustomUrl != null)
            {
                result = lastCustomUrl;
            }
            else if (lastSystemUrl != null)
            {
                result = lastSystemUrl;
            }

			Logger.LogWarning("<<< Exiting CurrentUrl(redirects={Redirects},cultureCode={CultureCode})", redirects, cultureCode);
            return result;
        }
    }
}
