// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ExclusiveLockStrategyTests : LockStrategyTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        [TestCaseSource(nameof(GetExclusiveLockStrategyObjectDisposedExceptionMethods))]
        public override void MethodsThrowAfterDisposed(Action<ILockStrategy> methodCall)
        {
			Logger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            base.MethodsThrowAfterDisposed(methodCall);
			Logger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        internal override ILockStrategy GetLockStrategy()
        {
			Logger.LogDebug(">>> Entering GetLockStrategy()");
			Logger.LogDebug("<<< Exiting GetLockStrategy()");
            return new ExclusiveLockStrategy();
        }

        protected static IEnumerable<Action<ILockStrategy>> GetExclusiveLockStrategyObjectDisposedExceptionMethods()
        {
			Logger.LogDebug(">>> Entering GetExclusiveLockStrategyObjectDisposedExceptionMethods()");
            var l = (List<Action<ILockStrategy>>)LockStrategyTests.GetObjectDisposedExceptionMethods();

			Logger.LogDebug("GetExclusiveLockStrategyObjectDisposedExceptionMethods(): l → {L}", l);
            l.Add((ILockStrategy strategy) =>
                      {
                          ExclusiveLockStrategy els = (ExclusiveLockStrategy)strategy;
                          els.Exit();
						Logger.LogDebug("GetExclusiveLockStrategyObjectDisposedExceptionMethods(): els → {Els}", els);
                      });

			Logger.LogDebug("<<< Exiting GetExclusiveLockStrategyObjectDisposedExceptionMethods()");
            return l;
        }
    }
}
