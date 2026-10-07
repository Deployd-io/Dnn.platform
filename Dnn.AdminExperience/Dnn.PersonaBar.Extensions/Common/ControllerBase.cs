// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Common
{
    using System.Net;
    using System.Net.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Services.Localization;

    public abstract class ControllerBase : PersonaBarApiController
    {
        protected HttpResponseMessage OkResponse(string msg, object data = null)
        {
			eventLogger.LogError(">>> Entering OkResponse(msg={Msg},data={Data})", msg, data);
            eventLogger.LogWarning("<<< Exiting OkResponse(msg={Msg},data={Data})", msg, data);
            return this.Request.CreateResponse(HttpStatusCode.OK, new ResponseModel(false, msg, data?.ToString() ?? string.Empty));
        }

        protected HttpResponseMessage UnauthorizedResponse(string msg = "", object data = null)
        {
			eventLogger.LogError(">>> Entering UnauthorizedResponse(msg={Msg},data={Data})", msg, data);
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotAuthorized", Components.Constants.LocalResourcesFile, true);
				eventLogger.LogError("UnauthorizedResponse(msg={Msg},data={Data}): msg → {Msg1}", msg, data, msg);
            }
            else
            {
                msg += " " + Localization.GetString("Prompt_SessionTimedOut", Components.Constants.LocalResourcesFile, true);
            }

            return this.Request.CreateResponse(HttpStatusCode.Unauthorized, new ResponseModel(true, msg, data?.ToString() ?? string.Empty));
        }

        protected HttpResponseMessage BadRequestResponse(string msg = "", object data = null)
        {
			eventLogger.LogError(">>> Entering BadRequestResponse(msg={Msg},data={Data})", msg, data);
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_InvalidData", Components.Constants.LocalResourcesFile, true);
				eventLogger.LogError("BadRequestResponse(msg={Msg},data={Data}): msg → {Msg1}", msg, data, msg);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseModel(true, msg, data?.ToString() ?? string.Empty));
        }

        protected HttpResponseMessage ServerErrorResponse(string msg = "", object data = null)
        {
			eventLogger.LogError(">>> Entering ServerErrorResponse(msg={Msg},data={Data})", msg, data);
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_ServerError", Components.Constants.LocalResourcesFile, true);
				eventLogger.LogError("ServerErrorResponse(msg={Msg},data={Data}): msg → {Msg1}", msg, data, msg);
            }

            return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseModel(true, msg, data?.ToString() ?? string.Empty));
        }

        protected HttpResponseMessage NotImplementedResponse(string msg = "", object data = null)
        {
			eventLogger.LogError(">>> Entering NotImplementedResponse(msg={Msg},data={Data})", msg, data);
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotImplemented", Components.Constants.LocalResourcesFile, true);
				eventLogger.LogError("NotImplementedResponse(msg={Msg},data={Data}): msg → {Msg1}", msg, data, msg);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotImplemented, new ResponseModel(true, msg, data?.ToString() ?? string.Empty));
        }
    }
}
