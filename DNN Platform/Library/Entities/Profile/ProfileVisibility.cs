// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Roles;

    [Serializable]
    public class ProfileVisibility
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfileVisibility));
        /// <summary>Initializes a new instance of the <see cref="ProfileVisibility"/> class.</summary>
        public ProfileVisibility()
        {
			Logger.LogDebug(">>> Entering ProfileVisibility()");
            this.RoleVisibilities = new List<RoleInfo>();
            this.RelationshipVisibilities = new List<Relationship>();
			Logger.LogDebug("ProfileVisibility(): RoleVisibilities → {RoleVisibilities}", RoleVisibilities);
			Logger.LogDebug("ProfileVisibility(): RelationshipVisibilities → {RelationshipVisibilities}", RelationshipVisibilities);
			Logger.LogDebug("<<< Exiting ProfileVisibility()");
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileVisibility"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="extendedVisibility">The extended visibility string, a semicolon-delimited list of comma-delimited lists of role IDs and relationship IDs.</param>
        public ProfileVisibility(int portalId, string extendedVisibility)
            : this()
        {
            if (!string.IsNullOrEmpty(extendedVisibility))
            {
                var relationshipController = new RelationshipController();
				Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): relationshipController → {RelationshipController}", portalId, extendedVisibility, relationshipController);

                var lists = extendedVisibility.Split(';');
				Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): lists → {Lists}", portalId, extendedVisibility, lists);

                if (!string.IsNullOrEmpty(lists[0].Substring(2).TrimEnd(',')))
                {
                    var roles = lists[0].Substring(2).TrimEnd(',').Split(',');
					Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): roles → {Roles}", portalId, extendedVisibility, roles);
                    foreach (var role in roles)
                    {
                        int roleId = int.Parse(role, CultureInfo.InvariantCulture);
						Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): roleId → {RoleId}", portalId, extendedVisibility, roleId);
                        RoleInfo userRole = RoleController.Instance.GetRole(portalId, r => r.RoleID == roleId);
						Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): userRole → {UserRole}", portalId, extendedVisibility, userRole);
                        this.RoleVisibilities.Add(userRole);
                    }
                }

                if (!string.IsNullOrEmpty(lists[1].Substring(2).TrimEnd(',')))
                {
                    var relationships = lists[1].Substring(2).TrimEnd(',').Split(',');
					Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): relationships → {Relationships}", portalId, extendedVisibility, relationships);
                    foreach (var relationship in relationships)
                    {
                        Relationship userRelationship = RelationshipController.Instance.GetRelationship(int.Parse(relationship, CultureInfo.InvariantCulture));
						Logger.LogDebug("ProfileVisibility(portalId={PortalId},extendedVisibility={ExtendedVisibility}): userRelationship → {UserRelationship}", portalId, extendedVisibility, userRelationship);
                        this.RelationshipVisibilities.Add(userRelationship);
                    }
                }
            }
        }

        public UserVisibilityMode VisibilityMode { get; set; }

        public IList<RoleInfo> RoleVisibilities { get; set; }

        public IList<Relationship> RelationshipVisibilities { get; set; }

        public ProfileVisibility Clone()
        {
			Logger.LogDebug(">>> Entering Clone()");
            var pv = new ProfileVisibility()
            {
                VisibilityMode = this.VisibilityMode,
                RoleVisibilities = new List<RoleInfo>(this.RoleVisibilities),
                RelationshipVisibilities = new List<Relationship>(this.RelationshipVisibilities),
            };
			Logger.LogDebug("<<< Exiting Clone()");
            return pv;
        }

        public string ExtendedVisibilityString()
        {
			Logger.LogDebug(">>> Entering ExtendedVisibilityString()");
            if (this.VisibilityMode == UserVisibilityMode.FriendsAndGroups)
            {
                var sb = new StringBuilder();
				Logger.LogDebug("ExtendedVisibilityString(): sb → {Sb}", sb);
                sb.Append("G:");
                foreach (var role in this.RoleVisibilities)
                {
                    sb.Append(role.RoleID.ToString(CultureInfo.InvariantCulture) + ",");
                }

                sb.Append(";R:");
                foreach (var relationship in this.RelationshipVisibilities)
                {
                    sb.Append(relationship.RelationshipId.ToString(CultureInfo.InvariantCulture) + ",");
                }

			Logger.LogDebug("<<< Exiting ExtendedVisibilityString()");
                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
