using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ContactList.Api;
using Dnn.ContactList.SpaReact.Dto;
using DotNetNuke.Collections;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.SpaReact.Api
{
    public class ContactsController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IContactRepository contactRepository;

        public ContactsController(IContactRepository contactRepository)
        {
			Logger.LogDebug(">>> Entering ContactsController(contactRepository={ContactRepository})", contactRepository);
            this.contactRepository = contactRepository;
			Logger.LogDebug("<<< Exiting ContactsController(contactRepository={ContactRepository})", contactRepository);
			Logger.LogDebug("ContactsController(contactRepository={ContactRepository}): contactRepository → {ContactRepository1}", contactRepository, contactRepository);
			Logger.LogDebug("ContactsController(contactRepository={ContactRepository}): this.contactRepository → {Arg1}", contactRepository, this.contactRepository);
        }

        /// <summary>
        /// The Page method gets a page list of contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = DotNetNuke.Security.SecurityAccessLevel.View)]
        public HttpResponseMessage Page(string searchTerm, int pageSize, int pageIndex)
        {
			Logger.LogDebug(">>> Entering Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
            var contactList = contactRepository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, pageSize);
			Logger.LogDebug("Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex}): contactList → {ContactList}", searchTerm, pageSize, pageIndex, contactList);
			Logger.LogDebug("<<< Exiting Page(searchTerm={SearchTerm},pageSize={PageSize},pageIndex={PageIndex})", searchTerm, pageSize, pageIndex);
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
			Logger.LogDebug(">>> Entering Contact(id={Id})", id);
			Logger.LogDebug("<<< Exiting Contact(id={Id})", id);
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
			Logger.LogDebug(">>> Entering Delete(id={Id})", id);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var contact = this.contactRepository.GetContact(id, PortalSettings.PortalId);
            this.contactRepository.DeleteContact(contact);
			Logger.LogInformation("Delete(id)={Id}: find query executed in {Arg1} ms", id, __logStart.ElapsedMilliseconds);

            var response = new
			Logger.LogDebug("Delete(id={Id}): contact → {Contact}", id, contact);
			Logger.LogInformation("Delete(id)={Id}: delete query executed in {Arg1} ms", id, __logStart.ElapsedMilliseconds);
            {
                success = true
				Logger.LogDebug("Delete(id={Id}): success → {Success}", id, success);
            };

			Logger.LogDebug("<<< Exiting Delete(id={Id})", id);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
