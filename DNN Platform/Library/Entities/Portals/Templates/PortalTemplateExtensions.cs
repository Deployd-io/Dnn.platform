// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateExtensions));
    internal static class PortalTemplateExtensions
    {
        internal static PortalTemplateModuleAction ToOldEnum(this Abstractions.Portals.Templates.PortalTemplateModuleAction value)
        {
			Logger.LogDebug(">>> Entering ToOldEnum(value={Value})", value);
            switch (value)
            {
                case Abstractions.Portals.Templates.PortalTemplateModuleAction.Ignore:
                    return PortalTemplateModuleAction.Ignore;
                case Abstractions.Portals.Templates.PortalTemplateModuleAction.Merge:
                    return PortalTemplateModuleAction.Merge;
                default:
					Logger.LogDebug("<<< Exiting ToOldEnum(value={Value})", value);
                    return PortalTemplateModuleAction.Replace;
            }
        }

        internal static Abstractions.Portals.Templates.PortalTemplateModuleAction ToNewEnum(this PortalTemplateModuleAction value)
        {
			Logger.LogDebug(">>> Entering ToNewEnum(value={Value})", value);
            switch (value)
            {
                case PortalTemplateModuleAction.Ignore:
                    return Abstractions.Portals.Templates.PortalTemplateModuleAction.Ignore;
                case PortalTemplateModuleAction.Merge:
                    return Abstractions.Portals.Templates.PortalTemplateModuleAction.Merge;
                default:
					Logger.LogDebug("<<< Exiting ToNewEnum(value={Value})", value);
                    return Abstractions.Portals.Templates.PortalTemplateModuleAction.Replace;
            }
        }
    }
}
