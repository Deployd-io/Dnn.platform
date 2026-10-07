// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Personalization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IHttpModule"/> which handles user personalization.</summary>
    public class PersonalizationModule : IHttpModule
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonalizationModule));
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="PersonalizationModule"/> class.</summary>
        public PersonalizationModule()
            : this(null)
        {
		Logger.LogError(">>> Entering PersonalizationModule()");
        Logger.LogWarning("<<< Exiting PersonalizationModule()");
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationModule"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public PersonalizationModule(PersonalizationController personalizationController)
        {
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
			Logger.LogDebug("PersonalizationModule(personalizationController={PersonalizationController}): personalizationController → {PersonalizationController1}", personalizationController, personalizationController);
        Logger.LogWarning("<<< Exiting PersonalizationModule(personalizationController={PersonalizationController})", personalizationController);
        }

        /// <summary>Gets the HttpModule module name.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "PersonalizationModule";

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
			Logger.LogError(">>> Entering Init(application={Application})", application);
            application.EndRequest += this.OnEndRequest;
			Logger.LogWarning("<<< Exiting Init(application={Application})", application);
        }

        /// <inheritdoc />
        public void Dispose()
        {
		Logger.LogError(">>> Entering Dispose()");
        Logger.LogWarning("<<< Exiting Dispose()");
        }

        /// <summary>Handles the <see cref="HttpApplication.EndRequest"/> event.</summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event args.</param>
        public void OnEndRequest(object s, EventArgs e)
        {
			Logger.LogError(">>> Entering OnEndRequest(s={S},e={E})", s, e);
            HttpContext context = ((HttpApplication)s).Context;
            HttpRequest request = context.Request;
			Logger.LogError("OnEndRequest(s={S},e={E}): context → {Context}", s, e, context);

            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            // Obtain PortalSettings from Current Context
            var portalSettings = (PortalSettings)context.Items["PortalSettings"];
			Logger.LogError("OnEndRequest(s={S},e={E}): portalSettings → {PortalSettings}", s, e, portalSettings);
            if (portalSettings != null)
            {
                // load the user info object
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
				Logger.LogError("OnEndRequest(s={S},e={E}): userInfo → {UserInfo}", s, e, userInfo);
                this.personalizationController.SaveProfile(context, userInfo.UserID, portalSettings.PortalId);
            }
			Logger.LogWarning("<<< Exiting OnEndRequest(s={S},e={E})", s, e);
        }
    }
}
