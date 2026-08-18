// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    /// <summary>Class used as a utility to help manage paging in database queries.</summary>
    public class PagingInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagingInfo));
        /// <summary>Initializes a new instance of the <see cref="PagingInfo"/> class.</summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        public PagingInfo(int pageNumber, int pageSize)
        {
			Logger.LogDebug(">>> Entering PagingInfo(pageNumber={PageNumber},pageSize={PageSize})", pageNumber, pageSize);
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
			Logger.LogDebug("PagingInfo(pageNumber={PageNumber},pageSize={PageSize}): this.PageNumber → {Arg2}", pageNumber, pageSize, this.PageNumber);
			Logger.LogDebug("<<< Exiting PagingInfo(pageNumber={PageNumber},pageSize={PageSize})", pageNumber, pageSize);
        }

        public bool IsLastPage
        {
            get
            {
                if (this.LastRow >= this.TotalRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int PageNumber { get; private set; }

        public int PageSize { get; private set; }

        public int FirstRow { get; private set; }

        public int LastRow { get; private set; }

        public int TotalRows { get; private set; }

        public int TotalPages { get; private set; }

        public void UpdatePageResults(int firstRow, int lastRow, int totalRows, int totalPages)
        {
			Logger.LogDebug(">>> Entering UpdatePageResults(firstRow={FirstRow},lastRow={LastRow},totalRows={TotalRows},totalPages={TotalPages})", firstRow, lastRow, totalRows, totalPages);
            this.FirstRow = firstRow;
            this.LastRow = lastRow;
			Logger.LogDebug("UpdatePageResults(firstRow={FirstRow},lastRow={LastRow},totalRows={TotalRows},totalPages={TotalPages}): this.FirstRow → {Arg4}", firstRow, lastRow, totalRows, totalPages, this.FirstRow);
			Logger.LogDebug("UpdatePageResults(firstRow={FirstRow},lastRow={LastRow},totalRows={TotalRows},totalPages={TotalPages}): this.LastRow → {Arg4}", firstRow, lastRow, totalRows, totalPages, this.LastRow);
            this.TotalRows = totalRows;
            this.TotalPages = totalPages;
			Logger.LogDebug("<<< Exiting UpdatePageResults(firstRow={FirstRow},lastRow={LastRow},totalRows={TotalRows},totalPages={TotalPages})", firstRow, lastRow, totalRows, totalPages);
        }
    }
}
