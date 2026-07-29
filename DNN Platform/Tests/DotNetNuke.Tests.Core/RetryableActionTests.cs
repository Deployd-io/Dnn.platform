// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class RetryableActionTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private SleepMonitor _sleepMonitor;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this._sleepMonitor = new SleepMonitor();
            RetryableAction.SleepAction = this._sleepMonitor.GoToSleep;
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): RetryableAction.SleepAction → {Arg0}", RetryableAction.SleepAction);
			Logger.LogDebug("Setup(): _sleepMonitor → {_sleepMonitor}", _sleepMonitor);
        }

        [Test]
        public void ActionSucceedsFirstTime()
        {
			Logger.LogDebug(">>> Entering ActionSucceedsFirstTime()");
            var monitor = new ActionMonitor();
            var retryable = CreateRetryable(monitor.Action);
			Logger.LogDebug("ActionSucceedsFirstTime(): monitor → {Monitor}", monitor);
			Logger.LogDebug("ActionSucceedsFirstTime(): retryable → {Retryable}", retryable);

            retryable.TryIt();

            Assert.That(monitor.TimesCalled, Is.EqualTo(1));
			Logger.LogDebug("<<< Exiting ActionSucceedsFirstTime()");
        }

        [Test]
        public void ActionFailsTwice()
        {
			Logger.LogDebug(">>> Entering ActionFailsTwice()");
            var monitor = new ActionMonitor(2);
            var retryable = CreateRetryable(monitor.Action);
			Logger.LogDebug("ActionFailsTwice(): monitor → {Monitor}", monitor);
			Logger.LogDebug("ActionFailsTwice(): retryable → {Retryable}", retryable);

            retryable.TryIt();

            Assert.That(monitor.TimesCalled, Is.EqualTo(3));
			Logger.LogDebug("<<< Exiting ActionFailsTwice()");
        }

        [Test]
        public void DelaysIncreaseWithEachRetry()
        {
			Logger.LogDebug(">>> Entering DelaysIncreaseWithEachRetry()");
            var monitor = new ActionMonitor(3);
            var retryable = CreateRetryable(monitor.Action, 10);
			Logger.LogDebug("DelaysIncreaseWithEachRetry(): monitor → {Monitor}", monitor);
			Logger.LogDebug("DelaysIncreaseWithEachRetry(): retryable → {Retryable}", retryable);

            retryable.TryIt();

            var firstRetry = this._sleepMonitor.SleepPeriod[0];
            var secondRetry = this._sleepMonitor.SleepPeriod[1];
            var thirdRetry = this._sleepMonitor.SleepPeriod[2];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstRetry, Is.EqualTo(5));
                Assert.That(secondRetry, Is.EqualTo(50));
                Assert.That(thirdRetry, Is.EqualTo(500));
            }
			Logger.LogDebug("<<< Exiting DelaysIncreaseWithEachRetry()");
        }

        [Test]
        public void ActionNeverSucceeds()
        {
			Logger.LogDebug(">>> Entering ActionNeverSucceeds()");
            var monitor = new ActionMonitor(-1);
            var retryable = CreateRetryable(monitor.Action);
			Logger.LogDebug("ActionNeverSucceeds(): monitor → {Monitor}", monitor);
			Logger.LogDebug("ActionNeverSucceeds(): retryable → {Retryable}", retryable);

            Assert.Throws<Exception>(() => retryable.TryIt());
			Logger.LogDebug("<<< Exiting ActionNeverSucceeds()");
        }

        private static RetryableAction CreateRetryable(Action action)
        {
			Logger.LogDebug(">>> Entering CreateRetryable(action={Action})", action);
			Logger.LogDebug("<<< Exiting CreateRetryable(action={Action})", action);
            return CreateRetryable(action, 1);
        }

        private static RetryableAction CreateRetryable(Action action, int factor)
        {
			Logger.LogDebug("<<< Exiting CreateRetryable(action={Action},factor={Factor})", action, factor);
            return new RetryableAction(action, "foo", 10, TimeSpan.FromMilliseconds(5), factor);
        }
    }

    internal class SleepMonitor
    {
        private readonly List<int> _periods = new List<int>();

        public IList<int> SleepPeriod
        {
            get
            {
                return this._periods.AsReadOnly();
            }
        }

        public void GoToSleep(int delay)
        {
			Logger.LogDebug(">>> Entering GoToSleep(delay={Delay})", delay);
            this._periods.Add(delay);
			Logger.LogDebug("<<< Exiting GoToSleep(delay={Delay})", delay);
        }
    }

    internal class ActionMonitor
    {
        private readonly List<DateTime> _callTimes = new List<DateTime>();
        private int _failuresRemaining;

        public ActionMonitor()
            : this(0)
        {
		Logger.LogDebug(">>> Entering ActionMonitor()");
		Logger.LogDebug("<<< Exiting ActionMonitor()");
		Logger.LogDebug(">>> Entering ActionMonitor(failureCount={FailureCount})", failureCount);
		Logger.LogDebug("<<< Exiting ActionMonitor(failureCount={FailureCount})", failureCount);
        }

        public ActionMonitor(int failureCount)
        {
            this._failuresRemaining = failureCount;
			Logger.LogDebug("ActionMonitor(failureCount={FailureCount}): _failuresRemaining → {_failuresRemaining}", failureCount, _failuresRemaining);
			Logger.LogDebug("ActionMonitor(failureCount={FailureCount}): this._failuresRemaining → {Arg1}", failureCount, this._failuresRemaining);
        }

        public IList<DateTime> CallTime
        {
            get
            {
                return this._callTimes.AsReadOnly();
            }
        }

        public int TimesCalled { get; private set; }

        public void Action()
        {
			Logger.LogDebug(">>> Entering Action()");
            this._callTimes.Add(DateTime.Now);
            this.TimesCalled++;

            if (this._failuresRemaining != 0)
            {
                if (this._failuresRemaining > 0)
                {
                    this._failuresRemaining--;
                }

                throw new Exception("it failed");
            }
			Logger.LogDebug("<<< Exiting Action()");
        }
    }
}
