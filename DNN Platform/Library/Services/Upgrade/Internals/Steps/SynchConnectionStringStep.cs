// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// SynchConnectionStringStep - Step that synchs connection string between DotNetNuke.Install.Config and Web.Config
    /// The connection information stored in DotNetNuke.Install.Config takes precendence (if present)
    /// Synchornization only happens when settings are different.
    /// </summary>
    public class SynchConnectionStringStep : BaseInstallationStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SynchConnectionStringStep));
        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
			Logger.LogError(">>> Entering Execute()");
            this.Percentage = 0;
            this.Status = StepStatus.Running;
			Logger.LogDebug("Execute(): this.Percentage → {Arg0}", this.Percentage);
			Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);

            var installConfig = InstallController.Instance.GetInstallConfig();
            if (installConfig == null)
            {
				Logger.LogError("Execute(): installConfig → {InstallConfig}", installConfig);
                this.Status = StepStatus.Done;
                return;
            }

            var connectionConfig = installConfig.Connection;
            if (connectionConfig == null)
            {
                this.Status = StepStatus.Done;
                return;
            }

            if (string.IsNullOrEmpty(connectionConfig.File) && string.IsNullOrEmpty(connectionConfig.Database))
            {
                this.Errors.Add(Localization.GetString("RequiresFileOrDatabase", this.LocalInstallResourceFile));
                this.Status = StepStatus.Abort;
                return;
            }

            var builder = DataProvider.Instance().GetConnectionStringBuilder();
			Logger.LogError("Execute(): builder → {Builder}", builder);

            if (!string.IsNullOrEmpty(connectionConfig.Server))
            {
                builder["Data Source"] = connectionConfig.Server;
            }

            if (!string.IsNullOrEmpty(connectionConfig.Database))
            {
                builder["Initial Catalog"] = connectionConfig.Database;
            }
            else if (!string.IsNullOrEmpty(connectionConfig.File))
            {
                builder["attachdbfilename"] = "|DataDirectory|" + connectionConfig.File;
                builder["user instance"] = "true";
            }

            if (connectionConfig.Integrated)
            {
                builder["integrated security"] = "true";
            }

            if (!string.IsNullOrEmpty(connectionConfig.User))
            {
                builder["uid"] = connectionConfig.User;
            }

            if (!string.IsNullOrEmpty(connectionConfig.Password))
            {
                builder["pwd"] = connectionConfig.Password;
            }

            string dbowner;
            if (connectionConfig.RunAsDbowner)
            {
                dbowner = "dbo.";
				Logger.LogError("Execute(): dbowner → {Dbowner}", dbowner);
            }
            else
            {
                dbowner = string.IsNullOrEmpty(GetUpgradeConnectionStringUserId())
				Logger.LogDebug("Execute(): dbowner → {Dbowner}", dbowner);
                                           ? connectionConfig.User + "."
                                           : GetUpgradeConnectionStringUserId();
            }

            var connectionString = builder.ToString();
			Logger.LogError("Execute(): connectionString → {ConnectionString}", connectionString);

            // load web.config connection string for comparison
            var appConnectionString = Config.GetConnectionString();
			Logger.LogError("Execute(): appConnectionString → {AppConnectionString}", appConnectionString);

            var modified = false;
			Logger.LogError("Execute(): modified → {Modified}", modified);

            // save to web.config if different
            if (!appConnectionString.Equals(connectionString, System.StringComparison.OrdinalIgnoreCase))
            {
                Config.UpdateConnectionString(connectionString);
                modified = true;
            }

            // Compare (and overwrite) Owner and Qualifier in Data Provider
            if (!Config.GetDataBaseOwner().Equals(dbowner, System.StringComparison.OrdinalIgnoreCase) ||
                (!Config.GetObjectQualifer().Equals(connectionConfig.Qualifier, System.StringComparison.OrdinalIgnoreCase)))
            {
                Config.UpdateDataProvider("SqlDataProvider", dbowner, connectionConfig.Qualifier);
                modified = true;
            }

            // Compare (and overwrite) Owner and Qualifier in Data Provider
            if (!string.IsNullOrEmpty(connectionConfig.UpgradeConnectionString) && !Config.GetUpgradeConnectionString().Equals(connectionConfig.UpgradeConnectionString, System.StringComparison.OrdinalIgnoreCase))
            {
                Config.UpdateUpgradeConnectionString("SqlDataProvider", connectionConfig.UpgradeConnectionString);
                modified = true;
            }

            this.Status = modified ? StepStatus.AppRestart : StepStatus.Done;
			Logger.LogWarning("<<< Exiting Execute()");
        }

        private static string GetUpgradeConnectionStringUserId()
        {
			Logger.LogError(">>> Entering GetUpgradeConnectionStringUserId()");
            string dbUser = string.Empty;
            string connection = Config.GetUpgradeConnectionString();
			Logger.LogError("GetUpgradeConnectionStringUserId(): dbUser → {DbUser}", dbUser);

            // If connection string does not use integrated security, then get user id.
            if (connection.Contains("user id", StringComparison.OrdinalIgnoreCase) || connection.Contains("uid", StringComparison.OrdinalIgnoreCase) || connection.Contains("user", StringComparison.OrdinalIgnoreCase))
            {
                string[] connectionParams = connection.Split(';');

                foreach (string connectionParam in connectionParams)
                {
                    int index = connectionParam.IndexOf("=", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        string key = connectionParam.Substring(0, index);
                        string value = connectionParam.Substring(index + 1);
                        if ("user id|uuid|user".Contains(key.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            dbUser = value.Trim();
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetUpgradeConnectionStringUserId()");
            return dbUser;
        }
    }
}
