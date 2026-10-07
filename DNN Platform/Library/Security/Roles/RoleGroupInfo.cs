// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// <summary>The RoleGroupInfo class provides the Entity Layer RoleGroup object.</summary>
    [Serializable]
    public class RoleGroupInfo : BaseEntityInfo, IHydratable, IXmlSerializable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleGroupInfo));
        private string description;
        private int portalID = Null.NullInteger;
        private int roleGroupID = Null.NullInteger;
        private string roleGroupName;
        private Dictionary<string, RoleInfo> roles;

        /// <summary>Initializes a new instance of the <see cref="RoleGroupInfo"/> class.</summary>
        public RoleGroupInfo()
        {
		Logger.LogError(">>> Entering RoleGroupInfo()");
		Logger.LogWarning("<<< Exiting RoleGroupInfo()");
        }

        /// <summary>Initializes a new instance of the <see cref="RoleGroupInfo"/> class.</summary>
        /// <param name="roleGroupID">The role group ID.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="loadRoles">Whether to eagerly load the group's roles.</param>
        public RoleGroupInfo(int roleGroupID, int portalID, bool loadRoles)
        {
			Logger.LogError(">>> Entering RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles})", roleGroupID, portalID, loadRoles);
            this.portalID = portalID;
			Logger.LogWarning("<<< Exiting RoleGroupInfo()");
			Logger.LogDebug("RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles}): this.portalID → {Arg3}", roleGroupID, portalID, loadRoles, this.portalID);
            this.roleGroupID = roleGroupID;
            if (loadRoles)
            {
				Logger.LogDebug("RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles}): roleGroupID → {RoleGroupID1}", roleGroupID, portalID, loadRoles, roleGroupID);
				Logger.LogDebug("RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles}): this.roleGroupID → {Arg3}", roleGroupID, portalID, loadRoles, this.roleGroupID);
				Logger.LogDebug("RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles}): portalID → {PortalID1}", roleGroupID, portalID, loadRoles, portalID);
        Logger.LogWarning("<<< Exiting RoleGroupInfo(roleGroupID={RoleGroupID},portalID={PortalID},loadRoles={LoadRoles})", roleGroupID, portalID, loadRoles);
                this.GetRoles();
            }
        }

        /// <summary>Gets the Roles for this Role Group.</summary>
        public Dictionary<string, RoleInfo> Roles
        {
            get
            {
                if (this.roles == null && this.RoleGroupID > Null.NullInteger)
                {
                    this.GetRoles();
                }

                return this.roles;
            }
        }

        /// <summary>Gets or sets the RoleGroup Id.</summary>
        public int RoleGroupID
        {
            get
            {
                return this.roleGroupID;
            }

            set
            {
                this.roleGroupID = value;
            }
        }

        /// <summary>Gets or sets the Portal Id for the RoleGroup.</summary>
        public int PortalID
        {
            get
            {
                return this.portalID;
            }

            set
            {
                this.portalID = value;
            }
        }

        /// <summary>Gets or sets the RoleGroup Name.</summary>
        public string RoleGroupName
        {
            get
            {
                return this.roleGroupName;
            }

            set
            {
                this.roleGroupName = value;
            }
        }

        /// <summary>Gets or sets the Description of the RoleGroup.</summary>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
            }
        }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.RoleGroupID;
            }

            set
            {
                this.RoleGroupID = value;
            }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogError(">>> Entering Fill(dr={Dr})", dr);
            this.RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.RoleGroupName = Null.SetNullString(dr["RoleGroupName"]);
            this.Description = Null.SetNullString(dr["Description"]);

            // Fill base class fields
            this.FillInternal(dr);
			Logger.LogWarning("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			Logger.LogError(">>> Entering GetSchema()");
			Logger.LogWarning("<<< Exiting GetSchema()");
            return null;
        }

        /// <summary>Reads a RoleGroupInfo from an XmlReader.</summary>
        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
			Logger.LogError(">>> Entering ReadXml(reader={Reader})", reader);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "rolegroup":
                            break;
                        case "roles":
                            if (!reader.IsEmptyElement)
                            {
                                this.ReadRoles(reader);
                            }

                            break;
                        case "rolegroupname":
                            this.RoleGroupName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        default:
                            if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                            {
                                reader.ReadElementContentAsString();
                            }

                            break;
                    }
                }
            }
			Logger.LogWarning("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <summary>Writes a RoleGroupInfo to an XmlWriter.</summary>
        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elements
            writer.WriteStartElement("rolegroup");

            // write out properties
            writer.WriteElementString("rolegroupname", this.RoleGroupName);
            writer.WriteElementString("description", this.Description);

            // Write start of roles
            writer.WriteStartElement("roles");

            // Iterate through roles
            if (this.Roles != null)
            {
                foreach (RoleInfo role in this.Roles.Values)
                {
                    role.WriteXml(writer);
                }
            }

            // Write end of Roles
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        private void GetRoles()
        {
			Logger.LogError(">>> Entering GetRoles()");
            this.roles = new Dictionary<string, RoleInfo>();
			Logger.LogDebug("GetRoles(): roles → {Roles}", roles);
            foreach (var role in RoleController.Instance.GetRoles(this.PortalID, r => r.RoleGroupID == this.RoleGroupID))
            {
                this.roles[role.RoleName] = role;
            }
			Logger.LogWarning("<<< Exiting GetRoles()");
        }

        /// <summary>Reads a Roles from an XmlReader.</summary>
        /// <param name="reader">The XmlReader to use.</param>
        private void ReadRoles(XmlReader reader)
        {
			Logger.LogError(">>> Entering ReadRoles(reader={Reader})", reader);
            reader.ReadStartElement("roles");
            this.roles = new Dictionary<string, RoleInfo>();
			Logger.LogDebug("ReadRoles(reader={Reader}): roles → {Roles}", reader, roles);
            do
            {
                reader.ReadStartElement("role");
                var role = new RoleInfo();
				Logger.LogError("ReadRoles(reader={Reader}): role → {Role}", reader, role);
                role.ReadXml(reader);
                this.roles.Add(role.RoleName, role);
            }
            while (reader.ReadToNextSibling("role"));
			Logger.LogWarning("<<< Exiting ReadRoles(reader={Reader})", reader);
        }
    }
}
