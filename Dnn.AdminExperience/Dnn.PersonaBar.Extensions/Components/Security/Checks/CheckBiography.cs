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
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="CheckBiography"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public CheckBiography()
            : this(null, null, null, null, null)
        {
		eventLogger.LogError(">>> Entering CheckBiography()");
        eventLogger.LogWarning("<<< Exiting CheckBiography()");
        }

        /// <inheritdoc />
        public string Id => "CheckBiography";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
			eventLogger.LogError("Execute(): result → {Result}", result);
            try
            {
                var richTextDataType = this.listController.GetListEntryInfo("DataType", "RichText");
				eventLogger.LogError("Execute(): richTextDataType → {RichTextDataType}", richTextDataType);
				eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                result.Severity = SeverityEnum.Pass;
                foreach (IPortalInfo portal in this.portalController.GetPortals())
                {
                    var pd = ProfileController.GetPropertyDefinitionByName(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portal.PortalId, "Biography");
					eventLogger.LogError("Execute(): pd → {Pd}", pd);
                    if (pd != null && pd.DataType == richTextDataType.EntryID && !pd.Deleted)
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add($"Portal:{portal.PortalName}");
                    }
                }
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in Execute(): {Arg0}", Exception.Message);
			eventLogger.LogWarning("<<< Exiting Execute()");
                throw;
            }

            return result;
        }
    }
}
