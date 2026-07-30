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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public override void DoubleReadLockThrows()
        {
			Logger.LogDebug(">>> Entering DoubleReadLockThrows()");
            Assert.DoesNotThrow(base.DoubleReadLock);
			Logger.LogDebug("<<< Exiting DoubleReadLockThrows()");
        }

        [Test]
        public override void DoubleWriteLockThrows()
        {
			Logger.LogDebug(">>> Entering DoubleWriteLockThrows()");
            Assert.DoesNotThrow(base.DoubleWriteLock);
			Logger.LogDebug("<<< Exiting DoubleWriteLockThrows()");
        }

        internal override ILockStrategy GetLockStrategy()
        {
			Logger.LogDebug(">>> Entering GetLockStrategy()");
			Logger.LogDebug("<<< Exiting GetLockStrategy()");
            return new ReaderWriterLockStrategy();
        }
    }
}
