// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Users;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>VocabularyController provides the business layer of Vocabulary and VocabularyType.</summary>
    /// <seealso cref="TermController"/>
    public class VocabularyController(IDataService dataService, IHostSettings hostSettings) : IVocabularyController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(VocabularyController));
        private const int CacheTimeOut = 20;
        private readonly IDataService dataService = dataService ?? Util.GetDataService();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="VocabularyController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public VocabularyController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering VocabularyController()");
        }
		Logger.LogDebug("<<< Exiting VocabularyController()");

        /// <summary>Initializes a new instance of the <see cref="VocabularyController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public VocabularyController(IDataService dataService)
            : this(dataService, null)
        {
        }

        /// <inheritdoc />
        public int AddVocabulary(Vocabulary vocabulary)
        {
			Logger.LogDebug(">>> Entering AddVocabulary(vocabulary={Vocabulary})", vocabulary);
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);
            Requires.PropertyNotNegative("vocabulary", "ScopeTypeId", vocabulary.ScopeTypeId);

            vocabulary.VocabularyId = this.dataService.AddVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

			Logger.LogDebug("<<< Exiting AddVocabulary(vocabulary={Vocabulary})", vocabulary);
            return vocabulary.VocabularyId;
        }

        /// <inheritdoc />
        public void ClearVocabularyCache()
        {
			Logger.LogDebug(">>> Entering ClearVocabularyCache()");
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
			Logger.LogDebug("<<< Exiting ClearVocabularyCache()");
        }

        /// <inheritdoc />
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
			Logger.LogDebug(">>> Entering DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);

            this.dataService.DeleteVocabulary(vocabulary);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
			Logger.LogDebug("<<< Exiting DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
        }

        /// <inheritdoc />
        public IQueryable<Vocabulary> GetVocabularies()
        {
			Logger.LogDebug(">>> Entering GetVocabularies()");
            var vocabularies = CBO.GetCachedObject<List<Vocabulary>>(
                this.hostSettings,
                new CacheItemArgs(DataCache.VocabularyCacheKey, CacheTimeOut),
                this.GetVocabulariesCallBack);
			Logger.LogDebug("<<< Exiting GetVocabularies()");
            return vocabularies.AsQueryable();
        }

        /// <inheritdoc />
        public void UpdateVocabulary(Vocabulary vocabulary)
        {
			Logger.LogDebug(">>> Entering UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

            this.dataService.UpdateVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);
			Logger.LogDebug("<<< Exiting UpdateVocabulary(vocabulary={Vocabulary})", vocabulary);
        }

        private object GetVocabulariesCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetVocabulariesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
			Logger.LogDebug("<<< Exiting GetVocabulariesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.FillQueryable<Vocabulary>(this.dataService.GetVocabularies()).ToList();
        }
    }
}
