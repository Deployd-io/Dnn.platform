// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using DotNetNuke.ComponentModel;

using SchwabenCode.QuickIO;

/// <summary>The default <see cref="IDirectory"/> implementation.</summary>
public class DirectoryWrapper : ComponentBase<IDirectory, DirectoryWrapper>, IDirectory
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DirectoryWrapper));
    /// <inheritdoc />
    public void Delete(string path, bool recursive)
    {
		Logger.LogError(">>> Entering Delete(path={Path},recursive={Recursive})", path, recursive);
        if (this.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
		Logger.LogWarning("<<< Exiting Delete(path={Path},recursive={Recursive})", path, recursive);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string path, bool recursive)
    {
		Logger.LogError(">>> Entering DeleteAsync(path={Path},recursive={Recursive})", path, recursive);
        var __logStart = System.Diagnostics.Stopwatch.StartNew();
        if (await this.ExistsAsync(path))
        {
            await QuickIODirectory.DeleteAsync(path, recursive);
			Logger.LogWarning("<<< Exiting DeleteAsync(path={Path},recursive={Recursive})", path, recursive);
			Logger.LogError("DeleteAsync(path,recursive)={Path},{Recursive}: external service call took {Arg2} ms", path, recursive, __logStart.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc />
    public bool Exists(string path)
    {
		Logger.LogError(">>> Entering Exists(path={Path})", path);
        Logger.LogWarning("<<< Exiting Exists(path={Path})", path);
        return Directory.Exists(path);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path)
    {
		Logger.LogError(">>> Entering ExistsAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting ExistsAsync(path={Path})", path);
        return await QuickIODirectory.ExistsAsync(path);
    }

    /// <inheritdoc />
    public string[] GetDirectories(string path)
    {
		Logger.LogError(">>> Entering GetDirectories(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetDirectories(path={Path})", path);
        return Directory.GetDirectories(path);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetDirectoriesAsync(string path)
    {
		Logger.LogError(">>> Entering GetDirectoriesAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetDirectoriesAsync(path={Path})", path);
        return await QuickIODirectory.EnumerateDirectoryPathsAsync(path);
    }

    /// <inheritdoc />
    public string[] GetFiles(string path)
    {
		Logger.LogError(">>> Entering GetFiles(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetFiles(path={Path})", path);
        return Directory.GetFiles(path);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
		Logger.LogError(">>> Entering GetFilesAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetFilesAsync(path={Path})", path);
        return await QuickIODirectory.EnumerateFilePathsAsync(path);
    }

    /// <inheritdoc />
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(path, searchPattern, searchOption);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetFilesAsync(string path, string searchPattern, SearchOption searchOption)
    {
        return await QuickIODirectory.EnumerateFilePathsAsync(path, searchPattern, searchOption);
    }

    /// <inheritdoc />
    public void Move(string sourceDirName, string destDirName)
    {
		Logger.LogError(">>> Entering Move(sourceDirName={SourceDirName},destDirName={DestDirName})", sourceDirName, destDirName);
        Directory.Move(sourceDirName, destDirName);
		Logger.LogWarning("<<< Exiting Move(sourceDirName={SourceDirName},destDirName={DestDirName})", sourceDirName, destDirName);
    }

    /// <inheritdoc />
    public async Task MoveAsync(string sourceDirName, string destDirName)
    {
		Logger.LogError(">>> Entering MoveAsync(sourceDirName={SourceDirName},destDirName={DestDirName})", sourceDirName, destDirName);
        await QuickIODirectory.MoveAsync(sourceDirName, destDirName);
		Logger.LogWarning("<<< Exiting MoveAsync(sourceDirName={SourceDirName},destDirName={DestDirName})", sourceDirName, destDirName);
    }

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
		Logger.LogError(">>> Entering CreateDirectory(path={Path})", path);
        Directory.CreateDirectory(path);
		Logger.LogWarning("<<< Exiting CreateDirectory(path={Path})", path);
    }

    /// <inheritdoc />
    public async Task CreateDirectoryAsync(string path)
    {
		Logger.LogError(">>> Entering CreateDirectoryAsync(path={Path})", path);
        await QuickIODirectory.CreateAsync(path);
		Logger.LogWarning("<<< Exiting CreateDirectoryAsync(path={Path})", path);
    }

    /// <inheritdoc />
    public void CreateDirectory(string path, bool recursive)
    {
		Logger.LogError(">>> Entering CreateDirectory(path={Path},recursive={Recursive})", path, recursive);
        QuickIODirectory.Create(path, recursive);
		Logger.LogWarning("<<< Exiting CreateDirectory(path={Path},recursive={Recursive})", path, recursive);
    }

    /// <inheritdoc />
    public async Task CreateDirectoryAsync(string path, bool recursive)
    {
		Logger.LogError(">>> Entering CreateDirectoryAsync(path={Path},recursive={Recursive})", path, recursive);
        await QuickIODirectory.CreateAsync(path, recursive);
		Logger.LogWarning("<<< Exiting CreateDirectoryAsync(path={Path},recursive={Recursive})", path, recursive);
    }
}
