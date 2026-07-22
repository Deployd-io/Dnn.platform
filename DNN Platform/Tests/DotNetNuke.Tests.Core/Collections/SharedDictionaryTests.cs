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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public abstract LockingStrategy LockingStrategy { get; }

        [Test]
        public void TryAdd()
        {
			Logger.LogDebug(">>> Entering TryAdd()");
            const string KEY = "key";
            const string VALUE = "value";

            var sharedDictionary = new SharedDictionary<string, string>(this.LockingStrategy);

			Logger.LogDebug("TryAdd(): sharedDictionary → {SharedDictionary}", sharedDictionary);
            bool doInsert = false;
            using (ISharedCollectionLock l = sharedDictionary.GetReadLock())
            {
			Logger.LogDebug("TryAdd(): doInsert → {DoInsert}", doInsert);
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
			Logger.LogDebug("<<< Exiting TryAdd()");
        }

        [Test]
        [TestCaseSource(nameof(GetWriteMethods))]
        public void WriteRequiresLock(Action<SharedDictionary<string, string>> writeAction)
        {
            Assert.Throws<WriteLockRequiredException>(() => writeAction.Invoke(this.InitSharedDictionary("key", "value")));
        }

        [Test]
        [TestCaseSource(nameof(GetReadMethods))]
        public void ReadRequiresLock(Action<SharedDictionary<string, string>> readAction)
        {
            Assert.Throws<ReadLockRequiredException>(() => readAction.Invoke(this.InitSharedDictionary("key", "value")));
        }

        [Test]
        public void DisposedReadLockDeniesRead()
        {
			Logger.LogDebug(">>> Entering DisposedReadLockDeniesRead()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			Logger.LogDebug("DisposedReadLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetReadLock();
            l.Dispose();
			Logger.LogDebug("DisposedReadLockDeniesRead(): l → {L}", l);

            Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
			Logger.LogDebug("<<< Exiting DisposedReadLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesRead()
        {
			Logger.LogDebug(">>> Entering DisposedWriteLockDeniesRead()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			Logger.LogDebug("DisposedWriteLockDeniesRead(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
            l.Dispose();
			Logger.LogDebug("DisposedWriteLockDeniesRead(): l → {L}", l);

            Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
			Logger.LogDebug("<<< Exiting DisposedWriteLockDeniesRead()");
        }

        [Test]
        public void DisposedWriteLockDeniesWrite()
        {
			Logger.LogDebug(">>> Entering DisposedWriteLockDeniesWrite()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			Logger.LogDebug("DisposedWriteLockDeniesWrite(): d → {D}", d);
            ISharedCollectionLock l = d.GetWriteLock();
            l.Dispose();
			Logger.LogDebug("DisposedWriteLockDeniesWrite(): l → {L}", l);

            Assert.Throws<WriteLockRequiredException>(() => d["ke"] = "foo");
			Logger.LogDebug("<<< Exiting DisposedWriteLockDeniesWrite()");
        }

        [Test]
        public void WriteLockEnablesRead()
        {
			Logger.LogDebug(">>> Entering WriteLockEnablesRead()");
            var d = this.InitSharedDictionary("key", "value");

			Logger.LogDebug("WriteLockEnablesRead(): d → {D}", d);
            string actualValue = null;
            using (ISharedCollectionLock l = d.GetWriteLock())
            {
			Logger.LogDebug("WriteLockEnablesRead(): actualValue → {ActualValue}", actualValue);
                actualValue = d["key"];
            }

            Assert.That(actualValue, Is.EqualTo("value"));
			Logger.LogDebug("<<< Exiting WriteLockEnablesRead()");
        }

        [Test]
        public void CanGetAnotherLockAfterDisposingLock()
        {
			Logger.LogDebug(">>> Entering CanGetAnotherLockAfterDisposingLock()");
            var d = new SharedDictionary<string, string>(this.LockingStrategy);
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
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

			Logger.LogDebug("DoubleDispose(): d → {D}", d);
            d.Dispose();
            d.Dispose();
			Logger.LogDebug("<<< Exiting DoubleDispose()");
        }

        [Test]
        [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
        public void MethodsThrowAfterDisposed(Action<SharedDictionary<string, string>> methodCall)
        {
            var d = new SharedDictionary<string, string>(this.LockingStrategy);

            d.Dispose();
            Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(d));
        }

        [Test]
        public void TwoDictsShareALockWriteTest()
        {
			Logger.LogDebug(">>> Entering TwoDictsShareALockWriteTest()");
            var lockStrategy = LockingStrategyFactory.Create(this.LockingStrategy);
            var dict1 = new SharedDictionary<string, string>(lockStrategy);
			Logger.LogDebug("TwoDictsShareALockWriteTest(): lockStrategy → {LockStrategy}", lockStrategy);
			Logger.LogDebug("TwoDictsShareALockWriteTest(): dict1 → {Dict1}", dict1);
            var dict2 = new SharedDictionary<string, string>(lockStrategy);

			Logger.LogDebug("TwoDictsShareALockWriteTest(): dict2 → {Dict2}", dict2);
            using (dict1.GetReadLock())
            {
                ISharedCollectionLock writeLock = null;
                try
				Logger.LogDebug("TwoDictsShareALockWriteTest(): writeLock → {WriteLock}", writeLock);
                {
                    writeLock = dict2.GetWriteLock();
                }
                catch (LockRecursionException)
                {
					Logger.LogError("Exception in TwoDictsShareALockWriteTest(): {Arg0}", ex.Message);
                    Assert.Pass();
                }
                catch (Exception exception)
                {
                    Assert.Fail($"Expected LockRecursionException, got {exception}");
                }
                finally
                {
                    writeLock?.Dispose();
                }
            }

            Assert.Fail("Expected LockRecursionException, did not throw");
			Logger.LogDebug("<<< Exiting TwoDictsShareALockWriteTest()");
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
