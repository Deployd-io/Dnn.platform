// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Entities.Groups;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Tokens;

    public class GroupItemTokenReplace : Services.Tokens.BaseCustomTokenReplace
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="GroupItemTokenReplace"/> class.</summary>
        /// <param name="groupInfo">The group info.</param>
        public GroupItemTokenReplace(RoleInfo groupInfo)
        {
			Logger.LogDebug(">>> Entering GroupItemTokenReplace(groupInfo={GroupInfo})", groupInfo);
            this.AddPropertySource("groupitem", groupInfo);
			Logger.LogDebug("<<< Exiting GroupItemTokenReplace(groupInfo={GroupInfo})", groupInfo);
        }

        public string ReplaceGroupItemTokens(string source)
        {
			Logger.LogDebug(">>> Entering ReplaceGroupItemTokens(source={Source})", source);
			Logger.LogDebug("<<< Exiting ReplaceGroupItemTokens(source={Source})", source);
            return this.ReplaceTokens(source);
        }
    }
}
