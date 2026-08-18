// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Dnn.ContactList.Api;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    /// <summary>
    /// ContactViewModel represents a Contact object within the Contact Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContactViewModel
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Constructs a ContactViewModel
        /// </summary>
        public ContactViewModel()
        {
		_logger.LogDebug(">>> Entering ContactViewModel()");
		_logger.LogDebug("<<< Exiting ContactViewModel()");
        }

        /// <summary>
        /// Constructs a ContactViewModel from a Contact object
        /// </summary>
        /// <param name="contact"></param>
        public ContactViewModel(Contact contact)
        {
			_logger.LogDebug(">>> Entering ContactViewModel(contact={Contact})", contact);
            ContactId = contact.ContactId;
            Email = contact.Email;
            FirstName = contact.FirstName;
			_logger.LogDebug("ContactViewModel(contact={Contact}): Email → {Email}", contact, Email);
			_logger.LogDebug("ContactViewModel(contact={Contact}): FirstName → {FirstName}", contact, FirstName);
            LastName = contact.LastName;
            Phone = contact.Phone;
			_logger.LogDebug("ContactViewModel(contact={Contact}): LastName → {LastName}", contact, LastName);
			_logger.LogDebug("ContactViewModel(contact={Contact}): Phone → {Phone}", contact, Phone);
            Social = contact.Social;
			_logger.LogDebug("<<< Exiting ContactViewModel(contact={Contact})", contact);
        }

        /// <summary>
        /// The Id of the contact
        /// </summary>
        [JsonProperty("contactId")]
        public int ContactId { get; set; }

        /// <summary>
        /// The Email of the contact
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The First Name of the contact
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The Last Name of the contact
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The Phone of the contact
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// The Social of the contact
        /// </summary>
        [JsonProperty("social")]
        public string Social { get; set; }
    }
}
