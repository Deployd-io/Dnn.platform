// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ContentTypeController provides the business layer of ContentType.</summary>
    /// <example>
    /// <code lang="C#">
    /// IContentTypeController typeController = new ContentTypeController();
    /// ContentType contentType = (from t in typeController.GetContentTypes()
    ///                            where t.ContentType == "DesktopModule"
    ///                            select t).SingleOrDefault();
    /// if(contentType == null)
    /// {
    ///     contentType = new ContentType {ContentType = "DesktopModule"};
    ///     contentType.ContentTypeId = typeController.AddContentType(contentType);
    /// }
    /// </code>
    /// </example>
    public class ContentTypeController(IDataService dataService, IHostSettings hostSettings) : IContentTypeController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentTypeController));
        private readonly IDataService dataService = dataService ?? Util.GetDataService();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ContentTypeController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentTypeController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ContentTypeController()");
        }

        /// <summary>Initializes a new instance of the <see cref="ContentTypeController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentTypeController(IDataService dataService)
            : this(dataService, null)
        {
        }

        /// <summary>Adds the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public int AddContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering AddContentType(contentType={ContentType})", contentType);
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNullOrEmpty("contentType", "ContentType", contentType.ContentType);

            contentType.ContentTypeId = this.dataService.AddContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();

			Logger.LogDebug("<<< Exiting AddContentType(contentType={ContentType})", contentType);
            return contentType.ContentTypeId;
        }

        /// <summary>Clears the content type cache.</summary>
        public void ClearContentTypeCache()
        {
			Logger.LogDebug(">>> Entering ClearContentTypeCache()");
            DataCache.RemoveCache(DataCache.ContentTypesCacheKey);
			Logger.LogDebug("<<< Exiting ClearContentTypeCache()");
        }

        /// <summary>Deletes the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        public void DeleteContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering DeleteContentType(contentType={ContentType})", contentType);
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative("contentType", "ContentTypeId", contentType.ContentTypeId);

            this.dataService.DeleteContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();
			Logger.LogDebug("<<< Exiting DeleteContentType(contentType={ContentType})", contentType);
        }

        /// <summary>Gets the content types.</summary>
        /// <returns>content type collection.</returns>
        public IQueryable<ContentType> GetContentTypes()
        {
			Logger.LogDebug(">>> Entering GetContentTypes()");
            var contentTypes = CBO.GetCachedObject<List<ContentType>>(
                this.hostSettings,
                new CacheItemArgs(DataCache.ContentTypesCacheKey, DataCache.ContentTypesCacheTimeOut, DataCache.ContentTypesCachePriority),
                _ => CBO.FillQueryable<ContentType>(this.dataService.GetContentTypes()).ToList());
			Logger.LogDebug("<<< Exiting GetContentTypes()");
            return contentTypes.AsQueryable();
        }

        /// <summary>Updates the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public void UpdateContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering UpdateContentType(contentType={ContentType})", contentType);
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative("contentType", "ContentTypeId", contentType.ContentTypeId);
            Requires.PropertyNotNullOrEmpty("contentType", "ContentType", contentType.ContentType);

            this.dataService.UpdateContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();
			Logger.LogDebug("<<< Exiting UpdateContentType(contentType={ContentType})", contentType);
        }
		Logger.LogDebug("<<< Exiting ContentTypeController()");
    }
}
