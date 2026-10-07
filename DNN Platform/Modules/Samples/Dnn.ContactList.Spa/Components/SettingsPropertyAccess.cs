// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Components
{
    /// <summary>
    /// Implements the Interface IPropertyAccess. This mechanism is used by the Token Replace
    /// Engine implemented to be used in SPA modules. The method 'GetProperty' allows to access to 
    /// information that will be available as properties in the custom token.
    /// </summary>
    public class SettingsPropertyAccess : IPropertyAccess
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly ISettingsService _service;
        private readonly int _moduleId;
        private readonly int _tabId;

        /// <summary>
        /// Default Constructor constructs a new PreloadedDataPropertyAccess
        /// </summary>
        public SettingsPropertyAccess(int moduleId, int tabId) : this(moduleId, tabId, SettingsService.Instance)
        {
		_logger.LogError(">>> Entering SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);

		_logger.LogWarning("<<< Exiting SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
        }

        /// <summary>
        /// Constructor constructs a new PreloadedDataPropertyAccess with a passed in service
        /// </summary>
        public SettingsPropertyAccess(int moduleId, int tabId, ISettingsService service)
        {
			_logger.LogError(">>> Entering SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId},service={Service})", moduleId, tabId, service);
            Requires.NotNull(service);
			_logger.LogWarning("<<< Exiting SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);

            _service = service;
			_logger.LogDebug("SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId},service={Service}): _service → {_service}", moduleId, tabId, service, _service);
            _tabId = tabId;
			_logger.LogDebug("SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId},service={Service}): _tabId → {_tabId}", moduleId, tabId, service, _tabId);
            _moduleId = moduleId;
			_logger.LogDebug("SettingsPropertyAccess(moduleId={ModuleId},tabId={TabId},service={Service}): _moduleId → {_moduleId}", moduleId, tabId, service, _moduleId);
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Setting Property.
        /// </summary>
        /// <param name="propertyName">property name.</param>
        /// <param name="format">format.</param>
        /// <param name="formatProvider">format provider.</param>
        /// <param name="accessingUser">accessing user.</param>
        /// <param name="accessLevel">access level.</param>
        /// <param name="propertyNotFound">Whether found the property value.</param>
        /// <returns></returns>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			_logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            string propertyValue = "";

			_logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): propertyValue → {PropertyValue}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, propertyValue);
            switch (propertyName)
            {
                case "IsFormEnabled":
                    propertyValue = _service.IsFormEnabled(_moduleId, _tabId).ToString().ToLowerInvariant();
                    break;
                case "EmailRegex":
                    propertyValue = Regex.Escape(Globals.glbEmailRegEx);
                    break;
                default:
                    propertyNotFound = true;
                    break;
            }

			_logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return propertyValue;
        }
    }
}
