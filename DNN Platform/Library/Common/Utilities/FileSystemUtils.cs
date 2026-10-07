// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using ICSharpCode.SharpZipLib.Zip;
    using Microsoft.Extensions.DependencyInjection;

    using Directory = SchwabenCode.QuickIO.QuickIODirectory;
    using DirectoryInfo = SchwabenCode.QuickIO.QuickIODirectoryInfo;
    using File = SchwabenCode.QuickIO.QuickIOFile;

    /// <summary>File System utilities.</summary>
    public partial class FileSystemUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemUtils));

        /// <summary>Adds a File to a Zip File.</summary>
        /// <param name="zipFile">The Zip File to add to.</param>
        /// <param name="filePath">The path to the file to add.</param>
        /// <param name="fileName">The name of the file to add.</param>
        /// <param name="folder">The name of the folder to use for the zip entry.</param>
        public static void AddToZip(ref ZipArchive zipFile, string filePath, string fileName, string folder)
        {
            using var fs = File.OpenRead(FixPath(filePath));

            var buffer = new byte[fs.Length];

            var len = fs.Read(buffer, 0, buffer.Length);
            if (len != fs.Length)
            {
                Logger.ErrorFormat(
                    CultureInfo.InvariantCulture,
                    "Reading from {0} didn't read all data in buffer. Requested to read {1} bytes, but was read {2} bytes",
                    filePath,
                    fs.Length,
                    len);
            }

            // Create Zip Entry
            zipFile.CreateEntryFromFile(FixPath(filePath), Path.Combine(folder, fileName));
			Logger.LogError(">>> Entering AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder})", zipFile, filePath, fileName, folder);
			Logger.LogWarning("<<< Exiting AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder})", zipFile, filePath, fileName, folder);
        }

        /// <summary>Adds a File to a Zip File.</summary>
        /// <param name="zipFile">The Zip File to add to.</param>
        /// <param name="filePath">The path to the file to add.</param>
        /// <param name="fileName">The name of the file to add.</param>
        /// <param name="folder">The name of the folder to use for the zip entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task AddToZipAsync(ZipArchive zipFile, string filePath, string fileName, string folder, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering AddToZipAsync(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder},cancellationToken=<redacted>)", zipFile, filePath, fileName, folder);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            using var fs = File.OpenRead(FixPath(filePath));

			Logger.LogError("AddToZipAsync(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder},cancellationToken=<redacted>): fs → {Fs}", zipFile, filePath, fileName, folder, fs);
            // Read file into byte array buffer
            var buffer = new byte[fs.Length];

            var len = await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
			Logger.LogError("AddToZipAsync(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder},cancellationToken=<redacted>): len → {Len}", zipFile, filePath, fileName, folder, len);
            if (len != fs.Length)
            {
                Logger.ErrorFormat(
                    CultureInfo.InvariantCulture,
                    "Reading from {0} didn't read all data in buffer. Requested to read {1} bytes, but was read {2} bytes",
                    filePath,
                    fs.Length,
                    len);
            }

            // Create Zip Entry
            zipFile.CreateEntryFromFile(FixPath(filePath), Path.Combine(folder, fileName));
			Logger.LogError("AddToZipAsync(zipFile,filePath,fileName,folder,cancellationToken)={ZipFile},{FilePath},{FileName},{Folder},<redacted>: external service call took {Arg5} ms", zipFile, filePath, fileName, folder, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddToZipAsync(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder},cancellationToken=<redacted>)", zipFile, filePath, fileName, folder);
        }

        /// <summary>Tries to copy a file in the file system.</summary>
        /// <param name="sourceFileName">The name of the source file.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        public static void CopyFile(string sourceFileName, string destFileName)
        {
			Logger.LogError(">>> Entering CopyFile(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
            if (File.Exists(destFileName))
            {
                File.SetAttributes(destFileName, FileAttributes.Normal);
            }

            File.Copy(sourceFileName, destFileName, true);
			Logger.LogWarning("<<< Exiting CopyFile(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        }

        /// <summary>Tries to copy a file in the file system.</summary>
        /// <param name="sourceFileName">The name of the source file.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task CopyFileAsync(string sourceFileName, string destFileName)
        {
			Logger.LogError(">>> Entering CopyFileAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
            if (await File.ExistsAsync(destFileName))
            {
                await File.SetAttributesAsync(destFileName, FileAttributes.Normal);
            }

            await File.CopyAsync(sourceFileName, destFileName, true);
			Logger.LogWarning("<<< Exiting CopyFileAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        }

        /// <summary>
        /// Deletes file in areas with a high degree of concurrent file access (i.e. caching, logging).
        /// This solves file concurrency issues under heavy load.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="waitInMilliseconds">The number of milliseconds to wait.</param>
        /// <param name="maxAttempts">The maximum number of attempts.</param>
        /// <returns>Whether the file is deleted.</returns>
        public static bool DeleteFileWithWait(string fileName, short waitInMilliseconds, short maxAttempts)
        {
			Logger.LogError(">>> Entering DeleteFileWithWait(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts})", fileName, waitInMilliseconds, maxAttempts);
            fileName = FixPath(fileName);
			Logger.LogError("DeleteFileWithWait(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts}): fileName → {FileName1}", fileName, waitInMilliseconds, maxAttempts, fileName);
            if (!File.Exists(fileName))
            {
                return true;
            }

            var fileDeleted = false;
			Logger.LogError("DeleteFileWithWait(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts}): fileDeleted → {FileDeleted}", fileName, waitInMilliseconds, maxAttempts, fileDeleted);
            var i = 0;
            while (fileDeleted != true)
            {
                if (i > maxAttempts)
                {
                    break;
                }

                i = i + 1;
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    fileDeleted = true; // we don't care if it didn't exist...the operation didn't fail, that's what we care about
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    fileDeleted = false;
                }

                if (fileDeleted == false)
                {
                    Thread.Sleep(waitInMilliseconds);
                }
            }

			Logger.LogWarning("<<< Exiting DeleteFileWithWait(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts})", fileName, waitInMilliseconds, maxAttempts);
            return fileDeleted;
        }

        /// <summary>
        /// Deletes file in areas with a high degree of concurrent file access (i.e. caching, logging).
        /// This solves file concurrency issues under heavy load.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="waitInMilliseconds">The number of milliseconds to wait.</param>
        /// <param name="maxAttempts">The maximum number of attempts.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Whether the file is deleted.</returns>
        public static async Task<bool> DeleteFileWithWaitAsync(string fileName, short waitInMilliseconds, short maxAttempts, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering DeleteFileWithWaitAsync(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts},cancellationToken=<redacted>)", fileName, waitInMilliseconds, maxAttempts);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            fileName = FixPath(fileName);
			Logger.LogError("DeleteFileWithWaitAsync(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts},cancellationToken=<redacted>): fileName → {FileName1}", fileName, waitInMilliseconds, maxAttempts, fileName);
            if (!await File.ExistsAsync(fileName))
            {
                return true;
            }

            var fileDeleted = false;
			Logger.LogError("DeleteFileWithWaitAsync(fileName={FileName},waitInMilliseconds={WaitInMilliseconds},maxAttempts={MaxAttempts},cancellationToken=<redacted>): fileDeleted → {FileDeleted}", fileName, waitInMilliseconds, maxAttempts, fileDeleted);
            var i = 0;
            while (fileDeleted != true)
            {
                if (i > maxAttempts)
                {
                    break;
                }

                i = i + 1;
                try
                {
                    if (await File.ExistsAsync(fileName))
                    {
                        await File.DeleteAsync(fileName);
                    }

                    fileDeleted = true; // we don't care if it didn't exist...the operation didn't fail, that's what we care about
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    fileDeleted = false;
                }

                if (fileDeleted == false)
                {
                    await Task.Delay(waitInMilliseconds, cancellationToken);
                }
            }

            return fileDeleted;
        }

        /// <summary>Tries to delete a file from the file system.</summary>
        /// <param name="fileName">The name of the file.</param>
        public static void DeleteFile(string fileName)
        {
			Logger.LogError(">>> Entering DeleteFile(fileName={FileName})", fileName);
            fileName = FixPath(fileName);
			Logger.LogError("DeleteFile(fileName={FileName}): fileName → {FileName1}", fileName, fileName);
            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
			Logger.LogWarning("<<< Exiting DeleteFile(fileName={FileName})", fileName);
        }

        /// <summary>Tries to delete a file from the file system.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteFileAsync(string fileName)
        {
			Logger.LogError(">>> Entering DeleteFileAsync(fileName={FileName})", fileName);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            fileName = FixPath(fileName);
			Logger.LogError("DeleteFileAsync(fileName={FileName}): fileName → {FileName1}", fileName, fileName);
            if (await File.ExistsAsync(fileName))
            {
                await File.SetAttributesAsync(fileName, FileAttributes.Normal);
                await File.DeleteAsync(fileName);
				Logger.LogWarning("<<< Exiting DeleteFileAsync(fileName={FileName})", fileName);
				Logger.LogError("DeleteFileAsync(fileName)={FileName}: external service call took {Arg1} ms", fileName, __logStart.ElapsedMilliseconds);
            }
        }

        /// <summary>Reads a file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The string content of the file.</returns>
        public static string ReadFile(string filePath)
        {
			Logger.LogError(">>> Entering ReadFile(filePath={FilePath})", filePath);
            using var reader = File.OpenText(filePath);
			Logger.LogError("ReadFile(filePath={FilePath}): reader → {Reader}", filePath, reader);
			Logger.LogWarning("<<< Exiting ReadFile(filePath={FilePath})", filePath);
            return reader.ReadToEnd();
        }

        /// <summary>Reads a file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string content of the file.</returns>
        public static async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering ReadFileAsync(filePath={FilePath},cancellationToken=<redacted>)", filePath);
            using var reader = File.OpenText(filePath);
            return await reader.ReadToEndAsync();
        }

        /// <summary>Unzips a resources zip file.</summary>
        /// <param name="zipStream">The zip archive stream.</param>
        /// <param name="destPath">The destination path to extract to.</param>
        public static void UnzipResources(ZipArchive zipStream, string destPath)
        {
            try
            {
                UnzipResources(zipStream.FileEntries(), destPath);
            }
            finally
            {
                zipStream?.Dispose();
            }
			Logger.LogWarning("<<< Exiting UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath})", zipArchiveEntries, destPath);
        }

        /// <summary>Unzips a resources zip file.</summary>
        /// <param name="zipArchiveEntries">The zip entries to unzip.</param>
        /// <param name="destPath">The destination path to extract to.</param>
        public static void UnzipResources(IEnumerable<ZipArchiveEntry> zipArchiveEntries, string destPath)
        {
			Logger.LogError(">>> Entering UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath})", zipArchiveEntries, destPath);
            foreach (var zipEntry in zipArchiveEntries)
            {
                HtmlUtils.WriteKeepAlive();
                var localFileName = zipEntry.FullName;
                var relativeDir = Path.GetDirectoryName(zipEntry.FullName);
				Logger.LogError("UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath}): relativeDir → {RelativeDir}", zipArchiveEntries, destPath, relativeDir);
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    var destDir = Path.Combine(destPath, relativeDir);
					Logger.LogError("UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath}): destDir → {DestDir}", zipArchiveEntries, destPath, destDir);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.Create(destDir, true);
                    }
                }

                if (string.IsNullOrEmpty(localFileName))
                {
                    continue;
                }

                var fileNamePath = FixPath(Path.Combine(destPath, localFileName));
				Logger.LogError("UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath}): fileNamePath → {FileNamePath}", zipArchiveEntries, destPath, fileNamePath);
                try
                {
                    if (File.Exists(fileNamePath))
                    {
                        File.SetAttributes(fileNamePath, FileAttributes.Normal);
                        File.Delete(fileNamePath);
                    }

                    using var fileStream = File.Open(fileNamePath, FileMode.CreateNew);
					Logger.LogError("UnzipResources(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath}): fileStream → {FileStream}", zipArchiveEntries, destPath, fileStream);
                    zipEntry.Open().CopyToStream(fileStream, 25000);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>Unzips a resources zip file.</summary>
        /// <param name="zipStream">The zip archive stream.</param>
        /// <param name="destPath">The destination path to extract to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task UnzipResourcesAsync(ZipArchive zipStream, string destPath, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering UnzipResourcesAsync(zipStream={ZipStream},destPath={DestPath},cancellationToken=<redacted>)", zipStream, destPath);
            try
            {
                await UnzipResourcesAsync(zipStream.FileEntries(), destPath, cancellationToken);
            }
            finally
            {
                zipStream?.Dispose();
            }
			Logger.LogWarning("<<< Exiting UnzipResourcesAsync(zipStream={ZipStream},destPath={DestPath},cancellationToken=<redacted>)", zipStream, destPath);
        }

        /// <summary>Unzips a resources zip file.</summary>
        /// <param name="zipArchiveEntries">The zip archive entries to extract.</param>
        /// <param name="destPath">The destination path to extract to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task UnzipResourcesAsync(IEnumerable<ZipArchiveEntry> zipArchiveEntries, string destPath, CancellationToken cancellationToken)
        {
			Logger.LogError(">>> Entering UnzipResourcesAsync(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath},cancellationToken=<redacted>)", zipArchiveEntries, destPath);
            foreach (var zipEntry in zipArchiveEntries)
            {
				Logger.LogWarning("<<< Exiting UnzipResourcesAsync(zipStream={ZipStream},destPath={DestPath},cancellationToken=<redacted>)", zipStream, destPath);
                cancellationToken.ThrowIfCancellationRequested();
                HtmlUtils.WriteKeepAlive();

                var localFileName = zipEntry.FullName;
                var relativeDir = Path.GetDirectoryName(zipEntry.FullName);
				Logger.LogError("UnzipResourcesAsync(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath},cancellationToken=<redacted>): relativeDir → {RelativeDir}", zipArchiveEntries, destPath, relativeDir);
                if (!string.IsNullOrEmpty(relativeDir))
                {
                    var destDir = Path.Combine(destPath, relativeDir);
					Logger.LogError("UnzipResourcesAsync(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath},cancellationToken=<redacted>): destDir → {DestDir}", zipArchiveEntries, destPath, destDir);
                    if (!await Directory.ExistsAsync(destDir))
                    {
                        await Directory.CreateAsync(destDir, true);
                    }
                }

                if (string.IsNullOrEmpty(localFileName))
                {
                    continue;
                }

                var fileNamePath = FixPath(Path.Combine(destPath, localFileName));
				Logger.LogError("UnzipResourcesAsync(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath},cancellationToken=<redacted>): fileNamePath → {FileNamePath}", zipArchiveEntries, destPath, fileNamePath);
                try
                {
                    if (await File.ExistsAsync(fileNamePath))
                    {
                        await File.SetAttributesAsync(fileNamePath, FileAttributes.Normal);
                        await File.DeleteAsync(fileNamePath);
                    }

                    using var fileStream = File.Open(fileNamePath, FileMode.CreateNew);
					Logger.LogError("UnzipResourcesAsync(zipArchiveEntries={ZipArchiveEntries},destPath={DestPath},cancellationToken=<redacted>): fileStream → {FileStream}", zipArchiveEntries, destPath, fileStream);
                    await zipEntry.Open().CopyToAsync(fileStream, 25000, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>Deletes the files specified.</summary>
        /// <param name="arrPaths">An array of the file paths for the files to delete.</param>
        /// <returns>An empty string if succeeded or a list of errors in case of failures.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public static string DeleteFiles(Array arrPaths)
            => DeleteFiles(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), arrPaths);

        /// <summary>Deletes the files specified.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="arrPaths">An array of the file paths for the files to delete.</param>
        /// <returns>An empty string if succeeded or a list of errors in case of failures.</returns>
        public static string DeleteFiles(IApplicationStatusInfo appStatus, Array arrPaths)
        {
			Logger.LogError(">>> Entering DeleteFiles(arrPaths={ArrPaths})", arrPaths);
            var strExceptions = string.Empty;
            for (var i = 0; i < arrPaths.Length; i++)
            {
                var strPath = (arrPaths.GetValue(i) ?? string.Empty).ToString();
				Logger.LogError("DeleteFiles(arrPaths={ArrPaths}): strPath → {StrPath}", arrPaths, strPath);
				Logger.LogError("DeleteFiles(appStatus={AppStatus},arrPaths={ArrPaths}): strPath → {StrPath}", appStatus, arrPaths, strPath);
                var pos = strPath.IndexOf("'", StringComparison.Ordinal);
                if (pos != -1)
                {
					Logger.LogError("DeleteFiles(arrPaths={ArrPaths}): pos → {Pos}", arrPaths, pos);
					Logger.LogError("DeleteFiles(appStatus={AppStatus},arrPaths={ArrPaths}): pos → {Pos}", appStatus, arrPaths, pos);
                    // the (') represents a comment to the end of the line
                    strPath = strPath.Substring(0, pos);
                }

                strPath = FixPath(strPath).TrimStart('\\');
                if (string.IsNullOrEmpty(strPath))
                {
                    continue;
                }

                strPath = Path.Combine(appStatus.ApplicationMapPath, strPath);
                if (strPath.EndsWith(@"\", StringComparison.Ordinal) && Directory.Exists(strPath))
                {
                    var directoryInfo = new System.IO.DirectoryInfo(strPath);
					Logger.LogError("DeleteFiles(arrPaths={ArrPaths}): directoryInfo → {DirectoryInfo}", arrPaths, directoryInfo);
					Logger.LogError("DeleteFiles(appStatus={AppStatus},arrPaths={ArrPaths}): directoryInfo → {DirectoryInfo}", appStatus, arrPaths, directoryInfo);
                    var applicationPath = $@"{appStatus.ApplicationMapPath}\";
                    if (!directoryInfo.FullName.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase) ||
                        directoryInfo.FullName.Equals(applicationPath, StringComparison.OrdinalIgnoreCase))
                    {
						Logger.LogError("DeleteFiles(appStatus={AppStatus},arrPaths={ArrPaths}): applicationPath → {ApplicationPath}", appStatus, arrPaths, applicationPath);
						Logger.LogError("DeleteFiles(arrPaths={ArrPaths}): applicationPath → {ApplicationPath}", arrPaths, applicationPath);
                        continue;
                    }

                    try
                    {
                        Globals.DeleteFolderRecursive(strPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        strExceptions += $"Processing folder ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                    }
                }
                else
                {
                    if (!File.Exists(strPath))
                    {
                        continue;
                    }

                    try
                    {
                        File.SetAttributes(strPath, FileAttributes.Normal);
                        File.Delete(strPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        strExceptions += $"Processing file ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                    }
                }
            }

			Logger.LogWarning("<<< Exiting DeleteFiles(appStatus={AppStatus},arrPaths={ArrPaths})", appStatus, arrPaths);
            return strExceptions;
        }

        /// <summary>Deletes the files specified.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="arrPaths">An array of the file paths for the files to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty string if succeeded or a list of errors in case of failures.</returns>
        public static async Task<string> DeleteFilesAsync(IApplicationStatusInfo appStatus, Array arrPaths, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering DeleteFilesAsync(appStatus={AppStatus},arrPaths={ArrPaths},cancellationToken=<redacted>)", appStatus, arrPaths);
            var strExceptions = string.Empty;
            for (var i = 0; i < arrPaths.Length; i++)
            {
                var strPath = (arrPaths.GetValue(i) ?? string.Empty).ToString();
				Logger.LogError("DeleteFilesAsync(appStatus={AppStatus},arrPaths={ArrPaths},cancellationToken=<redacted>): strPath → {StrPath}", appStatus, arrPaths, strPath);
                var pos = strPath.IndexOf("'", StringComparison.Ordinal);
				Logger.LogError("DeleteFilesAsync(appStatus={AppStatus},arrPaths={ArrPaths},cancellationToken=<redacted>): pos → {Pos}", appStatus, arrPaths, pos);
                if (pos != -1)
                {
                    // the (') represents a comment to the end of the line
                    strPath = strPath.Substring(0, pos);
                }

                strPath = FixPath(strPath).TrimStart('\\');
                if (string.IsNullOrEmpty(strPath))
                {
                    continue;
                }

                strPath = Path.Combine(appStatus.ApplicationMapPath, strPath);
                if (strPath.EndsWith(@"\", StringComparison.Ordinal) && await Directory.ExistsAsync(strPath))
                {
                    var directoryInfo = new System.IO.DirectoryInfo(strPath);
					Logger.LogError("DeleteFilesAsync(appStatus={AppStatus},arrPaths={ArrPaths},cancellationToken=<redacted>): directoryInfo → {DirectoryInfo}", appStatus, arrPaths, directoryInfo);
                    var applicationPath = $@"{appStatus.ApplicationMapPath}\";
					Logger.LogError("DeleteFilesAsync(appStatus={AppStatus},arrPaths={ArrPaths},cancellationToken=<redacted>): applicationPath → {ApplicationPath}", appStatus, arrPaths, applicationPath);
                    if (!directoryInfo.FullName.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase) ||
                        directoryInfo.FullName.Equals(applicationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        await DeleteFolderRecursiveAsync(strPath, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        strExceptions += $"Processing folder ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                    }
                }
                else
                {
                    if (!await File.ExistsAsync(strPath))
                    {
                        continue;
                    }

                    try
                    {
                        await File.SetAttributesAsync(strPath, FileAttributes.Normal);
                        await File.DeleteAsync(strPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        strExceptions += $"Processing file ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                    }
                }
            }

            return strExceptions;
        }

        /// <summary>Deletes files that match a filter recursively within folders.</summary>
        /// <param name="strRoot">The root path to filter from.</param>
        /// <param name="filter">The filter to select the files to delete.</param>
        public static void DeleteFilesRecursive(string strRoot, string filter)
        {
			Logger.LogError(">>> Entering DeleteFilesRecursive(strRoot={StrRoot},filter={Filter})", strRoot, filter);
            if (string.IsNullOrEmpty(strRoot))
            {
                return;
            }

            strRoot = FixPath(strRoot);
			Logger.LogError("DeleteFilesRecursive(strRoot={StrRoot},filter={Filter}): strRoot → {StrRoot1}", strRoot, filter, strRoot);
            if (!Directory.Exists(strRoot))
            {
                return;
            }

            foreach (var strFolder in Directory.EnumerateDirectoryPaths(strRoot))
            {
                var directory = new DirectoryInfo(strFolder);
				Logger.LogError("DeleteFilesRecursive(strRoot={StrRoot},filter={Filter}): directory → {Directory}", strRoot, filter, directory);
                if ((directory.Attributes & FileAttributes.Hidden) == 0 && (directory.Attributes & FileAttributes.System) == 0)
                {
                    DeleteFilesRecursive(strFolder, filter);
                }
            }

            foreach (var strFile in Directory.EnumerateFilePaths(strRoot).Where(f => f.Contains(filter)))
            {
                try
                {
                    DeleteFile(strFile);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
			Logger.LogWarning("<<< Exiting DeleteFilesRecursive(strRoot={StrRoot},filter={Filter})", strRoot, filter);
        }

        /// <summary>Deletes files that match a filter recursively within folders.</summary>
        /// <param name="strRoot">The root path to filter from.</param>
        /// <param name="filter">The filter to select the files to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteFilesRecursiveAsync(string strRoot, string filter, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering DeleteFilesRecursiveAsync(strRoot={StrRoot},filter={Filter},cancellationToken=<redacted>)", strRoot, filter);
            if (string.IsNullOrEmpty(strRoot))
            {
                return;
            }

            strRoot = FixPath(strRoot);
			Logger.LogError("DeleteFilesRecursiveAsync(strRoot={StrRoot},filter={Filter},cancellationToken=<redacted>): strRoot → {StrRoot1}", strRoot, filter, strRoot);
            if (!await Directory.ExistsAsync(strRoot))
            {
                return;
            }

            foreach (var strFolder in await Directory.EnumerateDirectoryPathsAsync(strRoot))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directory = new DirectoryInfo(strFolder);
				Logger.LogError("DeleteFilesRecursiveAsync(strRoot={StrRoot},filter={Filter},cancellationToken=<redacted>): directory → {Directory}", strRoot, filter, directory);
                if ((directory.Attributes & FileAttributes.Hidden) == 0 && (directory.Attributes & FileAttributes.System) == 0)
                {
                    await DeleteFilesRecursiveAsync(strFolder, filter, cancellationToken);
                }
            }

            foreach (var strFile in (await Directory.EnumerateFilePathsAsync(strRoot)).Where(f => f.Contains(filter)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await DeleteFileAsync(strFile);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
			Logger.LogWarning("<<< Exiting DeleteFilesRecursiveAsync(strRoot={StrRoot},filter={Filter},cancellationToken=<redacted>)", strRoot, filter);
        }

        /// <summary>Deletes a folder and all its child files and folders.</summary>
        /// <param name="strRoot">The root path to delete from.</param>
        public static void DeleteFolderRecursive(string strRoot)
        {
			Logger.LogError(">>> Entering DeleteFolderRecursive(strRoot={StrRoot})", strRoot);
            strRoot = FixPath(strRoot);
            if (string.IsNullOrEmpty(strRoot) || !Directory.Exists(strRoot))
            {
                Logger.Info($"{strRoot} does not exist. ");
                return;
            }

            foreach (var strFolder in Directory.EnumerateDirectoryPaths(strRoot))
            {
                DeleteFolderRecursive(strFolder);
            }

            foreach (var strFile in Directory.EnumerateFilePaths(strRoot))
            {
                try
                {
                    DeleteFile(strFile);
                }
                catch (Exception ex)
                {
                    Logger.Info($"{strRoot} does not exist.");
                    Logger.Error(ex);
                }
            }

            try
            {
                Directory.SetAttributes(strRoot, FileAttributes.Normal);
                Directory.Delete(strRoot);
            }
            catch (Exception ex)
            {
                Logger.Info($"{strRoot} does not exist.");
                Logger.Error(ex);
            }
        }

        /// <summary>Deletes a folder and all its child files and folders.</summary>
        /// <param name="strRoot">The root path to delete from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteFolderRecursiveAsync(string strRoot, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering DeleteFolderRecursiveAsync(strRoot={StrRoot},cancellationToken=<redacted>)", strRoot);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            strRoot = FixPath(strRoot);
            if (string.IsNullOrEmpty(strRoot) || !await Directory.ExistsAsync(strRoot))
            {
                Logger.Info($"{strRoot} does not exist. ");
                return;
            }

            foreach (var strFolder in await Directory.EnumerateDirectoryPathsAsync(strRoot))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DeleteFolderRecursiveAsync(strFolder, cancellationToken);
            }

            foreach (var strFile in await Directory.EnumerateFilePathsAsync(strRoot))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await DeleteFileAsync(strFile);
                }
                catch (Exception ex)
                {
                    Logger.Info($"{strRoot} does not exist.");
                    Logger.Error(ex);
                }
            }

            try
            {
                await Directory.SetAttributesAsync(strRoot, FileAttributes.Normal);
                await Directory.DeleteAsync(strRoot);
            }
            catch (Exception ex)
            {
                Logger.Info($"{strRoot} does not exist.");
                Logger.Error(ex);
				Logger.LogError("DeleteFolderRecursiveAsync(strRoot,cancellationToken)={StrRoot},<redacted>: external service call took {Arg2} ms", strRoot, __logStart.ElapsedMilliseconds);
			Logger.LogWarning("<<< Exiting DeleteFolderRecursiveAsync(strRoot={StrRoot},cancellationToken=<redacted>)", strRoot);
            }
        }

        /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
        /// <param name="path">The root folder path.</param>
        public static void DeleteEmptyFoldersRecursive(string path)
        {
			Logger.LogError(">>> Entering DeleteEmptyFoldersRecursive(path={Path})", path);
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                Logger.Info($"{path} does not exist.");
                return;
            }

            // first take care of folders
            foreach (var folder in Directory.EnumerateDirectoryPaths(path))
            {
                DeleteEmptyFoldersRecursive(folder);
            }

            // if any files or folders left, return
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                return;
            }

            try
            {
                // delete this empty folder
                Directory.SetAttributes(path, FileAttributes.Normal);
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteEmptyFoldersRecursive(path={Path})", path);
        }

        /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
        /// <param name="path">The root folder path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteEmptyFoldersRecursiveAsync(string path, CancellationToken cancellationToken = default)
        {
			Logger.LogError(">>> Entering DeleteEmptyFoldersRecursiveAsync(path={Path},cancellationToken=<redacted>)", path);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (string.IsNullOrWhiteSpace(path) || !await Directory.ExistsAsync(path))
            {
                Logger.Info($"{path} does not exist.");
                return;
            }

            // first take care of folders
            foreach (var folder in await Directory.EnumerateDirectoryPathsAsync(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DeleteEmptyFoldersRecursiveAsync(folder, cancellationToken);
            }

            // if any files or folders left, return
            if ((await Directory.EnumerateFileSystemEntriesAsync(path)).Any())
            {
                return;
            }

            try
            {
                // delete this empty folder
                await Directory.SetAttributesAsync(path, FileAttributes.Normal);
                await Directory.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogError("DeleteEmptyFoldersRecursiveAsync(path,cancellationToken)={Path},<redacted>: external service call took {Arg2} ms", path, __logStart.ElapsedMilliseconds);
            }
			Logger.LogWarning("<<< Exiting DeleteEmptyFoldersRecursiveAsync(path={Path},cancellationToken=<redacted>)", path);
        }

        /// <summary>Fixes the path in case the path separator is not windows style.</summary>
        /// <param name="input">The path to fix.</param>
        /// <returns>A valid Windows path.</returns>
        public static string FixPath(string input)
        {
			Logger.LogError(">>> Entering FixPath(input={Input})", input);
            Logger.LogWarning("<<< Exiting FixPath(input={Input})", input);
            return string.IsNullOrEmpty(input) ? input : input.Trim().Replace("/", @"\");
        }

        /// <summary>Adds a file to a zip.</summary>
        /// <param name="zipFile">The zip file stream to add to.</param>
        /// <param name="filePath">The path to the file to add.</param>
        /// <param name="fileName">Name of the file to use in the zip entry.</param>
        /// <param name="folder">The name of the folder to use in the zip entry.</param>
        [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static partial void AddToZip(ref ZipOutputStream zipFile, string filePath, string fileName, string folder)
#pragma warning restore CS3001
        {
            FileStream fs = null;
			Logger.LogError("AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder}): fs → {Fs}", zipFile, filePath, fileName, folder, fs);
            try
            {
                // Open File Stream
                fs = File.OpenRead(FixPath(filePath));

                // Read file into byte array buffer
                var buffer = new byte[fs.Length];

                var len = fs.Read(buffer, 0, buffer.Length);
				Logger.LogError("AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder}): len → {Len}", zipFile, filePath, fileName, folder, len);
                if (len != fs.Length)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "Reading from {0} didn't read all data in buffer. Requested to read {1} bytes, but was read {2} bytes",
                        filePath,
                        fs.Length,
                        len);
                }

                // Create Zip Entry
                var entry = new ZipEntry(Path.Combine(folder, fileName));
				Logger.LogError("AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder}): entry → {Entry}", zipFile, filePath, fileName, folder, entry);
				Logger.LogDebug("AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder}): entry.Size → {Arg4}", zipFile, filePath, fileName, folder, entry.Size);
                entry.DateTime = DateTime.Now;
				Logger.LogDebug("AddToZip(zipFile={ZipFile},filePath={FilePath},fileName={FileName},folder={Folder}): entry.DateTime → {Arg4}", zipFile, filePath, fileName, folder, entry.DateTime);
                entry.Size = fs.Length;
                fs.Close();

                // Compress file and add to Zip file
                zipFile.PutNextEntry(entry);
                zipFile.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        /// <summary>Unzips a resources zip file.</summary>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="destPath">The destination path to extract to.</param>
        [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static partial void UnzipResources(ZipInputStream zipStream, string destPath)
#pragma warning restore CS3001
        {
			Logger.LogError(">>> Entering UnzipResources(zipStream={ZipStream},destPath={DestPath})", zipStream, destPath);
            try
            {
                var zipEntry = zipStream.GetNextEntry();
				Logger.LogError("UnzipResources(zipStream={ZipStream},destPath={DestPath}): zipEntry → {ZipEntry}", zipStream, destPath, zipEntry);
                while (zipEntry != null)
                {
                    zipEntry.CheckZipEntry();
                    HtmlUtils.WriteKeepAlive();
                    var localFileName = zipEntry.Name;
                    var relativeDir = Path.GetDirectoryName(zipEntry.Name);
					Logger.LogError("UnzipResources(zipStream={ZipStream},destPath={DestPath}): relativeDir → {RelativeDir}", zipStream, destPath, relativeDir);
                    if (!string.IsNullOrEmpty(relativeDir) && (!Directory.Exists(Path.Combine(destPath, relativeDir))))
                    {
                        Directory.Create(Path.Combine(destPath, relativeDir), true);
                    }

                    if (!zipEntry.IsDirectory && (!string.IsNullOrEmpty(localFileName)))
                    {
                        var fileNamePath = FixPath(Path.Combine(destPath, localFileName));
						Logger.LogError("UnzipResources(zipStream={ZipStream},destPath={DestPath}): fileNamePath → {FileNamePath}", zipStream, destPath, fileNamePath);
                        try
                        {
                            if (File.Exists(fileNamePath))
                            {
                                File.SetAttributes(fileNamePath, FileAttributes.Normal);
                                File.Delete(fileNamePath);
                            }

                            FileStream objFileStream = null;
							Logger.LogError("UnzipResources(zipStream={ZipStream},destPath={DestPath}): objFileStream → {ObjFileStream}", zipStream, destPath, objFileStream);
                            try
                            {
                                File.Create(fileNamePath);
                                objFileStream = File.Open(fileNamePath);
                                var arrData = new byte[2048];
                                var intSize = zipStream.Read(arrData, 0, arrData.Length);
								Logger.LogError("UnzipResources(zipStream={ZipStream},destPath={DestPath}): intSize → {IntSize}", zipStream, destPath, intSize);
                                while (intSize > 0)
                                {
                                    objFileStream.Write(arrData, 0, intSize);
                                    intSize = zipStream.Read(arrData, 0, arrData.Length);
                                }
                            }
                            finally
                            {
                                if (objFileStream != null)
                                {
                                    objFileStream.Close();
                                    objFileStream.Dispose();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }

                    zipEntry = zipStream.GetNextEntry();
                }
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
            }
			Logger.LogWarning("<<< Exiting UnzipResources(zipStream={ZipStream},destPath={DestPath})", zipStream, destPath);
        }
    }
}
