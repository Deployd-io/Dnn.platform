// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.IO;

    using NUnit.Framework;

    public static class FileAssert
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void TextFilesAreEqual(string expectedFile, string actualFile)
        {
			Logger.LogDebug(">>> Entering TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
            TextFilesAreEqual(expectedFile, actualFile, string.Empty);
			Logger.LogDebug("<<< Exiting TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
        }

        public static void TextFilesAreEqual(string expectedFile, string actualFile, string message)
        {
			Logger.LogDebug(">>> Entering TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
            string expectedContent = File.ReadAllText(expectedFile);
            string actualContent = File.ReadAllText(actualFile);
            Assert.That(actualContent, Is.EqualTo(expectedContent), message);
			Logger.LogDebug("<<< Exiting TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile)
        {
			Logger.LogDebug(">>> Entering BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
            BinaryFilesAreEqual(expectedFile, actualFile, string.Empty);
			Logger.LogDebug("<<< Exiting BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile, string message)
        {
			Logger.LogDebug(">>> Entering BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
            byte[] expectedContent = File.ReadAllBytes(expectedFile);
            byte[] actualContent = File.ReadAllBytes(actualFile);
            Assert.That(actualContent, Is.EqualTo(expectedContent).AsCollection, message);
			Logger.LogDebug("<<< Exiting BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
        }
    }
}
