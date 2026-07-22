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

    public class UrlTestBase : DnnWebTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        protected virtual string TestType
        {
            get { return string.Empty; }
        }

        protected virtual string DefaultAlias { get; private set; }

        public virtual void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.ExecuteScriptFile($@"{this.TestType}\{this.GetTestFolder()}\SetUp.sql");
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        public override void OneTimeSetUp()
        {
			Logger.LogDebug(">>> Entering OneTimeSetUp()");
            this.ExecuteScriptFile($@"{this.TestType}\SetUp.sql");
            base.OneTimeSetUp();
			Logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        public virtual void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.ExecuteScriptFile($@"{this.TestType}\{this.GetTestFolder()}\TearDown.sql");
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        public virtual void OneTimeTearDown()
        {
			Logger.LogDebug(">>> Entering OneTimeTearDown()");
            this.ExecuteScriptFile($@"{this.TestType}\TearDown.sql");
			Logger.LogDebug("<<< Exiting OneTimeTearDown()");
        }

        protected void CreateTab(string tabName)
        {
			Logger.LogDebug(">>> Entering CreateTab(tabName={TabName})", tabName);
            var tab = new TabInfo { PortalID = this.PortalId, TabName = tabName };

            TabController.Instance.AddTab(tab);
			Logger.LogDebug("<<< Exiting CreateTab(tabName={TabName})", tabName);
        }

        protected void GetDefaultAlias()
        {
			Logger.LogDebug(">>> Entering GetDefaultAlias()");
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (alias.IsPrimary)
                {
                    this.DefaultAlias = alias.HTTPAlias;
					Logger.LogDebug("GetDefaultAlias(): this.DefaultAlias → {Arg0}", this.DefaultAlias);
                    break;
                }
            }
			Logger.LogDebug("<<< Exiting GetDefaultAlias()");
        }

        protected void SetDefaultAlias(Dictionary<string, string> testFields)
        {
            this.SetDefaultAlias(testFields["Alias"]);
        }

        protected void SetDefaultAlias(string defaultAlias)
        {
			Logger.LogDebug(">>> Entering SetDefaultAlias(defaultAlias={DefaultAlias})", defaultAlias);
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (string.Equals(alias.HTTPAlias, defaultAlias, StringComparison.InvariantCultureIgnoreCase))
                {
                    alias.IsPrimary = true;
					Logger.LogDebug("SetDefaultAlias(defaultAlias={DefaultAlias}): alias.IsPrimary → {Arg1}", defaultAlias, alias.IsPrimary);
                    PortalAliasController.Instance.UpdatePortalAlias(alias);
                    break;
                }
            }
			Logger.LogDebug("<<< Exiting SetDefaultAlias(defaultAlias={DefaultAlias})", defaultAlias);
        }

        private void ExecuteScriptFile(string fileName)
        {
			Logger.LogDebug(">>> Entering ExecuteScriptFile(fileName={FileName})", fileName);
            var sql = TestUtil.ReadStream(fileName);

			Logger.LogDebug("ExecuteScriptFile(fileName={FileName}): sql → {Sql}", fileName, sql);
            if (!string.IsNullOrEmpty(sql))
            {
                DataProvider.Instance().ExecuteScript(sql);
            }
			Logger.LogDebug("<<< Exiting ExecuteScriptFile(fileName={FileName})", fileName);
        }

        private string GetTestFolder()
        {
			Logger.LogDebug(">>> Entering GetTestFolder()");
            var testName = TestContext.CurrentContext.Test.Name;
			Logger.LogDebug("<<< Exiting GetTestFolder()");
            return testName.Substring(0, testName.IndexOf("_", StringComparison.Ordinal));
        }
    }
}
