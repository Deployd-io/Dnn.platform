// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System.Collections;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Html.Components;
	using Microsoft.Extensions.Logging;

    /// <summary>The HtmlTextLogController is the Controller class for managing Log History information the HtmlText module.</summary>
    public class HtmlTextLogController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>GetHtmlTextLog retrieves a collection of HtmlTextLogInfo objects for an Item.</summary>
        /// <param name="itemID">The Id of the Item.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="HtmlTextLogInfo"/> instances.</returns>
        public ArrayList GetHtmlTextLog(int itemID)
        {
			_logger.LogError(">>> Entering GetHtmlTextLog(itemID={ItemID})", itemID);
            _logger.LogWarning("<<< Exiting GetHtmlTextLog(itemID={ItemID})", itemID);
            return CBO.FillCollection(DataProvider.Instance().GetHtmlTextLog(itemID), typeof(HtmlTextLogInfo));
        }

        /// <summary>AddHtmlTextLog creates a new HtmlTextLog for an Item.</summary>
        /// <param name="objHtmlTextLog">An HtmlTextLogInfo object.</param>
        public void AddHtmlTextLog(HtmlTextLogInfo objHtmlTextLog)
        {
			_logger.LogError(">>> Entering AddHtmlTextLog(objHtmlTextLog={ObjHtmlTextLog})", objHtmlTextLog);
            DataProvider.Instance().AddHtmlTextLog(objHtmlTextLog.ItemID, objHtmlTextLog.StateID, objHtmlTextLog.Comment, objHtmlTextLog.Approved, UserController.Instance.GetCurrentUserInfo().UserID);
			_logger.LogWarning("<<< Exiting AddHtmlTextLog(objHtmlTextLog={ObjHtmlTextLog})", objHtmlTextLog);
        }
    }
}
