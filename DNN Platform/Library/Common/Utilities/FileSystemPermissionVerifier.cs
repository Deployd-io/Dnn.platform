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
			Logger.LogDebug(">>> Entering FileSystemPermissionVerifier(basePath={BasePath})", basePath);
            this.basePath = basePath;
			Logger.LogDebug("<<< Exiting FileSystemPermissionVerifier(basePath={BasePath})", basePath);
			Logger.LogDebug("FileSystemPermissionVerifier(basePath={BasePath}): this.basePath → {Arg1}", basePath, this.basePath);
			Logger.LogDebug("FileSystemPermissionVerifier(basePath={BasePath},retryTimes={RetryTimes}): retryTimes → {RetryTimes1}", basePath, retryTimes, retryTimes);
        }

        /// <summary>Initializes a new instance of the <see cref="FileSystemPermissionVerifier"/> class.</summary>
        /// <param name="basePath">The path at which to start verifying.</param>
        /// <param name="retryTimes">The number of times to retry (defaults to 30).</param>
        public FileSystemPermissionVerifier(string basePath, int retryTimes)
            : this(basePath)
        {
            this.retryTimes = retryTimes;
			Logger.LogDebug("FileSystemPermissionVerifier(basePath={BasePath},retryTimes={RetryTimes}): this.retryTimes → {Arg2}", basePath, retryTimes, this.retryTimes);
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
			Logger.LogDebug(">>> Entering VerifyAll()");
            lock (typeof(FileSystemPermissionVerifier))
            {
                // All these steps must be executed in this sequence as one unit
				Logger.LogDebug("<<< Exiting VerifyAll()");
                return this.VerifyFolderCreate() &&
                       this.VerifyFileCreate() &&
                       this.VerifyFileDelete() &&
                       this.VerifyFolderDelete();
            }
        }

        private static void FileCreateAction(string verifyPath)
        {
			Logger.LogDebug(">>> Entering FileCreateAction(verifyPath={VerifyPath})", verifyPath);
            if (File.Exists(verifyPath))
            {
                File.Delete(verifyPath);
            }

            using (File.Create(verifyPath))
            {
                // do nothing just let it close
            }
			Logger.LogDebug("<<< Exiting FileCreateAction(verifyPath={VerifyPath})", verifyPath);
        }

        private static void FolderCreateAction(string verifyPath)
        {
			Logger.LogDebug(">>> Entering FolderCreateAction(verifyPath={VerifyPath})", verifyPath);
            if (Directory.Exists(verifyPath))
            {
                Directory.Delete(verifyPath, true);
            }

            Directory.CreateDirectory(verifyPath);
			Logger.LogDebug("<<< Exiting FolderCreateAction(verifyPath={VerifyPath})", verifyPath);
        }

        /// <summary>  VerifyFileCreate checks whether a file can be created.</summary>
        private bool VerifyFileCreate()
        {
			Logger.LogDebug(">>> Entering VerifyFileCreate()");
            string verifyPath = Path.Combine(this.basePath, "Verify\\Verify.txt");
            bool verified = true;
			Logger.LogDebug("VerifyFileCreate(): verified → {Verified}", verified);

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

			Logger.LogDebug("<<< Exiting VerifyFileCreate()");
            return verified;
        }

        /// <summary>  VerifyFileDelete checks whether a file can be deleted.</summary>
        private bool VerifyFileDelete()
        {
			Logger.LogDebug(">>> Entering VerifyFileDelete()");
            string verifyPath = Path.Combine(this.basePath, "Verify\\Verify.txt");
            bool verified = true;
			Logger.LogDebug("VerifyFileDelete(): verified → {Verified}", verified);

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

			Logger.LogDebug("<<< Exiting VerifyFileDelete()");
            return verified;
        }

        /// <summary>  VerifyFolderCreate checks whether a folder can be created.</summary>
        private bool VerifyFolderCreate()
        {
			Logger.LogDebug(">>> Entering VerifyFolderCreate()");
            string verifyPath = Path.Combine(this.basePath, "Verify");
            bool verified = true;
			Logger.LogDebug("VerifyFolderCreate(): verified → {Verified}", verified);

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

			Logger.LogDebug("<<< Exiting VerifyFolderCreate()");
            return verified;
        }

        /// <summary>  VerifyFolderDelete checks whether a folder can be deleted.</summary>
        private bool VerifyFolderDelete()
        {
			Logger.LogDebug(">>> Entering VerifyFolderDelete()");
            string verifyPath = Path.Combine(this.basePath, "Verify");
            bool verified = true;
			Logger.LogDebug("VerifyFolderDelete(): verified → {Verified}", verified);

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

			Logger.LogDebug("<<< Exiting VerifyFolderDelete()");
            return verified;
        }

        private void Try(Action action, string description)
        {
			Logger.LogDebug(">>> Entering Try(action={Action},description={Description})", action, description);
            new RetryableAction(action, description, this.retryTimes, TimeSpan.FromSeconds(1)).TryIt();
			Logger.LogDebug("<<< Exiting Try(action={Action},description={Description})", action, description);
        }
    }
}
