// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System.Collections.Generic;

    using DotNetNuke.Services.FileSystem.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultFolderProvidersTests
    {
        [Test]
        public void GetDefaultProviders_Should_Return_3_Valid_Providers()
        {
			mockEventLogger.LogDebug(">>> Entering GetDefaultProviders_Should_Return_3_Valid_Providers()");
            var expectedValues = new List<string> { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };

			mockEventLogger.LogDebug("GetDefaultProviders_Should_Return_3_Valid_Providers(): expectedValues → {ExpectedValues}", expectedValues);
            var defaultProviders = DefaultFolderProviders.GetDefaultProviders();
			mockEventLogger.LogDebug("GetDefaultProviders_Should_Return_3_Valid_Providers(): defaultProviders → {DefaultProviders}", defaultProviders);

            Assert.That(defaultProviders, Is.EqualTo(expectedValues).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting GetDefaultProviders_Should_Return_3_Valid_Providers()");
        }
    }
}
