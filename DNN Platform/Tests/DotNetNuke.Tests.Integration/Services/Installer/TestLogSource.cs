// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Services.Installer
{
    using System;

    using DotNetNuke.Instrumentation;
	using Microsoft.Extensions.Logging;

    internal class TestLogSource : ILoggerSource
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public ILog GetLogger(string name)
        {
			_logger.LogDebug(">>> Entering GetLogger(name={Name})", name);
			_logger.LogDebug("<<< Exiting GetLogger(name={Name})", name);
			_logger.LogDebug("<<< Exiting GetLogger(type={Type})", type);
            return new TestLogger();
        }

        public ILog GetLogger(Type type)
        {
            return new TestLogger();
        }
    }
}
