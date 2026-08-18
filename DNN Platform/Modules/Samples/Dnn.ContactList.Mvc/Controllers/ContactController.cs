// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.Mvc;
using Dnn.ContactList.Api;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Mvc.Controllers
{
    /// <summary>
    /// ContactController is the MVC Controller class for managing Contacts in the UI
    /// </summary>
    [DnnHandleError]
    public class ContactController : DnnController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IContactRepository _repository;

        /// <summary>
        /// Default Constructor constructs a new ContactController
        /// </summary>
        public ContactController() : this(ContactRepository.Instance) { }

        /// <summary>
        /// Constructor constructs a new ContactController with a passed in repository
        /// </summary>
        public ContactController(IContactRepository repository)
        {
			_logger.LogDebug(">>> Entering ContactController(repository={Repository})", repository);
            Requires.NotNull(repository);

            _repository = repository;
			_logger.LogDebug("<<< Exiting ContactController(repository={Repository})", repository);
			_logger.LogDebug("ContactController(repository={Repository}): _repository → {_repository}", repository, _repository);
        }

        /// <summary>
        /// The Delete method is used to delete a Contact
        /// </summary>
        /// <param name="contactId">The Id of the Contact to delete</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Delete(int contactId)
        {
			_logger.LogDebug(">>> Entering Delete(contactId={ContactId})", contactId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var contact = _repository.GetContact(contactId, PortalSettings.PortalId);

			_logger.LogInformation("Delete(contactId)={ContactId}: find query executed in {Arg1} ms", contactId, __logStart.ElapsedMilliseconds);
            _repository.DeleteContact(contact);

			_logger.LogDebug("Delete(contactId={ContactId}): contact → {Contact}", contactId, contact);
			_logger.LogDebug("<<< Exiting Delete(contactId={ContactId})", contactId);
            return RedirectToDefaultRoute();
        }

        /// <summary>
        /// This Edit method is used to set up editing a Contact
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int contactId = -1)
        {
			_logger.LogDebug(">>> Entering Edit(contactId={ContactId})", contactId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var contact = (contactId == -1)
                        ? new Contact { PortalId = PortalSettings.PortalId }
                        : _repository.GetContact(contactId, PortalSettings.PortalId);
						_logger.LogInformation("Edit(contactId)={ContactId}: find query executed in {Arg1} ms", contactId, __logStart.ElapsedMilliseconds);

            return View(contact);
        }

        /// <summary>
        /// This Edit method is used to save the posted Contact
        /// </summary>
        /// <param name="contact">The contact to save</param>
        /// <returns></returns>
        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.PortalId = PortalSettings.PortalId;

                if (contact.ContactId == -1)
                {
                    _repository.AddContact(contact, User.UserID);
                }
                else
                {
                    var existing = _repository.GetContact(contact.ContactId, PortalSettings.PortalId);
                    existing.FirstName = contact.FirstName;
                    existing.LastName = contact.LastName;
                    existing.Email = contact.Email;
                    existing.Phone = contact.Phone;
                    existing.Social = contact.Social;
                    _repository.UpdateContact(existing, User.UserID);
                }

                return RedirectToDefaultRoute();
            }
            else
            {
                return View(contact);
            }
        }

        /// <summary>
        /// The Index method is the default Action method.
        /// </summary>
        /// <param name="searchTerm">Term to search.</param>
        /// <param name="pageIndex">Index of the current page.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns></returns>
        [ModuleAction(ControlKey = "Edit", TitleKey = "AddContact")]
        public ActionResult Index(string searchTerm = "", int pageIndex = 0)
        {
			_logger.LogDebug(">>> Entering Index(searchTerm={SearchTerm},pageIndex={PageIndex})", searchTerm, pageIndex);
            var contacts = _repository.GetContacts(searchTerm, PortalSettings.PortalId, pageIndex, ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("PageSize", 6));

			_logger.LogDebug("Index(searchTerm={SearchTerm},pageIndex={PageIndex}): contacts → {Contacts}", searchTerm, pageIndex, contacts);
			_logger.LogDebug("<<< Exiting Index(searchTerm={SearchTerm},pageIndex={PageIndex})", searchTerm, pageIndex);
            return View(contacts);
        }

        public JsonResult GetJsonResult()
        {
			_logger.LogDebug(">>> Entering GetJsonResult()");
			_logger.LogDebug("<<< Exiting GetJsonResult()");
            return new JsonResult()
            {
                Data = new
                {
                    User.UserID,
                    PortalSettings.PortalId,
                    Alias = PortalSettings.PortalAlias.HTTPAlias,
                    Time = DateTime.Now.ToString("HH:mm:ss ttt")
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDemoPartial()
        {
			_logger.LogDebug(">>> Entering GetDemoPartial()");
            TempData["UserID"] = User.UserID;
            ViewData["PortalId"] = PortalSettings.PortalId;
            ViewBag.Alias = PortalSettings.PortalAlias.HTTPAlias;
			_logger.LogDebug("GetDemoPartial(): ViewBag.Alias → {Arg0}", ViewBag.Alias);

			_logger.LogDebug("<<< Exiting GetDemoPartial()");
            return PartialView("_DemoPartial", DateTime.Now);
        }
    }
}
