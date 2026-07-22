// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PagedListTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(12, 2)]
        public void PagedList_Constructor_Succeeds_When_Given_Valid_Index(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            // Act
            Assert.DoesNotThrow(() => new PagedList<int>(list, pageIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(12, 2)]
        public void PagedList_Constructor_Overload_Succeeds_When_Given_Valid_Index(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Overload_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Overload_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            Assert.DoesNotThrow(() => new PagedList<int>(list, totalCount, pageIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Overload_Succeeds_When_Given_Valid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Throws_When_Given_InValid_Index(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, pageIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Overload_Throws_When_Given_InValid_Index(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Overload_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Overload_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, totalCount, pageIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Overload_Throws_When_Given_InValid_Index(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Throws_When_Given_Invalid_PageSize(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, pageIndex, -1));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Overload_Throws_When_Given_Invalid_PageSize(int totalCount, int pageIndex)
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Overload_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
            // Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

			Logger.LogDebug("PagedList_Constructor_Overload_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex}): list → {List}", totalCount, pageIndex, list);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, totalCount, pageIndex, -1));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Overload_Throws_When_Given_Invalid_PageSize(totalCount={TotalCount},pageIndex={PageIndex})", totalCount, pageIndex);
        }

        [Test]
        public void PagedList_Constructor_Throws_When_Given_Negative_Index()
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Throws_When_Given_Negative_Index()");
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Constructor_Throws_When_Given_Negative_Index(): list → {List}", list);
            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, Constants.PAGE_NegativeIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Throws_When_Given_Negative_Index()");
        }

        [Test]
        public void PagedList_Constructor_Overload_Throws_When_Given_Negative_Index()
        {
			Logger.LogDebug(">>> Entering PagedList_Constructor_Overload_Throws_When_Given_Negative_Index()");
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_Constructor_Overload_Throws_When_Given_Negative_Index(): list → {List}", list);
            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>(list, Constants.PAGE_TotalCount, Constants.PAGE_NegativeIndex, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting PagedList_Constructor_Overload_Throws_When_Given_Negative_Index()");
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(Constants.PAGE_Last, false)]
        public void PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool hasNext)
        {
			Logger.LogDebug(">>> Entering PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasNext={HasNext})", index, hasNext);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasNext={HasNext}): list → {List}", index, hasNext, list);
            // Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasNext={HasNext}): pagedList → {PagedList}", index, hasNext, pagedList);
            // Assert
            Assert.That(pagedList.HasNextPage, Is.EqualTo(hasNext));
			Logger.LogDebug("<<< Exiting PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasNext={HasNext})", index, hasNext);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(Constants.PAGE_Last, true)]
        public void PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool hasPrevious)
        {
			Logger.LogDebug(">>> Entering PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasPrevious={HasPrevious})", index, hasPrevious);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasPrevious={HasPrevious}): list → {List}", index, hasPrevious, list);
            // Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasPrevious={HasPrevious}): pagedList → {PagedList}", index, hasPrevious, pagedList);
            // Assert
            Assert.That(pagedList.HasPreviousPage, Is.EqualTo(hasPrevious));
			Logger.LogDebug("<<< Exiting PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},hasPrevious={HasPrevious})", index, hasPrevious);
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(Constants.PAGE_Last, false)]
        public void PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool isFirst)
        {
			Logger.LogDebug(">>> Entering PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isFirst={IsFirst})", index, isFirst);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isFirst={IsFirst}): list → {List}", index, isFirst, list);
            // Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isFirst={IsFirst}): pagedList → {PagedList}", index, isFirst, pagedList);
            // Assert
            Assert.That(pagedList.IsFirstPage, Is.EqualTo(isFirst));
			Logger.LogDebug("<<< Exiting PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isFirst={IsFirst})", index, isFirst);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(Constants.PAGE_Last, true)]
        public void PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool isLast)
        {
			Logger.LogDebug(">>> Entering PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isLast={IsLast})", index, isLast);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isLast={IsLast}): list → {List}", index, isLast, list);
            // Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isLast={IsLast}): pagedList → {PagedList}", index, isLast, pagedList);
            // Assert
            Assert.That(pagedList.IsLastPage, Is.EqualTo(isLast));
			Logger.LogDebug("<<< Exiting PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(index={Index},isLast={IsLast})", index, isLast);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Constants.PAGE_Last)]
        public void PagedList_Returns_Correct_Page_When_Given_Valid_Index(int index)
        {
			Logger.LogDebug(">>> Entering PagedList_Returns_Correct_Page_When_Given_Valid_Index(index={Index})", index);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Returns_Correct_Page_When_Given_Valid_Index(index={Index}): list → {List}", index, list);
            // Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

			Logger.LogDebug("PagedList_Returns_Correct_Page_When_Given_Valid_Index(index={Index}): pagedList → {PagedList}", index, pagedList);
            // Assert
            Assert.That(pagedList.PageIndex, Is.EqualTo(index));
			Logger.LogDebug("<<< Exiting PagedList_Returns_Correct_Page_When_Given_Valid_Index(index={Index})", index);
        }

        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(int pageSize)
        {
			Logger.LogDebug(">>> Entering PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize})", pageSize);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize}): list → {List}", pageSize, list);
            // Act
            var pagedList = new PagedList<int>(list, Constants.PAGE_First, pageSize);

			Logger.LogDebug("PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize}): pagedList → {PagedList}", pageSize, pagedList);
            // Assert
            Assert.That(pagedList.PageSize, Is.EqualTo(pageSize));
			Logger.LogDebug("<<< Exiting PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize})", pageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(int index, int pageSize)
        {
			Logger.LogDebug(">>> Entering PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize}): list → {List}", index, pageSize, list);
            // Act
            var pagedList = new PagedList<int>(list, index, pageSize);

			Logger.LogDebug("PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);
            // Assert
            for (int i = 0; i < pageSize; i++)
            {
                Assert.That(pagedList[i], Is.EqualTo((index * pageSize) + i));
            }
			Logger.LogDebug("<<< Exiting PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize})", index, pageSize);
        }

        [Test]
        [TestCase(0, 5, 5)]
        [TestCase(0, 6, 4)]
        [TestCase(2, 4, 6)]
        [TestCase(1, 10, 3)]
        public void PagedList_Sets_Correct_PageCount(int index, int pageSize, int pageCount)
        {
			Logger.LogDebug(">>> Entering PagedList_Sets_Correct_PageCount(index={Index},pageSize={PageSize},pageCount={PageCount})", index, pageSize, pageCount);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Sets_Correct_PageCount(index={Index},pageSize={PageSize},pageCount={PageCount}): list → {List}", index, pageSize, pageCount, list);
            // Act
            var pagedList = new PagedList<int>(list, index, pageSize);

			Logger.LogDebug("PagedList_Sets_Correct_PageCount(index={Index},pageSize={PageSize},pageCount={PageCount}): pagedList → {PagedList}", index, pageSize, pageCount, pagedList);
            // Assert
            Assert.That(pagedList.PageCount, Is.EqualTo(pageCount));
			Logger.LogDebug("<<< Exiting PagedList_Sets_Correct_PageCount(index={Index},pageSize={PageSize},pageCount={PageCount})", index, pageSize, pageCount);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagedList_Sets_TotalCount_To_Total_Number_Of_Items(int index, int pageSize)
        {
			Logger.LogDebug(">>> Entering PagedList_Sets_TotalCount_To_Total_Number_Of_Items(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

			Logger.LogDebug("PagedList_Sets_TotalCount_To_Total_Number_Of_Items(index={Index},pageSize={PageSize}): list → {List}", index, pageSize, list);
            // Act
            var pagedList = new PagedList<int>(list, index, pageSize);

			Logger.LogDebug("PagedList_Sets_TotalCount_To_Total_Number_Of_Items(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);
            // Assert
            Assert.That(pagedList.TotalCount, Is.EqualTo(Constants.PAGE_TotalCount));
			Logger.LogDebug("<<< Exiting PagedList_Sets_TotalCount_To_Total_Number_Of_Items(index={Index},pageSize={PageSize})", index, pageSize);
        }
    }
}
