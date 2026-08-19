// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    public class UrlTestBase : DnnWebTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        protected virtual string TestType
        {
            get { return string.Empty; }
        }

        protected virtual string DefaultAlias { get; private set; }

        public virtual void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            this.ExecuteScriptFile($@"{this.TestType}\{this.GetTestFolder()}\SetUp.sql");
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        public override void OneTimeSetUp()
        {
			_logger.LogDebug(">>> Entering OneTimeSetUp()");
            this.ExecuteScriptFile($@"{this.TestType}\SetUp.sql");
            base.OneTimeSetUp();
			_logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        public virtual void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.ExecuteScriptFile($@"{this.TestType}\{this.GetTestFolder()}\TearDown.sql");
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        public virtual void OneTimeTearDown()
        {
			_logger.LogDebug(">>> Entering OneTimeTearDown()");
            this.ExecuteScriptFile($@"{this.TestType}\TearDown.sql");
			_logger.LogDebug("<<< Exiting OneTimeTearDown()");
        }

        protected void CreateTab(string tabName)
        {
			_logger.LogDebug(">>> Entering CreateTab(tabName={TabName})", tabName);
            var tab = new TabInfo { PortalID = this.PortalId, TabName = tabName };

            TabController.Instance.AddTab(tab);
			_logger.LogDebug("<<< Exiting CreateTab(tabName={TabName})", tabName);
        }

        protected void GetDefaultAlias()
        {
			_logger.LogDebug(">>> Entering GetDefaultAlias()");
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (alias.IsPrimary)
                {
                    this.DefaultAlias = alias.HTTPAlias;
					_logger.LogDebug("GetDefaultAlias(): this.DefaultAlias → {Arg0}", this.DefaultAlias);
                    break;
                }
            }
			_logger.LogDebug("<<< Exiting GetDefaultAlias()");
        }

        protected void SetDefaultAlias(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering SetDefaultAlias(testFields={TestFields})", testFields);
            this.SetDefaultAlias(testFields["Alias"]);
			_logger.LogDebug("<<< Exiting SetDefaultAlias(testFields={TestFields})", testFields);
        }

        protected void SetDefaultAlias(string defaultAlias)
        {
			_logger.LogDebug(">>> Entering SetDefaultAlias(defaultAlias={DefaultAlias})", defaultAlias);
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (string.Equals(alias.HTTPAlias, defaultAlias, StringComparison.InvariantCultureIgnoreCase))
                {
                    alias.IsPrimary = true;
					_logger.LogDebug("SetDefaultAlias(defaultAlias={DefaultAlias}): alias.IsPrimary → {Arg1}", defaultAlias, alias.IsPrimary);
                    PortalAliasController.Instance.UpdatePortalAlias(alias);
                    break;
                }
            }
			_logger.LogDebug("<<< Exiting SetDefaultAlias(defaultAlias={DefaultAlias})", defaultAlias);
        }

        private void ExecuteScriptFile(string fileName)
        {
			_logger.LogDebug(">>> Entering ExecuteScriptFile(fileName={FileName})", fileName);
            var sql = TestUtil.ReadStream(fileName);

			_logger.LogDebug("ExecuteScriptFile(fileName={FileName}): sql → {Sql}", fileName, sql);
            if (!string.IsNullOrEmpty(sql))
            {
                DataProvider.Instance().ExecuteScript(sql);
            }
			_logger.LogDebug("<<< Exiting ExecuteScriptFile(fileName={FileName})", fileName);
        }

        private string GetTestFolder()
        {
			_logger.LogDebug(">>> Entering GetTestFolder()");
            var testName = TestContext.CurrentContext.Test.Name;
			_logger.LogDebug("<<< Exiting GetTestFolder()");
            return testName.Substring(0, testName.IndexOf("_", StringComparison.Ordinal));
        }
    }
}
