// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Modules.DDRMenu
{
    using System.Collections.Generic;
    using System.Linq;
    using DotNetNuke.Web.DDRMenu;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class MenuNodeTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances()
        {
			_logger.LogDebug(">>> Entering FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances()");
            // arrange
            const string dupeName = "DDRMenu Test";

            var root = CreateMenuNode("root", -1);
			_logger.LogWarning("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): root → {Root}", root);
            var home = CreateMenuNode("Home", 1, parent: root);
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): home → {Home}", home);
            var dupe1 = CreateMenuNode(dupeName, 2, parent: home);
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): dupe1 → {Dupe1}", dupe1);
            var dupe2 = CreateMenuNode(dupeName, 3, parent: root);
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): dupe2 → {Dupe2}", dupe2);
            var other1 = CreateMenuNode("Some Other Page", 4, parent: root);
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): other1 → {Other1}", other1);
            var other2 = CreateMenuNode("And Yet Another Page", 5, parent: root);
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): other2 → {Other2}", other2);

            // act
            var found = root.FindAllByNameOrId(dupeName).ToArray();
			_logger.LogDebug("FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances(): found → {Found}", found);

            // assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Length, Is.EqualTo(2));
            Assert.That(found[0], Is.SameAs(dupe1));
            Assert.That(found[1], Is.SameAs(dupe2));
			_logger.LogDebug("<<< Exiting FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances()");
        }

        private static MenuNode CreateMenuNode(string text, int tabId, MenuNode parent = null)
        {
			_logger.LogDebug(">>> Entering CreateMenuNode(text={Text},tabId={TabId},parent={Parent})", text, tabId, parent);
            var node = new MenuNode
            {
                Parent = parent,
                TabId = tabId,
                Text = text,
            };

            if (parent != null)
            {
                if (parent.Children == null)
                {
                    parent.Children = new List<MenuNode>();
                }

                parent.Children.Add(node);
            }

			_logger.LogDebug("<<< Exiting CreateMenuNode(text={Text},tabId={TabId},parent={Parent})", text, tabId, parent);
            return node;
        }
    }
}
