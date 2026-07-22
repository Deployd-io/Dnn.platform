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
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Registration;

    [AttributeUsage(AttributeTargets.Property)]
    internal class RegistrationFieldsAttribute : ValidationAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public RegistrationFieldsAttribute(string registrationFormType, string requireUniqueDisplayName, string displayNameFormat)
        {
			Logger.LogDebug(">>> Entering RegistrationFieldsAttribute(registrationFormType={RegistrationFormType},requireUniqueDisplayName={RequireUniqueDisplayName},displayNameFormat={DisplayNameFormat})", registrationFormType, requireUniqueDisplayName, displayNameFormat);
            this.RegistrationFormTypePropertyName = registrationFormType;
            this.RequireUniqueDisplayNamePropertyName = requireUniqueDisplayName;
			Logger.LogDebug("RegistrationFieldsAttribute(registrationFormType={RegistrationFormType},requireUniqueDisplayName={RequireUniqueDisplayName},displayNameFormat={DisplayNameFormat}): this.RegistrationFormTypePropertyName → {Arg3}", registrationFormType, requireUniqueDisplayName, displayNameFormat, this.RegistrationFormTypePropertyName);
			Logger.LogDebug("RegistrationFieldsAttribute(registrationFormType={RegistrationFormType},requireUniqueDisplayName={RequireUniqueDisplayName},displayNameFormat={DisplayNameFormat}): this.RequireUniqueDisplayNamePropertyName → {Arg3}", registrationFormType, requireUniqueDisplayName, displayNameFormat, this.RequireUniqueDisplayNamePropertyName);
            this.DisplayNameFormatPropertyName = displayNameFormat;
			Logger.LogDebug("<<< Exiting RegistrationFieldsAttribute(registrationFormType={RegistrationFormType},requireUniqueDisplayName={RequireUniqueDisplayName},displayNameFormat={DisplayNameFormat})", registrationFormType, requireUniqueDisplayName, displayNameFormat);
			Logger.LogDebug("RegistrationFieldsAttribute(registrationFormType={RegistrationFormType},requireUniqueDisplayName={RequireUniqueDisplayName},displayNameFormat={DisplayNameFormat}): this.DisplayNameFormatPropertyName → {Arg3}", registrationFormType, requireUniqueDisplayName, displayNameFormat, this.DisplayNameFormatPropertyName);
        }

        public string RegistrationFormTypePropertyName { get; private set; }

        public string RequireUniqueDisplayNamePropertyName { get; private set; }

        public string DisplayNameFormatPropertyName { get; private set; }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
			Logger.LogDebug(">>> Entering IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            var registrationFormTypeValue = string.Empty;

			Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): registrationFormTypeValue → {RegistrationFormTypeValue}", value, validationContext, registrationFormTypeValue);
            try
            {
                registrationFormTypeValue = validationContext.ObjectType.GetProperty(this.RegistrationFormTypePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
            }
            catch
            {
				Logger.LogError("Exception in IsValid(value={Value},validationContext={ValidationContext}): {Arg2}", value, validationContext, ex.Message);
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName, registrationFormTypeValue));
            }

            if (string.IsNullOrWhiteSpace(registrationFormTypeValue))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName));
            }

            if (!int.TryParse(registrationFormTypeValue, out var registrationFormType))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName, registrationFormTypeValue));
            }

            if (registrationFormType == 1)
            {
                var registrationFields = value.ToString();
                var registrationTokens = registrationFields.Split(',');
				Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): registrationFields → {RegistrationFields}", value, validationContext, registrationFields);
				Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): registrationTokens → {RegistrationTokens}", value, validationContext, registrationTokens);
                var portalId = PortalController.Instance.GetCurrentSettings().PortalId;

				Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): portalId → {PortalId}", value, validationContext, portalId);
                foreach (var registrationField in registrationTokens)
                {
                    if (!string.IsNullOrWhiteSpace(registrationField) && !RegistrationProfileController.Instance.Search(portalId, registrationField).Any())
                    {
                        return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), validationContext.DisplayName, registrationField));
                    }
                }

                if (!registrationFields.Contains("Email"))
                {
                    return new ValidationResult(Localization.GetString(Constants.NoEmail, Constants.LocalResourcesFile));
                }

                var requireUniqueDisplayNameValue = string.Empty;

				Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): requireUniqueDisplayNameValue → {RequireUniqueDisplayNameValue}", value, validationContext, requireUniqueDisplayNameValue);
                try
                {
                    requireUniqueDisplayNameValue = validationContext.ObjectType.GetProperty(this.RequireUniqueDisplayNamePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
                }
                catch
                {
					Logger.LogError("Exception in IsValid(value={Value},validationContext={ValidationContext}): {Arg2}", value, validationContext, ex.Message);
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
                }

                if (string.IsNullOrWhiteSpace(requireUniqueDisplayNameValue))
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName));
                }

                if (!bool.TryParse(requireUniqueDisplayNameValue, out var requireUniqueDisplayName))
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
                }

                if (!registrationFields.Contains("DisplayName") && requireUniqueDisplayName)
                {
                    PortalController.UpdatePortalSetting(portalId, "Registration_RegistrationFormType", "0", false);
                    return new ValidationResult(Localization.GetString(Constants.NoDisplayName, Constants.LocalResourcesFile));
                }

                var displayNameFormatValue = string.Empty;

				Logger.LogDebug("IsValid(value={Value},validationContext={ValidationContext}): displayNameFormatValue → {DisplayNameFormatValue}", value, validationContext, displayNameFormatValue);
                try
                {
                    displayNameFormatValue = validationContext.ObjectType.GetProperty(this.DisplayNameFormatPropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
                }
                catch
                {
                    return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.DisplayNameFormatPropertyName, displayNameFormatValue));
                }

                if (!registrationFields.Contains("DisplayName") && string.IsNullOrWhiteSpace(displayNameFormatValue))
                {
                    return new ValidationResult(Localization.GetString(Constants.IncorrectDisplayNameConfiguration, Constants.LocalResourcesFile));
                }
            }

			Logger.LogDebug("<<< Exiting IsValid(value={Value},validationContext={ValidationContext})", value, validationContext);
            return ValidationResult.Success;
        }
    }
}
