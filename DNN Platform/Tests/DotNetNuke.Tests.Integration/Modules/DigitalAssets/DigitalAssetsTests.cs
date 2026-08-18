// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Modules.DigitalAssets
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DigitalAssetsTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly int PortalId = 0;

        [Test]
        [Ignore("Digital Assets module has been replaced by Resource Manager")]
        public async Task File_Url_Should_Update_After_Rename_Folder()
        {
			_logger.LogDebug(">>> Entering File_Url_Should_Update_After_Rename_Folder()");
            var connector = WebApiTestHelper.LoginAdministrator();

			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): connector → {Connector}", connector);
            var folder = this.CreateNewFolder(connector);
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): folder → {Folder}", folder);
            var folderId = Convert.ToInt32(folder.FolderID);
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): folderId → {FolderId}", folderId);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files\\Test.png");
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): filePath → {FilePath}", filePath);
            await connector.UploadCmsFile(filePath, folder.FolderPath.ToString());
            var fileId = this.GetFileId(folderId, "Test.png");
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): fileId → {FileId}", fileId);

            var newFolderName = Guid.NewGuid().ToString();
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): newFolderName → {NewFolderName}", newFolderName);
            this.RenameFolder(connector, folderId, newFolderName);

            var getUrlApi = "API/DigitalAssets/ContentService/GetUrl";
			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): getUrlApi → {GetUrlApi}", getUrlApi);
            var fileUrl = connector.PostJson(getUrlApi, new { fileId = fileId }, this.GetRequestHeaders()).Content.ReadAsStringAsync().Result;

			_logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): fileUrl → {FileUrl}", fileUrl);
            Assert.That(fileUrl, Does.Contain(newFolderName));
			_logger.LogDebug("<<< Exiting File_Url_Should_Update_After_Rename_Folder()");
        }

        private int GetRootFolderId()
        {
			_logger.LogDebug(">>> Entering GetRootFolderId()");
			_logger.LogDebug("<<< Exiting GetRootFolderId()");
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderID FROM {{objectQualifier}}[Folders] WHERE PortalId = {this.PortalId} AND FolderPath = ''");
        }

        private int GetStandardFolderMappingId()
        {
			_logger.LogDebug(">>> Entering GetStandardFolderMappingId()");
			_logger.LogDebug("<<< Exiting GetStandardFolderMappingId()");
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderMappingId from {{objectQualifier}}[FolderMappings] where PortalID = {this.PortalId} and MappingName = 'Standard'");
        }

        private int GetFileId(int folderId, string fileName)
        {
			_logger.LogDebug(">>> Entering GetFileId(folderId={FolderId},fileName={FileName})", folderId, fileName);
			_logger.LogDebug("<<< Exiting GetFileId(folderId={FolderId},fileName={FileName})", folderId, fileName);
            return DatabaseHelper.ExecuteScalar<int>(
                $"select FileId from {{objectQualifier}}[Files] WHERE FolderID = {folderId} and FileName = '{fileName}'");
        }

        private dynamic CreateNewFolder(IWebApiConnector connector)
        {
			_logger.LogDebug(">>> Entering CreateNewFolder(connector={Connector})", connector);
            var rootFolderId = this.GetRootFolderId();
            var apiUrl = "API/DigitalAssets/ContentService/CreateNewFolder";
			_logger.LogDebug("CreateNewFolder(connector={Connector}): apiUrl → {ApiUrl}", connector, apiUrl);
			_logger.LogDebug("CreateNewFolder(connector={Connector}): rootFolderId → {RootFolderId}", connector, rootFolderId);
            var postData = new
            {
                FolderName = Guid.NewGuid().ToString(),
                ParentFolderId = rootFolderId,
                FolderMappingId = this.GetStandardFolderMappingId(),
                MappedName = string.Empty,
            };
			_logger.LogDebug("CreateNewFolder(connector={Connector}): FolderMappingId → {FolderMappingId}", connector, FolderMappingId);
			_logger.LogDebug("CreateNewFolder(connector={Connector}): MappedName → {MappedName}", connector, MappedName);
			_logger.LogDebug("CreateNewFolder(connector={Connector}): ParentFolderId → {ParentFolderId}", connector, ParentFolderId);
			_logger.LogDebug("CreateNewFolder(connector={Connector}): FolderName → {FolderName}", connector, FolderName);

            var response = connector.PostJson(apiUrl, postData, this.GetRequestHeaders());
			_logger.LogDebug("CreateNewFolder(connector={Connector}): response → {Response}", connector, response);
			_logger.LogDebug("<<< Exiting CreateNewFolder(connector={Connector})", connector);
            return Json.Deserialize<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        private void RenameFolder(IWebApiConnector connector, int folderId, string newFolderName)
        {
			_logger.LogDebug(">>> Entering RenameFolder(connector={Connector},folderId={FolderId},newFolderName={NewFolderName})", connector, folderId, newFolderName);
            var apiUrl = "API/DigitalAssets/ContentService/RenameFolder";
            var postData = new
			_logger.LogDebug("RenameFolder(connector={Connector},folderId={FolderId},newFolderName={NewFolderName}): apiUrl → {ApiUrl}", connector, folderId, newFolderName, apiUrl);
            {
                folderId = folderId,
                newFolderName = newFolderName,
            };

            connector.PostJson(apiUrl, postData, this.GetRequestHeaders());
			_logger.LogDebug("<<< Exiting RenameFolder(connector={Connector},folderId={FolderId},newFolderName={NewFolderName})", connector, folderId, newFolderName);
        }

        private IDictionary<string, string> GetRequestHeaders()
        {
            return WebApiTestHelper.GetRequestHeaders("//Admin//FileManagement", "Digital Asset Management", this.PortalId);
        }
    }
}
