// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteGroups.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SiteGroups.Models;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SiteGroupsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteGroupsController));

        private static IManagePortalGroups GroupManager => SiteGroups.Instance;

        [HttpGet]
        public HttpResponseMessage GetSiteGroups()
        {
			Logger.LogDebug(">>> Entering GetSiteGroups()");
            var groups = GroupManager.SiteGroups();
			Logger.LogDebug("GetSiteGroups(): groups → {Groups}", groups);
			Logger.LogDebug("<<< Exiting GetSiteGroups()");
            return this.Request.CreateResponse(HttpStatusCode.OK, groups);
        }

        [HttpGet]
        public HttpResponseMessage GetAvailablePortals()
        {
			Logger.LogDebug(">>> Entering GetAvailablePortals()");
            var portals = GroupManager.AvailablePortals();
			Logger.LogDebug("GetAvailablePortals(): portals → {Portals}", portals);
			Logger.LogDebug("<<< Exiting GetAvailablePortals()");
            return this.Request.CreateResponse(HttpStatusCode.OK, portals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Save(PortalGroupInfo portalGroup)
        {
			Logger.LogDebug(">>> Entering Save(portalGroup={PortalGroup})", portalGroup);
            try
            {
                var id = GroupManager.Save(portalGroup);
                return this.Request.CreateResponse(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting Save(portalGroup={PortalGroup})", portalGroup);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Delete(int groupId)
        {
			Logger.LogDebug(">>> Entering Delete(groupId={GroupId})", groupId);
            try
            {
                GroupManager.Delete(groupId);
                return this.Request.CreateResponse(HttpStatusCode.OK, groupId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting Delete(groupId={GroupId})", groupId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
