// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using Microsoft.ApplicationBlocks.Data;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Base implementation of a provider of core database activities.</summary>
    public abstract partial class DataProvider
    {
        private const int DuplicateKey = 2601;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataProvider));
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="DataProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IA. Scheduled removal in v12.0.0.")]
        protected DataProvider()
            : this(null)
        {
		Logger.LogError(">>> Entering DataProvider()");
        Logger.LogWarning("<<< Exiting DataProvider()");
        }

        /// <summary>Initializes a new instance of the <see cref="DataProvider"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        protected DataProvider(IApplicationStatusInfo appStatus)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("DataProvider(appStatus={AppStatus}): appStatus → {AppStatus1}", appStatus, appStatus);
        Logger.LogWarning("<<< Exiting DataProvider(appStatus={AppStatus})", appStatus);
        }

        public virtual string ConnectionString
        {
            get
            {
                // Get Connection string from web.config
                string connectionString = Config.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Use connection string specified in provider
                    connectionString = this.Settings["connectionString"];
                }

                return connectionString;
            }
        }

        public virtual string DatabaseOwner
        {
            get
            {
                string databaseOwner = this.Settings["databaseOwner"];
                if (!string.IsNullOrEmpty(databaseOwner) && !databaseOwner.EndsWith(".", StringComparison.Ordinal))
                {
                    databaseOwner += ".";
                }

                return databaseOwner;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string DefaultProviderName => Instance().ProviderName;

        public abstract bool IsConnectionValid { get; }

        public virtual string ObjectQualifier
        {
            get
            {
                string objectQualifier = this.Settings["objectQualifier"];
                if (!string.IsNullOrEmpty(objectQualifier) && !objectQualifier.EndsWith("_", StringComparison.Ordinal))
                {
                    objectQualifier += "_";
                }

                return objectQualifier;
            }
        }

        public virtual string ProviderName
        {
            get { return this.Settings["providerName"]; }
        }

        public virtual string ProviderPath
        {
            get { return this.Settings["providerPath"]; }
        }

        public abstract Dictionary<string, string> Settings { get; }

        public virtual string UpgradeConnectionString
        {
            get
            {
                return !string.IsNullOrEmpty(this.Settings["upgradeConnectionString"])
                                        ? this.Settings["upgradeConnectionString"]
                                        : this.ConnectionString;
            }
        }

        public static DataProvider Instance()
        {
			Logger.LogError(">>> Entering Instance()");
            Logger.LogWarning("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<DataProvider>();
        }

        public abstract void ExecuteNonQuery(string procedureName, params object[] commandParameters);

        public abstract void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> commandParameters);

        public abstract IDataReader ExecuteReader(string procedureName, params object[] commandParameters);

        public abstract IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract IDataReader ExecuteSQL(string sql);

        public abstract IDataReader ExecuteSQL(string sql, int timeoutSec);

        public abstract string ExecuteScript(string script);

        public abstract string ExecuteScript(string script, int timeoutSec);

        public abstract string ExecuteScript(string connectionString, string sql);

        public abstract string ExecuteScript(string connectionString, string sql, int timeoutSec);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage);

        public virtual void CommitTransaction(DbTransaction transaction)
        {
			Logger.LogError(">>> Entering CommitTransaction(transaction={Transaction})", transaction);
            try
            {
                transaction.Commit();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
			Logger.LogWarning("<<< Exiting CommitTransaction(transaction={Transaction})", transaction);
        }

        public virtual DbTransaction GetTransaction()
        {
			Logger.LogError(">>> Entering GetTransaction()");
            var conn = new SqlConnection(this.UpgradeConnectionString);
            conn.Open();
			Logger.LogError("GetTransaction(): conn → {Conn}", conn);
            SqlTransaction transaction = conn.BeginTransaction();
			Logger.LogError("GetTransaction(): transaction → {Transaction}", transaction);
			Logger.LogWarning("<<< Exiting GetTransaction()");
            return transaction;
        }

        public virtual void RollbackTransaction(DbTransaction transaction)
        {
			Logger.LogError(">>> Entering RollbackTransaction(transaction={Transaction})", transaction);
            try
            {
                transaction.Rollback();
            }
            finally
            {
                if (transaction is { Connection: not null, })
                {
                    transaction.Connection.Close();
                }
            }
			Logger.LogWarning("<<< Exiting RollbackTransaction(transaction={Transaction})", transaction);
        }

        public virtual object GetNull(object field)
        {
			Logger.LogError(">>> Entering GetNull(field={Field})", field);
            Logger.LogWarning("<<< Exiting GetNull(field={Field})", field);
            return Null.GetNull(field, DBNull.Value, CultureInfo.CurrentCulture);
        }

        public virtual IDataReader FindDatabaseVersion(int major, int minor, int build)
        {
			Logger.LogError(">>> Entering FindDatabaseVersion(major={Major},minor={Minor},build={Build})", major, minor, build);
            Logger.LogWarning("<<< Exiting FindDatabaseVersion(major={Major},minor={Minor},build={Build})", major, minor, build);
            return this.ExecuteReader("FindDatabaseVersion", major, minor, build);
        }

        public virtual Version GetDatabaseEngineVersion()
        {
			Logger.LogError(">>> Entering GetDatabaseEngineVersion()");
            string version = "0.0";
            IDataReader dr = null;
			Logger.LogError("GetDatabaseEngineVersion(): version → {Version}", version);
            try
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("GetDatabaseEngineVersion(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                dr = this.ExecuteReader("GetDatabaseServer");
                if (dr.Read())
                {
					Logger.LogError("GetDatabaseEngineVersion(): dr → {Dr}", dr);
                    version = dr["Version"].ToString();
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetDatabaseEngineVersion()");
            return new Version(version);
        }

        public virtual IDataReader GetDatabaseVersion()
        {
			Logger.LogError(">>> Entering GetDatabaseVersion()");
            Logger.LogWarning("<<< Exiting GetDatabaseVersion()");
            return this.ExecuteReader("GetDatabaseVersion");
        }

        public virtual IDataReader GetDatabaseInstallVersion()
        {
			Logger.LogError(">>> Entering GetDatabaseInstallVersion()");
            Logger.LogWarning("<<< Exiting GetDatabaseInstallVersion()");
            return this.ExecuteReader("GetDatabaseInstallVersion");
        }

        public virtual Version GetVersion()
        {
			Logger.LogError(">>> Entering GetVersion()");
            Logger.LogWarning("<<< Exiting GetVersion()");
            return this.GetVersionInternal(true);
        }

        public virtual Version GetInstallVersion()
        {
			Logger.LogError(">>> Entering GetInstallVersion()");
            Logger.LogWarning("<<< Exiting GetInstallVersion()");
            return this.GetVersionInternal(false);
        }

        public virtual DbConnectionStringBuilder GetConnectionStringBuilder()
        {
			Logger.LogError(">>> Entering GetConnectionStringBuilder()");
            Logger.LogWarning("<<< Exiting GetConnectionStringBuilder()");
            return new SqlConnectionStringBuilder();
        }

        public virtual string GetProviderPath()
        {
			Logger.LogError(">>> Entering GetProviderPath()");
            string path = this.ProviderPath;
			Logger.LogError("GetProviderPath(): path → {Path}", path);
            if (!string.IsNullOrEmpty(path))
            {
                path = HostingEnvironment.MapPath(path);

                // ReSharper disable once AssignNullToNotNullAttribute
                if (Directory.Exists(path))
                {
                    if (!this.IsConnectionValid)
                    {
                        path = "ERROR: Could not connect to database specified in connectionString for SqlDataProvider";
                    }
                }
                else
                {
                    path = "ERROR: providerPath folder " + path +
                           " specified for SqlDataProvider does not exist on web server";
                }
            }
            else
            {
                path = "ERROR: providerPath folder value not specified in web.config for SqlDataProvider";
            }

			Logger.LogWarning("<<< Exiting GetProviderPath()");
            return path;
        }

        /// <summary>Tests the Database Connection using the database connection config.</summary>
        /// <param name="builder">The <see cref="SqlConnectionStringBuilder"/>.</param>
        /// <param name="owner">The database owner/schema.</param>
        /// <param name="qualifier">The object qualifier.</param>
        /// <returns>The connection string, or an error message (prefixed with <c>"ERROR:"</c>), or <see cref="Null.NullString"/> if <paramref name="builder"/> is <see langword="null"/>.</returns>
        public virtual string TestDatabaseConnection(DbConnectionStringBuilder builder, string owner, string qualifier)
        {
			Logger.LogError(">>> Entering TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier})", builder, owner, qualifier);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var sqlBuilder = builder as SqlConnectionStringBuilder;
            string connectionString = Null.NullString;
			Logger.LogError("TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): connectionString → {ConnectionString}", builder, owner, qualifier, connectionString);
            if (sqlBuilder != null)
            {
                connectionString = sqlBuilder.ToString();
                IDataReader dr = null;
				Logger.LogError("TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): dr → {Dr}", builder, owner, qualifier, dr);
                try
                {
                    dr = PetaPocoHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, owner + qualifier + "GetDatabaseVersion");
                }
                catch (SqlException ex)
                {
					Logger.LogError("TestDatabaseConnection(builder,owner,qualifier)={Builder},{Owner},{Qualifier}: database query executed in {Arg3} ms", builder, owner, qualifier, __logStart.ElapsedMilliseconds);
                    const string message = "ERROR:";
					Logger.LogError("Exception in TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): {Arg3}", builder, owner, qualifier, ex.Message);
                    bool bError = true;
                    int i;
					Logger.LogError("TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): bError → {BError}", builder, owner, qualifier, bError);
                    var errorMessages = new StringBuilder();
                    for (i = 0; i <= ex.Errors.Count - 1; i++)
                    {
						Logger.LogError("TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): errorMessages → {ErrorMessages}", builder, owner, qualifier, errorMessages);
                        SqlError sqlError = ex.Errors[i];
                        if (sqlError.Number == 2812 && sqlError.Class == 16)
                        {
                            bError = false;
                            break;
                        }

                        string filteredMessage = string.Empty;
						Logger.LogError("TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier}): filteredMessage → {FilteredMessage}", builder, owner, qualifier, filteredMessage);
                        switch (sqlError.Number)
                        {
                            case 17:
                                filteredMessage = "Sql server does not exist or access denied";
                                break;
                            case 4060:
                                filteredMessage = "Invalid Database";
                                break;
                            case 18456:
                                filteredMessage = "Sql login failed";
                                break;
                            case 1205:
                                filteredMessage = "Sql deadlock victim";
                                break;
                        }

                        errorMessages.Append("<b>Index #:</b> " + i + "<br/>" + "<b>Source:</b> " + sqlError.Source +
                                             "<br/>" + "<b>Class:</b> " + sqlError.Class + "<br/>" + "<b>Number:</b> " +
                                             sqlError.Number + "<br/>" + "<b>Message:</b> " + filteredMessage +
                                             "<br/><br/>");
                    }

                    if (bError)
                    {
                        connectionString = message + errorMessages;
                    }
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
            }

			Logger.LogWarning("<<< Exiting TestDatabaseConnection(builder={Builder},owner={Owner},qualifier={Qualifier})", builder, owner, qualifier);
            return connectionString;
        }

        public virtual void UpdateDatabaseVersion(int major, int minor, int build, string name)
        {
			Logger.LogError(">>> Entering UpdateDatabaseVersion(major={Major},minor={Minor},build={Build},name={Name})", major, minor, build, name);
            if (major >= 5 || (major == 4 && minor == 9 && build > 0))
            {
                // If the version > 4.9.0 use the new sproc, which is added in 4.9.1 script
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("UpdateDatabaseVersion(major,minor,build,name)={Major},{Minor},{Build},{Name}: database query executed in {Arg4} ms", major, minor, build, name, __logStart.ElapsedMilliseconds);
                this.ExecuteNonQuery("UpdateDatabaseVersionAndName", major, minor, build, name);
            }
            else
            {
                this.ExecuteNonQuery("UpdateDatabaseVersion", major, minor, build);
            }
			Logger.LogWarning("<<< Exiting UpdateDatabaseVersion(major={Major},minor={Minor},build={Build},name={Name})", major, minor, build, name);
        }

        public virtual void UpdateDatabaseVersionIncrement(int major, int minor, int build, int increment, string appName)
        {
			Logger.LogError(">>> Entering UpdateDatabaseVersionIncrement(major={Major},minor={Minor},build={Build},increment={Increment},appName={AppName})", major, minor, build, increment, appName);
            this.ExecuteNonQuery("UpdateDatabaseVersionIncrement", major, minor, build, increment, appName);
			Logger.LogWarning("<<< Exiting UpdateDatabaseVersionIncrement(major={Major},minor={Minor},build={Build},increment={Increment},appName={AppName})", major, minor, build, increment, appName);
        }

        public virtual int GetLastAppliedIteration(int major, int minor, int build)
        {
			Logger.LogError(">>> Entering GetLastAppliedIteration(major={Major},minor={Minor},build={Build})", major, minor, build);
            Logger.LogWarning("<<< Exiting GetLastAppliedIteration(major={Major},minor={Minor},build={Build})", major, minor, build);
            return this.ExecuteScalar<int>("GetLastAppliedIteration", major, minor, build);
        }

        public virtual string GetUnappliedIterations(string version)
        {
			Logger.LogError(">>> Entering GetUnappliedIterations(version={Version})", version);
            Logger.LogWarning("<<< Exiting GetUnappliedIterations(version={Version})", version);
            return this.ExecuteScalar<string>("GetUnappliedIterations", version);
        }

        public virtual IDataReader GetHostSetting(string settingName)
        {
			Logger.LogError(">>> Entering GetHostSetting(settingName={SettingName})", settingName);
            Logger.LogWarning("<<< Exiting GetHostSetting(settingName={SettingName})", settingName);
            return this.ExecuteReader("GetHostSetting", settingName);
        }

        public virtual IDataReader GetHostSettings()
        {
			Logger.LogError(">>> Entering GetHostSettings()");
            Logger.LogWarning("<<< Exiting GetHostSettings()");
            return this.ExecuteReader("GetHostSettings");
        }

        public virtual void UpdateHostSetting(string settingName, string settingValue, bool settingIsSecure, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateHostSetting(settingName={SettingName},settingValue={SettingValue},settingIsSecure={SettingIsSecure},lastModifiedByUserID={LastModifiedByUserID})", settingName, settingValue, settingIsSecure, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateHostSetting", settingName, settingValue, settingIsSecure, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateHostSetting(settingName={SettingName},settingValue={SettingValue},settingIsSecure={SettingIsSecure},lastModifiedByUserID={LastModifiedByUserID})", settingName, settingValue, settingIsSecure, lastModifiedByUserID);
        }

        public virtual void DeleteServer(int serverId)
        {
			Logger.LogError(">>> Entering DeleteServer(serverId={ServerId})", serverId);
            this.ExecuteNonQuery("DeleteServer", serverId);
			Logger.LogWarning("<<< Exiting DeleteServer(serverId={ServerId})", serverId);
        }

        public virtual IDataReader GetServers()
        {
			Logger.LogError(">>> Entering GetServers()");
            Logger.LogWarning("<<< Exiting GetServers()");
            return this.ExecuteReader("GetServers");
        }

        public virtual void UpdateServer(int serverId, string url, string uniqueId, bool enabled, string group)
        {
			Logger.LogError(">>> Entering UpdateServer(serverId={ServerId},url={Url},uniqueId={UniqueId},enabled={Enabled},group={Group})", serverId, url, uniqueId, enabled, group);
            this.ExecuteNonQuery("UpdateServer", serverId, url, uniqueId, enabled, group);
			Logger.LogWarning("<<< Exiting UpdateServer(serverId={ServerId},url={Url},uniqueId={UniqueId},enabled={Enabled},group={Group})", serverId, url, uniqueId, enabled, group);
        }

        public virtual int UpdateServerActivity(string serverName, string iisAppName, DateTime createdDate, DateTime lastActivityDate, int pingFailureCount, bool enabled)
        {
			Logger.LogError(">>> Entering UpdateServerActivity(serverName={ServerName},iisAppName={IisAppName},createdDate={CreatedDate},lastActivityDate={LastActivityDate},pingFailureCount=<redacted>,enabled={Enabled})", serverName, iisAppName, createdDate, lastActivityDate, enabled);
            Logger.LogWarning("<<< Exiting UpdateServerActivity(serverName={ServerName},iisAppName={IisAppName},createdDate={CreatedDate},lastActivityDate={LastActivityDate},pingFailureCount=<redacted>,enabled={Enabled})", serverName, iisAppName, createdDate, lastActivityDate, enabled);
            return this.ExecuteScalar<int>("UpdateServerActivity", serverName, iisAppName, createdDate, lastActivityDate, pingFailureCount, enabled);
        }

        public virtual int CreatePortal(string portalname, string currency, DateTime expiryDate, double hostFee, double hostSpace, int pageQuota, int userQuota, int siteLogHistory, string homeDirectory, string cultureCode, int createdByUserID)
        {
			Logger.LogError(">>> Entering CreatePortal(portalname={Portalname},currency={Currency},expiryDate={ExpiryDate},hostFee={HostFee},hostSpace={HostSpace},pageQuota={PageQuota},userQuota={UserQuota},siteLogHistory={SiteLogHistory},homeDirectory={HomeDirectory},cultureCode={CultureCode},createdByUserID={CreatedByUserID})", portalname, currency, expiryDate, hostFee, hostSpace, pageQuota, userQuota, siteLogHistory, homeDirectory, cultureCode, createdByUserID);
				Logger.LogWarning("<<< Exiting CreatePortal(portalname={Portalname},currency={Currency},expiryDate={ExpiryDate},hostFee={HostFee},hostSpace={HostSpace},pageQuota={PageQuota},userQuota={UserQuota},siteLogHistory={SiteLogHistory},homeDirectory={HomeDirectory},cultureCode={CultureCode},createdByUserID={CreatedByUserID})", portalname, currency, expiryDate, hostFee, hostSpace, pageQuota, userQuota, siteLogHistory, homeDirectory, cultureCode, createdByUserID);
            return
                this.ExecuteScalar<int>(
                    "AddPortalInfo",
#pragma warning disable CS0618 // Type or member is obsolete
                    PortalSecurity.Instance.InputFilter(portalname, PortalSecurity.FilterFlag.NoMarkup),
#pragma warning restore CS0618 // Type or member is obsolete
                    currency,
                    this.GetNull(expiryDate),
                    hostFee,
                    hostSpace,
                    pageQuota,
                    userQuota,
                    this.GetNull(siteLogHistory),
                    homeDirectory,
                    cultureCode,
                    createdByUserID);
        }

        public virtual void DeletePortalInfo(int portalId)
        {
			Logger.LogError(">>> Entering DeletePortalInfo(portalId={PortalId})", portalId);
            this.ExecuteNonQuery("DeletePortalInfo", portalId);
			Logger.LogWarning("<<< Exiting DeletePortalInfo(portalId={PortalId})", portalId);
        }

        public virtual void DeletePortalSetting(int portalId, string settingName, string cultureCode)
        {
			Logger.LogError(">>> Entering DeletePortalSetting(portalId={PortalId},settingName={SettingName},cultureCode={CultureCode})", portalId, settingName, cultureCode);
            this.ExecuteNonQuery("DeletePortalSetting", portalId, settingName, cultureCode);
			Logger.LogWarning("<<< Exiting DeletePortalSetting(portalId={PortalId},settingName={SettingName},cultureCode={CultureCode})", portalId, settingName, cultureCode);
        }

        public virtual void DeletePortalSettings(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeletePortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            this.ExecuteNonQuery("DeletePortalSettings", portalId, cultureCode);
			Logger.LogWarning("<<< Exiting DeletePortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
        }

        public virtual IDataReader GetExpiredPortals()
        {
			Logger.LogError(">>> Entering GetExpiredPortals()");
            Logger.LogWarning("<<< Exiting GetExpiredPortals()");
            return this.ExecuteReader("GetExpiredPortals");
        }

        public virtual IDataReader GetPortals(string cultureCode)
        {
			Logger.LogError(">>> Entering GetPortals(cultureCode={CultureCode})", cultureCode);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            IDataReader reader;
            if (this.appStatus.Status == UpgradeStatus.Upgrade && this.appStatus.DatabaseVersion < new Version(6, 1, 0))
            {
                reader = this.ExecuteReader("GetPortals");
            }
            else
            {
				Logger.LogError("GetPortals(cultureCode)={CultureCode}: database query executed in {Arg1} ms", cultureCode, __logStart.ElapsedMilliseconds);
                reader = this.ExecuteReader("GetPortals", cultureCode);
				Logger.LogError("GetPortals(cultureCode={CultureCode}): reader → {Reader}", cultureCode, reader);
            }

			Logger.LogDebug("GetPortals(cultureCode={CultureCode}): reader → {Reader}", cultureCode, reader);
			Logger.LogWarning("<<< Exiting GetPortals(cultureCode={CultureCode})", cultureCode);
            return reader;
        }

        public virtual IDataReader GetAllPortals()
        {
			Logger.LogError(">>> Entering GetAllPortals()");
            Logger.LogWarning("<<< Exiting GetAllPortals()");
            return this.ExecuteReader("GetAllPortals");
        }

        public virtual IDataReader GetPortalsByName(string nameToMatch, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetPortalsByName(nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize})", nameToMatch, pageIndex, pageSize);
            Logger.LogWarning("<<< Exiting GetPortalsByName(nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize})", nameToMatch, pageIndex, pageSize);
            return this.ExecuteReader("GetPortalsByName", nameToMatch, pageIndex, pageSize);
        }

        public virtual IDataReader GetPortalsByUser(int userId)
        {
			Logger.LogError(">>> Entering GetPortalsByUser(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetPortalsByUser(userId={UserId})", userId);
            return this.ExecuteReader("GetPortalsByUser", userId);
        }

        public virtual IDataReader GetPortalSettings(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetPortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            Logger.LogWarning("<<< Exiting GetPortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.ExecuteReader("GetPortalSettings", portalId, cultureCode);
        }

        public virtual IDataReader GetPortalSpaceUsed(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalSpaceUsed(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPortalSpaceUsed(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetPortalSpaceUsed", this.GetNull(portalId));
        }

        /// <summary>Updates the portal information.Saving basic portal settings at Admin - Site settings / Host - Portals - Edit Portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="portalGroupId">The portal group ID or <see cref="Null.NullInteger"/>.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="logoFile">The logo file.</param>
        /// <param name="footerText">The footer text.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="userRegistration">The user registration.</param>
        /// <param name="bannerAdvertising">The banner advertising.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="administratorId">The ID of the administrator user.</param>
        /// <param name="hostFee">The host fee.</param>
        /// <param name="hostSpace">The host space.</param>
        /// <param name="pageQuota">The page quota.</param>
        /// <param name="userQuota">The user quota.</param>
        /// <param name="paymentProcessor">The payment processor.</param>
        /// <param name="processorUserId">The processor user ID.</param>
        /// <param name="processorPassword">The processor password.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="backgroundFile">The background file.</param>
        /// <param name="siteLogHistory">The site log history.</param>
        /// <param name="splashTabId">The ID of the splash tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="homeTabId">The ID of the home tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="loginTabId">The ID of the login tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="registerTabId">The ID of the register tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="userTabId">The ID of the user tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="searchTabId">The ID of the search tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="custom404TabId">The ID of the 404 error tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="custom500TabId">The ID of the 500 error tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="termsTabId">The ID of the terms tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="privacyTabId">The ID of the privacy tab, or <see cref="Null.NullInteger"/>.</param>
        /// <param name="defaultLanguage">The default language.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="lastModifiedByUserID">The ID of the user that last modified the portal.</param>
        /// <param name="cultureCode">The culture code.</param>
        public virtual void UpdatePortalInfo(int portalId, int portalGroupId, string portalName, string logoFile, string footerText, DateTime expiryDate, int userRegistration, int bannerAdvertising, string currency, int administratorId, double hostFee, double hostSpace, int pageQuota, int userQuota, string paymentProcessor, string processorUserId, string processorPassword, string description, string keyWords, string backgroundFile, int siteLogHistory, int splashTabId, int homeTabId, int loginTabId, int registerTabId, int userTabId, int searchTabId, int custom404TabId, int custom500TabId, int termsTabId, int privacyTabId, string defaultLanguage, string homeDirectory, int lastModifiedByUserID, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalInfo(portalId={PortalId},portalGroupId={PortalGroupId},portalName={PortalName},logoFile={LogoFile},footerText={FooterText},expiryDate={ExpiryDate},userRegistration={UserRegistration},bannerAdvertising={BannerAdvertising},currency={Currency},administratorId={AdministratorId},hostFee={HostFee},hostSpace={HostSpace},pageQuota={PageQuota},userQuota={UserQuota},paymentProcessor={PaymentProcessor},processorUserId={ProcessorUserId},processorPassword=<redacted>,description={Description},keyWords={KeyWords},backgroundFile={BackgroundFile},siteLogHistory={SiteLogHistory},splashTabId={SplashTabId},homeTabId={HomeTabId},loginTabId={LoginTabId},registerTabId={RegisterTabId},userTabId={UserTabId},searchTabId={SearchTabId},custom404TabId={Custom404TabId},custom500TabId={Custom500TabId},termsTabId={TermsTabId},privacyTabId={PrivacyTabId},defaultLanguage={DefaultLanguage},homeDirectory={HomeDirectory},lastModifiedByUserID={LastModifiedByUserID},cultureCode={CultureCode})", portalId, portalGroupId, portalName, logoFile, footerText, expiryDate, userRegistration, bannerAdvertising, currency, administratorId, hostFee, hostSpace, pageQuota, userQuota, paymentProcessor, processorUserId, description, keyWords, backgroundFile, siteLogHistory, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, defaultLanguage, homeDirectory, lastModifiedByUserID, cultureCode);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdatePortalInfo",
                portalId,
			Logger.LogError("UpdatePortalInfo(portalId,portalGroupId,portalName,logoFile,footerText,expiryDate,userRegistration,bannerAdvertising,currency,administratorId,hostFee,hostSpace,pageQuota,userQuota,paymentProcessor,processorUserId,processorPassword,description,keyWords,backgroundFile,siteLogHistory,splashTabId,homeTabId,loginTabId,registerTabId,userTabId,searchTabId,custom404TabId,custom500TabId,termsTabId,privacyTabId,defaultLanguage,homeDirectory,lastModifiedByUserID,cultureCode)={PortalId},{PortalGroupId},{PortalName},{LogoFile},{FooterText},{ExpiryDate},{UserRegistration},{BannerAdvertising},{Currency},{AdministratorId},{HostFee},{HostSpace},{PageQuota},{UserQuota},{PaymentProcessor},{ProcessorUserId},<redacted>,{Description},{KeyWords},{BackgroundFile},{SiteLogHistory},{SplashTabId},{HomeTabId},{LoginTabId},{RegisterTabId},{UserTabId},{SearchTabId},{Custom404TabId},{Custom500TabId},{TermsTabId},{PrivacyTabId},{DefaultLanguage},{HomeDirectory},{LastModifiedByUserID},{CultureCode}: database query executed in {Arg35} ms", portalId, portalGroupId, portalName, logoFile, footerText, expiryDate, userRegistration, bannerAdvertising, currency, administratorId, hostFee, hostSpace, pageQuota, userQuota, paymentProcessor, processorUserId, description, keyWords, backgroundFile, siteLogHistory, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, defaultLanguage, homeDirectory, lastModifiedByUserID, cultureCode, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdatePortalInfo(portalId={PortalId},portalGroupId={PortalGroupId},portalName={PortalName},logoFile={LogoFile},footerText={FooterText},expiryDate={ExpiryDate},userRegistration={UserRegistration},bannerAdvertising={BannerAdvertising},currency={Currency},administratorId={AdministratorId},hostFee={HostFee},hostSpace={HostSpace},pageQuota={PageQuota},userQuota={UserQuota},paymentProcessor={PaymentProcessor},processorUserId={ProcessorUserId},processorPassword=<redacted>,description={Description},keyWords={KeyWords},backgroundFile={BackgroundFile},siteLogHistory={SiteLogHistory},splashTabId={SplashTabId},homeTabId={HomeTabId},loginTabId={LoginTabId},registerTabId={RegisterTabId},userTabId={UserTabId},searchTabId={SearchTabId},custom404TabId={Custom404TabId},custom500TabId={Custom500TabId},termsTabId={TermsTabId},privacyTabId={PrivacyTabId},defaultLanguage={DefaultLanguage},homeDirectory={HomeDirectory},lastModifiedByUserID={LastModifiedByUserID},cultureCode={CultureCode})", portalId, portalGroupId, portalName, logoFile, footerText, expiryDate, userRegistration, bannerAdvertising, currency, administratorId, hostFee, hostSpace, pageQuota, userQuota, paymentProcessor, processorUserId, description, keyWords, backgroundFile, siteLogHistory, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, defaultLanguage, homeDirectory, lastModifiedByUserID, cultureCode);
                portalGroupId,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.Instance.InputFilter(portalName, PortalSecurity.FilterFlag.NoMarkup),
#pragma warning restore CS0618 // Type or member is obsolete
                this.GetNull(logoFile),
                this.GetNull(footerText),
                this.GetNull(expiryDate),
                userRegistration,
                bannerAdvertising,
                currency,
                this.GetNull(administratorId),
                hostFee,
                hostSpace,
                pageQuota,
                userQuota,
                this.GetNull(paymentProcessor),
                this.GetNull(processorUserId),
                this.GetNull(processorPassword),
                this.GetNull(description),
                this.GetNull(keyWords),
                this.GetNull(backgroundFile),
                this.GetNull(siteLogHistory),
                this.GetNull(splashTabId),
                this.GetNull(homeTabId),
                this.GetNull(loginTabId),
                this.GetNull(registerTabId),
                this.GetNull(userTabId),
                this.GetNull(searchTabId),
                this.GetNull(custom404TabId),
                this.GetNull(custom500TabId),
                this.GetNull(termsTabId),
                this.GetNull(privacyTabId),
                this.GetNull(defaultLanguage),
                homeDirectory,
                lastModifiedByUserID,
                cultureCode);
        }

        public virtual void UpdatePortalSetting(int portalId, string settingName, string settingValue, int userId, string cultureCode, bool isSecure)
        {
			Logger.LogError(">>> Entering UpdatePortalSetting(portalId={PortalId},settingName={SettingName},settingValue={SettingValue},userId={UserId},cultureCode={CultureCode},isSecure={IsSecure})", portalId, settingName, settingValue, userId, cultureCode, isSecure);
            this.ExecuteNonQuery("UpdatePortalSetting", portalId, settingName, settingValue, userId, cultureCode, isSecure);
			Logger.LogWarning("<<< Exiting UpdatePortalSetting(portalId={PortalId},settingName={SettingName},settingValue={SettingValue},userId={UserId},cultureCode={CultureCode},isSecure={IsSecure})", portalId, settingName, settingValue, userId, cultureCode, isSecure);
        }

        public virtual void UpdatePortalSetup(int portalId, int administratorId, int administratorRoleId, int registeredRoleId, int splashTabId, int homeTabId, int loginTabId, int registerTabId, int userTabId, int searchTabId, int custom404TabId, int custom500TabId, int termsTabId, int privacyTabId, int adminTabId, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalSetup(portalId={PortalId},administratorId={AdministratorId},administratorRoleId={AdministratorRoleId},registeredRoleId={RegisteredRoleId},splashTabId={SplashTabId},homeTabId={HomeTabId},loginTabId={LoginTabId},registerTabId={RegisterTabId},userTabId={UserTabId},searchTabId={SearchTabId},custom404TabId={Custom404TabId},custom500TabId={Custom500TabId},termsTabId={TermsTabId},privacyTabId={PrivacyTabId},adminTabId={AdminTabId},cultureCode={CultureCode})", portalId, administratorId, administratorRoleId, registeredRoleId, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, adminTabId, cultureCode);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdatePortalSetup",
                portalId,
                administratorId,
                administratorRoleId,
                registeredRoleId,
                splashTabId,
                homeTabId,
                loginTabId,
                registerTabId,
                userTabId,
                searchTabId,
                custom404TabId,
                custom500TabId,
                termsTabId,
                privacyTabId,
                adminTabId,
                cultureCode);
				Logger.LogError("UpdatePortalSetup(portalId,administratorId,administratorRoleId,registeredRoleId,splashTabId,homeTabId,loginTabId,registerTabId,userTabId,searchTabId,custom404TabId,custom500TabId,termsTabId,privacyTabId,adminTabId,cultureCode)={PortalId},{AdministratorId},{AdministratorRoleId},{RegisteredRoleId},{SplashTabId},{HomeTabId},{LoginTabId},{RegisterTabId},{UserTabId},{SearchTabId},{Custom404TabId},{Custom500TabId},{TermsTabId},{PrivacyTabId},{AdminTabId},{CultureCode}: database query executed in {Arg16} ms", portalId, administratorId, administratorRoleId, registeredRoleId, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, adminTabId, cultureCode, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdatePortalSetup(portalId={PortalId},administratorId={AdministratorId},administratorRoleId={AdministratorRoleId},registeredRoleId={RegisteredRoleId},splashTabId={SplashTabId},homeTabId={HomeTabId},loginTabId={LoginTabId},registerTabId={RegisterTabId},userTabId={UserTabId},searchTabId={SearchTabId},custom404TabId={Custom404TabId},custom500TabId={Custom500TabId},termsTabId={TermsTabId},privacyTabId={PrivacyTabId},adminTabId={AdminTabId},cultureCode={CultureCode})", portalId, administratorId, administratorRoleId, registeredRoleId, splashTabId, homeTabId, loginTabId, registerTabId, userTabId, searchTabId, custom404TabId, custom500TabId, termsTabId, privacyTabId, adminTabId, cultureCode);
        }

        public virtual int AddTabAfter(TabInfo tab, int afterTabId, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddTabAfter(tab={Tab},afterTabId={AfterTabId},createdByUserID={CreatedByUserID})", tab, afterTabId, createdByUserID);
			Logger.LogWarning("<<< Exiting AddTabAfter(tab={Tab},afterTabId={AfterTabId},createdByUserID={CreatedByUserID})", tab, afterTabId, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddTabAfter",
                afterTabId,
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual int AddTabBefore(TabInfo tab, int beforeTabId, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddTabBefore(tab={Tab},beforeTabId={BeforeTabId},createdByUserID={CreatedByUserID})", tab, beforeTabId, createdByUserID);
			Logger.LogWarning("<<< Exiting AddTabBefore(tab={Tab},beforeTabId={BeforeTabId},createdByUserID={CreatedByUserID})", tab, beforeTabId, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddTabBefore",
                beforeTabId,
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual int AddTabToEnd(TabInfo tab, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddTabToEnd(tab={Tab},createdByUserID={CreatedByUserID})", tab, createdByUserID);
			Logger.LogWarning("<<< Exiting AddTabToEnd(tab={Tab},createdByUserID={CreatedByUserID})", tab, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddTabToEnd",
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual void DeleteTab(int tabId)
        {
			Logger.LogError(">>> Entering DeleteTab(tabId={TabId})", tabId);
            this.ExecuteNonQuery("DeleteTab", tabId);
			Logger.LogWarning("<<< Exiting DeleteTab(tabId={TabId})", tabId);
        }

        public virtual void DeleteTabSetting(int tabId, string settingName)
        {
			Logger.LogError(">>> Entering DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
            this.ExecuteNonQuery("DeleteTabSetting", tabId, settingName);
			Logger.LogWarning("<<< Exiting DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
        }

        public virtual void DeleteTabSettings(int tabId)
        {
			Logger.LogError(">>> Entering DeleteTabSettings(tabId={TabId})", tabId);
            this.ExecuteNonQuery("DeleteTabSettings", tabId);
			Logger.LogWarning("<<< Exiting DeleteTabSettings(tabId={TabId})", tabId);
        }

        public virtual void DeleteTabUrl(int tabId, int seqNum)
        {
			Logger.LogError(">>> Entering DeleteTabUrl(tabId={TabId},seqNum={SeqNum})", tabId, seqNum);
            this.ExecuteNonQuery("DeleteTabUrl", tabId, seqNum);
			Logger.LogWarning("<<< Exiting DeleteTabUrl(tabId={TabId},seqNum={SeqNum})", tabId, seqNum);
        }

        public virtual void DeleteTabVersion(int tabVersionId)
        {
			Logger.LogError(">>> Entering DeleteTabVersion(tabVersionId={TabVersionId})", tabVersionId);
            this.ExecuteNonQuery("DeleteTabVersion", tabVersionId);
			Logger.LogWarning("<<< Exiting DeleteTabVersion(tabVersionId={TabVersionId})", tabVersionId);
        }

        public virtual void DeleteTabVersionDetail(int tabVersionDetailId)
        {
			Logger.LogError(">>> Entering DeleteTabVersionDetail(tabVersionDetailId={TabVersionDetailId})", tabVersionDetailId);
            this.ExecuteNonQuery("DeleteTabVersionDetail", tabVersionDetailId);
			Logger.LogWarning("<<< Exiting DeleteTabVersionDetail(tabVersionDetailId={TabVersionDetailId})", tabVersionDetailId);
        }

        public virtual void DeleteTabVersionDetailByModule(int moduleId)
        {
			Logger.LogError(">>> Entering DeleteTabVersionDetailByModule(moduleId={ModuleId})", moduleId);
            this.ExecuteNonQuery("DeleteTabVersionDetailByModule", moduleId);
			Logger.LogWarning("<<< Exiting DeleteTabVersionDetailByModule(moduleId={ModuleId})", moduleId);
        }

        public virtual void DeleteTranslatedTabs(int tabId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeleteTranslatedTabs(tabId={TabId},cultureCode={CultureCode})", tabId, cultureCode);
            this.ExecuteNonQuery("DeleteTranslatedTabs", tabId, cultureCode);
			Logger.LogWarning("<<< Exiting DeleteTranslatedTabs(tabId={TabId},cultureCode={CultureCode})", tabId, cultureCode);
        }

        public virtual void EnsureNeutralLanguage(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering EnsureNeutralLanguage(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            this.ExecuteNonQuery("EnsureNeutralLanguage", portalId, cultureCode);
			Logger.LogWarning("<<< Exiting EnsureNeutralLanguage(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
        }

        public virtual void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode)
        {
			Logger.LogError(">>> Entering ConvertTabToNeutralLanguage(portalId={PortalId},tabId={TabId},cultureCode={CultureCode})", portalId, tabId, cultureCode);
            this.ExecuteNonQuery("ConvertTabToNeutralLanguage", portalId, tabId, cultureCode);
			Logger.LogWarning("<<< Exiting ConvertTabToNeutralLanguage(portalId={PortalId},tabId={TabId},cultureCode={CultureCode})", portalId, tabId, cultureCode);
        }

        public virtual IDataReader GetAllTabs()
        {
			Logger.LogError(">>> Entering GetAllTabs()");
            Logger.LogWarning("<<< Exiting GetAllTabs()");
            return this.ExecuteReader("GetAllTabs");
        }

        public virtual IDataReader GetTab(int tabId)
        {
			Logger.LogError(">>> Entering GetTab(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetTab(tabId={TabId})", tabId);
            return this.ExecuteReader("GetTab", tabId);
        }

        public virtual IDataReader GetTabByUniqueID(Guid uniqueId)
        {
			Logger.LogError(">>> Entering GetTabByUniqueID(uniqueId={UniqueId})", uniqueId);
            Logger.LogWarning("<<< Exiting GetTabByUniqueID(uniqueId={UniqueId})", uniqueId);
            return this.ExecuteReader("GetTabByUniqueID", uniqueId);
        }

        public virtual IDataReader GetTabPanes(int tabId)
        {
			Logger.LogError(">>> Entering GetTabPanes(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetTabPanes(tabId={TabId})", tabId);
            return this.ExecuteReader("GetTabPanes", tabId);
        }

        public virtual IDataReader GetTabPaths(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetTabPaths(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            Logger.LogWarning("<<< Exiting GetTabPaths(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.ExecuteReader("GetTabPaths", this.GetNull(portalId), cultureCode);
        }

        public virtual IDataReader GetTabs(int portalId)
        {
			Logger.LogError(">>> Entering GetTabs(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabs(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabs", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabsByModuleID(int moduleID)
        {
			Logger.LogError(">>> Entering GetTabsByModuleID(moduleID={ModuleID})", moduleID);
            Logger.LogWarning("<<< Exiting GetTabsByModuleID(moduleID={ModuleID})", moduleID);
            return this.ExecuteReader("GetTabsByModuleID", moduleID);
        }

        public virtual IDataReader GetTabsByTabModuleID(int tabModuleID)
        {
			Logger.LogError(">>> Entering GetTabsByTabModuleID(tabModuleID={TabModuleID})", tabModuleID);
            Logger.LogWarning("<<< Exiting GetTabsByTabModuleID(tabModuleID={TabModuleID})", tabModuleID);
            return this.ExecuteReader("GetTabsByTabModuleID", tabModuleID);
        }

        public virtual IDataReader GetTabsByPackageID(int portalID, int packageID, bool forHost)
        {
			Logger.LogError(">>> Entering GetTabsByPackageID(portalID={PortalID},packageID={PackageID},forHost={ForHost})", portalID, packageID, forHost);
            Logger.LogWarning("<<< Exiting GetTabsByPackageID(portalID={PortalID},packageID={PackageID},forHost={ForHost})", portalID, packageID, forHost);
            return this.ExecuteReader("GetTabsByPackageID", this.GetNull(portalID), packageID, forHost);
        }

        public virtual IDataReader GetTabSetting(int tabID, string settingName)
        {
			Logger.LogError(">>> Entering GetTabSetting(tabID={TabID},settingName={SettingName})", tabID, settingName);
            Logger.LogWarning("<<< Exiting GetTabSetting(tabID={TabID},settingName={SettingName})", tabID, settingName);
            return this.ExecuteReader("GetTabSetting", tabID, settingName);
        }

        public virtual IDataReader GetTabSettings(int portalId)
        {
			Logger.LogError(">>> Entering GetTabSettings(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabSettings(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabSettings", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabAliasSkins(int portalId)
        {
			Logger.LogError(">>> Entering GetTabAliasSkins(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabAliasSkins(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabAliasSkins", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabCustomAliases(int portalId)
        {
			Logger.LogError(">>> Entering GetTabCustomAliases(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabCustomAliases(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabCustomAliases", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabUrls(int portalId)
        {
			Logger.LogError(">>> Entering GetTabUrls(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabUrls(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabUrls", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabVersions(int tabId)
        {
			Logger.LogError(">>> Entering GetTabVersions(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetTabVersions(tabId={TabId})", tabId);
            return this.ExecuteReader("GetTabVersions", this.GetNull(tabId));
        }

        public virtual IDataReader GetTabVersionDetails(int tabVersionId)
        {
			Logger.LogError(">>> Entering GetTabVersionDetails(tabVersionId={TabVersionId})", tabVersionId);
            Logger.LogWarning("<<< Exiting GetTabVersionDetails(tabVersionId={TabVersionId})", tabVersionId);
            return this.ExecuteReader("GetTabVersionDetails", this.GetNull(tabVersionId));
        }

        public virtual IDataReader GetTabVersionDetailsHistory(int tabId, int version)
        {
			Logger.LogError(">>> Entering GetTabVersionDetailsHistory(tabId={TabId},version={Version})", tabId, version);
            Logger.LogWarning("<<< Exiting GetTabVersionDetailsHistory(tabId={TabId},version={Version})", tabId, version);
            return this.ExecuteReader("GetTabVersionDetailsHistory", this.GetNull(tabId), this.GetNull(version));
        }

        public virtual IDataReader GetCustomAliasesForTabs()
        {
			Logger.LogError(">>> Entering GetCustomAliasesForTabs()");
            Logger.LogWarning("<<< Exiting GetCustomAliasesForTabs()");
            return this.ExecuteReader("GetCustomAliasesForTabs");
        }

        public virtual void LocalizeTab(int tabId, string cultureCode, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering LocalizeTab(tabId={TabId},cultureCode={CultureCode},lastModifiedByUserID={LastModifiedByUserID})", tabId, cultureCode, lastModifiedByUserID);
            this.ExecuteNonQuery("LocalizeTab", tabId, cultureCode, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting LocalizeTab(tabId={TabId},cultureCode={CultureCode},lastModifiedByUserID={LastModifiedByUserID})", tabId, cultureCode, lastModifiedByUserID);
        }

        public virtual void MoveTabAfter(int tabId, int afterTabId, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering MoveTabAfter(tabId={TabId},afterTabId={AfterTabId},lastModifiedByUserID={LastModifiedByUserID})", tabId, afterTabId, lastModifiedByUserID);
            this.ExecuteNonQuery("MoveTabAfter", tabId, afterTabId, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting MoveTabAfter(tabId={TabId},afterTabId={AfterTabId},lastModifiedByUserID={LastModifiedByUserID})", tabId, afterTabId, lastModifiedByUserID);
        }

        public virtual void MoveTabBefore(int tabId, int beforeTabId, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering MoveTabBefore(tabId={TabId},beforeTabId={BeforeTabId},lastModifiedByUserID={LastModifiedByUserID})", tabId, beforeTabId, lastModifiedByUserID);
            this.ExecuteNonQuery("MoveTabBefore", tabId, beforeTabId, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting MoveTabBefore(tabId={TabId},beforeTabId={BeforeTabId},lastModifiedByUserID={LastModifiedByUserID})", tabId, beforeTabId, lastModifiedByUserID);
        }

        public virtual void MoveTabToParent(int tabId, int parentId, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering MoveTabToParent(tabId={TabId},parentId={ParentId},lastModifiedByUserID={LastModifiedByUserID})", tabId, parentId, lastModifiedByUserID);
            this.ExecuteNonQuery("MoveTabToParent", tabId, this.GetNull(parentId), lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting MoveTabToParent(tabId={TabId},parentId={ParentId},lastModifiedByUserID={LastModifiedByUserID})", tabId, parentId, lastModifiedByUserID);
        }

        public virtual void SaveTabUrl(int tabId, int seqNum, int portalAliasId, int portalAliasUsage, string url, string queryString, string cultureCode, string httpStatus, bool isSystem, int modifiedByUserID)
        {
			Logger.LogError(">>> Entering SaveTabUrl(tabId={TabId},seqNum={SeqNum},portalAliasId={PortalAliasId},portalAliasUsage={PortalAliasUsage},url={Url},queryString={QueryString},cultureCode={CultureCode},httpStatus={HttpStatus},isSystem={IsSystem},modifiedByUserID={ModifiedByUserID})", tabId, seqNum, portalAliasId, portalAliasUsage, url, queryString, cultureCode, httpStatus, isSystem, modifiedByUserID);
            this.ExecuteNonQuery("SaveTabUrl", tabId, seqNum, this.GetNull(portalAliasId), portalAliasUsage, url, queryString, cultureCode, httpStatus, isSystem, modifiedByUserID);
			Logger.LogWarning("<<< Exiting SaveTabUrl(tabId={TabId},seqNum={SeqNum},portalAliasId={PortalAliasId},portalAliasUsage={PortalAliasUsage},url={Url},queryString={QueryString},cultureCode={CultureCode},httpStatus={HttpStatus},isSystem={IsSystem},modifiedByUserID={ModifiedByUserID})", tabId, seqNum, portalAliasId, portalAliasUsage, url, queryString, cultureCode, httpStatus, isSystem, modifiedByUserID);
        }

        public virtual int SaveTabVersion(int tabVersionId, int tabId, DateTime timeStamp, int version, bool isPublished, int createdByUserID, int modifiedByUserID)
        {
			Logger.LogError(">>> Entering SaveTabVersion(tabVersionId={TabVersionId},tabId={TabId},timeStamp={TimeStamp},version={Version},isPublished={IsPublished},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionId, tabId, timeStamp, version, isPublished, createdByUserID, modifiedByUserID);
            Logger.LogWarning("<<< Exiting SaveTabVersion(tabVersionId={TabVersionId},tabId={TabId},timeStamp={TimeStamp},version={Version},isPublished={IsPublished},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionId, tabId, timeStamp, version, isPublished, createdByUserID, modifiedByUserID);
            return this.ExecuteScalar<int>("SaveTabVersion", tabVersionId, tabId, timeStamp, version, isPublished, createdByUserID, modifiedByUserID);
        }

        public virtual int SaveTabVersionDetail(int tabVersionDetailId, int tabVersionId, int moduleId, int moduleVersion, string paneName, int moduleOrder, int action, int createdByUserID, int modifiedByUserID)
        {
			Logger.LogError(">>> Entering SaveTabVersionDetail(tabVersionDetailId={TabVersionDetailId},tabVersionId={TabVersionId},moduleId={ModuleId},moduleVersion={ModuleVersion},paneName={PaneName},moduleOrder={ModuleOrder},action={Action},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionDetailId, tabVersionId, moduleId, moduleVersion, paneName, moduleOrder, action, createdByUserID, modifiedByUserID);
            Logger.LogWarning("<<< Exiting SaveTabVersionDetail(tabVersionDetailId={TabVersionDetailId},tabVersionId={TabVersionId},moduleId={ModuleId},moduleVersion={ModuleVersion},paneName={PaneName},moduleOrder={ModuleOrder},action={Action},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionDetailId, tabVersionId, moduleId, moduleVersion, paneName, moduleOrder, action, createdByUserID, modifiedByUserID);
            return this.ExecuteScalar<int>("SaveTabVersionDetail", tabVersionDetailId, tabVersionId, moduleId, moduleVersion, paneName, moduleOrder, action, createdByUserID, modifiedByUserID);
        }

        public virtual void UpdateTab(int tabId, int contentItemId, int portalId, Guid versionGuid, Guid defaultLanguageGuid, Guid localizedVersionGuid, string tabName, bool isVisible, bool disableLink, int parentId, string iconFile, string iconFileLarge, string title, string description, string keyWords, bool isDeleted, string url, string skinSrc, string containerSrc, DateTime startDate, DateTime endDate, int refreshInterval, string pageHeadText, bool isSecure, bool permanentRedirect, float siteMapPriority, int lastModifiedByuserID, string cultureCode, bool isSystem)
        {
			Logger.LogError(">>> Entering UpdateTab(tabId={TabId},contentItemId={ContentItemId},portalId={PortalId},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},tabName={TabName},isVisible={IsVisible},disableLink={DisableLink},parentId={ParentId},iconFile={IconFile},iconFileLarge={IconFileLarge},title={Title},description={Description},keyWords={KeyWords},isDeleted={IsDeleted},url={Url},skinSrc={SkinSrc},containerSrc={ContainerSrc},startDate={StartDate},endDate={EndDate},refreshInterval={RefreshInterval},pageHeadText={PageHeadText},isSecure={IsSecure},permanentRedirect={PermanentRedirect},siteMapPriority={SiteMapPriority},lastModifiedByuserID={LastModifiedByuserID},cultureCode={CultureCode},isSystem={IsSystem})", tabId, contentItemId, portalId, versionGuid, defaultLanguageGuid, localizedVersionGuid, tabName, isVisible, disableLink, parentId, iconFile, iconFileLarge, title, description, keyWords, isDeleted, url, skinSrc, containerSrc, startDate, endDate, refreshInterval, pageHeadText, isSecure, permanentRedirect, siteMapPriority, lastModifiedByuserID, cultureCode, isSystem);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateTab",
                tabId,
                contentItemId,
                this.GetNull(portalId),
                versionGuid,
                this.GetNull(defaultLanguageGuid),
                localizedVersionGuid,
                tabName,
                isVisible,
                disableLink,
                this.GetNull(parentId),
                iconFile,
                iconFileLarge,
                title,
                description,
                keyWords,
                isDeleted,
                url,
                this.GetNull(skinSrc),
                this.GetNull(containerSrc),
                this.GetNull(startDate),
                this.GetNull(endDate),
                this.GetNull(refreshInterval),
                this.GetNull(pageHeadText),
                isSecure,
                permanentRedirect,
                siteMapPriority,
                lastModifiedByuserID,
                this.GetNull(cultureCode),
                isSystem);
				Logger.LogError("UpdateTab(tabId,contentItemId,portalId,versionGuid,defaultLanguageGuid,localizedVersionGuid,tabName,isVisible,disableLink,parentId,iconFile,iconFileLarge,title,description,keyWords,isDeleted,url,skinSrc,containerSrc,startDate,endDate,refreshInterval,pageHeadText,isSecure,permanentRedirect,siteMapPriority,lastModifiedByuserID,cultureCode,isSystem)={TabId},{ContentItemId},{PortalId},{VersionGuid},{DefaultLanguageGuid},{LocalizedVersionGuid},{TabName},{IsVisible},{DisableLink},{ParentId},{IconFile},{IconFileLarge},{Title},{Description},{KeyWords},{IsDeleted},{Url},{SkinSrc},{ContainerSrc},{StartDate},{EndDate},{RefreshInterval},{PageHeadText},{IsSecure},{PermanentRedirect},{SiteMapPriority},{LastModifiedByuserID},{CultureCode},{IsSystem}: database query executed in {Arg29} ms", tabId, contentItemId, portalId, versionGuid, defaultLanguageGuid, localizedVersionGuid, tabName, isVisible, disableLink, parentId, iconFile, iconFileLarge, title, description, keyWords, isDeleted, url, skinSrc, containerSrc, startDate, endDate, refreshInterval, pageHeadText, isSecure, permanentRedirect, siteMapPriority, lastModifiedByuserID, cultureCode, isSystem, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateTab(tabId={TabId},contentItemId={ContentItemId},portalId={PortalId},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},tabName={TabName},isVisible={IsVisible},disableLink={DisableLink},parentId={ParentId},iconFile={IconFile},iconFileLarge={IconFileLarge},title={Title},description={Description},keyWords={KeyWords},isDeleted={IsDeleted},url={Url},skinSrc={SkinSrc},containerSrc={ContainerSrc},startDate={StartDate},endDate={EndDate},refreshInterval={RefreshInterval},pageHeadText={PageHeadText},isSecure={IsSecure},permanentRedirect={PermanentRedirect},siteMapPriority={SiteMapPriority},lastModifiedByuserID={LastModifiedByuserID},cultureCode={CultureCode},isSystem={IsSystem})", tabId, contentItemId, portalId, versionGuid, defaultLanguageGuid, localizedVersionGuid, tabName, isVisible, disableLink, parentId, iconFile, iconFileLarge, title, description, keyWords, isDeleted, url, skinSrc, containerSrc, startDate, endDate, refreshInterval, pageHeadText, isSecure, permanentRedirect, siteMapPriority, lastModifiedByuserID, cultureCode, isSystem);
        }

        public virtual void UpdateTabOrder(int tabId, int tabOrder, int parentId, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabOrder(tabId={TabId},tabOrder={TabOrder},parentId={ParentId},lastModifiedByUserID={LastModifiedByUserID})", tabId, tabOrder, parentId, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateTabOrder", tabId, tabOrder, this.GetNull(parentId), lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateTabOrder(tabId={TabId},tabOrder={TabOrder},parentId={ParentId},lastModifiedByUserID={LastModifiedByUserID})", tabId, tabOrder, parentId, lastModifiedByUserID);
        }

        public virtual void UpdateTabSetting(int tabId, string settingName, string settingValue, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", tabId, settingName, settingValue, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateTabSetting", tabId, settingName, settingValue, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", tabId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabTranslationStatus(int tabId, Guid localizedVersionGuid, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabTranslationStatus(tabId={TabId},localizedVersionGuid={LocalizedVersionGuid},lastModifiedByUserID={LastModifiedByUserID})", tabId, localizedVersionGuid, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateTabTranslationStatus", tabId, localizedVersionGuid, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateTabTranslationStatus(tabId={TabId},localizedVersionGuid={LocalizedVersionGuid},lastModifiedByUserID={LastModifiedByUserID})", tabId, localizedVersionGuid, lastModifiedByUserID);
        }

        public virtual void MarkAsPublished(int tabId)
        {
			Logger.LogError(">>> Entering MarkAsPublished(tabId={TabId})", tabId);
            this.ExecuteNonQuery("PublishTab", tabId);
			Logger.LogWarning("<<< Exiting MarkAsPublished(tabId={TabId})", tabId);
        }

        public virtual void UpdateTabVersion(int tabId, Guid versionGuid)
        {
			Logger.LogError(">>> Entering UpdateTabVersion(tabId={TabId},versionGuid={VersionGuid})", tabId, versionGuid);
            this.ExecuteNonQuery("UpdateTabVersion", tabId, versionGuid);
			Logger.LogWarning("<<< Exiting UpdateTabVersion(tabId={TabId},versionGuid={VersionGuid})", tabId, versionGuid);
        }

        public virtual int AddModule(int contentItemId, int portalId, int moduleDefId, bool allTabs, DateTime startDate, DateTime endDate, bool inheritViewPermissions, bool isShareable, bool isShareableViewOnly, bool isDeleted, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddModule(contentItemId={ContentItemId},portalId={PortalId},moduleDefId={ModuleDefId},allTabs={AllTabs},startDate={StartDate},endDate={EndDate},inheritViewPermissions={InheritViewPermissions},isShareable={IsShareable},isShareableViewOnly={IsShareableViewOnly},isDeleted={IsDeleted},createdByUserID={CreatedByUserID})", contentItemId, portalId, moduleDefId, allTabs, startDate, endDate, inheritViewPermissions, isShareable, isShareableViewOnly, isDeleted, createdByUserID);
            Logger.LogWarning("<<< Exiting AddModule(contentItemId={ContentItemId},portalId={PortalId},moduleDefId={ModuleDefId},allTabs={AllTabs},startDate={StartDate},endDate={EndDate},inheritViewPermissions={InheritViewPermissions},isShareable={IsShareable},isShareableViewOnly={IsShareableViewOnly},isDeleted={IsDeleted},createdByUserID={CreatedByUserID})", contentItemId, portalId, moduleDefId, allTabs, startDate, endDate, inheritViewPermissions, isShareable, isShareableViewOnly, isDeleted, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddModule",
                contentItemId,
                this.GetNull(portalId),
                moduleDefId,
                allTabs,
                this.GetNull(startDate),
                this.GetNull(endDate),
                inheritViewPermissions,
                isShareable,
                isShareableViewOnly,
                isDeleted,
                createdByUserID);
        }

        public virtual void AddTabModule(int tabId, int moduleId, string moduleTitle, string header, string footer, int moduleOrder, string paneName, int cacheTime, string cacheMethod, string alignment, string color, string border, string iconFile, int visibility, string containerSrc, bool displayTitle, bool displayPrint, bool displaySyndicate, Guid uniqueId, Guid versionGuid, Guid defaultLanguageGuid, Guid localizedVersionGuid, string cultureCode, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddTabModule(tabId={TabId},moduleId={ModuleId},moduleTitle={ModuleTitle},header={Header},footer={Footer},moduleOrder={ModuleOrder},paneName={PaneName},cacheTime={CacheTime},cacheMethod={CacheMethod},alignment={Alignment},color={Color},border={Border},iconFile={IconFile},visibility={Visibility},containerSrc={ContainerSrc},displayTitle={DisplayTitle},displayPrint={DisplayPrint},displaySyndicate={DisplaySyndicate},uniqueId={UniqueId},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},cultureCode={CultureCode},createdByUserID={CreatedByUserID})", tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, uniqueId, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, createdByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "AddTabModule",
                tabId,
                moduleId,
                moduleTitle,
                this.GetNull(header),
                this.GetNull(footer),
                moduleOrder,
                paneName,
                cacheTime,
                this.GetNull(cacheMethod),
                this.GetNull(alignment),
                this.GetNull(color),
                this.GetNull(border),
                this.GetNull(iconFile),
                visibility,
                this.GetNull(containerSrc),
                displayTitle,
                displayPrint,
                displaySyndicate,
                uniqueId,
                versionGuid,
                this.GetNull(defaultLanguageGuid),
                localizedVersionGuid,
                cultureCode,
                createdByUserID);
				Logger.LogError("AddTabModule(tabId,moduleId,moduleTitle,header,footer,moduleOrder,paneName,cacheTime,cacheMethod,alignment,color,border,iconFile,visibility,containerSrc,displayTitle,displayPrint,displaySyndicate,uniqueId,versionGuid,defaultLanguageGuid,localizedVersionGuid,cultureCode,createdByUserID)={TabId},{ModuleId},{ModuleTitle},{Header},{Footer},{ModuleOrder},{PaneName},{CacheTime},{CacheMethod},{Alignment},{Color},{Border},{IconFile},{Visibility},{ContainerSrc},{DisplayTitle},{DisplayPrint},{DisplaySyndicate},{UniqueId},{VersionGuid},{DefaultLanguageGuid},{LocalizedVersionGuid},{CultureCode},{CreatedByUserID}: database query executed in {Arg24} ms", tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, uniqueId, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, createdByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddTabModule(tabId={TabId},moduleId={ModuleId},moduleTitle={ModuleTitle},header={Header},footer={Footer},moduleOrder={ModuleOrder},paneName={PaneName},cacheTime={CacheTime},cacheMethod={CacheMethod},alignment={Alignment},color={Color},border={Border},iconFile={IconFile},visibility={Visibility},containerSrc={ContainerSrc},displayTitle={DisplayTitle},displayPrint={DisplayPrint},displaySyndicate={DisplaySyndicate},uniqueId={UniqueId},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},cultureCode={CultureCode},createdByUserID={CreatedByUserID})", tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, uniqueId, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, createdByUserID);
        }

        public virtual void DeleteModule(int moduleId)
        {
			Logger.LogError(">>> Entering DeleteModule(moduleId={ModuleId})", moduleId);
            this.ExecuteNonQuery("DeleteModule", moduleId);
			Logger.LogWarning("<<< Exiting DeleteModule(moduleId={ModuleId})", moduleId);
        }

        public virtual void DeleteModuleSetting(int moduleId, string settingName)
        {
			Logger.LogError(">>> Entering DeleteModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            this.ExecuteNonQuery("DeleteModuleSetting", moduleId, settingName);
			Logger.LogWarning("<<< Exiting DeleteModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
        }

        public virtual void DeleteModuleSettings(int moduleId)
        {
			Logger.LogError(">>> Entering DeleteModuleSettings(moduleId={ModuleId})", moduleId);
            this.ExecuteNonQuery("DeleteModuleSettings", moduleId);
			Logger.LogWarning("<<< Exiting DeleteModuleSettings(moduleId={ModuleId})", moduleId);
        }

        public virtual void DeleteTabModule(int tabId, int moduleId, bool softDelete, int lastModifiedByUserId = -1)
        {
			Logger.LogError(">>> Entering DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},lastModifiedByUserId={LastModifiedByUserId})", tabId, moduleId, softDelete, lastModifiedByUserId);
            this.ExecuteNonQuery("DeleteTabModule", tabId, moduleId, softDelete, lastModifiedByUserId);
			Logger.LogWarning("<<< Exiting DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},lastModifiedByUserId={LastModifiedByUserId})", tabId, moduleId, softDelete, lastModifiedByUserId);
        }

        public virtual void DeleteTabModuleSetting(int tabModuleId, string settingName)
        {
			Logger.LogError(">>> Entering DeleteTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
            this.ExecuteNonQuery("DeleteTabModuleSetting", tabModuleId, settingName);
			Logger.LogWarning("<<< Exiting DeleteTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
        }

        public virtual void DeleteTabModuleSettings(int tabModuleId)
        {
			Logger.LogError(">>> Entering DeleteTabModuleSettings(tabModuleId={TabModuleId})", tabModuleId);
            this.ExecuteNonQuery("DeleteTabModuleSettings", tabModuleId);
			Logger.LogWarning("<<< Exiting DeleteTabModuleSettings(tabModuleId={TabModuleId})", tabModuleId);
        }

        public virtual IDataReader GetTabModuleSettingsByName(int portalId, string settingName)
        {
			Logger.LogError(">>> Entering GetTabModuleSettingsByName(portalId={PortalId},settingName={SettingName})", portalId, settingName);
            Logger.LogWarning("<<< Exiting GetTabModuleSettingsByName(portalId={PortalId},settingName={SettingName})", portalId, settingName);
            return this.ExecuteReader("GetTabModuleSettingsByName", portalId, settingName);
        }

        public virtual IDataReader GetTabModuleIdsBySettingNameAndValue(int portalId, string settingName, string expectedValue)
        {
			Logger.LogError(">>> Entering GetTabModuleIdsBySettingNameAndValue(portalId={PortalId},settingName={SettingName},expectedValue={ExpectedValue})", portalId, settingName, expectedValue);
            Logger.LogWarning("<<< Exiting GetTabModuleIdsBySettingNameAndValue(portalId={PortalId},settingName={SettingName},expectedValue={ExpectedValue})", portalId, settingName, expectedValue);
            return this.ExecuteReader("GetTabModuleIdsBySettingNameAndValue", portalId, settingName, expectedValue);
        }

        public virtual IDataReader GetAllModules()
        {
			Logger.LogError(">>> Entering GetAllModules()");
            Logger.LogWarning("<<< Exiting GetAllModules()");
            return this.ExecuteReader("GetAllModules");
        }

        public virtual IDataReader GetAllTabsModules(int portalId, bool allTabs)
        {
			Logger.LogError(">>> Entering GetAllTabsModules(portalId={PortalId},allTabs={AllTabs})", portalId, allTabs);
            Logger.LogWarning("<<< Exiting GetAllTabsModules(portalId={PortalId},allTabs={AllTabs})", portalId, allTabs);
            return this.ExecuteReader("GetAllTabsModules", portalId, allTabs);
        }

        public virtual IDataReader GetAllTabsModulesByModuleID(int moduleId)
        {
			Logger.LogError(">>> Entering GetAllTabsModulesByModuleID(moduleId={ModuleId})", moduleId);
            Logger.LogWarning("<<< Exiting GetAllTabsModulesByModuleID(moduleId={ModuleId})", moduleId);
            return this.ExecuteReader("GetAllTabsModulesByModuleID", moduleId);
        }

        public virtual IDataReader GetModule(int moduleId, int tabId)
        {
			Logger.LogError(">>> Entering GetModule(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            Logger.LogWarning("<<< Exiting GetModule(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            return this.ExecuteReader("GetModule", moduleId, this.GetNull(tabId));
        }

        public virtual IDataReader GetModuleByDefinition(int portalId, string definitionName)
        {
			Logger.LogError(">>> Entering GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName})", portalId, definitionName);
            Logger.LogWarning("<<< Exiting GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName})", portalId, definitionName);
            return this.ExecuteReader("GetModuleByDefinition", this.GetNull(portalId), definitionName);
        }

        public virtual IDataReader GetModuleByUniqueID(Guid uniqueId)
        {
			Logger.LogError(">>> Entering GetModuleByUniqueID(uniqueId={UniqueId})", uniqueId);
            Logger.LogWarning("<<< Exiting GetModuleByUniqueID(uniqueId={UniqueId})", uniqueId);
            return this.ExecuteReader("GetModuleByUniqueID", uniqueId);
        }

        public virtual IDataReader GetModules(int portalId)
        {
			Logger.LogError(">>> Entering GetModules(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetModules(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetModules", portalId);
        }

        public virtual IDataReader GetModuleSetting(int moduleId, string settingName)
        {
			Logger.LogError(">>> Entering GetModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            Logger.LogWarning("<<< Exiting GetModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            return this.ExecuteReader("GetModuleSetting", moduleId, settingName);
        }

        public virtual IDataReader GetModuleSettings(int moduleId)
        {
			Logger.LogError(">>> Entering GetModuleSettings(moduleId={ModuleId})", moduleId);
            Logger.LogWarning("<<< Exiting GetModuleSettings(moduleId={ModuleId})", moduleId);
            return this.ExecuteReader("GetModuleSettings", moduleId);
        }

        public virtual IDataReader GetModuleSettingsByTab(int tabId)
        {
			Logger.LogError(">>> Entering GetModuleSettingsByTab(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetModuleSettingsByTab(tabId={TabId})", tabId);
            return this.ExecuteReader("GetModuleSettingsByTab", tabId);
        }

        public virtual IDataReader GetSearchModules(int portalId)
        {
			Logger.LogError(">>> Entering GetSearchModules(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetSearchModules(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetSearchModules", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabModule(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetTabModule(tabModuleId={TabModuleId})", tabModuleId);
            Logger.LogWarning("<<< Exiting GetTabModule(tabModuleId={TabModuleId})", tabModuleId);
            return this.ExecuteReader("GetTabModule", tabModuleId);
        }

        public virtual IDataReader GetTabModuleOrder(int tabId, string paneName)
        {
			Logger.LogError(">>> Entering GetTabModuleOrder(tabId={TabId},paneName={PaneName})", tabId, paneName);
            Logger.LogWarning("<<< Exiting GetTabModuleOrder(tabId={TabId},paneName={PaneName})", tabId, paneName);
            return this.ExecuteReader("GetTabModuleOrder", tabId, paneName);
        }

        public virtual IDataReader GetTabModules(int tabId)
        {
			Logger.LogError(">>> Entering GetTabModules(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetTabModules(tabId={TabId})", tabId);
            return this.ExecuteReader("GetTabModules", tabId);
        }

        public virtual IDataReader GetTabModuleSetting(int tabModuleId, string settingName)
        {
			Logger.LogError(">>> Entering GetTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
            Logger.LogWarning("<<< Exiting GetTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
            return this.ExecuteReader("GetTabModuleSetting", tabModuleId, settingName);
        }

        public virtual IDataReader GetTabModuleSettings(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetTabModuleSettings(tabModuleId={TabModuleId})", tabModuleId);
            Logger.LogWarning("<<< Exiting GetTabModuleSettings(tabModuleId={TabModuleId})", tabModuleId);
            return this.ExecuteReader("GetTabModuleSettings", tabModuleId);
        }

        public virtual IDataReader GetTabModuleSettingsByTab(int tabId)
        {
			Logger.LogError(">>> Entering GetTabModuleSettingsByTab(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetTabModuleSettingsByTab(tabId={TabId})", tabId);
            return this.ExecuteReader("GetTabModuleSettingsByTab", tabId);
        }

        public virtual void MoveTabModule(int fromTabId, int moduleId, int toTabId, string toPaneName, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering MoveTabModule(fromTabId={FromTabId},moduleId={ModuleId},toTabId={ToTabId},toPaneName={ToPaneName},lastModifiedByUserID={LastModifiedByUserID})", fromTabId, moduleId, toTabId, toPaneName, lastModifiedByUserID);
            this.ExecuteNonQuery("MoveTabModule", fromTabId, moduleId, toTabId, toPaneName, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting MoveTabModule(fromTabId={FromTabId},moduleId={ModuleId},toTabId={ToTabId},toPaneName={ToPaneName},lastModifiedByUserID={LastModifiedByUserID})", fromTabId, moduleId, toTabId, toPaneName, lastModifiedByUserID);
        }

        public virtual void RestoreTabModule(int tabId, int moduleId)
        {
			Logger.LogError(">>> Entering RestoreTabModule(tabId={TabId},moduleId={ModuleId})", tabId, moduleId);
            this.ExecuteNonQuery("RestoreTabModule", tabId, moduleId);
			Logger.LogWarning("<<< Exiting RestoreTabModule(tabId={TabId},moduleId={ModuleId})", tabId, moduleId);
        }

        public virtual void UpdateModule(int moduleId, int moduleDefId, int contentItemId, bool allTabs, DateTime startDate, DateTime endDate, bool inheritViewPermissions, bool isShareable, bool isShareableViewOnly, bool isDeleted, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateModule(moduleId={ModuleId},moduleDefId={ModuleDefId},contentItemId={ContentItemId},allTabs={AllTabs},startDate={StartDate},endDate={EndDate},inheritViewPermissions={InheritViewPermissions},isShareable={IsShareable},isShareableViewOnly={IsShareableViewOnly},isDeleted={IsDeleted},lastModifiedByUserID={LastModifiedByUserID})", moduleId, moduleDefId, contentItemId, allTabs, startDate, endDate, inheritViewPermissions, isShareable, isShareableViewOnly, isDeleted, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateModule",
                moduleId,
                moduleDefId,
                contentItemId,
                allTabs,
                this.GetNull(startDate),
                this.GetNull(endDate),
                inheritViewPermissions,
                isShareable,
                isShareableViewOnly,
                isDeleted,
                lastModifiedByUserID);
				Logger.LogError("UpdateModule(moduleId,moduleDefId,contentItemId,allTabs,startDate,endDate,inheritViewPermissions,isShareable,isShareableViewOnly,isDeleted,lastModifiedByUserID)={ModuleId},{ModuleDefId},{ContentItemId},{AllTabs},{StartDate},{EndDate},{InheritViewPermissions},{IsShareable},{IsShareableViewOnly},{IsDeleted},{LastModifiedByUserID}: database query executed in {Arg11} ms", moduleId, moduleDefId, contentItemId, allTabs, startDate, endDate, inheritViewPermissions, isShareable, isShareableViewOnly, isDeleted, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateModule(moduleId={ModuleId},moduleDefId={ModuleDefId},contentItemId={ContentItemId},allTabs={AllTabs},startDate={StartDate},endDate={EndDate},inheritViewPermissions={InheritViewPermissions},isShareable={IsShareable},isShareableViewOnly={IsShareableViewOnly},isDeleted={IsDeleted},lastModifiedByUserID={LastModifiedByUserID})", moduleId, moduleDefId, contentItemId, allTabs, startDate, endDate, inheritViewPermissions, isShareable, isShareableViewOnly, isDeleted, lastModifiedByUserID);
        }

        public virtual void UpdateModuleLastContentModifiedOnDate(int moduleId)
        {
			Logger.LogError(">>> Entering UpdateModuleLastContentModifiedOnDate(moduleId={ModuleId})", moduleId);
            this.ExecuteNonQuery("UpdateModuleLastContentModifiedOnDate", moduleId);
			Logger.LogWarning("<<< Exiting UpdateModuleLastContentModifiedOnDate(moduleId={ModuleId})", moduleId);
        }

        public virtual void UpdateModuleOrder(int tabId, int moduleId, int moduleOrder, string paneName)
        {
			Logger.LogError(">>> Entering UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName})", tabId, moduleId, moduleOrder, paneName);
            this.ExecuteNonQuery("UpdateModuleOrder", tabId, moduleId, moduleOrder, paneName);
			Logger.LogWarning("<<< Exiting UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName})", tabId, moduleId, moduleOrder, paneName);
        }

        public virtual void UpdateModuleSetting(int moduleId, string settingName, string settingValue, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateModuleSetting(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", moduleId, settingName, settingValue, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateModuleSetting", moduleId, settingName, settingValue, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateModuleSetting(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", moduleId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabModule(int tabModuleId, int tabId, int moduleId, string moduleTitle, string header, string footer, int moduleOrder, string paneName, int cacheTime, string cacheMethod, string alignment, string color, string border, string iconFile, int visibility, string containerSrc, bool displayTitle, bool displayPrint, bool displaySyndicate, Guid versionGuid, Guid defaultLanguageGuid, Guid localizedVersionGuid, string cultureCode, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabModule(tabModuleId={TabModuleId},tabId={TabId},moduleId={ModuleId},moduleTitle={ModuleTitle},header={Header},footer={Footer},moduleOrder={ModuleOrder},paneName={PaneName},cacheTime={CacheTime},cacheMethod={CacheMethod},alignment={Alignment},color={Color},border={Border},iconFile={IconFile},visibility={Visibility},containerSrc={ContainerSrc},displayTitle={DisplayTitle},displayPrint={DisplayPrint},displaySyndicate={DisplaySyndicate},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},cultureCode={CultureCode},lastModifiedByUserID={LastModifiedByUserID})", tabModuleId, tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateTabModule",
                tabModuleId,
                tabId,
                moduleId,
                moduleTitle,
                this.GetNull(header),
                this.GetNull(footer),
                moduleOrder,
                paneName,
                cacheTime,
                this.GetNull(cacheMethod),
                this.GetNull(alignment),
                this.GetNull(color),
                this.GetNull(border),
                this.GetNull(iconFile),
                visibility,
                this.GetNull(containerSrc),
                displayTitle,
                displayPrint,
                displaySyndicate,
                versionGuid,
                this.GetNull(defaultLanguageGuid),
                localizedVersionGuid,
                cultureCode,
                lastModifiedByUserID);
				Logger.LogError("UpdateTabModule(tabModuleId,tabId,moduleId,moduleTitle,header,footer,moduleOrder,paneName,cacheTime,cacheMethod,alignment,color,border,iconFile,visibility,containerSrc,displayTitle,displayPrint,displaySyndicate,versionGuid,defaultLanguageGuid,localizedVersionGuid,cultureCode,lastModifiedByUserID)={TabModuleId},{TabId},{ModuleId},{ModuleTitle},{Header},{Footer},{ModuleOrder},{PaneName},{CacheTime},{CacheMethod},{Alignment},{Color},{Border},{IconFile},{Visibility},{ContainerSrc},{DisplayTitle},{DisplayPrint},{DisplaySyndicate},{VersionGuid},{DefaultLanguageGuid},{LocalizedVersionGuid},{CultureCode},{LastModifiedByUserID}: database query executed in {Arg24} ms", tabModuleId, tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateTabModule(tabModuleId={TabModuleId},tabId={TabId},moduleId={ModuleId},moduleTitle={ModuleTitle},header={Header},footer={Footer},moduleOrder={ModuleOrder},paneName={PaneName},cacheTime={CacheTime},cacheMethod={CacheMethod},alignment={Alignment},color={Color},border={Border},iconFile={IconFile},visibility={Visibility},containerSrc={ContainerSrc},displayTitle={DisplayTitle},displayPrint={DisplayPrint},displaySyndicate={DisplaySyndicate},versionGuid={VersionGuid},defaultLanguageGuid={DefaultLanguageGuid},localizedVersionGuid={LocalizedVersionGuid},cultureCode={CultureCode},lastModifiedByUserID={LastModifiedByUserID})", tabModuleId, tabId, moduleId, moduleTitle, header, footer, moduleOrder, paneName, cacheTime, cacheMethod, alignment, color, border, iconFile, visibility, containerSrc, displayTitle, displayPrint, displaySyndicate, versionGuid, defaultLanguageGuid, localizedVersionGuid, cultureCode, lastModifiedByUserID);
        }

        public virtual void UpdateTabModuleTranslationStatus(int tabModuleId, Guid localizedVersionGuid, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateTabModuleTranslationStatus(tabModuleId={TabModuleId},localizedVersionGuid={LocalizedVersionGuid},lastModifiedByUserId={LastModifiedByUserId})", tabModuleId, localizedVersionGuid, lastModifiedByUserId);
            this.ExecuteNonQuery("UpdateTabModuleTranslationStatus", tabModuleId, localizedVersionGuid, lastModifiedByUserId);
			Logger.LogWarning("<<< Exiting UpdateTabModuleTranslationStatus(tabModuleId={TabModuleId},localizedVersionGuid={LocalizedVersionGuid},lastModifiedByUserId={LastModifiedByUserId})", tabModuleId, localizedVersionGuid, lastModifiedByUserId);
        }

        public virtual void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", tabModuleId, settingName, settingValue, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", tabModuleId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabModuleVersion(int tabModuleId, Guid versionGuid)
        {
			Logger.LogError(">>> Entering UpdateTabModuleVersion(tabModuleId={TabModuleId},versionGuid={VersionGuid})", tabModuleId, versionGuid);
            this.ExecuteNonQuery("UpdateTabModuleVersion", tabModuleId, versionGuid);
			Logger.LogWarning("<<< Exiting UpdateTabModuleVersion(tabModuleId={TabModuleId},versionGuid={VersionGuid})", tabModuleId, versionGuid);
        }

        public virtual void UpdateTabModuleVersionByModule(int moduleId)
        {
			Logger.LogError(">>> Entering UpdateTabModuleVersionByModule(moduleId={ModuleId})", moduleId);
            this.ExecuteNonQuery("UpdateTabModuleVersionByModule", moduleId);
			Logger.LogWarning("<<< Exiting UpdateTabModuleVersionByModule(moduleId={ModuleId})", moduleId);
        }

        public virtual IDataReader GetInstalledModules()
        {
			Logger.LogError(">>> Entering GetInstalledModules()");
            Logger.LogWarning("<<< Exiting GetInstalledModules()");
            return this.ExecuteReader("GetInstalledModules");
        }

        public virtual int AddDesktopModule(int packageID, string moduleName, string folderName, string friendlyName, string description, string version, bool isPremium, bool isAdmin, string businessControllerClass, int supportedFeatures, int shareable, string compatibleVersions, string dependencies, string permissions, int contentItemId, int createdByUserID, string adminPage, string hostPage)
        {
			Logger.LogError(">>> Entering AddDesktopModule(packageID={PackageID},moduleName={ModuleName},folderName={FolderName},friendlyName={FriendlyName},description={Description},version={Version},isPremium={IsPremium},isAdmin={IsAdmin},businessControllerClass={BusinessControllerClass},supportedFeatures={SupportedFeatures},shareable={Shareable},compatibleVersions={CompatibleVersions},dependencies={Dependencies},permissions={Permissions},contentItemId={ContentItemId},createdByUserID={CreatedByUserID},adminPage={AdminPage},hostPage={HostPage})", packageID, moduleName, folderName, friendlyName, description, version, isPremium, isAdmin, businessControllerClass, supportedFeatures, shareable, compatibleVersions, dependencies, permissions, contentItemId, createdByUserID, adminPage, hostPage);
			Logger.LogWarning("<<< Exiting AddDesktopModule(packageID={PackageID},moduleName={ModuleName},folderName={FolderName},friendlyName={FriendlyName},description={Description},version={Version},isPremium={IsPremium},isAdmin={IsAdmin},businessControllerClass={BusinessControllerClass},supportedFeatures={SupportedFeatures},shareable={Shareable},compatibleVersions={CompatibleVersions},dependencies={Dependencies},permissions={Permissions},contentItemId={ContentItemId},createdByUserID={CreatedByUserID},adminPage={AdminPage},hostPage={HostPage})", packageID, moduleName, folderName, friendlyName, description, version, isPremium, isAdmin, businessControllerClass, supportedFeatures, shareable, compatibleVersions, dependencies, permissions, contentItemId, createdByUserID, adminPage, hostPage);
            return this.ExecuteScalar<int>(
                "AddDesktopModule",
                packageID,
                moduleName,
                folderName,
                friendlyName,
                this.GetNull(description),
                this.GetNull(version),
                isPremium,
                isAdmin,
                businessControllerClass,
                supportedFeatures,
                shareable,
                this.GetNull(compatibleVersions),
                this.GetNull(dependencies),
                this.GetNull(permissions),
                contentItemId,
                createdByUserID,
                adminPage,
                hostPage);
        }

        public virtual void DeleteDesktopModule(int desktopModuleId)
        {
			Logger.LogError(">>> Entering DeleteDesktopModule(desktopModuleId={DesktopModuleId})", desktopModuleId);
            this.ExecuteNonQuery("DeleteDesktopModule", desktopModuleId);
			Logger.LogWarning("<<< Exiting DeleteDesktopModule(desktopModuleId={DesktopModuleId})", desktopModuleId);
        }

        public virtual IDataReader GetDesktopModules()
        {
			Logger.LogError(">>> Entering GetDesktopModules()");
            Logger.LogWarning("<<< Exiting GetDesktopModules()");
            return this.ExecuteReader("GetDesktopModules");
        }

        public virtual IDataReader GetDesktopModulesByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetDesktopModulesByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetDesktopModulesByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetDesktopModulesByPortal", portalId);
        }

        public virtual void UpdateDesktopModule(int desktopModuleId, int packageID, string moduleName, string folderName, string friendlyName, string description, string version, bool isPremium, bool isAdmin, string businessControllerClass, int supportedFeatures, int shareable, string compatibleVersions, string dependencies, string permissions, int contentItemId, int lastModifiedByUserID, string adminpage, string hostpage)
        {
			Logger.LogError(">>> Entering UpdateDesktopModule(desktopModuleId={DesktopModuleId},packageID={PackageID},moduleName={ModuleName},folderName={FolderName},friendlyName={FriendlyName},description={Description},version={Version},isPremium={IsPremium},isAdmin={IsAdmin},businessControllerClass={BusinessControllerClass},supportedFeatures={SupportedFeatures},shareable={Shareable},compatibleVersions={CompatibleVersions},dependencies={Dependencies},permissions={Permissions},contentItemId={ContentItemId},lastModifiedByUserID={LastModifiedByUserID},adminpage={Adminpage},hostpage={Hostpage})", desktopModuleId, packageID, moduleName, folderName, friendlyName, description, version, isPremium, isAdmin, businessControllerClass, supportedFeatures, shareable, compatibleVersions, dependencies, permissions, contentItemId, lastModifiedByUserID, adminpage, hostpage);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateDesktopModule",
                desktopModuleId,
                packageID,
                moduleName,
                folderName,
                friendlyName,
                this.GetNull(description),
                this.GetNull(version),
                isPremium,
                isAdmin,
                businessControllerClass,
                supportedFeatures,
                shareable,
                this.GetNull(compatibleVersions),
                this.GetNull(dependencies),
                this.GetNull(permissions),
                contentItemId,
                lastModifiedByUserID,
                adminpage,
                hostpage);
				Logger.LogError("UpdateDesktopModule(desktopModuleId,packageID,moduleName,folderName,friendlyName,description,version,isPremium,isAdmin,businessControllerClass,supportedFeatures,shareable,compatibleVersions,dependencies,permissions,contentItemId,lastModifiedByUserID,adminpage,hostpage)={DesktopModuleId},{PackageID},{ModuleName},{FolderName},{FriendlyName},{Description},{Version},{IsPremium},{IsAdmin},{BusinessControllerClass},{SupportedFeatures},{Shareable},{CompatibleVersions},{Dependencies},{Permissions},{ContentItemId},{LastModifiedByUserID},{Adminpage},{Hostpage}: database query executed in {Arg19} ms", desktopModuleId, packageID, moduleName, folderName, friendlyName, description, version, isPremium, isAdmin, businessControllerClass, supportedFeatures, shareable, compatibleVersions, dependencies, permissions, contentItemId, lastModifiedByUserID, adminpage, hostpage, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateDesktopModule(desktopModuleId={DesktopModuleId},packageID={PackageID},moduleName={ModuleName},folderName={FolderName},friendlyName={FriendlyName},description={Description},version={Version},isPremium={IsPremium},isAdmin={IsAdmin},businessControllerClass={BusinessControllerClass},supportedFeatures={SupportedFeatures},shareable={Shareable},compatibleVersions={CompatibleVersions},dependencies={Dependencies},permissions={Permissions},contentItemId={ContentItemId},lastModifiedByUserID={LastModifiedByUserID},adminpage={Adminpage},hostpage={Hostpage})", desktopModuleId, packageID, moduleName, folderName, friendlyName, description, version, isPremium, isAdmin, businessControllerClass, supportedFeatures, shareable, compatibleVersions, dependencies, permissions, contentItemId, lastModifiedByUserID, adminpage, hostpage);
        }

        public virtual int AddPortalDesktopModule(int portalId, int desktopModuleId, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddPortalDesktopModule(portalId={PortalId},desktopModuleId={DesktopModuleId},createdByUserID={CreatedByUserID})", portalId, desktopModuleId, createdByUserID);
            Logger.LogWarning("<<< Exiting AddPortalDesktopModule(portalId={PortalId},desktopModuleId={DesktopModuleId},createdByUserID={CreatedByUserID})", portalId, desktopModuleId, createdByUserID);
            return this.ExecuteScalar<int>("AddPortalDesktopModule", portalId, desktopModuleId, createdByUserID);
        }

        public virtual void DeletePortalDesktopModules(int portalId, int desktopModuleId)
        {
			Logger.LogError(">>> Entering DeletePortalDesktopModules(portalId={PortalId},desktopModuleId={DesktopModuleId})", portalId, desktopModuleId);
            this.ExecuteNonQuery("DeletePortalDesktopModules", this.GetNull(portalId), this.GetNull(desktopModuleId));
			Logger.LogWarning("<<< Exiting DeletePortalDesktopModules(portalId={PortalId},desktopModuleId={DesktopModuleId})", portalId, desktopModuleId);
        }

        public virtual IDataReader GetPortalDesktopModules(int portalId, int desktopModuleId)
        {
			Logger.LogError(">>> Entering GetPortalDesktopModules(portalId={PortalId},desktopModuleId={DesktopModuleId})", portalId, desktopModuleId);
            Logger.LogWarning("<<< Exiting GetPortalDesktopModules(portalId={PortalId},desktopModuleId={DesktopModuleId})", portalId, desktopModuleId);
            return this.ExecuteReader("GetPortalDesktopModules", this.GetNull(portalId), this.GetNull(desktopModuleId));
        }

        public virtual int AddModuleDefinition(int desktopModuleId, string friendlyName, string definitionName, int defaultCacheTime, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddModuleDefinition(desktopModuleId={DesktopModuleId},friendlyName={FriendlyName},definitionName={DefinitionName},defaultCacheTime={DefaultCacheTime},createdByUserID={CreatedByUserID})", desktopModuleId, friendlyName, definitionName, defaultCacheTime, createdByUserID);
            Logger.LogWarning("<<< Exiting AddModuleDefinition(desktopModuleId={DesktopModuleId},friendlyName={FriendlyName},definitionName={DefinitionName},defaultCacheTime={DefaultCacheTime},createdByUserID={CreatedByUserID})", desktopModuleId, friendlyName, definitionName, defaultCacheTime, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddModuleDefinition",
                desktopModuleId,
                friendlyName,
                definitionName,
                defaultCacheTime,
                createdByUserID);
        }

        public virtual void DeleteModuleDefinition(int moduleDefId)
        {
			Logger.LogError(">>> Entering DeleteModuleDefinition(moduleDefId={ModuleDefId})", moduleDefId);
            this.ExecuteNonQuery("DeleteModuleDefinition", moduleDefId);
			Logger.LogWarning("<<< Exiting DeleteModuleDefinition(moduleDefId={ModuleDefId})", moduleDefId);
        }

        public virtual IDataReader GetModuleDefinitions()
        {
			Logger.LogError(">>> Entering GetModuleDefinitions()");
            Logger.LogWarning("<<< Exiting GetModuleDefinitions()");
            return this.ExecuteReader("GetModuleDefinitions");
        }

        public virtual void UpdateModuleDefinition(int moduleDefId, string friendlyName, string definitionName, int defaultCacheTime, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateModuleDefinition(moduleDefId={ModuleDefId},friendlyName={FriendlyName},definitionName={DefinitionName},defaultCacheTime={DefaultCacheTime},lastModifiedByUserID={LastModifiedByUserID})", moduleDefId, friendlyName, definitionName, defaultCacheTime, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateModuleDefinition",
                moduleDefId,
                friendlyName,
                definitionName,
                defaultCacheTime,
                lastModifiedByUserID);
				Logger.LogError("UpdateModuleDefinition(moduleDefId,friendlyName,definitionName,defaultCacheTime,lastModifiedByUserID)={ModuleDefId},{FriendlyName},{DefinitionName},{DefaultCacheTime},{LastModifiedByUserID}: database query executed in {Arg5} ms", moduleDefId, friendlyName, definitionName, defaultCacheTime, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateModuleDefinition(moduleDefId={ModuleDefId},friendlyName={FriendlyName},definitionName={DefinitionName},defaultCacheTime={DefaultCacheTime},lastModifiedByUserID={LastModifiedByUserID})", moduleDefId, friendlyName, definitionName, defaultCacheTime, lastModifiedByUserID);
        }

        public virtual int AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, int controlType, int viewOrder, string helpUrl, bool supportsPartialRendering, bool supportsPopUps, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddModuleControl(moduleDefId={ModuleDefId},controlKey={ControlKey},controlTitle={ControlTitle},controlSrc={ControlSrc},iconFile={IconFile},controlType={ControlType},viewOrder={ViewOrder},helpUrl={HelpUrl},supportsPartialRendering={SupportsPartialRendering},supportsPopUps={SupportsPopUps},createdByUserID={CreatedByUserID})", moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpUrl, supportsPartialRendering, supportsPopUps, createdByUserID);
			Logger.LogWarning("<<< Exiting AddModuleControl(moduleDefId={ModuleDefId},controlKey={ControlKey},controlTitle={ControlTitle},controlSrc={ControlSrc},iconFile={IconFile},controlType={ControlType},viewOrder={ViewOrder},helpUrl={HelpUrl},supportsPartialRendering={SupportsPartialRendering},supportsPopUps={SupportsPopUps},createdByUserID={CreatedByUserID})", moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpUrl, supportsPartialRendering, supportsPopUps, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddModuleControl",
                this.GetNull(moduleDefId),
                this.GetNull(controlKey),
                this.GetNull(controlTitle),
                controlSrc,
                this.GetNull(iconFile),
                controlType,
                this.GetNull(viewOrder),
                this.GetNull(helpUrl),
                supportsPartialRendering,
                supportsPopUps,
                createdByUserID);
        }

        public virtual void DeleteModuleControl(int moduleControlId)
        {
			Logger.LogError(">>> Entering DeleteModuleControl(moduleControlId={ModuleControlId})", moduleControlId);
            this.ExecuteNonQuery("DeleteModuleControl", moduleControlId);
			Logger.LogWarning("<<< Exiting DeleteModuleControl(moduleControlId={ModuleControlId})", moduleControlId);
        }

        public virtual IDataReader GetModuleControls()
        {
			Logger.LogError(">>> Entering GetModuleControls()");
            Logger.LogWarning("<<< Exiting GetModuleControls()");
            return this.ExecuteReader("GetModuleControls");
        }

        public virtual void UpdateModuleControl(int moduleControlId, int moduleDefId, string controlKey, string controlTitle, string controlSrc, string iconFile, int controlType, int viewOrder, string helpUrl, bool supportsPartialRendering, bool supportsPopUps, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateModuleControl(moduleControlId={ModuleControlId},moduleDefId={ModuleDefId},controlKey={ControlKey},controlTitle={ControlTitle},controlSrc={ControlSrc},iconFile={IconFile},controlType={ControlType},viewOrder={ViewOrder},helpUrl={HelpUrl},supportsPartialRendering={SupportsPartialRendering},supportsPopUps={SupportsPopUps},lastModifiedByUserID={LastModifiedByUserID})", moduleControlId, moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpUrl, supportsPartialRendering, supportsPopUps, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateModuleControl",
                moduleControlId,
                this.GetNull(moduleDefId),
                this.GetNull(controlKey),
                this.GetNull(controlTitle),
                controlSrc,
                this.GetNull(iconFile),
                controlType,
                this.GetNull(viewOrder),
                this.GetNull(helpUrl),
                supportsPartialRendering,
                supportsPopUps,
                lastModifiedByUserID);
				Logger.LogError("UpdateModuleControl(moduleControlId,moduleDefId,controlKey,controlTitle,controlSrc,iconFile,controlType,viewOrder,helpUrl,supportsPartialRendering,supportsPopUps,lastModifiedByUserID)={ModuleControlId},{ModuleDefId},{ControlKey},{ControlTitle},{ControlSrc},{IconFile},{ControlType},{ViewOrder},{HelpUrl},{SupportsPartialRendering},{SupportsPopUps},{LastModifiedByUserID}: database query executed in {Arg12} ms", moduleControlId, moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpUrl, supportsPartialRendering, supportsPopUps, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateModuleControl(moduleControlId={ModuleControlId},moduleDefId={ModuleDefId},controlKey={ControlKey},controlTitle={ControlTitle},controlSrc={ControlSrc},iconFile={IconFile},controlType={ControlType},viewOrder={ViewOrder},helpUrl={HelpUrl},supportsPartialRendering={SupportsPartialRendering},supportsPopUps={SupportsPopUps},lastModifiedByUserID={LastModifiedByUserID})", moduleControlId, moduleDefId, controlKey, controlTitle, controlSrc, iconFile, controlType, viewOrder, helpUrl, supportsPartialRendering, supportsPopUps, lastModifiedByUserID);
        }

        public virtual int AddFolder(int portalId, Guid uniqueId, Guid versionGuid, string folderPath, string mappedPath, int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated, int createdByUserId, int folderMappingId, bool isVersioned, int workflowId, int parentId)
        {
			Logger.LogError(">>> Entering AddFolder(portalId={PortalId},uniqueId={UniqueId},versionGuid={VersionGuid},folderPath={FolderPath},mappedPath={MappedPath},storageLocation={StorageLocation},isProtected={IsProtected},isCached={IsCached},lastUpdated={LastUpdated},createdByUserId={CreatedByUserId},folderMappingId=<redacted>,isVersioned={IsVersioned},workflowId={WorkflowId},parentId={ParentId})", portalId, uniqueId, versionGuid, folderPath, mappedPath, storageLocation, isProtected, isCached, lastUpdated, createdByUserId, isVersioned, workflowId, parentId);
			Logger.LogWarning("<<< Exiting AddFolder(portalId={PortalId},uniqueId={UniqueId},versionGuid={VersionGuid},folderPath={FolderPath},mappedPath={MappedPath},storageLocation={StorageLocation},isProtected={IsProtected},isCached={IsCached},lastUpdated={LastUpdated},createdByUserId={CreatedByUserId},folderMappingId=<redacted>,isVersioned={IsVersioned},workflowId={WorkflowId},parentId={ParentId})", portalId, uniqueId, versionGuid, folderPath, mappedPath, storageLocation, isProtected, isCached, lastUpdated, createdByUserId, isVersioned, workflowId, parentId);
            return this.ExecuteScalar<int>(
                "AddFolder",
                this.GetNull(portalId),
                uniqueId,
                versionGuid,
                folderPath,
                mappedPath,
                storageLocation,
                isProtected,
                isCached,
                this.GetNull(lastUpdated),
                createdByUserId,
                folderMappingId,
                isVersioned,
                this.GetNull(workflowId),
                this.GetNull(parentId));
        }

        public virtual void DeleteFolder(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering DeleteFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            this.ExecuteNonQuery("DeleteFolder", this.GetNull(portalId), folderPath);
			Logger.LogWarning("<<< Exiting DeleteFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
        }

        public virtual IDataReader GetFolder(int folderId)
        {
			Logger.LogError(">>> Entering GetFolder(folderId={FolderId})", folderId);
            Logger.LogWarning("<<< Exiting GetFolder(folderId={FolderId})", folderId);
            return this.ExecuteReader("GetFolderByFolderID", folderId);
        }

        public virtual IDataReader GetFolder(int portalId, string folderPath)
        {
            return this.ExecuteReader("GetFolderByFolderPath", this.GetNull(portalId), folderPath);
        }

        public virtual IDataReader GetFolderByUniqueID(Guid uniqueId)
        {
			Logger.LogError(">>> Entering GetFolderByUniqueID(uniqueId={UniqueId})", uniqueId);
            Logger.LogWarning("<<< Exiting GetFolderByUniqueID(uniqueId={UniqueId})", uniqueId);
            return this.ExecuteReader("GetFolderByUniqueID", uniqueId);
        }

        public virtual IDataReader GetFoldersByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetFoldersByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetFoldersByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetFolders", this.GetNull(portalId));
        }

        public virtual IDataReader GetFoldersByPortalAndPermissions(int portalId, string permissions, int userId)
        {
			Logger.LogError(">>> Entering GetFoldersByPortalAndPermissions(portalId={PortalId},permissions={Permissions},userId={UserId})", portalId, permissions, userId);
            Logger.LogWarning("<<< Exiting GetFoldersByPortalAndPermissions(portalId={PortalId},permissions={Permissions},userId={UserId})", portalId, permissions, userId);
            return this.ExecuteReader("GetFoldersByPermissions", this.GetNull(portalId), this.GetNull(permissions), this.GetNull(userId), -1, string.Empty);
        }

        public virtual int GetLegacyFolderCount()
        {
			Logger.LogError(">>> Entering GetLegacyFolderCount()");
            Logger.LogWarning("<<< Exiting GetLegacyFolderCount()");
            return this.ExecuteScalar<int>("GetLegacyFolderCount");
        }

        public virtual void UpdateFolder(int portalId, Guid versionGuid, int folderId, string folderPath, int storageLocation, string mappedPath, bool isProtected, bool isCached, DateTime lastUpdated, int lastModifiedByUserID, int folderMappingID, bool isVersioned, int workflowID, int parentID)
        {
			Logger.LogError(">>> Entering UpdateFolder(portalId={PortalId},versionGuid={VersionGuid},folderId={FolderId},folderPath={FolderPath},storageLocation={StorageLocation},mappedPath={MappedPath},isProtected={IsProtected},isCached={IsCached},lastUpdated={LastUpdated},lastModifiedByUserID={LastModifiedByUserID},folderMappingID=<redacted>,isVersioned={IsVersioned},workflowID={WorkflowID},parentID={ParentID})", portalId, versionGuid, folderId, folderPath, storageLocation, mappedPath, isProtected, isCached, lastUpdated, lastModifiedByUserID, isVersioned, workflowID, parentID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFolder",
                this.GetNull(portalId),
                versionGuid,
                folderId,
                folderPath,
                mappedPath,
                storageLocation,
                isProtected,
                isCached,
                this.GetNull(lastUpdated),
                lastModifiedByUserID,
                folderMappingID,
                isVersioned,
                this.GetNull(workflowID),
                this.GetNull(parentID));
				Logger.LogError("UpdateFolder(portalId,versionGuid,folderId,folderPath,storageLocation,mappedPath,isProtected,isCached,lastUpdated,lastModifiedByUserID,folderMappingID,isVersioned,workflowID,parentID)={PortalId},{VersionGuid},{FolderId},{FolderPath},{StorageLocation},{MappedPath},{IsProtected},{IsCached},{LastUpdated},{LastModifiedByUserID},<redacted>,{IsVersioned},{WorkflowID},{ParentID}: database query executed in {Arg14} ms", portalId, versionGuid, folderId, folderPath, storageLocation, mappedPath, isProtected, isCached, lastUpdated, lastModifiedByUserID, isVersioned, workflowID, parentID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFolder(portalId={PortalId},versionGuid={VersionGuid},folderId={FolderId},folderPath={FolderPath},storageLocation={StorageLocation},mappedPath={MappedPath},isProtected={IsProtected},isCached={IsCached},lastUpdated={LastUpdated},lastModifiedByUserID={LastModifiedByUserID},folderMappingID=<redacted>,isVersioned={IsVersioned},workflowID={WorkflowID},parentID={ParentID})", portalId, versionGuid, folderId, folderPath, storageLocation, mappedPath, isProtected, isCached, lastUpdated, lastModifiedByUserID, isVersioned, workflowID, parentID);
        }

        public virtual void UpdateFolderVersion(int folderId, Guid versionGuid)
        {
			Logger.LogError(">>> Entering UpdateFolderVersion(folderId={FolderId},versionGuid={VersionGuid})", folderId, versionGuid);
            this.ExecuteNonQuery("UpdateFolderVersion", folderId, versionGuid);
			Logger.LogWarning("<<< Exiting UpdateFolderVersion(folderId={FolderId},versionGuid={VersionGuid})", folderId, versionGuid);
        }

        public virtual IDataReader UpdateLegacyFolders()
        {
			Logger.LogError(">>> Entering UpdateLegacyFolders()");
            Logger.LogWarning("<<< Exiting UpdateLegacyFolders()");
            return this.ExecuteReader("UpdateLegacyFolders");
        }

        public virtual int AddFile(int portalId, Guid uniqueId, Guid versionGuid, string fileName, string extension, long size, int width, int height, string contentType, string folder, int folderId, int createdByUserID, string hash, DateTime lastModificationTime, string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
        {
			Logger.LogError(">>> Entering AddFile(portalId={PortalId},uniqueId={UniqueId},versionGuid={VersionGuid},fileName={FileName},extension={Extension},size={Size},width={Width},height={Height},contentType={ContentType},folder={Folder},folderId={FolderId},createdByUserID={CreatedByUserID},hash=<redacted>,lastModificationTime={LastModificationTime},title={Title},description={Description},startDate={StartDate},endDate={EndDate},enablePublishPeriod={EnablePublishPeriod},contentItemId={ContentItemId})", portalId, uniqueId, versionGuid, fileName, extension, size, width, height, contentType, folder, folderId, createdByUserID, lastModificationTime, title, description, startDate, endDate, enablePublishPeriod, contentItemId);
			Logger.LogWarning("<<< Exiting AddFile(portalId={PortalId},uniqueId={UniqueId},versionGuid={VersionGuid},fileName={FileName},extension={Extension},size={Size},width={Width},height={Height},contentType={ContentType},folder={Folder},folderId={FolderId},createdByUserID={CreatedByUserID},hash=<redacted>,lastModificationTime={LastModificationTime},title={Title},description={Description},startDate={StartDate},endDate={EndDate},enablePublishPeriod={EnablePublishPeriod},contentItemId={ContentItemId})", portalId, uniqueId, versionGuid, fileName, extension, size, width, height, contentType, folder, folderId, createdByUserID, lastModificationTime, title, description, startDate, endDate, enablePublishPeriod, contentItemId);
            return this.ExecuteScalar<int>(
                "AddFile",
                this.GetNull(portalId),
                uniqueId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folder,
                folderId,
                createdByUserID,
                hash,
                lastModificationTime,
                title,
                description,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemId));
        }

        public virtual void SetFileHasBeenPublished(int fileId, bool hasBeenPublished)
        {
			Logger.LogError(">>> Entering SetFileHasBeenPublished(fileId={FileId},hasBeenPublished={HasBeenPublished})", fileId, hasBeenPublished);
            this.ExecuteNonQuery("SetFileHasBeenPublished", fileId, hasBeenPublished);
			Logger.LogWarning("<<< Exiting SetFileHasBeenPublished(fileId={FileId},hasBeenPublished={HasBeenPublished})", fileId, hasBeenPublished);
        }

        public virtual int CountLegacyFiles()
        {
			Logger.LogError(">>> Entering CountLegacyFiles()");
            Logger.LogWarning("<<< Exiting CountLegacyFiles()");
            return this.ExecuteScalar<int>("CountLegacyFiles");
        }

        public virtual void ClearFileContent(int fileId)
        {
			Logger.LogError(">>> Entering ClearFileContent(fileId={FileId})", fileId);
            this.ExecuteNonQuery("ClearFileContent", fileId);
			Logger.LogWarning("<<< Exiting ClearFileContent(fileId={FileId})", fileId);
        }

        public virtual void DeleteFile(int portalId, string fileName, int folderId)
        {
			Logger.LogError(">>> Entering DeleteFile(portalId={PortalId},fileName={FileName},folderId={FolderId})", portalId, fileName, folderId);
            this.ExecuteNonQuery("DeleteFile", this.GetNull(portalId), fileName, folderId);
			Logger.LogWarning("<<< Exiting DeleteFile(portalId={PortalId},fileName={FileName},folderId={FolderId})", portalId, fileName, folderId);
        }

        public virtual void DeleteFiles(int portalId)
        {
			Logger.LogError(">>> Entering DeleteFiles(portalId={PortalId})", portalId);
            this.ExecuteNonQuery("DeleteFiles", this.GetNull(portalId));
			Logger.LogWarning("<<< Exiting DeleteFiles(portalId={PortalId})", portalId);
        }

        public virtual DataTable GetAllFiles()
        {
			Logger.LogError(">>> Entering GetAllFiles()");
            Logger.LogWarning("<<< Exiting GetAllFiles()");
            return Globals.ConvertDataReaderToDataTable(this.ExecuteReader("GetAllFiles"));
        }

        public virtual IDataReader GetFile(string fileName, int folderId, bool retrieveUnpublishedFiles = false)
        {
			Logger.LogError(">>> Entering GetFile(fileName={FileName},folderId={FolderId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", fileName, folderId, retrieveUnpublishedFiles);
            Logger.LogWarning("<<< Exiting GetFile(fileName={FileName},folderId={FolderId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", fileName, folderId, retrieveUnpublishedFiles);
            return this.ExecuteReader("GetFile", fileName, folderId, retrieveUnpublishedFiles);
        }

        public virtual IDataReader GetFileById(int fileId, bool retrieveUnpublishedFiles = false)
        {
			Logger.LogError(">>> Entering GetFileById(fileId={FileId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", fileId, retrieveUnpublishedFiles);
            Logger.LogWarning("<<< Exiting GetFileById(fileId={FileId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", fileId, retrieveUnpublishedFiles);
            return this.ExecuteReader("GetFileById", fileId, retrieveUnpublishedFiles);
        }

        public virtual IDataReader GetFileByUniqueID(Guid uniqueId)
        {
			Logger.LogError(">>> Entering GetFileByUniqueID(uniqueId={UniqueId})", uniqueId);
            Logger.LogWarning("<<< Exiting GetFileByUniqueID(uniqueId={UniqueId})", uniqueId);
            return this.ExecuteReader("GetFileByUniqueID", uniqueId);
        }

        public virtual IDataReader GetFileContent(int fileId)
        {
			Logger.LogError(">>> Entering GetFileContent(fileId={FileId})", fileId);
            Logger.LogWarning("<<< Exiting GetFileContent(fileId={FileId})", fileId);
            return this.ExecuteReader("GetFileContent", fileId);
        }

        public virtual IDataReader GetFileVersionContent(int fileId, int version)
        {
			Logger.LogError(">>> Entering GetFileVersionContent(fileId={FileId},version={Version})", fileId, version);
            Logger.LogWarning("<<< Exiting GetFileVersionContent(fileId={FileId},version={Version})", fileId, version);
            return this.ExecuteReader("GetFileVersionContent", fileId, version);
        }

        public virtual IDataReader GetFiles(int folderId, bool retrieveUnpublishedFiles, bool recursive)
        {
			Logger.LogError(">>> Entering GetFiles(folderId={FolderId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles},recursive={Recursive})", folderId, retrieveUnpublishedFiles, recursive);
            Logger.LogWarning("<<< Exiting GetFiles(folderId={FolderId},retrieveUnpublishedFiles={RetrieveUnpublishedFiles},recursive={Recursive})", folderId, retrieveUnpublishedFiles, recursive);
            return this.ExecuteReader("GetFiles", folderId, retrieveUnpublishedFiles, recursive);
        }

        /// <summary>
        /// This is an internal method for communication between DNN business layer and SQL database.
        /// Do not use in custom modules, please use API (DotNetNuke.Services.FileSystem.FileManager.UpdateFile)
        ///
        /// Stores information about a specific file, stored in DNN filesystem
        /// calling petapoco method to call the underlying stored procedure "UpdateFile".
        /// </summary>
        /// <param name="fileId">ID of the (already existing) file.</param>
        /// <param name="versionGuid">GUID of this file version  (should usually not be modified).</param>
        /// <param name="fileName">Name of the file in the file system (including extension).</param>
        /// <param name="extension">File type - should meet extension in FileName.</param>
        /// <param name="size">Size of file (bytes).</param>
        /// <param name="width">Width of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file sync).</param>
        /// <param name="height">Height of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file snyc).</param>
        /// <param name="contentType">MIME type of the file.</param>
        /// <param name="folderId">ID of the folder, the file resides in.</param>
        /// <param name="lastModifiedByUserID">ID of the user, who performed last update of file or file info.</param>
        /// <param name="hash">SHa1 hash of the file content, used for file versioning (lazy load: pass Null, will be generated by DNN platform on db file sync).</param>
        /// <param name="lastModificationTime">timestamp, when last update of file or file info happened.</param>
        /// <param name="title">Display title of the file - optional (pass Null if not provided).</param>
        /// <param name="description">Description of the file.</param>
        /// <param name="startDate">date and time (server TZ), from which the file should be displayed/accessible (according to folder permission).</param>
        /// <param name="endDate">date and time (server TZ), until which the file should be displayed/accessible (according to folder permission).</param>
        /// <param name="enablePublishPeriod">shall startdate/end date be used?.</param>
        /// <param name="contentItemId">ID of the associated contentitem with description etc. (optional).</param>
        public virtual void UpdateFile(int fileId, Guid versionGuid, string fileName, string extension, long size, int width, int height, string contentType, int folderId, int lastModifiedByUserID, string hash, DateTime lastModificationTime, string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
        {
			Logger.LogError(">>> Entering UpdateFile(fileId={FileId},versionGuid={VersionGuid},fileName={FileName},extension={Extension},size={Size},width={Width},height={Height},contentType={ContentType},folderId={FolderId},lastModifiedByUserID={LastModifiedByUserID},hash=<redacted>,lastModificationTime={LastModificationTime},title={Title},description={Description},startDate={StartDate},endDate={EndDate},enablePublishPeriod={EnablePublishPeriod},contentItemId={ContentItemId})", fileId, versionGuid, fileName, extension, size, width, height, contentType, folderId, lastModifiedByUserID, lastModificationTime, title, description, startDate, endDate, enablePublishPeriod, contentItemId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFile",
                fileId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folderId,
                lastModifiedByUserID,
                hash,
                lastModificationTime,
                title,
                description,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemId));
				Logger.LogError("UpdateFile(fileId,versionGuid,fileName,extension,size,width,height,contentType,folderId,lastModifiedByUserID,hash,lastModificationTime,title,description,startDate,endDate,enablePublishPeriod,contentItemId)={FileId},{VersionGuid},{FileName},{Extension},{Size},{Width},{Height},{ContentType},{FolderId},{LastModifiedByUserID},<redacted>,{LastModificationTime},{Title},{Description},{StartDate},{EndDate},{EnablePublishPeriod},{ContentItemId}: database query executed in {Arg18} ms", fileId, versionGuid, fileName, extension, size, width, height, contentType, folderId, lastModifiedByUserID, lastModificationTime, title, description, startDate, endDate, enablePublishPeriod, contentItemId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFile(fileId={FileId},versionGuid={VersionGuid},fileName={FileName},extension={Extension},size={Size},width={Width},height={Height},contentType={ContentType},folderId={FolderId},lastModifiedByUserID={LastModifiedByUserID},hash=<redacted>,lastModificationTime={LastModificationTime},title={Title},description={Description},startDate={StartDate},endDate={EndDate},enablePublishPeriod={EnablePublishPeriod},contentItemId={ContentItemId})", fileId, versionGuid, fileName, extension, size, width, height, contentType, folderId, lastModifiedByUserID, lastModificationTime, title, description, startDate, endDate, enablePublishPeriod, contentItemId);
        }

        public virtual void UpdateFileLastModificationTime(int fileId, DateTime lastModificationTime)
        {
			Logger.LogError(">>> Entering UpdateFileLastModificationTime(fileId={FileId},lastModificationTime={LastModificationTime})", fileId, lastModificationTime);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFileLastModificationTime",
                fileId,
                lastModificationTime);
				Logger.LogError("UpdateFileLastModificationTime(fileId,lastModificationTime)={FileId},{LastModificationTime}: database query executed in {Arg2} ms", fileId, lastModificationTime, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFileLastModificationTime(fileId={FileId},lastModificationTime={LastModificationTime})", fileId, lastModificationTime);
        }

        public virtual void UpdateFileHashCode(int fileId, string hashCode)
        {
			Logger.LogError(">>> Entering UpdateFileHashCode(fileId={FileId},hashCode=<redacted>)", fileId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFileHashCode",
                fileId,
                hashCode);
				Logger.LogError("UpdateFileHashCode(fileId,hashCode)={FileId},<redacted>: database query executed in {Arg2} ms", fileId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFileHashCode(fileId={FileId},hashCode=<redacted>)", fileId);
        }

        public virtual void UpdateFileContent(int fileId, byte[] content)
        {
			Logger.LogError(">>> Entering UpdateFileContent(fileId={FileId},content={Content})", fileId, content);
            this.ExecuteNonQuery("UpdateFileContent", fileId, this.GetNull(content));
			Logger.LogWarning("<<< Exiting UpdateFileContent(fileId={FileId},content={Content})", fileId, content);
        }

        public virtual void UpdateFileVersion(int fileId, Guid versionGuid)
        {
			Logger.LogError(">>> Entering UpdateFileVersion(fileId={FileId},versionGuid={VersionGuid})", fileId, versionGuid);
            this.ExecuteNonQuery("UpdateFileVersion", fileId, versionGuid);
			Logger.LogWarning("<<< Exiting UpdateFileVersion(fileId={FileId},versionGuid={VersionGuid})", fileId, versionGuid);
        }

        public virtual int AddPermission(string permissionCode, int moduleDefID, string permissionKey, string permissionName, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddPermission(permissionCode={PermissionCode},moduleDefID={ModuleDefID},permissionKey={PermissionKey},permissionName={PermissionName},createdByUserID={CreatedByUserID})", permissionCode, moduleDefID, permissionKey, permissionName, createdByUserID);
            Logger.LogWarning("<<< Exiting AddPermission(permissionCode={PermissionCode},moduleDefID={ModuleDefID},permissionKey={PermissionKey},permissionName={PermissionName},createdByUserID={CreatedByUserID})", permissionCode, moduleDefID, permissionKey, permissionName, createdByUserID);
            return this.ExecuteScalar<int>("AddPermission", moduleDefID, permissionCode, permissionKey, permissionName, createdByUserID);
        }

        public virtual void DeletePermission(int permissionID)
        {
			Logger.LogError(">>> Entering DeletePermission(permissionID={PermissionID})", permissionID);
            this.ExecuteNonQuery("DeletePermission", permissionID);
			Logger.LogWarning("<<< Exiting DeletePermission(permissionID={PermissionID})", permissionID);
        }

        public virtual void UpdatePermission(int permissionID, string permissionCode, int moduleDefID, string permissionKey, string permissionName, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdatePermission(permissionID={PermissionID},permissionCode={PermissionCode},moduleDefID={ModuleDefID},permissionKey={PermissionKey},permissionName={PermissionName},lastModifiedByUserID={LastModifiedByUserID})", permissionID, permissionCode, moduleDefID, permissionKey, permissionName, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdatePermission", permissionID, permissionCode, moduleDefID, permissionKey, permissionName, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdatePermission(permissionID={PermissionID},permissionCode={PermissionCode},moduleDefID={ModuleDefID},permissionKey={PermissionKey},permissionName={PermissionName},lastModifiedByUserID={LastModifiedByUserID})", permissionID, permissionCode, moduleDefID, permissionKey, permissionName, lastModifiedByUserID);
        }

        public virtual int AddModulePermission(int moduleId, int portalId, int permissionId, int roleId, bool allowAccess, int userId, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddModulePermission(moduleId={ModuleId},portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", moduleId, portalId, permissionId, roleId, allowAccess, userId, createdByUserId);
            Logger.LogWarning("<<< Exiting AddModulePermission(moduleId={ModuleId},portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", moduleId, portalId, permissionId, roleId, allowAccess, userId, createdByUserId);
            return this.ExecuteScalar<int>(
                "AddModulePermission",
                moduleId,
                portalId,
                permissionId,
                GetRoleNull(roleId),
                allowAccess,
                this.GetNull(userId),
                createdByUserId);
        }

        public virtual void DeleteModulePermission(int modulePermissionId)
        {
			Logger.LogError(">>> Entering DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
            this.ExecuteNonQuery("DeleteModulePermission", modulePermissionId);
			Logger.LogWarning("<<< Exiting DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
        }

        public virtual void DeleteModulePermissionsByModuleID(int moduleId, int portalId)
        {
			Logger.LogError(">>> Entering DeleteModulePermissionsByModuleID(moduleId={ModuleId},portalId={PortalId})", moduleId, portalId);
            this.ExecuteNonQuery("DeleteModulePermissionsByModuleID", moduleId, portalId);
			Logger.LogWarning("<<< Exiting DeleteModulePermissionsByModuleID(moduleId={ModuleId},portalId={PortalId})", moduleId, portalId);
        }

        public virtual void DeleteModulePermissionsByUserID(int portalId, int userId)
        {
			Logger.LogError(">>> Entering DeleteModulePermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("DeleteModulePermissionsByUserID", portalId, userId);
			Logger.LogWarning("<<< Exiting DeleteModulePermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual IDataReader GetModulePermission(int modulePermissionId)
        {
			Logger.LogError(">>> Entering GetModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
            Logger.LogWarning("<<< Exiting GetModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
            return this.ExecuteReader("GetModulePermission", modulePermissionId);
        }

        public virtual IDataReader GetModulePermissionsByModuleID(int moduleID, int permissionId)
        {
			Logger.LogError(">>> Entering GetModulePermissionsByModuleID(moduleID={ModuleID},permissionId={PermissionId})", moduleID, permissionId);
            Logger.LogWarning("<<< Exiting GetModulePermissionsByModuleID(moduleID={ModuleID},permissionId={PermissionId})", moduleID, permissionId);
            return this.ExecuteReader("GetModulePermissionsByModuleID", moduleID, permissionId);
        }

        public virtual IDataReader GetModulePermissionsByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetModulePermissionsByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetModulePermissionsByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetModulePermissionsByPortal", portalId);
        }

        public virtual IDataReader GetModulePermissionsByTabID(int tabId)
        {
			Logger.LogError(">>> Entering GetModulePermissionsByTabID(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetModulePermissionsByTabID(tabId={TabId})", tabId);
            return this.ExecuteReader("GetModulePermissionsByTabID", tabId);
        }

        public virtual void UpdateModulePermission(int modulePermissionId, int moduleId, int portalId, int permissionId, int roleId, bool allowAccess, int userId, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateModulePermission(modulePermissionId={ModulePermissionId},moduleId={ModuleId},portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserId={LastModifiedByUserId})", modulePermissionId, moduleId, portalId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateModulePermission",
                modulePermissionId,
                moduleId,
                portalId,
                permissionId,
                GetRoleNull(roleId),
                allowAccess,
                this.GetNull(userId),
                lastModifiedByUserId);
				Logger.LogError("UpdateModulePermission(modulePermissionId,moduleId,portalId,permissionId,roleId,allowAccess,userId,lastModifiedByUserId)={ModulePermissionId},{ModuleId},{PortalId},{PermissionId},{RoleId},{AllowAccess},{UserId},{LastModifiedByUserId}: database query executed in {Arg8} ms", modulePermissionId, moduleId, portalId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateModulePermission(modulePermissionId={ModulePermissionId},moduleId={ModuleId},portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserId={LastModifiedByUserId})", modulePermissionId, moduleId, portalId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId);
        }

        public virtual int AddTabPermission(int tabId, int permissionId, int roleID, bool allowAccess, int userId, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddTabPermission(tabId={TabId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},createdByUserID={CreatedByUserID})", tabId, permissionId, roleID, allowAccess, userId, createdByUserID);
            Logger.LogWarning("<<< Exiting AddTabPermission(tabId={TabId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},createdByUserID={CreatedByUserID})", tabId, permissionId, roleID, allowAccess, userId, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddTabPermission",
                tabId,
                permissionId,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                createdByUserID);
        }

        public virtual void DeleteTabPermission(int tabPermissionId)
        {
			Logger.LogError(">>> Entering DeleteTabPermission(tabPermissionId={TabPermissionId})", tabPermissionId);
            this.ExecuteNonQuery("DeleteTabPermission", tabPermissionId);
			Logger.LogWarning("<<< Exiting DeleteTabPermission(tabPermissionId={TabPermissionId})", tabPermissionId);
        }

        public virtual void DeleteTabPermissionsByTabID(int tabId)
        {
			Logger.LogError(">>> Entering DeleteTabPermissionsByTabID(tabId={TabId})", tabId);
            this.ExecuteNonQuery("DeleteTabPermissionsByTabID", tabId);
			Logger.LogWarning("<<< Exiting DeleteTabPermissionsByTabID(tabId={TabId})", tabId);
        }

        public virtual void DeleteTabPermissionsByUserID(int portalId, int userId)
        {
			Logger.LogError(">>> Entering DeleteTabPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("DeleteTabPermissionsByUserID", portalId, userId);
			Logger.LogWarning("<<< Exiting DeleteTabPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual IDataReader GetTabPermissionsByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetTabPermissionsByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetTabPermissionsByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetTabPermissionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabPermissionsByTabID(int tabId, int permissionId)
        {
			Logger.LogError(">>> Entering GetTabPermissionsByTabID(tabId={TabId},permissionId={PermissionId})", tabId, permissionId);
            Logger.LogWarning("<<< Exiting GetTabPermissionsByTabID(tabId={TabId},permissionId={PermissionId})", tabId, permissionId);
            return this.ExecuteReader("GetTabPermissionsByTabID", tabId, permissionId);
        }

        public virtual void UpdateTabPermission(int tabPermissionId, int tabId, int permissionId, int roleID, bool allowAccess, int userId, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateTabPermission(tabPermissionId={TabPermissionId},tabId={TabId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserID={LastModifiedByUserID})", tabPermissionId, tabId, permissionId, roleID, allowAccess, userId, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateTabPermission",
                tabPermissionId,
                tabId,
                permissionId,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                lastModifiedByUserID);
				Logger.LogError("UpdateTabPermission(tabPermissionId,tabId,permissionId,roleID,allowAccess,userId,lastModifiedByUserID)={TabPermissionId},{TabId},{PermissionId},{RoleID},{AllowAccess},{UserId},{LastModifiedByUserID}: database query executed in {Arg7} ms", tabPermissionId, tabId, permissionId, roleID, allowAccess, userId, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateTabPermission(tabPermissionId={TabPermissionId},tabId={TabId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserID={LastModifiedByUserID})", tabPermissionId, tabId, permissionId, roleID, allowAccess, userId, lastModifiedByUserID);
        }

        public virtual int AddFolderPermission(int folderId, int permissionId, int roleID, bool allowAccess, int userId, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddFolderPermission(folderId={FolderId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},createdByUserID={CreatedByUserID})", folderId, permissionId, roleID, allowAccess, userId, createdByUserID);
            Logger.LogWarning("<<< Exiting AddFolderPermission(folderId={FolderId},permissionId={PermissionId},roleID={RoleID},allowAccess={AllowAccess},userId={UserId},createdByUserID={CreatedByUserID})", folderId, permissionId, roleID, allowAccess, userId, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddFolderPermission",
                folderId,
                permissionId,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                createdByUserID);
        }

        public virtual IDataReader GetPortalPermissionsByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalPermissionsByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPortalPermissionsByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetPortalPermissionsByPortal", this.GetNull(portalId));
        }

        public virtual int AddPortalPermission(int portalId, int permissionId, int roleId, bool allowAccess, int userId, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddPortalPermission(portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", portalId, permissionId, roleId, allowAccess, userId, createdByUserId);
            Logger.LogWarning("<<< Exiting AddPortalPermission(portalId={PortalId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", portalId, permissionId, roleId, allowAccess, userId, createdByUserId);
            return this.ExecuteScalar<int>(
                "SaveTabPermission",
                portalId,
                permissionId,
                GetRoleNull(roleId),
                allowAccess,
                this.GetNull(userId),
                createdByUserId);
        }

        public virtual void DeletePortalPermission(int portalPermissionId)
        {
			Logger.LogError(">>> Entering DeletePortalPermission(portalPermissionId={PortalPermissionId})", portalPermissionId);
            this.ExecuteNonQuery("DeletePortalPermission", portalPermissionId);
			Logger.LogWarning("<<< Exiting DeletePortalPermission(portalPermissionId={PortalPermissionId})", portalPermissionId);
        }

        public virtual void DeletePortalPermissionsByPortalID(int portalId)
        {
			Logger.LogError(">>> Entering DeletePortalPermissionsByPortalID(portalId={PortalId})", portalId);
            this.ExecuteNonQuery("DeletePortalPermissionsByPortalID", portalId);
			Logger.LogWarning("<<< Exiting DeletePortalPermissionsByPortalID(portalId={PortalId})", portalId);
        }

        public virtual void DeletePortalPermissionsByUserID(int portalId, int userId)
        {
			Logger.LogError(">>> Entering DeletePortalPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("DeletePortalPermissionsByUserID", portalId, userId);
			Logger.LogWarning("<<< Exiting DeletePortalPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual void DeleteFolderPermission(int folderPermissionId)
        {
			Logger.LogError(">>> Entering DeleteFolderPermission(folderPermissionId={FolderPermissionId})", folderPermissionId);
            this.ExecuteNonQuery("DeleteFolderPermission", folderPermissionId);
			Logger.LogWarning("<<< Exiting DeleteFolderPermission(folderPermissionId={FolderPermissionId})", folderPermissionId);
        }

        public virtual void DeleteFolderPermissionsByFolderPath(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering DeleteFolderPermissionsByFolderPath(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            this.ExecuteNonQuery("DeleteFolderPermissionsByFolderPath", this.GetNull(portalId), folderPath);
			Logger.LogWarning("<<< Exiting DeleteFolderPermissionsByFolderPath(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
        }

        public virtual void DeleteFolderPermissionsByUserID(int portalId, int userId)
        {
			Logger.LogError(">>> Entering DeleteFolderPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("DeleteFolderPermissionsByUserID", portalId, userId);
			Logger.LogWarning("<<< Exiting DeleteFolderPermissionsByUserID(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual IDataReader GetFolderPermission(int folderPermissionId)
        {
			Logger.LogError(">>> Entering GetFolderPermission(folderPermissionId={FolderPermissionId})", folderPermissionId);
            Logger.LogWarning("<<< Exiting GetFolderPermission(folderPermissionId={FolderPermissionId})", folderPermissionId);
            return this.ExecuteReader("GetFolderPermission", folderPermissionId);
        }

        public virtual IDataReader GetFolderPermissionsByFolderPath(int portalId, string folderPath, int permissionId)
        {
			Logger.LogError(">>> Entering GetFolderPermissionsByFolderPath(portalId={PortalId},folderPath={FolderPath},permissionId={PermissionId})", portalId, folderPath, permissionId);
            Logger.LogWarning("<<< Exiting GetFolderPermissionsByFolderPath(portalId={PortalId},folderPath={FolderPath},permissionId={PermissionId})", portalId, folderPath, permissionId);
            return this.ExecuteReader("GetFolderPermissionsByFolderPath", this.GetNull(portalId), folderPath, permissionId);
        }

        public virtual IDataReader GetFolderPermissionsByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetFolderPermissionsByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetFolderPermissionsByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetFolderPermissionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetFolderPermissionsByPortalAndPath(int portalId, string pathName)
        {
			Logger.LogError(">>> Entering GetFolderPermissionsByPortalAndPath(portalId={PortalId},pathName={PathName})", portalId, pathName);
            Logger.LogWarning("<<< Exiting GetFolderPermissionsByPortalAndPath(portalId={PortalId},pathName={PathName})", portalId, pathName);
            return this.ExecuteReader("GetFolderPermissionsByPortalAndPath", this.GetNull(portalId), pathName ?? string.Empty);
        }

        public virtual void UpdateFolderPermission(int folderPermissionID, int folderID, int permissionID, int roleID, bool allowAccess, int userID, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateFolderPermission(folderPermissionID={FolderPermissionID},folderID={FolderID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},lastModifiedByUserID={LastModifiedByUserID})", folderPermissionID, folderID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFolderPermission",
                folderPermissionID,
                folderID,
                permissionID,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userID),
                lastModifiedByUserID);
				Logger.LogError("UpdateFolderPermission(folderPermissionID,folderID,permissionID,roleID,allowAccess,userID,lastModifiedByUserID)={FolderPermissionID},{FolderID},{PermissionID},{RoleID},{AllowAccess},{UserID},{LastModifiedByUserID}: database query executed in {Arg7} ms", folderPermissionID, folderID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFolderPermission(folderPermissionID={FolderPermissionID},folderID={FolderID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},lastModifiedByUserID={LastModifiedByUserID})", folderPermissionID, folderID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID);
        }

        public virtual int AddDesktopModulePermission(int portalDesktopModuleID, int permissionID, int roleID, bool allowAccess, int userID, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddDesktopModulePermission(portalDesktopModuleID={PortalDesktopModuleID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},createdByUserID={CreatedByUserID})", portalDesktopModuleID, permissionID, roleID, allowAccess, userID, createdByUserID);
            Logger.LogWarning("<<< Exiting AddDesktopModulePermission(portalDesktopModuleID={PortalDesktopModuleID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},createdByUserID={CreatedByUserID})", portalDesktopModuleID, permissionID, roleID, allowAccess, userID, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddDesktopModulePermission",
                portalDesktopModuleID,
                permissionID,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userID),
                createdByUserID);
        }

        public virtual void DeleteDesktopModulePermission(int desktopModulePermissionID)
        {
			Logger.LogError(">>> Entering DeleteDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
            this.ExecuteNonQuery("DeleteDesktopModulePermission", desktopModulePermissionID);
			Logger.LogWarning("<<< Exiting DeleteDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
        }

        public virtual void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
			Logger.LogError(">>> Entering DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
            this.ExecuteNonQuery("DeleteDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
			Logger.LogWarning("<<< Exiting DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
        }

        public virtual void DeleteDesktopModulePermissionsByUserID(int userID, int portalID)
        {
			Logger.LogError(">>> Entering DeleteDesktopModulePermissionsByUserID(userID={UserID},portalID={PortalID})", userID, portalID);
            this.ExecuteNonQuery("DeleteDesktopModulePermissionsByUserID", userID, portalID);
			Logger.LogWarning("<<< Exiting DeleteDesktopModulePermissionsByUserID(userID={UserID},portalID={PortalID})", userID, portalID);
        }

        public virtual IDataReader GetDesktopModulePermission(int desktopModulePermissionID)
        {
			Logger.LogError(">>> Entering GetDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
            Logger.LogWarning("<<< Exiting GetDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
            return this.ExecuteReader("GetDesktopModulePermission", desktopModulePermissionID);
        }

        public virtual IDataReader GetDesktopModulePermissions()
        {
			Logger.LogError(">>> Entering GetDesktopModulePermissions()");
            Logger.LogWarning("<<< Exiting GetDesktopModulePermissions()");
            return this.ExecuteReader("GetDesktopModulePermissions");
        }

        public virtual IDataReader GetDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
			Logger.LogError(">>> Entering GetDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
            Logger.LogWarning("<<< Exiting GetDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
            return this.ExecuteReader("GetDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
        }

        public virtual void UpdateDesktopModulePermission(int desktopModulePermissionID, int portalDesktopModuleID, int permissionID, int roleID, bool allowAccess, int userID, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID},portalDesktopModuleID={PortalDesktopModuleID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},lastModifiedByUserID={LastModifiedByUserID})", desktopModulePermissionID, portalDesktopModuleID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateDesktopModulePermission",
                desktopModulePermissionID,
                portalDesktopModuleID,
                permissionID,
                GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userID),
                lastModifiedByUserID);
				Logger.LogError("UpdateDesktopModulePermission(desktopModulePermissionID,portalDesktopModuleID,permissionID,roleID,allowAccess,userID,lastModifiedByUserID)={DesktopModulePermissionID},{PortalDesktopModuleID},{PermissionID},{RoleID},{AllowAccess},{UserID},{LastModifiedByUserID}: database query executed in {Arg7} ms", desktopModulePermissionID, portalDesktopModuleID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID},portalDesktopModuleID={PortalDesktopModuleID},permissionID={PermissionID},roleID={RoleID},allowAccess={AllowAccess},userID={UserID},lastModifiedByUserID={LastModifiedByUserID})", desktopModulePermissionID, portalDesktopModuleID, permissionID, roleID, allowAccess, userID, lastModifiedByUserID);
        }

        public virtual int AddRoleGroup(int portalId, string groupName, string description, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddRoleGroup(portalId={PortalId},groupName={GroupName},description={Description},createdByUserID={CreatedByUserID})", portalId, groupName, description, createdByUserID);
            Logger.LogWarning("<<< Exiting AddRoleGroup(portalId={PortalId},groupName={GroupName},description={Description},createdByUserID={CreatedByUserID})", portalId, groupName, description, createdByUserID);
            return this.ExecuteScalar<int>("AddRoleGroup", portalId, groupName, description, createdByUserID);
        }

        public virtual void DeleteRoleGroup(int roleGroupId)
        {
			Logger.LogError(">>> Entering DeleteRoleGroup(roleGroupId={RoleGroupId})", roleGroupId);
            this.ExecuteNonQuery("DeleteRoleGroup", roleGroupId);
			Logger.LogWarning("<<< Exiting DeleteRoleGroup(roleGroupId={RoleGroupId})", roleGroupId);
        }

        public virtual IDataReader GetRoleGroup(int portalId, int roleGroupId)
        {
			Logger.LogError(">>> Entering GetRoleGroup(portalId={PortalId},roleGroupId={RoleGroupId})", portalId, roleGroupId);
            Logger.LogWarning("<<< Exiting GetRoleGroup(portalId={PortalId},roleGroupId={RoleGroupId})", portalId, roleGroupId);
            return this.ExecuteReader("GetRoleGroup", portalId, roleGroupId);
        }

        public virtual IDataReader GetRoleGroupByName(int portalID, string roleGroupName)
        {
			Logger.LogError(">>> Entering GetRoleGroupByName(portalID={PortalID},roleGroupName={RoleGroupName})", portalID, roleGroupName);
            Logger.LogWarning("<<< Exiting GetRoleGroupByName(portalID={PortalID},roleGroupName={RoleGroupName})", portalID, roleGroupName);
            return this.ExecuteReader("GetRoleGroupByName", portalID, roleGroupName);
        }

        public virtual IDataReader GetRoleGroups(int portalId)
        {
			Logger.LogError(">>> Entering GetRoleGroups(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetRoleGroups(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetRoleGroups", portalId);
        }

        public virtual void UpdateRoleGroup(int roleGroupId, string groupName, string description, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateRoleGroup(roleGroupId={RoleGroupId},groupName={GroupName},description={Description},lastModifiedByUserID={LastModifiedByUserID})", roleGroupId, groupName, description, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateRoleGroup", roleGroupId, groupName, description, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateRoleGroup(roleGroupId={RoleGroupId},groupName={GroupName},description={Description},lastModifiedByUserID={LastModifiedByUserID})", roleGroupId, groupName, description, lastModifiedByUserID);
        }

        public virtual int AddRole(int portalId, int roleGroupId, string roleName, string description, float serviceFee, string billingPeriod, string billingFrequency, float trialFee, int trialPeriod, string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode, string iconFile, int createdByUserID, int status, int securityMode, bool isSystemRole)
        {
			Logger.LogError(">>> Entering AddRole(portalId={PortalId},roleGroupId={RoleGroupId},roleName={RoleName},description={Description},serviceFee={ServiceFee},billingPeriod={BillingPeriod},billingFrequency={BillingFrequency},trialFee={TrialFee},trialPeriod={TrialPeriod},trialFrequency={TrialFrequency},isPublic={IsPublic},autoAssignment={AutoAssignment},rsvpCode={RsvpCode},iconFile={IconFile},createdByUserID={CreatedByUserID},status={Status},securityMode={SecurityMode},isSystemRole={IsSystemRole})", portalId, roleGroupId, roleName, description, serviceFee, billingPeriod, billingFrequency, trialFee, trialPeriod, trialFrequency, isPublic, autoAssignment, rsvpCode, iconFile, createdByUserID, status, securityMode, isSystemRole);
			Logger.LogWarning("<<< Exiting AddRole(portalId={PortalId},roleGroupId={RoleGroupId},roleName={RoleName},description={Description},serviceFee={ServiceFee},billingPeriod={BillingPeriod},billingFrequency={BillingFrequency},trialFee={TrialFee},trialPeriod={TrialPeriod},trialFrequency={TrialFrequency},isPublic={IsPublic},autoAssignment={AutoAssignment},rsvpCode={RsvpCode},iconFile={IconFile},createdByUserID={CreatedByUserID},status={Status},securityMode={SecurityMode},isSystemRole={IsSystemRole})", portalId, roleGroupId, roleName, description, serviceFee, billingPeriod, billingFrequency, trialFee, trialPeriod, trialFrequency, isPublic, autoAssignment, rsvpCode, iconFile, createdByUserID, status, securityMode, isSystemRole);
            return this.ExecuteScalar<int>(
                "AddRole",
                portalId,
                this.GetNull(roleGroupId),
                roleName,
                description,
                serviceFee,
                billingPeriod,
                this.GetNull(billingFrequency),
                trialFee,
                trialPeriod,
                this.GetNull(trialFrequency),
                isPublic,
                autoAssignment,
                rsvpCode,
                iconFile,
                createdByUserID,
                status,
                securityMode,
                isSystemRole);
        }

        public virtual void DeleteRole(int roleId)
        {
			Logger.LogError(">>> Entering DeleteRole(roleId={RoleId})", roleId);
            this.ExecuteNonQuery("DeleteRole", roleId);
			Logger.LogWarning("<<< Exiting DeleteRole(roleId={RoleId})", roleId);
        }

        public virtual IDataReader GetPortalRoles(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalRoles(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPortalRoles(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetPortalRoles", portalId);
        }

        public virtual IDataReader GetRoles()
        {
			Logger.LogError(">>> Entering GetRoles()");
            Logger.LogWarning("<<< Exiting GetRoles()");
            return this.ExecuteReader("GetRoles");
        }

        public virtual IDataReader GetRolesBasicSearch(int portalID, int pageIndex, int pageSize, string filterBy)
        {
			Logger.LogError(">>> Entering GetRolesBasicSearch(portalID={PortalID},pageIndex={PageIndex},pageSize={PageSize},filterBy={FilterBy})", portalID, pageIndex, pageSize, filterBy);
            Logger.LogWarning("<<< Exiting GetRolesBasicSearch(portalID={PortalID},pageIndex={PageIndex},pageSize={PageSize},filterBy={FilterBy})", portalID, pageIndex, pageSize, filterBy);
            return this.ExecuteReader("GetRolesBasicSearch", portalID, pageIndex, pageSize, filterBy);
        }

        public virtual IDataReader GetRoleSettings(int roleId)
        {
			Logger.LogError(">>> Entering GetRoleSettings(roleId={RoleId})", roleId);
            Logger.LogWarning("<<< Exiting GetRoleSettings(roleId={RoleId})", roleId);
            return this.ExecuteReader("GetRoleSettings", roleId);
        }

        public virtual void UpdateRole(int roleId, int roleGroupId, string roleName, string description, float serviceFee, string billingPeriod, string billingFrequency, float trialFee, int trialPeriod, string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode, string iconFile, int lastModifiedByUserID, int status, int securityMode, bool isSystemRole)
        {
			Logger.LogError(">>> Entering UpdateRole(roleId={RoleId},roleGroupId={RoleGroupId},roleName={RoleName},description={Description},serviceFee={ServiceFee},billingPeriod={BillingPeriod},billingFrequency={BillingFrequency},trialFee={TrialFee},trialPeriod={TrialPeriod},trialFrequency={TrialFrequency},isPublic={IsPublic},autoAssignment={AutoAssignment},rsvpCode={RsvpCode},iconFile={IconFile},lastModifiedByUserID={LastModifiedByUserID},status={Status},securityMode={SecurityMode},isSystemRole={IsSystemRole})", roleId, roleGroupId, roleName, description, serviceFee, billingPeriod, billingFrequency, trialFee, trialPeriod, trialFrequency, isPublic, autoAssignment, rsvpCode, iconFile, lastModifiedByUserID, status, securityMode, isSystemRole);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateRole",
                roleId,
                this.GetNull(roleGroupId),
                roleName,
                description,
                serviceFee,
                billingPeriod,
                this.GetNull(billingFrequency),
                trialFee,
                trialPeriod,
                this.GetNull(trialFrequency),
                isPublic,
                autoAssignment,
                rsvpCode,
                iconFile,
                lastModifiedByUserID,
                status,
                securityMode,
                isSystemRole);
				Logger.LogError("UpdateRole(roleId,roleGroupId,roleName,description,serviceFee,billingPeriod,billingFrequency,trialFee,trialPeriod,trialFrequency,isPublic,autoAssignment,rsvpCode,iconFile,lastModifiedByUserID,status,securityMode,isSystemRole)={RoleId},{RoleGroupId},{RoleName},{Description},{ServiceFee},{BillingPeriod},{BillingFrequency},{TrialFee},{TrialPeriod},{TrialFrequency},{IsPublic},{AutoAssignment},{RsvpCode},{IconFile},{LastModifiedByUserID},{Status},{SecurityMode},{IsSystemRole}: database query executed in {Arg18} ms", roleId, roleGroupId, roleName, description, serviceFee, billingPeriod, billingFrequency, trialFee, trialPeriod, trialFrequency, isPublic, autoAssignment, rsvpCode, iconFile, lastModifiedByUserID, status, securityMode, isSystemRole, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateRole(roleId={RoleId},roleGroupId={RoleGroupId},roleName={RoleName},description={Description},serviceFee={ServiceFee},billingPeriod={BillingPeriod},billingFrequency={BillingFrequency},trialFee={TrialFee},trialPeriod={TrialPeriod},trialFrequency={TrialFrequency},isPublic={IsPublic},autoAssignment={AutoAssignment},rsvpCode={RsvpCode},iconFile={IconFile},lastModifiedByUserID={LastModifiedByUserID},status={Status},securityMode={SecurityMode},isSystemRole={IsSystemRole})", roleId, roleGroupId, roleName, description, serviceFee, billingPeriod, billingFrequency, trialFee, trialPeriod, trialFrequency, isPublic, autoAssignment, rsvpCode, iconFile, lastModifiedByUserID, status, securityMode, isSystemRole);
        }

        public virtual void UpdateRoleSetting(int roleId, string settingName, string settingValue, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateRoleSetting(roleId={RoleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", roleId, settingName, settingValue, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateRoleSetting",
                roleId,
                settingName,
                settingValue,
                lastModifiedByUserID);
				Logger.LogError("UpdateRoleSetting(roleId,settingName,settingValue,lastModifiedByUserID)={RoleId},{SettingName},{SettingValue},{LastModifiedByUserID}: database query executed in {Arg4} ms", roleId, settingName, settingValue, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateRoleSetting(roleId={RoleId},settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", roleId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual int AddUser(int portalID, string username, string firstName, string lastName, int affiliateId, bool isSuperUser, string email, string displayName, bool updatePassword, bool isApproved, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddUser(portalID={PortalID},username={Username},firstName={FirstName},lastName={LastName},affiliateId={AffiliateId},isSuperUser={IsSuperUser},email={Email},displayName={DisplayName},updatePassword=<redacted>,isApproved={IsApproved},createdByUserID={CreatedByUserID})", portalID, username, firstName, lastName, affiliateId, isSuperUser, email, displayName, isApproved, createdByUserID);
            Logger.LogWarning("<<< Exiting AddUser(portalID={PortalID},username={Username},firstName={FirstName},lastName={LastName},affiliateId={AffiliateId},isSuperUser={IsSuperUser},email={Email},displayName={DisplayName},updatePassword=<redacted>,isApproved={IsApproved},createdByUserID={CreatedByUserID})", portalID, username, firstName, lastName, affiliateId, isSuperUser, email, displayName, isApproved, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddUser",
                portalID,
                username,
                firstName,
                lastName,
                this.GetNull(affiliateId),
                isSuperUser,
                email,
                displayName,
                updatePassword,
                isApproved,
                createdByUserID);
        }

        public virtual void AddUserPortal(int portalId, int userId)
        {
			Logger.LogError(">>> Entering AddUserPortal(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("AddUserPortal", portalId, userId);
			Logger.LogWarning("<<< Exiting AddUserPortal(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual void ChangeUsername(int userId, string newUsername)
        {
			Logger.LogError(">>> Entering ChangeUsername(userId={UserId},newUsername={NewUsername})", userId, newUsername);
            this.ExecuteNonQuery("ChangeUsername", userId, newUsername);
			Logger.LogWarning("<<< Exiting ChangeUsername(userId={UserId},newUsername={NewUsername})", userId, newUsername);
        }

        public virtual void DeleteUserFromPortal(int userId, int portalId)
        {
			Logger.LogError(">>> Entering DeleteUserFromPortal(userId={UserId},portalId={PortalId})", userId, portalId);
            this.ExecuteNonQuery("DeleteUserPortal", userId, this.GetNull(portalId));
			Logger.LogWarning("<<< Exiting DeleteUserFromPortal(userId={UserId},portalId={PortalId})", userId, portalId);
        }

        public virtual IDataReader GetAllUsers(int portalID, int pageIndex, int pageSize, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetAllUsers(portalID={PortalID},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalID, pageIndex, pageSize, includeDeleted, superUsersOnly);
            Logger.LogWarning("<<< Exiting GetAllUsers(portalID={PortalID},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalID, pageIndex, pageSize, includeDeleted, superUsersOnly);
            return this.ExecuteReader("GetAllUsers", this.GetNull(portalID), pageIndex, pageSize, includeDeleted, superUsersOnly);
        }

        public virtual IDataReader GetDeletedUsers(int portalId)
        {
			Logger.LogError(">>> Entering GetDeletedUsers(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetDeletedUsers(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetDeletedUsers", this.GetNull(portalId));
        }

        public virtual IDataReader GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetUnAuthorizedUsers(portalId={PortalId},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, includeDeleted, superUsersOnly);
            Logger.LogWarning("<<< Exiting GetUnAuthorizedUsers(portalId={PortalId},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, includeDeleted, superUsersOnly);
            return this.ExecuteReader("GetUnAuthorizedUsers", this.GetNull(portalId), includeDeleted, superUsersOnly);
        }

        public virtual IDataReader GetUser(int portalId, int userId)
        {
			Logger.LogError(">>> Entering GetUser(portalId={PortalId},userId={UserId})", portalId, userId);
            Logger.LogWarning("<<< Exiting GetUser(portalId={PortalId},userId={UserId})", portalId, userId);
            return this.ExecuteReader("GetUser", portalId, userId);
        }

        public virtual IDataReader GetUserByAuthToken(int portalID, string userToken, string authType)
        {
			Logger.LogError(">>> Entering GetUserByAuthToken(portalID={PortalID},userToken=<redacted>,authType={AuthType})", portalID, authType);
            Logger.LogWarning("<<< Exiting GetUserByAuthToken(portalID={PortalID},userToken=<redacted>,authType={AuthType})", portalID, authType);
            return this.ExecuteReader("GetUserByAuthToken", portalID, userToken, authType);
        }

        public virtual IDataReader GetUserByDisplayName(int portalId, string displayName)
        {
			Logger.LogError(">>> Entering GetUserByDisplayName(portalId={PortalId},displayName={DisplayName})", portalId, displayName);
            Logger.LogWarning("<<< Exiting GetUserByDisplayName(portalId={PortalId},displayName={DisplayName})", portalId, displayName);
            return this.ExecuteReader("GetUserByDisplayName", this.GetNull(portalId), displayName);
        }

        public virtual IDataReader GetUserByUsername(int portalId, string username)
        {
			Logger.LogError(">>> Entering GetUserByUsername(portalId={PortalId},username={Username})", portalId, username);
            Logger.LogWarning("<<< Exiting GetUserByUsername(portalId={PortalId},username={Username})", portalId, username);
            return this.ExecuteReader("GetUserByUsername", this.GetNull(portalId), username);
        }

        public virtual IDataReader GetUserByUsername(string username, string spaceReplacement)
        {
            return this.ExecuteReader("GetUserByUsernameForUrl", username, spaceReplacement);
        }

        public virtual IDataReader GetUserByVanityUrl(int portalId, string vanityUrl)
        {
			Logger.LogError(">>> Entering GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl})", portalId, vanityUrl);
            Logger.LogWarning("<<< Exiting GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl})", portalId, vanityUrl);
            return this.ExecuteReader("GetUserByVanityUrl", this.GetNull(portalId), vanityUrl);
        }

        public virtual IDataReader GetUserByPasswordResetToken(int portalId, string resetToken)
        {
			Logger.LogError(">>> Entering GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>)", portalId);
            Logger.LogWarning("<<< Exiting GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>)", portalId);
            return this.ExecuteReader("GetUserByPasswordResetToken", this.GetNull(portalId), resetToken);
        }

        public virtual IDataReader GetDisplayNameForUser(int userId, string spaceReplacement)
        {
			Logger.LogError(">>> Entering GetDisplayNameForUser(userId={UserId},spaceReplacement={SpaceReplacement})", userId, spaceReplacement);
            Logger.LogWarning("<<< Exiting GetDisplayNameForUser(userId={UserId},spaceReplacement={SpaceReplacement})", userId, spaceReplacement);
            return this.ExecuteReader("GetDisplayNameForUser", userId, spaceReplacement);
        }

        public virtual int GetUserCountByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetUserCountByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetUserCountByPortal(portalId={PortalId})", portalId);
            return this.ExecuteScalar<int>("GetUserCountByPortal", portalId);
        }

        public virtual IDataReader GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int fitlerRoleId, int relationTypeId, bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames, string propertyValues)
        {
			Logger.LogError(">>> Entering GetUsersAdvancedSearch(portalId={PortalId},userId={UserId},filterUserId={FilterUserId},fitlerRoleId={FitlerRoleId},relationTypeId={RelationTypeId},isAdmin={IsAdmin},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyNames={PropertyNames},propertyValues={PropertyValues})", portalId, userId, filterUserId, fitlerRoleId, relationTypeId, isAdmin, pageIndex, pageSize, sortColumn, sortAscending, propertyNames, propertyValues);
			Logger.LogWarning("<<< Exiting GetUsersAdvancedSearch(portalId={PortalId},userId={UserId},filterUserId={FilterUserId},fitlerRoleId={FitlerRoleId},relationTypeId={RelationTypeId},isAdmin={IsAdmin},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyNames={PropertyNames},propertyValues={PropertyValues})", portalId, userId, filterUserId, fitlerRoleId, relationTypeId, isAdmin, pageIndex, pageSize, sortColumn, sortAscending, propertyNames, propertyValues);
            var ps = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
            string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
            string filterName = ps.InputFilter(propertyNames, PortalSecurity.FilterFlag.NoSQL);
            string filterValue = ps.InputFilter(propertyValues, PortalSecurity.FilterFlag.NoSQL);
#pragma warning restore CS0618 // Type or member is obsolete
            return this.ExecuteReader(
                "GetUsersAdvancedSearch",
                portalId,
                userId,
                filterUserId,
                fitlerRoleId,
                relationTypeId,
                isAdmin,
                pageSize,
                pageIndex,
                filterSort,
                sortAscending,
                filterName,
                filterValue);
        }

        public virtual IDataReader GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyName, string propertyValue)
        {
			Logger.LogError(">>> Entering GetUsersBasicSearch(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyName={PropertyName},propertyValue={PropertyValue})", portalId, pageIndex, pageSize, sortColumn, sortAscending, propertyName, propertyValue);
			Logger.LogWarning("<<< Exiting GetUsersBasicSearch(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyName={PropertyName},propertyValue={PropertyValue})", portalId, pageIndex, pageSize, sortColumn, sortAscending, propertyName, propertyValue);
            var ps = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
            string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
            string filterName = ps.InputFilter(propertyName, PortalSecurity.FilterFlag.NoSQL);
            string filterValue = ps.InputFilter(propertyValue, PortalSecurity.FilterFlag.NoSQL);
#pragma warning restore CS0618 // Type or member is obsolete
            return this.ExecuteReader(
                "GetUsersBasicSearch",
                portalId,
                pageSize,
                pageIndex,
                filterSort,
                sortAscending,
                filterName,
                filterValue);
        }

        public virtual IDataReader GetUsersByEmail(int portalID, string email, int pageIndex, int pageSize, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetUsersByEmail(portalID={PortalID},email={Email},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalID, email, pageIndex, pageSize, includeDeleted, superUsersOnly);
            return this.ExecuteReader(
                "GetUsersByEmail",
                this.GetNull(portalID),
                email,
                pageIndex,
                pageSize,
                includeDeleted,
                superUsersOnly);
        }

        public virtual IDataReader GetUsersByProfileProperty(int portalID, string propertyName, string propertyValue, int pageIndex, int pageSize, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetUsersByProfileProperty(portalID={PortalID},propertyName={PropertyName},propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalID, propertyName, propertyValue, pageIndex, pageSize, includeDeleted, superUsersOnly);
            return this.ExecuteReader(
                "GetUsersByProfileProperty",
                this.GetNull(portalID),
                propertyName,
                propertyValue,
                pageIndex,
                pageSize,
                includeDeleted,
                superUsersOnly);
        }

        public virtual IDataReader GetUsersByRolename(int portalID, string rolename)
        {
			Logger.LogError(">>> Entering GetUsersByRolename(portalID={PortalID},rolename={Rolename})", portalID, rolename);
            Logger.LogWarning("<<< Exiting GetUsersByRolename(portalID={PortalID},rolename={Rolename})", portalID, rolename);
            return this.ExecuteReader("GetUsersByRolename", this.GetNull(portalID), rolename);
        }

        public virtual IDataReader GetUsersByUsername(int portalID, string username, int pageIndex, int pageSize, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetUsersByUsername(portalID={PortalID},username={Username},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalID, username, pageIndex, pageSize, includeDeleted, superUsersOnly);
            return this.ExecuteReader(
                "GetUsersByUsername",
                this.GetNull(portalID),
                username,
                pageIndex,
                pageSize,
                includeDeleted,
                superUsersOnly);
        }

        public virtual IDataReader GetUsersByDisplayname(int portalId, string name, int pageIndex, int pageSize, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogError(">>> Entering GetUsersByDisplayname(portalId={PortalId},name={Name},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, name, pageIndex, pageSize, includeDeleted, superUsersOnly);
            return this.ExecuteReader(
                "GetUsersByDisplayname",
                this.GetNull(portalId),
                name,
                pageIndex,
                pageSize,
                includeDeleted,
                superUsersOnly);
        }

        public virtual int GetDuplicateEmailCount(int portalId)
        {
			Logger.LogError(">>> Entering GetDuplicateEmailCount(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetDuplicateEmailCount(portalId={PortalId})", portalId);
            return this.ExecuteScalar<int>("GetDuplicateEmailCount", portalId);
        }

        public virtual int GetSingleUserByEmail(int portalId, string emailToMatch)
        {
			Logger.LogError(">>> Entering GetSingleUserByEmail(portalId={PortalId},emailToMatch={EmailToMatch})", portalId, emailToMatch);
            Logger.LogWarning("<<< Exiting GetSingleUserByEmail(portalId={PortalId},emailToMatch={EmailToMatch})", portalId, emailToMatch);
            return this.ExecuteScalar<int>("GetSingleUserByEmail", portalId, emailToMatch);
        }

        public virtual void RemoveUser(int userId, int portalId)
        {
			Logger.LogError(">>> Entering RemoveUser(userId={UserId},portalId={PortalId})", userId, portalId);
            this.ExecuteNonQuery("RemoveUser", userId, this.GetNull(portalId));
			Logger.LogWarning("<<< Exiting RemoveUser(userId={UserId},portalId={PortalId})", userId, portalId);
        }

        public virtual void ReplaceServerOnSchedules(string oldServername, string newServerName)
        {
			Logger.LogError(">>> Entering ReplaceServerOnSchedules(oldServername={OldServername},newServerName={NewServerName})", oldServername, newServerName);
            this.ExecuteNonQuery("ReplaceServerOnSchedules", oldServername, newServerName);
			Logger.LogWarning("<<< Exiting ReplaceServerOnSchedules(oldServername={OldServername},newServerName={NewServerName})", oldServername, newServerName);
        }

        public virtual void ResetTermsAgreement(int portalId)
        {
			Logger.LogError(">>> Entering ResetTermsAgreement(portalId={PortalId})", portalId);
            this.ExecuteNonQuery("ResetTermsAgreement", portalId);
			Logger.LogWarning("<<< Exiting ResetTermsAgreement(portalId={PortalId})", portalId);
        }

        public virtual void RestoreUser(int userId, int portalId)
        {
			Logger.LogError(">>> Entering RestoreUser(userId={UserId},portalId={PortalId})", userId, portalId);
            this.ExecuteNonQuery("RestoreUser", userId, this.GetNull(portalId));
			Logger.LogWarning("<<< Exiting RestoreUser(userId={UserId},portalId={PortalId})", userId, portalId);
        }

        public virtual void UpdateUser(int userId, int portalID, string firstName, string lastName, bool isSuperUser, string email, string displayName, string vanityUrl, bool updatePassword, bool isApproved, bool refreshRoles, string lastIpAddress, Guid passwordResetToken, DateTime passwordResetExpiration, bool isDeleted, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateUser(userId={UserId},portalID={PortalID},firstName={FirstName},lastName={LastName},isSuperUser={IsSuperUser},email={Email},displayName={DisplayName},vanityUrl={VanityUrl},updatePassword=<redacted>,isApproved={IsApproved},refreshRoles={RefreshRoles},lastIpAddress={LastIpAddress},passwordResetToken=<redacted>,passwordResetExpiration=<redacted>,isDeleted={IsDeleted},lastModifiedByUserID={LastModifiedByUserID})", userId, portalID, firstName, lastName, isSuperUser, email, displayName, vanityUrl, isApproved, refreshRoles, lastIpAddress, isDeleted, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateUser",
                userId,
                this.GetNull(portalID),
                firstName,
                lastName,
                isSuperUser,
                email,
                displayName,
                vanityUrl,
                updatePassword,
                isApproved,
                refreshRoles,
                lastIpAddress,
                this.GetNull(passwordResetToken),
                this.GetNull(passwordResetExpiration),
                isDeleted,
                lastModifiedByUserID);
				Logger.LogError("UpdateUser(userId,portalID,firstName,lastName,isSuperUser,email,displayName,vanityUrl,updatePassword,isApproved,refreshRoles,lastIpAddress,passwordResetToken,passwordResetExpiration,isDeleted,lastModifiedByUserID)={UserId},{PortalID},{FirstName},{LastName},{IsSuperUser},{Email},{DisplayName},{VanityUrl},<redacted>,{IsApproved},{RefreshRoles},{LastIpAddress},<redacted>,<redacted>,{IsDeleted},{LastModifiedByUserID}: database query executed in {Arg16} ms", userId, portalID, firstName, lastName, isSuperUser, email, displayName, vanityUrl, isApproved, refreshRoles, lastIpAddress, isDeleted, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateUser(userId={UserId},portalID={PortalID},firstName={FirstName},lastName={LastName},isSuperUser={IsSuperUser},email={Email},displayName={DisplayName},vanityUrl={VanityUrl},updatePassword=<redacted>,isApproved={IsApproved},refreshRoles={RefreshRoles},lastIpAddress={LastIpAddress},passwordResetToken=<redacted>,passwordResetExpiration=<redacted>,isDeleted={IsDeleted},lastModifiedByUserID={LastModifiedByUserID})", userId, portalID, firstName, lastName, isSuperUser, email, displayName, vanityUrl, isApproved, refreshRoles, lastIpAddress, isDeleted, lastModifiedByUserID);
        }

        public virtual void UpdateUserLastIpAddress(int userId, string lastIpAddress)
        {
			Logger.LogError(">>> Entering UpdateUserLastIpAddress(userId={UserId},lastIpAddress={LastIpAddress})", userId, lastIpAddress);
            this.ExecuteNonQuery("UpdateUserLastIpAddress", userId, lastIpAddress);
			Logger.LogWarning("<<< Exiting UpdateUserLastIpAddress(userId={UserId},lastIpAddress={LastIpAddress})", userId, lastIpAddress);
        }

        public virtual void UserAgreedToTerms(int portalId, int userId)
        {
			Logger.LogError(">>> Entering UserAgreedToTerms(portalId={PortalId},userId={UserId})", portalId, userId);
            this.ExecuteNonQuery("UserAgreedToTerms", portalId, userId);
			Logger.LogWarning("<<< Exiting UserAgreedToTerms(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public virtual void UserRequestsRemoval(int portalId, int userId, bool remove)
        {
			Logger.LogError(">>> Entering UserRequestsRemoval(portalId={PortalId},userId={UserId},remove={Remove})", portalId, userId, remove);
            this.ExecuteNonQuery("UserRequestsRemoval", portalId, userId, remove);
			Logger.LogWarning("<<< Exiting UserRequestsRemoval(portalId={PortalId},userId={UserId},remove={Remove})", portalId, userId, remove);
        }

        public virtual int AddUserRole(int portalId, int userId, int roleId, int status, bool isOwner, DateTime effectiveDate, DateTime expiryDate, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddUserRole(portalId={PortalId},userId={UserId},roleId={RoleId},status={Status},isOwner={IsOwner},effectiveDate={EffectiveDate},expiryDate={ExpiryDate},createdByUserID={CreatedByUserID})", portalId, userId, roleId, status, isOwner, effectiveDate, expiryDate, createdByUserID);
            Logger.LogWarning("<<< Exiting AddUserRole(portalId={PortalId},userId={UserId},roleId={RoleId},status={Status},isOwner={IsOwner},effectiveDate={EffectiveDate},expiryDate={ExpiryDate},createdByUserID={CreatedByUserID})", portalId, userId, roleId, status, isOwner, effectiveDate, expiryDate, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddUserRole",
                portalId,
                userId,
                roleId,
                status,
                isOwner,
                this.GetNull(effectiveDate),
                this.GetNull(expiryDate),
                createdByUserID);
        }

        public virtual void DeleteUserRole(int userId, int roleId)
        {
			Logger.LogError(">>> Entering DeleteUserRole(userId={UserId},roleId={RoleId})", userId, roleId);
            this.ExecuteNonQuery("DeleteUserRole", userId, roleId);
			Logger.LogWarning("<<< Exiting DeleteUserRole(userId={UserId},roleId={RoleId})", userId, roleId);
        }

        public virtual IDataReader GetServices(int portalId, int userId)
        {
			Logger.LogError(">>> Entering GetServices(portalId={PortalId},userId={UserId})", portalId, userId);
            Logger.LogWarning("<<< Exiting GetServices(portalId={PortalId},userId={UserId})", portalId, userId);
            return this.ExecuteReader("GetServices", portalId, this.GetNull(userId));
        }

        public virtual IDataReader GetUserRole(int portalID, int userId, int roleId)
        {
			Logger.LogError(">>> Entering GetUserRole(portalID={PortalID},userId={UserId},roleId={RoleId})", portalID, userId, roleId);
            Logger.LogWarning("<<< Exiting GetUserRole(portalID={PortalID},userId={UserId},roleId={RoleId})", portalID, userId, roleId);
            return this.ExecuteReader("GetUserRole", portalID, userId, roleId);
        }

        public virtual IDataReader GetUserRoles(int portalID, int userId)
        {
			Logger.LogError(">>> Entering GetUserRoles(portalID={PortalID},userId={UserId})", portalID, userId);
            Logger.LogWarning("<<< Exiting GetUserRoles(portalID={PortalID},userId={UserId})", portalID, userId);
            return this.ExecuteReader("GetUserRoles", portalID, userId);
        }

        public virtual IDataReader GetUserRolesByUsername(int portalID, string username, string rolename)
        {
			Logger.LogError(">>> Entering GetUserRolesByUsername(portalID={PortalID},username={Username},rolename={Rolename})", portalID, username, rolename);
            Logger.LogWarning("<<< Exiting GetUserRolesByUsername(portalID={PortalID},username={Username},rolename={Rolename})", portalID, username, rolename);
            return this.ExecuteReader("GetUserRolesByUsername", portalID, this.GetNull(username), this.GetNull(rolename));
        }

        public virtual void UpdateUserRole(int userRoleId, int status, bool isOwner, DateTime effectiveDate, DateTime expiryDate, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateUserRole(userRoleId={UserRoleId},status={Status},isOwner={IsOwner},effectiveDate={EffectiveDate},expiryDate={ExpiryDate},lastModifiedByUserID={LastModifiedByUserID})", userRoleId, status, isOwner, effectiveDate, expiryDate, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateUserRole", userRoleId, status, isOwner, this.GetNull(effectiveDate), this.GetNull(expiryDate), lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateUserRole(userRoleId={UserRoleId},status={Status},isOwner={IsOwner},effectiveDate={EffectiveDate},expiryDate={ExpiryDate},lastModifiedByUserID={LastModifiedByUserID})", userRoleId, status, isOwner, effectiveDate, expiryDate, lastModifiedByUserID);
        }

        /// <summary>Delete outdated users online.</summary>
        /// <param name="timeWindow">The time window in which to delete.</param>
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public virtual partial void DeleteUsersOnline(int timeWindow)
        {
			Logger.LogError(">>> Entering DeleteUsersOnline(timeWindow={TimeWindow})", timeWindow);
            this.ExecuteNonQuery("DeleteUsersOnline", timeWindow);
			Logger.LogWarning("<<< Exiting DeleteUsersOnline(timeWindow={TimeWindow})", timeWindow);
        }

        /// <summary>Get the online user record.</summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A data reader.</returns>
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public virtual partial IDataReader GetOnlineUser(int userId)
        {
			Logger.LogError(">>> Entering GetOnlineUser(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetOnlineUser(userId={UserId})", userId);
            return this.ExecuteReader("GetOnlineUser", userId);
        }

        /// <summary>Get the online user records for a portal.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>A data reader.</returns>
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public virtual partial IDataReader GetOnlineUsers(int portalId)
        {
			Logger.LogError(">>> Entering GetOnlineUsers(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetOnlineUsers(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetOnlineUsers", portalId);
        }

        /// <summary>Update the online user records.</summary>
        /// <param name="userList">The users.</param>
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public virtual partial void UpdateUsersOnline(Hashtable userList)
        {
			Logger.LogError(">>> Entering UpdateUsersOnline(userList={UserList})", userList);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (userList.Count == 0)
            {
                // No users to process, quit method
                return;
            }

            foreach (string key in userList.Keys)
            {
                var info = userList[key] as AnonymousUserInfo;
                if (info != null)
                {
                    var user = info;
                    this.ExecuteNonQuery("UpdateAnonymousUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
                }
                else if (userList[key] is OnlineUserInfo)
                {
					Logger.LogError("UpdateUsersOnline(userList)={UserList}: database query executed in {Arg1} ms", userList, __logStart.ElapsedMilliseconds);
                    var user = (OnlineUserInfo)userList[key];
					Logger.LogError("UpdateUsersOnline(userList={UserList}): user → {User}", userList, user);
                    this.ExecuteNonQuery("UpdateOnlineUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
                }
            }
			Logger.LogWarning("<<< Exiting UpdateUsersOnline(userList={UserList})", userList);
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual int AddPropertyDefinition(int portalId, int moduleDefId, int dataType, string defaultValue, string propertyCategory, string propertyName, bool readOnly, bool required, string validationExpression, int viewOrder, bool visible, int length, int defaultVisibility, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddPropertyDefinition(portalId={PortalId},moduleDefId={ModuleDefId},dataType={DataType},defaultValue={DefaultValue},propertyCategory={PropertyCategory},propertyName={PropertyName},readOnly={ReadOnly},required={Required},validationExpression={ValidationExpression},viewOrder={ViewOrder},visible={Visible},length={Length},defaultVisibility={DefaultVisibility},createdByUserId={CreatedByUserId})", portalId, moduleDefId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validationExpression, viewOrder, visible, length, defaultVisibility, createdByUserId);
            int retValue;
            try
            {
                retValue = this.ExecuteScalar<int>(
                    "AddPropertyDefinition",
                    this.GetNull(portalId),
                    moduleDefId,
                    dataType,
                    defaultValue,
                    propertyCategory,
                    propertyName,
                    readOnly,
                    required,
                    validationExpression,
                    viewOrder,
                    visible,
                    length,
                    defaultVisibility,
                    createdByUserId);
            }
            catch (SqlException ex)
            {
                Logger.Debug(ex);

                // If not a duplicate (throw an Exception)
                retValue = -ex.Number;
				Logger.LogDebug("AddPropertyDefinition(portalId={PortalId},moduleDefId={ModuleDefId},dataType={DataType},defaultValue={DefaultValue},propertyCategory={PropertyCategory},propertyName={PropertyName},readOnly={ReadOnly},required={Required},validationExpression={ValidationExpression},viewOrder={ViewOrder},visible={Visible},length={Length},defaultVisibility={DefaultVisibility},createdByUserId={CreatedByUserId}): retValue → {RetValue}", portalId, moduleDefId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validationExpression, viewOrder, visible, length, defaultVisibility, createdByUserId, retValue);
                if (ex.Number != DuplicateKey)
                {
                    throw;
                }
            }

			Logger.LogWarning("<<< Exiting AddPropertyDefinition(portalId={PortalId},moduleDefId={ModuleDefId},dataType={DataType},defaultValue={DefaultValue},propertyCategory={PropertyCategory},propertyName={PropertyName},readOnly={ReadOnly},required={Required},validationExpression={ValidationExpression},viewOrder={ViewOrder},visible={Visible},length={Length},defaultVisibility={DefaultVisibility},createdByUserId={CreatedByUserId})", portalId, moduleDefId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validationExpression, viewOrder, visible, length, defaultVisibility, createdByUserId);
            return retValue;
        }

        public virtual void DeletePropertyDefinition(int definitionId)
        {
			Logger.LogError(">>> Entering DeletePropertyDefinition(definitionId={DefinitionId})", definitionId);
            this.ExecuteNonQuery("DeletePropertyDefinition", definitionId);
			Logger.LogWarning("<<< Exiting DeletePropertyDefinition(definitionId={DefinitionId})", definitionId);
        }

        public virtual IDataReader GetPropertyDefinition(int definitionId)
        {
			Logger.LogError(">>> Entering GetPropertyDefinition(definitionId={DefinitionId})", definitionId);
            Logger.LogWarning("<<< Exiting GetPropertyDefinition(definitionId={DefinitionId})", definitionId);
            return this.ExecuteReader("GetPropertyDefinition", definitionId);
        }

        public virtual IDataReader GetPropertyDefinitionByName(int portalId, string name)
        {
			Logger.LogError(">>> Entering GetPropertyDefinitionByName(portalId={PortalId},name={Name})", portalId, name);
            Logger.LogWarning("<<< Exiting GetPropertyDefinitionByName(portalId={PortalId},name={Name})", portalId, name);
            return this.ExecuteReader("GetPropertyDefinitionByName", this.GetNull(portalId), name);
        }

        public virtual IDataReader GetPropertyDefinitionsByPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetPropertyDefinitionsByPortal(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPropertyDefinitionsByPortal(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetPropertyDefinitionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetUserProfile(int userId)
        {
			Logger.LogError(">>> Entering GetUserProfile(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetUserProfile(userId={UserId})", userId);
            return this.ExecuteReader("GetUserProfile", userId);
        }

        public virtual void UpdateProfileProperty(int profileId, int userId, int propertyDefinitionID, string propertyValue, int visibility, string extendedVisibility, DateTime lastUpdatedDate)
        {
			Logger.LogError(">>> Entering UpdateProfileProperty(profileId={ProfileId},userId={UserId},propertyDefinitionID={PropertyDefinitionID},propertyValue={PropertyValue},visibility={Visibility},extendedVisibility={ExtendedVisibility},lastUpdatedDate={LastUpdatedDate})", profileId, userId, propertyDefinitionID, propertyValue, visibility, extendedVisibility, lastUpdatedDate);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateUserProfileProperty",
                this.GetNull(profileId),
                userId,
                propertyDefinitionID,
                propertyValue,
                visibility,
                extendedVisibility,
                lastUpdatedDate);
				Logger.LogError("UpdateProfileProperty(profileId,userId,propertyDefinitionID,propertyValue,visibility,extendedVisibility,lastUpdatedDate)={ProfileId},{UserId},{PropertyDefinitionID},{PropertyValue},{Visibility},{ExtendedVisibility},{LastUpdatedDate}: database query executed in {Arg7} ms", profileId, userId, propertyDefinitionID, propertyValue, visibility, extendedVisibility, lastUpdatedDate, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateProfileProperty(profileId={ProfileId},userId={UserId},propertyDefinitionID={PropertyDefinitionID},propertyValue={PropertyValue},visibility={Visibility},extendedVisibility={ExtendedVisibility},lastUpdatedDate={LastUpdatedDate})", profileId, userId, propertyDefinitionID, propertyValue, visibility, extendedVisibility, lastUpdatedDate);
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual void UpdatePropertyDefinition(int propertyDefinitionId, int dataType, string defaultValue, string propertyCategory, string propertyName, bool readOnly, bool required, string validation, int viewOrder, bool visible, int length, int defaultVisibility, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdatePropertyDefinition(propertyDefinitionId={PropertyDefinitionId},dataType={DataType},defaultValue={DefaultValue},propertyCategory={PropertyCategory},propertyName={PropertyName},readOnly={ReadOnly},required={Required},validation={Validation},viewOrder={ViewOrder},visible={Visible},length={Length},defaultVisibility={DefaultVisibility},lastModifiedByUserId={LastModifiedByUserId})", propertyDefinitionId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validation, viewOrder, visible, length, defaultVisibility, lastModifiedByUserId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdatePropertyDefinition",
                propertyDefinitionId,
                dataType,
                defaultValue,
                propertyCategory,
                propertyName,
                readOnly,
                required,
                validation,
                viewOrder,
                visible,
                length,
                defaultVisibility,
                lastModifiedByUserId);
				Logger.LogError("UpdatePropertyDefinition(propertyDefinitionId,dataType,defaultValue,propertyCategory,propertyName,readOnly,required,validation,viewOrder,visible,length,defaultVisibility,lastModifiedByUserId)={PropertyDefinitionId},{DataType},{DefaultValue},{PropertyCategory},{PropertyName},{ReadOnly},{Required},{Validation},{ViewOrder},{Visible},{Length},{DefaultVisibility},{LastModifiedByUserId}: database query executed in {Arg13} ms", propertyDefinitionId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validation, viewOrder, visible, length, defaultVisibility, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdatePropertyDefinition(propertyDefinitionId={PropertyDefinitionId},dataType={DataType},defaultValue={DefaultValue},propertyCategory={PropertyCategory},propertyName={PropertyName},readOnly={ReadOnly},required={Required},validation={Validation},viewOrder={ViewOrder},visible={Visible},length={Length},defaultVisibility={DefaultVisibility},lastModifiedByUserId={LastModifiedByUserId})", propertyDefinitionId, dataType, defaultValue, propertyCategory, propertyName, readOnly, required, validation, viewOrder, visible, length, defaultVisibility, lastModifiedByUserId);
        }

        public virtual IDataReader SearchProfilePropertyValues(int portalId, string propertyName, string searchString)
        {
			Logger.LogError(">>> Entering SearchProfilePropertyValues(portalId={PortalId},propertyName={PropertyName},searchString={SearchString})", portalId, propertyName, searchString);
            Logger.LogWarning("<<< Exiting SearchProfilePropertyValues(portalId={PortalId},propertyName={PropertyName},searchString={SearchString})", portalId, propertyName, searchString);
            return this.ExecuteReader("SearchProfilePropertyValues", portalId, propertyName, searchString);
        }

        public virtual int AddSkinControl(int packageID, string controlKey, string controlSrc, bool supportsPartialRendering, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddSkinControl(packageID={PackageID},controlKey={ControlKey},controlSrc={ControlSrc},supportsPartialRendering={SupportsPartialRendering},createdByUserID={CreatedByUserID})", packageID, controlKey, controlSrc, supportsPartialRendering, createdByUserID);
            Logger.LogWarning("<<< Exiting AddSkinControl(packageID={PackageID},controlKey={ControlKey},controlSrc={ControlSrc},supportsPartialRendering={SupportsPartialRendering},createdByUserID={CreatedByUserID})", packageID, controlKey, controlSrc, supportsPartialRendering, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddSkinControl",
                this.GetNull(packageID),
                this.GetNull(controlKey),
                controlSrc,
                supportsPartialRendering,
                createdByUserID);
        }

        public virtual void DeleteSkinControl(int skinControlID)
        {
			Logger.LogError(">>> Entering DeleteSkinControl(skinControlID={SkinControlID})", skinControlID);
            this.ExecuteNonQuery("DeleteSkinControl", skinControlID);
			Logger.LogWarning("<<< Exiting DeleteSkinControl(skinControlID={SkinControlID})", skinControlID);
        }

        public virtual IDataReader GetSkinControls()
        {
			Logger.LogError(">>> Entering GetSkinControls()");
            Logger.LogWarning("<<< Exiting GetSkinControls()");
            return this.ExecuteReader("GetSkinControls");
        }

        public virtual IDataReader GetSkinControl(int skinControlID)
        {
			Logger.LogError(">>> Entering GetSkinControl(skinControlID={SkinControlID})", skinControlID);
            Logger.LogWarning("<<< Exiting GetSkinControl(skinControlID={SkinControlID})", skinControlID);
            return this.ExecuteReader("GetSkinControl", skinControlID);
        }

        public virtual IDataReader GetSkinControlByKey(string controlKey)
        {
			Logger.LogError(">>> Entering GetSkinControlByKey(controlKey={ControlKey})", controlKey);
            Logger.LogWarning("<<< Exiting GetSkinControlByKey(controlKey={ControlKey})", controlKey);
            return this.ExecuteReader("GetSkinControlByKey", controlKey);
        }

        public virtual IDataReader GetSkinControlByPackageID(int packageID)
        {
			Logger.LogError(">>> Entering GetSkinControlByPackageID(packageID={PackageID})", packageID);
            Logger.LogWarning("<<< Exiting GetSkinControlByPackageID(packageID={PackageID})", packageID);
            return this.ExecuteReader("GetSkinControlByPackageID", packageID);
        }

        public virtual void UpdateSkinControl(int skinControlID, int packageID, string controlKey, string controlSrc, bool supportsPartialRendering, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateSkinControl(skinControlID={SkinControlID},packageID={PackageID},controlKey={ControlKey},controlSrc={ControlSrc},supportsPartialRendering={SupportsPartialRendering},lastModifiedByUserID={LastModifiedByUserID})", skinControlID, packageID, controlKey, controlSrc, supportsPartialRendering, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateSkinControl",
                skinControlID,
                this.GetNull(packageID),
                this.GetNull(controlKey),
                controlSrc,
                supportsPartialRendering,
                lastModifiedByUserID);
				Logger.LogError("UpdateSkinControl(skinControlID,packageID,controlKey,controlSrc,supportsPartialRendering,lastModifiedByUserID)={SkinControlID},{PackageID},{ControlKey},{ControlSrc},{SupportsPartialRendering},{LastModifiedByUserID}: database query executed in {Arg6} ms", skinControlID, packageID, controlKey, controlSrc, supportsPartialRendering, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateSkinControl(skinControlID={SkinControlID},packageID={PackageID},controlKey={ControlKey},controlSrc={ControlSrc},supportsPartialRendering={SupportsPartialRendering},lastModifiedByUserID={LastModifiedByUserID})", skinControlID, packageID, controlKey, controlSrc, supportsPartialRendering, lastModifiedByUserID);
        }

        public virtual int AddSkin(int skinPackageID, string skinSrc)
        {
			Logger.LogError(">>> Entering AddSkin(skinPackageID={SkinPackageID},skinSrc={SkinSrc})", skinPackageID, skinSrc);
            Logger.LogWarning("<<< Exiting AddSkin(skinPackageID={SkinPackageID},skinSrc={SkinSrc})", skinPackageID, skinSrc);
            return this.ExecuteScalar<int>("AddSkin", skinPackageID, skinSrc);
        }

        public virtual int AddSkinPackage(int packageID, int portalID, string skinName, string skinType, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddSkinPackage(packageID={PackageID},portalID={PortalID},skinName={SkinName},skinType={SkinType},createdByUserId={CreatedByUserId})", packageID, portalID, skinName, skinType, createdByUserId);
            Logger.LogWarning("<<< Exiting AddSkinPackage(packageID={PackageID},portalID={PortalID},skinName={SkinName},skinType={SkinType},createdByUserId={CreatedByUserId})", packageID, portalID, skinName, skinType, createdByUserId);
            return this.ExecuteScalar<int>(
                "AddSkinPackage",
                packageID,
                this.GetNull(portalID),
                skinName,
                skinType,
                createdByUserId);
        }

        public virtual bool CanDeleteSkin(string skinType, string skinFoldername)
        {
			Logger.LogError(">>> Entering CanDeleteSkin(skinType={SkinType},skinFoldername={SkinFoldername})", skinType, skinFoldername);
            Logger.LogWarning("<<< Exiting CanDeleteSkin(skinType={SkinType},skinFoldername={SkinFoldername})", skinType, skinFoldername);
            return this.ExecuteScalar<int>("CanDeleteSkin", skinType, skinFoldername) == 1;
        }

        public virtual void DeleteSkin(int skinID)
        {
            this.ExecuteNonQuery("DeleteSkin", skinID);
        }

        public virtual void DeleteSkinPackage(int skinPackageID)
        {
			Logger.LogError(">>> Entering DeleteSkinPackage(skinPackageID={SkinPackageID})", skinPackageID);
            this.ExecuteNonQuery("DeleteSkinPackage", skinPackageID);
			Logger.LogWarning("<<< Exiting DeleteSkinPackage(skinPackageID={SkinPackageID})", skinPackageID);
        }

        public virtual IDataReader GetSkinByPackageID(int packageID)
        {
			Logger.LogError(">>> Entering GetSkinByPackageID(packageID={PackageID})", packageID);
            Logger.LogWarning("<<< Exiting GetSkinByPackageID(packageID={PackageID})", packageID);
            return this.ExecuteReader("GetSkinPackageByPackageID", packageID);
        }

        public virtual IDataReader GetSkinPackage(int portalID, string skinName, string skinType)
        {
			Logger.LogError(">>> Entering GetSkinPackage(portalID={PortalID},skinName={SkinName},skinType={SkinType})", portalID, skinName, skinType);
            Logger.LogWarning("<<< Exiting GetSkinPackage(portalID={PortalID},skinName={SkinName},skinType={SkinType})", portalID, skinName, skinType);
            return this.ExecuteReader("GetSkinPackage", this.GetNull(portalID), skinName, skinType);
        }

        public virtual void UpdateSkin(int skinID, string skinSrc)
        {
			Logger.LogError(">>> Entering UpdateSkin(skinID={SkinID},skinSrc={SkinSrc})", skinID, skinSrc);
            this.ExecuteNonQuery("UpdateSkin", skinID, skinSrc);
			Logger.LogWarning("<<< Exiting UpdateSkin(skinID={SkinID},skinSrc={SkinSrc})", skinID, skinSrc);
        }

        public virtual void UpdateSkinPackage(int skinPackageID, int packageID, int portalID, string skinName, string skinType, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateSkinPackage(skinPackageID={SkinPackageID},packageID={PackageID},portalID={PortalID},skinName={SkinName},skinType={SkinType},lastModifiedByUserID={LastModifiedByUserID})", skinPackageID, packageID, portalID, skinName, skinType, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateSkinPackage",
                skinPackageID,
                packageID,
                this.GetNull(portalID),
                skinName,
                skinType,
                lastModifiedByUserID);
				Logger.LogError("UpdateSkinPackage(skinPackageID,packageID,portalID,skinName,skinType,lastModifiedByUserID)={SkinPackageID},{PackageID},{PortalID},{SkinName},{SkinType},{LastModifiedByUserID}: database query executed in {Arg6} ms", skinPackageID, packageID, portalID, skinName, skinType, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateSkinPackage(skinPackageID={SkinPackageID},packageID={PackageID},portalID={PortalID},skinName={SkinName},skinType={SkinType},lastModifiedByUserID={LastModifiedByUserID})", skinPackageID, packageID, portalID, skinName, skinType, lastModifiedByUserID);
        }

        public virtual void AddProfile(int userId, int portalId)
        {
			Logger.LogError(">>> Entering AddProfile(userId={UserId},portalId={PortalId})", userId, portalId);
            this.ExecuteNonQuery("AddProfile", userId, portalId);
			Logger.LogWarning("<<< Exiting AddProfile(userId={UserId},portalId={PortalId})", userId, portalId);
        }

        public virtual IDataReader GetAllProfiles()
        {
			Logger.LogError(">>> Entering GetAllProfiles()");
            Logger.LogWarning("<<< Exiting GetAllProfiles()");
            return this.ExecuteReader("GetAllProfiles");
        }

        public virtual IDataReader GetProfile(int userId, int portalId)
        {
			Logger.LogError(">>> Entering GetProfile(userId={UserId},portalId={PortalId})", userId, portalId);
            Logger.LogWarning("<<< Exiting GetProfile(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.ExecuteReader("GetProfile", userId, portalId);
        }

        public virtual void UpdateProfile(int userId, int portalId, string profileData)
        {
			Logger.LogError(">>> Entering UpdateProfile(userId={UserId},portalId={PortalId},profileData={ProfileData})", userId, portalId, profileData);
            this.ExecuteNonQuery("UpdateProfile", userId, portalId, profileData);
			Logger.LogWarning("<<< Exiting UpdateProfile(userId={UserId},portalId={PortalId},profileData={ProfileData})", userId, portalId, profileData);
        }

        public virtual void AddUrl(int portalID, string url)
        {
			Logger.LogError(">>> Entering AddUrl(portalID={PortalID},url={Url})", portalID, url);
            this.ExecuteNonQuery("AddUrl", portalID, url);
			Logger.LogWarning("<<< Exiting AddUrl(portalID={PortalID},url={Url})", portalID, url);
        }

        public virtual void AddUrlLog(int urlTrackingID, int userID)
        {
			Logger.LogError(">>> Entering AddUrlLog(urlTrackingID={UrlTrackingID},userID={UserID})", urlTrackingID, userID);
            this.ExecuteNonQuery("AddUrlLog", urlTrackingID, this.GetNull(userID));
			Logger.LogWarning("<<< Exiting AddUrlLog(urlTrackingID={UrlTrackingID},userID={UserID})", urlTrackingID, userID);
        }

        public virtual void AddUrlTracking(int portalID, string url, string urlType, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
        {
			Logger.LogError(">>> Entering AddUrlTracking(portalID={PortalID},url={Url},urlType={UrlType},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow})", portalID, url, urlType, logActivity, trackClicks, moduleID, newWindow);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "AddUrlTracking",
                portalID,
                url,
                urlType,
                logActivity,
                trackClicks,
                this.GetNull(moduleID),
                newWindow);
				Logger.LogError("AddUrlTracking(portalID,url,urlType,logActivity,trackClicks,moduleID,newWindow)={PortalID},{Url},{UrlType},{LogActivity},{TrackClicks},{ModuleID},{NewWindow}: database query executed in {Arg7} ms", portalID, url, urlType, logActivity, trackClicks, moduleID, newWindow, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddUrlTracking(portalID={PortalID},url={Url},urlType={UrlType},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow})", portalID, url, urlType, logActivity, trackClicks, moduleID, newWindow);
        }

        public virtual void DeleteUrl(int portalID, string url)
        {
			Logger.LogError(">>> Entering DeleteUrl(portalID={PortalID},url={Url})", portalID, url);
            this.ExecuteNonQuery("DeleteUrl", portalID, url);
			Logger.LogWarning("<<< Exiting DeleteUrl(portalID={PortalID},url={Url})", portalID, url);
        }

        public virtual void DeleteUrlTracking(int portalID, string url, int moduleID)
        {
			Logger.LogError(">>> Entering DeleteUrlTracking(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
            this.ExecuteNonQuery("DeleteUrlTracking", portalID, url, this.GetNull(moduleID));
			Logger.LogWarning("<<< Exiting DeleteUrlTracking(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
        }

        public virtual IDataReader GetUrl(int portalID, string url)
        {
			Logger.LogError(">>> Entering GetUrl(portalID={PortalID},url={Url})", portalID, url);
            Logger.LogWarning("<<< Exiting GetUrl(portalID={PortalID},url={Url})", portalID, url);
            return this.ExecuteReader("GetUrl", portalID, url);
        }

        public virtual IDataReader GetUrlLog(int urlTrackingID, DateTime startDate, DateTime endDate)
        {
			Logger.LogError(">>> Entering GetUrlLog(urlTrackingID={UrlTrackingID},startDate={StartDate},endDate={EndDate})", urlTrackingID, startDate, endDate);
            Logger.LogWarning("<<< Exiting GetUrlLog(urlTrackingID={UrlTrackingID},startDate={StartDate},endDate={EndDate})", urlTrackingID, startDate, endDate);
            return this.ExecuteReader("GetUrlLog", urlTrackingID, this.GetNull(startDate), this.GetNull(endDate));
        }

        public virtual IDataReader GetUrls(int portalID)
        {
			Logger.LogError(">>> Entering GetUrls(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetUrls(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetUrls", portalID);
        }

        public virtual IDataReader GetUrlTracking(int portalID, string url, int moduleID)
        {
			Logger.LogError(">>> Entering GetUrlTracking(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
            Logger.LogWarning("<<< Exiting GetUrlTracking(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
            return this.ExecuteReader("GetUrlTracking", portalID, url, this.GetNull(moduleID));
        }

        public virtual void UpdateUrlTracking(int portalID, string url, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
        {
			Logger.LogError(">>> Entering UpdateUrlTracking(portalID={PortalID},url={Url},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow})", portalID, url, logActivity, trackClicks, moduleID, newWindow);
            this.ExecuteNonQuery("UpdateUrlTracking", portalID, url, logActivity, trackClicks, this.GetNull(moduleID), newWindow);
			Logger.LogWarning("<<< Exiting UpdateUrlTracking(portalID={PortalID},url={Url},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow})", portalID, url, logActivity, trackClicks, moduleID, newWindow);
        }

        public virtual void UpdateUrlTrackingStats(int portalID, string url, int moduleID)
        {
			Logger.LogError(">>> Entering UpdateUrlTrackingStats(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
            this.ExecuteNonQuery("UpdateUrlTrackingStats", portalID, url, this.GetNull(moduleID));
			Logger.LogWarning("<<< Exiting UpdateUrlTrackingStats(portalID={PortalID},url={Url},moduleID={ModuleID})", portalID, url, moduleID);
        }

        public virtual IDataReader GetDefaultLanguageByModule(string moduleList)
        {
			Logger.LogError(">>> Entering GetDefaultLanguageByModule(moduleList={ModuleList})", moduleList);
            Logger.LogWarning("<<< Exiting GetDefaultLanguageByModule(moduleList={ModuleList})", moduleList);
            return this.ExecuteReader("GetDefaultLanguageByModule", moduleList);
        }

        public virtual IDataReader GetSearchCommonWordsByLocale(string locale)
        {
			Logger.LogError(">>> Entering GetSearchCommonWordsByLocale(locale={Locale})", locale);
            Logger.LogWarning("<<< Exiting GetSearchCommonWordsByLocale(locale={Locale})", locale);
            return this.ExecuteReader("GetSearchCommonWordsByLocale", locale);
        }

        public virtual IDataReader GetSearchIndexers()
        {
			Logger.LogError(">>> Entering GetSearchIndexers()");
            Logger.LogWarning("<<< Exiting GetSearchIndexers()");
            return this.ExecuteReader("GetSearchIndexers");
        }

        public virtual IDataReader GetSearchResultModules(int portalID)
        {
			Logger.LogError(">>> Entering GetSearchResultModules(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetSearchResultModules(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetSearchResultModules", portalID);
        }

        public virtual IDataReader GetSearchSettings(int moduleId)
        {
			Logger.LogError(">>> Entering GetSearchSettings(moduleId={ModuleId})", moduleId);
            Logger.LogWarning("<<< Exiting GetSearchSettings(moduleId={ModuleId})", moduleId);
            return this.ExecuteReader("GetSearchSettings", moduleId);
        }

        public virtual int AddListEntry(string listName, string value, string text, int parentID, int level, bool enableSortOrder, int definitionID, string description, int portalID, bool systemList, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddListEntry(listName={ListName},value={Value},text={Text},parentID={ParentID},level={Level},enableSortOrder={EnableSortOrder},definitionID={DefinitionID},description={Description},portalID={PortalID},systemList={SystemList},createdByUserID={CreatedByUserID})", listName, value, text, parentID, level, enableSortOrder, definitionID, description, portalID, systemList, createdByUserID);
            try
            {
                return this.ExecuteScalar<int>(
                    "AddListEntry",
                    listName,
                    value,
                    text,
                    parentID,
                    level,
                    enableSortOrder,
                    definitionID,
                    description,
                    portalID,
                    systemList,
                    createdByUserID);
            }
            catch (SqlException ex)
            {
				Logger.LogError("Exception in AddListEntry(listName={ListName},value={Value},text={Text},parentID={ParentID},level={Level},enableSortOrder={EnableSortOrder},definitionID={DefinitionID},description={Description},portalID={PortalID},systemList={SystemList},createdByUserID={CreatedByUserID}): {Arg11}", listName, value, text, parentID, level, enableSortOrder, definitionID, description, portalID, systemList, createdByUserID, ex.Message);
                if (ex.Number == DuplicateKey)
                {
                    return Null.NullInteger;
                }

			Logger.LogWarning("<<< Exiting AddListEntry(listName={ListName},value={Value},text={Text},parentID={ParentID},level={Level},enableSortOrder={EnableSortOrder},definitionID={DefinitionID},description={Description},portalID={PortalID},systemList={SystemList},createdByUserID={CreatedByUserID})", listName, value, text, parentID, level, enableSortOrder, definitionID, description, portalID, systemList, createdByUserID);
                throw;
            }
        }

        public virtual void DeleteList(string listName, string parentKey)
        {
			Logger.LogError(">>> Entering DeleteList(listName={ListName},parentKey={ParentKey})", listName, parentKey);
            this.ExecuteNonQuery("DeleteList", listName, parentKey);
			Logger.LogWarning("<<< Exiting DeleteList(listName={ListName},parentKey={ParentKey})", listName, parentKey);
        }

        public virtual void DeleteListEntryByID(int entryID, bool deleteChild)
        {
			Logger.LogError(">>> Entering DeleteListEntryByID(entryID={EntryID},deleteChild={DeleteChild})", entryID, deleteChild);
            this.ExecuteNonQuery("DeleteListEntryByID", entryID, deleteChild);
			Logger.LogWarning("<<< Exiting DeleteListEntryByID(entryID={EntryID},deleteChild={DeleteChild})", entryID, deleteChild);
        }

        public virtual void DeleteListEntryByListName(string listName, string value, bool deleteChild)
        {
			Logger.LogError(">>> Entering DeleteListEntryByListName(listName={ListName},value={Value},deleteChild={DeleteChild})", listName, value, deleteChild);
            this.ExecuteNonQuery("DeleteListEntryByListName", listName, value, deleteChild);
			Logger.LogWarning("<<< Exiting DeleteListEntryByListName(listName={ListName},value={Value},deleteChild={DeleteChild})", listName, value, deleteChild);
        }

        public virtual IDataReader GetList(string listName, string parentKey, int portalID)
        {
			Logger.LogError(">>> Entering GetList(listName={ListName},parentKey={ParentKey},portalID={PortalID})", listName, parentKey, portalID);
            Logger.LogWarning("<<< Exiting GetList(listName={ListName},parentKey={ParentKey},portalID={PortalID})", listName, parentKey, portalID);
            return this.ExecuteReader("GetList", listName, parentKey, portalID);
        }

        public virtual IDataReader GetListEntriesByListName(string listName, string parentKey, int portalID)
        {
			Logger.LogError(">>> Entering GetListEntriesByListName(listName={ListName},parentKey={ParentKey},portalID={PortalID})", listName, parentKey, portalID);
            Logger.LogWarning("<<< Exiting GetListEntriesByListName(listName={ListName},parentKey={ParentKey},portalID={PortalID})", listName, parentKey, portalID);
            return this.ExecuteReader("GetListEntries", listName, parentKey, this.GetNull(portalID));
        }

        public virtual IDataReader GetListEntry(string listName, string value)
        {
			Logger.LogError(">>> Entering GetListEntry(listName={ListName},value={Value})", listName, value);
            Logger.LogWarning("<<< Exiting GetListEntry(listName={ListName},value={Value})", listName, value);
            return this.ExecuteReader("GetListEntry", listName, value, -1);
        }

        public virtual IDataReader GetListEntry(int entryID)
        {
            return this.ExecuteReader("GetListEntry", string.Empty, string.Empty, entryID);
        }

        public virtual IDataReader GetLists(int portalID)
        {
			Logger.LogError(">>> Entering GetLists(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetLists(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetLists", portalID);
        }

        public virtual void UpdateListEntry(int entryID, string value, string text, string description, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateListEntry(entryID={EntryID},value={Value},text={Text},description={Description},lastModifiedByUserID={LastModifiedByUserID})", entryID, value, text, description, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateListEntry", entryID, value, text, description, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateListEntry(entryID={EntryID},value={Value},text={Text},description={Description},lastModifiedByUserID={LastModifiedByUserID})", entryID, value, text, description, lastModifiedByUserID);
        }

        public virtual void UpdateListSortOrder(int entryID, bool moveUp)
        {
			Logger.LogError(">>> Entering UpdateListSortOrder(entryID={EntryID},moveUp={MoveUp})", entryID, moveUp);
            this.ExecuteNonQuery("UpdateListSortOrder", entryID, moveUp);
			Logger.LogWarning("<<< Exiting UpdateListSortOrder(entryID={EntryID},moveUp={MoveUp})", entryID, moveUp);
        }

        public virtual int AddPortalAlias(int portalID, string hTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddPortalAlias(portalID={PortalID},hTTPAlias={HTTPAlias},cultureCode={CultureCode},skin={Skin},browserType={BrowserType},isPrimary={IsPrimary},createdByUserID={CreatedByUserID})", portalID, hTTPAlias, cultureCode, skin, browserType, isPrimary, createdByUserID);
            Logger.LogWarning("<<< Exiting AddPortalAlias(portalID={PortalID},hTTPAlias={HTTPAlias},cultureCode={CultureCode},skin={Skin},browserType={BrowserType},isPrimary={IsPrimary},createdByUserID={CreatedByUserID})", portalID, hTTPAlias, cultureCode, skin, browserType, isPrimary, createdByUserID);
            return this.ExecuteScalar<int>("AddPortalAlias", portalID, hTTPAlias, this.GetNull(cultureCode), this.GetNull(skin), this.GetNull(browserType), isPrimary, createdByUserID);
        }

        public virtual void DeletePortalAlias(int portalAliasID)
        {
			Logger.LogError(">>> Entering DeletePortalAlias(portalAliasID={PortalAliasID})", portalAliasID);
            this.ExecuteNonQuery("DeletePortalAlias", portalAliasID);
			Logger.LogWarning("<<< Exiting DeletePortalAlias(portalAliasID={PortalAliasID})", portalAliasID);
        }

        public virtual IDataReader GetPortalAliases()
        {
			Logger.LogError(">>> Entering GetPortalAliases()");
            Logger.LogWarning("<<< Exiting GetPortalAliases()");
            return this.ExecuteReader("GetPortalAliases");
        }

        public virtual IDataReader GetPortalByPortalAliasID(int portalAliasId)
        {
			Logger.LogError(">>> Entering GetPortalByPortalAliasID(portalAliasId={PortalAliasId})", portalAliasId);
            Logger.LogWarning("<<< Exiting GetPortalByPortalAliasID(portalAliasId={PortalAliasId})", portalAliasId);
            return this.ExecuteReader("GetPortalByPortalAliasID", portalAliasId);
        }

        public virtual void UpdatePortalAlias(string portalAlias, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdatePortalAlias(portalAlias={PortalAlias},lastModifiedByUserID={LastModifiedByUserID})", portalAlias, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdatePortalAliasOnInstall", portalAlias, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdatePortalAlias(portalAlias={PortalAlias},lastModifiedByUserID={LastModifiedByUserID})", portalAlias, lastModifiedByUserID);
        }

        public virtual void UpdatePortalAliasInfo(int portalAliasID, int portalID, string hTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdatePortalAliasInfo(portalAliasID={PortalAliasID},portalID={PortalID},hTTPAlias={HTTPAlias},cultureCode={CultureCode},skin={Skin},browserType={BrowserType},isPrimary={IsPrimary},lastModifiedByUserID={LastModifiedByUserID})", portalAliasID, portalID, hTTPAlias, cultureCode, skin, browserType, isPrimary, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdatePortalAlias", portalAliasID, portalID, hTTPAlias, this.GetNull(cultureCode), this.GetNull(skin), this.GetNull(browserType), isPrimary, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdatePortalAliasInfo(portalAliasID={PortalAliasID},portalID={PortalID},hTTPAlias={HTTPAlias},cultureCode={CultureCode},skin={Skin},browserType={BrowserType},isPrimary={IsPrimary},lastModifiedByUserID={LastModifiedByUserID})", portalAliasID, portalID, hTTPAlias, cultureCode, skin, browserType, isPrimary, lastModifiedByUserID);
        }

        public virtual int AddEventMessage(string eventName, int priority, string processorType, string processorCommand, string body, string sender, string subscriberId, string authorizedRoles, string exceptionMessage, DateTime sentDate, DateTime expirationDate, string attributes)
        {
			Logger.LogError(">>> Entering AddEventMessage(eventName={EventName},priority={Priority},processorType={ProcessorType},processorCommand={ProcessorCommand},body={Body},sender={Sender},subscriberId=<redacted>,authorizedRoles={AuthorizedRoles},exceptionMessage={ExceptionMessage},sentDate={SentDate},expirationDate={ExpirationDate},attributes={Attributes})", eventName, priority, processorType, processorCommand, body, sender, authorizedRoles, exceptionMessage, sentDate, expirationDate, attributes);
			Logger.LogWarning("<<< Exiting AddEventMessage(eventName={EventName},priority={Priority},processorType={ProcessorType},processorCommand={ProcessorCommand},body={Body},sender={Sender},subscriberId=<redacted>,authorizedRoles={AuthorizedRoles},exceptionMessage={ExceptionMessage},sentDate={SentDate},expirationDate={ExpirationDate},attributes={Attributes})", eventName, priority, processorType, processorCommand, body, sender, authorizedRoles, exceptionMessage, sentDate, expirationDate, attributes);
            return this.ExecuteScalar<int>(
                "AddEventMessage",
                eventName,
                priority,
                processorType,
                processorCommand,
                body,
                sender,
                subscriberId,
                authorizedRoles,
                exceptionMessage,
                sentDate,
                expirationDate,
                attributes);
        }

        public virtual IDataReader GetEventMessages(string eventName)
        {
			Logger.LogError(">>> Entering GetEventMessages(eventName={EventName})", eventName);
            Logger.LogWarning("<<< Exiting GetEventMessages(eventName={EventName})", eventName);
            return this.ExecuteReader("GetEventMessages", eventName);
        }

        public virtual IDataReader GetEventMessagesBySubscriber(string eventName, string subscriberId)
        {
			Logger.LogError(">>> Entering GetEventMessagesBySubscriber(eventName={EventName},subscriberId=<redacted>)", eventName);
            Logger.LogWarning("<<< Exiting GetEventMessagesBySubscriber(eventName={EventName},subscriberId=<redacted>)", eventName);
            return this.ExecuteReader("GetEventMessagesBySubscriber", eventName, subscriberId);
        }

        public virtual void SetEventMessageComplete(int eventMessageId)
        {
			Logger.LogError(">>> Entering SetEventMessageComplete(eventMessageId={EventMessageId})", eventMessageId);
            this.ExecuteNonQuery("SetEventMessageComplete", eventMessageId);
			Logger.LogWarning("<<< Exiting SetEventMessageComplete(eventMessageId={EventMessageId})", eventMessageId);
        }

        public virtual int AddAuthentication(int packageID, string authenticationType, bool isEnabled, string settingsControlSrc, string loginControlSrc, string logoffControlSrc, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddAuthentication(packageID={PackageID},authenticationType={AuthenticationType},isEnabled={IsEnabled},settingsControlSrc={SettingsControlSrc},loginControlSrc={LoginControlSrc},logoffControlSrc={LogoffControlSrc},createdByUserID={CreatedByUserID})", packageID, authenticationType, isEnabled, settingsControlSrc, loginControlSrc, logoffControlSrc, createdByUserID);
            Logger.LogWarning("<<< Exiting AddAuthentication(packageID={PackageID},authenticationType={AuthenticationType},isEnabled={IsEnabled},settingsControlSrc={SettingsControlSrc},loginControlSrc={LoginControlSrc},logoffControlSrc={LogoffControlSrc},createdByUserID={CreatedByUserID})", packageID, authenticationType, isEnabled, settingsControlSrc, loginControlSrc, logoffControlSrc, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddAuthentication",
                packageID,
                authenticationType,
                isEnabled,
                settingsControlSrc,
                loginControlSrc,
                logoffControlSrc,
                createdByUserID);
        }

        public virtual int AddUserAuthentication(int userID, string authenticationType, string authenticationToken, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddUserAuthentication(userID={UserID},authenticationType={AuthenticationType},authenticationToken=<redacted>,createdByUserID={CreatedByUserID})", userID, authenticationType, createdByUserID);
            Logger.LogWarning("<<< Exiting AddUserAuthentication(userID={UserID},authenticationType={AuthenticationType},authenticationToken=<redacted>,createdByUserID={CreatedByUserID})", userID, authenticationType, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddUserAuthentication",
                userID,
                authenticationType,
                authenticationToken,
                createdByUserID);
        }

        /// <summary>Get a User Authentication record from SQL database. DNN-4016.</summary>
        /// <param name="userID">The ID of the user.</param>
        /// <returns>UserAuthentication record.</returns>
        public virtual IDataReader GetUserAuthentication(int userID)
        {
			Logger.LogError(">>> Entering GetUserAuthentication(userID={UserID})", userID);
            Logger.LogWarning("<<< Exiting GetUserAuthentication(userID={UserID})", userID);
            return this.ExecuteReader("GetUserAuthentication", userID);
        }

        public virtual void DeleteAuthentication(int authenticationID)
        {
			Logger.LogError(">>> Entering DeleteAuthentication(authenticationID={AuthenticationID})", authenticationID);
            this.ExecuteNonQuery("DeleteAuthentication", authenticationID);
			Logger.LogWarning("<<< Exiting DeleteAuthentication(authenticationID={AuthenticationID})", authenticationID);
        }

        public virtual IDataReader GetAuthenticationService(int authenticationID)
        {
			Logger.LogError(">>> Entering GetAuthenticationService(authenticationID={AuthenticationID})", authenticationID);
            Logger.LogWarning("<<< Exiting GetAuthenticationService(authenticationID={AuthenticationID})", authenticationID);
            return this.ExecuteReader("GetAuthenticationService", authenticationID);
        }

        public virtual IDataReader GetAuthenticationServiceByPackageID(int packageID)
        {
			Logger.LogError(">>> Entering GetAuthenticationServiceByPackageID(packageID={PackageID})", packageID);
            Logger.LogWarning("<<< Exiting GetAuthenticationServiceByPackageID(packageID={PackageID})", packageID);
            return this.ExecuteReader("GetAuthenticationServiceByPackageID", packageID);
        }

        public virtual IDataReader GetAuthenticationServiceByType(string authenticationType)
        {
			Logger.LogError(">>> Entering GetAuthenticationServiceByType(authenticationType={AuthenticationType})", authenticationType);
            Logger.LogWarning("<<< Exiting GetAuthenticationServiceByType(authenticationType={AuthenticationType})", authenticationType);
            return this.ExecuteReader("GetAuthenticationServiceByType", authenticationType);
        }

        public virtual IDataReader GetAuthenticationServices()
        {
			Logger.LogError(">>> Entering GetAuthenticationServices()");
            Logger.LogWarning("<<< Exiting GetAuthenticationServices()");
            return this.ExecuteReader("GetAuthenticationServices");
        }

        public virtual IDataReader GetEnabledAuthenticationServices()
        {
			Logger.LogError(">>> Entering GetEnabledAuthenticationServices()");
            Logger.LogWarning("<<< Exiting GetEnabledAuthenticationServices()");
            return this.ExecuteReader("GetEnabledAuthenticationServices");
        }

        public virtual void UpdateAuthentication(int authenticationID, int packageID, string authenticationType, bool isEnabled, string settingsControlSrc, string loginControlSrc, string logoffControlSrc, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateAuthentication(authenticationID={AuthenticationID},packageID={PackageID},authenticationType={AuthenticationType},isEnabled={IsEnabled},settingsControlSrc={SettingsControlSrc},loginControlSrc={LoginControlSrc},logoffControlSrc={LogoffControlSrc},lastModifiedByUserID={LastModifiedByUserID})", authenticationID, packageID, authenticationType, isEnabled, settingsControlSrc, loginControlSrc, logoffControlSrc, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateAuthentication",
                authenticationID,
                packageID,
                authenticationType,
                isEnabled,
                settingsControlSrc,
                loginControlSrc,
                logoffControlSrc,
                lastModifiedByUserID);
				Logger.LogError("UpdateAuthentication(authenticationID,packageID,authenticationType,isEnabled,settingsControlSrc,loginControlSrc,logoffControlSrc,lastModifiedByUserID)={AuthenticationID},{PackageID},{AuthenticationType},{IsEnabled},{SettingsControlSrc},{LoginControlSrc},{LogoffControlSrc},{LastModifiedByUserID}: database query executed in {Arg8} ms", authenticationID, packageID, authenticationType, isEnabled, settingsControlSrc, loginControlSrc, logoffControlSrc, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateAuthentication(authenticationID={AuthenticationID},packageID={PackageID},authenticationType={AuthenticationType},isEnabled={IsEnabled},settingsControlSrc={SettingsControlSrc},loginControlSrc={LoginControlSrc},logoffControlSrc={LogoffControlSrc},lastModifiedByUserID={LastModifiedByUserID})", authenticationID, packageID, authenticationType, isEnabled, settingsControlSrc, loginControlSrc, logoffControlSrc, lastModifiedByUserID);
        }

        public virtual int AddPackage(int portalID, string name, string friendlyName, string description, string type, string version, string license, string manifest, string owner, string organization, string url, string email, string releaseNotes, bool isSystemPackage, int createdByUserID, string folderName, string iconFile)
        {
			Logger.LogError(">>> Entering AddPackage(portalID={PortalID},name={Name},friendlyName={FriendlyName},description={Description},type={Type},version={Version},license={License},manifest={Manifest},owner={Owner},organization={Organization},url={Url},email={Email},releaseNotes={ReleaseNotes},isSystemPackage={IsSystemPackage},createdByUserID={CreatedByUserID},folderName={FolderName},iconFile={IconFile})", portalID, name, friendlyName, description, type, version, license, manifest, owner, organization, url, email, releaseNotes, isSystemPackage, createdByUserID, folderName, iconFile);
			Logger.LogWarning("<<< Exiting AddPackage(portalID={PortalID},name={Name},friendlyName={FriendlyName},description={Description},type={Type},version={Version},license={License},manifest={Manifest},owner={Owner},organization={Organization},url={Url},email={Email},releaseNotes={ReleaseNotes},isSystemPackage={IsSystemPackage},createdByUserID={CreatedByUserID},folderName={FolderName},iconFile={IconFile})", portalID, name, friendlyName, description, type, version, license, manifest, owner, organization, url, email, releaseNotes, isSystemPackage, createdByUserID, folderName, iconFile);
            return this.ExecuteScalar<int>(
                "AddPackage",
                this.GetNull(portalID),
                name,
                friendlyName,
                description,
                type,
                version,
                license,
                manifest,
                owner,
                organization,
                url,
                email,
                releaseNotes,
                isSystemPackage,
                createdByUserID,
                folderName,
                iconFile);
        }

        public virtual void DeletePackage(int packageID)
        {
			Logger.LogError(">>> Entering DeletePackage(packageID={PackageID})", packageID);
            this.ExecuteNonQuery("DeletePackage", packageID);
			Logger.LogWarning("<<< Exiting DeletePackage(packageID={PackageID})", packageID);
        }

        public virtual IDataReader GetModulePackagesInUse(int portalID, bool forHost)
        {
			Logger.LogError(">>> Entering GetModulePackagesInUse(portalID={PortalID},forHost={ForHost})", portalID, forHost);
            Logger.LogWarning("<<< Exiting GetModulePackagesInUse(portalID={PortalID},forHost={ForHost})", portalID, forHost);
            return this.ExecuteReader("GetModulePackagesInUse", portalID, forHost);
        }

        public virtual IDataReader GetPackageDependencies()
        {
			Logger.LogError(">>> Entering GetPackageDependencies()");
            Logger.LogWarning("<<< Exiting GetPackageDependencies()");
            return this.ExecuteReader("GetPackageDependencies");
        }

        public virtual IDataReader GetPackages(int portalID)
        {
			Logger.LogError(">>> Entering GetPackages(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetPackages(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetPackages", this.GetNull(portalID));
        }

        public virtual IDataReader GetPackageTypes()
        {
			Logger.LogError(">>> Entering GetPackageTypes()");
            Logger.LogWarning("<<< Exiting GetPackageTypes()");
            return this.ExecuteReader("GetPackageTypes");
        }

        public virtual int RegisterAssembly(int packageID, string assemblyName, string version)
        {
			Logger.LogError(">>> Entering RegisterAssembly(packageID={PackageID},assemblyName={AssemblyName},version={Version})", packageID, assemblyName, version);
            Logger.LogWarning("<<< Exiting RegisterAssembly(packageID={PackageID},assemblyName={AssemblyName},version={Version})", packageID, assemblyName, version);
            return this.ExecuteScalar<int>("RegisterAssembly", this.GetNull(packageID), assemblyName, version);
        }

        public virtual int SavePackageDependency(int packageDependencyId, int packageId, string packageName, string version)
        {
			Logger.LogError(">>> Entering SavePackageDependency(packageDependencyId={PackageDependencyId},packageId={PackageId},packageName={PackageName},version={Version})", packageDependencyId, packageId, packageName, version);
            Logger.LogWarning("<<< Exiting SavePackageDependency(packageDependencyId={PackageDependencyId},packageId={PackageId},packageName={PackageName},version={Version})", packageDependencyId, packageId, packageName, version);
            return this.ExecuteScalar<int>("SavePackageDependency", packageDependencyId, packageId, packageName, version);
        }

        public virtual bool UnRegisterAssembly(int packageID, string assemblyName)
        {
			Logger.LogError(">>> Entering UnRegisterAssembly(packageID={PackageID},assemblyName={AssemblyName})", packageID, assemblyName);
            Logger.LogWarning("<<< Exiting UnRegisterAssembly(packageID={PackageID},assemblyName={AssemblyName})", packageID, assemblyName);
            return this.ExecuteScalar<int>("UnRegisterAssembly", packageID, assemblyName) == 1;
        }

        public virtual void UpdatePackage(int packageID, int portalID, string friendlyName, string description, string type, string version, string license, string manifest, string owner, string organization, string url, string email, string releaseNotes, bool isSystemPackage, int lastModifiedByUserID, string folderName, string iconFile)
        {
			Logger.LogError(">>> Entering UpdatePackage(packageID={PackageID},portalID={PortalID},friendlyName={FriendlyName},description={Description},type={Type},version={Version},license={License},manifest={Manifest},owner={Owner},organization={Organization},url={Url},email={Email},releaseNotes={ReleaseNotes},isSystemPackage={IsSystemPackage},lastModifiedByUserID={LastModifiedByUserID},folderName={FolderName},iconFile={IconFile})", packageID, portalID, friendlyName, description, type, version, license, manifest, owner, organization, url, email, releaseNotes, isSystemPackage, lastModifiedByUserID, folderName, iconFile);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdatePackage",
                packageID,
                this.GetNull(portalID),
                friendlyName,
                description,
                type,
                version,
                license,
                manifest,
                owner,
                organization,
                url,
                email,
                releaseNotes,
                isSystemPackage,
                lastModifiedByUserID,
                folderName,
                iconFile);
				Logger.LogError("UpdatePackage(packageID,portalID,friendlyName,description,type,version,license,manifest,owner,organization,url,email,releaseNotes,isSystemPackage,lastModifiedByUserID,folderName,iconFile)={PackageID},{PortalID},{FriendlyName},{Description},{Type},{Version},{License},{Manifest},{Owner},{Organization},{Url},{Email},{ReleaseNotes},{IsSystemPackage},{LastModifiedByUserID},{FolderName},{IconFile}: database query executed in {Arg17} ms", packageID, portalID, friendlyName, description, type, version, license, manifest, owner, organization, url, email, releaseNotes, isSystemPackage, lastModifiedByUserID, folderName, iconFile, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdatePackage(packageID={PackageID},portalID={PortalID},friendlyName={FriendlyName},description={Description},type={Type},version={Version},license={License},manifest={Manifest},owner={Owner},organization={Organization},url={Url},email={Email},releaseNotes={ReleaseNotes},isSystemPackage={IsSystemPackage},lastModifiedByUserID={LastModifiedByUserID},folderName={FolderName},iconFile={IconFile})", packageID, portalID, friendlyName, description, type, version, license, manifest, owner, organization, url, email, releaseNotes, isSystemPackage, lastModifiedByUserID, folderName, iconFile);
        }

        public virtual void SetCorePackageVersions()
        {
			Logger.LogError(">>> Entering SetCorePackageVersions()");
            this.ExecuteNonQuery("SetCorePackageVersions");
			Logger.LogWarning("<<< Exiting SetCorePackageVersions()");
        }

        public virtual int AddLanguage(string cultureCode, string cultureName, string fallbackCulture, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddLanguage(cultureCode={CultureCode},cultureName={CultureName},fallbackCulture={FallbackCulture},createdByUserID={CreatedByUserID})", cultureCode, cultureName, fallbackCulture, createdByUserID);
            Logger.LogWarning("<<< Exiting AddLanguage(cultureCode={CultureCode},cultureName={CultureName},fallbackCulture={FallbackCulture},createdByUserID={CreatedByUserID})", cultureCode, cultureName, fallbackCulture, createdByUserID);
            return this.ExecuteScalar<int>("AddLanguage", cultureCode, cultureName, fallbackCulture, createdByUserID);
        }

        public virtual int AddLanguagePack(int packageID, int languageID, int dependentPackageID, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddLanguagePack(packageID={PackageID},languageID={LanguageID},dependentPackageID={DependentPackageID},createdByUserID={CreatedByUserID})", packageID, languageID, dependentPackageID, createdByUserID);
            Logger.LogWarning("<<< Exiting AddLanguagePack(packageID={PackageID},languageID={LanguageID},dependentPackageID={DependentPackageID},createdByUserID={CreatedByUserID})", packageID, languageID, dependentPackageID, createdByUserID);
            return this.ExecuteScalar<int>("AddLanguagePack", packageID, languageID, dependentPackageID, createdByUserID);
        }

        public virtual int AddPortalLanguage(int portalID, int languageID, bool isPublished, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddPortalLanguage(portalID={PortalID},languageID={LanguageID},isPublished={IsPublished},createdByUserID={CreatedByUserID})", portalID, languageID, isPublished, createdByUserID);
            Logger.LogWarning("<<< Exiting AddPortalLanguage(portalID={PortalID},languageID={LanguageID},isPublished={IsPublished},createdByUserID={CreatedByUserID})", portalID, languageID, isPublished, createdByUserID);
            return this.ExecuteScalar<int>("AddPortalLanguage", portalID, languageID, isPublished, createdByUserID);
        }

        public virtual void DeleteLanguage(int languageID)
        {
			Logger.LogError(">>> Entering DeleteLanguage(languageID={LanguageID})", languageID);
            this.ExecuteNonQuery("DeleteLanguage", languageID);
			Logger.LogWarning("<<< Exiting DeleteLanguage(languageID={LanguageID})", languageID);
        }

        public virtual void DeleteLanguagePack(int languagePackID)
        {
			Logger.LogError(">>> Entering DeleteLanguagePack(languagePackID={LanguagePackID})", languagePackID);
            this.ExecuteNonQuery("DeleteLanguagePack", languagePackID);
			Logger.LogWarning("<<< Exiting DeleteLanguagePack(languagePackID={LanguagePackID})", languagePackID);
        }

        public virtual void DeletePortalLanguages(int portalID, int languageID)
        {
			Logger.LogError(">>> Entering DeletePortalLanguages(portalID={PortalID},languageID={LanguageID})", portalID, languageID);
            this.ExecuteNonQuery("DeletePortalLanguages", this.GetNull(portalID), this.GetNull(languageID));
			Logger.LogWarning("<<< Exiting DeletePortalLanguages(portalID={PortalID},languageID={LanguageID})", portalID, languageID);
        }

        public virtual void EnsureLocalizationExists(int portalID, string cultureCode)
        {
			Logger.LogError(">>> Entering EnsureLocalizationExists(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
            this.ExecuteNonQuery("EnsureLocalizationExists", portalID, cultureCode);
			Logger.LogWarning("<<< Exiting EnsureLocalizationExists(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
        }

        public virtual void RemovePortalLocalization(int portalID, string cultureCode)
        {
			Logger.LogError(">>> Entering RemovePortalLocalization(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
            this.ExecuteNonQuery("RemovePortalLocalization", portalID, cultureCode);
			Logger.LogWarning("<<< Exiting RemovePortalLocalization(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
        }

        public virtual IDataReader GetPortalLocalizations(int portalID)
        {
			Logger.LogError(">>> Entering GetPortalLocalizations(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetPortalLocalizations(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetPortalLocalizations", portalID);
        }

        public virtual IDataReader GetLanguages()
        {
			Logger.LogError(">>> Entering GetLanguages()");
            Logger.LogWarning("<<< Exiting GetLanguages()");
            return this.ExecuteReader("GetLanguages");
        }

        public virtual IDataReader GetLanguagePackByPackage(int packageID)
        {
			Logger.LogError(">>> Entering GetLanguagePackByPackage(packageID={PackageID})", packageID);
            Logger.LogWarning("<<< Exiting GetLanguagePackByPackage(packageID={PackageID})", packageID);
            return this.ExecuteReader("GetLanguagePackByPackage", packageID);
        }

        public virtual IDataReader GetLanguagesByPortal(int portalID)
        {
			Logger.LogError(">>> Entering GetLanguagesByPortal(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetLanguagesByPortal(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetLanguagesByPortal", portalID);
        }

        public virtual string GetPortalDefaultLanguage(int portalID)
        {
			Logger.LogError(">>> Entering GetPortalDefaultLanguage(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetPortalDefaultLanguage(portalID={PortalID})", portalID);
            return this.ExecuteScalar<string>("GetPortalDefaultLanguage", portalID);
        }

        public virtual void UpdateLanguage(int languageID, string cultureCode, string cultureName, string fallbackCulture, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateLanguage(languageID={LanguageID},cultureCode={CultureCode},cultureName={CultureName},fallbackCulture={FallbackCulture},lastModifiedByUserID={LastModifiedByUserID})", languageID, cultureCode, cultureName, fallbackCulture, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateLanguage",
                languageID,
                cultureCode,
                cultureName,
                fallbackCulture,
                lastModifiedByUserID);
				Logger.LogError("UpdateLanguage(languageID,cultureCode,cultureName,fallbackCulture,lastModifiedByUserID)={LanguageID},{CultureCode},{CultureName},{FallbackCulture},{LastModifiedByUserID}: database query executed in {Arg5} ms", languageID, cultureCode, cultureName, fallbackCulture, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateLanguage(languageID={LanguageID},cultureCode={CultureCode},cultureName={CultureName},fallbackCulture={FallbackCulture},lastModifiedByUserID={LastModifiedByUserID})", languageID, cultureCode, cultureName, fallbackCulture, lastModifiedByUserID);
        }

        public virtual int UpdateLanguagePack(int languagePackID, int packageID, int languageID, int dependentPackageID, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateLanguagePack(languagePackID={LanguagePackID},packageID={PackageID},languageID={LanguageID},dependentPackageID={DependentPackageID},lastModifiedByUserID={LastModifiedByUserID})", languagePackID, packageID, languageID, dependentPackageID, lastModifiedByUserID);
            Logger.LogWarning("<<< Exiting UpdateLanguagePack(languagePackID={LanguagePackID},packageID={PackageID},languageID={LanguageID},dependentPackageID={DependentPackageID},lastModifiedByUserID={LastModifiedByUserID})", languagePackID, packageID, languageID, dependentPackageID, lastModifiedByUserID);
            return this.ExecuteScalar<int>(
                "UpdateLanguagePack",
                languagePackID,
                packageID,
                languageID,
                dependentPackageID,
                lastModifiedByUserID);
        }

        public virtual void UpdatePortalDefaultLanguage(int portalID, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalDefaultLanguage(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
            this.ExecuteNonQuery("UpdatePortalDefaultLanguage", portalID, cultureCode);
			Logger.LogWarning("<<< Exiting UpdatePortalDefaultLanguage(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
        }

        public virtual void UpdatePortalLanguage(int portalID, int languageID, bool isPublished, int updatedByUserID)
        {
			Logger.LogError(">>> Entering UpdatePortalLanguage(portalID={PortalID},languageID={LanguageID},isPublished={IsPublished},updatedByUserID={UpdatedByUserID})", portalID, languageID, isPublished, updatedByUserID);
            this.ExecuteNonQuery("UpdatePortalLanguage", portalID, languageID, isPublished, updatedByUserID);
			Logger.LogWarning("<<< Exiting UpdatePortalLanguage(portalID={PortalID},languageID={LanguageID},isPublished={IsPublished},updatedByUserID={UpdatedByUserID})", portalID, languageID, isPublished, updatedByUserID);
        }

        public virtual void AddDefaultFolderTypes(int portalID)
        {
			Logger.LogError(">>> Entering AddDefaultFolderTypes(portalID={PortalID})", portalID);
            this.ExecuteNonQuery("AddDefaultFolderTypes", portalID);
			Logger.LogWarning("<<< Exiting AddDefaultFolderTypes(portalID={PortalID})", portalID);
        }

        public virtual int AddFolderMapping(int portalID, string mappingName, string folderProviderType, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddFolderMapping(portalID={PortalID},mappingName=<redacted>,folderProviderType={FolderProviderType},createdByUserID={CreatedByUserID})", portalID, folderProviderType, createdByUserID);
            Logger.LogWarning("<<< Exiting AddFolderMapping(portalID={PortalID},mappingName=<redacted>,folderProviderType={FolderProviderType},createdByUserID={CreatedByUserID})", portalID, folderProviderType, createdByUserID);
            return this.ExecuteScalar<int>(
                "AddFolderMapping",
                this.GetNull(portalID),
                mappingName,
                folderProviderType,
                createdByUserID);
        }

        public virtual void AddFolderMappingSetting(int folderMappingID, string settingName, string settingValue, int createdByUserID)
        {
			Logger.LogError(">>> Entering AddFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName},settingValue={SettingValue},createdByUserID={CreatedByUserID})", settingName, settingValue, createdByUserID);
            this.ExecuteNonQuery("AddFolderMappingsSetting", folderMappingID, settingName, settingValue, createdByUserID);
			Logger.LogWarning("<<< Exiting AddFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName},settingValue={SettingValue},createdByUserID={CreatedByUserID})", settingName, settingValue, createdByUserID);
        }

        public virtual void DeleteFolderMapping(int folderMappingID)
        {
			Logger.LogError(">>> Entering DeleteFolderMapping(folderMappingID=<redacted>)");
            this.ExecuteNonQuery("DeleteFolderMapping", folderMappingID);
			Logger.LogWarning("<<< Exiting DeleteFolderMapping(folderMappingID=<redacted>)");
        }

        public virtual IDataReader GetFolderMapping(int folderMappingID)
        {
			Logger.LogError(">>> Entering GetFolderMapping(folderMappingID=<redacted>)");
            Logger.LogWarning("<<< Exiting GetFolderMapping(folderMappingID=<redacted>)");
            return this.ExecuteReader("GetFolderMapping", folderMappingID);
        }

        public virtual IDataReader GetFolderMappingByMappingName(int portalID, string mappingName)
        {
			Logger.LogError(">>> Entering GetFolderMappingByMappingName(portalID={PortalID},mappingName=<redacted>)", portalID);
            Logger.LogWarning("<<< Exiting GetFolderMappingByMappingName(portalID={PortalID},mappingName=<redacted>)", portalID);
            return this.ExecuteReader("GetFolderMappingByMappingName", portalID, mappingName);
        }

        public virtual IDataReader GetFolderMappings(int portalID)
        {
			Logger.LogError(">>> Entering GetFolderMappings(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetFolderMappings(portalID={PortalID})", portalID);
            return this.ExecuteReader("GetFolderMappings", this.GetNull(portalID));
        }

        public virtual IDataReader GetFolderMappingSetting(int folderMappingID, string settingName)
        {
			Logger.LogError(">>> Entering GetFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName})", settingName);
            Logger.LogWarning("<<< Exiting GetFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName})", settingName);
            return this.ExecuteReader("GetFolderMappingsSetting", folderMappingID, settingName);
        }

        public virtual IDataReader GetFolderMappingSettings(int folderMappingID)
        {
			Logger.LogError(">>> Entering GetFolderMappingSettings(folderMappingID=<redacted>)");
            Logger.LogWarning("<<< Exiting GetFolderMappingSettings(folderMappingID=<redacted>)");
            return this.ExecuteReader("GetFolderMappingsSettings", folderMappingID);
        }

        public virtual void UpdateFolderMapping(int folderMappingID, string mappingName, int priority, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateFolderMapping(folderMappingID=<redacted>,mappingName=<redacted>,priority={Priority},lastModifiedByUserID={LastModifiedByUserID})", priority, lastModifiedByUserID);
            this.ExecuteNonQuery("UpdateFolderMapping", folderMappingID, mappingName, priority, lastModifiedByUserID);
			Logger.LogWarning("<<< Exiting UpdateFolderMapping(folderMappingID=<redacted>,mappingName=<redacted>,priority={Priority},lastModifiedByUserID={LastModifiedByUserID})", priority, lastModifiedByUserID);
        }

        public virtual void UpdateFolderMappingSetting(int folderMappingID, string settingName, string settingValue, int lastModifiedByUserID)
        {
			Logger.LogError(">>> Entering UpdateFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", settingName, settingValue, lastModifiedByUserID);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateFolderMappingsSetting",
                folderMappingID,
                settingName,
                settingValue,
                lastModifiedByUserID);
				Logger.LogError("UpdateFolderMappingSetting(folderMappingID,settingName,settingValue,lastModifiedByUserID)=<redacted>,{SettingName},{SettingValue},{LastModifiedByUserID}: database query executed in {Arg4} ms", settingName, settingValue, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateFolderMappingSetting(folderMappingID=<redacted>,settingName={SettingName},settingValue={SettingValue},lastModifiedByUserID={LastModifiedByUserID})", settingName, settingValue, lastModifiedByUserID);
        }

        /// <inheritdoc cref="GetPasswordHistory(int,int,int)" />
        [DnnDeprecated(9, 2, 0, "Please use the overload that takes passwordsRetained and daysRetained")]
        public virtual partial IDataReader GetPasswordHistory(int userId)
        {
			Logger.LogError(">>> Entering GetPasswordHistory(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetPasswordHistory(userId={UserId})", userId);
			Logger.LogWarning("<<< Exiting GetPasswordHistory(userId={UserId},passwordsRetained=<redacted>,daysRetained={DaysRetained})", userId, daysRetained);
            return this.GetPasswordHistory(userId, int.MaxValue, int.MaxValue);
        }

        public virtual IDataReader GetPasswordHistory(int userId, int passwordsRetained, int daysRetained)
        {
            return this.ExecuteReader("GetPasswordHistory", this.GetNull(userId), passwordsRetained, daysRetained);
        }

        /// <inheritdoc cref="AddPasswordHistory(int,string,string,int,int)" />
        [DnnDeprecated(9, 2, 0, "Please use the overload that takes daysRetained")]
        public virtual partial void AddPasswordHistory(int userId, string password, string passwordHistory, int retained)
        {
			Logger.LogError(">>> Entering AddPasswordHistory(userId={UserId},password=<redacted>,passwordHistory=<redacted>,retained={Retained})", userId, retained);
            this.AddPasswordHistory(userId, password, passwordHistory, retained, int.MaxValue);
			Logger.LogWarning("<<< Exiting AddPasswordHistory(userId={UserId},password=<redacted>,passwordHistory=<redacted>,retained={Retained})", userId, retained);
        }

        public virtual void AddPasswordHistory(int userId, string password, string passwordHistory, int passwordsRetained, int daysRetained)
        {
			Logger.LogError(">>> Entering AddPasswordHistory(userId={UserId},password=<redacted>,passwordHistory=<redacted>,passwordsRetained=<redacted>,daysRetained={DaysRetained})", userId, daysRetained);
            this.ExecuteNonQuery("AddPasswordHistory", this.GetNull(userId), password, passwordHistory, passwordsRetained, daysRetained, this.GetNull(userId));
			Logger.LogWarning("<<< Exiting AddPasswordHistory(userId={UserId},password=<redacted>,passwordHistory=<redacted>,passwordsRetained=<redacted>,daysRetained={DaysRetained})", userId, daysRetained);
        }

        public virtual DateTime GetDatabaseTimeUtc()
        {
			Logger.LogError(">>> Entering GetDatabaseTimeUtc()");
            Logger.LogWarning("<<< Exiting GetDatabaseTimeUtc()");
            return this.ExecuteScalar<DateTime>("GetDatabaseTimeUtc");
        }

        public virtual DateTime GetDatabaseTime()
        {
			Logger.LogError(">>> Entering GetDatabaseTime()");
            Logger.LogWarning("<<< Exiting GetDatabaseTime()");
            return this.ExecuteScalar<DateTime>("GetDatabaseTime");
        }

        public virtual DateTimeOffset GetDatabaseTimeOffset()
        {
			Logger.LogError(">>> Entering GetDatabaseTimeOffset()");
            using (var reader = (SqlDataReader)this.ExecuteSQL("SELECT SYSDATETIMEOFFSET()"))
            {
                reader.Read();
			Logger.LogWarning("<<< Exiting GetDatabaseTimeOffset()");
                return reader.GetDateTimeOffset(0);
            }
        }

        public virtual void DeletePreviewProfile(int id)
        {
			Logger.LogError(">>> Entering DeletePreviewProfile(id={Id})", id);
            this.ExecuteNonQuery("Mobile_DeletePreviewProfile", id);
			Logger.LogWarning("<<< Exiting DeletePreviewProfile(id={Id})", id);
        }

        public virtual void DeleteRedirection(int id)
        {
			Logger.LogError(">>> Entering DeleteRedirection(id={Id})", id);
            this.ExecuteNonQuery("Mobile_DeleteRedirection", id);
			Logger.LogWarning("<<< Exiting DeleteRedirection(id={Id})", id);
        }

        public virtual void DeleteRedirectionRule(int id)
        {
			Logger.LogError(">>> Entering DeleteRedirectionRule(id={Id})", id);
            this.ExecuteNonQuery("Mobile_DeleteRedirectionRule", id);
			Logger.LogWarning("<<< Exiting DeleteRedirectionRule(id={Id})", id);
        }

        public virtual IDataReader GetAllRedirections()
        {
			Logger.LogError(">>> Entering GetAllRedirections()");
            Logger.LogWarning("<<< Exiting GetAllRedirections()");
            return this.ExecuteReader("Mobile_GetAllRedirections");
        }

        public virtual IDataReader GetPreviewProfiles(int portalId)
        {
			Logger.LogError(">>> Entering GetPreviewProfiles(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPreviewProfiles(portalId={PortalId})", portalId);
            return this.ExecuteReader("Mobile_GetPreviewProfiles", portalId);
        }

        public virtual IDataReader GetRedirectionRules(int redirectionId)
        {
			Logger.LogError(">>> Entering GetRedirectionRules(redirectionId={RedirectionId})", redirectionId);
            Logger.LogWarning("<<< Exiting GetRedirectionRules(redirectionId={RedirectionId})", redirectionId);
            return this.ExecuteReader("Mobile_GetRedirectionRules", redirectionId);
        }

        public virtual IDataReader GetRedirections(int portalId)
        {
			Logger.LogError(">>> Entering GetRedirections(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetRedirections(portalId={PortalId})", portalId);
            return this.ExecuteReader("Mobile_GetRedirections", portalId);
        }

        public virtual int SavePreviewProfile(int id, int portalId, string name, int width, int height, string userAgent, int sortOrder, int userId)
        {
			Logger.LogError(">>> Entering SavePreviewProfile(id={Id},portalId={PortalId},name={Name},width={Width},height={Height},userAgent={UserAgent},sortOrder={SortOrder},userId={UserId})", id, portalId, name, width, height, userAgent, sortOrder, userId);
            Logger.LogWarning("<<< Exiting SavePreviewProfile(id={Id},portalId={PortalId},name={Name},width={Width},height={Height},userAgent={UserAgent},sortOrder={SortOrder},userId={UserId})", id, portalId, name, width, height, userAgent, sortOrder, userId);
            return this.ExecuteScalar<int>(
                "Mobile_SavePreviewProfile",
                id,
                portalId,
                name,
                width,
                height,
                userAgent,
                sortOrder,
                userId);
        }

        public virtual int SaveRedirection(int id, int portalId, string name, int type, int sortOrder, int sourceTabId, bool includeChildTabs, int targetType, object targetValue, bool enabled, int userId)
        {
			Logger.LogError(">>> Entering SaveRedirection(id={Id},portalId={PortalId},name={Name},type={Type},sortOrder={SortOrder},sourceTabId={SourceTabId},includeChildTabs={IncludeChildTabs},targetType={TargetType},targetValue={TargetValue},enabled={Enabled},userId={UserId})", id, portalId, name, type, sortOrder, sourceTabId, includeChildTabs, targetType, targetValue, enabled, userId);
            Logger.LogWarning("<<< Exiting SaveRedirection(id={Id},portalId={PortalId},name={Name},type={Type},sortOrder={SortOrder},sourceTabId={SourceTabId},includeChildTabs={IncludeChildTabs},targetType={TargetType},targetValue={TargetValue},enabled={Enabled},userId={UserId})", id, portalId, name, type, sortOrder, sourceTabId, includeChildTabs, targetType, targetValue, enabled, userId);
            return this.ExecuteScalar<int>(
                "Mobile_SaveRedirection",
                id,
                portalId,
                name,
                type,
                sortOrder,
                sourceTabId,
                includeChildTabs,
                targetType,
                targetValue,
                enabled,
                userId);
        }

        public virtual void SaveRedirectionRule(int id, int redirectionId, string capbility, string expression)
        {
			Logger.LogError(">>> Entering SaveRedirectionRule(id={Id},redirectionId={RedirectionId},capbility={Capbility},expression={Expression})", id, redirectionId, capbility, expression);
            this.ExecuteNonQuery("Mobile_SaveRedirectionRule", id, redirectionId, capbility, expression);
			Logger.LogWarning("<<< Exiting SaveRedirectionRule(id={Id},redirectionId={RedirectionId},capbility={Capbility},expression={Expression})", id, redirectionId, capbility, expression);
        }

        public virtual void AddLog(string logGUID, string logTypeKey, int logUserID, string logUserName, int logPortalID, string logPortalName, DateTime logCreateDate, string logServerName, string logProperties, int logConfigID, ExceptionInfo exception, bool notificationActive)
        {
			Logger.LogError(">>> Entering AddLog(logGUID={LogGUID},logTypeKey={LogTypeKey},logUserID={LogUserID},logUserName={LogUserName},logPortalID={LogPortalID},logPortalName={LogPortalName},logCreateDate={LogCreateDate},logServerName={LogServerName},logProperties={LogProperties},logConfigID={LogConfigID},exception={Exception},notificationActive={NotificationActive})", logGUID, logTypeKey, logUserID, logUserName, logPortalID, logPortalName, logCreateDate, logServerName, logProperties, logConfigID, exception, notificationActive);
            if (exception != null)
            {
                if (!string.IsNullOrEmpty(exception.ExceptionHash))
                {
					var __logStart = System.Diagnostics.Stopwatch.StartNew();
                    this.ExecuteNonQuery(
                        "AddException",
                        exception.ExceptionHash,
                        exception.Message,
                        exception.StackTrace,
                        exception.InnerMessage,
                        exception.InnerStackTrace,
                        exception.Source);
						Logger.LogError("AddLog(logGUID,logTypeKey,logUserID,logUserName,logPortalID,logPortalName,logCreateDate,logServerName,logProperties,logConfigID,exception,notificationActive)={LogGUID},{LogTypeKey},{LogUserID},{LogUserName},{LogPortalID},{LogPortalName},{LogCreateDate},{LogServerName},{LogProperties},{LogConfigID},{Exception},{NotificationActive}: database query executed in {Arg12} ms", logGUID, logTypeKey, logUserID, logUserName, logPortalID, logPortalName, logCreateDate, logServerName, logProperties, logConfigID, exception, notificationActive, __logStart.ElapsedMilliseconds);
                }

                // DNN-6218 + DNN-6239 + DNN-6242: Due to change in the AddEventLog stored
                // procedure in 7.4.0, we need to try a fallback especially during upgrading
                int logEventID;
                try
                {
                    logEventID = this.ExecuteScalar<int>(
                        "AddEventLog",
                        logGUID,
                        logTypeKey,
                        this.GetNull(logUserID),
                        this.GetNull(logUserName),
                        this.GetNull(logPortalID),
                        this.GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID,
                        this.GetNull(exception.ExceptionHash),
                        notificationActive);
                }
                catch (SqlException)
                {
					Logger.LogError("Exception in AddLog(logGUID={LogGUID},logTypeKey={LogTypeKey},logUserID={LogUserID},logUserName={LogUserName},logPortalID={LogPortalID},logPortalName={LogPortalName},logCreateDate={LogCreateDate},logServerName={LogServerName},logProperties={LogProperties},logConfigID={LogConfigID},exception={Exception},notificationActive={NotificationActive}): {Arg12}", logGUID, logTypeKey, logUserID, logUserName, logPortalID, logPortalName, logCreateDate, logServerName, logProperties, logConfigID, exception, notificationActive, SqlException.Message);
                    var s = this.ExecuteScalar<string>(
                        "AddEventLog",
                        logGUID,
                        logTypeKey,
                        this.GetNull(logUserID),
                        this.GetNull(logUserName),
                        this.GetNull(logPortalID),
                        this.GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID);

                    // old SPROC wasn't returning anything; trying a workaround
                    if (!int.TryParse(s ?? "-1", out logEventID))
                    {
                        logEventID = Null.NullInteger;
						Logger.LogDebug("AddLog(logGUID={LogGUID},logTypeKey={LogTypeKey},logUserID={LogUserID},logUserName={LogUserName},logPortalID={LogPortalID},logPortalName={LogPortalName},logCreateDate={LogCreateDate},logServerName={LogServerName},logProperties={LogProperties},logConfigID={LogConfigID},exception={Exception},notificationActive={NotificationActive}): logEventID → {LogEventID}", logGUID, logTypeKey, logUserID, logUserName, logPortalID, logPortalName, logCreateDate, logServerName, logProperties, logConfigID, exception, notificationActive, logEventID);
                    }
                }

                if (!string.IsNullOrEmpty(exception.AssemblyVersion) && exception.AssemblyVersion != "-1")
                {
                    this.ExecuteNonQuery(
                        "AddExceptionEvent",
                        logEventID,
                        exception.AssemblyVersion,
                        exception.PortalId,
                        exception.UserId,
                        exception.TabId,
                        exception.RawUrl,
                        exception.Referrer,
                        exception.UserAgent);
                }
            }
            else
            {
                this.ExecuteScalar<int>(
                    "AddEventLog",
                    logGUID,
                    logTypeKey,
                    this.GetNull(logUserID),
                    this.GetNull(logUserName),
                    this.GetNull(logPortalID),
                    this.GetNull(logPortalName),
                    logCreateDate,
                    logServerName,
                    logProperties,
                    logConfigID,
                    DBNull.Value,
                    notificationActive);
            }
			Logger.LogWarning("<<< Exiting AddLog(logGUID={LogGUID},logTypeKey={LogTypeKey},logUserID={LogUserID},logUserName={LogUserName},logPortalID={LogPortalID},logPortalName={LogPortalName},logCreateDate={LogCreateDate},logServerName={LogServerName},logProperties={LogProperties},logConfigID={LogConfigID},exception={Exception},notificationActive={NotificationActive})", logGUID, logTypeKey, logUserID, logUserName, logPortalID, logPortalName, logCreateDate, logServerName, logProperties, logConfigID, exception, notificationActive);
        }

        public virtual void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
			Logger.LogError(">>> Entering AddLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "AddEventLogType",
                logTypeKey,
                logTypeFriendlyName,
                logTypeDescription,
                logTypeOwner,
                logTypeCSSClass);
				Logger.LogError("AddLogType(logTypeKey,logTypeFriendlyName,logTypeDescription,logTypeCSSClass,logTypeOwner)={LogTypeKey},{LogTypeFriendlyName},{LogTypeDescription},{LogTypeCSSClass},{LogTypeOwner}: database query executed in {Arg5} ms", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        public virtual void AddLogTypeConfigInfo(bool loggingIsActive, string logTypeKey, string logTypePortalID, int keepMostRecent, bool emailNotificationIsActive, int threshold, int notificationThresholdTime, int notificationThresholdTimeType, string mailFromAddress, string mailToAddress)
        {
			Logger.LogError(">>> Entering AddLogTypeConfigInfo(loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress);
            int portalID;
            if (logTypeKey == "*")
            {
                logTypeKey = string.Empty;
            }

            if (logTypePortalID == "*")
            {
                portalID = -1;
				Logger.LogError("AddLogTypeConfigInfo(loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): portalID → {PortalID}", loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, portalID);
            }
            else
            {
                portalID = Convert.ToInt32(logTypePortalID, CultureInfo.InvariantCulture);
				Logger.LogDebug("AddLogTypeConfigInfo(loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): portalID → {PortalID}", loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, portalID);
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "AddEventLogConfig",
                this.GetNull(logTypeKey),
                this.GetNull(portalID),
                loggingIsActive,
                keepMostRecent,
                emailNotificationIsActive,
                this.GetNull(threshold),
                this.GetNull(notificationThresholdTime),
                this.GetNull(notificationThresholdTimeType),
                mailFromAddress,
                mailToAddress);
				Logger.LogError("AddLogTypeConfigInfo(loggingIsActive,logTypeKey,logTypePortalID,keepMostRecent,emailNotificationIsActive,threshold,notificationThresholdTime,notificationThresholdTimeType,mailFromAddress,mailToAddress)={LoggingIsActive},{LogTypeKey},{LogTypePortalID},{KeepMostRecent},{EmailNotificationIsActive},{Threshold},{NotificationThresholdTime},{NotificationThresholdTimeType},{MailFromAddress},{MailToAddress}: database query executed in {Arg10} ms", loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddLogTypeConfigInfo(loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress);
        }

        public virtual void ClearLog()
        {
			Logger.LogError(">>> Entering ClearLog()");
            this.ExecuteNonQuery("DeleteEventLog", DBNull.Value);
			Logger.LogWarning("<<< Exiting ClearLog()");
        }

        public virtual void DeleteLog(string logGUID)
        {
			Logger.LogError(">>> Entering DeleteLog(logGUID={LogGUID})", logGUID);
            this.ExecuteNonQuery("DeleteEventLog", logGUID);
			Logger.LogWarning("<<< Exiting DeleteLog(logGUID={LogGUID})", logGUID);
        }

        public virtual void DeleteLogType(string logTypeKey)
        {
			Logger.LogError(">>> Entering DeleteLogType(logTypeKey={LogTypeKey})", logTypeKey);
            this.ExecuteNonQuery("DeleteEventLogType", logTypeKey);
			Logger.LogWarning("<<< Exiting DeleteLogType(logTypeKey={LogTypeKey})", logTypeKey);
        }

        public virtual void DeleteLogTypeConfigInfo(string id)
        {
			Logger.LogError(">>> Entering DeleteLogTypeConfigInfo(id={Id})", id);
            this.ExecuteNonQuery("DeleteEventLogConfig", id);
			Logger.LogWarning("<<< Exiting DeleteLogTypeConfigInfo(id={Id})", id);
        }

        public virtual IDataReader GetEventLogPendingNotif(int logConfigID)
        {
			Logger.LogError(">>> Entering GetEventLogPendingNotif(logConfigID={LogConfigID})", logConfigID);
            Logger.LogWarning("<<< Exiting GetEventLogPendingNotif(logConfigID={LogConfigID})", logConfigID);
            return this.ExecuteReader("GetEventLogPendingNotif", logConfigID);
        }

        public virtual IDataReader GetEventLogPendingNotifConfig()
        {
			Logger.LogError(">>> Entering GetEventLogPendingNotifConfig()");
            Logger.LogWarning("<<< Exiting GetEventLogPendingNotifConfig()");
            return this.ExecuteReader("GetEventLogPendingNotifConfig");
        }

        public virtual IDataReader GetLogs(int portalID, string logType, int pageSize, int pageIndex)
        {
			Logger.LogError(">>> Entering GetLogs(portalID={PortalID},logType={LogType},pageSize={PageSize},pageIndex={PageIndex})", portalID, logType, pageSize, pageIndex);
            Logger.LogWarning("<<< Exiting GetLogs(portalID={PortalID},logType={LogType},pageSize={PageSize},pageIndex={PageIndex})", portalID, logType, pageSize, pageIndex);
            return this.ExecuteReader("GetEventLog", this.GetNull(portalID), this.GetNull(logType), pageSize, pageIndex);
        }

        public virtual IDataReader GetLogTypeConfigInfo()
        {
			Logger.LogError(">>> Entering GetLogTypeConfigInfo()");
            Logger.LogWarning("<<< Exiting GetLogTypeConfigInfo()");
            return this.ExecuteReader("GetEventLogConfig", DBNull.Value);
        }

        public virtual IDataReader GetLogTypeConfigInfoByID(int id)
        {
			Logger.LogError(">>> Entering GetLogTypeConfigInfoByID(id={Id})", id);
            Logger.LogWarning("<<< Exiting GetLogTypeConfigInfoByID(id={Id})", id);
            return this.ExecuteReader("GetEventLogConfig", id);
        }

        public virtual IDataReader GetLogTypeInfo()
        {
			Logger.LogError(">>> Entering GetLogTypeInfo()");
            Logger.LogWarning("<<< Exiting GetLogTypeInfo()");
            return this.ExecuteReader("GetEventLogType");
        }

        public virtual IDataReader GetSingleLog(string logGUID)
        {
			Logger.LogError(">>> Entering GetSingleLog(logGUID={LogGUID})", logGUID);
            Logger.LogWarning("<<< Exiting GetSingleLog(logGUID={LogGUID})", logGUID);
            return this.ExecuteReader("GetEventLogByLogGUID", logGUID);
        }

        public virtual void PurgeLog()
        {
			Logger.LogError(">>> Entering PurgeLog()");
            // Because event log is run on application end, app may not be fully installed, so check for the sproc first
            string sql = "IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'" + this.DatabaseOwner + this.ObjectQualifier +
                         "PurgeEventLog') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) " + " BEGIN " +
                         "    EXEC " + this.DatabaseOwner + this.ObjectQualifier + "PurgeEventLog" + " END ";
						var __logStart = System.Diagnostics.Stopwatch.StartNew();
						Logger.LogError("PurgeLog(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting PurgeLog()");
            PetaPocoHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql);
        }

        public virtual void UpdateEventLogPendingNotif(int logConfigID)
        {
			Logger.LogError(">>> Entering UpdateEventLogPendingNotif(logConfigID={LogConfigID})", logConfigID);
            this.ExecuteNonQuery("UpdateEventLogPendingNotif", logConfigID);
			Logger.LogWarning("<<< Exiting UpdateEventLogPendingNotif(logConfigID={LogConfigID})", logConfigID);
        }

        public virtual void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
			Logger.LogError(">>> Entering UpdateLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateEventLogType",
                logTypeKey,
                logTypeFriendlyName,
                logTypeDescription,
                logTypeOwner,
                logTypeCSSClass);
				Logger.LogError("UpdateLogType(logTypeKey,logTypeFriendlyName,logTypeDescription,logTypeCSSClass,logTypeOwner)={LogTypeKey},{LogTypeFriendlyName},{LogTypeDescription},{LogTypeCSSClass},{LogTypeOwner}: database query executed in {Arg5} ms", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        public virtual void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, int keepMostRecent, bool emailNotificationIsActive, int threshold, int notificationThresholdTime, int notificationThresholdTimeType, string mailFromAddress, string mailToAddress)
        {
			Logger.LogError(">>> Entering UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress);
            if (logTypeKey == "*")
            {
                logTypeKey = string.Empty;
            }

            int portalId;
            if (logTypePortalID == "*")
            {
                portalId = -1;
				Logger.LogError("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): portalId → {PortalId}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, portalId);
            }
            else
            {
                portalId = Convert.ToInt32(logTypePortalID, CultureInfo.CurrentCulture);
				Logger.LogDebug("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): portalId → {PortalId}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, portalId);
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateEventLogConfig",
                id,
                this.GetNull(logTypeKey),
                this.GetNull(portalId),
                loggingIsActive,
                keepMostRecent,
                emailNotificationIsActive,
                this.GetNull(threshold),
                this.GetNull(notificationThresholdTime),
                this.GetNull(notificationThresholdTimeType),
                mailFromAddress,
                mailToAddress);
				Logger.LogError("UpdateLogTypeConfigInfo(id,loggingIsActive,logTypeKey,logTypePortalID,keepMostRecent,emailNotificationIsActive,threshold,notificationThresholdTime,notificationThresholdTimeType,mailFromAddress,mailToAddress)={Id},{LoggingIsActive},{LogTypeKey},{LogTypePortalID},{KeepMostRecent},{EmailNotificationIsActive},{Threshold},{NotificationThresholdTime},{NotificationThresholdTimeType},{MailFromAddress},{MailToAddress}: database query executed in {Arg11} ms", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},notificationThresholdTime={NotificationThresholdTime},notificationThresholdTimeType={NotificationThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, emailNotificationIsActive, threshold, notificationThresholdTime, notificationThresholdTimeType, mailFromAddress, mailToAddress);
        }

        /// <summary>Add a new scheduled task.</summary>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="createdByUserID">The ID of the user creating the task.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        /// <returns>The schedule ID.</returns>
        public virtual int AddSchedule(string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, int createdByUserID, string friendlyName, DateTime scheduleStartDate)
        {
			Logger.LogError(">>> Entering AddSchedule(typeFullName={TypeFullName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},servers={Servers},createdByUserID={CreatedByUserID},friendlyName={FriendlyName},scheduleStartDate={ScheduleStartDate})", typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, createdByUserID, friendlyName, scheduleStartDate);
			Logger.LogWarning("<<< Exiting AddSchedule(typeFullName={TypeFullName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},servers={Servers},createdByUserID={CreatedByUserID},friendlyName={FriendlyName},scheduleStartDate={ScheduleStartDate})", typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, createdByUserID, friendlyName, scheduleStartDate);
            return this.ExecuteScalar<int>(
                "AddSchedule",
                typeFullName,
                timeLapse,
                timeLapseMeasurement,
                retryTimeLapse,
                retryTimeLapseMeasurement,
                retainHistoryNum,
                attachToEvent,
                catchUpEnabled,
                enabled,
                objectDependencies,
                this.GetNull(servers),
                createdByUserID,
                friendlyName,
                this.GetNull(scheduleStartDate));
        }

        public virtual int AddScheduleHistory(int scheduleID, DateTime startDate, string server)
        {
			Logger.LogError(">>> Entering AddScheduleHistory(scheduleID={ScheduleID},startDate={StartDate},server={Server})", scheduleID, startDate, server);
            Logger.LogWarning("<<< Exiting AddScheduleHistory(scheduleID={ScheduleID},startDate={StartDate},server={Server})", scheduleID, startDate, server);
            return this.ExecuteScalar<int>("AddScheduleHistory", scheduleID, FixDate(startDate), server);
        }

        public virtual void AddScheduleItemSetting(int scheduleID, string name, string value)
        {
			Logger.LogError(">>> Entering AddScheduleItemSetting(scheduleID={ScheduleID},name={Name},value={Value})", scheduleID, name, value);
            this.ExecuteNonQuery("AddScheduleItemSetting", scheduleID, name, value);
			Logger.LogWarning("<<< Exiting AddScheduleItemSetting(scheduleID={ScheduleID},name={Name},value={Value})", scheduleID, name, value);
        }

        public virtual void DeleteSchedule(int scheduleID)
        {
			Logger.LogError(">>> Entering DeleteSchedule(scheduleID={ScheduleID})", scheduleID);
            this.ExecuteNonQuery("DeleteSchedule", scheduleID);
			Logger.LogWarning("<<< Exiting DeleteSchedule(scheduleID={ScheduleID})", scheduleID);
        }

        public virtual IDataReader GetNextScheduledTask(string server)
        {
			Logger.LogError(">>> Entering GetNextScheduledTask(server={Server})", server);
            Logger.LogWarning("<<< Exiting GetNextScheduledTask(server={Server})", server);
            return this.ExecuteReader("GetScheduleNextTask", this.GetNull(server));
        }

        public virtual IDataReader GetSchedule()
        {
			Logger.LogError(">>> Entering GetSchedule()");
            Logger.LogWarning("<<< Exiting GetSchedule()");
            return this.ExecuteReader("GetSchedule", DBNull.Value);
        }

        public virtual IDataReader GetSchedule(string server)
        {
            return this.ExecuteReader("GetSchedule", this.GetNull(server));
        }

        public virtual IDataReader GetSchedule(int scheduleID)
        {
            return this.ExecuteReader("GetScheduleByScheduleID", scheduleID);
        }

        public virtual IDataReader GetSchedule(string typeFullName, string server)
        {
            return this.ExecuteReader("GetScheduleByTypeFullName", typeFullName, this.GetNull(server));
        }

        public virtual IDataReader GetScheduleByEvent(string eventName, string server)
        {
			Logger.LogError(">>> Entering GetScheduleByEvent(eventName={EventName},server={Server})", eventName, server);
            Logger.LogWarning("<<< Exiting GetScheduleByEvent(eventName={EventName},server={Server})", eventName, server);
            return this.ExecuteReader("GetScheduleByEvent", eventName, this.GetNull(server));
        }

        public virtual IDataReader GetScheduleHistory(int scheduleID)
        {
			Logger.LogError(">>> Entering GetScheduleHistory(scheduleID={ScheduleID})", scheduleID);
            Logger.LogWarning("<<< Exiting GetScheduleHistory(scheduleID={ScheduleID})", scheduleID);
            return this.ExecuteReader("GetScheduleHistory", scheduleID);
        }

        public virtual IDataReader GetScheduleItemSettings(int scheduleID)
        {
			Logger.LogError(">>> Entering GetScheduleItemSettings(scheduleID={ScheduleID})", scheduleID);
            Logger.LogWarning("<<< Exiting GetScheduleItemSettings(scheduleID={ScheduleID})", scheduleID);
            return this.ExecuteReader("GetScheduleItemSettings", scheduleID);
        }

        public virtual void PurgeScheduleHistory()
        {
			Logger.LogError(">>> Entering PurgeScheduleHistory()");
            this.ExecuteNonQuery(90, "PurgeScheduleHistory");
			Logger.LogWarning("<<< Exiting PurgeScheduleHistory()");
        }

        /// <summary>Updates the scheduled task.</summary>
        /// <param name="scheduleID">The ID of the scheduled task.</param>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="lastModifiedByUserID">The ID of the user modifying the task.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        public virtual void UpdateSchedule(int scheduleID, string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, int lastModifiedByUserID, string friendlyName, DateTime scheduleStartDate)
        {
			Logger.LogError(">>> Entering UpdateSchedule(scheduleID={ScheduleID},typeFullName={TypeFullName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},servers={Servers},lastModifiedByUserID={LastModifiedByUserID},friendlyName={FriendlyName},scheduleStartDate={ScheduleStartDate})", scheduleID, typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, lastModifiedByUserID, friendlyName, scheduleStartDate);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateSchedule",
                scheduleID,
                typeFullName,
                timeLapse,
                timeLapseMeasurement,
                retryTimeLapse,
                retryTimeLapseMeasurement,
                retainHistoryNum,
                attachToEvent,
                catchUpEnabled,
                enabled,
                objectDependencies,
                this.GetNull(servers),
                lastModifiedByUserID,
                friendlyName,
                this.GetNull(scheduleStartDate));
				Logger.LogError("UpdateSchedule(scheduleID,typeFullName,timeLapse,timeLapseMeasurement,retryTimeLapse,retryTimeLapseMeasurement,retainHistoryNum,attachToEvent,catchUpEnabled,enabled,objectDependencies,servers,lastModifiedByUserID,friendlyName,scheduleStartDate)={ScheduleID},{TypeFullName},{TimeLapse},{TimeLapseMeasurement},{RetryTimeLapse},{RetryTimeLapseMeasurement},{RetainHistoryNum},{AttachToEvent},{CatchUpEnabled},{Enabled},{ObjectDependencies},{Servers},{LastModifiedByUserID},{FriendlyName},{ScheduleStartDate}: database query executed in {Arg15} ms", scheduleID, typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, lastModifiedByUserID, friendlyName, scheduleStartDate, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateSchedule(scheduleID={ScheduleID},typeFullName={TypeFullName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},servers={Servers},lastModifiedByUserID={LastModifiedByUserID},friendlyName={FriendlyName},scheduleStartDate={ScheduleStartDate})", scheduleID, typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, lastModifiedByUserID, friendlyName, scheduleStartDate);
        }

        public virtual void UpdateScheduleHistory(int scheduleHistoryID, DateTime endDate, bool succeeded, string logNotes, DateTime nextStart)
        {
			Logger.LogError(">>> Entering UpdateScheduleHistory(scheduleHistoryID={ScheduleHistoryID},endDate={EndDate},succeeded={Succeeded},logNotes={LogNotes},nextStart={NextStart})", scheduleHistoryID, endDate, succeeded, logNotes, nextStart);
            this.ExecuteNonQuery("UpdateScheduleHistory", scheduleHistoryID, FixDate(endDate), this.GetNull(succeeded), logNotes, FixDate(nextStart));
			Logger.LogWarning("<<< Exiting UpdateScheduleHistory(scheduleHistoryID={ScheduleHistoryID},endDate={EndDate},succeeded={Succeeded},logNotes={LogNotes},nextStart={NextStart})", scheduleHistoryID, endDate, succeeded, logNotes, nextStart);
        }

        public virtual int AddExtensionUrlProvider(
            int providerId,
            int desktopModuleId,
            string providerName,
            string providerType,
            string settingsControlSrc,
            bool isActive,
            bool rewriteAllUrls,
            bool redirectAllUrls,
            bool replaceAllUrls)
        {
			Logger.LogError(">>> Entering AddExtensionUrlProvider(providerId=<redacted>,desktopModuleId={DesktopModuleId},providerName={ProviderName},providerType={ProviderType},settingsControlSrc={SettingsControlSrc},isActive={IsActive},rewriteAllUrls={RewriteAllUrls},redirectAllUrls={RedirectAllUrls},replaceAllUrls={ReplaceAllUrls})", desktopModuleId, providerName, providerType, settingsControlSrc, isActive, rewriteAllUrls, redirectAllUrls, replaceAllUrls);
            Logger.LogWarning("<<< Exiting AddExtensionUrlProvider(providerId=<redacted>,desktopModuleId={DesktopModuleId},providerName={ProviderName},providerType={ProviderType},settingsControlSrc={SettingsControlSrc},isActive={IsActive},rewriteAllUrls={RewriteAllUrls},redirectAllUrls={RedirectAllUrls},replaceAllUrls={ReplaceAllUrls})", desktopModuleId, providerName, providerType, settingsControlSrc, isActive, rewriteAllUrls, redirectAllUrls, replaceAllUrls);
            return this.ExecuteScalar<int>(
                "AddExtensionUrlProvider",
                providerId,
                desktopModuleId,
                providerName,
                providerType,
                settingsControlSrc,
                isActive,
                rewriteAllUrls,
                redirectAllUrls,
                replaceAllUrls);
        }

        public virtual void DeleteExtensionUrlProvider(int providerId)
        {
			Logger.LogError(">>> Entering DeleteExtensionUrlProvider(providerId=<redacted>)");
            this.ExecuteNonQuery("DeleteExtensionUrlProvider", providerId);
			Logger.LogWarning("<<< Exiting DeleteExtensionUrlProvider(providerId=<redacted>)");
        }

        public virtual IDataReader GetExtensionUrlProviders(int portalId)
        {
			Logger.LogError(">>> Entering GetExtensionUrlProviders(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetExtensionUrlProviders(portalId={PortalId})", portalId);
            return this.ExecuteReader("GetExtensionUrlProviders", this.GetNull(portalId));
        }

        public virtual void SaveExtensionUrlProviderSetting(int providerId, int portalId, string settingName, string settingValue)
        {
			Logger.LogError(">>> Entering SaveExtensionUrlProviderSetting(providerId=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalId, settingName, settingValue);
            this.ExecuteNonQuery("SaveExtensionUrlProviderSetting", providerId, portalId, settingName, settingValue);
			Logger.LogWarning("<<< Exiting SaveExtensionUrlProviderSetting(providerId=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalId, settingName, settingValue);
        }

        public virtual void UpdateExtensionUrlProvider(int providerId, bool isActive)
        {
			Logger.LogError(">>> Entering UpdateExtensionUrlProvider(providerId=<redacted>,isActive={IsActive})", isActive);
            this.ExecuteNonQuery("UpdateExtensionUrlProvider", providerId, isActive);
			Logger.LogWarning("<<< Exiting UpdateExtensionUrlProvider(providerId=<redacted>,isActive={IsActive})", isActive);
        }

        public virtual IDataReader GetIPFilters()
        {
			Logger.LogError(">>> Entering GetIPFilters()");
            Logger.LogWarning("<<< Exiting GetIPFilters()");
            return this.ExecuteReader("GetIPFilters");
        }

        /// <summary>Adds a new IP filter.</summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="subnetMask">The subnet mask if the filter is for a range.</param>
        /// <param name="ruleType">The type of rule (1 for Allow, 2 for Deny).</param>
        /// <param name="createdByUserId">The ID of the acting user.</param>
        /// <returns>The ID of the newly created IP Filter.</returns>
        [DnnDeprecated(9, 11, 1, "Use the overload that takes a notes string")]
        public virtual partial int AddIPFilter(string ipAddress, string subnetMask, int ruleType, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddIPFilter(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},createdByUserId={CreatedByUserId})", ipAddress, subnetMask, ruleType, createdByUserId);
            Logger.LogWarning("<<< Exiting AddIPFilter(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},createdByUserId={CreatedByUserId})", ipAddress, subnetMask, ruleType, createdByUserId);
            return this.AddIPFilter(ipAddress, subnetMask, ruleType, createdByUserId, null);
        }

        /// <summary>Adds a new IP filter.</summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="subnetMask">The subnet mask if the filter is for a range.</param>
        /// <param name="ruleType">The type of rule (1 for Allow, 2 for Deny).</param>
        /// <param name="createdByUserId">The ID of the acting user.</param>
        /// <param name="notes">Notes about this filter.</param>
        /// <returns>The ID of the newly created IP Filter.</returns>
        public virtual int AddIPFilter(string ipAddress, string subnetMask, int ruleType, int createdByUserId, string notes)
        {
            return this.ExecuteScalar<int>("AddIPFilter", ipAddress, subnetMask, ruleType, createdByUserId, notes);
        }

        public virtual void DeleteIPFilter(int ipFilterid)
        {
			Logger.LogError(">>> Entering DeleteIPFilter(ipFilterid={IpFilterid})", ipFilterid);
            this.ExecuteNonQuery("DeleteIPFilter", ipFilterid);
			Logger.LogWarning("<<< Exiting DeleteIPFilter(ipFilterid={IpFilterid})", ipFilterid);
        }

        /// <summary>Updates an existing IP filter.</summary>
        /// <param name="ipFilterid">The ID of the IP Filter to update.</param>
        /// <param name="ipAddress">The IP address for the filter.</param>
        /// <param name="subnetMask">The IP mask to use for an IP range.</param>
        /// <param name="ruleType">The type of filter (1 to allow, 2 to deny).</param>
        /// <param name="lastModifiedByUserId">The ID of the acting user.</param>
        [DnnDeprecated(9, 11, 1, "Use the overload that takes a notes string")]
        public virtual partial void UpdateIPFilter(int ipFilterid, string ipAddress, string subnetMask, int ruleType, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateIPFilter(ipFilterid={IpFilterid},ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},lastModifiedByUserId={LastModifiedByUserId})", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId);
            this.UpdateIPFilter(ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId, null);
			Logger.LogWarning("<<< Exiting UpdateIPFilter(ipFilterid={IpFilterid},ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},lastModifiedByUserId={LastModifiedByUserId})", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId);
        }

        /// <summary>Updates an existing IP filter.</summary>
        /// <param name="ipFilterid">The ID of the IP Filter to update.</param>
        /// <param name="ipAddress">The IP address for the filter.</param>
        /// <param name="subnetMask">The IP mask to use for an IP range.</param>
        /// <param name="ruleType">The type of filter (1 to allow, 2 to deny).</param>
        /// <param name="lastModifiedByUserId">The ID of the acting user.</param>
        /// <param name="notes">Notes about the filter.</param>
        public virtual void UpdateIPFilter(int ipFilterid, string ipAddress, string subnetMask, int ruleType, int lastModifiedByUserId, string notes)
        {
			Logger.LogError(">>> Entering UpdateIPFilter(ipFilterid={IpFilterid},ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},lastModifiedByUserId={LastModifiedByUserId},notes={Notes})", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId, notes);
            this.ExecuteNonQuery("UpdateIPFilter", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId, notes);
			Logger.LogWarning("<<< Exiting UpdateIPFilter(ipFilterid={IpFilterid},ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType},lastModifiedByUserId={LastModifiedByUserId},notes={Notes})", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId, notes);
        }

        public virtual IDataReader GetIPFilter(int ipf)
        {
			Logger.LogError(">>> Entering GetIPFilter(ipf={Ipf})", ipf);
            Logger.LogWarning("<<< Exiting GetIPFilter(ipf={Ipf})", ipf);
            return this.ExecuteReader("GetIPFilter", ipf);
        }

        public virtual IDataReader GetFileVersions(int fileId)
        {
			Logger.LogError(">>> Entering GetFileVersions(fileId={FileId})", fileId);
            Logger.LogWarning("<<< Exiting GetFileVersions(fileId={FileId})", fileId);
            return this.ExecuteReader("GetFileVersions", fileId);
        }

        public virtual IDataReader GetFileVersionsInFolder(int folderId)
        {
			Logger.LogError(">>> Entering GetFileVersionsInFolder(folderId={FolderId})", folderId);
            Logger.LogWarning("<<< Exiting GetFileVersionsInFolder(folderId={FolderId})", folderId);
            return this.ExecuteReader("GetFileVersionsInFolder", folderId);
        }

        public virtual int AddFileVersion(int fileId, Guid uniqueId, Guid versionGuid, string fileName, string extension, long size, int width, int height, string contentType, string folder, int folderId, int userId, string hash, DateTime lastModificationTime, string title, bool enablePublishPeriod, DateTime startDate, DateTime endDate, int contentItemID, bool published, byte[] content = null)
        {
			Logger.LogError(">>> Entering AddFileVersion(fileId={FileId},uniqueId={UniqueId},versionGuid={VersionGuid},fileName={FileName},extension={Extension},size={Size},width={Width},height={Height},contentType={ContentType},folder={Folder},folderId={FolderId},userId={UserId},hash=<redacted>,lastModificationTime={LastModificationTime},title={Title},enablePublishPeriod={EnablePublishPeriod},startDate={StartDate},endDate={EndDate},contentItemID={ContentItemID},published={Published},content={Content})", fileId, uniqueId, versionGuid, fileName, extension, size, width, height, contentType, folder, folderId, userId, lastModificationTime, title, enablePublishPeriod, startDate, endDate, contentItemID, published, content);
            if (content == null)
            {
                return this.ExecuteScalar<int>(
                    "AddFileVersion",
                    fileId,
                    uniqueId,
                    versionGuid,
                    fileName,
                    extension,
                    size,
                    this.GetNull(width),
                    this.GetNull(height),
                    contentType,
                    folder,
                    folderId,
                    userId,
                    hash,
                    lastModificationTime,
                    title,
                    enablePublishPeriod,
                    startDate,
                    this.GetNull(endDate),
                    this.GetNull(contentItemID),
                    published);
            }

            return this.ExecuteScalar<int>(
                "AddFileVersion",
                fileId,
                uniqueId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folder,
                folderId,
                userId,
                hash,
                lastModificationTime,
                title,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemID),
                published,
                this.GetNull(content));
        }

        public virtual int DeleteFileVersion(int fileId, int version)
        {
			Logger.LogError(">>> Entering DeleteFileVersion(fileId={FileId},version={Version})", fileId, version);
            Logger.LogWarning("<<< Exiting DeleteFileVersion(fileId={FileId},version={Version})", fileId, version);
            return this.ExecuteScalar<int>("DeleteFileVersion", fileId, version);
        }

        public virtual void ResetFilePublishedVersion(int fileId)
        {
			Logger.LogError(">>> Entering ResetFilePublishedVersion(fileId={FileId})", fileId);
            this.ExecuteNonQuery("ResetFilePublishedVersion", fileId);
			Logger.LogWarning("<<< Exiting ResetFilePublishedVersion(fileId={FileId})", fileId);
        }

        public virtual IDataReader GetFileVersion(int fileId, int version)
        {
			Logger.LogError(">>> Entering GetFileVersion(fileId={FileId},version={Version})", fileId, version);
            Logger.LogWarning("<<< Exiting GetFileVersion(fileId={FileId},version={Version})", fileId, version);
            return this.ExecuteReader("GetFileVersion", fileId, version);
        }

        public void SetPublishedVersion(int fileId, int newPublishedVersion)
        {
			Logger.LogError(">>> Entering SetPublishedVersion(fileId={FileId},newPublishedVersion={NewPublishedVersion})", fileId, newPublishedVersion);
            this.ExecuteNonQuery("SetPublishedVersion", fileId, newPublishedVersion);
			Logger.LogWarning("<<< Exiting SetPublishedVersion(fileId={FileId},newPublishedVersion={NewPublishedVersion})", fileId, newPublishedVersion);
        }

        public virtual int GetContentWorkflowUsageCount(int workflowId)
        {
			Logger.LogError(">>> Entering GetContentWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
            Logger.LogWarning("<<< Exiting GetContentWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
            return this.ExecuteScalar<int>("GetContentWorkflowUsageCount", workflowId);
        }

        public virtual IDataReader GetContentWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetContentWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
            Logger.LogWarning("<<< Exiting GetContentWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
            return this.ExecuteReader("GetContentWorkflowUsage", workflowId, pageIndex, pageSize);
        }

        public virtual int GetContentWorkflowStateUsageCount(int stateId)
        {
			Logger.LogError(">>> Entering GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
            return this.ExecuteScalar<int>("GetContentWorkflowStateUsageCount", stateId);
        }

        public virtual int AddContentWorkflowStatePermission(int stateId, int permissionId, int roleId, bool allowAccess, int userId, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddContentWorkflowStatePermission(stateId={StateId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", stateId, permissionId, roleId, allowAccess, userId, createdByUserId);
            Logger.LogWarning("<<< Exiting AddContentWorkflowStatePermission(stateId={StateId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},createdByUserId={CreatedByUserId})", stateId, permissionId, roleId, allowAccess, userId, createdByUserId);
            return this.ExecuteScalar<int>(
                "AddContentWorkflowStatePermission",
                stateId,
                permissionId,
                this.GetNull(roleId),
                allowAccess,
                this.GetNull(userId),
                this.GetNull(createdByUserId));
        }

        public virtual void UpdateContentWorkflowStatePermission(int workflowStatePermissionId, int stateId, int permissionId, int roleId, bool allowAccess, int userId, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId},stateId={StateId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserId={LastModifiedByUserId})", workflowStatePermissionId, stateId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.ExecuteNonQuery(
                "UpdateContentWorkflowStatePermission",
                workflowStatePermissionId,
                stateId,
                permissionId,
                this.GetNull(roleId),
                allowAccess,
                this.GetNull(userId),
                this.GetNull(lastModifiedByUserId));
				Logger.LogError("UpdateContentWorkflowStatePermission(workflowStatePermissionId,stateId,permissionId,roleId,allowAccess,userId,lastModifiedByUserId)={WorkflowStatePermissionId},{StateId},{PermissionId},{RoleId},{AllowAccess},{UserId},{LastModifiedByUserId}: database query executed in {Arg7} ms", workflowStatePermissionId, stateId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId},stateId={StateId},permissionId={PermissionId},roleId={RoleId},allowAccess={AllowAccess},userId={UserId},lastModifiedByUserId={LastModifiedByUserId})", workflowStatePermissionId, stateId, permissionId, roleId, allowAccess, userId, lastModifiedByUserId);
        }

        public virtual void DeleteContentWorkflowStatePermission(int workflowStatePermissionId)
        {
			Logger.LogError(">>> Entering DeleteContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            this.ExecuteNonQuery("DeleteContentWorkflowStatePermission", workflowStatePermissionId);
			Logger.LogWarning("<<< Exiting DeleteContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
        }

        public virtual IDataReader GetContentWorkflowStatePermission(int workflowStatePermissionId)
        {
			Logger.LogError(">>> Entering GetContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            Logger.LogWarning("<<< Exiting GetContentWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            return this.ExecuteReader("GetContentWorkflowStatePermission", workflowStatePermissionId);
        }

        public virtual IDataReader GetContentWorkflowStatePermissions()
        {
			Logger.LogError(">>> Entering GetContentWorkflowStatePermissions()");
            Logger.LogWarning("<<< Exiting GetContentWorkflowStatePermissions()");
            return this.ExecuteReader("GetContentWorkflowStatePermissions");
        }

        public virtual IDataReader GetContentWorkflowStatePermissionsByStateID(int stateId)
        {
			Logger.LogError(">>> Entering GetContentWorkflowStatePermissionsByStateID(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetContentWorkflowStatePermissionsByStateID(stateId={StateId})", stateId);
            return this.ExecuteReader("GetContentWorkflowStatePermissionsByStateID", stateId);
        }

        public virtual IDataReader GetAllSearchTypes()
        {
			Logger.LogError(">>> Entering GetAllSearchTypes()");
            Logger.LogWarning("<<< Exiting GetAllSearchTypes()");
            return this.ExecuteReader("SearchTypes_GetAll");
        }

        public virtual IDataReader GetAllSynonymsGroups(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetAllSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            Logger.LogWarning("<<< Exiting GetAllSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.ExecuteReader("GetAllSynonymsGroups", portalId, cultureCode);
        }

        public virtual int AddSynonymsGroup(string synonymsTags, int createdByUserId, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering AddSynonymsGroup(synonymsTags={SynonymsTags},createdByUserId={CreatedByUserId},portalId={PortalId},cultureCode={CultureCode})", synonymsTags, createdByUserId, portalId, cultureCode);
            Logger.LogWarning("<<< Exiting AddSynonymsGroup(synonymsTags={SynonymsTags},createdByUserId={CreatedByUserId},portalId={PortalId},cultureCode={CultureCode})", synonymsTags, createdByUserId, portalId, cultureCode);
            return this.ExecuteScalar<int>(
                "AddSynonymsGroup",
                synonymsTags,
                createdByUserId,
                portalId,
                cultureCode);
        }

        public virtual void UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int lastModifiedUserId)
        {
			Logger.LogError(">>> Entering UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},lastModifiedUserId={LastModifiedUserId})", synonymsGroupId, synonymsTags, lastModifiedUserId);
            this.ExecuteNonQuery("UpdateSynonymsGroup", synonymsGroupId, synonymsTags, lastModifiedUserId);
			Logger.LogWarning("<<< Exiting UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},lastModifiedUserId={LastModifiedUserId})", synonymsGroupId, synonymsTags, lastModifiedUserId);
        }

        public virtual void DeleteSynonymsGroup(int synonymsGroupId)
        {
			Logger.LogError(">>> Entering DeleteSynonymsGroup(synonymsGroupId={SynonymsGroupId})", synonymsGroupId);
            this.ExecuteNonQuery("DeleteSynonymsGroup", synonymsGroupId);
			Logger.LogWarning("<<< Exiting DeleteSynonymsGroup(synonymsGroupId={SynonymsGroupId})", synonymsGroupId);
        }

        public virtual IDataReader GetSearchStopWords(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            Logger.LogWarning("<<< Exiting GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.ExecuteReader("GetSearchStopWords", portalId, cultureCode);
        }

        public virtual int AddSearchStopWords(string stopWords, int createdByUserId, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering AddSearchStopWords(stopWords={StopWords},createdByUserId={CreatedByUserId},portalId={PortalId},cultureCode={CultureCode})", stopWords, createdByUserId, portalId, cultureCode);
            Logger.LogWarning("<<< Exiting AddSearchStopWords(stopWords={StopWords},createdByUserId={CreatedByUserId},portalId={PortalId},cultureCode={CultureCode})", stopWords, createdByUserId, portalId, cultureCode);
            return this.ExecuteScalar<int>("InsertSearchStopWords", stopWords, createdByUserId, portalId, cultureCode);
        }

        public virtual void UpdateSearchStopWords(int stopWordsId, string stopWords, int lastModifiedUserId)
        {
			Logger.LogError(">>> Entering UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},lastModifiedUserId={LastModifiedUserId})", stopWordsId, stopWords, lastModifiedUserId);
            this.ExecuteNonQuery("UpdateSearchStopWords", stopWordsId, stopWords, lastModifiedUserId);
			Logger.LogWarning("<<< Exiting UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},lastModifiedUserId={LastModifiedUserId})", stopWordsId, stopWords, lastModifiedUserId);
        }

        public virtual void DeleteSearchStopWords(int stopWordsId)
        {
			Logger.LogError(">>> Entering DeleteSearchStopWords(stopWordsId={StopWordsId})", stopWordsId);
            this.ExecuteNonQuery("DeleteSearchStopWords", stopWordsId);
			Logger.LogWarning("<<< Exiting DeleteSearchStopWords(stopWordsId={StopWordsId})", stopWordsId);
        }

        public void AddSearchDeletedItems(SearchDocumentToDelete deletedIDocument)
        {
			Logger.LogError(">>> Entering AddSearchDeletedItems(deletedIDocument={DeletedIDocument})", deletedIDocument);
            try
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("AddSearchDeletedItems(deletedIDocument)={DeletedIDocument}: database query executed in {Arg1} ms", deletedIDocument, __logStart.ElapsedMilliseconds);
                this.ExecuteNonQuery("SearchDeletedItems_Add", deletedIDocument.ToJsonString());
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting AddSearchDeletedItems(deletedIDocument={DeletedIDocument})", deletedIDocument);
        }

        public void DeleteProcessedSearchDeletedItems(DateTime cutoffTime)
        {
			Logger.LogError(">>> Entering DeleteProcessedSearchDeletedItems(cutoffTime={CutoffTime})", cutoffTime);
            try
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("DeleteProcessedSearchDeletedItems(cutoffTime)={CutoffTime}: database query executed in {Arg1} ms", cutoffTime, __logStart.ElapsedMilliseconds);
                this.ExecuteNonQuery("SearchDeletedItems_DeleteProcessed", cutoffTime);
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteProcessedSearchDeletedItems(cutoffTime={CutoffTime})", cutoffTime);
        }

        public IDataReader GetSearchDeletedItems(DateTime cutoffTime)
        {
			Logger.LogError(">>> Entering GetSearchDeletedItems(cutoffTime={CutoffTime})", cutoffTime);
            Logger.LogWarning("<<< Exiting GetSearchDeletedItems(cutoffTime={CutoffTime})", cutoffTime);
            return this.ExecuteReader("SearchDeletedItems_Select", cutoffTime);
        }

        public virtual IDataReader GetAvailableUsersForIndex(int portalId, DateTime startDate, int startUserId, int numberOfUsers)
        {
			Logger.LogError(">>> Entering GetAvailableUsersForIndex(portalId={PortalId},startDate={StartDate},startUserId={StartUserId},numberOfUsers={NumberOfUsers})", portalId, startDate, startUserId, numberOfUsers);
            Logger.LogWarning("<<< Exiting GetAvailableUsersForIndex(portalId={PortalId},startDate={StartDate},startUserId={StartUserId},numberOfUsers={NumberOfUsers})", portalId, startDate, startUserId, numberOfUsers);
            return this.ExecuteReader(90, "GetAvailableUsersForIndex", portalId, startDate, startUserId, numberOfUsers);
        }

        public virtual void AddOutputCacheItem(int itemId, string cacheKey, string output, DateTime expiration)
        {
			Logger.LogError(">>> Entering AddOutputCacheItem(itemId={ItemId},cacheKey={CacheKey},output={Output},expiration={Expiration})", itemId, cacheKey, output, expiration);
            Instance().ExecuteNonQuery("OutputCacheAddItem", itemId, cacheKey, output, expiration);
			Logger.LogWarning("<<< Exiting AddOutputCacheItem(itemId={ItemId},cacheKey={CacheKey},output={Output},expiration={Expiration})", itemId, cacheKey, output, expiration);
        }

        public virtual IDataReader GetOutputCacheItem(string cacheKey)
        {
			Logger.LogError(">>> Entering GetOutputCacheItem(cacheKey={CacheKey})", cacheKey);
            Logger.LogWarning("<<< Exiting GetOutputCacheItem(cacheKey={CacheKey})", cacheKey);
            return Instance().ExecuteReader("OutputCacheGetItem", cacheKey);
        }

        public virtual int GetOutputCacheItemCount(int itemId)
        {
			Logger.LogError(">>> Entering GetOutputCacheItemCount(itemId={ItemId})", itemId);
            Logger.LogWarning("<<< Exiting GetOutputCacheItemCount(itemId={ItemId})", itemId);
            return Instance().ExecuteScalar<int>("OutputCacheGetItemCount", itemId);
        }

        public virtual IDataReader GetOutputCacheKeys()
        {
			Logger.LogError(">>> Entering GetOutputCacheKeys()");
            Logger.LogWarning("<<< Exiting GetOutputCacheKeys()");
            return Instance().ExecuteReader("OutputCacheGetKeys", DBNull.Value);
        }

        public virtual IDataReader GetOutputCacheKeys(int itemId)
        {
            return Instance().ExecuteReader("OutputCacheGetKeys", itemId);
        }

        public virtual void PurgeExpiredOutputCacheItems()
        {
			Logger.LogError(">>> Entering PurgeExpiredOutputCacheItems()");
            Instance().ExecuteNonQuery("OutputCachePurgeExpiredItems", DateTime.UtcNow);
			Logger.LogWarning("<<< Exiting PurgeExpiredOutputCacheItems()");
        }

        public virtual void PurgeOutputCache()
        {
			Logger.LogError(">>> Entering PurgeOutputCache()");
            Instance().ExecuteNonQuery("OutputCachePurgeCache");
			Logger.LogWarning("<<< Exiting PurgeOutputCache()");
        }

        public virtual void RemoveOutputCacheItem(int itemId)
        {
			Logger.LogError(">>> Entering RemoveOutputCacheItem(itemId={ItemId})", itemId);
            Instance().ExecuteNonQuery("OutputCacheRemoveItem", itemId);
			Logger.LogWarning("<<< Exiting RemoveOutputCacheItem(itemId={ItemId})", itemId);
        }

        public virtual Guid AddRedirectMessage(int userId, int tabId, string text)
        {
			Logger.LogError(">>> Entering AddRedirectMessage(userId={UserId},tabId={TabId},text={Text})", userId, tabId, text);
            Logger.LogWarning("<<< Exiting AddRedirectMessage(userId={UserId},tabId={TabId},text={Text})", userId, tabId, text);
            return this.ExecuteScalar<Guid>("AddRedirectMessage", userId, tabId, text);
        }

        public virtual string GetRedirectMessage(Guid id)
        {
			Logger.LogError(">>> Entering GetRedirectMessage(id={Id})", id);
            Logger.LogWarning("<<< Exiting GetRedirectMessage(id={Id})", id);
            return this.ExecuteScalar<string>("GetRedirectMessage", id);
        }

        public virtual void DeleteOldRedirectMessage(DateTime cutofDateTime)
        {
			Logger.LogError(">>> Entering DeleteOldRedirectMessage(cutofDateTime={CutofDateTime})", cutofDateTime);
            this.ExecuteNonQuery("DeleteOldRedirectMessage", FixDate(cutofDateTime));
			Logger.LogWarning("<<< Exiting DeleteOldRedirectMessage(cutofDateTime={CutofDateTime})", cutofDateTime);
        }

        public virtual void UpdateAuthCookie(string cookieValue, DateTime utcExpiry, int userId)
        {
			Logger.LogError(">>> Entering UpdateAuthCookie(cookieValue=<redacted>,utcExpiry={UtcExpiry},userId={UserId})", utcExpiry, userId);
            this.ExecuteNonQuery("AuthCookies_Update", cookieValue, FixDate(utcExpiry), userId);
			Logger.LogWarning("<<< Exiting UpdateAuthCookie(cookieValue=<redacted>,utcExpiry={UtcExpiry},userId={UserId})", utcExpiry, userId);
        }

        public virtual IDataReader FindAuthCookie(string cookieValue)
        {
			Logger.LogError(">>> Entering FindAuthCookie(cookieValue=<redacted>)");
            Logger.LogWarning("<<< Exiting FindAuthCookie(cookieValue=<redacted>)");
            return this.ExecuteReader("AuthCookies_Find", cookieValue);
        }

        public virtual void DeleteAuthCookie(string cookieValue)
        {
			Logger.LogError(">>> Entering DeleteAuthCookie(cookieValue=<redacted>)");
            this.ExecuteNonQuery("AuthCookies_DeleteByValue", cookieValue);
			Logger.LogWarning("<<< Exiting DeleteAuthCookie(cookieValue=<redacted>)");
        }

        public virtual void DeleteExpiredAuthCookies(DateTime utcExpiredBefore)
        {
			Logger.LogError(">>> Entering DeleteExpiredAuthCookies(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
            this.ExecuteNonQuery("AuthCookies_DeleteOld", FixDate(utcExpiredBefore));
			Logger.LogWarning("<<< Exiting DeleteExpiredAuthCookies(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
        }

        /// <summary>Sets all tabs of the portal to the specified secure value.</summary>
        /// <param name="portalId">The portal to update.</param>
        /// <param name="secure">True to set all pages to secure, false to set them all to non secure.</param>
        public virtual void SetAllPortalTabsSecure(int portalId, bool secure)
        {
			Logger.LogError(">>> Entering SetAllPortalTabsSecure(portalId={PortalId},secure={Secure})", portalId, secure);
            Instance().ExecuteNonQuery("SetAllPortalTabsSecure", portalId, secure);
			Logger.LogWarning("<<< Exiting SetAllPortalTabsSecure(portalId={PortalId},secure={Secure})", portalId, secure);
        }

        public virtual DataSet ExecuteDataSet(string procedureName, params object[] commandParameters)
        {
			Logger.LogError(">>> Entering ExecuteDataSet(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            Logger.LogWarning("<<< Exiting ExecuteDataSet(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            return Globals.ConvertDataReaderToDataSet(this.ExecuteReader(procedureName, commandParameters));
        }

        public virtual IDataReader ExecuteSQL(string sql, params IDataParameter[] commandParameters)
        {
            SqlParameter[] sqlCommandParameters = null;
            if (commandParameters != null)
            {
                sqlCommandParameters = new SqlParameter[commandParameters.Length];
                for (int intIndex = 0; intIndex <= commandParameters.Length - 1; intIndex++)
                {
                    sqlCommandParameters[intIndex] = (SqlParameter)commandParameters[intIndex];
                }
            }

            sql = DataUtil.ReplaceTokens(sql);
            try
            {
                return SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, sqlCommandParameters);
            }
            catch
            {
                // error in SQL query
                return null;
            }
        }

        /// <inheritdoc cref="GetFiles(int,bool,bool)" />
        [DnnDeprecated(9, 3, 0, "Please use GetFiles(int, bool, bool) instead")]
#pragma warning disable CS1066
        public virtual partial IDataReader GetFiles(int folderId, bool retrieveUnpublishedFiles = false)
        {
            return this.GetFiles(folderId, retrieveUnpublishedFiles, false);
        }
#pragma warning restore CS1066

        internal virtual IDictionary<int, string> GetPortalSettingsBySetting(string settingName, string cultureCode)
        {
            var result = new Dictionary<int, string>();
            using (var reader = this.ExecuteReader("GetPortalSettingsBySetting", settingName, cultureCode))
            {
                while (reader.Read())
                {
                    result[reader.GetInt32(0)] = reader.GetString(1);
                }
            }

            return result;
        }

        private static DateTime FixDate(DateTime dateToFix)
        {
			Logger.LogError(">>> Entering FixDate(dateToFix={DateToFix})", dateToFix);
            // Fix for Sql Dates having a minimum value of January 1, 1753
            // or maximum value of December 31, 9999
            if (dateToFix <= SqlDateTime.MinValue.Value)
            {
                dateToFix = SqlDateTime.MinValue.Value.AddDays(1);
				Logger.LogError("FixDate(dateToFix={DateToFix}): dateToFix → {DateToFix1}", dateToFix, dateToFix);
            }
            else if (dateToFix >= SqlDateTime.MaxValue.Value)
            {
                dateToFix = SqlDateTime.MaxValue.Value.AddDays(-1);
            }

			Logger.LogWarning("<<< Exiting FixDate(dateToFix={DateToFix})", dateToFix);
            return dateToFix;
        }

        private static object GetRoleNull(int roleID)
        {
			Logger.LogError(">>> Entering GetRoleNull(roleID={RoleID})", roleID);
            if (roleID.ToString(CultureInfo.InvariantCulture) == Globals.glbRoleNothing)
            {
                return DBNull.Value;
            }

			Logger.LogWarning("<<< Exiting GetRoleNull(roleID={RoleID})", roleID);
            return roleID;
        }

        private Version GetVersionInternal(bool current)
        {
			Logger.LogError(">>> Entering GetVersionInternal(current={Current})", current);
            Version version = null;
            IDataReader dr = null;
			Logger.LogError("GetVersionInternal(current={Current}): dr → {Dr}", current, dr);
            try
            {
                dr = current ? this.GetDatabaseVersion() : this.GetDatabaseInstallVersion();
                if (dr.Read())
                {
                    version = new Version(
                        Convert.ToInt32(dr["Major"], CultureInfo.InvariantCulture),
                        Convert.ToInt32(dr["Minor"], CultureInfo.InvariantCulture),
                        Convert.ToInt32(dr["Build"], CultureInfo.InvariantCulture));
                }
            }
            catch (SqlException ex)
            {
				Logger.LogError("Exception in GetVersionInternal(current={Current}): {Arg1}", current, ex.Message);
                bool noStoredProc = false;
				Logger.LogError("GetVersionInternal(current={Current}): noStoredProc → {NoStoredProc}", current, noStoredProc);
                for (int i = 0; i <= ex.Errors.Count - 1; i++)
                {
                    SqlError sqlError = ex.Errors[i];
                    if (sqlError.Number == 2812 && sqlError.Class == 16)
                    {
                        // 2812 - 16 means SP could not be found
                        noStoredProc = true;
                        break;
                    }
                }

                if (!noStoredProc)
                {
                    throw;
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetVersionInternal(current={Current})", current);
            return version;
        }
    }
}
