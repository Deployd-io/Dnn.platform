// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class NaiveLockingListTests
    {
        // Most of the tests here are very simple, as the real functionality is provided by List<T>
        // these tests only ensure that the methods are actually wired up
        // Complicated threading tests would be nice, but are not practical as the
        // NaiveLockList releases all locks immediately making it impossible to ensure
        // a lock is held while launching another thread in the test
        [Test]
        public void Add()
        {
			mockEventLogger.LogDebug(">>> Entering Add()");
            var list = new NaiveLockingList<string>();

			mockEventLogger.LogDebug("Add(): list → {List}", list);
            list.Add("sumthin");

            Assert.That(list, Is.EqualTo(new[] { "sumthin" }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting Add()");
        }

        [Test]
        public void Clear()
        {
			mockEventLogger.LogDebug(">>> Entering Clear()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };
            list.Clear();

            Assert.That(list, Is.EqualTo(new int[] { }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting Clear()");
        }

        [Test]
        public void Contains()
        {
			mockEventLogger.LogDebug(">>> Entering Contains()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(list, Has.Some.EqualTo(2));
                Assert.That(list, Has.None.EqualTo(9999));
            }
			mockEventLogger.LogDebug("<<< Exiting Contains()");
        }

        [Test]
        public void CopyTo()
        {
			mockEventLogger.LogDebug(">>> Entering CopyTo()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            var dest = new int[4];
            list.CopyTo(dest, 0);

            Assert.That(dest, Is.EqualTo(list).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting CopyTo()");
        }

        [Test]
        public void Remove()
        {
			mockEventLogger.LogDebug(">>> Entering Remove()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list.Remove(2);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 3 }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting Remove()");
        }

        [Test]
        public void Count()
        {
			mockEventLogger.LogDebug(">>> Entering Count()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            Assert.That(list, Has.Count.EqualTo(4));
			mockEventLogger.LogDebug("<<< Exiting Count()");
        }

        [Test]
        public void IndexOf()
        {
			mockEventLogger.LogDebug(">>> Entering IndexOf()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            Assert.That(list.IndexOf(2), Is.EqualTo(2));
			mockEventLogger.LogDebug("<<< Exiting IndexOf()");
        }

        [Test]
        public void Insert()
        {
			mockEventLogger.LogDebug(">>> Entering Insert()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list.Insert(2, 999);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 999, 2, 3 }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting Insert()");
        }

        [Test]
        public void RemoveAt()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveAt()");
            var list = new NaiveLockingList<int> { 0, 1, 999, 2, 3 };

            list.RemoveAt(2);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2, 3 }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting RemoveAt()");
        }

        [Test]
        public void IndexOperator()
        {
			mockEventLogger.LogDebug(">>> Entering IndexOperator()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list[2] = 999;

            Assert.That(list[2], Is.EqualTo(999));
			mockEventLogger.LogDebug("<<< Exiting IndexOperator()");
        }

        [Test]
        public void NoWritesWhileEnumerating()
        {
			mockEventLogger.LogDebug(">>> Entering NoWritesWhileEnumerating()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            using (list.GetEnumerator())
            {
                Assert.Throws<LockRecursionException>(() => list.Add(4));
            }
			mockEventLogger.LogDebug("<<< Exiting NoWritesWhileEnumerating()");
        }

        [Test]
        public void CanWriteAfterEnumerating()
        {
			mockEventLogger.LogDebug(">>> Entering CanWriteAfterEnumerating()");
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            using (IEnumerator<int> e = list.GetEnumerator())
            {
                e.MoveNext();
                Debug.WriteLine(e.Current);
                e.MoveNext();
            }

            list.Add(4);
			mockEventLogger.LogDebug("<<< Exiting CanWriteAfterEnumerating()");
        }
    }
}
