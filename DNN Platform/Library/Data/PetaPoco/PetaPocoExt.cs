// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Data;

    using global::PetaPoco;

    public static class PetaPocoExt
    {
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PetaPocoExt));
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static IDataReader ExecuteReader(this Database database, string sql, params object[] args)
#pragma warning restore CS3001
        {
			Logger.LogError(">>> Entering ExecuteReader(database={Database},sql={Sql},args={Args})", database, sql, args);
            IDataReader reader;
            try
            {
                database.OpenSharedConnection();
                sql = NormalizeSql(sql, args);
				Logger.LogError("ExecuteReader(database={Database},sql={Sql},args={Args}): sql → {Sql1}", database, sql, args, sql);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();

                using (IDbCommand command = database.CreateCommand(database.Connection, sql, args))
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    database.OnExecutedCommand(command);
					Logger.LogError("ExecuteReader(database={Database},sql={Sql},args={Args}): reader → {Reader}", database, sql, args, reader);
					Logger.LogError("ExecuteReader(database,args)={Database},{Args}: database query executed in {Arg2} ms", database, args, __logStart.ElapsedMilliseconds);
                }
            }
            catch (Exception exception)
            {
				Logger.LogDebug("ExecuteReader(database={Database},sql={Sql},args={Args}): reader → {Reader}", database, sql, args, reader);
                if (database.OnException(exception))
                {
					Logger.LogError("Exception in ExecuteReader(database={Database},sql={Sql},args={Args}): {Arg3}", database, sql, args, exception.Message);
                    throw;
                }

                reader = null;
            }

			Logger.LogWarning("<<< Exiting ExecuteReader(database={Database},sql={Sql},args={Args})", database, sql, args);
            return reader;
        }

        /// <summary>Escapes the "@" character if the arguments are empty and the sql string contains any "@" characters.</summary>
        /// <param name="sql">Sql string to normalize.</param>
        /// <param name="args">Sql command arguments.</param>
        /// <returns>Normalized sql string.</returns>
        private static string NormalizeSql(string sql, object[] args)
        {
			Logger.LogError(">>> Entering NormalizeSql(sql={Sql},args={Args})", sql, args);
            const string ReplaceFrom = "@";
            const string ReplaceTo = "@@";

            if (
                (args != null && args.Length != 0) ||
                string.IsNullOrWhiteSpace(sql))
            {
			Logger.LogWarning("<<< Exiting NormalizeSql(sql={Sql},args={Args})", sql, args);
                return sql;
            }

            return sql.Replace(ReplaceFrom, ReplaceTo);
        }
    }
}
