// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>Represents a permission for a user.</summary>
    [DataContract]
    public class UserPermission
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserPermission));
        /// <summary>Initializes a new instance of the <see cref="UserPermission"/> class.</summary>
        public UserPermission()
        {
			Logger.LogDebug(">>> Entering UserPermission()");
            this.Permissions = new List<Permission>();
			Logger.LogDebug("<<< Exiting UserPermission()");
        }

        /// <summary>Gets or sets the id of the user.</summary>
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        /// <summary>Gets or sets the user display name.</summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the list of permissions for the user.</summary>
        [DataMember(Name = "permissions")]
        public IList<Permission> Permissions { get; set; }
    }
}
