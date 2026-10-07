// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    using DotNetNuke.Internal.SourceGenerators;

    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>Extension methods for types related to the file system (e.g. <see cref="ZipArchiveEntry"/> and <see cref="Stream"/>).</summary>
    public static partial class FileSystemExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemExtensions));
        public static void CheckZipEntry(this ZipArchiveEntry input)
        {
            var fullName = input.FullName.Replace('\\', '/');
            if (fullName.StartsWith("..", StringComparison.Ordinal) || fullName.Contains("/../", StringComparison.Ordinal))
            {
                throw new IllegalZipFileException("Illegal Zip File");
            }
        }

        public static string ReadTextFile(this ZipArchiveEntry input)
        {
			Logger.LogError(">>> Entering ReadTextFile(input={Input})", input);
            var text = string.Empty;
            using var reader = new StreamReader(input.Open());
			Logger.LogError("ReadTextFile(input={Input}): reader → {Reader}", input, reader);
            text = reader.ReadToEnd();
			Logger.LogError("ReadTextFile(input={Input}): text → {Text}", input, text);

			Logger.LogWarning("<<< Exiting ReadTextFile(input={Input})", input);
            return text;
        }

        public static void CopyToStream(this Stream strIn, Stream strOut, int bufferSize)
        {
			Logger.LogError(">>> Entering CopyToStream(strIn={StrIn},strOut={StrOut},bufferSize={BufferSize})", strIn, strOut, bufferSize);
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = strIn.Read(buffer, 0, buffer.Length)) > 0)
            {
                strOut.Write(buffer, 0, bytesRead);
            }
			Logger.LogWarning("<<< Exiting CopyToStream(strIn={StrIn},strOut={StrOut},bufferSize={BufferSize})", strIn, strOut, bufferSize);
        }

        public static IEnumerable<ZipArchiveEntry> FileEntries(this ZipArchive zip)
        {
			Logger.LogError(">>> Entering FileEntries(zip={Zip})", zip);
            foreach (var entry in zip.Entries)
            {
                entry.CheckZipEntry();
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    yield return entry;
                }
            }
			Logger.LogWarning("<<< Exiting FileEntries(zip={Zip})", zip);
        }

        /// <summary>Check whether the <paramref name="input"/> is invalid.</summary>
        /// <param name="input">The entry to check.</param>
        /// <exception cref="Exception">Illegal zip file.</exception>
        [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static partial void CheckZipEntry(this ZipEntry input)
#pragma warning restore CS3001
        {
			Logger.LogError(">>> Entering CheckZipEntry(input={Input})", input);
            var fullName = input.Name.Replace('\\', '/');
			Logger.LogError("CheckZipEntry(input={Input}): fullName → {FullName}", input, fullName);
            if (fullName.StartsWith("..", StringComparison.Ordinal) || fullName.Contains("/../", StringComparison.Ordinal))
            {
				Logger.LogError("Throwing IllegalZipFileException from CheckZipEntry(input={Input})", input);
                throw new IllegalZipFileException("Illegal Zip File");
            }
			Logger.LogWarning("<<< Exiting CheckZipEntry(input={Input})", input);
        }
    }
}
