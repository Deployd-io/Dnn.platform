// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    public class UrlController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ArrayList GetUrls(int portalID)
        {
			Logger.LogDebug(">>> Entering GetUrls(portalID={PortalID})", portalID);
			Logger.LogDebug("<<< Exiting GetUrls(portalID={PortalID})", portalID);
            return CBO.FillCollection(DataProvider.Instance().GetUrls(portalID), typeof(UrlInfo));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public UrlInfo GetUrl(int portalID, string url)
        {
			Logger.LogDebug(">>> Entering GetUrl(portalID={PortalID},url={Url})", portalID, url);
			Logger.LogDebug("<<< Exiting GetUrl(portalID={PortalID},url={Url})", portalID, url);
            return CBO.FillObject<UrlInfo>(DataProvider.Instance().GetUrl(portalID, url));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public UrlTrackingInfo GetUrlTracking(int portalID, string url, int moduleId)
        {
			Logger.LogDebug(">>> Entering GetUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId})", portalID, url, moduleId);
			Logger.LogDebug("<<< Exiting GetUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId})", portalID, url, moduleId);
            return CBO.FillObject<UrlTrackingInfo>(DataProvider.Instance().GetUrlTracking(portalID, url, moduleId));
        }

        public void UpdateUrl(int portalID, string url, string urlType, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
        {
            this.UpdateUrl(portalID, url, urlType, 0, Null.NullDate, Null.NullDate, logActivity, trackClicks, moduleID, newWindow);
        }

        public void UpdateUrl(int portalID, string url, string urlType, int clicks, DateTime lastClick, DateTime createdDate, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (urlType == "U")
                {
                    if (this.GetUrl(portalID, url) == null)
                    {
                        DataProvider.Instance().AddUrl(portalID, url.Replace(@"\", @"/"));
                    }
                }

                UrlTrackingInfo objURLTracking = this.GetUrlTracking(portalID, url, moduleID);
                if (objURLTracking == null)
				Logger.LogDebug("UpdateUrl(portalID={PortalID},url={Url},urlType={UrlType},clicks={Clicks},lastClick={LastClick},createdDate={CreatedDate},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow}): objURLTracking → {ObjURLTracking}", portalID, url, urlType, clicks, lastClick, createdDate, logActivity, trackClicks, moduleID, newWindow, objURLTracking);
                {
                    DataProvider.Instance().AddUrlTracking(portalID, url, urlType, logActivity, trackClicks, moduleID, newWindow);
                }
                else
                {
                    DataProvider.Instance().UpdateUrlTracking(portalID, url, logActivity, trackClicks, moduleID, newWindow);
                }
            }
			Logger.LogDebug("<<< Exiting UpdateUrl(portalID={PortalID},url={Url},urlType={UrlType},clicks={Clicks},lastClick={LastClick},createdDate={CreatedDate},logActivity={LogActivity},trackClicks={TrackClicks},moduleID={ModuleID},newWindow={NewWindow})", portalID, url, urlType, clicks, lastClick, createdDate, logActivity, trackClicks, moduleID, newWindow);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeleteUrl(int portalID, string url)
        {
			Logger.LogDebug(">>> Entering DeleteUrl(portalID={PortalID},url={Url})", portalID, url);
            DataProvider.Instance().DeleteUrl(portalID, url);
			Logger.LogDebug("<<< Exiting DeleteUrl(portalID={PortalID},url={Url})", portalID, url);
        }

        public void UpdateUrlTracking(int portalID, string url, int moduleId, int userID)
        {
			Logger.LogDebug(">>> Entering UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID})", portalID, url, moduleId, userID);
            TabType urlType = Globals.GetURLType(url);
            if (urlType == TabType.File && !url.StartsWith("fileid=", StringComparison.OrdinalIgnoreCase))
			Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): urlType → {UrlType}", portalID, url, moduleId, userID, urlType);
            {
                // to handle legacy scenarios before the introduction of the FileServerHandler
                var fileName = Path.GetFileName(url);

				Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): fileName → {FileName}", portalID, url, moduleId, userID, fileName);
                var folderPath = url.Substring(0, url.LastIndexOf(fileName, StringComparison.OrdinalIgnoreCase));
                var folder = FolderManager.Instance.GetFolder(portalID, folderPath);
				Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): folderPath → {FolderPath}", portalID, url, moduleId, userID, folderPath);
				Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): folder → {Folder}", portalID, url, moduleId, userID, folder);

                var file = FileManager.Instance.GetFile(folder, fileName);

				Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): file → {File}", portalID, url, moduleId, userID, file);
                url = "FileID=" + file.FileId;
            }

            UrlTrackingInfo objUrlTracking = this.GetUrlTracking(portalID, url, moduleId);
            if (objUrlTracking != null)
			Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): objUrlTracking → {ObjUrlTracking}", portalID, url, moduleId, userID, objUrlTracking);
            {
                if (objUrlTracking.TrackClicks)
                {
                    DataProvider.Instance().UpdateUrlTrackingStats(portalID, url, moduleId);
                    if (objUrlTracking.LogActivity)
                    {
                        if (userID == -1)
                        {
                            userID = UserController.Instance.GetCurrentUserInfo().UserID;
                        }
						Logger.LogDebug("UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID}): userID → {UserID1}", portalID, url, moduleId, userID, userID);

                        DataProvider.Instance().AddUrlLog(objUrlTracking.UrlTrackingID, userID);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting UpdateUrlTracking(portalID={PortalID},url={Url},moduleId={ModuleId},userID={UserID})", portalID, url, moduleId, userID);
        }

        public ArrayList GetUrlLog(int portalID, string url, int moduleId, DateTime startDate, DateTime endDate)
        {
			Logger.LogDebug(">>> Entering GetUrlLog(portalID={PortalID},url={Url},moduleId={ModuleId},startDate={StartDate},endDate={EndDate})", portalID, url, moduleId, startDate, endDate);
            ArrayList arrUrlLog = null;
            UrlTrackingInfo objUrlTracking = this.GetUrlTracking(portalID, url, moduleId);
			Logger.LogDebug("GetUrlLog(portalID={PortalID},url={Url},moduleId={ModuleId},startDate={StartDate},endDate={EndDate}): arrUrlLog → {ArrUrlLog}", portalID, url, moduleId, startDate, endDate, arrUrlLog);
			Logger.LogDebug("GetUrlLog(portalID={PortalID},url={Url},moduleId={ModuleId},startDate={StartDate},endDate={EndDate}): objUrlTracking → {ObjUrlTracking}", portalID, url, moduleId, startDate, endDate, objUrlTracking);
            if (objUrlTracking != null)
            {
                arrUrlLog = CBO.FillCollection(DataProvider.Instance().GetUrlLog(objUrlTracking.UrlTrackingID, startDate, endDate), typeof(UrlLogInfo));
            }

			Logger.LogDebug("<<< Exiting GetUrlLog(portalID={PortalID},url={Url},moduleId={ModuleId},startDate={StartDate},endDate={EndDate})", portalID, url, moduleId, startDate, endDate);
            return arrUrlLog;
        }
    }
}
