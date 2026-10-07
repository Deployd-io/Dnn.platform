// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Services.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Security.Roles;

    [DataContract]
    public class RoleGroupDto
    {
        public RoleGroupDto()
        {
			eventLogger.LogError(">>> Entering RoleGroupDto()");
            this.Id = -2;
			eventLogger.LogDebug("RoleGroupDto(): Id → {Id}", Id);
        eventLogger.LogDebug("RoleGroupDto(): this.Id → {Arg0}", this.Id);
        eventLogger.LogWarning("<<< Exiting RoleGroupDto()");
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "rolesCount")]
        public int RolesCount { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        public static RoleGroupDto FromRoleGroupInfo(RoleGroupInfo roleGroup)
        {
			eventLogger.LogError(">>> Entering FromRoleGroupInfo(roleGroup={RoleGroup})", roleGroup);
            return new RoleGroupDto()
            {
                Id = roleGroup.RoleGroupID,
                Name = roleGroup.RoleGroupName?.Trim(),
                Description = roleGroup.Description,
                RolesCount = roleGroup.Roles?.Count ?? 0,
            };
        }

        public RoleGroupInfo ToRoleGroupInfo()
        {
			eventLogger.LogError(">>> Entering ToRoleGroupInfo()");
			eventLogger.LogWarning("<<< Exiting ToRoleGroupInfo()");
            return new RoleGroupInfo()
            {
                RoleGroupID = this.Id,
                RoleGroupName = this.Name?.Trim(),
                Description = this.Description ?? string.Empty,
            };
        }
    }
}
