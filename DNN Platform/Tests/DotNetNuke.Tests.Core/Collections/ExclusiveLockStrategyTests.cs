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
        [Test]
        [TestCaseSource(nameof(GetExclusiveLockStrategyObjectDisposedExceptionMethods))]
        public override void MethodsThrowAfterDisposed(Action<ILockStrategy> methodCall)
        {
			mockEventLogger.LogDebug(">>> Entering MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
            base.MethodsThrowAfterDisposed(methodCall);
			mockEventLogger.LogDebug("<<< Exiting MethodsThrowAfterDisposed(methodCall={MethodCall})", methodCall);
        }

        internal override ILockStrategy GetLockStrategy()
        {
			mockEventLogger.LogDebug(">>> Entering GetLockStrategy()");
			mockEventLogger.LogDebug("<<< Exiting GetLockStrategy()");
            return new ExclusiveLockStrategy();
        }

        protected static IEnumerable<Action<ILockStrategy>> GetExclusiveLockStrategyObjectDisposedExceptionMethods()
        {
			mockEventLogger.LogDebug(">>> Entering GetExclusiveLockStrategyObjectDisposedExceptionMethods()");
            var l = (List<Action<ILockStrategy>>)LockStrategyTests.GetObjectDisposedExceptionMethods();

			mockEventLogger.LogDebug("GetExclusiveLockStrategyObjectDisposedExceptionMethods(): l → {L}", l);
            l.Add((ILockStrategy strategy) =>
                      {
                          ExclusiveLockStrategy els = (ExclusiveLockStrategy)strategy;
						mockEventLogger.LogDebug("GetExclusiveLockStrategyObjectDisposedExceptionMethods(): els → {Els}", els);
                          els.Exit();
                      });

			mockEventLogger.LogDebug("<<< Exiting GetExclusiveLockStrategyObjectDisposedExceptionMethods()");
            return l;
        }
    }
}
