// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;

    [Serializable]
    public class PathSizes
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PathSizes));
        public int MaxAliasDepth { get; set; }

        public int MaxTabPathDepth { get; set; }

        public int MinAliasDepth { get; set; }

        public int MinTabPathDepth { get; set; }

        public void SetAliasDepth(string httpAlias)
        {
			Logger.LogError(">>> Entering SetAliasDepth(httpAlias={HttpAlias})", httpAlias);
            int aliasPathDepth = httpAlias.Length - httpAlias.Replace("/", string.Empty).Length;
			Logger.LogError("SetAliasDepth(httpAlias={HttpAlias}): aliasPathDepth → {AliasPathDepth}", httpAlias, aliasPathDepth);
            if (aliasPathDepth > this.MaxAliasDepth)
            {
                this.MaxAliasDepth = aliasPathDepth;
				Logger.LogDebug("SetAliasDepth(httpAlias={HttpAlias}): this.MaxAliasDepth → {Arg1}", httpAlias, this.MaxAliasDepth);
            }

            if (aliasPathDepth < this.MinAliasDepth)
            {
				Logger.LogWarning("<<< Exiting SetAliasDepth(httpAlias={HttpAlias})", httpAlias);
                this.MinAliasDepth = aliasPathDepth;
				Logger.LogDebug("SetAliasDepth(httpAlias={HttpAlias}): this.MinAliasDepth → {Arg1}", httpAlias, this.MinAliasDepth);
            }
        }

        public void SetTabPathDepth(int tabPathDepth)
        {
			Logger.LogError(">>> Entering SetTabPathDepth(tabPathDepth={TabPathDepth})", tabPathDepth);
            if (tabPathDepth > this.MaxTabPathDepth)
            {
                this.MaxTabPathDepth = tabPathDepth;
				Logger.LogDebug("SetTabPathDepth(tabPathDepth={TabPathDepth}): this.MaxTabPathDepth → {Arg1}", tabPathDepth, this.MaxTabPathDepth);
            }

            if (tabPathDepth < this.MinTabPathDepth)
            {
				Logger.LogWarning("<<< Exiting SetTabPathDepth(tabPathDepth={TabPathDepth})", tabPathDepth);
                this.MinTabPathDepth = tabPathDepth;
				Logger.LogDebug("SetTabPathDepth(tabPathDepth={TabPathDepth}): this.MinTabPathDepth → {Arg1}", tabPathDepth, this.MinTabPathDepth);
            }
        }
    }
}
