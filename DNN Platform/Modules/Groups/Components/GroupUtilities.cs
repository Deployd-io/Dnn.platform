// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Journal;

    public class GroupUtilities
    {
        public static void CreateJournalEntry(RoleInfo roleInfo, UserInfo createdBy)
        {
			eventLogger.LogError(">>> Entering CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy})", roleInfo, createdBy);
            var journalController = JournalController.Instance;
            var journalItem = new JournalItem();
			eventLogger.LogError("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem → {JournalItem}", roleInfo, createdBy, journalItem);

            string url = string.Empty;
			eventLogger.LogError("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): url → {Url}", roleInfo, createdBy, url);

            if (roleInfo.Settings.ContainsKey("URL"))
            {
                url = roleInfo.Settings["URL"];
            }

            journalItem.PortalId = roleInfo.PortalID;
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.PortalId → {Arg2}", roleInfo, createdBy, journalItem.PortalId);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.ProfileId → {Arg2}", roleInfo, createdBy, journalItem.ProfileId);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.UserId → {Arg2}", roleInfo, createdBy, journalItem.UserId);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.Title → {Arg2}", roleInfo, createdBy, journalItem.Title);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.ItemData → {Arg2}", roleInfo, createdBy, journalItem.ItemData);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.SocialGroupId → {Arg2}", roleInfo, createdBy, journalItem.SocialGroupId);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.Summary → {Arg2}", roleInfo, createdBy, journalItem.Summary);
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.Body → {Arg2}", roleInfo, createdBy, journalItem.Body);
            journalItem.ProfileId = createdBy.UserID;
            journalItem.UserId = createdBy.UserID;
            journalItem.Title = roleInfo.RoleName;
            journalItem.ItemData = new ItemData { Url = url };
            journalItem.SocialGroupId = roleInfo.RoleID;
            journalItem.Summary = roleInfo.Description;
            journalItem.Body = null;
            journalItem.JournalTypeId = journalController.GetJournalType("groupcreate").JournalTypeId;
            journalItem.ObjectKey = string.Format("groupcreate:{0}:{1}", roleInfo.RoleID.ToString(CultureInfo.InvariantCulture), createdBy.UserID.ToString(CultureInfo.InvariantCulture));

            if (journalController.GetJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey) != null)
            {
                journalController.DeleteJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey);
            }

            journalItem.SecuritySet = string.Empty;
			eventLogger.LogDebug("CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy}): journalItem.SecuritySet → {Arg2}", roleInfo, createdBy, journalItem.SecuritySet);

            if (roleInfo.IsPublic)
            {
                journalItem.SecuritySet += "E,";
            }

            journalController.SaveJournalItem(journalItem, null);
			eventLogger.LogWarning("<<< Exiting CreateJournalEntry(roleInfo={RoleInfo},createdBy={CreatedBy})", roleInfo, createdBy);
        }
    }
}
