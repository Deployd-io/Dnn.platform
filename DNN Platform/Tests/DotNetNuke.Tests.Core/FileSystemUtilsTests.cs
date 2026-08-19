// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core
{
    using System;
    using System.IO;
    using System.IO.Compression;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileSystemUtilsTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            var applicationStatusInfo = new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>());
            var rootPath = Path.Combine(applicationStatusInfo.ApplicationMapPath, "FileSystemUtilsTest");
			mockEventLogger.LogDebug("SetUp(): rootPath → {RootPath}", rootPath);
			mockEventLogger.LogDebug("SetUp(): applicationStatusInfo → {ApplicationStatusInfo}", applicationStatusInfo);
            PrepareRootPath(rootPath, applicationStatusInfo.ApplicationMapPath);

            var mock = new Mock<IApplicationStatusInfo>();
			mockEventLogger.LogDebug("SetUp(): mock → {Mock}", mock);
            mock.Setup(info => info.ApplicationMapPath).Returns(rootPath);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(mock.Object);
                });
				mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase("/")]
        [TestCase("//")]
        [TestCase("///")]
        [TestCase(@"\")]
        [TestCase("\\")]
        [TestCase(@"\\\")]
        [TestCase("/Test/../")]
        [TestCase("/Test/mmm/../../")]
        [TestCase(@"\Test\..\")]
        [TestCase(@"\Test\mmm\..\..\")]
        [TestCase(@"\Test\")]
        [TestCase("..\\")]
        public void DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(string path)
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path})", path);
            // Action
            FileSystemUtils.DeleteFiles(new[] { path, });

            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.AllDirectories);
			mockEventLogger.LogDebug("DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path}): files → {Files}", path, files);
            Assert.That(files, Is.Not.Empty);
			mockEventLogger.LogDebug("<<< Exiting DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path})", path);
        }

        [Test]
        public void AddToZip_Should_Able_To_Add_Multiple_Files()
        {
			mockEventLogger.LogDebug(">>> Entering AddToZip_Should_Able_To_Add_Multiple_Files()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Action
            DeleteZippedFiles();
            var zipFilePath = Path.Combine(Globals.ApplicationMapPath, $"Test{Guid.NewGuid().ToString().Substring(0, 8)}.zip");
			mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): zipFilePath → {ZipFilePath}", zipFilePath);
            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.TopDirectoryOnly);
			mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): files → {Files}", files);
            using (var stream = File.Create(zipFilePath))
            {
                var zipStream = new ZipArchive(stream, ZipArchiveMode.Create, true);
				mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): zipStream → {ZipStream}", zipStream);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
					mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): fileName → {FileName}", fileName);
                    FileSystemUtils.AddToZip(ref zipStream, file, fileName, string.Empty);
                }

                zipStream.Dispose();
            }

            // Assert
            var destPath = Path.Combine(Globals.ApplicationMapPath, Path.GetFileNameWithoutExtension(zipFilePath));
			mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): destPath → {DestPath}", destPath);
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            try
            {
                using (var stream = File.OpenRead(zipFilePath))
                {
                    var zipStream = new ZipArchive(stream, ZipArchiveMode.Read, true);
                    FileSystemUtils.UnzipResources(zipStream, destPath);
                    zipStream.Dispose();
                }

                var unZippedFiles = Directory.GetFiles(destPath, "*.*", SearchOption.TopDirectoryOnly);
				mockEventLogger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): unZippedFiles → {UnZippedFiles}", unZippedFiles);
                Assert.That(unZippedFiles, Has.Length.EqualTo(files.Length));
            }
            finally
            {
                DeleteZippedFiles();
                DeleteUnzippedFolder(destPath);
            }
			mockEventLogger.LogInformation("AddToZip_Should_Able_To_Add_Multiple_Files(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting AddToZip_Should_Able_To_Add_Multiple_Files()");
        }

        [Test]
        public void DeleteFile_Should_Delete_File()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFile_Should_Delete_File()");
            // Action
            var testPath = Globals.ApplicationMapPath + $"/Test{Guid.NewGuid().ToString().Substring(0, 8)}.txt";
			mockEventLogger.LogDebug("DeleteFile_Should_Delete_File(): testPath → {TestPath}", testPath);
            using (StreamWriter sw = File.CreateText(testPath))
            {
                sw.WriteLine("48");
            }

            FileSystemUtils.DeleteFile(testPath);

            // Assert
            Assert.That(testPath.Replace("/", @"\"), Does.Not.Exist);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("/")]
        [TestCase("Test/Test ")]
        public void FixPath_Should_Change_Slashes_And_Trim(string input)
        {
			mockEventLogger.LogDebug(">>> Entering FixPath_Should_Change_Slashes_And_Trim(input={Input})", input);
            // Action
            var result = FileSystemUtils.FixPath(input);
			mockEventLogger.LogDebug("FixPath_Should_Change_Slashes_And_Trim(input={Input}): result → {Result}", input, result);

            // Assert
            if (string.IsNullOrEmpty(input))
            {
                Assert.That(result, Is.EqualTo(input));
            }
            else if (string.IsNullOrWhiteSpace(input))
            {
                Assert.That(result, Is.Empty);
            }
            else
            {
                using var scope = Assert.EnterMultipleScope();
				mockEventLogger.LogDebug("FixPath_Should_Change_Slashes_And_Trim(input={Input}): scope → {Scope}", input, scope);
                Assert.That(result, Does.Not.Contain(" "));
                Assert.That(result, Does.Not.Contain("/"));
            }
			mockEventLogger.LogDebug("<<< Exiting FixPath_Should_Change_Slashes_And_Trim(input={Input})", input);
        }

        private static void PrepareRootPath(string rootPath, string applicationMapPath)
        {
			mockEventLogger.LogDebug(">>> Entering PrepareRootPath(rootPath=<redacted>,applicationMapPath={ApplicationMapPath})", applicationMapPath);
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            foreach (var file in Directory.GetFiles(applicationMapPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    File.Copy(file, Path.Combine(rootPath, Path.GetFileName(file)), true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
            }
			mockEventLogger.LogDebug("<<< Exiting PrepareRootPath(rootPath=<redacted>,applicationMapPath={ApplicationMapPath})", applicationMapPath);
        }

        private static void DeleteZippedFiles()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteZippedFiles()");
            var excludedFiles = Directory.GetFiles(Globals.ApplicationMapPath, "Test*", SearchOption.TopDirectoryOnly);
            foreach (var f in excludedFiles)
            {
			mockEventLogger.LogDebug("DeleteZippedFiles(): excludedFiles → {ExcludedFiles}", excludedFiles);
                try
                {
                    File.Delete(f);
                }
                catch (Exception)
                {
					mockEventLogger.LogError("Exception in DeleteZippedFiles(): {Arg0}", Exception.Message);
                    // ignore
                }
            }
			mockEventLogger.LogDebug("<<< Exiting DeleteZippedFiles()");
        }

        private static void DeleteUnzippedFolder(string zippedFolder)
        {
			mockEventLogger.LogDebug(">>> Entering DeleteUnzippedFolder(zippedFolder={ZippedFolder})", zippedFolder);
            try
            {
                Directory.Delete(zippedFolder, true);
            }
            catch (Exception)
            {
				mockEventLogger.LogError("Exception in DeleteUnzippedFolder(zippedFolder={ZippedFolder}): {Arg1}", zippedFolder, Exception.Message);
                // ignore
            }
			mockEventLogger.LogDebug("<<< Exiting DeleteUnzippedFolder(zippedFolder={ZippedFolder})", zippedFolder);
        }
    }
}
