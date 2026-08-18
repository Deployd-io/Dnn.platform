// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SqlConsole.Components
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class SqlQueryController : ServiceLocator<ISqlQueryController, SqlQueryController>, ISqlQueryController
    {
        /// <inheritdoc />
        public void AddQuery(SqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering AddQuery(query={Query})");
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
				eventLogger.LogDebug("AddQuery(query={Query}): rep → {Rep}", rep);
                rep.Insert(query);
            }
			eventLogger.LogDebug("<<< Exiting AddQuery(query={Query})");
        }

        /// <inheritdoc />
        public void DeleteQuery(SqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering DeleteQuery(query={Query})");
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
				eventLogger.LogDebug("DeleteQuery(query={Query}): rep → {Rep}", rep);
                rep.Delete(query);
            }
			eventLogger.LogDebug("<<< Exiting DeleteQuery(query={Query})");
        }

        /// <inheritdoc />
        public IEnumerable<SqlQuery> GetQueries()
        {
			eventLogger.LogDebug(">>> Entering GetQueries()");
            IEnumerable<SqlQuery> queries;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
				eventLogger.LogDebug("GetQueries(): rep → {Rep}", rep);
                queries = rep.Get();
            }
			eventLogger.LogDebug("GetQueries(): queries → {Queries}", queries);

			eventLogger.LogDebug("<<< Exiting GetQueries()");
            return queries;
        }

        /// <inheritdoc />
        public SqlQuery GetQuery(int id)
        {
			eventLogger.LogDebug(">>> Entering GetQuery(id={Id})", id);
            SqlQuery query;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
				eventLogger.LogDebug("GetQuery(id={Id}): rep → {Rep}", id, rep);
                query = rep.GetById(id);
            }
			eventLogger.LogDebug("GetQuery(id={Id}): query → {Query}", id, query);

			eventLogger.LogDebug("<<< Exiting GetQuery(id={Id})", id);
			eventLogger.LogDebug("<<< Exiting GetQuery(name={Name})", name);
            return query;
        }

        /// <inheritdoc />
        public SqlQuery GetQuery(string name)
        {
            List<SqlQuery> queries;
            SqlQuery query = null;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                queries = rep.Find("where name = @0", name).ToList();
                if (queries != null && queries.Count > 0)
                {
                    query = queries.ElementAt(0);
                }
            }

            return query;
        }

        /// <inheritdoc />
        public void UpdateQuery(SqlQuery query)
        {
			eventLogger.LogDebug(">>> Entering UpdateQuery(query={Query})", query);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
				eventLogger.LogDebug("UpdateQuery(query={Query}): rep → {Rep}", query, rep);
                rep.Update(query);
            }
			eventLogger.LogDebug("<<< Exiting UpdateQuery(query={Query})", query);
        }

        /// <inheritdoc />
        public IList<string> GetConnections()
        {
			eventLogger.LogDebug(">>> Entering GetConnections()");
            var connections = new List<string>();
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
			eventLogger.LogDebug("GetConnections(): connections → {Connections}", connections);
                if (!connection.Name.Equals("localmysqlserver", StringComparison.OrdinalIgnoreCase) && !connection.Name.Equals("localsqlserver", StringComparison.OrdinalIgnoreCase))
                {
                    connections.Add(connection.Name);
                }
            }

			eventLogger.LogDebug("<<< Exiting GetConnections()");
            return connections;
        }

        /// <inheritdoc />
        protected override Func<ISqlQueryController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new SqlQueryController();
        }
    }
}
