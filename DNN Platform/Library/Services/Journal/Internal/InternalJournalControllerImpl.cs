// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal.Internal
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class InternalJournalControllerImpl : IInternalJournalController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InternalJournalControllerImpl));
        private readonly IJournalDataService dataService;

        /// <summary>Initializes a new instance of the <see cref="InternalJournalControllerImpl"/> class.</summary>
        public InternalJournalControllerImpl()
        {
			Logger.LogError(">>> Entering InternalJournalControllerImpl()");
            this.dataService = JournalDataService.Instance;
			Logger.LogDebug("InternalJournalControllerImpl(): dataService → {DataService}", dataService);
        Logger.LogDebug("InternalJournalControllerImpl(): this.dataService → {Arg0}", this.dataService);
        Logger.LogWarning("<<< Exiting InternalJournalControllerImpl()");
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItemsByProfile(int portalId, int moduleId, int currentUserId, int profileId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering GetJournalItemsByProfile(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},profileId={ProfileId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, profileId, rowIndex, maxRows);
				Logger.LogWarning("<<< Exiting GetJournalItemsByProfile(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},profileId={ProfileId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, profileId, rowIndex, maxRows);
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForProfile(
                        portalId,
                        moduleId,
                        currentUserId,
                        profileId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering GetJournalItemsByGroup(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},groupId={GroupId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, groupId, rowIndex, maxRows);
				Logger.LogWarning("<<< Exiting GetJournalItemsByGroup(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},groupId={GroupId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, groupId, rowIndex, maxRows);
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForGroup(
                        portalId,
                        moduleId,
                        currentUserId,
                        groupId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItems(int portalId, int moduleId, int currentUserId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering GetJournalItems(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, rowIndex, maxRows);
				Logger.LogWarning("<<< Exiting GetJournalItems(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, rowIndex, maxRows);
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForSummary(
                        portalId,
                        moduleId,
                        currentUserId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc />
        public void DeleteFilters(int portalId, int moduleId)
        {
			Logger.LogError(">>> Entering DeleteFilters(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
            this.dataService.Journal_TypeFilters_Delete(portalId, moduleId);
			Logger.LogWarning("<<< Exiting DeleteFilters(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveFilters(int portalId, int moduleId, int journalTypeId)
        {
			Logger.LogError(">>> Entering SaveFilters(portalId={PortalId},moduleId={ModuleId},journalTypeId={JournalTypeId})", portalId, moduleId, journalTypeId);
            this.dataService.Journal_TypeFilters_Save(portalId, moduleId, journalTypeId);
			Logger.LogWarning("<<< Exiting SaveFilters(portalId={PortalId},moduleId={ModuleId},journalTypeId={JournalTypeId})", portalId, moduleId, journalTypeId);
        }
    }
}
