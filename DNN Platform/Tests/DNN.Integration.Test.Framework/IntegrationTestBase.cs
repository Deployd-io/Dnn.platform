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

    public abstract class IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // public static string DatabaseName { get; }
        static IntegrationTestBase()
        {
			Logger.LogDebug(">>> Entering IntegrationTestBase()");
            ServicePointManager.Expect100Continue = false;

			Logger.LogDebug("IntegrationTestBase(): ServicePointManager.Expect100Continue → {Arg0}", ServicePointManager.Expect100Continue);
            // setup of the whole system: take a snapshot and keep using it for all tests
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            ConnectionString = AppConfigHelper.ConnectionString;
			Logger.LogDebug("IntegrationTestBase(): ConnectionString → {ConnectionString}", ConnectionString);

            // DatabaseName = GetDbNameFromConnectionString(ConnectionString);

            // SchedulerController.DisableAllSchedulers(false);
            // SchedulerController.DisableAppStartDelay(false);
			Logger.LogDebug("<<< Exiting IntegrationTestBase()");
        }

        public static string ConnectionString { get; }

        public static void LogText(string text)
        {
			Logger.LogDebug(">>> Entering LogText(text={Text})", text);
            // Don't write anything to console when we run in TeamCity
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
            {
                Console.WriteLine(text);
            }
			Logger.LogDebug("<<< Exiting LogText(text={Text})", text);
        }

        [OneTimeSetUp]
        public virtual async Task TestFixtureSetUp()
        {
			Logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await Task.Yield();
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [OneTimeTearDown]
        public virtual void TestFixtureTearDown()
        {
		Logger.LogDebug(">>> Entering TestFixtureTearDown()");
		Logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        private static string GetDbNameFromConnectionString(string connectionString)
        {
			Logger.LogDebug(">>> Entering GetDbNameFromConnectionString(connectionString={ConnectionString})", connectionString);
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
			Logger.LogDebug("<<< Exiting GetDbNameFromConnectionString(connectionString={ConnectionString})", connectionString);
            return builder["Database"] as string;
        }
    }
}
