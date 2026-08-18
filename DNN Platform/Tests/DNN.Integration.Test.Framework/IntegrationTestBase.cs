// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework.Helpers;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    public abstract class IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        // public static string DatabaseName { get; }
        static IntegrationTestBase()
        {
			_logger.LogDebug(">>> Entering IntegrationTestBase()");
            ServicePointManager.Expect100Continue = false;

			_logger.LogDebug("IntegrationTestBase(): ServicePointManager.Expect100Continue → {Arg0}", ServicePointManager.Expect100Continue);
            // setup of the whole system: take a snapshot and keep using it for all tests
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            ConnectionString = AppConfigHelper.ConnectionString;
			_logger.LogDebug("IntegrationTestBase(): ConnectionString → {ConnectionString}", ConnectionString);

            // DatabaseName = GetDbNameFromConnectionString(ConnectionString);

            // SchedulerController.DisableAllSchedulers(false);
            // SchedulerController.DisableAppStartDelay(false);
			_logger.LogDebug("<<< Exiting IntegrationTestBase()");
        }

        public static string ConnectionString { get; }

        public static void LogText(string text)
        {
			_logger.LogDebug(">>> Entering LogText(text={Text})", text);
            // Don't write anything to console when we run in TeamCity
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
            {
                Console.WriteLine(text);
            }
			_logger.LogDebug("<<< Exiting LogText(text={Text})", text);
        }

        [OneTimeSetUp]
        public virtual async Task TestFixtureSetUp()
        {
			_logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await Task.Yield();
			_logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [OneTimeTearDown]
        public virtual void TestFixtureTearDown()
        {
		_logger.LogDebug(">>> Entering TestFixtureTearDown()");
		_logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        private static string GetDbNameFromConnectionString(string connectionString)
        {
			_logger.LogDebug(">>> Entering GetDbNameFromConnectionString(connectionString={ConnectionString})", connectionString);
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
			_logger.LogDebug("<<< Exiting GetDbNameFromConnectionString(connectionString={ConnectionString})", connectionString);
            return builder["Database"] as string;
        }
    }
}
