// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using Newtonsoft.Json;

    public class Utility
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const long MaxFileSize = 1024 * 1024 * 10; // 10M

        private const int ModifiedFilesCount = 50;

        private static readonly IList<Regex> ExcludedFilePathRegexList = new List<Regex>()
        {
            new Regex(Regex.Escape("\\App_Data\\ClientDependency"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\App_Data\\Search"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\d+-System\\Cache\\Pages"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\d+-System\\Thumbnailsy"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\Portals\\_default\\Logs"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\App_Data\\_imagecache"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "Default.aspx"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "Default.aspx.cs"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "web.config"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        /// <summary>delete unneeded installwizard files.</summary>
        public static void CleanUpInstallerFiles()
        {
			Logger.LogDebug(">>> Entering CleanUpInstallerFiles()");
            var files = new List<string>
            {
                "DotNetNuke.install.config",
                "DotNetNuke.install.config.resources",
                "InstallWizard.aspx",
                "InstallWizard.aspx.cs",
                "InstallWizard.aspx.designer.cs",
                "UpgradeWizard.aspx",
                "UpgradeWizard.aspx.cs",
                "UpgradeWizard.aspx.designer.cs",
                "Install.aspx",
                "Install.aspx.cs",
                "Install.aspx.designer.cs",
            };

            foreach (var file in files)
            {
                try
                {
                    FileSystemUtils.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install\\" + file));
                }
                catch (Exception)
                {
					Logger.LogError("Exception in CleanUpInstallerFiles(): {Arg0}", ex.Message);
                    // do nothing.
                }
            }
			Logger.LogDebug("<<< Exiting CleanUpInstallerFiles()");
        }

        /// <summary>search all files in the website for matching text.</summary>
        /// <param name="searchText">the matching text.</param>
        /// <returns>A sequence of new <see cref="SearchFileInfo"/> instances.</returns>
        public static IEnumerable<object> SearchFiles(string searchText)
        {
			Logger.LogDebug(">>> Entering SearchFiles(searchText={SearchText})", searchText);
            try
            {
                var fileList = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories);
                return
                    from file in fileList
					Logger.LogDebug("SearchFiles(searchText={SearchText}): fileList → {FileList}", searchText, fileList);
                    where FileContainsText(file, searchText)
                    let f = new FileInfo(file)
                    select new SearchFileInfo
                    {
                        FileName = f.FullName,
                        LastWriteTime = f.LastWriteTime.ToString(CultureInfo.InvariantCulture),
						Logger.LogDebug(">>> Entering FileContainsText(name={Name},searchText={SearchText})", name, searchText);
						Logger.LogError("Exception in FileContainsText(name={Name},searchText={SearchText}): {Arg2}", name, searchText, ex.Message);
						Logger.LogDebug("<<< Exiting FileContainsText(name={Name},searchText={SearchText})", name, searchText);
                    };
            }
            catch
            {
				Logger.LogError("Exception in SearchFiles(searchText={SearchText}): {Arg1}", searchText, ex.Message);
                // suppress any unexpected error
            }

			Logger.LogDebug("<<< Exiting SearchFiles(searchText={SearchText})", searchText);
            return null;
        }

        /// <summary>search all website files for files with a potential dangerous extension.</summary>
        /// <param name="invalidFolders">A list to which invalid folders will be added.</param>
        /// <returns>A sequence of file paths.</returns>
        public static IEnumerable<string> FindUnexpectedExtensions(IList<string> invalidFolders)
        {
			Logger.LogDebug(">>> Entering FindUnexpectedExtensions(invalidFolders={InvalidFolders})", invalidFolders);
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories, invalidFolders)
            .Where(s => s.EndsWith(".asp", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".php", StringComparison.OrdinalIgnoreCase));
			Logger.LogDebug("<<< Exiting FindUnexpectedExtensions(invalidFolders={InvalidFolders})", invalidFolders);
            return files;
        }

        /// <summary>search all website files which are hidden or system.</summary>
        /// <returns>A sequence of file paths.</returns>
        public static IEnumerable<string> FineHiddenSystemFiles()
        {
			Logger.LogDebug(">>> Entering FineHiddenSystemFiles()");
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                if (Path.GetFileName(f)?.Equals("thumbs.db", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return false;
                }

                var attributes = File.GetAttributes(f);
                return attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System);
            });
			Logger.LogDebug(">>> Entering GetFiles(path={Path},searchPattern={SearchPattern},searchOption={SearchOption})", path, searchPattern, searchOption);
			Logger.LogDebug("<<< Exiting GetFiles(path={Path},searchPattern={SearchPattern},searchOption={SearchOption})", path, searchPattern, searchOption);
			Logger.LogDebug("<<< Exiting GetFiles(path={Path},searchPattern={SearchPattern},searchOption={SearchOption},invalidFolders={InvalidFolders})", path, searchPattern, searchOption, invalidFolders);
            return files;
        }

        public static List<object> SearchDatabase(string searchText)
        {
			Logger.LogDebug(">>> Entering SearchDatabase(searchText={SearchText})", searchText);
            List<object> results = new List<object>();
            var dataProvider = DataProvider.Instance();
			Logger.LogDebug("SearchDatabase(searchText={SearchText}): results → {Results}", searchText, results);
			Logger.LogDebug("SearchDatabase(searchText={SearchText}): dataProvider → {DataProvider}", searchText, dataProvider);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (var dr = dataProvider.ExecuteReader("SecurityAnalyzer_SearchAllTables", searchText))
                {
                    while (dr.Read())
                    {
                        results.Add(new
                        {
                            ColumnName = dr["ColumnName"],
                            ColumnValue = dr["ColumnValue"],
                        });
							Logger.LogDebug("SearchDatabase(searchText={SearchText}): ColumnName → {ColumnName}", searchText, ColumnName);
							Logger.LogDebug("SearchDatabase(searchText={SearchText}): ColumnValue → {ColumnValue}", searchText, ColumnValue);
							Logger.LogInformation("SearchDatabase(searchText)={SearchText}: database query executed in {Arg1} ms", searchText, __logStart.ElapsedMilliseconds);
                    }
                }
            }
            catch
            {
				Logger.LogError("Exception in SearchDatabase(searchText={SearchText}): {Arg1}", searchText, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting SearchDatabase(searchText={SearchText})", searchText);
            return results;
        }

        public static XmlDocument LoadFileSumData()
        {
			Logger.LogDebug(">>> Entering LoadFileSumData()");
            using (
                var stream =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Dnn.PersonaBar.Extensions.Components.Security.Resources.sums.resources"))
            {
                if (stream != null)
                {
                    var xmlDocument = new XmlDocument { XmlResolver = null };
                    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { XmlResolver = null, });
                    xmlDocument.Load(xmlReader);
					Logger.LogDebug("LoadFileSumData(): xmlReader → {XmlReader}", xmlReader);

                    return xmlDocument;
                }
                else
                {
					Logger.LogDebug("<<< Exiting LoadFileSumData()");
                    return null;
                }
            }
        }

        public static string GetFileCheckSum(string fileName)
        {
			Logger.LogDebug(">>> Entering GetFileCheckSum(fileName={FileName})", fileName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            using (var cryptographyProvider = SHA256.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? "System.Security.Cryptography.SHA256CryptoServiceProvider" : "System.Security.Cryptography.SHA256Cng"))
            {
                using (var stream = File.OpenRead(fileName))
                {
					Logger.LogInformation("GetFileCheckSum(fileName)={FileName}: external service call took {Arg1} ms", fileName, __logStart.ElapsedMilliseconds);
					Logger.LogDebug("<<< Exiting GetFileCheckSum(fileName={FileName})", fileName);
                    return BitConverter.ToString(cryptographyProvider.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
        }

        public static string GetApplicationVersion()
        {
			Logger.LogDebug(">>> Entering GetApplicationVersion()");
			Logger.LogDebug("<<< Exiting GetApplicationVersion()");
            return DotNetNukeContext.Current.Application.Version.ToString(3);
        }

        public static string GetApplicationType()
        {
			Logger.LogDebug(">>> Entering GetApplicationType()");
            switch (DotNetNukeContext.Current.Application.Name)
            {
                case "DNNCORP.CE":
                    return "Platform";
                case "DNNCORP.XE":
                case "DNNCORP.PE":
                    return "Content";
                case "DNNCORP.SOCIAL":
                    return "Social";
                default:
					Logger.LogDebug("<<< Exiting GetApplicationType()");
                    return "Platform";
            }
        }

        public static IList<FileInfo> GetLastModifiedFiles()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedFiles()");
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => !ExcludedFilePathRegexList.Any(r => r.IsMatch(f)))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Take(ModifiedFilesCount).ToList();

			Logger.LogDebug("<<< Exiting GetLastModifiedFiles()");
            return files;
        }

        public static IList<FileInfo> GetLastModifiedExecutableFiles()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedExecutableFiles()");
            var executableExtensions = new List<string>() { ".asp", ".aspx", ".php" };
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
				Logger.LogDebug("GetLastModifiedExecutableFiles(): executableExtensions → {ExecutableExtensions}", executableExtensions);
                    var extension = Path.GetExtension(f);
                    return extension != null && executableExtensions.Contains(extension.ToLowerInvariant());
                }).ToList();
            files.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Default.aspx.cs"));
            files.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config"));

            var defaultPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Default.aspx");
            if (!files.Contains(defaultPage))
			Logger.LogDebug("GetLastModifiedExecutableFiles(): defaultPage → {DefaultPage}", defaultPage);
            {
                files.Add(defaultPage);
            }

			Logger.LogDebug("<<< Exiting GetLastModifiedExecutableFiles()");
            return files
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .Take(ModifiedFilesCount).ToList();
        }

        private static bool FileContainsText(string name, string searchText)
        {
			Logger.LogDebug(">>> Entering FileContainsText(name={Name},searchText={SearchText})", name, searchText);
            try
            {
                // If the file has been deleted since we took
                // the snapshot, ignore it and return the empty string.
                if (IsReadable(name))
                {
                    return File.ReadAllText(name).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in FileContainsText(name={Name},searchText={SearchText}): {Arg2}", name, searchText, ex.Message);
                // might be a locking issue
            }

			Logger.LogDebug("<<< Exiting FileContainsText(name={Name},searchText={SearchText})", name, searchText);
            return false;
        }

        private static bool IsReadable(string name)
        {
			Logger.LogDebug(">>> Entering IsReadable(name={Name})", name);
            if (!File.Exists(name))
            {
                return false;
            }

            var file = new FileInfo(name);
            if (file.Length > MaxFileSize)
			Logger.LogDebug("IsReadable(name={Name}): file → {File}", name, file);
            {
                // when file large than 10M, then don't read it.
                return false;
            }

			Logger.LogDebug("<<< Exiting IsReadable(name={Name})", name);
            return true;
        }

        /// <summary>Recursively finds file.</summary>
        /// <returns>A sequence of file paths.</returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                return GetFiles(path, searchPattern, searchOption, new List<string>());
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>Recursively finds file.</summary>
        /// <returns>A sequence of file paths.</returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption, ICollection<string> invalidFolders)
        {
            // Looking at the root folder only. There should not be any permission issue here.
            IList<string> files;
            try
            {
                files = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception)
            {
                invalidFolders.Add(path);
                yield break;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                IList<string> folders;
                try
                {
                    folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception)
                {
                    invalidFolders.Add(path);
                    yield break;
                }

                foreach (var folder in folders)
                {
                    // recursive call to the same method
                    foreach (var f in GetFiles(folder, searchPattern, searchOption, invalidFolders))
                    {
                        yield return f;
                    }
                }
            }
        }

        // This will reduce serialization thrown exceptions of anonymous type
        [JsonObject]
        private class SearchFileInfo
        {
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string FileName;
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string LastWriteTime;
        }
    }
}
