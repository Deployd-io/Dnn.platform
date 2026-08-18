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
			Logger.LogDebug(">>> Entering SetAliasDepth(httpAlias={HttpAlias})", httpAlias);
            int aliasPathDepth = httpAlias.Length - httpAlias.Replace("/", string.Empty).Length;
            if (aliasPathDepth > this.MaxAliasDepth)
			Logger.LogDebug("SetAliasDepth(httpAlias={HttpAlias}): aliasPathDepth → {AliasPathDepth}", httpAlias, aliasPathDepth);
            {
                this.MaxAliasDepth = aliasPathDepth;
				Logger.LogDebug("SetAliasDepth(httpAlias={HttpAlias}): this.MaxAliasDepth → {Arg1}", httpAlias, this.MaxAliasDepth);
            }

            if (aliasPathDepth < this.MinAliasDepth)
            {
                this.MinAliasDepth = aliasPathDepth;
				Logger.LogDebug("SetAliasDepth(httpAlias={HttpAlias}): this.MinAliasDepth → {Arg1}", httpAlias, this.MinAliasDepth);
            }
			Logger.LogDebug("<<< Exiting SetAliasDepth(httpAlias={HttpAlias})", httpAlias);
        }

        public void SetTabPathDepth(int tabPathDepth)
        {
			Logger.LogDebug(">>> Entering SetTabPathDepth(tabPathDepth={TabPathDepth})", tabPathDepth);
            if (tabPathDepth > this.MaxTabPathDepth)
            {
                this.MaxTabPathDepth = tabPathDepth;
				Logger.LogDebug("SetTabPathDepth(tabPathDepth={TabPathDepth}): this.MaxTabPathDepth → {Arg1}", tabPathDepth, this.MaxTabPathDepth);
            }

            if (tabPathDepth < this.MinTabPathDepth)
            {
                this.MinTabPathDepth = tabPathDepth;
				Logger.LogDebug("SetTabPathDepth(tabPathDepth={TabPathDepth}): this.MinTabPathDepth → {Arg1}", tabPathDepth, this.MinTabPathDepth);
            }
			Logger.LogDebug("<<< Exiting SetTabPathDepth(tabPathDepth={TabPathDepth})", tabPathDepth);
        }
    }
}
