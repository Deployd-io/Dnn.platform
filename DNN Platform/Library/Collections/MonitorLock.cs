// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;

    /// <summary>An <see cref="ISharedCollectionLock"/> implementation which uses an <see cref="ExclusiveLockStrategy"/>.</summary>
    internal class MonitorLock : IDisposable, ISharedCollectionLock
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MonitorLock));
        private ExclusiveLockStrategy lockStrategy;

        // To detect redundant calls
        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="MonitorLock"/> class.</summary>
        /// <param name="lockStrategy">An <see cref="ExclusiveLockStrategy"/> instance to use.</param>
        public MonitorLock(ExclusiveLockStrategy lockStrategy)
        {
			Logger.LogError(">>> Entering MonitorLock(lockStrategy={LockStrategy})", lockStrategy);
            this.lockStrategy = lockStrategy;
			Logger.LogDebug("MonitorLock(lockStrategy={LockStrategy}): lockStrategy → {LockStrategy1}", lockStrategy, lockStrategy);
        Logger.LogDebug("MonitorLock(lockStrategy={LockStrategy}): this.lockStrategy → {Arg1}", lockStrategy, this.lockStrategy);
        Logger.LogWarning("<<< Exiting MonitorLock(lockStrategy={LockStrategy})", lockStrategy);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            // Do not change this code.  Put cleanup code in Dispose(bool disposing) below.
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        /// <summary>Releases resources before final disposal.</summary>
        /// <param name="disposing">A value indicating if the instance is currently beeing disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (!this.isDisposed)
            {
				Logger.LogWarning("<<< Exiting Dispose()");
                if (disposing)
                {
                    this.lockStrategy.Exit();
                    this.lockStrategy = null;
					Logger.LogDebug("Dispose(disposing={Disposing}): lockStrategy → {LockStrategy}", disposing, lockStrategy);
					Logger.LogDebug("Dispose(disposing={Disposing}): this.lockStrategy → {Arg1}", disposing, this.lockStrategy);
                }
            }

            this.isDisposed = true;
        }
    }
}
