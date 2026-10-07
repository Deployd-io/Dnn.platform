// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    internal class TabExistAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
			eventLogger.LogError(">>> Entering IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            var propertyName = validationContext.DisplayName;
            int tabId = Convert.ToInt32(value.ToString(), CultureInfo.InvariantCulture);
			eventLogger.LogError("IsValid(value={Value},validationContext={ValidationContext}): tabId → {TabId}", value, validationContext, tabId);

            if (tabId != -1)
            {
                var portalSetting = PortalController.Instance.GetCurrentSettings();
				eventLogger.LogError("IsValid(value={Value},validationContext={ValidationContext}): portalSetting → {PortalSetting}", value, validationContext, portalSetting);
                var tab = TabController.Instance.GetTab(tabId, portalSetting.PortalId);
				eventLogger.LogError("IsValid(value={Value},validationContext={ValidationContext}): tab → {Tab}", value, validationContext, tab);

                if (tab == null)
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, tabId));
                }

                if (tab.DisableLink)
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.DisabledTab, Components.Constants.LocalResourcesFile), tabId));
                }

                if (tab.IsDeleted)
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.DeletedTab, Components.Constants.LocalResourcesFile), tabId));
                }
            }

			eventLogger.LogWarning("<<< Exiting IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            return ValidationResult.Success;
        }
    }
}
