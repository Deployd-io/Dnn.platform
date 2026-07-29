// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;

    using Microsoft.Extensions.DependencyInjection;

    public class CheckBiography(ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IHostSettings hostSettings) : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
		Logger.LogDebug(">>> Entering CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings})", listController, portalController, appStatus, portalGroupController, hostSettings);
		Logger.LogDebug("CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): listController → {ListController1}", listController, portalController, appStatus, portalGroupController, hostSettings, listController);
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
		Logger.LogDebug("CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): portalController → {PortalController1}", listController, portalController, appStatus, portalGroupController, hostSettings, portalController);
		Logger.LogDebug("CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): appStatus → {AppStatus1}", listController, portalController, appStatus, portalGroupController, hostSettings, appStatus);
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): portalGroupController → {PortalGroupController1}", listController, portalController, appStatus, portalGroupController, hostSettings, portalGroupController);
		Logger.LogDebug("CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): hostSettings → {HostSettings1}", listController, portalController, appStatus, portalGroupController, hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="CheckBiography"/> class.</summary>
		Logger.LogDebug("<<< Exiting CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings})", listController, portalController, appStatus, portalGroupController, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public CheckBiography()
            : this(null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        public string Id => "CheckBiography";

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
                var richTextDataType = this.listController.GetListEntryInfo("DataType", "RichText");
                result.Severity = SeverityEnum.Pass;
				Logger.LogDebug("Execute(): richTextDataType → {RichTextDataType}", richTextDataType);
				Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                foreach (IPortalInfo portal in this.portalController.GetPortals())
                {
                    var pd = ProfileController.GetPropertyDefinitionByName(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portal.PortalId, "Biography");
                    if (pd != null && pd.DataType == richTextDataType.EntryID && !pd.Deleted)
					Logger.LogDebug("Execute(): pd → {Pd}", pd);
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add($"Portal:{portal.PortalName}");
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CheckBiography(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings}): {Arg5}", listController, portalController, appStatus, portalGroupController, hostSettings, ex.Message);
                throw;
				Logger.LogError("Exception in Execute(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }
    }
}
