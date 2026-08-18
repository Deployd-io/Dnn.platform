// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System.Data;
	using Microsoft.Extensions.Logging;

    /// <summary>  The DataProvider is an abstract class that provides the Data Access Layer for the HtmlText module.</summary>
    public class DataProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        // singleton reference to the instantiated object
        private static readonly DataProvider Provider;

        // constructor
        static DataProvider()
        {
			_logger.LogDebug(">>> Entering DataProvider()");
            Provider = new DataProvider();
			_logger.LogDebug("DataProvider(): Provider → {Provider}", Provider);
			_logger.LogDebug("<<< Exiting DataProvider()");
        }

        // return the provider
        public static DataProvider Instance()
        {
			_logger.LogDebug(">>> Entering Instance()");
			_logger.LogDebug("<<< Exiting Instance()");
            return Provider;
        }

        public virtual IDataReader GetHtmlText(int moduleID, int itemID)
        {
			_logger.LogDebug(">>> Entering GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
			_logger.LogDebug("<<< Exiting GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlText", moduleID, itemID);
        }

        public virtual IDataReader GetTopHtmlText(int moduleID, bool isPublished)
        {
			_logger.LogDebug(">>> Entering GetTopHtmlText(moduleID={ModuleID},isPublished={IsPublished})", moduleID, isPublished);
			_logger.LogDebug("<<< Exiting GetTopHtmlText(moduleID={ModuleID},isPublished={IsPublished})", moduleID, isPublished);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetTopHtmlText", moduleID, isPublished);
        }

        public virtual IDataReader GetAllHtmlText(int moduleID)
        {
			_logger.LogDebug(">>> Entering GetAllHtmlText(moduleID={ModuleID})", moduleID);
			_logger.LogDebug("<<< Exiting GetAllHtmlText(moduleID={ModuleID})", moduleID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetAllHtmlText", moduleID);
        }

        public virtual int AddHtmlText(int moduleID, string content, string summary, int stateID, bool isPublished, int createdByUserID, int history)
        {
			_logger.LogDebug(">>> Entering AddHtmlText(moduleID={ModuleID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},createdByUserID={CreatedByUserID},history={History})", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
			_logger.LogDebug("<<< Exiting AddHtmlText(moduleID={ModuleID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},createdByUserID={CreatedByUserID},history={History})", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteScalar<int>("AddHtmlText", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
        }

        public virtual void UpdateHtmlText(int itemID, string content, string summary, int stateID, bool isPublished, int lastModifiedByUserID)
        {
			_logger.LogDebug(">>> Entering UpdateHtmlText(itemID={ItemID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},lastModifiedByUserID={LastModifiedByUserID})", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("UpdateHtmlText", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
			_logger.LogDebug("<<< Exiting UpdateHtmlText(itemID={ItemID},content={Content},summary={Summary},stateID={StateID},isPublished={IsPublished},lastModifiedByUserID={LastModifiedByUserID})", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
        }

        public virtual void DeleteHtmlText(int moduleID, int itemID)
        {
			_logger.LogDebug(">>> Entering DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlText", moduleID, itemID);
			_logger.LogDebug("<<< Exiting DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
        }

        public virtual IDataReader GetHtmlTextLog(int itemID)
        {
			_logger.LogDebug(">>> Entering GetHtmlTextLog(itemID={ItemID})", itemID);
			_logger.LogDebug("<<< Exiting GetHtmlTextLog(itemID={ItemID})", itemID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextLog", itemID);
        }

        public virtual void AddHtmlTextLog(int itemID, int stateID, string comment, bool approved, int createdByUserID)
        {
			_logger.LogDebug(">>> Entering AddHtmlTextLog(itemID={ItemID},stateID={StateID},comment={Comment},approved={Approved},createdByUserID={CreatedByUserID})", itemID, stateID, comment, approved, createdByUserID);
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextLog", itemID, stateID, comment, approved, createdByUserID);
			_logger.LogDebug("<<< Exiting AddHtmlTextLog(itemID={ItemID},stateID={StateID},comment={Comment},approved={Approved},createdByUserID={CreatedByUserID})", itemID, stateID, comment, approved, createdByUserID);
        }

        public virtual IDataReader GetHtmlTextUser(int userID)
        {
			_logger.LogDebug(">>> Entering GetHtmlTextUser(userID={UserID})", userID);
			_logger.LogDebug("<<< Exiting GetHtmlTextUser(userID={UserID})", userID);
            return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextUser", userID);
        }

        public virtual void AddHtmlTextUser(int itemID, int stateID, int moduleID, int tabID, int userID)
        {
			_logger.LogDebug(">>> Entering AddHtmlTextUser(itemID={ItemID},stateID={StateID},moduleID={ModuleID},tabID={TabID},userID={UserID})", itemID, stateID, moduleID, tabID, userID);
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextUser", itemID, stateID, moduleID, tabID, userID);
			_logger.LogDebug("<<< Exiting AddHtmlTextUser(itemID={ItemID},stateID={StateID},moduleID={ModuleID},tabID={TabID},userID={UserID})", itemID, stateID, moduleID, tabID, userID);
        }

        public virtual void DeleteHtmlTextUsers()
        {
			_logger.LogDebug(">>> Entering DeleteHtmlTextUsers()");
            DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlTextUsers");
			_logger.LogDebug("<<< Exiting DeleteHtmlTextUsers()");
        }
    }
}
