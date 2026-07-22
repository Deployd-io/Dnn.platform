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
		Logger.LogDebug(">>> Entering FileSecurityController(serviceProvider={ServiceProvider})", serviceProvider);
        private const int BufferSize = 4096;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSecurityController));
        private readonly IServiceProvider serviceProvider = serviceProvider ?? Globals.GetCurrentServiceProvider();
		Logger.LogDebug("FileSecurityController(serviceProvider={ServiceProvider}): Logger → {Logger}", serviceProvider, Logger);
		Logger.LogDebug("FileSecurityController(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);

        /// <summary>Initializes a new instance of the <see cref="FileSecurityController"/> class.</summary>
		Logger.LogDebug("<<< Exiting FileSecurityController(serviceProvider={ServiceProvider})", serviceProvider);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public FileSecurityController()
            : this(null)
        {
        }

        /// <inheritdoc />
        public bool Validate(string fileName, Stream fileContent)
        {
			Logger.LogDebug(">>> Entering Validate(fileName={FileName},fileContent={FileContent})", fileName, fileContent);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("fileContent", fileContent);

            var extension = Path.GetExtension(fileName);
            var checker = this.GetSecurityChecker(extension?.ToLowerInvariant().TrimStart('.'));
			Logger.LogDebug("Validate(fileName={FileName},fileContent={FileContent}): extension → {Extension}", fileName, fileContent, extension);
			Logger.LogDebug("Validate(fileName={FileName},fileContent={FileContent}): checker → {Checker}", fileName, fileContent, checker);

            // when there is no specific file check for the file type, then treat it as validated.
            if (checker == null)
            {
                return true;
            }

            // use copy of the stream as we can't make sure how the check process the stream.
            using var copyStream = CopyStream(fileContent);
			Logger.LogDebug("Validate(fileName={FileName},fileContent={FileContent}): copyStream → {CopyStream}", fileName, fileContent, copyStream);
			Logger.LogDebug("<<< Exiting Validate(fileName={FileName},fileContent={FileContent})", fileName, fileContent);
            return checker.Validate(copyStream);
        }

        /// <inheritdoc />
        public bool ValidateNotExectuable(Stream fileContent)
        {
			Logger.LogDebug(">>> Entering ValidateNotExectuable(fileContent={FileContent})", fileContent);
            Requires.NotNull("fileContent", fileContent);

            var firstBytes = new byte[2];
            int bytesRead = fileContent.Read(firstBytes, 0, 2);
            fileContent.Position = 0;
			Logger.LogDebug("ValidateNotExectuable(fileContent={FileContent}): bytesRead → {BytesRead}", fileContent, bytesRead);
			Logger.LogDebug("ValidateNotExectuable(fileContent={FileContent}): fileContent.Position → {Arg1}", fileContent, fileContent.Position);

            // Windows executable files start with 0x4D 0x5A
			Logger.LogDebug("<<< Exiting ValidateNotExectuable(fileContent={FileContent})", fileContent);
            return bytesRead < 2 || firstBytes[0] != 0x4D || firstBytes[1] != 0x5A;
        }

        /// <inheritdoc />
        protected override Func<IFileSecurityController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFileSecurityController>();
        }

        private static Stream CopyStream(Stream stream)
        {
			Logger.LogDebug(">>> Entering CopyStream(stream={Stream})", stream);
            var folderPath = ((FileManager)FileManager.Instance).GetHostMapPath();
            string filePath;
			Logger.LogDebug("CopyStream(stream={Stream}): folderPath → {FolderPath}", stream, folderPath);
            do
            {
                filePath = Path.Combine(folderPath, Path.GetRandomFileName()) + ".resx";
				Logger.LogDebug("CopyStream(stream={Stream}): filePath → {FilePath}", stream, filePath);
            }
            while (File.Exists(filePath));

            var fileStream = ((FileManager)FileManager.Instance).GetAutoDeleteFileStream(filePath);

			Logger.LogDebug("CopyStream(stream={Stream}): fileStream → {FileStream}", stream, fileStream);
            var array = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = stream.Read(array, 0, BufferSize)) > 0)
            {
                fileStream.Write(array, 0, bytesRead);
            }

            stream.Position = 0;
			Logger.LogDebug("CopyStream(stream={Stream}): stream.Position → {Arg1}", stream, stream.Position);
			Logger.LogDebug("CopyStream(stream={Stream}): fileStream.Position → {Arg1}", stream, fileStream.Position);
            fileStream.Position = 0;

			Logger.LogDebug("<<< Exiting CopyStream(stream={Stream})", stream);
            return fileStream;
        }

        private IFileSecurityChecker GetSecurityChecker(string extension)
        {
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

            return null;
        }
    }
}
