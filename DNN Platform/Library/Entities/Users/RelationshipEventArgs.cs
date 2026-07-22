// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;

    using DotNetNuke.Entities.Users.Social;

    public class RelationshipEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RelationshipEventArgs"/> class.</summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="portalId">The portal ID.</param>
        internal RelationshipEventArgs(UserRelationship relationship, int portalId)
        {
			Logger.LogDebug(">>> Entering RelationshipEventArgs(relationship={Relationship},portalId={PortalId})", relationship, portalId);
            this.Relationship = relationship;
            this.PortalID = portalId;
			Logger.LogDebug("<<< Exiting RelationshipEventArgs(relationship={Relationship},portalId={PortalId})", relationship, portalId);
			Logger.LogDebug("RelationshipEventArgs(relationship={Relationship},portalId={PortalId}): this.Relationship → {Arg2}", relationship, portalId, this.Relationship);
			Logger.LogDebug("RelationshipEventArgs(relationship={Relationship},portalId={PortalId}): this.PortalID → {Arg2}", relationship, portalId, this.PortalID);
        }

        public UserRelationship Relationship { get; private set; }

        public int PortalID { get; private set; }
    }
}
