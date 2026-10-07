using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ContactList.Api;
using Dnn.ContactList.SpaReact.Dto;
using DotNetNuke.Collections;
using DotNetNuke.Web.Api;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.SpaReact.Api
{
    public class ContactsController : DnnApiController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IContactRepository contactRepository;

        public ContactsController(IContactRepository contactRepository)
        {
			_logger.LogError(">>> Entering ContactsController(contactRepository={ContactRepository})", contactRepository);
            this.contactRepository = contactRepository;
			_logger.LogDebug("ContactsController(contactRepository={ContactRepository}): contactRepository → {ContactRepository1}", contactRepository, contactRepository);
        _logger.LogDebug("ContactsController(contactRepository={ContactRepository}): this.contactRepository → {Arg1}", contactRepository, this.contactRepository);
        _logger.LogWarning("<<< Exiting ContactsController(contactRepository={ContactRepository})", contactRepository);
        }

        /// <summary>
        /// The Page method gets a page list of contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
        public HttpResponseMessage Page(string searchTerm, int pageSize, int pageIndex)
        {
			_logger.LogError(">>> Entering Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
            var contactList = contactRepository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
			_logger.LogError("Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): contactList → {ContactList}", searchTerm, pageSize, pageIndex, contactList);
			_logger.LogWarning("<<< Exiting Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
            return Request.CreateResponse(HttpStatusCode.OK, contactList.Serialize(x => new ContactDto(x)));
        }

        /// <summary>
        /// The Contact method gets a single contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
        public HttpResponseMessage Contact(int id)
        {
			_logger.LogError(">>> Entering Contact(id={Id})", id);
            _logger.LogWarning("<<< Exiting Contact(id={Id})", id);
            return Request.CreateResponse(HttpStatusCode.OK, new ContactDto(contactRepository.GetContact(id, PortalSettings.PortalId)));
        }

        /// <summary>
        /// The Update method persists the Contact to the repository
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.Edit)]
        public HttpResponseMessage Contact(int id, ContactDto viewModel)
        {
            Contact contact;

            if (id == -1)
            {
                contact = new Contact
                {
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Social = viewModel.Social,
                    PortalId = PortalSettings.PortalId
                };
                contact.ContactId = this.contactRepository.AddContact(contact, UserInfo.UserID);
            }
            else
            {
                //Update
                contact = this.contactRepository.GetContact(id, PortalSettings.PortalId);

                if (contact != null)
                {
                    contact.FirstName = viewModel.FirstName;
                    contact.LastName = viewModel.LastName;
                    contact.Email = viewModel.Email;
                    contact.Phone = viewModel.Phone;
                    contact.Social = viewModel.Social;
                }
                this.contactRepository.UpdateContact(contact, UserInfo.UserID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new ContactDto(contactRepository.GetContact(contact.ContactId, PortalSettings.PortalId)));
        }

        /// <summary>
        /// The Delete method deletes a single contact
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.Edit)]
        public HttpResponseMessage Delete(int id)
        {
			_logger.LogError(">>> Entering Delete(id={Id})", id);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var contact = this.contactRepository.GetContact(id, PortalSettings.PortalId);
            this.contactRepository.DeleteContact(contact);
			_logger.LogError("Delete(id)={Id}: find query executed in {Arg1} ms", id, __logStart.ElapsedMilliseconds);

            var response = new
            {
				_logger.LogError("Delete(id={Id}): contact → {Contact}", id, contact);
                success = true
            };
			_logger.LogError("Delete(id)={Id}: delete query executed in {Arg1} ms", id, __logStart.ElapsedMilliseconds);
			_logger.LogDebug("Delete(id={Id}): success → {Success}", id, success);

			_logger.LogWarning("<<< Exiting Delete(id={Id})", id);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
