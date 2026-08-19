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
        private SleepMonitor _sleepMonitor;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this._sleepMonitor = new SleepMonitor();
            RetryableAction.SleepAction = this._sleepMonitor.GoToSleep;
			mockEventLogger.LogDebug("Setup(): RetryableAction.SleepAction → {Arg0}", RetryableAction.SleepAction);
			mockEventLogger.LogDebug("Setup(): _sleepMonitor → {_sleepMonitor}", _sleepMonitor);
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void ActionSucceedsFirstTime()
        {
			mockEventLogger.LogDebug(">>> Entering ActionSucceedsFirstTime()");
            var monitor = new ActionMonitor();
            var retryable = CreateRetryable(monitor.Action);
			mockEventLogger.LogDebug("ActionSucceedsFirstTime(): retryable → {Retryable}", retryable);
			mockEventLogger.LogDebug("ActionSucceedsFirstTime(): monitor → {Monitor}", monitor);

            retryable.TryIt();

            Assert.That(monitor.TimesCalled, Is.EqualTo(1));
			mockEventLogger.LogDebug("<<< Exiting ActionSucceedsFirstTime()");
        }

        [Test]
        public void ActionFailsTwice()
        {
			mockEventLogger.LogDebug(">>> Entering ActionFailsTwice()");
            var monitor = new ActionMonitor(2);
            var retryable = CreateRetryable(monitor.Action);
			mockEventLogger.LogDebug("ActionFailsTwice(): retryable → {Retryable}", retryable);
			mockEventLogger.LogDebug("ActionFailsTwice(): monitor → {Monitor}", monitor);

            retryable.TryIt();

            Assert.That(monitor.TimesCalled, Is.EqualTo(3));
			mockEventLogger.LogDebug("<<< Exiting ActionFailsTwice()");
        }

        [Test]
        public void DelaysIncreaseWithEachRetry()
        {
			mockEventLogger.LogDebug(">>> Entering DelaysIncreaseWithEachRetry()");
            var monitor = new ActionMonitor(3);
            var retryable = CreateRetryable(monitor.Action, 10);
			mockEventLogger.LogDebug("DelaysIncreaseWithEachRetry(): retryable → {Retryable}", retryable);
			mockEventLogger.LogDebug("DelaysIncreaseWithEachRetry(): monitor → {Monitor}", monitor);

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
			mockEventLogger.LogDebug("<<< Exiting DelaysIncreaseWithEachRetry()");
        }

        [Test]
        public void ActionNeverSucceeds()
        {
			mockEventLogger.LogDebug(">>> Entering ActionNeverSucceeds()");
            var monitor = new ActionMonitor(-1);
            var retryable = CreateRetryable(monitor.Action);
			mockEventLogger.LogDebug("ActionNeverSucceeds(): retryable → {Retryable}", retryable);
			mockEventLogger.LogDebug("ActionNeverSucceeds(): monitor → {Monitor}", monitor);

            Assert.Throws<Exception>(() => retryable.TryIt());
			mockEventLogger.LogDebug("<<< Exiting ActionNeverSucceeds()");
        }

        private static RetryableAction CreateRetryable(Action action)
        {
			mockEventLogger.LogDebug(">>> Entering CreateRetryable(action={Action})", action);
			mockEventLogger.LogDebug("<<< Exiting CreateRetryable(action={Action})", action);
			mockEventLogger.LogDebug("<<< Exiting CreateRetryable(action={Action},factor={Factor})", action, factor);
            return CreateRetryable(action, 1);
        }

        private static RetryableAction CreateRetryable(Action action, int factor)
        {
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
			mockEventLogger.LogDebug(">>> Entering GoToSleep(delay={Delay})", delay);
            this._periods.Add(delay);
			mockEventLogger.LogDebug("<<< Exiting GoToSleep(delay={Delay})", delay);
        }
    }

    internal class ActionMonitor
    {
        private readonly List<DateTime> _callTimes = new List<DateTime>();
        private int _failuresRemaining;

        public ActionMonitor()
            : this(0)
        {
		mockEventLogger.LogDebug(">>> Entering ActionMonitor()");
		mockEventLogger.LogDebug("<<< Exiting ActionMonitor()");
		mockEventLogger.LogDebug(">>> Entering ActionMonitor(failureCount={FailureCount})", failureCount);
        }

        public ActionMonitor(int failureCount)
        {
            this._failuresRemaining = failureCount;
			mockEventLogger.LogDebug("ActionMonitor(failureCount={FailureCount}): _failuresRemaining → {_failuresRemaining}", failureCount, _failuresRemaining);
			mockEventLogger.LogDebug("<<< Exiting ActionMonitor(failureCount={FailureCount})", failureCount);
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
			mockEventLogger.LogDebug(">>> Entering Action()");
            this._callTimes.Add(DateTime.Now);
            this.TimesCalled++;

            if (this._failuresRemaining != 0)
            {
                if (this._failuresRemaining > 0)
                {
                    this._failuresRemaining--;
                }

				mockEventLogger.LogWarning("Throwing Exception from Action()");
			mockEventLogger.LogDebug("<<< Exiting Action()");
                throw new Exception("it failed");
            }
        }
    }
}
