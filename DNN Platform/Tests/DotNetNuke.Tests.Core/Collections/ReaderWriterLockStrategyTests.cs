// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using DotNetNuke.Collections.Internal;

    using NUnit.Framework;

    [TestFixture]
    public class ReaderWriterLockStrategyTests : LockStrategyTests
    {
        [Test]
        public override void DoubleReadLockThrows()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleReadLockThrows()");
            Assert.DoesNotThrow(base.DoubleReadLock);
			mockEventLogger.LogDebug("<<< Exiting DoubleReadLockThrows()");
        }

        [Test]
        public override void DoubleWriteLockThrows()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleWriteLockThrows()");
            Assert.DoesNotThrow(base.DoubleWriteLock);
			mockEventLogger.LogDebug("<<< Exiting DoubleWriteLockThrows()");
        }

        internal override ILockStrategy GetLockStrategy()
        {
			mockEventLogger.LogDebug(">>> Entering GetLockStrategy()");
			mockEventLogger.LogDebug("<<< Exiting GetLockStrategy()");
            return new ReaderWriterLockStrategy();
        }
    }
}
