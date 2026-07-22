// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PageSelectorTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IEnumerable<int> list;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.list = Util.CreateIntegerList(Constants.PAGE_TotalCount);
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): list → {List}", list);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Constants.PAGE_Last)]
        public void PageSelector_Returns_CorrectPage_When_Given_Valid_Index(int index)
        {
			Logger.LogDebug(">>> Entering PageSelector_Returns_CorrectPage_When_Given_Valid_Index(index={Index})", index);
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

			Logger.LogDebug("PageSelector_Returns_CorrectPage_When_Given_Valid_Index(index={Index}): selector → {Selector}", index, selector);
            // Act
            IPagedList<int> pagedList = selector.GetPage(index);

			Logger.LogDebug("PageSelector_Returns_CorrectPage_When_Given_Valid_Index(index={Index}): pagedList → {PagedList}", index, pagedList);
            // Assert
            Assert.That(pagedList.PageIndex, Is.EqualTo(index));
			Logger.LogDebug("<<< Exiting PageSelector_Returns_CorrectPage_When_Given_Valid_Index(index={Index})", index);
        }

        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(int pageSize)
        {
			Logger.LogDebug(">>> Entering PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize})", pageSize);
            // Arrange
            var selector = new PageSelector<int>(this.list, pageSize);

			Logger.LogDebug("PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize}): selector → {Selector}", pageSize, selector);
            // Act
            IPagedList<int> pagedList = selector.GetPage(Constants.PAGE_First);

			Logger.LogDebug("PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize}): pagedList → {PagedList}", pageSize, pagedList);
            // Assert
            Assert.That(pagedList.PageSize, Is.EqualTo(pageSize));
			Logger.LogDebug("<<< Exiting PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(pageSize={PageSize})", pageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(int index, int pageSize)
        {
			Logger.LogDebug(">>> Entering PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize})", index, pageSize);
            // Arrange
            var selector = new PageSelector<int>(this.list, pageSize);

			Logger.LogDebug("PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize}): selector → {Selector}", index, pageSize, selector);
            // Act
            IPagedList<int> pagedList = selector.GetPage(index);

			Logger.LogDebug("PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize}): pagedList → {PagedList}", index, pageSize, pagedList);
            // Assert
            for (int i = 0; i < pageSize; i++)
            {
                Assert.That(pagedList[i], Is.EqualTo((index * pageSize) + i));
            }
			Logger.LogDebug("<<< Exiting PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(index={Index},pageSize={PageSize})", index, pageSize);
        }

        [Test]
        public void PageSelector_Throws_When_Given_InValid_Index()
        {
			Logger.LogDebug(">>> Entering PageSelector_Throws_When_Given_InValid_Index()");
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

			Logger.LogDebug("PageSelector_Throws_When_Given_InValid_Index(): selector → {Selector}", selector);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selector.GetPage(Constants.PAGE_OutOfRange));
			Logger.LogDebug("<<< Exiting PageSelector_Throws_When_Given_InValid_Index()");
        }

        [Test]
        public void PageSelector_Throws_When_Given_Negative_Index()
        {
			Logger.LogDebug(">>> Entering PageSelector_Throws_When_Given_Negative_Index()");
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

			Logger.LogDebug("PageSelector_Throws_When_Given_Negative_Index(): selector → {Selector}", selector);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selector.GetPage(Constants.PAGE_NegativeIndex));
			Logger.LogDebug("<<< Exiting PageSelector_Throws_When_Given_Negative_Index()");
        }
    }
}
