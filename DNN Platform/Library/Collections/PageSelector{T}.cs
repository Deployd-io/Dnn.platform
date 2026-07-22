// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections
{
    using System.Collections.Generic;

    /// <summary>  Provides options to allow the consumer to select a page of data from a paged data store.</summary>
    /// <typeparam name = "T">The type of object in the data store.</typeparam>
    public class PageSelector<T>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly int pageSize;
        private readonly IEnumerable<T> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSelector{T}"/> class.
        /// Initializes a new instance of the <see cref="PageSelector{T}"/> for use on the specified data store.
        /// </summary>
        /// <param name="source">The data store to page.</param>
        /// <param name="pageSize">The size of each page.</param>
        public PageSelector(IEnumerable<T> source, int pageSize)
        {
			Logger.LogDebug(">>> Entering PageSelector(source={Source},pageSize={PageSize})", source, pageSize);
            this.source = source;
            this.pageSize = pageSize;
			Logger.LogDebug("<<< Exiting PageSelector(source={Source},pageSize={PageSize})", source, pageSize);
			Logger.LogDebug("PageSelector(source={Source},pageSize={PageSize}): this.source → {Arg2}", source, pageSize, this.source);
			Logger.LogDebug("PageSelector(source={Source},pageSize={PageSize}): pageSize → {PageSize1}", source, pageSize, pageSize);
			Logger.LogDebug("PageSelector(source={Source},pageSize={PageSize}): this.pageSize → {Arg2}", source, pageSize, this.pageSize);
			Logger.LogDebug("PageSelector(source={Source},pageSize={PageSize}): source → {Source1}", source, pageSize, source);
        }

        /// <summary>  Retrieves the specified page as a <see cref = "IPagedList{T}" />.</summary>
        /// <param name="pageIndex">The index (zero-based) of the page to retrieve.</param>
        /// <returns>
        /// An <see cref = "IPagedList{T}" /> containing the page of data, or an
        /// empty list if the page does not exist.
        /// </returns>
        public IPagedList<T> GetPage(int pageIndex)
        {
			Logger.LogDebug(">>> Entering GetPage(pageIndex={PageIndex})", pageIndex);
			Logger.LogDebug("<<< Exiting GetPage(pageIndex={PageIndex})", pageIndex);
            return new PagedList<T>(this.source, pageIndex, this.pageSize);
        }
    }
}
