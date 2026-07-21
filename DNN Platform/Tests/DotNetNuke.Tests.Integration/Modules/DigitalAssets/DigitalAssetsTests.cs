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

    [TestFixture]
    public class DigitalAssetsTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly int PortalId = 0;

        [Test]
        [Ignore("Digital Assets module has been replaced by Resource Manager")]
        public async Task File_Url_Should_Update_After_Rename_Folder()
        {
			Logger.LogDebug(">>> Entering File_Url_Should_Update_After_Rename_Folder()");
            var connector = WebApiTestHelper.LoginAdministrator();

			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): connector → {Connector}", connector);
            var folder = this.CreateNewFolder(connector);
            var folderId = Convert.ToInt32(folder.FolderID);
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): folder → {Folder}", folder);
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): folderId → {FolderId}", folderId);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files\\Test.png");
            await connector.UploadCmsFile(filePath, folder.FolderPath.ToString());
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): filePath → {FilePath}", filePath);
            var fileId = this.GetFileId(folderId, "Test.png");

			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): fileId → {FileId}", fileId);
            var newFolderName = Guid.NewGuid().ToString();
            this.RenameFolder(connector, folderId, newFolderName);
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): newFolderName → {NewFolderName}", newFolderName);

            var getUrlApi = "API/DigitalAssets/ContentService/GetUrl";
            var fileUrl = connector.PostJson(getUrlApi, new { fileId = fileId }, this.GetRequestHeaders()).Content.ReadAsStringAsync().Result;
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): getUrlApi → {GetUrlApi}", getUrlApi);
			Logger.LogDebug("File_Url_Should_Update_After_Rename_Folder(): fileUrl → {FileUrl}", fileUrl);

            Assert.That(fileUrl, Does.Contain(newFolderName));
			Logger.LogDebug("<<< Exiting File_Url_Should_Update_After_Rename_Folder()");
        }

        private int GetRootFolderId()
        {
			Logger.LogDebug(">>> Entering GetRootFolderId()");
			Logger.LogDebug("<<< Exiting GetRootFolderId()");
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderID FROM {{objectQualifier}}[Folders] WHERE PortalId = {this.PortalId} AND FolderPath = ''");
        }

        private int GetStandardFolderMappingId()
        {
			Logger.LogDebug(">>> Entering GetStandardFolderMappingId()");
			Logger.LogDebug("<<< Exiting GetStandardFolderMappingId()");
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderMappingId from {{objectQualifier}}[FolderMappings] where PortalID = {this.PortalId} and MappingName = 'Standard'");
        }

        private int GetFileId(int folderId, string fileName)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                $"select FileId from {{objectQualifier}}[Files] WHERE FolderID = {folderId} and FileName = '{fileName}'");
        }

        private dynamic CreateNewFolder(IWebApiConnector connector)
        {
            var rootFolderId = this.GetRootFolderId();
            var apiUrl = "API/DigitalAssets/ContentService/CreateNewFolder";
            var postData = new
            {
                FolderName = Guid.NewGuid().ToString(),
                ParentFolderId = rootFolderId,
                FolderMappingId = this.GetStandardFolderMappingId(),
                MappedName = string.Empty,
            };

            var response = connector.PostJson(apiUrl, postData, this.GetRequestHeaders());
            return Json.Deserialize<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        private void RenameFolder(IWebApiConnector connector, int folderId, string newFolderName)
        {
            var apiUrl = "API/DigitalAssets/ContentService/RenameFolder";
            var postData = new
            {
                folderId = folderId,
                newFolderName = newFolderName,
            };

            connector.PostJson(apiUrl, postData, this.GetRequestHeaders());
        }

        private IDictionary<string, string> GetRequestHeaders()
        {
            return WebApiTestHelper.GetRequestHeaders("//Admin//FileManagement", "Digital Asset Management", this.PortalId);
        }
    }
}
