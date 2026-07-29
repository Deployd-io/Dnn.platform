// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Taxonomy;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;

    using Util = DotNetNuke.Entities.Content.Common.Util;

    /// <summary>An export service for vocabularies.</summary>
    public class VocabularyService : BasePortableService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override string Category => Constants.Category_Vocabularies;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 5;

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogDebug(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): fromDate → {FromDate}", exportJob, exportDto, fromDate);
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): toDate → {ToDate}", exportJob, exportDto, toDate);
            List<TaxonomyVocabularyType> vocabularyTypes = null;

			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): vocabularyTypes → {VocabularyTypes}", exportJob, exportDto, vocabularyTypes);
            if (this.CheckPoint.Stage == 0)
            {
                var taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                var taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
				Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): taxonomyTerms → {TaxonomyTerms}", exportJob, exportDto, taxonomyTerms);
				Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): taxonomyVocabularies → {TaxonomyVocabularies}", exportJob, exportDto, taxonomyVocabularies);
                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    var scopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());

					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): scopeTypes → {ScopeTypes}", exportJob, exportDto, scopeTypes);
                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? scopeTypes.Count : this.CheckPoint.TotalItems;
                    if (this.CheckPoint.TotalItems == scopeTypes.Count)
                    {
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                        taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                        taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                        this.CheckPoint.TotalItems += taxonomyTerms.Count + taxonomyVocabularies.Count;
                    }

                    this.CheckPointStageCallback(this);

                    this.Repository.CreateItems(scopeTypes);
					Logger.LogInformation("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);

                    ////Result.AddSummary("Exported Taxonomy Scopes", scopeTypes.Count.ToString()); -- not imported so don't show
                    ////CheckPoint.ProcessedItems += scopeTypes.Count;
                }

                this.CheckPoint.Progress = 25;

                if (taxonomyVocabularies == null)
                {
                    taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                }

                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    if (vocabularyTypes == null)
                    {
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                    }

                    this.Repository.CreateItems(vocabularyTypes);

                    ////Result.AddSummary("Exported Vocabulary Types", vocabularyTypes.Count.ToString()); -- not imported so don't show
                    ////CheckPoint.ProcessedItems += vocabularyTypes.Count;
                }

                this.Repository.CreateItems(taxonomyTerms);
                this.Result.AddSummary("Exported Vocabularies", taxonomyTerms.Count.ToString(CultureInfo.InvariantCulture));
                this.CheckPoint.Progress = 75;
                this.CheckPoint.ProcessedItems += taxonomyTerms.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (taxonomyVocabularies == null)
                {
                    taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                }

                this.Repository.CreateItems(taxonomyVocabularies);
                this.Result.AddSummary("Exported Terms", taxonomyVocabularies.Count.ToString(CultureInfo.InvariantCulture));
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += taxonomyVocabularies.Count;
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
            }
			Logger.LogDebug("<<< Exiting ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
        }

        /// <inheritdoc />
        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
			Logger.LogDebug(">>> Entering ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? this.GetImportTotal() : this.CheckPoint.TotalItems;
            if (this.CheckPoint.Stage == 0)
            {
                var otherScopeTypes = this.Repository.GetAllItems<TaxonomyScopeType>().ToList();

				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): otherScopeTypes → {OtherScopeTypes}", importJob, importDto, otherScopeTypes);
                // the table Taxonomy_ScopeTypes is used for lookup only and never changed/updated in the database
                ////CheckPoint.Progress = 10;

                // var otherVocabularyTypes = Repository.GetAllItems<TaxonomyVocabularyType>().ToList();
                // the table Taxonomy_VocabularyTypes is used for lookup only and never changed/updated in the database
                this.CheckPoint.Progress = 20;

                var otherVocabularies = this.Repository.GetAllItems<TaxonomyVocabulary>().ToList();
                this.ProcessVocabularies(importJob, importDto, otherScopeTypes, otherVocabularies);
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): otherVocabularies → {OtherVocabularies}", importJob, importDto, otherVocabularies);
				Logger.LogInformation("ImportData(importJob,importDto)={ImportJob},{ImportDto}: update query executed in {Arg2} ms", importJob, importDto, __logStart.ElapsedMilliseconds);
                this.Repository.UpdateItems(otherVocabularies);
                this.Result.AddSummary("Imported Vocabularies", otherVocabularies.Count.ToString(CultureInfo.InvariantCulture));
                this.CheckPoint.Progress = 60;
                this.CheckPoint.ProcessedItems += otherVocabularies.Count;

                var otherTaxonomyTerms = this.Repository.GetAllItems<TaxonomyTerm>().ToList();
                this.ProcessTaxonomyTerms(importJob, importDto, otherVocabularies, otherTaxonomyTerms);
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): otherTaxonomyTerms → {OtherTaxonomyTerms}", importJob, importDto, otherTaxonomyTerms);
                this.Repository.UpdateItems(otherTaxonomyTerms);
                this.Result.AddSummary("Imported Terms", otherTaxonomyTerms.Count.ToString(CultureInfo.InvariantCulture));
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += otherTaxonomyTerms.Count;
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
            }
			Logger.LogDebug("<<< Exiting ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogDebug(">>> Entering GetImportTotal()");
			Logger.LogDebug("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<TaxonomyVocabulary>() + this.Repository.GetCount<TaxonomyTerm>();
        }

        private static List<TaxonomyTerm> GetTaxonomyTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms(portalId, toDate, fromDate));
        }

        private static List<TaxonomyVocabulary> GetTaxonomyVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies(portalId, toDate, fromDate));
        }

        private void ProcessVocabularies(ExportImportJob importJob, ImportDto importDto, IList<TaxonomyScopeType> otherScopeTypes, IEnumerable<TaxonomyVocabulary> otherVocabularies)
        {
            var changed = false;
            var dataService = Util.GetDataService();
            var localVocabularies = GetTaxonomyVocabularies(importDto.PortalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null);
            foreach (var other in otherVocabularies)
            {
                var createdBy = Common.Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localVocabularies.FirstOrDefault(t => t.Name == other.Name);
                var scope = otherScopeTypes.FirstOrDefault(s => s.ScopeTypeID == other.ScopeTypeID);

                var scopeId = other.ScopeID ?? Null.NullInteger;
                if (scope != null && scope.ScopeType.Equals("Application", StringComparison.OrdinalIgnoreCase))
                {
                    scopeId = Null.NullInteger;
                }
                else if (scope != null && scope.ScopeType.Equals("Portal", StringComparison.OrdinalIgnoreCase))
                {
                    scopeId = importDto.PortalId;
                }

                if (local != null)
                {
                    other.LocalId = local.VocabularyID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored vocabulary", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var vocabulary = new Vocabulary(other.Name, other.Description)
                            {
                                IsSystem = other.IsSystem,
                                Weight = other.Weight,
                                ScopeId = scopeId,
                                ScopeTypeId = scope?.LocalId ?? other.ScopeTypeID,
                            };
                            dataService.UpdateVocabulary(vocabulary, modifiedBy);
                            this.Result.AddLogEntry("Updated vocabulary", other.Name);
                            changed = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var vocabulary = new Vocabulary(other.Name, other.Description, (VocabularyType)other.VocabularyTypeID)
                    {
                        IsSystem = other.IsSystem,
                        Weight = other.Weight,
                        ScopeId = scopeId,
                        ScopeTypeId = scope?.LocalId ?? other.ScopeTypeID,
                    };
                    other.LocalId = dataService.AddVocabulary(vocabulary, createdBy);
                    this.Result.AddLogEntry("Added vocabulary", other.Name);
                    changed = true;
                }
            }

            if (changed)
            {
                DataCache.ClearCache(DataCache.VocabularyCacheKey);
            }
        }

        private void ProcessTaxonomyTerms(ExportImportJob importJob, ImportDto importDto, IList<TaxonomyVocabulary> otherVocabularies, IList<TaxonomyTerm> otherTaxonomyTerms)
        {
            var dataService = Util.GetDataService();

            ////var vocabularyController = new VocabularyController();
            var localTaxonomyTerms = GetTaxonomyTerms(importDto.PortalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null);
            foreach (var other in otherTaxonomyTerms)
            {
                var createdBy = Common.Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);

                var vocabulary = otherVocabularies.FirstOrDefault(v => v.VocabularyID == other.VocabularyID);
                var vocabularyId = vocabulary?.LocalId ?? 0;
                var isHierarchical = vocabulary != null && vocabulary.VocabularyTypeID == (int)VocabularyType.Hierarchy;
                var local = localTaxonomyTerms.FirstOrDefault(t => t.Name == other.Name && t.VocabularyID == vocabularyId);

                if (local != null)
                {
                    other.LocalId = local.TermID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored taxonomy", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var parent = other.ParentTermID.HasValue
                                ? otherTaxonomyTerms.FirstOrDefault(v => v.TermID == other.ParentTermID.Value)
                                : null;
                            var term = new Term(other.Name, other.Description, vocabularyId)
                            {
                                TermId = local.TermID,
                                ParentTermId = parent?.LocalId,
                                Weight = other.Weight,
                            };

                            if (isHierarchical)
                            {
                                dataService.UpdateHeirarchicalTerm(term, modifiedBy);
                            }
                            else
                            {
                                dataService.UpdateSimpleTerm(term, modifiedBy);
                            }

                            DataCache.ClearCache(string.Format(CultureInfo.InvariantCulture, DataCache.TermCacheKey, term.TermId));
                            this.Result.AddLogEntry("Updated taxonomy", other.Name);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var parent = other.ParentTermID.HasValue
                        ? otherTaxonomyTerms.FirstOrDefault(v => v.TermID == other.ParentTermID.Value)
                        : null;
                    var term = new Term(other.Name, other.Description, vocabularyId)
                    {
                        ParentTermId = parent?.LocalId,
                        Weight = other.Weight,
                    };

                    other.LocalId = isHierarchical
                        ? dataService.AddHeirarchicalTerm(term, createdBy)
                        : dataService.AddSimpleTerm(term, createdBy);
                    this.Result.AddLogEntry("Added taxonomy", other.Name);
                }
            }
        }
    }
}
