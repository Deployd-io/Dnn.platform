// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Services.Installer
{
    using System;

    using DotNetNuke.Instrumentation;

    internal class TestLogSource : ILoggerSource
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ILog GetLogger(string name)
        {
			Logger.LogDebug(">>> Entering GetLogger(name={Name})", name);
			Logger.LogDebug("<<< Exiting GetLogger(name={Name})", name);
            return new TestLogger();
        }

        public ILog GetLogger(Type type)
        {
			Logger.LogDebug("<<< Exiting GetLogger(type={Type})", type);
            return new TestLogger();
        }
    }
}
