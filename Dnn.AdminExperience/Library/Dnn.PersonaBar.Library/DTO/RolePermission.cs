// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Dto
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [DataContract]
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class RolePermission
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RolePermission"/> class.</summary>
        public RolePermission()
        {
			Logger.LogDebug(">>> Entering RolePermission()");
            this.Permissions = new List<Permission>();
			Logger.LogDebug("<<< Exiting RolePermission()");
        }

        [DataMember(Name = "roleId")]
        public int RoleId { get; set; }

        [DataMember(Name = "roleName")]
        public string RoleName { get; set; }

        [DataMember(Name = "permissions")]
        public IList<Permission> Permissions { get; set; }

        [DataMember(Name = "locked")]
        public bool Locked { get; set; }

        [DataMember(Name = "default")]
        public bool IsDefault { get; set; }
    }
}
