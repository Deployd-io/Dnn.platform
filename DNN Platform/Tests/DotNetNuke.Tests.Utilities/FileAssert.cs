// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.IO;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    public static class FileAssert
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static void TextFilesAreEqual(string expectedFile, string actualFile)
        {
			_logger.LogDebug(">>> Entering TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
            TextFilesAreEqual(expectedFile, actualFile, string.Empty);
			_logger.LogDebug("<<< Exiting TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
        }

        public static void TextFilesAreEqual(string expectedFile, string actualFile, string message)
        {
			_logger.LogDebug(">>> Entering TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
            string expectedContent = File.ReadAllText(expectedFile);
            string actualContent = File.ReadAllText(actualFile);
            Assert.That(actualContent, Is.EqualTo(expectedContent), message);
			_logger.LogDebug("<<< Exiting TextFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile)
        {
			_logger.LogDebug(">>> Entering BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
            BinaryFilesAreEqual(expectedFile, actualFile, string.Empty);
			_logger.LogDebug("<<< Exiting BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile})", expectedFile, actualFile);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile, string message)
        {
			_logger.LogDebug(">>> Entering BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
            byte[] expectedContent = File.ReadAllBytes(expectedFile);
            byte[] actualContent = File.ReadAllBytes(actualFile);
            Assert.That(actualContent, Is.EqualTo(expectedContent).AsCollection, message);
			_logger.LogDebug("<<< Exiting BinaryFilesAreEqual(expectedFile={ExpectedFile},actualFile={ActualFile},message={Message})", expectedFile, actualFile, message);
        }
    }
}
