// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Api
{
    [Serializable]
    [TableName("Dnn_Contacts")]
    [PrimaryKey("ContactId")]
    [Cacheable("Contacts", CacheItemPriority.Normal, 20)]
    [Scope("PortalId")]
    public class Contact
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public Contact()
        {
			_logger.LogError(">>> Entering Contact()");
            ContactId = -1;
			_logger.LogDebug("Contact(): ContactId → {ContactId}", ContactId);
        _logger.LogWarning("<<< Exiting Contact()");
        }

        public int ContactId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        public string Phone { get; set; }

        public int PortalId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Social { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; }

        public DateTime LastModifiedOnDate { get; set; }
    }
}
