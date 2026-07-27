// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Entities.Users;

    public class CheckRarelyUsedSuperuser : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckRarelyUsedSuperuser";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			Logger.LogDebug("Execute(): result → {Result}", result);
            {
                var totalRecords = 0;

                var superUsers = UserController.GetUsers(-1, 1, int.MaxValue, ref totalRecords, false, true);
                result.Severity = SeverityEnum.Pass;
				Logger.LogDebug("Execute(): superUsers → {SuperUsers}", superUsers);
				Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                foreach (UserInfo user in superUsers)
                {
                    if (DateTime.Now.AddMonths(-6) > user.Membership.LastLoginDate ||
                        DateTime.Now.AddMonths(-6) > user.Membership.LastActivityDate)
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add("Superuser:" + user.Username);
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in Execute(): {Arg0}", ex.Message);
                throw;
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }
    }
}
