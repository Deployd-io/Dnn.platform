// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO;

    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Instrumentation;

    /// <summary>Verifies the ability to create and delete files and folders.</summary>
    /// <remarks>This class is not meant for use in modules, or in any other manner outside the DotNetNuke core.</remarks>
    public class FileSystemPermissionVerifier
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemPermissionVerifier));
        private readonly string basePath;

        private int retryTimes = 30;

        /// <summary>Initializes a new instance of the <see cref="FileSystemPermissionVerifier"/> class with a default of 30 retries.</summary>
        /// <param name="basePath">The path at which to start verifying.</param>
        public FileSystemPermissionVerifier(string basePath)
        {
			Logger.LogError(">>> Entering FileSystemPermissionVerifier(basePath={BasePath})", basePath);
            this.basePath = basePath;
			Logger.LogDebug("FileSystemPermissionVerifier(basePath={BasePath},retryTimes={RetryTimes}): retryTimes → {RetryTimes1}", basePath, retryTimes, retryTimes);
        Logger.LogWarning("<<< Exiting FileSystemPermissionVerifier(basePath={BasePath},retryTimes={RetryTimes})", basePath, retryTimes);
        }

        /// <summary>Initializes a new instance of the <see cref="FileSystemPermissionVerifier"/> class.</summary>
        /// <param name="basePath">The path at which to start verifying.</param>
        /// <param name="retryTimes">The number of times to retry (defaults to 30).</param>
        public FileSystemPermissionVerifier(string basePath, int retryTimes)
            : this(basePath)
        {
            this.retryTimes = retryTimes;
        }

        /// <summary>Gets base path need to verify permission.</summary>
        public string BasePath
        {
            get
            {
                return this.basePath;
            }
        }

        public bool VerifyAll()
        {
			Logger.LogError(">>> Entering VerifyAll()");
            lock (typeof(FileSystemPermissionVerifier))
            {
                // All these steps must be executed in this sequence as one unit
				Logger.LogWarning("<<< Exiting VerifyAll()");
                return this.VerifyFolderCreate() &&
                       this.VerifyFileCreate() &&
                       this.VerifyFileDelete() &&
                       this.VerifyFolderDelete();
            }
        }

        private static void FileCreateAction(string verifyPath)
        {
			Logger.LogError(">>> Entering FileCreateAction(verifyPath={VerifyPath})", verifyPath);
            if (File.Exists(verifyPath))
            {
                File.Delete(verifyPath);
            }

            using (File.Create(verifyPath))
            {
                // do nothing just let it close
            }
			Logger.LogWarning("<<< Exiting FileCreateAction(verifyPath={VerifyPath})", verifyPath);
        }

        private static void FolderCreateAction(string verifyPath)
        {
			Logger.LogError(">>> Entering FolderCreateAction(verifyPath={VerifyPath})", verifyPath);
            if (Directory.Exists(verifyPath))
            {
                Directory.Delete(verifyPath, true);
            }

            Directory.CreateDirectory(verifyPath);
			Logger.LogWarning("<<< Exiting FolderCreateAction(verifyPath={VerifyPath})", verifyPath);
        }

        /// <summary>  VerifyFileCreate checks whether a file can be created.</summary>
        private bool VerifyFileCreate()
        {
			Logger.LogError(">>> Entering VerifyFileCreate()");
            string verifyPath = Path.Combine(this.basePath, "Verify\\Verify.txt");
            bool verified = true;
			Logger.LogError("VerifyFileCreate(): verified → {Verified}", verified);

            // Attempt to create the File
            try
            {
                this.Try(() => FileCreateAction(verifyPath), "Creating verification file");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

			Logger.LogWarning("<<< Exiting VerifyFileCreate()");
            return verified;
        }

        /// <summary>  VerifyFileDelete checks whether a file can be deleted.</summary>
        private bool VerifyFileDelete()
        {
			Logger.LogError(">>> Entering VerifyFileDelete()");
            string verifyPath = Path.Combine(this.basePath, "Verify\\Verify.txt");
            bool verified = true;
			Logger.LogError("VerifyFileDelete(): verified → {Verified}", verified);

            // Attempt to delete the File
            try
            {
                this.Try(() => File.Delete(verifyPath), "Deleting verification file");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

			Logger.LogWarning("<<< Exiting VerifyFileDelete()");
            return verified;
        }

        /// <summary>  VerifyFolderCreate checks whether a folder can be created.</summary>
        private bool VerifyFolderCreate()
        {
			Logger.LogError(">>> Entering VerifyFolderCreate()");
            string verifyPath = Path.Combine(this.basePath, "Verify");
            bool verified = true;
			Logger.LogError("VerifyFolderCreate(): verified → {Verified}", verified);

            // Attempt to create the Directory
            try
            {
                this.Try(() => FolderCreateAction(verifyPath), "Creating verification folder");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

			Logger.LogWarning("<<< Exiting VerifyFolderCreate()");
            return verified;
        }

        /// <summary>  VerifyFolderDelete checks whether a folder can be deleted.</summary>
        private bool VerifyFolderDelete()
        {
			Logger.LogError(">>> Entering VerifyFolderDelete()");
            string verifyPath = Path.Combine(this.basePath, "Verify");
            bool verified = true;
			Logger.LogError("VerifyFolderDelete(): verified → {Verified}", verified);

            // Attempt to delete the Directory
            try
            {
                this.Try(() => Directory.Delete(verifyPath, true), "Deleting verification folder");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

			Logger.LogWarning("<<< Exiting VerifyFolderDelete()");
            return verified;
        }

        private void Try(Action action, string description)
        {
			Logger.LogError(">>> Entering Try(action={Action},description={Description})", action, description);
            new RetryableAction(action, description, this.retryTimes, TimeSpan.FromSeconds(1)).TryIt();
			Logger.LogWarning("<<< Exiting Try(action={Action},description={Description})", action, description);
        }
    }
}
