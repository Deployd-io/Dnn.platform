// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.UsersOnline
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>An HTTP module which keeps track of the users actively on the site.</summary>
    [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
    public partial class UsersOnlineModule : IHttpModule
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersOnlineModule));
        /// <summary>Gets the HttpModule module name.</summary>
        public string ModuleName => "UsersOnlineModule";

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
			Logger.LogError(">>> Entering Init(application={Application})", application);
            application.AuthorizeRequest += this.OnAuthorizeRequest;
			Logger.LogWarning("<<< Exiting Init(application={Application})", application);
        }

        /// <inheritdoc />
        public void Dispose()
        {
		Logger.LogError(">>> Entering Dispose()");
        Logger.LogWarning("<<< Exiting Dispose()");
        }

        /// <summary>Handles the <see cref="HttpApplication.AuthorizeRequest"/> event.</summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event args.</param>
        public void OnAuthorizeRequest(object s, EventArgs e)
        {
			Logger.LogError(">>> Entering OnAuthorizeRequest(s={S},e={E})", s, e);
            // First check if we are upgrading/installing
            var app = (HttpApplication)s;
			Logger.LogError("OnAuthorizeRequest(s={S},e={E}): app → {App}", s, e, app);
            HttpRequest request = app.Request;

            // check if we are upgrading/installing or if this is a captcha request
            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            // Create a Users Online Controller
            var objUserOnlineController = new UserOnlineController();
			Logger.LogError("OnAuthorizeRequest(s={S},e={E}): objUserOnlineController → {ObjUserOnlineController}", s, e, objUserOnlineController);

            // Is Users Online Enabled?
            if (objUserOnlineController.IsEnabled())
            {
                objUserOnlineController.TrackUsers();
            }
			Logger.LogWarning("<<< Exiting OnAuthorizeRequest(s={S},e={E})", s, e);
        }
    }
}
