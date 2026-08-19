// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    internal class CultureCodeExistAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
			eventLogger.LogDebug(">>> Entering IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            var propertyName = validationContext.DisplayName;

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.EmptyValue, Components.Constants.LocalResourcesFile), propertyName));
            }

            var allLocales = LocaleController.Instance.GetLocales(PortalController.Instance.GetCurrentSettings().PortalId);
			eventLogger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): allLocales → {AllLocales}", value, validationContext, allLocales);

            if (!allLocales.Select(l => l.Value.Code.ToLowerInvariant()).Contains(value.ToString().ToLowerInvariant()))
            {
			eventLogger.LogDebug("<<< Exiting IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            return ValidationResult.Success;
        }
    }
}
