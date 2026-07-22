// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Dnn.ContactList.Api;
using Newtonsoft.Json;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    /// <summary>
    /// ContactViewModel represents a Contact object within the Contact Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContactViewModel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Constructs a ContactViewModel
        /// </summary>
        public ContactViewModel()
        {
		Logger.LogDebug(">>> Entering ContactViewModel()");
        }

        /// <summary>
        /// Constructs a ContactViewModel from a Contact object
        /// </summary>
        /// <param name="contact"></param>
        public ContactViewModel(Contact contact)
        {
			Logger.LogDebug(">>> Entering ContactViewModel(contact={Contact})", contact);
            ContactId = contact.ContactId;
            Email = contact.Email;
			Logger.LogDebug("ContactViewModel(contact={Contact}): Email → {Email}", contact, Email);
			Logger.LogDebug("ContactViewModel(contact={Contact}): ContactId → {ContactId}", contact, ContactId);
            FirstName = contact.FirstName;
            LastName = contact.LastName;
			Logger.LogDebug("ContactViewModel(contact={Contact}): FirstName → {FirstName}", contact, FirstName);
			Logger.LogDebug("ContactViewModel(contact={Contact}): LastName → {LastName}", contact, LastName);
            Phone = contact.Phone;
            Social = contact.Social;
			Logger.LogDebug("<<< Exiting ContactViewModel(contact={Contact})", contact);
			Logger.LogDebug("ContactViewModel(contact={Contact}): Phone → {Phone}", contact, Phone);
			Logger.LogDebug("ContactViewModel(contact={Contact}): Social → {Social}", contact, Social);
			Logger.LogDebug("<<< Exiting ContactViewModel()");
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
