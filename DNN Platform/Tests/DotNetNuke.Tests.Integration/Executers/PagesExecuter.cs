using Microsoft.Extensions.Logging;
﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers
{
    public class PagesExecuter : WebApiExecuter
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public PagesExecuter GetPageDetails(int pageId)
        {
			_logger.LogDebug(">>> Entering GetPageDetails(pageId={PageId})", pageId);
            this.Responses.Add(this.Connector.GetContent(
                    "API/PersonaBar/Pages/GetPageDetails?pageId=" + pageId));

			_logger.LogDebug("<<< Exiting GetPageDetails(pageId={PageId})", pageId);
            return this;
        }

        public dynamic SavePageDetails(dynamic pageDetails)
        {
			_logger.LogDebug(">>> Entering SavePageDetails(pageDetails={PageDetails})", pageDetails);
            this.Responses.Add(this.Connector.PostJson(
                "API/PersonaBar/Pages/SavePageDetails",
                pageDetails));
			_logger.LogDebug("<<< Exiting SavePageDetails(pageDetails={PageDetails})", pageDetails);
            return this.GetLastDeserializeResponseMessage();
        }
    }
}
