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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            var applicationStatusInfo = new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>());
            var rootPath = Path.Combine(applicationStatusInfo.ApplicationMapPath, "FileSystemUtilsTest");
			Logger.LogDebug("SetUp(): applicationStatusInfo → {ApplicationStatusInfo}", applicationStatusInfo);
			Logger.LogDebug("SetUp(): rootPath → {RootPath}", rootPath);
            PrepareRootPath(rootPath, applicationStatusInfo.ApplicationMapPath);

            var mock = new Mock<IApplicationStatusInfo>();
            mock.Setup(info => info.ApplicationMapPath).Returns(rootPath);
			Logger.LogDebug("SetUp(): mock → {Mock}", mock);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(mock.Object);
                });
				Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
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
			Logger.LogDebug(">>> Entering DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path})", path);
            // Action
            FileSystemUtils.DeleteFiles(new[] { path, });

            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.AllDirectories);
            Assert.That(files, Is.Not.Empty);
			Logger.LogDebug("<<< Exiting DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path})", path);
			Logger.LogDebug("DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(path={Path}): files → {Files}", path, files);
        }

        [Test]
        public void AddToZip_Should_Able_To_Add_Multiple_Files()
        {
			Logger.LogDebug(">>> Entering AddToZip_Should_Able_To_Add_Multiple_Files()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Action
            DeleteZippedFiles();
            var zipFilePath = Path.Combine(Globals.ApplicationMapPath, $"Test{Guid.NewGuid().ToString().Substring(0, 8)}.zip");
            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.TopDirectoryOnly);
			Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): zipFilePath → {ZipFilePath}", zipFilePath);
			Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): files → {Files}", files);
            using (var stream = File.Create(zipFilePath))
            {
                var zipStream = new ZipArchive(stream, ZipArchiveMode.Create, true);

				Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): zipStream → {ZipStream}", zipStream);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    FileSystemUtils.AddToZip(ref zipStream, file, fileName, string.Empty);
					Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): fileName → {FileName}", fileName);
                }

                zipStream.Dispose();
            }

            // Assert
            var destPath = Path.Combine(Globals.ApplicationMapPath, Path.GetFileNameWithoutExtension(zipFilePath));
            if (!Directory.Exists(destPath))
			Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): destPath → {DestPath}", destPath);
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
                Assert.That(unZippedFiles, Has.Length.EqualTo(files.Length));
				Logger.LogDebug("AddToZip_Should_Able_To_Add_Multiple_Files(): unZippedFiles → {UnZippedFiles}", unZippedFiles);
            }
            finally
            {
                DeleteZippedFiles();
                DeleteUnzippedFolder(destPath);
            }
			Logger.LogDebug("<<< Exiting AddToZip_Should_Able_To_Add_Multiple_Files()");
			Logger.LogInformation("AddToZip_Should_Able_To_Add_Multiple_Files(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void DeleteFile_Should_Delete_File()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Should_Delete_File()");
            // Action
            var testPath = Globals.ApplicationMapPath + $"/Test{Guid.NewGuid().ToString().Substring(0, 8)}.txt";
            using (StreamWriter sw = File.CreateText(testPath))
            {
			Logger.LogDebug("DeleteFile_Should_Delete_File(): testPath → {TestPath}", testPath);
                sw.WriteLine("48");
            }

            FileSystemUtils.DeleteFile(testPath);

            // Assert
            Assert.That(testPath.Replace("/", @"\"), Does.Not.Exist);
			Logger.LogDebug("<<< Exiting DeleteFile_Should_Delete_File()");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("/")]
        [TestCase("Test/Test ")]
        public void FixPath_Should_Change_Slashes_And_Trim(string input)
        {
			Logger.LogDebug(">>> Entering FixPath_Should_Change_Slashes_And_Trim(input={Input})", input);
            // Action
            var result = FileSystemUtils.FixPath(input);

			Logger.LogDebug("FixPath_Should_Change_Slashes_And_Trim(input={Input}): result → {Result}", input, result);
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
                Assert.That(result, Does.Not.Contain(" "));
				Logger.LogDebug("FixPath_Should_Change_Slashes_And_Trim(input={Input}): scope → {Scope}", input, scope);
                Assert.That(result, Does.Not.Contain("/"));
            }
			Logger.LogDebug("<<< Exiting FixPath_Should_Change_Slashes_And_Trim(input={Input})", input);
        }

        private static void PrepareRootPath(string rootPath, string applicationMapPath)
        {
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
        }

        private static void DeleteZippedFiles()
        {
			Logger.LogDebug(">>> Entering DeleteZippedFiles()");
            var excludedFiles = Directory.GetFiles(Globals.ApplicationMapPath, "Test*", SearchOption.TopDirectoryOnly);
            foreach (var f in excludedFiles)
            {
			Logger.LogDebug("DeleteZippedFiles(): excludedFiles → {ExcludedFiles}", excludedFiles);
                try
                {
                    File.Delete(f);
                }
                catch (Exception)
                {
					Logger.LogError("Exception in DeleteZippedFiles(): {Arg0}", ex.Message);
                    // ignore
                }
            }
			Logger.LogDebug("<<< Exiting DeleteZippedFiles()");
        }

        private static void DeleteUnzippedFolder(string zippedFolder)
        {
			Logger.LogDebug(">>> Entering DeleteUnzippedFolder(zippedFolder={ZippedFolder})", zippedFolder);
            try
            {
                Directory.Delete(zippedFolder, true);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in DeleteUnzippedFolder(zippedFolder={ZippedFolder}): {Arg1}", zippedFolder, ex.Message);
                // ignore
            }
			Logger.LogDebug("<<< Exiting DeleteUnzippedFolder(zippedFolder={ZippedFolder})", zippedFolder);
        }
    }
}
