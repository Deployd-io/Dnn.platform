// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Api
{
    /// <summary>
    /// ContactRepository provides a concrete implemetation of the IContactRepository interface for interacting with the Contact Repository(Database)
    /// </summary>
    public class ContactRepository : ServiceLocator<IContactRepository, ContactRepository>, IContactRepository
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        protected override Func<IContactRepository> GetFactory()
        {
			_logger.LogError(">>> Entering GetFactory()");
			_logger.LogWarning("<<< Exiting GetFactory()");
            return () => new ContactRepository();
        }

        /// <summary>
        /// AddContact adds a contact to the repository
        /// </summary>
        /// <param name="contact">The contact to add</param>
        /// <returns>The Id of the contact</returns>
        public int AddContact(Contact contact, int userId)
        {
			_logger.LogError(">>> Entering AddContact(contact={Contact},userId={UserId})", contact, userId);
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "PortalId");

            contact.CreatedByUserId = userId;
			_logger.LogDebug("AddContact(contact={Contact},userId={UserId}): contact.CreatedByUserId → {Arg2}", contact, userId, contact.CreatedByUserId);
			_logger.LogDebug("AddContact(contact={Contact},userId={UserId}): contact.LastModifiedByUserId → {Arg2}", contact, userId, contact.LastModifiedByUserId);
			_logger.LogDebug("AddContact(contact={Contact},userId={UserId}): contact.LastModifiedOnDate → {Arg2}", contact, userId, contact.LastModifiedOnDate);
            contact.CreatedOnDate = DateTime.UtcNow;
			_logger.LogDebug("AddContact(contact={Contact},userId={UserId}): contact.CreatedOnDate → {Arg2}", contact, userId, contact.CreatedOnDate);
            contact.LastModifiedByUserId = userId;
            contact.LastModifiedOnDate = DateTime.UtcNow;

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();
				_logger.LogError("AddContact(contact={Contact},userId={UserId}): rep → {Rep}", contact, userId, rep);

                rep.Insert(contact);
            }

			_logger.LogWarning("<<< Exiting AddContact(contact={Contact},userId={UserId})", contact, userId);
            return contact.ContactId;
        }

        /// <summary>
        /// DeleteContact deletes a contact from the repository
        /// </summary>
        /// <param name="contact">The contact to delete</param>
        public void DeleteContact(Contact contact)
        {
			_logger.LogError(">>> Entering DeleteContact(contact={Contact})", contact);
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "ContactId");

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();
				_logger.LogError("DeleteContact(contact={Contact}): rep → {Rep}", contact, rep);

                rep.Delete(contact);
            }
			_logger.LogWarning("<<< Exiting DeleteContact(contact={Contact})", contact);
        }

        public Contact GetContact(int contactId, int portalId)
        {
			_logger.LogError(">>> Entering GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
            Requires.NotNegative("contactId", contactId);
            Requires.NotNegative("portalId", portalId);

			_logger.LogWarning("<<< Exiting GetContact(contactId={ContactId},portalId={PortalId})", contactId, portalId);
            return GetContacts(portalId).SingleOrDefault(c => c.ContactId == contactId);
        }

        /// <summary>
        /// This GetContacts overload retrieves all the Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="portalId">The Id of the portal</param>
        /// <returns>A collection of contacts</returns>
        public IQueryable<Contact> GetContacts(int portalId)
        {
			_logger.LogError(">>> Entering GetContacts(portalId={PortalId})", portalId);
            Requires.NotNegative("portalId", portalId);

            IQueryable<Contact> contacts = null;

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();
				_logger.LogError("GetContacts(portalId={PortalId}): rep → {Rep}", portalId, rep);

                contacts = rep.Get(portalId).AsQueryable();
				_logger.LogError("GetContacts(portalId={PortalId}): contacts → {Contacts}", portalId, contacts);
            }

			_logger.LogWarning("<<< Exiting GetContacts(portalId={PortalId})", portalId);
			_logger.LogWarning("<<< Exiting GetContacts(searchTerm={SearchTerm},portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize})", searchTerm, portalId, pageIndex, pageSize);
            return contacts;
        }

        /// <summary>
        /// This GetContacts overload retrieves a page of Contacts for a portal
        /// </summary>
        /// <remarks>Contacts are cached by portal, so this call will check the cache before going to the Database</remarks>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page Index to fetch - this is 0 based so the first page is when pageIndex = 0</param>
        /// <param name="pageSize">The size of the page to fetch from the database</param>
        /// <returns>A paged collection of contacts</returns>

        public IPagedList<Contact> GetContacts(string searchTerm, int portalId, int pageIndex, int pageSize)
        {
            Requires.NotNegative("portalId", portalId);

            if (string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = "";
            }
            var contacts = GetContacts(portalId).Where(c => c.FirstName.Contains(searchTerm)
                                                                || c.LastName.Contains(searchTerm) ||
                                                                c.Email.Contains(searchTerm));


            return new PagedList<Contact>(contacts, pageIndex, pageSize);
        }

        /// <summary>
        /// UpdateContact updates a contact in the repository
        /// </summary>
        /// <param name="contact">The contact to update</param>
        public void UpdateContact(Contact contact, int userId)
        {
			_logger.LogError(">>> Entering UpdateContact(contact={Contact},userId={UserId})", contact, userId);
            Requires.NotNull(contact);
            Requires.PropertyNotNegative(contact, "ContactId");
            contact.LastModifiedByUserId = userId;
			_logger.LogDebug("UpdateContact(contact={Contact},userId={UserId}): contact.LastModifiedByUserId → {Arg2}", contact, userId, contact.LastModifiedByUserId);
            contact.LastModifiedOnDate = DateTime.UtcNow;
			_logger.LogDebug("UpdateContact(contact={Contact},userId={UserId}): contact.LastModifiedOnDate → {Arg2}", contact, userId, contact.LastModifiedOnDate);

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Contact>();
				_logger.LogError("UpdateContact(contact={Contact},userId={UserId}): rep → {Rep}", contact, userId, rep);

                rep.Update(contact);
            }
			_logger.LogWarning("<<< Exiting UpdateContact(contact={Contact},userId={UserId})", contact, userId);
        }
    }
}
