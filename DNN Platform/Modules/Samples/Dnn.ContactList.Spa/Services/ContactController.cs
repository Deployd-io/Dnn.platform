// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.ContactList.Api;
using Dnn.ContactList.Spa.Components;
using Dnn.ContactList.Spa.Services.ViewModels;
using DotNetNuke.Common;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.Spa.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.ContactList.Spa")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContactController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IContactService _contactService;

        /// <summary>
        /// Default Constructor constructs a new ContactController
        /// </summary>
        public ContactController() : this(ContactService.Instance)
        {
		Logger.LogDebug(">>> Entering ContactController()");

		Logger.LogDebug("<<< Exiting ContactController()");
		Logger.LogDebug(">>> Entering ContactController(service={Service})", service);
		Logger.LogDebug("<<< Exiting ContactController(service={Service})", service);
        }

        /// <summary>
        /// Constructor constructs a new ContactController with a passed in repository
        /// </summary>
        public ContactController(IContactService service)
        {
            Requires.NotNull(service);

            _contactService = service;
			Logger.LogDebug("ContactController(service={Service}): _contactService → {_contactService}", service, _contactService);
        }

        /// <summary>
        /// The DeleteContact method deletes a single contact
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeleteContact(ContactViewModel viewModel)
        {
			Logger.LogDebug(">>> Entering DeleteContact(viewModel={ViewModel})", viewModel);
            var contact = _contactService.GetContact(viewModel.ContactId, PortalSettings.PortalId);

			Logger.LogDebug("DeleteContact(viewModel={ViewModel}): contact → {Contact}", viewModel, contact);
            _contactService.DeleteContact(contact);

            var response = new
            {
                success = true
				Logger.LogDebug("DeleteContact(viewModel={ViewModel}): success → {Success}", viewModel, success);
            };

			Logger.LogDebug("<<< Exiting DeleteContact(viewModel={ViewModel})", viewModel);
            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The GetContact method gets a single contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContact(int contactId)
        {
			Logger.LogDebug(">>> Entering GetContact(contactId={ContactId})", contactId);
            var contact = new ContactViewModel(_contactService.GetContact(contactId, PortalSettings.PortalId));

			Logger.LogDebug("GetContact(contactId={ContactId}): contact → {Contact}", contactId, contact);
            var response = new
            {
                success = true,
                data = new
				Logger.LogDebug("GetContact(contactId={ContactId}): success → {Success}", contactId, success);
                {
                    contact = contact
                }
            };

			Logger.LogDebug("<<< Exiting GetContact(contactId={ContactId})", contactId);
            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The GetContacts method gets all the contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContacts(string searchTerm, int pageSize, int pageIndex)
        {
			Logger.LogDebug(">>> Entering GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
            var contactList = _contactService.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
            var contacts = contactList
								Logger.LogDebug("GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): contactList → {ContactList}", searchTerm, pageSize, pageIndex, contactList);
                                 .Select(contact => new ContactViewModel(contact))
                                 .ToList();

            var response = new
            {
                success = true,
                data = new
				Logger.LogDebug("GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): success → {Success}", searchTerm, pageSize, pageIndex, success);
                {
                    results = contacts,
                    totalCount = contactList.TotalCount
					Logger.LogDebug("GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): results → {Results}", searchTerm, pageSize, pageIndex, results);
					Logger.LogDebug("GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): totalCount → {TotalCount}", searchTerm, pageSize, pageIndex, totalCount);
                }
            };

			Logger.LogDebug("<<< Exiting GetContacts(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
            return Request.CreateResponse(response);
        }

        /// <summary>
        /// The SaveContact method persists the Contact to the repository
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SaveContact(ContactViewModel viewModel)
        {
			Logger.LogDebug(">>> Entering SaveContact(viewModel={ViewModel})", viewModel);
            Contact contact;

            if (viewModel.ContactId == -1)
            {
                contact = new Contact
				Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact → {Contact}", viewModel, contact);
                {
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Social = viewModel.Social,
                    PortalId = PortalSettings.PortalId
                };
                _contactService.AddContact(contact, UserInfo.UserID);
            }
            else
            {
                //Update
                contact = _contactService.GetContact(viewModel.ContactId, PortalSettings.PortalId);

                if (contact != null)
                {
                    contact.FirstName = viewModel.FirstName;
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact.FirstName → {Arg1}", viewModel, contact.FirstName);
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact.LastName → {Arg1}", viewModel, contact.LastName);
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact.Email → {Arg1}", viewModel, contact.Email);
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact.Phone → {Arg1}", viewModel, contact.Phone);
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contact.Social → {Arg1}", viewModel, contact.Social);
                    contact.LastName = viewModel.LastName;
                    contact.Email = viewModel.Email;
                    contact.Phone = viewModel.Phone;
                    contact.Social = viewModel.Social;
                }
                _contactService.UpdateContact(contact, UserInfo.UserID);
            }
            var response = new
            {
                success = true,
                data = new
				Logger.LogDebug("SaveContact(viewModel={ViewModel}): success → {Success}", viewModel, success);
                {
                    contactId = contact.ContactId
					Logger.LogDebug("SaveContact(viewModel={ViewModel}): contactId → {ContactId}", viewModel, contactId);
                }
            };

			Logger.LogDebug("<<< Exiting SaveContact(viewModel={ViewModel})", viewModel);
            return Request.CreateResponse(response);

        }
    }
}
