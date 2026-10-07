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
			eventLogger.LogError(">>> Entering LoadScript(name={Name})", name);
            var resourceName = $"Dnn.PersonaBar.Extensions.Components.Security.Resources.{name}.resources";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			eventLogger.LogError("LoadScript(name={Name}): resourceName → {ResourceName}", name, resourceName);
            if (stream != null)
            {
				eventLogger.LogError("LoadScript(name={Name}): stream → {Stream}", name, stream);
                var script = new StreamReader(stream).ReadToEnd();
				eventLogger.LogError("LoadScript(name={Name}): script → {Script}", name, script);
                return script.Replace("%SiteRoot%", Globals.ApplicationMapPath);
            }

			eventLogger.LogWarning("<<< Exiting LoadScript(name={Name})", name);
            return null;
        }

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
			eventLogger.LogError("Execute(): result → {Result}", result);
            IList<string> checkList = new List<string>()
            {
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

			eventLogger.LogWarning("<<< Exiting Execute()");
            return result;
        }

        private static bool VerifyScript(string name)
        {
			eventLogger.LogError(">>> Entering VerifyScript(name={Name})", name);
            try
            {
                var script = LoadScript(name);
				eventLogger.LogError("VerifyScript(name={Name}): script → {Script}", name, script);
                if (!string.IsNullOrEmpty(script))
                {
                    if (name == "ExecuteCommand")
                    {
                        // since sql error is expected here, do not go through DataProvider so that no error will be logged
                        using var connection = new SqlConnection(DataProvider.Instance().ConnectionString);
						eventLogger.LogError("VerifyScript(name={Name}): connection → {Connection}", name, connection);
                        try
                        {
                            connection.Open();
                            var command = new SqlCommand(script, connection) { CommandType = CommandType.Text };
							var __logStart = System.Diagnostics.Stopwatch.StartNew();
							eventLogger.LogError("VerifyScript(name={Name}): command → {Command}", name, command);
                            using var reader = command.ExecuteReader();
                            if (reader.Read())
                            {
								eventLogger.LogError("VerifyScript(name={Name}): reader → {Reader}", name, reader);
                                if (!int.TryParse(reader[0].ToString(), out var affectCount))
                                {
                                    affectCount = 0;
                                }

                                return affectCount == 0;
                            }
							eventLogger.LogError("VerifyScript(name)={Name}: database query executed in {Arg1} ms", name, __logStart.ElapsedMilliseconds);
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
				eventLogger.LogError("Exception in VerifyScript(name={Name}): {Arg1}", name, SqlException.Message);
                // ignore; return no failure
            }

            return true;
        }
    }
}
