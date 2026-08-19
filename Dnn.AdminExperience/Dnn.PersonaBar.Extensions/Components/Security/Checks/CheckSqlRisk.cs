// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Localization;

    using Assembly = System.Reflection.Assembly;

    public class CheckSqlRisk : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckSqlRisk";

        /// <inheritdoc />
        public bool LazyLoad => false;

        private static string LocalResourceFile => "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx";

        public static string LoadScript(string name)
        {
			eventLogger.LogDebug(">>> Entering LoadScript(name={Name})", name);
            var resourceName = $"Dnn.PersonaBar.Extensions.Components.Security.Resources.{name}.resources";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			eventLogger.LogDebug("LoadScript(name={Name}): stream → {Stream}", name, stream);
			eventLogger.LogDebug("LoadScript(name={Name}): resourceName → {ResourceName}", name, resourceName);
            if (stream != null)
            {
                var script = new StreamReader(stream).ReadToEnd();
				eventLogger.LogDebug("LoadScript(name={Name}): script → {Script}", name, script);
			eventLogger.LogDebug("<<< Exiting LoadScript(name={Name})", name);
                return script.Replace("%SiteRoot%", Globals.ApplicationMapPath);
            }

            return null;
        }

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            IList<string> checkList = new List<string>()
            {
			eventLogger.LogDebug("Execute(): result → {Result}", result);
                "SysAdmin",
                "ExecuteCommand",
                "GetFolderTree",
                "CheckFileExists",
                "RegRead",
            };

            result.Severity = SeverityEnum.Pass;
			eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
            foreach (var name in checkList)
            {
                if (!VerifyScript(name))
                {
                    result.Severity = SeverityEnum.Warning;
                    result.Notes.Add(Localization.GetString(name + ".Error", LocalResourceFile));
                }
            }

			eventLogger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static bool VerifyScript(string name)
        {
			eventLogger.LogDebug(">>> Entering VerifyScript(name={Name})", name);
            try
            {
                var script = LoadScript(name);
				eventLogger.LogDebug("VerifyScript(name={Name}): script → {Script}", name, script);
                if (!string.IsNullOrEmpty(script))
                {
                    if (name == "ExecuteCommand")
                    {
                        // since sql error is expected here, do not go through DataProvider so that no error will be logged
                        using var connection = new SqlConnection(DataProvider.Instance().ConnectionString);
						eventLogger.LogDebug("VerifyScript(name={Name}): connection → {Connection}", name, connection);
                        try
                        {
                            connection.Open();
                            var command = new SqlCommand(script, connection) { CommandType = CommandType.Text };
							var __logStart = System.Diagnostics.Stopwatch.StartNew();
                            using var reader = command.ExecuteReader();
                            if (reader.Read())
							eventLogger.LogDebug("VerifyScript(name={Name}): command → {Command}", name, command);
							eventLogger.LogDebug("VerifyScript(name={Name}): reader → {Reader}", name, reader);
							eventLogger.LogInformation("VerifyScript(name)={Name}: database query executed in {Arg1} ms", name, __logStart.ElapsedMilliseconds);
                            {
                                if (!int.TryParse(reader[0].ToString(), out var affectCount))
                                {
                                    affectCount = 0;
                                }

                                return affectCount == 0;
                            }
                        }
                        catch (Exception)
                        {
							eventLogger.LogError("Exception in VerifyScript(name={Name}): {Arg1}", name, Exception.Message);
                            // ignore;
                        }
                    }
                    else
                    {
                        using var reader = DataProvider.Instance().ExecuteSQL(script);
                        if (reader != null && reader.Read())
                        {
                            if (!int.TryParse(reader[0].ToString(), out var affectCount))
                            {
                                affectCount = 0;
                            }

                            return affectCount == 0;
                        }
                    }
                }
            }
            catch (SqlException)
            {
				eventLogger.LogError("Exception in VerifyScript(name={Name}): {Arg1}", name, SqlException.getMessage());
                // ignore; return no failure
            }

			eventLogger.LogDebug("<<< Exiting VerifyScript(name={Name})", name);
            return true;
        }
    }
}
