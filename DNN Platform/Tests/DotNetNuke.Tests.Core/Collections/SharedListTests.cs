// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    public abstract class SharedListTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal abstract LockingStrategy LockingStrategy { get; }

        [Test]
        public void TryAdd()
        {
			Logger.LogDebug(">>> Entering TryAdd()");
            const string value = "value";

			Logger.LogDebug("TryAdd(): value → {Value}", value);
            var sharedList = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("TryAdd(): sharedList → {SharedList}", sharedList);
            bool doInsert = false;
            using (ISharedCollectionLock l = sharedList.GetReadLock())
            {
			Logger.LogDebug("TryAdd(): doInsert → {DoInsert}", doInsert);
                if (!sharedList.Contains(value))
                {
                    doInsert = true;
                }
            }

            if (doInsert)
            {
                using (ISharedCollectionLock l = sharedList.GetWriteLock())
                {
                    if (!sharedList.Contains(value))
                    {
                        sharedList.Add(value);
                    }
                }
            }

            Assert.That(sharedList.BackingList, Is.EqualTo(new List<string> { value }).AsCollection);
			Logger.LogDebug("<<< Exiting TryAdd()");
        }

        [Test]
        [TestCaseSource(nameof(GetWriteMethods))]
        public void WriteRequiresLock(Action<SharedList<string>> writeAction)
        {
			Logger.LogDebug(">>> Entering WriteRequiresLock(writeAction={WriteAction})", writeAction);
            Assert.Throws<WriteLockRequiredException>(() => writeAction.Invoke(this.InitSharedList("value")));
			Logger.LogDebug("<<< Exiting WriteRequiresLock(writeAction={WriteAction})", writeAction);
        }

        [Test]
        [TestCaseSource(nameof(GetReadMethods))]
        public void ReadRequiresLock(Action<SharedList<string>> readAction)
        {
			Logger.LogDebug(">>> Entering ReadRequiresLock(readAction={ReadAction})", readAction);
            Assert.Throws<ReadLockRequiredException>(() => readAction.Invoke(this.InitSharedList("value")));
			Logger.LogDebug("<<< Exiting ReadRequiresLock(readAction={ReadAction})", readAction);
        }

        [Test]
        public void DisposedReadLockDeniesRead()
        {
			Logger.LogDebug(">>> Entering DisposedReadLockDeniesRead()");
            var d = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("DisposedReadLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetReadLock();
            l.Dispose();
			Logger.LogDebug("DisposedReadLockDeniesRead(): l → {L}", l);

            Assert.Throws<ReadLockRequiredException>(() => d.Contains("foo"));
			Logger.LogDebug("<<< Exiting DisposedReadLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesRead()
        {
			Logger.LogDebug(">>> Entering DisposedWriteLockDeniesRead()");
            var d = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("DisposedWriteLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
            l.Dispose();
			Logger.LogDebug("DisposedWriteLockDeniesRead(): l → {L}", l);

            Assert.Throws<ReadLockRequiredException>(() => d.Contains("foo"));
			Logger.LogDebug("<<< Exiting DisposedWriteLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesWrite()
        {
			Logger.LogDebug(">>> Entering DisposedWriteLockDeniesWrite()");
            var sharedList = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("DisposedWriteLockDeniesWrite(): sharedList → {SharedList}", sharedList);
            ISharedCollectionLock l = sharedList.GetWriteLock();
            l.Dispose();
			Logger.LogDebug("DisposedWriteLockDeniesWrite(): l → {L}", l);

            Assert.Throws<WriteLockRequiredException>(() => sharedList[0] = "foo");
			Logger.LogDebug("<<< Exiting DisposedWriteLockDeniesWrite()");
        }

        [Test]
        public void WriteLockEnablesRead()
        {
			Logger.LogDebug(">>> Entering WriteLockEnablesRead()");
            var sharedList = this.InitSharedList("value");

			Logger.LogDebug("WriteLockEnablesRead(): sharedList → {SharedList}", sharedList);
            string actualValue = null;
            using (ISharedCollectionLock l = sharedList.GetWriteLock())
            {
			Logger.LogDebug("WriteLockEnablesRead(): actualValue → {ActualValue}", actualValue);
                actualValue = sharedList[0];
            }

            Assert.That(actualValue, Is.EqualTo("value"));
			Logger.LogDebug("<<< Exiting WriteLockEnablesRead()");
        }

        [Test]
        public void CanGetAnotherLockAfterDisposingLock()
        {
			Logger.LogDebug(">>> Entering CanGetAnotherLockAfterDisposingLock()");
            var d = new SharedList<string>(this.LockingStrategy);
            ISharedCollectionLock l = d.GetReadLock();
			Logger.LogDebug("CanGetAnotherLockAfterDisposingLock(): d → {D}", d);
			Logger.LogDebug("CanGetAnotherLockAfterDisposingLock(): l → {L}", l);
            l.Dispose();

            l = d.GetReadLock();
            l.Dispose();
			Logger.LogDebug("<<< Exiting CanGetAnotherLockAfterDisposingLock()");
        }

        [Test]
        public void DoubleDispose()
        {
			Logger.LogDebug(">>> Entering DoubleDispose()");
            var d = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("DoubleDispose(): d → {D}", d);
            d.Dispose();
            d.Dispose();
			Logger.LogDebug("<<< Exiting DoubleDispose()");
        }

        [Test]
        [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
        public void MethodsThrowAfterDisposed(Action<SharedList<string>> methodCall)
        {
			Logger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            var d = new SharedList<string>(this.LockingStrategy);

			Logger.LogDebug("MethodsThrowAfterDisposed(methodCall={MethodCall}): d → {D}", methodCall, d);
            d.Dispose();
            Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(d));
			Logger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        [Test]
        public void TwoDictsShareALockWriteTest()
        {
			Logger.LogDebug(">>> Entering TwoDictsShareALockWriteTest()");
            ILockStrategy ls = new ReaderWriterLockStrategy();
            var d1 = new SharedList<string>(ls);
			Logger.LogDebug("TwoDictsShareALockWriteTest(): ls → {Ls}", ls);
			Logger.LogDebug("TwoDictsShareALockWriteTest(): d1 → {D1}", d1);
            var d2 = new SharedList<string>(ls);

			Logger.LogDebug("TwoDictsShareALockWriteTest(): d2 → {D2}", d2);
            using (ISharedCollectionLock readLock = d1.GetReadLock())
            {
                Assert.Throws<LockRecursionException>(
                    () =>
                    {
                        using (ISharedCollectionLock writeLock = d2.GetWriteLock())
                        {
                            // do nothing
                        }
                    });
            }
			Logger.LogDebug("<<< Exiting TwoDictsShareALockWriteTest()");
        }

        protected static IEnumerable<Action<SharedList<string>>> GetObjectDisposedExceptionMethods()
        {
			Logger.LogDebug(">>> Entering GetObjectDisposedExceptionMethods()");
            var list = new List<Action<SharedList<string>>> { (SharedList<string> l) => l.GetReadLock(), (SharedList<string> l) => l.GetWriteLock() };

			Logger.LogDebug("GetObjectDisposedExceptionMethods(): list → {List}", list);
            list.AddRange(GetReadMethods());
            list.AddRange(GetWriteMethods());

			Logger.LogDebug("<<< Exiting GetObjectDisposedExceptionMethods()");
            return list;
        }

        protected static IEnumerable<Action<SharedList<string>>> GetReadMethods()
        {
			Logger.LogDebug(">>> Entering GetReadMethods()");
            var list = new List<Action<SharedList<string>>>();

			Logger.LogDebug("GetReadMethods(): list → {List}", list);
            list.Add(l => l.Contains("value"));
            list.Add(l => Console.WriteLine(l.Count));
            list.Add(l => Console.WriteLine(l.IsReadOnly));
            list.Add(l => l.GetEnumerator());
            list.Add(l => l.IndexOf("value"));
            list.Add(l => Console.WriteLine(l[0]));

            list.Add(l =>
                         {
                             var arr = new string[2];
                             l.CopyTo(arr, 0);
                         });

			Logger.LogDebug("<<< Exiting GetReadMethods()");
            return list;
        }

        protected static IEnumerable<Action<SharedList<string>>> GetWriteMethods()
        {
			Logger.LogDebug(">>> Entering GetWriteMethods()");
            var list = new List<Action<SharedList<string>>> { l => l.Add("more"), l => l.Clear(), l => l.Remove("value"), l => l.Insert(0, "more"), l => l[0] = "more", l => l.RemoveAt(0) };

			Logger.LogDebug("GetWriteMethods(): list → {List}", list);
			Logger.LogDebug("<<< Exiting GetWriteMethods()");
            return list;
        }

        private SharedList<T> InitSharedList<T>(T value)
        {
			Logger.LogDebug(">>> Entering InitSharedList(value={Value})", value);
            var list = new SharedList<T>(this.LockingStrategy);
            list.BackingList.Add(value);
			Logger.LogDebug("InitSharedList(value={Value}): list → {List}", value, list);
            return list;
        }
    }
}
