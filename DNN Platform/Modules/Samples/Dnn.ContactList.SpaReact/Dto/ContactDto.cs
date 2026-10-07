using Microsoft.Extensions.Logging;
namespace Dnn.ContactList.SpaReact.Dto
{
    public class ContactDto
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public ContactDto()
        {
        }
        public ContactDto(Dnn.ContactList.Api.Contact contact)
        {
			_logger.LogError(">>> Entering ContactDto(contact={Contact})", contact);
            this.ContactId = contact.ContactId;
			_logger.LogWarning("<<< Exiting ContactDto(contact={Contact})", contact);
			_logger.LogDebug("ContactDto(contact={Contact}): this.ContactId → {Arg1}", contact, this.ContactId);
            this.PortalId = contact.PortalId;
            this.FirstName = contact.FirstName;
			_logger.LogDebug("ContactDto(contact={Contact}): this.PortalId → {Arg1}", contact, this.PortalId);
			_logger.LogDebug("ContactDto(contact={Contact}): this.FirstName → {Arg1}", contact, this.FirstName);
            this.LastName = contact.LastName;
            this.Email = contact.Email;
            this.Phone = contact.Phone;
            this.Social = contact.Social;
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
