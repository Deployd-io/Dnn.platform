// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System.Data;

    /// <summary>  The DataProvider is an abstract class that provides the Data Access Layer for the HtmlText module.</summary>
    public class DataProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // singleton reference to the instantiated object
        private static readonly DataProvider Provider;

        // constructor
        static DataProvider()
        {
			Logger.LogDebug(">>> Entering DataProvider()");
            Provider = new DataProvider();
			Logger.LogDebug("<<< Exiting DataProvider()");
			Logger.LogDebug("DataProvider(): Provider → {Provider}", Provider);
        }

        // return the provider
        public static DataProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return Provider;
        }

        public virtual IDataReader GetHtmlText(int moduleID, int itemID)
        {
			Logger.LogDebug(">>> Entering GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
			Logger.LogDebug("<<< Exiting GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlText", moduleID, itemID);
        }

        public virtual IDataReader GetTopHtmlText(int moduleID, bool isPublished)
        {
			Logger.LogDebug(">>> Entering GetTopHtmlText(moduleID={ModuleID},isPublished={IsPublished})", moduleID, isPublished);
			Logger.LogDebug("<<< Exiting GetTopHtmlText(moduleID={ModuleID},isPublished={IsPublished})", moduleID, isPublished);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetTopHtmlText", moduleID, isPublished);
        }

        public virtual IDataReader GetAllHtmlText(int moduleID)
        {
			Logger.LogDebug(">>> Entering GetAllHtmlText(moduleID={ModuleID})", moduleID);
			Logger.LogDebug("<<< Exiting GetAllHtmlText(moduleID={ModuleID})", moduleID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetAllHtmlText", moduleID);
        }

        public virtual int AddHtmlText(int moduleID, string content, string summary, int stateID, bool isPublished, int createdByUserID, int history)
        {
			Logger.LogDebug(">>> Entering AddHtmlText(moduleID={ModuleID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},createdByUserID={CreatedByUserID},history={History})", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
			Logger.LogDebug("<<< Exiting AddHtmlText(moduleID={ModuleID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},createdByUserID={CreatedByUserID},history={History})", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteScalar<int>("AddHtmlText", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
        }

        public virtual void UpdateHtmlText(int itemID, string content, string summary, int stateID, bool isPublished, int lastModifiedByUserID)
        {
			Logger.LogDebug(">>> Entering UpdateHtmlText(itemID={ItemID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},lastModifiedByUserID={LastModifiedByUserID})", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("UpdateHtmlText", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
			Logger.LogDebug("<<< Exiting UpdateHtmlText(itemID={ItemID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},lastModifiedByUserID={LastModifiedByUserID})", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateHtmlText(itemID,content,summary,stateID,isPublished,lastModifiedByUserID)={ItemID},{Content},{Summary},{StateID},{IsPublished},{LastModifiedByUserID}: database query executed in {Arg6} ms", itemID, content, summary, stateID, isPublished, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateHtmlText(itemID,content,summary,stateID,isPublished,lastModifiedByUserID)={ItemID},{Content},{Summary},{StateID},{IsPublished},{LastModifiedByUserID}: database query executed in {Arg6} ms", itemID, content, summary, stateID, isPublished, lastModifiedByUserID, __logStart.ElapsedMilliseconds);
        public virtual void DeleteHtmlText(int moduleID, int itemID)
        {
			Logger.LogDebug(">>> Entering DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlText", moduleID, itemID);
			Logger.LogDebug("<<< Exiting DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteHtmlText(moduleID,itemID)={ModuleID},{ItemID}: database query executed in {Arg2} ms", moduleID, itemID, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteHtmlText(moduleID,itemID)={ModuleID},{ItemID}: database query executed in {Arg2} ms", moduleID, itemID, __logStart.ElapsedMilliseconds);
        public virtual IDataReader GetHtmlTextLog(int itemID)
        {
			Logger.LogDebug(">>> Entering GetHtmlTextLog(itemID={ItemID})", itemID);
			Logger.LogDebug("<<< Exiting GetHtmlTextLog(itemID={ItemID})", itemID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextLog", itemID);
        }

        public virtual void AddHtmlTextLog(int itemID, int stateID, string comment, bool approved, int createdByUserID)
        {
			Logger.LogDebug(">>> Entering AddHtmlTextLog(itemID={ItemID},stateID={StateID},comment={Comment},approved={Approved},createdByUserID={CreatedByUserID})", itemID, stateID, comment, approved, createdByUserID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextLog", itemID, stateID, comment, approved, createdByUserID);
			Logger.LogDebug("<<< Exiting AddHtmlTextLog(itemID={ItemID},stateID={StateID},comment={Comment},approved={Approved},createdByUserID={CreatedByUserID})", itemID, stateID, comment, approved, createdByUserID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("AddHtmlTextLog(itemID,stateID,comment,approved,createdByUserID)={ItemID},{StateID},{Comment},{Approved},{CreatedByUserID}: database query executed in {Arg5} ms", itemID, stateID, comment, approved, createdByUserID, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("AddHtmlTextLog(itemID,stateID,comment,approved,createdByUserID)={ItemID},{StateID},{Comment},{Approved},{CreatedByUserID}: database query executed in {Arg5} ms", itemID, stateID, comment, approved, createdByUserID, __logStart.ElapsedMilliseconds);
        public virtual IDataReader GetHtmlTextUser(int userID)
        {
			Logger.LogDebug(">>> Entering GetHtmlTextUser(userID={UserID})", userID);
			Logger.LogDebug("<<< Exiting GetHtmlTextUser(userID={UserID})", userID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextUser", userID);
        }

        public virtual void AddHtmlTextUser(int itemID, int stateID, int moduleID, int tabID, int userID)
        {
			Logger.LogDebug(">>> Entering AddHtmlTextUser(itemID={ItemID},stateID={StateID},moduleID={ModuleID},tabID={TabID},userID={UserID})", itemID, stateID, moduleID, tabID, userID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextUser", itemID, stateID, moduleID, tabID, userID);
			Logger.LogDebug("<<< Exiting AddHtmlTextUser(itemID={ItemID},stateID={StateID},moduleID={ModuleID},tabID={TabID},userID={UserID})", itemID, stateID, moduleID, tabID, userID);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("AddHtmlTextUser(itemID,stateID,moduleID,tabID,userID)={ItemID},{StateID},{ModuleID},{TabID},{UserID}: database query executed in {Arg5} ms", itemID, stateID, moduleID, tabID, userID, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("AddHtmlTextUser(itemID,stateID,moduleID,tabID,userID)={ItemID},{StateID},{ModuleID},{TabID},{UserID}: database query executed in {Arg5} ms", itemID, stateID, moduleID, tabID, userID, __logStart.ElapsedMilliseconds);
        public virtual void DeleteHtmlTextUsers()
        {
			Logger.LogDebug(">>> Entering DeleteHtmlTextUsers()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlTextUsers");
			Logger.LogDebug("<<< Exiting DeleteHtmlTextUsers()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteHtmlTextUsers(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }
    }
	Logger.LogInformation("DeleteHtmlTextUsers(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
}
