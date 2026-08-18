// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.ConfigConsole.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dnn.PersonaBar.ConfigConsole.Components;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Services.FileSystem.Internal;

using Moq;

using NUnit.Framework;
using Microsoft.Extensions.Logging;

/// <summary><see cref="ConfigConsoleController"/> unit tests.</summary>
[TestFixture]
public class ConfigConsoleControllerTests
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private const string GoodConfigXml = "<configuration></configuration>";
    private const string BadConfigXml = "<configuration1></configuration1>";
    private const string BadXml = "<configuration></configuration1>";

    /// <summary>Unit test for <see cref="ConfigConsoleController.ValidateConfigFileAsync(string, string)"/>.</summary>
    /// <param name="fileName">File name to test.</param>
    /// <param name="fileContent">File content to test.</param>
    /// <param name="expectedErrorCount">Expected number of validation errors.</param>
    /// <param name="shouldThrow">Whether the method should throw an exception or not.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    [TestCase(ConfigConsoleController.WebConfig, GoodConfigXml, 0, false)]
    [TestCase(ConfigConsoleController.WebConfig, BadConfigXml, 1, false)]
    [TestCase(ConfigConsoleController.WebConfig, BadXml, 0, true)]
    [TestCase("Random.config", GoodConfigXml, 0, false)]
    [TestCase("Random.config", BadConfigXml, 0, false)]
    [TestCase("Random.config", BadXml, 0, false)]
    [TestCase("portal-0.robots.txt", "Random content", 0, false)]
    public async Task ValidateConfigFile(string fileName, string fileContent, int expectedErrorCount, bool shouldThrow)
    {
		_logger.LogDebug(">>> Entering ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow})", fileName, fileContent, expectedErrorCount, shouldThrow);
        var applicationStatusInfoMock = new Mock<IApplicationStatusInfo>();
        applicationStatusInfoMock
            .Setup(info => info.ApplicationMapPath)
            .Returns(Environment.CurrentDirectory);
			_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): applicationStatusInfoMock → {ApplicationStatusInfoMock}", fileName, fileContent, expectedErrorCount, shouldThrow, applicationStatusInfoMock);

        var directoryMock = new Mock<IDirectory>();
		_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): directoryMock → {DirectoryMock}", fileName, fileContent, expectedErrorCount, shouldThrow, directoryMock);
        directoryMock
            .Setup(directory => directory.GetFilesAsync(applicationStatusInfoMock.Object.ApplicationMapPath))
            .ReturnsAsync([ConfigConsoleController.WebConfig, "Random.config", "portal-0.robots.txt",]);

        var controller = new ConfigConsoleController(applicationStatusInfoMock.Object, Mock.Of<IApplicationInfo>(), directoryMock.Object, Mock.Of<IFile>());
		_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): controller → {Controller}", fileName, fileContent, expectedErrorCount, shouldThrow, controller);

        Exception exception = null;
		_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): exception → {Exception}", fileName, fileContent, expectedErrorCount, shouldThrow, exception);
        IEnumerable<string> errors = [];
		_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): errors → {Errors}", fileName, fileContent, expectedErrorCount, shouldThrow, errors);
        try
        {
            errors = await controller.ValidateConfigFileAsync(fileName, fileContent);
        }
        catch (Exception ex)
        {
			_logger.LogError("Exception in ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): {Arg4}", fileName, fileContent, expectedErrorCount, shouldThrow, ex.getMessage());
            exception = ex;
        }

        using var scope = Assert.EnterMultipleScope();
		_logger.LogDebug("ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow}): scope → {Scope}", fileName, fileContent, expectedErrorCount, shouldThrow, scope);
        Assert.That(exception, shouldThrow ? Is.Not.Null : Is.Null);
        Assert.That(errors.ToList(), Has.Count.EqualTo(expectedErrorCount));
		_logger.LogDebug("<<< Exiting ValidateConfigFile(fileName={FileName},fileContent={FileContent},expectedErrorCount={ExpectedErrorCount},shouldThrow={ShouldThrow})", fileName, fileContent, expectedErrorCount, shouldThrow);
    }
}
