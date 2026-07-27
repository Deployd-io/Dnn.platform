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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckSqlRisk";

        /// <inheritdoc />
        public bool LazyLoad => false;

        private static string LocalResourceFile => "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx";

        public static string LoadScript(string name)
        {
			Logger.LogDebug(">>> Entering LoadScript(name={Name})", name);
            var resourceName = $"Dnn.PersonaBar.Extensions.Components.Security.Resources.{name}.resources";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			Logger.LogDebug("LoadScript(name={Name}): resourceName → {ResourceName}", name, resourceName);
			Logger.LogDebug("LoadScript(name={Name}): stream → {Stream}", name, stream);
            if (stream != null)
            {
                var script = new StreamReader(stream).ReadToEnd();
                return script.Replace("%SiteRoot%", Globals.ApplicationMapPath);
            }

			Logger.LogDebug("<<< Exiting LoadScript(name={Name})", name);
            return null;
        }

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            IList<string> checkList = new List<string>()
            {
			Logger.LogDebug("Execute(): result → {Result}", result);
                "SysAdmin",
                "ExecuteCommand",
                "GetFolderTree",
                "CheckFileExists",
                "RegRead",
            };

            result.Severity = SeverityEnum.Pass;
			Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
            foreach (var name in checkList)
            {
                if (!VerifyScript(name))
                {
                    result.Severity = SeverityEnum.Warning;
                    result.Notes.Add(Localization.GetString(name + ".Error", LocalResourceFile));
                }
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static bool VerifyScript(string name)
        {
			Logger.LogDebug(">>> Entering VerifyScript(name={Name})", name);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var script = LoadScript(name);
                if (!string.IsNullOrEmpty(script))
				Logger.LogDebug("VerifyScript(name={Name}): script → {Script}", name, script);
                {
                    if (name == "ExecuteCommand")
                    {
                        // since sql error is expected here, do not go through DataProvider so that no error will be logged
                        using var connection = new SqlConnection(DataProvider.Instance().ConnectionString);
                        try
						Logger.LogDebug("VerifyScript(name={Name}): connection → {Connection}", name, connection);
                        {
                            connection.Open();
                            var command = new SqlCommand(script, connection) { CommandType = CommandType.Text };
                            using var reader = command.ExecuteReader();
                            if (reader.Read())
							Logger.LogDebug("VerifyScript(name={Name}): command → {Command}", name, command);
							Logger.LogDebug("VerifyScript(name={Name}): reader → {Reader}", name, reader);
							Logger.LogInformation("VerifyScript(name)={Name}: database query executed in {Arg1} ms", name, __logStart.ElapsedMilliseconds);
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
							Logger.LogError("Exception in VerifyScript(name={Name}): {Arg1}", name, ex.Message);
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
				Logger.LogError("Exception in VerifyScript(name={Name}): {Arg1}", name, ex.Message);
                // ignore; return no failure
            }

			Logger.LogDebug("<<< Exiting VerifyScript(name={Name})", name);
            return true;
        }
    }
}
