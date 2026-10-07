// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Linq;
using Dnn.ContactList.Spa.Services.ViewModels;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Components
{
    /// <summary>
    /// Implements the Interface IPropertyAccess. This mechanism is used by the Token Replace
    /// Engine implemented to be used in SPA modules. The method 'GetProperty' allows to access to 
    /// information that will be available as properties in the custom token.
    /// </summary>
    public class PreloadedDataPropertyAccess : IPropertyAccess
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IContactService _service;
        private readonly int _portalId;
        private readonly string searchTerm;
        private readonly int pageIndex;
        private readonly int pageSize;

        /// <summary>
        /// Default Constructor constructs a new PreloadedDataPropertyAccess
        /// </summary>
        public PreloadedDataPropertyAccess(int portalId) : this(portalId, ContactService.Instance)
        {
		_logger.LogError(">>> Entering PreloadedDataPropertyAccess(portalId={PortalId})", portalId);

		_logger.LogWarning("<<< Exiting PreloadedDataPropertyAccess(portalId={PortalId})", portalId);
        }

        /// <summary>
        /// Constructor constructs a new PreloadedDataPropertyAccess with a passed in service
        /// </summary>
        public PreloadedDataPropertyAccess(int portalId, IContactService service)
        {
			_logger.LogError(">>> Entering PreloadedDataPropertyAccess(portalId={PortalId},service={Service})", portalId, service);
            Requires.NotNull(service);
			_logger.LogWarning("<<< Exiting PreloadedDataPropertyAccess(portalId={PortalId})", portalId);

            _service = service;
			_logger.LogDebug("PreloadedDataPropertyAccess(portalId={PortalId},service={Service}): _service → {_service}", portalId, service, _service);
            _portalId = portalId;
			_logger.LogDebug("PreloadedDataPropertyAccess(portalId={PortalId},service={Service}): _portalId → {_portalId}", portalId, service, _portalId);
            searchTerm = "";
			_logger.LogError("PreloadedDataPropertyAccess(portalId={PortalId},service={Service}): searchTerm → {SearchTerm}", portalId, service, searchTerm);
			_logger.LogDebug("PreloadedDataPropertyAccess(portalId={PortalId},service={Service}): searchTerm → {SearchTerm}", portalId, service, searchTerm);
            pageIndex = 0;
            pageSize = 6;
			_logger.LogDebug("PreloadedDataPropertyAccess(portalId={PortalId},service={Service}): pageIndex → {PageIndex}", portalId, service, pageIndex);
        }

        /// <summary>
        /// Token Cacheability.
        /// </summary>
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Preloaded Data Property.
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
            var contactList = _service.GetContacts(searchTerm, _portalId, pageIndex, pageSize);
            var contacts = contactList
                                 .Select(contact => new ContactViewModel(contact))
                                 .ToList();
								_logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): contactList → {ContactList}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, contactList);
			_logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return "{ results: " + JsonConvert.SerializeObject(contacts) + ", pageCount: " + contactList.TotalCount + "}";
        }
    }
}
