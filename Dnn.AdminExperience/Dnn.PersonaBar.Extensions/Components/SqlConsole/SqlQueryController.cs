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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void AddQuery(SqlQuery query)
        {
			Logger.LogDebug(">>> Entering AddQuery(query={Query})");
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Insert(query);
				Logger.LogDebug("AddQuery(query={Query}): rep → {Rep}", rep);
            }
			Logger.LogDebug("<<< Exiting AddQuery(query={Query})");
        }

        /// <inheritdoc />
        public void DeleteQuery(SqlQuery query)
        {
			Logger.LogDebug(">>> Entering DeleteQuery(query={Query})");
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Delete(query);
				Logger.LogDebug("DeleteQuery(query={Query}): rep → {Rep}", rep);
            }
			Logger.LogDebug("<<< Exiting DeleteQuery(query={Query})");
        }

        /// <inheritdoc />
        public IEnumerable<SqlQuery> GetQueries()
        {
			Logger.LogDebug(">>> Entering GetQueries()");
            IEnumerable<SqlQuery> queries;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                queries = rep.Get();
				Logger.LogDebug("GetQueries(): rep → {Rep}", rep);
				Logger.LogDebug("GetQueries(): queries → {Queries}", queries);
            }

			Logger.LogDebug("<<< Exiting GetQueries()");
            return queries;
        }

        /// <inheritdoc />
        public SqlQuery GetQuery(int id)
        {
			Logger.LogDebug(">>> Entering GetQuery(id={Id})", id);
            SqlQuery query;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                query = rep.GetById(id);
				Logger.LogDebug("GetQuery(id={Id}): rep → {Rep}", id, rep);
				Logger.LogDebug("GetQuery(id={Id}): query → {Query}", id, query);
            }

			Logger.LogDebug("<<< Exiting GetQuery(id={Id})", id);
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

			Logger.LogDebug("<<< Exiting GetQuery(name={Name})", name);
            return query;
        }

        /// <inheritdoc />
        public void UpdateQuery(SqlQuery query)
        {
			Logger.LogDebug(">>> Entering UpdateQuery(query={Query})", query);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Update(query);
				Logger.LogDebug("UpdateQuery(query={Query}): rep → {Rep}", query, rep);
            }
			Logger.LogDebug("<<< Exiting UpdateQuery(query={Query})", query);
        }

        /// <inheritdoc />
        public IList<string> GetConnections()
        {
			Logger.LogDebug(">>> Entering GetConnections()");
            var connections = new List<string>();
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
			Logger.LogDebug("GetConnections(): connections → {Connections}", connections);
                if (!connection.Name.Equals("localmysqlserver", StringComparison.OrdinalIgnoreCase) && !connection.Name.Equals("localsqlserver", StringComparison.OrdinalIgnoreCase))
                {
                    connections.Add(connection.Name);
                }
            }

			Logger.LogDebug("<<< Exiting GetConnections()");
            return connections;
        }

        /// <inheritdoc />
        protected override Func<ISqlQueryController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new SqlQueryController();
        }
    }
}
