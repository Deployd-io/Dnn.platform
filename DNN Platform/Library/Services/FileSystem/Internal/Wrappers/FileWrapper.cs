// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DotNetNuke.ComponentModel;

using SchwabenCode.QuickIO;

/// <summary>The default <see cref="IFile"/> implementation.</summary>
public class FileWrapper : ComponentBase<IFile, FileWrapper>, IFile
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public Stream Create(string path)
    {
		Logger.LogDebug(">>> Entering Create(path={Path})", path);
        EnsureFileFolderExists(path);
		Logger.LogDebug("<<< Exiting Create(path={Path})", path);
        return File.Create(path);
    }

    /// <inheritdoc />
    public void Delete(string path)
    {
		Logger.LogDebug(">>> Entering Delete(path={Path})", path);
        File.Delete(path);
		Logger.LogDebug("<<< Exiting Delete(path={Path})", path);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string path)
    {
		Logger.LogDebug(">>> Entering DeleteAsync(path={Path})", path);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        await QuickIOFile.DeleteAsync(path);
		Logger.LogDebug("<<< Exiting DeleteAsync(path={Path})", path);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
		Logger.LogInformation("DeleteAsync(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
    }

	Logger.LogInformation("DeleteAsync(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
    /// <inheritdoc />
    public bool Exists(string path)
    {
		Logger.LogDebug(">>> Entering Exists(path={Path})", path);
		Logger.LogDebug("<<< Exiting Exists(path={Path})", path);
        return File.Exists(path);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path)
    {
		Logger.LogDebug(">>> Entering ExistsAsync(path={Path})", path);
		Logger.LogDebug("<<< Exiting ExistsAsync(path={Path})", path);
        return await QuickIOFile.ExistsAsync(path);
    }

    /// <inheritdoc />
    public FileAttributes GetAttributes(string path)
    {
		Logger.LogDebug(">>> Entering GetAttributes(path={Path})", path);
		Logger.LogDebug("<<< Exiting GetAttributes(path={Path})", path);
        return File.GetAttributes(path);
    }

    /// <inheritdoc />
    public async Task<FileAttributes> GetAttributesAsync(string path)
    {
		Logger.LogDebug(">>> Entering GetAttributesAsync(path={Path})", path);
		Logger.LogDebug("<<< Exiting GetAttributesAsync(path={Path})", path);
        return await QuickIOFile.GetAttributesAsync(path);
    }

    /// <inheritdoc />
    public DateTime GetLastWriteTime(string path)
    {
		Logger.LogDebug(">>> Entering GetLastWriteTime(path={Path})", path);
		Logger.LogDebug("<<< Exiting GetLastWriteTime(path={Path})", path);
        return File.GetLastWriteTime(path);
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLastWriteTimeAsync(string path)
    {
		Logger.LogDebug(">>> Entering GetLastWriteTimeAsync(path={Path})", path);
		Logger.LogDebug("<<< Exiting GetLastWriteTimeAsync(path={Path})", path);
        return await QuickIOFile.GetLastWriteTimeAsync(path);
    }

    /// <inheritdoc />
    public void Move(string sourceFileName, string destFileName)
    {
		Logger.LogDebug(">>> Entering Move(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        EnsureFileFolderExists(destFileName);
        File.Move(sourceFileName, destFileName);
		Logger.LogDebug("<<< Exiting Move(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
    }

    /// <inheritdoc />
    public async Task MoveAsync(string sourceFileName, string destFileName)
    {
		Logger.LogDebug(">>> Entering MoveAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        await EnsureFileFolderExistsAsync(destFileName);
        await QuickIOFile.MoveAsync(sourceFileName, destFileName);
		Logger.LogDebug("<<< Exiting MoveAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
    }

    /// <inheritdoc />
    public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
    {
		Logger.LogDebug(">>> Entering Copy(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
        EnsureFileFolderExists(destinationFileName);
        File.Copy(sourceFileName, destinationFileName, overwrite);
		Logger.LogDebug("<<< Exiting Copy(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
    }

    /// <inheritdoc />
    public async Task CopyAsync(string sourceFileName, string destinationFileName, bool overwrite)
    {
		Logger.LogDebug(">>> Entering CopyAsync(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
        await EnsureFileFolderExistsAsync(destinationFileName);
        await QuickIOFile.CopyAsync(sourceFileName, destinationFileName, overwrite);
		Logger.LogDebug("<<< Exiting CopyAsync(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
    }

    /// <inheritdoc />
    public Stream OpenRead(string path)
    {
		Logger.LogDebug(">>> Entering OpenRead(path={Path})", path);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
		Logger.LogInformation("OpenRead(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
		Logger.LogDebug("<<< Exiting OpenRead(path={Path})", path);
        return File.OpenRead(path);
    }

    /// <inheritdoc />
    public byte[] ReadAllBytes(string path)
    {
		Logger.LogDebug(">>> Entering ReadAllBytes(path={Path})", path);
		Logger.LogDebug("<<< Exiting ReadAllBytes(path={Path})", path);
        return File.ReadAllBytes(path);
    }

    /// <inheritdoc />
    public async Task<byte[]> ReadAllBytesAsync(string path)
    {
		Logger.LogDebug(">>> Entering ReadAllBytesAsync(path={Path})", path);
		Logger.LogDebug("<<< Exiting ReadAllBytesAsync(path={Path})", path);
        return await new QuickIOFileInfo(path).ReadAllBytesAsync();
    }

    /// <inheritdoc />
    public void SetAttributes(string path, FileAttributes fileAttributes)
    {
		Logger.LogDebug(">>> Entering SetAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        File.SetAttributes(path, fileAttributes);
		Logger.LogDebug("<<< Exiting SetAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
    }

    /// <inheritdoc />
    public async Task SetAttributesAsync(string path, FileAttributes fileAttributes)
    {
		Logger.LogDebug(">>> Entering SetAttributesAsync(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        await QuickIOFile.SetAttributesAsync(path, fileAttributes);
		Logger.LogDebug("<<< Exiting SetAttributesAsync(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
    }

    /// <inheritdoc />
    public void WriteAllText(string path, string contents)
    {
		Logger.LogDebug(">>> Entering WriteAllText(path={Path},contents={Contents})", path, contents);
        File.WriteAllText(path, contents);
		Logger.LogDebug("<<< Exiting WriteAllText(path={Path},contents={Contents})", path, contents);
    }

    /// <inheritdoc />
    public async Task WriteAllTextAsync(string path, string contents)
    {
		Logger.LogDebug(">>> Entering WriteAllTextAsync(path={Path},contents={Contents})", path, contents);
        using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
        await streamWriter.WriteAsync(contents);
		Logger.LogDebug("<<< Exiting WriteAllTextAsync(path={Path},contents={Contents})", path, contents);
		Logger.LogDebug("WriteAllTextAsync(path={Path},contents={Contents}): streamWriter → {StreamWriter}", path, contents, streamWriter);
    }

    private static void EnsureFileFolderExists(string filePath)
    {
		Logger.LogDebug(">>> Entering EnsureFileFolderExists(filePath={FilePath})", filePath);
        var fi = new FileInfo(filePath);
        if (fi.Directory != null && !fi.Directory.Exists)
		Logger.LogDebug("EnsureFileFolderExists(filePath={FilePath}): fi → {Fi}", filePath, fi);
        {
            fi.Directory.Create();
        }
		Logger.LogDebug("<<< Exiting EnsureFileFolderExists(filePath={FilePath})", filePath);
    }

    private static async Task EnsureFileFolderExistsAsync(string filePath)
    {
		Logger.LogDebug(">>> Entering EnsureFileFolderExistsAsync(filePath={FilePath})", filePath);
        var directoryName = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryName) && !await QuickIODirectory.ExistsAsync(directoryName))
		Logger.LogDebug("EnsureFileFolderExistsAsync(filePath={FilePath}): directoryName → {DirectoryName}", filePath, directoryName);
        {
            await QuickIODirectory.CreateAsync(directoryName);
        }
		Logger.LogDebug("<<< Exiting EnsureFileFolderExistsAsync(filePath={FilePath})", filePath);
    }
}
