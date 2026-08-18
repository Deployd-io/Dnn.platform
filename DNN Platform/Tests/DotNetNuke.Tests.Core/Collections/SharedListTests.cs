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
        internal abstract LockingStrategy LockingStrategy { get; }

        [Test]
        public void TryAdd()
        {
			mockEventLogger.LogDebug(">>> Entering TryAdd()");
            const string value = "value";

			mockEventLogger.LogDebug("TryAdd(): value → {Value}", value);
            var sharedList = new SharedList<string>(this.LockingStrategy);
			mockEventLogger.LogDebug("TryAdd(): sharedList → {SharedList}", sharedList);

            bool doInsert = false;
			mockEventLogger.LogDebug("TryAdd(): doInsert → {DoInsert}", doInsert);
            using (ISharedCollectionLock l = sharedList.GetReadLock())
            {
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
			mockEventLogger.LogDebug("<<< Exiting TryAdd()");
        }

        [Test]
        [TestCaseSource(nameof(GetWriteMethods))]
        public void WriteRequiresLock(Action<SharedList<string>> writeAction)
        {
			mockEventLogger.LogDebug(">>> Entering WriteRequiresLock(writeAction={WriteAction})", writeAction);
            Assert.Throws<WriteLockRequiredException>(() => writeAction.Invoke(this.InitSharedList("value")));
			mockEventLogger.LogDebug("<<< Exiting WriteRequiresLock(writeAction={WriteAction})", writeAction);
        }

        [Test]
        [TestCaseSource(nameof(GetReadMethods))]
        public void ReadRequiresLock(Action<SharedList<string>> readAction)
        {
			mockEventLogger.LogDebug(">>> Entering ReadRequiresLock(readAction={ReadAction})", readAction);
            Assert.Throws<ReadLockRequiredException>(() => readAction.Invoke(this.InitSharedList("value")));
			mockEventLogger.LogDebug("<<< Exiting ReadRequiresLock(readAction={ReadAction})", readAction);
        }

        [Test]
        public void DisposedReadLockDeniesRead()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedReadLockDeniesRead()");
            var d = new SharedList<string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedReadLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetReadLock();
			mockEventLogger.LogDebug("DisposedReadLockDeniesRead(): l → {L}", l);
            l.Dispose();

            Assert.Throws<ReadLockRequiredException>(() => d.Contains("foo"));
			mockEventLogger.LogDebug("<<< Exiting DisposedReadLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesRead()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedWriteLockDeniesRead()");
            var d = new SharedList<string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedWriteLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
			mockEventLogger.LogDebug("DisposedWriteLockDeniesRead(): l → {L}", l);
            l.Dispose();

            Assert.Throws<ReadLockRequiredException>(() => d.Contains("foo"));
			mockEventLogger.LogDebug("<<< Exiting DisposedWriteLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesWrite()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedWriteLockDeniesWrite()");
            var sharedList = new SharedList<string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedWriteLockDeniesWrite(): sharedList → {SharedList}", sharedList);
            ISharedCollectionLock l = sharedList.GetWriteLock();
			mockEventLogger.LogDebug("DisposedWriteLockDeniesWrite(): l → {L}", l);
            l.Dispose();

            Assert.Throws<WriteLockRequiredException>(() => sharedList[0] = "foo");
			mockEventLogger.LogDebug("<<< Exiting DisposedWriteLockDeniesWrite()");
        }

        [Test]
        public void WriteLockEnablesRead()
        {
			mockEventLogger.LogDebug(">>> Entering WriteLockEnablesRead()");
            var sharedList = this.InitSharedList("value");

			mockEventLogger.LogDebug("WriteLockEnablesRead(): sharedList → {SharedList}", sharedList);
            string actualValue = null;
			mockEventLogger.LogDebug("WriteLockEnablesRead(): actualValue → {ActualValue}", actualValue);
            using (ISharedCollectionLock l = sharedList.GetWriteLock())
            {
                actualValue = sharedList[0];
            }

            Assert.That(actualValue, Is.EqualTo("value"));
			mockEventLogger.LogDebug("<<< Exiting WriteLockEnablesRead()");
        }

        [Test]
        public void CanGetAnotherLockAfterDisposingLock()
        {
			mockEventLogger.LogDebug(">>> Entering CanGetAnotherLockAfterDisposingLock()");
            var d = new SharedList<string>(this.LockingStrategy);
            ISharedCollectionLock l = d.GetReadLock();
			mockEventLogger.LogDebug("CanGetAnotherLockAfterDisposingLock(): l → {L}", l);
			mockEventLogger.LogDebug("CanGetAnotherLockAfterDisposingLock(): d → {D}", d);
            l.Dispose();

            l = d.GetReadLock();
            l.Dispose();
			mockEventLogger.LogDebug("<<< Exiting CanGetAnotherLockAfterDisposingLock()");
        }

        [Test]
        public void DoubleDispose()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleDispose()");
            var d = new SharedList<string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DoubleDispose(): d → {D}", d);
            d.Dispose();
            d.Dispose();
			mockEventLogger.LogDebug("<<< Exiting DoubleDispose()");
        }

        [Test]
        [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
        public void MethodsThrowAfterDisposed(Action<SharedList<string>> methodCall)
        {
			mockEventLogger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            var d = new SharedList<string>(this.LockingStrategy);

			mockEventLogger.LogDebug("MethodsThrowAfterDisposed(methodCall={MethodCall}): d → {D}", methodCall, d);
            d.Dispose();
            Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(d));
			mockEventLogger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        [Test]
        public void TwoDictsShareALockWriteTest()
        {
			mockEventLogger.LogDebug(">>> Entering TwoDictsShareALockWriteTest()");
            ILockStrategy ls = new ReaderWriterLockStrategy();
            var d1 = new SharedList<string>(ls);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): d1 → {D1}", d1);
            var d2 = new SharedList<string>(ls);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): d2 → {D2}", d2);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): ls → {Ls}", ls);

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
			mockEventLogger.LogDebug("<<< Exiting TwoDictsShareALockWriteTest()");
        }

        protected static IEnumerable<Action<SharedList<string>>> GetObjectDisposedExceptionMethods()
        {
			mockEventLogger.LogDebug(">>> Entering GetObjectDisposedExceptionMethods()");
            var list = new List<Action<SharedList<string>>> { (SharedList<string> l) => l.GetReadLock(), (SharedList<string> l) => l.GetWriteLock() };

			mockEventLogger.LogDebug("GetObjectDisposedExceptionMethods(): list → {List}", list);
            list.AddRange(GetReadMethods());
            list.AddRange(GetWriteMethods());

			mockEventLogger.LogDebug("<<< Exiting GetObjectDisposedExceptionMethods()");
            return list;
        }

        protected static IEnumerable<Action<SharedList<string>>> GetReadMethods()
        {
			mockEventLogger.LogDebug(">>> Entering GetReadMethods()");
            var list = new List<Action<SharedList<string>>>();

			mockEventLogger.LogDebug("GetReadMethods(): list → {List}", list);
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

			mockEventLogger.LogDebug("<<< Exiting GetReadMethods()");
            return list;
        }

        protected static IEnumerable<Action<SharedList<string>>> GetWriteMethods()
        {
			mockEventLogger.LogDebug(">>> Entering GetWriteMethods()");
            var list = new List<Action<SharedList<string>>> { l => l.Add("more"), l => l.Clear(), l => l.Remove("value"), l => l.Insert(0, "more"), l => l[0] = "more", l => l.RemoveAt(0) };

			mockEventLogger.LogDebug("GetWriteMethods(): list → {List}", list);
			mockEventLogger.LogDebug("<<< Exiting GetWriteMethods()");
            return list;
        }

        private SharedList<T> InitSharedList<T>(T value)
        {
			mockEventLogger.LogDebug(">>> Entering InitSharedList(value={Value})", value);
            var list = new SharedList<T>(this.LockingStrategy);
            list.BackingList.Add(value);
			mockEventLogger.LogDebug("InitSharedList(value={Value}): list → {List}", value, list);
			mockEventLogger.LogDebug("<<< Exiting InitSharedList(value={Value})", value);
            return list;
        }
    }
}
