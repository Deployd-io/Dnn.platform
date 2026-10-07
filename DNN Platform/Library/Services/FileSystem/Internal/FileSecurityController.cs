// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Internal class to check file security.</summary>
    public class FileSecurityController(IServiceProvider serviceProvider) : ServiceLocator<IFileSecurityController, FileSecurityController>, IFileSecurityController
    {
        private const int BufferSize = 4096;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSecurityController));
        private readonly IServiceProvider serviceProvider = serviceProvider ?? Globals.GetCurrentServiceProvider();

        /// <summary>Initializes a new instance of the <see cref="FileSecurityController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public FileSecurityController()
            : this(null)
        {
		Logger.LogError(">>> Entering FileSecurityController()");
        Logger.LogWarning("<<< Exiting FileSecurityController()");
        }

        /// <inheritdoc />
        public bool Validate(string fileName, Stream fileContent)
        {
			Logger.LogError(">>> Entering Validate(fileName={FileName},fileContent={FileContent})", fileName, fileContent);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("fileContent", fileContent);

            var extension = Path.GetExtension(fileName);
			Logger.LogError("Validate(fileName={FileName},fileContent={FileContent}): extension → {Extension}", fileName, fileContent, extension);
            var checker = this.GetSecurityChecker(extension?.ToLowerInvariant().TrimStart('.'));
			Logger.LogError("Validate(fileName={FileName},fileContent={FileContent}): checker → {Checker}", fileName, fileContent, checker);

            // when there is no specific file check for the file type, then treat it as validated.
            if (checker == null)
            {
                return true;
            }

            // use copy of the stream as we can't make sure how the check process the stream.
            using var copyStream = CopyStream(fileContent);
			Logger.LogError("Validate(fileName={FileName},fileContent={FileContent}): copyStream → {CopyStream}", fileName, fileContent, copyStream);
			Logger.LogWarning("<<< Exiting Validate(fileName={FileName},fileContent={FileContent})", fileName, fileContent);
            return checker.Validate(copyStream);
        }

        /// <inheritdoc />
        public bool ValidateNotExectuable(Stream fileContent)
        {
			Logger.LogError(">>> Entering ValidateNotExectuable(fileContent={FileContent})", fileContent);
            Requires.NotNull("fileContent", fileContent);

            var firstBytes = new byte[2];
            int bytesRead = fileContent.Read(firstBytes, 0, 2);
			Logger.LogError("ValidateNotExectuable(fileContent={FileContent}): bytesRead → {BytesRead}", fileContent, bytesRead);
            fileContent.Position = 0;
			Logger.LogDebug("ValidateNotExectuable(fileContent={FileContent}): fileContent.Position → {Arg1}", fileContent, fileContent.Position);

            // Windows executable files start with 0x4D 0x5A
			Logger.LogWarning("<<< Exiting ValidateNotExectuable(fileContent={FileContent})", fileContent);
            return bytesRead < 2 || firstBytes[0] != 0x4D || firstBytes[1] != 0x5A;
        }

        /// <inheritdoc />
        protected override Func<IFileSecurityController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFileSecurityController>();
        }

        private static Stream CopyStream(Stream stream)
        {
			Logger.LogError(">>> Entering CopyStream(stream={Stream})", stream);
            var folderPath = ((FileManager)FileManager.Instance).GetHostMapPath();
            string filePath;
			Logger.LogError("CopyStream(stream={Stream}): folderPath → {FolderPath}", stream, folderPath);
            do
            {
                filePath = Path.Combine(folderPath, Path.GetRandomFileName()) + ".resx";
				Logger.LogError("CopyStream(stream={Stream}): filePath → {FilePath}", stream, filePath);
				Logger.LogDebug("CopyStream(stream={Stream}): filePath → {FilePath}", stream, filePath);
            }
            while (File.Exists(filePath));

            var fileStream = ((FileManager)FileManager.Instance).GetAutoDeleteFileStream(filePath);
			Logger.LogError("CopyStream(stream={Stream}): fileStream → {FileStream}", stream, fileStream);

            var array = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = stream.Read(array, 0, BufferSize)) > 0)
            {
                fileStream.Write(array, 0, bytesRead);
            }

            stream.Position = 0;
			Logger.LogDebug("CopyStream(stream={Stream}): stream.Position → {Arg1}", stream, stream.Position);
            fileStream.Position = 0;
			Logger.LogDebug("CopyStream(stream={Stream}): fileStream.Position → {Arg1}", stream, fileStream.Position);

			Logger.LogWarning("<<< Exiting CopyStream(stream={Stream})", stream);
            return fileStream;
        }

        private IFileSecurityChecker GetSecurityChecker(string extension)
        {
			Logger.LogError(">>> Entering GetSecurityChecker(extension={Extension})", extension);
            var listController = ActivatorUtilities.GetServiceOrCreateInstance<ListController>(this.serviceProvider);
            var listEntry = listController.GetListEntryInfo("FileSecurityChecker", extension);
            if (listEntry != null && !string.IsNullOrEmpty(listEntry.Text))
            {
                try
                {
                    var cacheKey = $"FileSecurityChecker_{extension}";
                    return Reflection.CreateObject(this.serviceProvider, listEntry.Text, cacheKey) as IFileSecurityChecker;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Create File Security Checker for '{extension}' failed.", ex);
                }
            }

			Logger.LogWarning("<<< Exiting GetSecurityChecker(extension={Extension})", extension);
            return null;
        }
    }
}
