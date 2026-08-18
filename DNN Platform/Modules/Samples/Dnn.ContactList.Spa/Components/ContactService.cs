// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using Dnn.ContactList.Api;
using DotNetNuke.Collections;
using DotNetNuke.Framework;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Components
{

    public class ContactService : ServiceLocator<IContactService, ContactService>, IContactService
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IContactRepository _repository;

        /// <summary>
        /// Default Constructor constructs a new ContactService
        /// </summary>
        public ContactService()
        {
			_logger.LogDebug(">>> Entering ContactService()");
            _repository = ContactRepository.Instance;
			_logger.LogDebug("ContactService(): _repository → {_repository}", _repository);
			_logger.LogDebug("<<< Exiting ContactService()");
        }


        protected override Func<IContactService> GetFactory()
        {
			_logger.LogDebug(">>> Entering GetFactory()");
			_logger.LogDebug("<<< Exiting GetFactory()");
            return () => new ContactService();
        }

        public int AddContact(Contact contact, int userId)
        {
			_logger.LogDebug(">>> Entering AddContact(contact={Contact},userId={UserId})", contact, userId);
			_logger.LogDebug("<<< Exiting AddContact(contact={Contact},userId={UserId})", contact, userId);
            return _repository.AddContact(contact, userId);
        }

        public void DeleteContact(Contact contact)
        {
			_logger.LogDebug(">>> Entering DeleteContact(contact={Contact})", contact);
            _repository.DeleteContact(contact);
			_logger.LogDebug("<<< Exiting DeleteContact(contact={Contact})", contact);
        }

        public Contact GetContact(int contactId, int portalId)
        {
			_logger.LogDebug(">>> Entering GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
			_logger.LogDebug("<<< Exiting GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
            return _repository.GetContact(contactId, portalId);
        }

        public IQueryable<Contact> GetContacts(int portalId)
        {
			_logger.LogDebug(">>> Entering GetContacts(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetContacts(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetContacts(searchTerm={SearchTerm},portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize})", searchTerm, portalId, pageIndex, pageSize);
            return _repository.GetContacts(portalId);
        }

        public IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize)
        {
            return _repository.GetContacts(searchTerm, portalId, pageIndex, pageSize);
        }

        public void UpdateContact(Contact contact, int userId)
        {
			_logger.LogDebug(">>> Entering UpdateContact(contact={Contact},userId={UserId})", contact, userId);
            _repository.UpdateContact(contact, userId);
			_logger.LogDebug("<<< Exiting UpdateContact(contact={Contact},userId={UserId})", contact, userId);
        }
    }
}
