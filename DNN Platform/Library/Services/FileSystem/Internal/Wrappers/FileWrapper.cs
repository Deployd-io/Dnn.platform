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
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileWrapper));
    /// <inheritdoc />
    public Stream Create(string path)
    {
		Logger.LogError(">>> Entering Create(path={Path})", path);
        EnsureFileFolderExists(path);
		Logger.LogWarning("<<< Exiting Create(path={Path})", path);
        return File.Create(path);
    }

    /// <inheritdoc />
    public void Delete(string path)
    {
		Logger.LogError(">>> Entering Delete(path={Path})", path);
        File.Delete(path);
		Logger.LogWarning("<<< Exiting Delete(path={Path})", path);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string path)
    {
		Logger.LogError(">>> Entering DeleteAsync(path={Path})", path);
        var __logStart = System.Diagnostics.Stopwatch.StartNew();
        await QuickIOFile.DeleteAsync(path);
		Logger.LogError("DeleteAsync(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
    Logger.LogWarning("<<< Exiting DeleteAsync(path={Path})", path);
    }

    /// <inheritdoc />
    public bool Exists(string path)
    {
		Logger.LogError(">>> Entering Exists(path={Path})", path);
        Logger.LogWarning("<<< Exiting Exists(path={Path})", path);
        return File.Exists(path);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path)
    {
		Logger.LogError(">>> Entering ExistsAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting ExistsAsync(path={Path})", path);
        return await QuickIOFile.ExistsAsync(path);
    }

    /// <inheritdoc />
    public FileAttributes GetAttributes(string path)
    {
		Logger.LogError(">>> Entering GetAttributes(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetAttributes(path={Path})", path);
        return File.GetAttributes(path);
    }

    /// <inheritdoc />
    public async Task<FileAttributes> GetAttributesAsync(string path)
    {
		Logger.LogError(">>> Entering GetAttributesAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetAttributesAsync(path={Path})", path);
        return await QuickIOFile.GetAttributesAsync(path);
    }

    /// <inheritdoc />
    public DateTime GetLastWriteTime(string path)
    {
		Logger.LogError(">>> Entering GetLastWriteTime(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetLastWriteTime(path={Path})", path);
        return File.GetLastWriteTime(path);
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLastWriteTimeAsync(string path)
    {
		Logger.LogError(">>> Entering GetLastWriteTimeAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting GetLastWriteTimeAsync(path={Path})", path);
        return await QuickIOFile.GetLastWriteTimeAsync(path);
    }

    /// <inheritdoc />
    public void Move(string sourceFileName, string destFileName)
    {
		Logger.LogError(">>> Entering Move(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        EnsureFileFolderExists(destFileName);
        File.Move(sourceFileName, destFileName);
		Logger.LogWarning("<<< Exiting Move(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
    }

    /// <inheritdoc />
    public async Task MoveAsync(string sourceFileName, string destFileName)
    {
		Logger.LogError(">>> Entering MoveAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
        await EnsureFileFolderExistsAsync(destFileName);
        await QuickIOFile.MoveAsync(sourceFileName, destFileName);
		Logger.LogWarning("<<< Exiting MoveAsync(sourceFileName={SourceFileName},destFileName={DestFileName})", sourceFileName, destFileName);
    }

    /// <inheritdoc />
    public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
    {
		Logger.LogError(">>> Entering Copy(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
        EnsureFileFolderExists(destinationFileName);
        File.Copy(sourceFileName, destinationFileName, overwrite);
		Logger.LogWarning("<<< Exiting Copy(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
    }

    /// <inheritdoc />
    public async Task CopyAsync(string sourceFileName, string destinationFileName, bool overwrite)
    {
		Logger.LogError(">>> Entering CopyAsync(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
        await EnsureFileFolderExistsAsync(destinationFileName);
        await QuickIOFile.CopyAsync(sourceFileName, destinationFileName, overwrite);
		Logger.LogWarning("<<< Exiting CopyAsync(sourceFileName={SourceFileName},destinationFileName={DestinationFileName},overwrite={Overwrite})", sourceFileName, destinationFileName, overwrite);
    }

    /// <inheritdoc />
    public Stream OpenRead(string path)
    {
		Logger.LogError(">>> Entering OpenRead(path={Path})", path);
        var __logStart = System.Diagnostics.Stopwatch.StartNew();
		Logger.LogError("OpenRead(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
    Logger.LogWarning("<<< Exiting OpenRead(path={Path})", path);
        return File.OpenRead(path);
    }

    /// <inheritdoc />
    public byte[] ReadAllBytes(string path)
    {
		Logger.LogError(">>> Entering ReadAllBytes(path={Path})", path);
        Logger.LogWarning("<<< Exiting ReadAllBytes(path={Path})", path);
        return File.ReadAllBytes(path);
    }

    /// <inheritdoc />
    public async Task<byte[]> ReadAllBytesAsync(string path)
    {
		Logger.LogError(">>> Entering ReadAllBytesAsync(path={Path})", path);
        Logger.LogWarning("<<< Exiting ReadAllBytesAsync(path={Path})", path);
        return await new QuickIOFileInfo(path).ReadAllBytesAsync();
    }

    /// <inheritdoc />
    public void SetAttributes(string path, FileAttributes fileAttributes)
    {
		Logger.LogError(">>> Entering SetAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        File.SetAttributes(path, fileAttributes);
		Logger.LogWarning("<<< Exiting SetAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
    }

    /// <inheritdoc />
    public async Task SetAttributesAsync(string path, FileAttributes fileAttributes)
    {
		Logger.LogError(">>> Entering SetAttributesAsync(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        await QuickIOFile.SetAttributesAsync(path, fileAttributes);
		Logger.LogWarning("<<< Exiting SetAttributesAsync(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
    }

    /// <inheritdoc />
    public void WriteAllText(string path, string contents)
    {
		Logger.LogError(">>> Entering WriteAllText(path={Path},contents={Contents})", path, contents);
        File.WriteAllText(path, contents);
		Logger.LogWarning("<<< Exiting WriteAllText(path={Path},contents={Contents})", path, contents);
    }

    /// <inheritdoc />
    public async Task WriteAllTextAsync(string path, string contents)
    {
		Logger.LogError(">>> Entering WriteAllTextAsync(path={Path},contents={Contents})", path, contents);
        using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
		Logger.LogWarning("<<< Exiting WriteAllTextAsync(path={Path},contents={Contents})", path, contents);
		Logger.LogError("WriteAllTextAsync(path={Path},contents={Contents}): streamWriter → {StreamWriter}", path, contents, streamWriter);
        await streamWriter.WriteAsync(contents);
    }

    private static void EnsureFileFolderExists(string filePath)
    {
		Logger.LogError(">>> Entering EnsureFileFolderExists(filePath={FilePath})", filePath);
        var fi = new FileInfo(filePath);
		Logger.LogError("EnsureFileFolderExists(filePath={FilePath}): fi → {Fi}", filePath, fi);
        if (fi.Directory != null && !fi.Directory.Exists)
        {
            fi.Directory.Create();
        }
		Logger.LogWarning("<<< Exiting EnsureFileFolderExists(filePath={FilePath})", filePath);
    }

    private static async Task EnsureFileFolderExistsAsync(string filePath)
    {
		Logger.LogError(">>> Entering EnsureFileFolderExistsAsync(filePath={FilePath})", filePath);
        var directoryName = Path.GetDirectoryName(filePath);
		Logger.LogError("EnsureFileFolderExistsAsync(filePath={FilePath}): directoryName → {DirectoryName}", filePath, directoryName);
        if (!string.IsNullOrEmpty(directoryName) && !await QuickIODirectory.ExistsAsync(directoryName))
        {
            await QuickIODirectory.CreateAsync(directoryName);
		Logger.LogWarning("<<< Exiting EnsureFileFolderExistsAsync(filePath={FilePath})", filePath);
        }
    }
}
