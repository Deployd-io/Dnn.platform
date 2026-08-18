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

    public abstract class LockStrategyTests
    {
        [Test]
        public void DoubleDisposeAllowed()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleDisposeAllowed()");
            var strategy = this.GetLockStrategy();

			mockEventLogger.LogDebug("DoubleDisposeAllowed(): strategy → {Strategy}", strategy);
            strategy.Dispose();
            strategy.Dispose();

            // no exception on 2nd dispose
			mockEventLogger.LogDebug("<<< Exiting DoubleDisposeAllowed()");
        }

        protected void DoubleReadLock()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleReadLock()");
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetReadLock())
                {
                    using (var readLock2 = strategy.GetReadLock())
                    {
                        // do nothing
                    }
                }
            }
			mockEventLogger.LogDebug("<<< Exiting DoubleReadLock()");
        }

        [Test]
        public virtual void DoubleReadLockThrows()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleReadLockThrows()");
            Assert.Throws<LockRecursionException>(this.DoubleReadLock);
			mockEventLogger.LogDebug("<<< Exiting DoubleReadLockThrows()");
        }

        [Test]
        public void ReadAndWriteLockOnSameThreadThrows()
        {
			mockEventLogger.LogDebug(">>> Entering ReadAndWriteLockOnSameThreadThrows()");
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetReadLock())
                {
                    Assert.Throws<LockRecursionException>(
                        () =>
                        {
                            using (var readLock2 = strategy.GetWriteLock())
                            {
                                // do nothing
                            }
                        });
                }
            }
			mockEventLogger.LogDebug("<<< Exiting ReadAndWriteLockOnSameThreadThrows()");
        }

        [Test]
        public void WriteAndReadLockOnSameThreadThrows()
        {
			mockEventLogger.LogDebug(">>> Entering WriteAndReadLockOnSameThreadThrows()");
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetWriteLock())
                {
                    Assert.Throws<LockRecursionException>(
                        () =>
                        {
                            using (var readLock2 = strategy.GetReadLock())
                            {
                                // do nothing
                            }
                        });
                }
            }
			mockEventLogger.LogDebug("<<< Exiting WriteAndReadLockOnSameThreadThrows()");
        }

        [Test]
        public void DoubleReadLockOnDifferentThreads()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleReadLockOnDifferentThreads()");
            using (var strategy = this.GetLockStrategy())
            {
                if (strategy.SupportsConcurrentReads)
                {
                    using (var readLock1 = strategy.GetReadLock())
                    {
                        var t = new Thread(GetReadLock);
						mockEventLogger.LogDebug("DoubleReadLockOnDifferentThreads(): t → {T}", t);
                        t.Start(strategy);

                        // sleep and let new thread run
                        t.Join(TimeSpan.FromMilliseconds(100));

                        // assert that read thread has terminated
                        Console.WriteLine(t.ThreadState.ToString());
                        Assert.That(t.ThreadState, Is.EqualTo(ThreadState.Stopped));
                    }
                }
            }
			mockEventLogger.LogDebug("<<< Exiting DoubleReadLockOnDifferentThreads()");
        }

        [Test]
        public void DoubleWriteLockOnDifferentThreadsWaits()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleWriteLockOnDifferentThreadsWaits()");
            using (ILockStrategy strategy = this.GetLockStrategy())
            {
                Thread t;
                using (var writeLock1 = strategy.GetWriteLock())
                {
                    t = new Thread(GetWriteLock);
					mockEventLogger.LogDebug("DoubleWriteLockOnDifferentThreadsWaits(): t → {T}", t);
                    t.Start(strategy);

                    // sleep and let new thread run and block
                    Thread.Sleep(50);

                    // assert that write thread has not terminated
                    Assert.That(t.IsAlive, Is.True);
                } // release write lock

                Thread.Sleep(50);

                // assert that getwritelock did complete once first writelock was released it's call
                Assert.That(t.IsAlive, Is.False);
            }
			mockEventLogger.LogDebug("<<< Exiting DoubleWriteLockOnDifferentThreadsWaits()");
        }

        [Test]
        public virtual void DoubleWriteLockThrows()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleWriteLockThrows()");
            Assert.Throws<LockRecursionException>(this.DoubleWriteLock);
			mockEventLogger.LogDebug("<<< Exiting DoubleWriteLockThrows()");
        }

        protected void DoubleWriteLock()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleWriteLock()");
            using (ILockStrategy strategy = this.GetLockStrategy())
            {
                using (var writeLock1 = strategy.GetWriteLock())
                {
                    using (var writeLock2 = strategy.GetWriteLock())
                    {
                        // do nothing
                    }
                }
            }
			mockEventLogger.LogDebug("<<< Exiting DoubleWriteLock()");
        }

        [Test]
        [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
        public virtual void MethodsThrowAfterDisposed(Action<ILockStrategy> methodCall)
        {
			mockEventLogger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            var strategy = this.GetLockStrategy();

			mockEventLogger.LogDebug("MethodsThrowAfterDisposed(methodCall={MethodCall}): strategy → {Strategy}", methodCall, strategy);
            strategy.Dispose();
            Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(strategy));
			mockEventLogger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        [Test]
        public void ReadLockPreventsWriteLock()
        {
			mockEventLogger.LogDebug(">>> Entering ReadLockPreventsWriteLock()");
            Thread t = null;

			mockEventLogger.LogDebug("ReadLockPreventsWriteLock(): t → {T}", t);
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock = strategy.GetReadLock())
                {
                    t = new Thread(GetWriteLock);
                    t.Start(strategy);

                    // sleep and let new thread run
                    Thread.Sleep(100);

                    // assert that write thread is still waiting
                    Assert.That(t.IsAlive, Is.True);
                }

                // release read lock

                // sleep and let write thread finish up
                Thread.Sleep(100);
                Assert.That(t.IsAlive, Is.False);
            }

            // release controller
			mockEventLogger.LogDebug("<<< Exiting ReadLockPreventsWriteLock()");
        }

        [Test]
        public void OnlyOneWriteLockAllowed()
        {
			mockEventLogger.LogDebug(">>> Entering OnlyOneWriteLockAllowed()");
            Thread t = null;

			mockEventLogger.LogDebug("OnlyOneWriteLockAllowed(): t → {T}", t);
            using (var strategy = this.GetLockStrategy())
            {
                using (var writeLock = strategy.GetWriteLock())
                {
                    t = new Thread(GetWriteLock);
                    t.Start(strategy);

                    // sleep and let new thread run
                    Thread.Sleep(100);

                    // assert that write thread is still waiting
                    Assert.That(t.IsAlive, Is.True);
                }

                // release write lock
                // sleep and let write thread finish up
                Thread.Sleep(100);
                Assert.That(t.IsAlive, Is.False);
            }
			mockEventLogger.LogDebug("<<< Exiting OnlyOneWriteLockAllowed()");
        }

        [Test]
        public void MultipleReadLocksAllowed()
        {
			mockEventLogger.LogDebug(">>> Entering MultipleReadLocksAllowed()");
            Thread t = null;

			mockEventLogger.LogDebug("MultipleReadLocksAllowed(): t → {T}", t);
            using (var strategy = this.GetLockStrategy())
            {
                if (strategy.SupportsConcurrentReads)
                {
                    using (var readLock = strategy.GetReadLock())
                    {
                        t = new Thread(GetReadLock);
                        t.Start(strategy);

                        // sleep and let new thread run
                        Thread.Sleep(100);

                        // assert that read thread has terminated
                        Assert.That(t.IsAlive, Is.False);
                    }
                }
                else
                {
                    Assert.Pass();
                }
            }
			mockEventLogger.LogDebug("<<< Exiting MultipleReadLocksAllowed()");
        }

        internal abstract ILockStrategy GetLockStrategy();

        protected static IEnumerable<Action<ILockStrategy>> GetObjectDisposedExceptionMethods()
        {
			mockEventLogger.LogDebug(">>> Entering GetObjectDisposedExceptionMethods()");
            var l = new List<Action<ILockStrategy>>();

			mockEventLogger.LogDebug("GetObjectDisposedExceptionMethods(): l → {L}", l);
            l.Add((ILockStrategy strategy) => strategy.GetReadLock());
            l.Add((ILockStrategy strategy) => strategy.GetWriteLock());
            l.Add((ILockStrategy strategy) => Console.WriteLine(strategy.ThreadCanRead));
            l.Add((ILockStrategy strategy) => Console.WriteLine(strategy.ThreadCanWrite));

			mockEventLogger.LogDebug("<<< Exiting GetObjectDisposedExceptionMethods()");
            return l;
        }

        private static void GetReadLock(object obj)
        {
			mockEventLogger.LogDebug(">>> Entering GetReadLock(obj={Obj})", obj);
            var strategy = (ILockStrategy)obj;
            using (var readLock = strategy.GetReadLock())
            {
                // do nothing
            }
			mockEventLogger.LogDebug("<<< Exiting GetReadLock(obj={Obj})", obj);
        }

        private static void GetWriteLock(object obj)
        {
			mockEventLogger.LogDebug(">>> Entering GetWriteLock(obj={Obj})", obj);
            var strategy = (ILockStrategy)obj;
            using (var writeLock = strategy.GetWriteLock())
            {
                // do nothing
            }
			mockEventLogger.LogDebug("<<< Exiting GetWriteLock(obj={Obj})", obj);
        }
    }
}
