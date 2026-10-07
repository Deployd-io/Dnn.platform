// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Security.Components;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    internal class AuthProviderSupportedAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
			eventLogger.LogError(">>> Entering IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            var propertyName = validationContext.DisplayName;

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), propertyName));
            }

            var allAuthProviders = new SecurityController().GetAuthenticationProviders();
			eventLogger.LogError("IsValid(value={Value},validationContext={ValidationContext}): allAuthProviders → {AllAuthProviders}", value, validationContext, allAuthProviders);

            if (!allAuthProviders.Contains(value))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

			eventLogger.LogWarning("<<< Exiting IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            return ValidationResult.Success;
        }
    }
}
