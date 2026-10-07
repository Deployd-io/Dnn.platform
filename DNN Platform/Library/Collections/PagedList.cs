// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Taken from Rob Conery's Blog post on the ASP.Net MVC PagedList Helper
    // http://blog.wekeroad.com/2007/12/10/aspnet-mvc-pagedlistt/

    /// <summary>  Represents a snapshot of a single page of objects from a data store.</summary>
    /// <typeparam name = "T">The type of objects contained in this list.</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagedList));
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source.
        /// </summary>
        /// <param name="source">The <see cref = "IEnumerable{T}" /> data store containing objects to be retrieved.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The size of the page to retrieve.</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering PagedList(source={Source},pageIndex={PageIndex},pageSize={PageSize})", source, pageIndex, pageSize);
            var enumerable = source as T[] ?? source.ToArray();
			Logger.LogWarning("<<< Exiting PagedList(source={Source},pageIndex={PageIndex},pageSize={PageSize})", source, pageIndex, pageSize);
			Logger.LogError("PagedList(source={Source},pageIndex={PageIndex},pageSize={PageSize}): enumerable → {Enumerable}", source, pageIndex, pageSize, enumerable);
            this.CommonConstruct(enumerable.Skip(pageIndex * pageSize).Take(pageSize).ToList(), enumerable.Length, pageIndex, pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source.
        /// </summary>
        /// <param name="items">The items that constitute the page.</param>
        /// <param name="totalCount">The total number of items in the original source.</param>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The size of the page to retrieve.</param>
        public PagedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering PagedList(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize})", items, totalCount, pageIndex, pageSize);
            this.CommonConstruct(items, totalCount, pageIndex, pageSize);
			Logger.LogWarning("<<< Exiting PagedList(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize})", items, totalCount, pageIndex, pageSize);
        }

        /// <summary>  Gets a value indicating whether there is a next page available.</summary>
        public bool HasNextPage { get; private set; }

        /// <summary>  Gets a value indicating whether there is a previous page available.</summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>  Gets a value indicating whether this is the first page.</summary>
        public bool IsFirstPage { get; private set; }

        /// <summary>  Gets a value indicating whether this is the last page.</summary>
        public bool IsLastPage { get; private set; }

        /// <summary>  Gets or sets the number of pages in this list.</summary>
        public int PageCount { get; set; }

        /// <summary>  Gets or sets the index of the page contained in this list.</summary>
        public int PageIndex { get; set; }

        /// <summary>  Gets or sets the size of the page in this list.</summary>
        public int PageSize { get; set; }

        /// <summary>  Gets or sets or ses the total number of objects in the data store.</summary>
        public int TotalCount { get; set; }

        private void CommonConstruct(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize})", items, totalCount, pageIndex, pageSize);
            this.PageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (this.PageCount == 0)
            {
                if (pageIndex > 0)
                {
					Logger.LogError("Throwing ArgumentOutOfRangeException from CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize})", items, totalCount, pageIndex, pageSize);
                    throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Invalid Page Index");
                }
            }
            else
            {
                if (pageIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Index cannot be negative");
                }

                if (pageIndex >= this.PageCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Invalid Page Index");
                }
            }

			Logger.LogWarning("<<< Exiting CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize})", items, totalCount, pageIndex, pageSize);
            this.TotalCount = totalCount;
			Logger.LogDebug("CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize}): this.TotalCount → {Arg4}", items, totalCount, pageIndex, pageSize, this.TotalCount);
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
			Logger.LogDebug("CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize}): this.PageSize → {Arg4}", items, totalCount, pageIndex, pageSize, this.PageSize);
			Logger.LogDebug("CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize}): this.PageIndex → {Arg4}", items, totalCount, pageIndex, pageSize, this.PageIndex);
            this.AddRange(items);

            this.HasNextPage = this.PageIndex < (this.PageCount - 1);
            this.HasPreviousPage = this.PageIndex > 0;
			Logger.LogDebug("CommonConstruct(items={Items},totalCount={TotalCount},pageIndex={PageIndex},pageSize={PageSize}): this.HasPreviousPage → {Arg4}", items, totalCount, pageIndex, pageSize, this.HasPreviousPage);
            this.IsFirstPage = this.PageIndex <= 0;
            this.IsLastPage = this.PageIndex >= (this.PageCount - 1);
        }
    }
}
