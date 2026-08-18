// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PagingExtensionsTests
    {
        [Test]
        public void PagingExtensions_InPagesOf_Returns_PageSelector()
        {
			mockEventLogger.LogDebug(">>> Entering PagingExtensions_InPagesOf_Returns_PageSelector()");
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();
			mockEventLogger.LogDebug("PagingExtensions_InPagesOf_Returns_PageSelector(): queryable → {Queryable}", queryable);

            // Act
            PageSelector<int> pageSelector = queryable.InPagesOf(Constants.PAGE_RecordCount);
			mockEventLogger.LogDebug("PagingExtensions_InPagesOf_Returns_PageSelector(): pageSelector → {PageSelector}", pageSelector);

            // Assert
            Assert.That(pageSelector, Is.InstanceOf<PageSelector<int>>());
			mockEventLogger.LogDebug("<<< Exiting PagingExtensions_InPagesOf_Returns_PageSelector()");
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(int index, int pageSize)
        {
			mockEventLogger.LogDebug(">>> Entering PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(index={Index},pageSize={PageSize}): enumerable → {Enumerable}", index, pageSize, enumerable);

            // Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);

            // Assert
            Assert.That(pagedList, Is.InstanceOf<IPagedList<int>>());
			mockEventLogger.LogDebug("<<< Exiting PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(index={Index},pageSize={PageSize})", index, pageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(
            int index, int pageSize)
        {
			mockEventLogger.LogDebug(">>> Entering PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize}): enumerable → {Enumerable}", index, pageSize, enumerable);

            // Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(pagedList.PageIndex, Is.EqualTo(index));
                Assert.That(pagedList.PageSize, Is.EqualTo(pageSize));
            }
			mockEventLogger.LogDebug("<<< Exiting PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize})", index, pageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(int index, int pageSize)
        {
			mockEventLogger.LogDebug(">>> Entering PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(index={Index},pageSize={PageSize}): queryable → {Queryable}", index, pageSize, queryable);

            // Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);

            // Assert
            Assert.That(pagedList, Is.InstanceOf<IPagedList<int>>());
			mockEventLogger.LogDebug("<<< Exiting PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(index={Index},pageSize={PageSize})", index, pageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(
            int index, int pageSize)
        {
			mockEventLogger.LogDebug(">>> Entering PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize}): queryable → {Queryable}", index, pageSize, queryable);

            // Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);
			mockEventLogger.LogDebug("PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(pagedList.PageIndex, Is.EqualTo(index));
                Assert.That(pagedList.PageSize, Is.EqualTo(pageSize));
            }
			mockEventLogger.LogDebug("<<< Exiting PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(index={Index},pageSize={PageSize})", index, pageSize);
        }
    }
}
