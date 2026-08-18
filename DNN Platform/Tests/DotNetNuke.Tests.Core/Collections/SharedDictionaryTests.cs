// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    public abstract class SharedDictionaryTests
    {
        public abstract LockingStrategy LockingStrategy { get; }

        [Test]
        public void TryAdd()
        {
			mockEventLogger.LogDebug(">>> Entering TryAdd()");
            const string KEY = "key";
            const string VALUE = "value";

            var sharedDictionary = new SharedDictionary<string, string>(this.LockingStrategy);
			mockEventLogger.LogDebug("TryAdd(): sharedDictionary → {SharedDictionary}", sharedDictionary);

            bool doInsert = false;
			mockEventLogger.LogDebug("TryAdd(): doInsert → {DoInsert}", doInsert);
            using (ISharedCollectionLock l = sharedDictionary.GetReadLock())
            {
                if (!sharedDictionary.ContainsKey(KEY))
                {
                    doInsert = true;
                }
            }

            if (doInsert)
            {
                using (ISharedCollectionLock l = sharedDictionary.GetWriteLock())
                {
                    if (!sharedDictionary.ContainsKey(KEY))
                    {
                        sharedDictionary.Add(KEY, VALUE);
                    }
                }
            }

            Assert.That(sharedDictionary.BackingDictionary, Is.EqualTo(new Dictionary<string, string> { { KEY, VALUE } }).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting TryAdd()");
        }

        [Test]
        [TestCaseSource(nameof(GetWriteMethods))]
        public void WriteRequiresLock(Action<SharedDictionary<string, string>> writeAction)
        {
			mockEventLogger.LogDebug(">>> Entering WriteRequiresLock(writeAction={WriteAction})", writeAction);
            Assert.Throws<WriteLockRequiredException>(() => writeAction.Invoke(this.InitSharedDictionary("key", "value")));
			mockEventLogger.LogDebug("<<< Exiting WriteRequiresLock(writeAction={WriteAction})", writeAction);
        }

        [Test]
        [TestCaseSource(nameof(GetReadMethods))]
        public void ReadRequiresLock(Action<SharedDictionary<string, string>> readAction)
        {
			mockEventLogger.LogDebug(">>> Entering ReadRequiresLock(readAction={ReadAction})", readAction);
            Assert.Throws<ReadLockRequiredException>(() => readAction.Invoke(this.InitSharedDictionary("key", "value")));
			mockEventLogger.LogDebug("<<< Exiting ReadRequiresLock(readAction={ReadAction})", readAction);
        }

        [Test]
        public void DisposedReadLockDeniesRead()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedReadLockDeniesRead()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedReadLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetReadLock();
			mockEventLogger.LogDebug("DisposedReadLockDeniesRead(): l → {L}", l);
            l.Dispose();

            Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
			mockEventLogger.LogDebug("<<< Exiting DisposedReadLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesRead()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedWriteLockDeniesRead()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedWriteLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
			mockEventLogger.LogDebug("DisposedWriteLockDeniesRead(): l → {L}", l);
            l.Dispose();

            Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
			mockEventLogger.LogDebug("<<< Exiting DisposedWriteLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesWrite()
        {
			mockEventLogger.LogDebug(">>> Entering DisposedWriteLockDeniesWrite()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DisposedWriteLockDeniesWrite(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
			mockEventLogger.LogDebug("DisposedWriteLockDeniesWrite(): l → {L}", l);
            l.Dispose();

            Assert.Throws<WriteLockRequiredException>(() => d["ke"] = "foo");
			mockEventLogger.LogDebug("<<< Exiting DisposedWriteLockDeniesWrite()");
        }

        [Test]
        public void WriteLockEnablesRead()
        {
			mockEventLogger.LogDebug(">>> Entering WriteLockEnablesRead()");
            var d = this.InitSharedDictionary("key", "value");

			mockEventLogger.LogDebug("WriteLockEnablesRead(): d → {D}", d);
            string actualValue = null;
			mockEventLogger.LogDebug("WriteLockEnablesRead(): actualValue → {ActualValue}", actualValue);
            using (ISharedCollectionLock l = d.GetWriteLock())
            {
                actualValue = d["key"];
            }

            Assert.That(actualValue, Is.EqualTo("value"));
			mockEventLogger.LogDebug("<<< Exiting WriteLockEnablesRead()");
        }

        [Test]
        public void CanGetAnotherLockAfterDisposingLock()
        {
			mockEventLogger.LogDebug(">>> Entering CanGetAnotherLockAfterDisposingLock()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);
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
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			mockEventLogger.LogDebug("DoubleDispose(): d → {D}", d);
            d.Dispose();
            d.Dispose();
			mockEventLogger.LogDebug("<<< Exiting DoubleDispose()");
        }

        [Test]
        [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
        public void MethodsThrowAfterDisposed(Action<SharedDictionary<string, string>> methodCall)
        {
			mockEventLogger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			mockEventLogger.LogDebug("MethodsThrowAfterDisposed(methodCall={MethodCall}): d → {D}", methodCall, d);
            d.Dispose();
            Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(d));
			mockEventLogger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        [Test]
        public void TwoDictsShareALockWriteTest()
        {
			mockEventLogger.LogDebug(">>> Entering TwoDictsShareALockWriteTest()");
            var lockStrategy = LockingStrategyFactory.Create(this.LockingStrategy);
            var dict1 = new SharedDictionary<string, string>(lockStrategy);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): dict1 → {Dict1}", dict1);
            var dict2 = new SharedDictionary<string, string>(lockStrategy);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): dict2 → {Dict2}", dict2);
			mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): lockStrategy → {LockStrategy}", lockStrategy);

            using (dict1.GetReadLock())
            {
                ISharedCollectionLock writeLock = null;
				mockEventLogger.LogDebug("TwoDictsShareALockWriteTest(): writeLock → {WriteLock}", writeLock);
                try
                {
                    writeLock = dict2.GetWriteLock();
                }
                catch (LockRecursionException)
                {
					mockEventLogger.LogError("Exception in TwoDictsShareALockWriteTest(): {Arg0}", LockRecursionException.getMessage());
                    Assert.Pass();
                }
                catch (Exception exception)
                {
					mockEventLogger.LogError("Exception in TwoDictsShareALockWriteTest(): {Arg0}", exception.Message);
                    Assert.Fail($"Expected LockRecursionException, got {exception}");
                }
                finally
                {
                    writeLock?.Dispose();
                }
            }

            Assert.Fail("Expected LockRecursionException, did not throw");
			mockEventLogger.LogDebug("<<< Exiting TwoDictsShareALockWriteTest()");
        }

        protected static IEnumerable<Action<SharedDictionary<string, string>>> GetObjectDisposedExceptionMethods()
        {
            var l = new List<Action<SharedDictionary<string, string>>> { (SharedDictionary<string, string> d) => d.GetReadLock(), (SharedDictionary<string, string> d) => d.GetWriteLock() };

            l.AddRange(GetReadMethods());
            l.AddRange(GetWriteMethods());

            return l;
        }

        protected static IEnumerable<Action<SharedDictionary<string, string>>> GetReadMethods()
        {
            var l = new List<Action<SharedDictionary<string, string>>>();

            l.Add(d => d.ContainsKey("key"));
            l.Add(d => d.Contains(new KeyValuePair<string, string>("key", "value")));
            l.Add(d => Console.WriteLine(d.Count));
            l.Add(d => d.GetEnumerator());
            l.Add(d => ((IEnumerable)d).GetEnumerator());
            l.Add(d => Console.WriteLine(d.IsReadOnly));
            l.Add(d => Console.WriteLine(d["key"]));
            l.Add(d => Console.WriteLine(d.Keys));
            l.Add(d => Console.WriteLine(d.Values));

            l.Add(d =>
                      {
                          var arr = new KeyValuePair<string, string>[1];
                          d.CopyTo(arr, 0);
                      });

            l.Add((SharedDictionary<string, string> d) =>
                      {
                          string value = string.Empty;
                          d.TryGetValue("key", out value);
                      });

            return l;
        }

        protected static IEnumerable<Action<SharedDictionary<string, string>>> GetWriteMethods()
        {
            var l = new List<Action<SharedDictionary<string, string>>>();

            l.Add(d => d.Add("more", "value"));
            l.Add(d => d.Add(new KeyValuePair<string, string>("more", "value")));
            l.Add(d => d.Clear());
            l.Add(d => d.Remove(new KeyValuePair<string, string>("more", "value")));
            l.Add(d => d.Remove("key"));
            l.Add(d => d["key"] = "different");

            return l;
        }

        private SharedDictionary<TKey, TValue> InitSharedDictionary<TKey, TValue>(TKey key, TValue value)
        {
            var sharedDict = new SharedDictionary<TKey, TValue>(this.LockingStrategy);
            sharedDict.BackingDictionary.Add(key, value);
            return sharedDict;
        }
    }
}
