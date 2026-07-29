namespace Dnn.ContactList.SpaReact.Dto
{
    public class ContactDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ContactDto()
        {
		Logger.LogDebug(">>> Entering ContactDto()");
		Logger.LogDebug("<<< Exiting ContactDto()");
        }
        public ContactDto(Dnn.ContactList.Api.Contact contact)
        {
			Logger.LogDebug(">>> Entering ContactDto(contact={Contact})", contact);
            this.ContactId = contact.ContactId;
            this.PortalId = contact.PortalId;
			Logger.LogDebug("ContactDto(contact={Contact}): this.ContactId → {Arg1}", contact, this.ContactId);
			Logger.LogDebug("ContactDto(contact={Contact}): this.PortalId → {Arg1}", contact, this.PortalId);
            this.FirstName = contact.FirstName;
            this.LastName = contact.LastName;
            this.Email = contact.Email;
			Logger.LogDebug("ContactDto(contact={Contact}): this.FirstName → {Arg1}", contact, this.FirstName);
			Logger.LogDebug("ContactDto(contact={Contact}): this.LastName → {Arg1}", contact, this.LastName);
			Logger.LogDebug("ContactDto(contact={Contact}): this.Email → {Arg1}", contact, this.Email);
            this.Phone = contact.Phone;
            this.Social = contact.Social;
			Logger.LogDebug("<<< Exiting ContactDto(contact={Contact})", contact);
			Logger.LogDebug("ContactDto(contact={Contact}): this.Phone → {Arg1}", contact, this.Phone);
			Logger.LogDebug("ContactDto(contact={Contact}): this.Social → {Arg1}", contact, this.Social);
        }
        public int ContactId { get; set; }
        public int PortalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Social { get; set; }
    }
}
