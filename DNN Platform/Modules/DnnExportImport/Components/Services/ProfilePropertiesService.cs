// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.ProfileProperties;
    using DotNetNuke.Common.Utilities;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    /// <summary>An export service for profile properties.</summary>
    public class ProfilePropertiesService : BasePortableService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfilePropertiesService));
        /// <inheritdoc />
        public override string Category => Constants.Category_ProfileProps;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 5;

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogError(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): fromDate → {FromDate}", exportJob, exportDto, fromDate);
            if (this.CheckCancelled(exportJob))
            {
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): toDate → {ToDate}", exportJob, exportDto, toDate);
                return;
            }

            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var profileProperties = CBO.FillCollection<ExportProfileProperty>(
                    DataProvider.Instance()
                        .GetPropertyDefinitionsByPortal(
                            exportJob.PortalId,
                            exportDto.IncludeDeletions,
                            toDate,
                            fromDate))
                .ToList();
            this.CheckPoint.Progress = 50;

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? profileProperties.Count : this.CheckPoint.TotalItems;
            this.CheckPointStageCallback(this);

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);
            this.Repository.CreateItems(profileProperties);
            this.Result.AddSummary("Exported Profile Properties", profileProperties.Count.ToString(CultureInfo.InvariantCulture));
            this.CheckPoint.Progress = 100;
            this.CheckPoint.ProcessedItems = profileProperties.Count;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Stage++;
            this.CheckPointStageCallback(this);
			Logger.LogWarning("<<< Exiting ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
        }

        /// <inheritdoc />
        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
			Logger.LogError(">>> Entering ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            var profileProperties = this.Repository.GetAllItems<ExportProfileProperty>().ToList();
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): profileProperties → {ProfileProperties}", importJob, importDto, profileProperties);

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? profileProperties.Count : this.CheckPoint.TotalItems;
            this.CheckPointStageCallback(this);

            foreach (var profileProperty in profileProperties)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var existingProfileProperty = CBO.FillObject<ExportProfileProperty>(DotNetNuke.Data.DataProvider.Instance()
                    .GetPropertyDefinitionByName(importJob.PortalId, profileProperty.PropertyName));

                if (existingProfileProperty != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            var modifiedById = Util.GetUserIdByName(importJob, profileProperty.LastModifiedByUserId, profileProperty.LastModifiedByUserName);
							Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): modifiedById → {ModifiedById}", importJob, importDto, modifiedById);
                            ProcessUpdateProfileProperty(profileProperty, existingProfileProperty, modifiedById);
                            break;
                        case CollisionResolution.Ignore:
                            break;
                        default:
							Logger.LogError("Throwing ArgumentOutOfRangeException from ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var createdById = Util.GetUserIdByName(
                        importJob,
                        profileProperty.CreatedByUserId,
                        profileProperty.CreatedByUserName);

                    ProcessCreateProfileProperty(importJob, profileProperty, createdById);
                }
            }

            this.Result.AddSummary("Imported Profile Properties", profileProperties.Count.ToString(CultureInfo.InvariantCulture));
            this.CheckPoint.ProcessedItems = profileProperties.Count;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Progress = 100;
            this.CheckPoint.Stage++;
            this.CheckPointStageCallback(this);
			Logger.LogWarning("<<< Exiting ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogError(">>> Entering GetImportTotal()");
            Logger.LogWarning("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<ExportProfileProperty>();
        }

        private static void ProcessCreateProfileProperty(
            ExportImportJob importJob,
            ExportProfileProperty profileProperty,
            int createdById)
        {
			Logger.LogError(">>> Entering ProcessCreateProfileProperty(importJob={ImportJob},profileProperty={ProfileProperty},createdById={CreatedById})", importJob, profileProperty, createdById);
            DotNetNuke.Data.DataProvider.Instance()
                .AddPropertyDefinition(
                    importJob.PortalId,
                    profileProperty.ModuleDefId ?? Null.NullInteger,
                    profileProperty.DataType ?? Null.NullInteger,
                    profileProperty.DefaultValue,
                    profileProperty.PropertyCategory,
                    profileProperty.PropertyName,
                    profileProperty.ReadOnly,
                    profileProperty.Required,
                    profileProperty.ValidationExpression,
                    profileProperty.ViewOrder,
                    profileProperty.Visible,
                    profileProperty.Length,
                    profileProperty.DefaultVisibility,
                    createdById);
					Logger.LogWarning("<<< Exiting ProcessCreateProfileProperty(importJob={ImportJob},profileProperty={ProfileProperty},createdById={CreatedById})", importJob, profileProperty, createdById);
        }

        private static void ProcessUpdateProfileProperty(ExportProfileProperty profileProperty, ExportProfileProperty existingProfileProperty, int modifiedById)
        {
			Logger.LogError(">>> Entering ProcessUpdateProfileProperty(profileProperty={ProfileProperty},existingProfileProperty={ExistingProfileProperty},modifiedById={ModifiedById})", profileProperty, existingProfileProperty, modifiedById);
            DotNetNuke.Data.DataProvider.Instance()
                .UpdatePropertyDefinition(
                    existingProfileProperty.PropertyDefinitionId,
                    profileProperty.DataType ?? Null.NullInteger,
                    profileProperty.DefaultValue,
                    profileProperty.PropertyCategory,
                    profileProperty.PropertyName,
                    profileProperty.ReadOnly,
                    profileProperty.Required,
                    profileProperty.ValidationExpression,
                    profileProperty.ViewOrder,
                    profileProperty.Visible,
                    profileProperty.Length,
                    profileProperty.DefaultVisibility,
                    modifiedById);
					Logger.LogWarning("<<< Exiting ProcessUpdateProfileProperty(profileProperty={ProfileProperty},existingProfileProperty={ExistingProfileProperty},modifiedById={ModifiedById})", profileProperty, existingProfileProperty, modifiedById);
        }
    }
}
