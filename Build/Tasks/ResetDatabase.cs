// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;

    using Cake.Common.Diagnostics;
    using Cake.Core;
    using Cake.FileHelpers;
    using Cake.Frosting;

    using Microsoft.Data.SqlClient;

    /// <summary>A cake task to reset the local dev database.</summary>
    public sealed class ResetDatabase : FrostingTask<Context>
    {
        private const string ScriptsPath = @".\Build\Tasks\sql\";

        private static readonly string[] GoStatement = ["\r\nGO\r\n", "\nGO\n", "\nGO\r\n", "\r\nGO\n",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            var script = ReplaceScriptVariables(context, LoadScript(context, "db-connections-drop"));
            ExecuteScript(context, script);
			Logger.LogDebug("Run(context={Context}): script → {Script}", context, script);
            script = ReplaceScriptVariables(context, LoadScript(context, "create-db"));
            ExecuteScript(context, script);
            if (context.Settings.DnnSqlUsername != string.Empty)
            {
                script = ReplaceScriptVariables(context, LoadScript(context, "add-db-user"));
                ExecuteScript(context, script);
            }
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static string LoadScript(ICakeContext context, string scriptName)
        {
			Logger.LogDebug(">>> Entering LoadScript(context={Context},scriptName={ScriptName})", context, scriptName);
            var script = scriptName + ".local.sql";
            if (!System.IO.File.Exists(ScriptsPath + script))
			Logger.LogDebug("LoadScript(context={Context},scriptName={ScriptName}): script → {Script}", context, scriptName, script);
            {
                script = scriptName + ".sql";
            }

			Logger.LogDebug("<<< Exiting LoadScript(context={Context},scriptName={ScriptName})", context, scriptName);
            return context.FileReadText(ScriptsPath + script);
        }

        private static string ReplaceScriptVariables(Context context, string script)
        {
            var dbPath = context.FileSystem.GetDirectory(context.Settings.DatabasePath);
            dbPath.Create();
            var fullDbPath = System.IO.Path.GetFullPath(dbPath.Path.FullPath);

            return script.Replace("{DBName}", context.Settings.DnnDatabaseName)
                .Replace("{DBPath}", fullDbPath)
                .Replace("{DBLogin}", context.Settings.DnnSqlUsername);
        }

        private static bool ExecuteScript(Context context, string scriptStatement)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(context={Context},scriptStatement={ScriptStatement})", context, scriptStatement);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using (var connection = new SqlConnection(context.Settings.SaConnectionString))
                {
                    connection.Open();
                    foreach (var cmd in scriptStatement.Split(GoStatement, StringSplitOptions.RemoveEmptyEntries))
                    {
						private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                        var command = new SqlCommand(cmd, connection);
                        command.ExecuteNonQuery();
						Logger.LogInformation("ExecuteScript(context)={Context}: database query executed in {Arg1} ms", context, __logStart.ElapsedMilliseconds);
                    }

                    connection.Close();
                }
            }
            catch (Exception err)
            {
				Logger.LogError("Exception in ExecuteScript(context={Context},scriptStatement={ScriptStatement}): {Arg2}", context, scriptStatement, ex.Message);
                context.Error(err);
                return false;
            }

			Logger.LogDebug("<<< Exiting ExecuteScript(context={Context},scriptStatement={ScriptStatement})", context, scriptStatement);
            return true;
        }
    }
}
