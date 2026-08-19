// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SqlConsole.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SqlConsole.Components;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SqlConsoleController : PersonaBarApiController
    {
        private const string ScriptDelimiterRegex = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";

        private static readonly Regex SqlObjRegex = new Regex(
            ScriptDelimiterRegex,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private ISqlQueryController controller = SqlQueryController.Instance;

        ////private const int MaxOutputRecords = 500;
        [HttpGet]
        public HttpResponseMessage GetSavedQueries()
        {
			eventLogger.LogDebug(">>> Entering GetSavedQueries()");
			eventLogger.LogDebug("<<< Exiting GetSavedQueries()");
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                queries = this.controller.GetQueries(),
                connections = this.controller.GetConnections(),
            });
        }

        [HttpGet]
        public HttpResponseMessage GetSavedQuery(int id)
        {
			eventLogger.LogDebug(">>> Entering GetSavedQuery(id={Id})", id);
			eventLogger.LogDebug("<<< Exiting GetSavedQuery(id={Id})", id);
            return this.Request.CreateResponse(HttpStatusCode.OK, this.controller.GetQuery(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveQuery(SqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering SaveQuery(query={Query})");
            query.CreatedOnDate = DateTime.Now;
            query.CreatedByUserId = this.UserInfo.UserID;
			eventLogger.LogDebug("SaveQuery(query={Query}): query.CreatedByUserId → {Arg1}", query.CreatedByUserId);
			eventLogger.LogDebug("SaveQuery(query={Query}): query.LastModifiedOnDate → {Arg1}", query.LastModifiedOnDate);
			eventLogger.LogDebug("SaveQuery(query={Query}): query.LastModifiedByUserId → {Arg1}", query.LastModifiedByUserId);
			eventLogger.LogDebug("SaveQuery(query={Query}): query.CreatedOnDate → {Arg1}", query.CreatedOnDate);
            query.LastModifiedOnDate = DateTime.Now;
            query.LastModifiedByUserId = this.UserInfo.UserID;

            if (query.QueryId <= 0)
            {
                var saveQueries = this.controller.GetQueries();
				eventLogger.LogDebug("SaveQuery(query={Query}): saveQueries → {SaveQueries}", saveQueries);
                var saveQuery = saveQueries.FirstOrDefault(q => q.Name.Equals(query.Name, StringComparison.OrdinalIgnoreCase));
				eventLogger.LogDebug("SaveQuery(query={Query}): saveQuery → {SaveQuery}", saveQuery);
                if (saveQuery != null)
                {
                    query.QueryId = saveQuery.QueryId;
					eventLogger.LogDebug("SaveQuery(query={Query}): query.QueryId → {Arg1}", query.QueryId);
                }
            }

            if (query.QueryId > 0)
            {
                this.controller.UpdateQuery(query);
            }
            else
            {
                this.controller.AddQuery(query);
            }

			eventLogger.LogDebug("<<< Exiting SaveQuery(query={Query})");
            return this.Request.CreateResponse(HttpStatusCode.OK, query);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteQuery(SqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering DeleteQuery(query={Query})");
            var savedQuery = this.controller.GetQuery(query.QueryId);
            if (savedQuery != null)
            {
                this.controller.DeleteQuery(savedQuery);

			eventLogger.LogDebug("<<< Exiting DeleteQuery(query={Query})");
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RunQuery(AdhocSqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering RunQuery(query={Query})");
            var connectionstring = Config.GetConnectionString(query.ConnectionStringName);

			eventLogger.LogDebug("RunQuery(query={Query}): connectionstring → {Connectionstring}", connectionstring);
            var outputTables = new List<DataTable>();
			eventLogger.LogDebug("RunQuery(query={Query}): outputTables → {OutputTables}", outputTables);
            string errorMessage;

            var runAsQuery = RunAsScript(query.Query);
			eventLogger.LogDebug("RunQuery(query={Query}): runAsQuery → {RunAsQuery}", runAsQuery);
            if (runAsQuery)
            {
                errorMessage = DataProvider.Instance().ExecuteScript(connectionstring, query.Query, query.Timeout);
            }
            else
            {
                try
                {
                    var dr = DataProvider.Instance().ExecuteSQLTemp(connectionstring, query.Query, query.Timeout, out errorMessage);
					eventLogger.LogDebug("RunQuery(query={Query}): dr → {Dr}", dr);
                    if (dr != null)
                    {
                        do
                        {
                            var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                            table.Load(dr);
                            outputTables.Add(table);
                        }
                        while (!dr.IsClosed);
                    }
                }
                catch (SqlException sqlException)
                {
                    errorMessage = sqlException.Message;
                }
            }

            this.RecordAuditEventLog(query.Query);

            var statusCode = string.IsNullOrEmpty(errorMessage) ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
			eventLogger.LogDebug("RunQuery(query={Query}): statusCode → {StatusCode}", statusCode);
			eventLogger.LogDebug("<<< Exiting RunQuery(query={Query})");
            return this.Request.CreateResponse(statusCode, new { Data = runAsQuery ? null : outputTables, Error = errorMessage });
        }

        private static bool RunAsScript(string query)
        {
			eventLogger.LogDebug(">>> Entering RunAsScript(query={Query})");
			eventLogger.LogDebug("<<< Exiting RunAsScript(query={Query})");
            return SqlObjRegex.IsMatch(query);
        }

        private void RecordAuditEventLog(string query)
        {
			eventLogger.LogDebug(">>> Entering RecordAuditEventLog(query={Query})");
            var props = new LogProperties { new LogDetailInfo("User", this.UserInfo.Username), new LogDetailInfo("SQL Query", query) };

			eventLogger.LogDebug("RecordAuditEventLog(query={Query}): props → {Props}", props);
            // Add the event log with host portal id.
            var log = new LogInfo
            {
                LogUserID = this.UserInfo.UserID,
                LogTypeKey = nameof(EventLogType.HOST_SQL_EXECUTED),
                LogProperties = props,
                BypassBuffering = true,
                LogPortalID = Null.NullInteger,
            };

            LogController.Instance.AddLog(log);
			eventLogger.LogDebug("<<< Exiting RecordAuditEventLog(query={Query})");
        }
    }
}
