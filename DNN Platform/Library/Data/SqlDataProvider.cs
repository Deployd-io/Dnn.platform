// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

    public sealed class SqlDataProvider : DataProvider
    {
        private const string ScriptDelimiter = @"(?<=(?:[^\w]+|^))GO(?=(?: |\t)*?(?:\r?\n|$))";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SqlDataProvider));
        private static DatabaseConnectionProvider dbConnectionProvider = DatabaseConnectionProvider.Instance() ?? new SqlDatabaseConnectionProvider();

        /// <summary>Initializes a new instance of the <see cref="SqlDataProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public SqlDataProvider()
            : this(null)
        {
		Logger.LogDebug(">>> Entering SqlDataProvider()");
        }
		Logger.LogDebug("<<< Exiting SqlDataProvider()");

        /// <summary>Initializes a new instance of the <see cref="SqlDataProvider"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        public SqlDataProvider(IApplicationStatusInfo appStatus)
            : base(appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>())
        {
        }

        /// <inheritdoc />
        public override bool IsConnectionValid => this.CanConnect();

        /// <inheritdoc />
        public override Dictionary<string, string> Settings => ComponentFactory.GetComponentSettings<SqlDataProvider>() as Dictionary<string, string>;

        /// <inheritdoc />
        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            dbConnectionProvider.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
			Logger.LogDebug("<<< Exiting ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("ExecuteNonQuery(procedureName,commandParameters)={ProcedureName},{CommandParameters}: database query executed in {Arg2} ms", procedureName, commandParameters, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("ExecuteNonQuery(procedureName,commandParameters)={ProcedureName},{CommandParameters}: database query executed in {Arg2} ms", procedureName, commandParameters, __logStart.ElapsedMilliseconds);
        /// <inheritdoc />
        public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogDebug(">>> Entering ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("ExecuteNonQuery(timeoutSec,procedureName,commandParameters)={TimeoutSec},{ProcedureName},{CommandParameters}: database query executed in {Arg3} ms", timeoutSec, procedureName, commandParameters, __logStart.ElapsedMilliseconds);
            dbConnectionProvider.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
			Logger.LogDebug("<<< Exiting ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
            dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable);
			Logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
        }

        /// <inheritdoc />
        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
        {
			Logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
            dbConnectionProvider.BulkInsert(this.ConnectionString, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable);
			Logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
        }

        /// <inheritdoc />
        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
            dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
        }

        /// <inheritdoc />
        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> commandParameters)
        {
            dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			Logger.LogDebug("<<< Exiting ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            return dbConnectionProvider.ExecuteReader(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug("<<< Exiting ExecuteReader(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            return dbConnectionProvider.ExecuteReader(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteScalar(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            return dbConnectionProvider.ExecuteScalar<T>(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            return dbConnectionProvider.ExecuteScalar<T>(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override string ExecuteScript(string script)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(script={Script})", script);
			Logger.LogDebug("<<< Exiting ExecuteScript(script={Script})", script);
            return this.ExecuteScript(script, 0);
        }

        /// <inheritdoc />
        public override string ExecuteScript(string script, int timeoutSec)
        {
            string exceptions = ExecuteScriptInternal(this.UpgradeConnectionString, script, timeoutSec);

            // if the upgrade connection string is specified or or db_owner setting is not set to dbo
            if (this.UpgradeConnectionString != this.ConnectionString || !this.DatabaseOwner.Trim().Equals("dbo.", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // grant execute rights to the public role or userid for all stored procedures. This is
                    // necesary because the UpgradeConnectionString will create stored procedures
                    // which restrict execute permissions for the ConnectionString user account. This is also
                    // necessary when db_owner is not set to "dbo"
                    exceptions += this.GrantStoredProceduresPermission("EXECUTE", this.GetConnectionStringUserId());
                }
                catch (SqlException objException)
                {
                    Logger.Error(objException);

                    exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
                }

                try
                {
                    // grant execute or select rights to the public role or userid for all user defined functions based
                    // on what type of function it is (scalar function or table function). This is
                    // necesary because the UpgradeConnectionString will create user defined functions
                    // which restrict execute permissions for the ConnectionString user account.  This is also
                    // necessary when db_owner is not set to "dbo"
                    exceptions += this.GrantUserDefinedFunctionsPermission("EXECUTE", "SELECT", this.GetConnectionStringUserId());
                }
                catch (SqlException objException)
                {
                    Logger.Error(objException);

                    exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
                }
            }

			Logger.LogDebug("<<< Exiting ExecuteScript(script={Script},timeoutSec={TimeoutSec})", script, timeoutSec);
            return exceptions;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override string ExecuteScript(string connectionString, string script)
        {
			Logger.LogDebug("<<< Exiting ExecuteScript(connectionString={ConnectionString},script={Script})", connectionString, script);
            return ExecuteScriptInternal(connectionString, script);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override string ExecuteScript(string connectionString, string script, int timeoutSec)
        {
			Logger.LogDebug("<<< Exiting ExecuteScript(connectionString={ConnectionString},script={Script},timeoutSec={TimeoutSec})", connectionString, script, timeoutSec);
            return ExecuteScriptInternal(connectionString, script, timeoutSec);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQL(string sql)
        {
            return ExecuteSQLInternal(this.ConnectionString, sql);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQL(string sql, int timeoutSec)
        {
            return ExecuteSQLInternal(this.ConnectionString, sql, timeoutSec);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            string errorMessage;
            return this.ExecuteSQLTemp(connectionString, sql, out errorMessage);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
        {
            string errorMessage;
            return this.ExecuteSQLTemp(connectionString, sql, timeoutSec, out errorMessage);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, 0, out errorMessage);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
        }

        private static string ExecuteScriptInternal(string connectionString, string script, int timeoutSec = 0)
        {
			Logger.LogDebug(">>> Entering ExecuteScriptInternal(connectionString={ConnectionString},script={Script},timeoutSec={TimeoutSec})", connectionString, script, timeoutSec);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            string exceptions = string.Empty;

            var sqlDelimiterRegex = RegexUtils.GetCachedRegex(ScriptDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] sqlStatements = sqlDelimiterRegex.Split(script);
			Logger.LogDebug("ExecuteScriptInternal(connectionString={ConnectionString},script={Script},timeoutSec={TimeoutSec}): sqlDelimiterRegex → {SqlDelimiterRegex}", connectionString, script, timeoutSec, sqlDelimiterRegex);
            foreach (string statement in sqlStatements)
            {
                var sql = statement.Trim();
                if (!string.IsNullOrEmpty(sql))
				Logger.LogDebug("ExecuteScriptInternal(connectionString={ConnectionString},script={Script},timeoutSec={TimeoutSec}): sql → {Sql}", connectionString, script, timeoutSec, sql);
                {
                    // script dynamic substitution
                    sql = DataUtil.ReplaceTokens(sql);

                    try
                    {
                        Logger.Trace("Executing SQL Script " + sql);

                        dbConnectionProvider.ExecuteNonQuery(connectionString, CommandType.Text, timeoutSec, sql);
						Logger.LogInformation("ExecuteScriptInternal(connectionString,script,timeoutSec)={ConnectionString},{Script},{TimeoutSec}: database query executed in {Arg3} ms", connectionString, script, timeoutSec, __logStart.ElapsedMilliseconds);
                    }
                    catch (SqlException objException)
                    {
                        Logger.Error(objException);
                        exceptions += objException + Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting ExecuteScriptInternal(connectionString={ConnectionString},script={Script},timeoutSec={TimeoutSec})", connectionString, script, timeoutSec);
            return exceptions;
        }

        private static IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeoutSec = 0)
        {
            string errorMessage;
            return ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
        }

        private static IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
            try
            {
                sql = DataUtil.ReplaceTokens(sql);
                errorMessage = string.Empty;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }

                return dbConnectionProvider.ExecuteSql(connectionString, CommandType.Text, timeoutSec, sql);
            }
            catch (SqlException sqlException)
            {
                // error in SQL query
                Logger.Error(sqlException);
                errorMessage = sqlException.Message;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.ToString();
            }

            errorMessage += Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
			Logger.LogDebug("<<< Exiting ExecuteSQLInternal(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            return null;
        }

        private bool CanConnect()
        {
			Logger.LogDebug(">>> Entering CanConnect()");
            bool connectionValid = true;

			Logger.LogDebug("CanConnect(): connectionValid → {ConnectionValid}", connectionValid);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                this.ExecuteNonQuery("GetDatabaseVersion");
				Logger.LogInformation("CanConnect(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }
            catch (SqlException ex)
            {
				Logger.LogError("Exception in CanConnect(): {Arg0}", ex.Message);
                if (ex.Errors.Cast<SqlError>().Any(c => !(c.Number == 2812 && c.Class == 16)))
                {
                    connectionValid = false;
                }
            }

			Logger.LogDebug("<<< Exiting CanConnect()");
            return connectionValid;
        }

        private string GetConnectionStringUserId()
        {
			Logger.LogDebug(">>> Entering GetConnectionStringUserId()");
            string dbUser = "public";

			Logger.LogDebug("GetConnectionStringUserId(): dbUser → {DbUser}", dbUser);
            // If connection string does not use integrated security, then get user id.
            // Normalize to uppercase before all the comparisons
            if (this.ConnectionString.Contains("USER ID", StringComparison.OrdinalIgnoreCase)
                || this.ConnectionString.Contains("UID", StringComparison.OrdinalIgnoreCase)
                || this.ConnectionString.Contains("USER", StringComparison.OrdinalIgnoreCase))
            {
                var connSettings = this.ConnectionString.Split(';');
                foreach (var s in connSettings)
                {
				Logger.LogDebug("GetConnectionStringUserId(): connSettings → {ConnSettings}", connSettings);
                    if (s != string.Empty)
                    {
                        var connSetting = s.Split('=');
                        if ("USER ID|UID|USER".Contains(connSetting[0].Trim(), StringComparison.OrdinalIgnoreCase))
						Logger.LogDebug("GetConnectionStringUserId(): connSetting → {ConnSetting}", connSetting);
                        {
                            dbUser = connSetting[1].Trim();
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetConnectionStringUserId()");
            return dbUser;
        }

        private string GrantStoredProceduresPermission(string permission, string loginOrRole)
        {
			Logger.LogDebug(">>> Entering GrantStoredProceduresPermission(permission={Permission},loginOrRole={LoginOrRole})", permission, loginOrRole);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // grant rights to a login or role for all stored procedures
            var sql = "if exists (select * from dbo.sysusers where name='" + loginOrRole + "')"
				Logger.LogInformation("GrantStoredProceduresPermission(permission,loginOrRole)={Permission},{LoginOrRole}: find query executed in {Arg2} ms", permission, loginOrRole, __logStart.ElapsedMilliseconds);
                + "  begin"
                + "    declare @exec nvarchar(2000) "
                + "    declare @name varchar(150) "
                + "    declare sp_cursor cursor for select o.name as name "
                + "    from dbo.sysobjects o "
                + "    where ( OBJECTPROPERTY(o.id, N'IsProcedure') = 1 or OBJECTPROPERTY(o.id, N'IsExtendedProc') = 1 or OBJECTPROPERTY(o.id, N'IsReplProc') = 1 ) "
                + "    and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 "
                + "    and o.name not like N'#%%' "
                + "    and (left(o.name,len('" + this.ObjectQualifier + "')) = '" + this.ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                + "    open sp_cursor "
                + "    fetch sp_cursor into @name "
                + "    while @@fetch_status >= 0 "
                + "      begin"
                + "        select @exec = 'grant " + permission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                + "        execute (@exec)"
                + "        fetch sp_cursor into @name "
                + "      end "
                + "    deallocate sp_cursor"
                + "  end ";

			Logger.LogDebug("<<< Exiting GrantStoredProceduresPermission(permission={Permission},loginOrRole={LoginOrRole})", permission, loginOrRole);
            return this.ExecuteUpgradedConnectionQuery(sql);
        }

        private string GrantUserDefinedFunctionsPermission(string scalarPermission, string tablePermission, string loginOrRole)
        {
			Logger.LogDebug(">>> Entering GrantUserDefinedFunctionsPermission(scalarPermission={ScalarPermission},tablePermission={TablePermission},loginOrRole={LoginOrRole})", scalarPermission, tablePermission, loginOrRole);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // grant EXECUTE rights to a login or role for all functions
            var sql = "if exists (select * from dbo.sysusers where name='" + loginOrRole + "')"
				Logger.LogInformation("GrantUserDefinedFunctionsPermission(scalarPermission,tablePermission,loginOrRole)={ScalarPermission},{TablePermission},{LoginOrRole}: find query executed in {Arg3} ms", scalarPermission, tablePermission, loginOrRole, __logStart.ElapsedMilliseconds);
                + "  begin"
                + "    declare @exec nvarchar(2000) "
                + "    declare @name varchar(150) "
                + "    declare @isscalarfunction int "
                + "    declare @istablefunction int "
                + "    declare sp_cursor cursor for select o.name as name, OBJECTPROPERTY(o.id, N'IsScalarFunction') as IsScalarFunction "
                + "    from dbo.sysobjects o "
                + "    where ( OBJECTPROPERTY(o.id, N'IsScalarFunction') = 1 OR OBJECTPROPERTY(o.id, N'IsTableFunction') = 1 ) "
                + "      and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 "
                + "      and o.name not like N'#%%' "
                + "      and (left(o.name,len('" + this.ObjectQualifier + "')) = '" + this.ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                + "    open sp_cursor "
                + "    fetch sp_cursor into @name, @isscalarfunction "
                + "    while @@fetch_status >= 0 "
                + "      begin "
                + "        if @IsScalarFunction = 1 "
                + "          begin"
                + "            select @exec = 'grant " + scalarPermission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                + "            execute (@exec)"
                + "            fetch sp_cursor into @name, @isscalarfunction  "
                + "          end "
                + "        else "
                + "          begin"
                + "            select @exec = 'grant " + tablePermission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                + "            execute (@exec)"
                + "            fetch sp_cursor into @name, @isscalarfunction  "
                + "          end "
                + "      end "
                + "    deallocate sp_cursor"
                + "  end ";

			Logger.LogDebug("<<< Exiting GrantUserDefinedFunctionsPermission(scalarPermission={ScalarPermission},tablePermission={TablePermission},loginOrRole={LoginOrRole})", scalarPermission, tablePermission, loginOrRole);
            return this.ExecuteUpgradedConnectionQuery(sql);
        }

        private string ExecuteUpgradedConnectionQuery(string sql)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var exceptions = string.Empty;

            try
            {
                dbConnectionProvider.ExecuteNonQuery(this.UpgradeConnectionString, CommandType.Text, 0, sql);
				Logger.LogInformation("ExecuteUpgradedConnectionQuery(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                exceptions += objException + Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
            }

            return exceptions;
        }
    }
}
