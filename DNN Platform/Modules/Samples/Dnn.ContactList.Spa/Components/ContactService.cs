// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using Dnn.ContactList.Api;
using DotNetNuke.Collections;
using DotNetNuke.Framework;

namespace Dnn.ContactList.Spa.Components
{

    public class ContactService : ServiceLocator<IContactService, ContactService>, IContactService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IContactRepository _repository;

        /// <summary>
        /// Default Constructor constructs a new ContactService
        /// </summary>
        public ContactService()
        {
			Logger.LogDebug(">>> Entering ContactService()");
            _repository = ContactRepository.Instance;
			Logger.LogDebug("<<< Exiting ContactService()");
			Logger.LogDebug("ContactService(): _repository → {_repository}", _repository);
        }


        protected override Func<IContactService> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new ContactService();
        }

        public int AddContact(Contact contact, int userId)
        {
			Logger.LogDebug(">>> Entering AddContact(contact={Contact},userId={UserId})", contact, userId);
			Logger.LogDebug("<<< Exiting AddContact(contact={Contact},userId={UserId})", contact, userId);
            return _repository.AddContact(contact, userId);
        }

        public void DeleteContact(Contact contact)
        {
			Logger.LogDebug(">>> Entering DeleteContact(contact={Contact})", contact);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            _repository.DeleteContact(contact);
			Logger.LogDebug("<<< Exiting DeleteContact(contact={Contact})", contact);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteContact(contact)={Contact}: delete query executed in {Arg1} ms", contact, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteContact(contact)={Contact}: delete query executed in {Arg1} ms", contact, __logStart.ElapsedMilliseconds);
        public Contact GetContact(int contactId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
			Logger.LogDebug("<<< Exiting GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
            return _repository.GetContact(contactId, portalId);
        }

        public IQueryable<Contact> GetContacts(int portalId)
        {
			Logger.LogDebug(">>> Entering GetContacts(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetContacts(portalId={PortalId})", portalId);
            return _repository.GetContacts(portalId);
        }

        public IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize)
        {
			Logger.LogDebug("<<< Exiting GetContacts(searchTerm={SearchTerm},portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize})", searchTerm, portalId, pageIndex, pageSize);
            return _repository.GetContacts(searchTerm, portalId, pageIndex, pageSize);
        }

        public void UpdateContact(Contact contact, int userId)
        {
			Logger.LogDebug(">>> Entering UpdateContact(contact={Contact},userId={UserId})", contact, userId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            _repository.UpdateContact(contact, userId);
			Logger.LogDebug("<<< Exiting UpdateContact(contact={Contact},userId={UserId})", contact, userId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateContact(contact,userId)={Contact},{UserId}: update query executed in {Arg2} ms", contact, userId, __logStart.ElapsedMilliseconds);
        }
    }
	Logger.LogInformation("UpdateContact(contact,userId)={Contact},{UserId}: update query executed in {Arg2} ms", contact, userId, __logStart.ElapsedMilliseconds);
}
